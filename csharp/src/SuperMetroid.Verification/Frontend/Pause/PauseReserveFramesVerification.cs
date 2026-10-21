using SuperMetroid.Tooling;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Frontend.Map.MapSpritesVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.Menus.Artwork;
using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend.Pause;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Verification.Frontend.Pause;

/// <summary>Checks all pause reserve-tank visual roles and their authored OAM parts.</summary>
internal static class PauseReserveFramesVerification
{
    /// <summary>Lists preconversion sparse frame identities independently of the production selector.</summary>
    private static ReserveFrameOracleResult[] ReserveFrameOracle() =>
        [new ReserveFrameOracleResult("Full", 0x1b), new ReserveFrameOracleResult("EndCap", 0x1f), new ReserveFrameOracleResult("Empty", 0x20), new ReserveFrameOracleResult("Fill1", 0x21), new ReserveFrameOracleResult("Fill2", 0x22),
         new ReserveFrameOracleResult("Fill3", 0x23), new ReserveFrameOracleResult("Fill4", 0x24), new ReserveFrameOracleResult("Fill5", 0x25), new ReserveFrameOracleResult("Fill6", 0x26), new ReserveFrameOracleResult("Fill7", 0x27)];

    /// <summary>Checks exact frame admission and that each edited reserve role changes only its selected OAM composition.</summary>
    internal static void VerifyPauseReserveFrameCases(ISnesAddressSpace rom)
    {
        byte[] bytes = PauseReserveTankExtractor.Extract(rom);
        var document = RequiredJson.Deserialize<PauseReserveTankDocument>(bytes, ArtworkJson.Options);
        var stock = PauseReserveTankPresentation.Load(new MemoryStream(bytes));
        Suite(nameof(VerifyPauseReserveNativeFrames), () => VerifyPauseReserveNativeFrames(rom, stock));
        Suite(nameof(VerifyPauseReserveStockParts), () => VerifyPauseReserveStockParts(rom, document));
        AssertEqual(0, stock.StoredFrameCount, "stock reserve frames use no stored compositions");
        foreach (var changed in ReserveFrameOracle())
        {
            var frames = new Dictionary<string, SpriteVisualPart[]>(document.Frames); frames[changed.Name] = [];
            using var stream = new MemoryStream(); PauseReserveTankPresentation.Write(stream, document with { Frames = frames }); stream.Position = 0;
            var edited = PauseReserveTankPresentation.Load(stream);
            AssertEqual(1, edited.StoredFrameCount, "only authored reserve frame stored");
            foreach (var frame in ReserveFrameOracle())
            {
                var expected = new OamBuffer(); var actual = ScreenSpritesFixtures.Blank(); expected.BeginFrame(); actual.BeginFrame();
                if (frame.Id != changed.Id)
                {
                    int pointer = 0x820000 | ReadVerificationWord(rom, 0x82c569 + frame.Id * 2);
                    DrawImportedSpritemap(rom, expected, pointer, new(24, 95), 0x600);
                }
                edited.Draw(actual, (PauseReserveTankVisual)frame.Id, 0);
                expected.FinalizeFrame(); actual.FinalizeFrame();
                AssertTrue(expected.ImageEquals(actual),
                    "each authored reserve frame independently selected");
            }
            frames.Remove(changed.Name);
            AssertThrows<InvalidDataException>(() => PauseReserveTankPresentation.Write(new MemoryStream(), document with { Frames = frames }),
                "each reserve role remains required");
        }
        foreach (ushort invalid in new ushort[] { 0, 0x1a, 0x1c, 0x1d, 0x1e, 0x28, ushort.MaxValue })
            AssertThrows<InvalidOperationException>(() => stock.Draw(ScreenSpritesFixtures.Blank(), (PauseReserveTankVisual)invalid, 0), "unsupported reserve identity");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.Draw(ScreenSpritesFixtures.Blank(), 0, -1), "anchor error precedes unknown visual");
    }

    /// <summary>Compares each stock reserve part and independently edited OAM attribute against its native map.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for pause reserve stock parts")]
    private static void VerifyPauseReserveStockParts(ISnesAddressSpace rom, PauseReserveTankDocument document)
    {
        foreach (var frame in ReserveFrameOracle())
        {
            int pointer = 0x820000 | ReadVerificationWord(rom, 0x82c569 + frame.Id * 2);
            AssertEqual((ushort)1, ReadVerificationWord(rom, pointer), "native reserve part count");
            var x = new SnesSpritemapXWord(ReadVerificationWord(rom, pointer + 2));
            var attributes = new SnesObjAttributeWord(ReadVerificationWord(rom, pointer + 5));
            var stockPart = PauseReserveTankDefinitions.StockPart((PauseReserveTankVisual)frame.Id);
            AssertEqual(x.SignedOffset, stockPart.OffsetX, "native reserve X offset");
            AssertEqual(unchecked((sbyte)rom.ReadCartridgeByte(pointer + 4)), stockPart.OffsetY, "native reserve Y offset");
            AssertEqual(x.IsLarge ? 16 : 8, stockPart.Size, "native reserve sprite size");
            AssertEqual(attributes.TileNumber, stockPart.TileRow * MapSpriteFormat.TileColumns + stockPart.TileColumn, "native reserve tile progression");
            AssertEqual(attributes.Priority, stockPart.Priority, "native reserve priority");
            AssertEqual(attributes.FlipHorizontally, stockPart.FlipX, "native reserve X flip");
            AssertEqual(attributes.FlipVertically, stockPart.FlipY, "native reserve Y flip");
            AssertTrue(stockPart.Palette is null, "reserve palette remains caller-owned");
            var original = document.Frames[frame.Name][0];
            SpriteVisualPart[] edits = [original with { OffsetX = 17 }, original with { OffsetY = -9 },
                original with { TileColumn = (original.TileColumn + 1) % 16 }, original with { TileRow = original.TileRow + 1 },
                original with { Size = 16, TileColumn = 0 }, original with { Priority = 2 }, original with { Palette = 5 },
                original with { FlipX = true }, original with { FlipY = true }];
            foreach (var part in edits)
            {
                var frames = new Dictionary<string, SpriteVisualPart[]>(document.Frames); frames[frame.Name] = [part];
                using var stream = new MemoryStream(); PauseReserveTankPresentation.Write(stream, document with { Frames = frames }); stream.Position = 0;
                var edited = PauseReserveTankPresentation.Load(stream);
                AssertEqual(1, edited.StoredFrameCount, "authored part difference remains compiled");
                var expected = new OamBuffer(); var actual = ScreenSpritesFixtures.Blank(); expected.BeginFrame(); actual.BeginFrame();
                MenuSpriteCompiler.Compile([part], frame.Name).DrawOnScreen(expected, new(24, 95), 0x600);
                edited.Draw(actual, (PauseReserveTankVisual)frame.Id, 0);
                expected.FinalizeFrame(); actual.FinalizeFrame();
                AssertTrue(expected.ImageEquals(actual),
                    "independent authored part field reaches OAM");
            }
        }
    }

    /// <summary>Checks native role and partial-fill selectors and OAM capacity for all reserve frames.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for pause reserve native frames")]
    internal static void VerifyPauseReserveNativeFrames(ISnesAddressSpace rom, PauseReserveTankPresentation presentation)
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
        foreach (int occupied in new[] { 0, 127, 128 })
        {
            var expected = new OamBuffer(); var actual = ScreenSpritesFixtures.Blank(); expected.BeginFrame(); actual.BeginFrame();
            for (int i = 0; i < occupied; i++) { expected.AddOccupyingPart(new Position16<OamPixels>(12, 34), 56); actual.AddOccupyingPart(new Position16<OamPixels>(12, 34), 56); }
            ushort x = ReadVerificationWord(rom, 0x82c1d6 + index * 2);
            ushort y = (ushort)(ReadVerificationWord(rom, 0x82c1e2) - 1);
            int pointer = 0x820000 | ReadVerificationWord(rom, 0x82c569 + frame.Id * 2);
            DrawImportedSpritemap(rom, expected, pointer, new(x, y), 0x600);
            presentation.Draw(actual, (PauseReserveTankVisual)frame.Id, index);
            expected.FinalizeFrame(); actual.FinalizeFrame();
            AssertTrue(expected.ImageEquals(actual),
                "native reserve frame selection, OAM attributes, ordering and capacity");
        }
    }
}
