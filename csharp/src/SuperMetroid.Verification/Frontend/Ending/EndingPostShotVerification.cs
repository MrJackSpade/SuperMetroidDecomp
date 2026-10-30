using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Frontend.Ending.EndingLogoTablesVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.AssetExtraction.Frontend.Ending;

using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Frontend.Ending;

/// <summary>Checks post-shot ending rotation, palette fades, VRAM uploads, and white-flash timing.</summary>
internal static class EndingPostShotVerification
{
    /// <summary>Compares every post-shot frame with native timing and exact palette and VRAM contents.</summary>
    /// <param name="bus">Retail cartridge data used as the expected artwork and palette source.</param>
    internal static void VerifyEndingPostShot(ISnesAddressSpace bus)
    {
        Suite(nameof(VerifyEndingPostShotTransferFields), () => VerifyEndingPostShotTransferFields(bus));
        var guardedBus = NewPostShotUploadTableReadGuard(bus);
        var nativeCgram = new SnesCgram();
        SuperMetroid.AssetExtraction.Cartridge.CartridgePaletteImporter.LoadToCgram(nativeCgram, bus, 0x8ce7e9);
        RgbColor[] initial = nativeCgram.Colors.ToArray();
        var screen = new EndingScreen();
        for (int color = 0; color < initial.Length; color++)
            screen.Palette.SetColor(color, initial[color]);
        var expected = new byte[SnesVram.ByteCount];
        byte[] font = RomDataReader.Decompress(SuperMetroid.AssetExtraction.Cartridge.CartridgeImportSource.Require(bus), 0x97e7de, 0x8000);
        using var fontPng = new MemoryStream(
            SuperMetroid.AssetExtraction.Frontend.Ending.EndingFontAtlasExtractor.Extract(bus), writable: false);
        EndingFontCharacters fontAtlas = EndingFontCharacters.Read(fontPng);
        var shot = new EndingPostShot(screen.Palette);
        EndingObjectArtwork objects = RepositoryInstallation.Installation.LoadEndingObjectArt();
        byte[] tiles = RomDataReader.Decompress(SuperMetroid.AssetExtraction.Cartridge.CartridgeImportSource.Require(bus), 0x99e089, 0x8000);
        byte[] map = RomDataReader.Decompress(SuperMetroid.AssetExtraction.Cartridge.CartridgeImportSource.Require(bus), 0x99ecc4, 0x8000);
        for (int frame = 1; frame <= 216; frame++)
        {
            shot.Step(screen);
            AssertEqual(Math.Max(24, 2304 - Math.Min(frame, 36) * 64), shot.Scale, "native post-shot scale trajectory");
            AssertEqual(RotationAngle.FromTableIndex(unchecked((byte)(-8 * Math.Min(frame, 36)))), shot.Angle, "native post-shot rotation trajectory, wrapping below zero");
            AssertEqual(frame >= 36, shot.RotationFinished, "rotation handoff occurs on call 36");
            AssertEqual(frame == 216, shot.ReadyForWhiteFlash, "white-flash handoff follows a full 180-frame hold");
            AssertEqual(Math.Clamp(frame - 47, 0, 6), shot.PartsShown, "logo uploads wait for Samus fade, one transfer per frame");
            foreach (int start in new[] { 192, 240 })
            {
                int remaining = 32 - Math.Clamp(frame - (start == 240 ? 16 : 0), 0, 32);
                for (int i = start; i < start + 16; i++)
                {
                    RgbColor color = initial[i];
                    ushort faded = (ushort)(((color.ToWord() & 31) * remaining / 32)
                        | ((color.ToWord() >> 5 & 31) * remaining / 32) << 5
                        | ((color.ToWord() >> 10 & 31) * remaining / 32) << 10);
                    AssertEqual(faded, screen.Palette.Colors[i], "post-shot palette component and delayed Samus fade");
                }
            }
            if (frame == 48) font.AsSpan(0x1000, 0x400).CopyTo(expected.AsSpan(0x9000));
            if (frame is >= 49 and <= 52)
            {
                int offset = (frame - 49) * 0x800;
                tiles.AsSpan(offset, 0x800).CopyTo(expected.AsSpan(0xc000 + offset));
            }
            if (frame == 53) map.AsSpan(0, 0x800).CopyTo(expected.AsSpan(0xa800));
            AssertEqual(shot.PartsShown, screen.PostShotPartsShown, "the screen shows each part the post-shot makes");
            byte[] shownScreen = EndingScreenMemory.Compose(screen, null, null, objects, fontAtlas, EndingReward.Armored);
            AssertTrue(expected.AsSpan().SequenceEqual(shownScreen), "each native logo transfer changes exactly its intended VRAM bytes");
        }
        AssertEqual(0, guardedBus.ForbiddenReadAttempts,
            "post-shot uploads never reread the fixed cartridge transfer table");
        Console.WriteLine("  Post-shot: 36 rotation frames, delayed palette fades, six exact uploads, 180-frame hold.");
    }

    /// <summary>Rejects runtime rereads of the fixed post-shot transfer table after import.</summary>
    private static CartridgeReadGuard NewPostShotUploadTableReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .ForbidRange(EndingPostShotUploadDefinitions.TableAddress,
                EndingPostShotUploadDefinitions.Count * EndingPostShotUploadDefinitions.RecordBytes,
                address => $"Post-shot upload reread fixed cartridge table at ${address:X6}.");
}
