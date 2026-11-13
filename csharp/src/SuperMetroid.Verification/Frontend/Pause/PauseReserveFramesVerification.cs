using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Frontend.Map.MapSpritesVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Frontend.Pause;
using SuperMetroid.Core.Rendering;
using SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;
using SuperMetroid.AssetExtraction;

namespace SuperMetroid.Verification.Frontend.Pause;

/// <summary>Checks all pause reserve-tank visual roles and their authored OAM parts.</summary>
internal static class PauseReserveFramesVerification
{
    /// <summary>Lists preconversion sparse frame identities independently of the production selector.</summary>
    private static ReserveFrameOracleResult[] ReserveFrameOracle() =>
        [new ReserveFrameOracleResult("Full", 0x1b), new ReserveFrameOracleResult("EndCap", 0x1f), new ReserveFrameOracleResult("Empty", 0x20), new ReserveFrameOracleResult("Fill1", 0x21), new ReserveFrameOracleResult("Fill2", 0x22),
         new ReserveFrameOracleResult("Fill3", 0x23), new ReserveFrameOracleResult("Fill4", 0x24), new ReserveFrameOracleResult("Fill5", 0x25), new ReserveFrameOracleResult("Fill6", 0x26), new ReserveFrameOracleResult("Fill7", 0x27)];

    /// <summary>Checks exact frame admission, required reserve roles and unsupported reserve identities.</summary>
    internal static void VerifyPauseReserveFrameCases(ISnesAddressSpace rom)
    {
        byte[] bytes = PauseReserveTankExtractor.Extract(rom);
        var document = RequiredJson.Deserialize<PauseReserveTankDocument>(bytes, ArtworkJson.Options);
        var stock = PauseReserveTankSourcePresentation.Load(new MemoryStream(bytes));
        Suite(nameof(VerifyPauseReserveNativeFrames), () => VerifyPauseReserveNativeFrames(rom, stock));
        foreach (var changed in ReserveFrameOracle())
        {
            var frames = new Dictionary<string, SpriteVisualPart[]>(document.Frames);
            frames.Remove(changed.Name);
            AssertThrows<InvalidDataException>(() =>
            {
                using var written = new MemoryStream();
                PauseReserveTankSourcePresentation.Write(written, document with { Frames = frames });
                _ = PauseReserveTankSourcePresentation.Load(new MemoryStream(written.ToArray(), writable: false));
            }, "each reserve role remains required on read");
        }
        foreach (ushort invalid in new ushort[] { 0, 0x1a, 0x1c, 0x1d, 0x1e, 0x28, ushort.MaxValue })
            AssertEqual(false, PauseReserveTankVisualWords.Ordinals.TryDecode(invalid, out _), "unsupported native reserve identity");
        var undefined = (PauseReserveTankVisual)(-1);
        AssertThrows<InvalidOperationException>(() => stock.Presentation.Draw(ScreenSpritesFixtures.Blank(), MenuSheetFixtures.PauseSprites(stock), undefined, 0), "unsupported reserve identity");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.Presentation.Draw(ScreenSpritesFixtures.Blank(), MenuSheetFixtures.PauseSprites(stock), undefined, -1), "anchor error precedes unknown visual");
    }

    /// <summary>
    /// Checks native role and partial-fill selectors for all reserve frames; the object limit that cuts a frame short in a
    /// nearly full sprite buffer is not replicated (#1513).
    /// </summary>
    internal static void VerifyPauseReserveNativeFrames(ISnesAddressSpace rom, PauseReserveTankSourcePresentation presentation)
    {
        foreach (var (site, identity) in new[] { new VerifyPauseReserveNativeFramesCase(0x82b305, 0x1b), new VerifyPauseReserveNativeFramesCase(0x82b37d, 0x20), new VerifyPauseReserveNativeFramesCase(0x82b396, 0x1f) })
        {
            AssertEqual((byte)0xa9, rom.ReadCartridgeByte(site), "native reserve role LDA");
            AssertEqual(identity, (int)ReadVerificationWord(rom, site + 1), "native reserve role identity");
        }
        for (int index = 0; index < 16; index++)
            AssertEqual(0x20 + index % 8, (int)ReadVerificationWord(rom, 0x82b3d9 + index * 2), "native repeated partial-fill identities");
        foreach (var frame in ReserveFrameOracle())
        for (int index = 0; index < 6; index++)
        {
            var expected = new OamBuffer(); var actual = ScreenSpritesFixtures.Blank(); expected.BeginFrame(); actual.BeginFrame();
            ushort x = ReadVerificationWord(rom, 0x82c1d6 + index * 2);
            ushort y = (ushort)(ReadVerificationWord(rom, 0x82c1e2) - 1);
            int pointer = 0x820000 | ReadVerificationWord(rom, 0x82c569 + frame.Id * 2);
            DrawImportedSpritemap(rom, expected, pointer, new(x, y), 0x600);
            SpriteSheet sheet = MenuSheetFixtures.PauseSprites(presentation);
            presentation.Presentation.Draw(actual, sheet, PauseReserveTankVisualWords.Ordinals.Decode(frame.Id, "reserve visual"), index);
            expected.FinalizeFrame(); actual.FinalizeFrame();
            AssertTrue(expected.ImageEquals(actual),
                "native reserve frame selection, OAM attributes and ordering");
        }
    }
}
