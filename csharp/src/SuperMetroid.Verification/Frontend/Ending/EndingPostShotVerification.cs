using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Assets;
using static SuperMetroid.Verification.Frontend.Ending.EndingLogoTablesVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.AssetExtraction.Frontend.Ending;

using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Frontend.Ending;

/// <summary>Checks post-shot ending rotation, palette fades, white-flash timing, and the logo's native transfers.</summary>
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
        // The import's fade levels scale the post-credits colours as the cartridge's accumulator does (#1512).
        IReadOnlyList<SheetShade> installedShades = EndingScreenShades.Screen(InstalledFixtures.EndingColours,
            InstalledFixtures.ImportMaps.RoomPaletteFx);
        foreach ((EndingFade fade, int start) in new[] { (EndingFade.Shot, 192), (EndingFade.ShootingSamus, 240) })
        {
            for (int level = 0; level <= EndingShades.FadeLevels; level++)
            {
                SheetShade shade = installedShades.Single(candidate => candidate.Shade == EndingShades.Faded(fade, level));
                AssertTrue(shade.Inks.SequenceEqual(Enumerable.Range(start, 16)), "a post-shot fade level covers its colour group");
                for (int i = 0; i < 16; i++)
                {
                    ushort word = initial[start + i].ToWord();
                    ushort faded = (ushort)(((word & 31) * level / 32) | ((word >> 5 & 31) * level / 32) << 5 | ((word >> 10 & 31) * level / 32) << 10);
                    AssertEqual(faded, shade.Colours[i].ToWord(), "post-shot fade level colour matches the cartridge's component fade");
                }
            }
        }
        var expected = new byte[SnesVram.ByteCount];
        byte[] font = RomDataReader.Decompress(SuperMetroid.AssetExtraction.Cartridge.CartridgeImportSource.Require(bus), 0x97e7de, 0x8000);
        using var fontPng = new MemoryStream(
            SuperMetroid.AssetExtraction.Frontend.Ending.EndingFontAtlasExtractor.Extract(bus), writable: false);
        EndingFontCharacters fontAtlas = EndingFontCharacters.Read(fontPng);
        var shot = new EndingPostShot();
        ImportEndingObjectArtwork objects = SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.EndingObjects;
        byte[] tiles = RomDataReader.Decompress(SuperMetroid.AssetExtraction.Cartridge.CartridgeImportSource.Require(bus), 0x99e089, 0x8000);
        byte[] map = RomDataReader.Decompress(SuperMetroid.AssetExtraction.Cartridge.CartridgeImportSource.Require(bus), 0x99ecc4, 0x8000);
        for (int frame = 1; frame <= 216; frame++)
        {
            shot.Step(screen);
            AssertEqual(FloorZoom.FromTwoHundredFiftySixths(Math.Max(24, 2304 - Math.Min(frame, 36) * 64)), shot.Zoom, "native post-shot zoom trajectory");
            AssertEqual(RotationAngle.FromSteps(unchecked((byte)(-8 * Math.Min(frame, 36)))), shot.Angle, "native post-shot rotation trajectory, wrapping below zero");
            AssertEqual(frame >= 36, shot.RotationFinished, "rotation handoff occurs on call 36");
            AssertEqual(frame == 216, shot.ReadyForWhiteFlash, "white-flash handoff follows a full 180-frame hold");
            // The screen names the shot's and Samus's fade levels, Samus's after its delay (#1512).
            AssertTrue(screen.Colours.Shading.Shades.Contains(EndingShades.Faded(EndingFade.Shot, 32 - Math.Clamp(frame, 0, 32))),
                "post-shot shot fade level");
            AssertTrue(screen.Colours.Shading.Shades.Contains(EndingShades.Faded(EndingFade.ShootingSamus, 32 - Math.Clamp(frame - 16, 0, 32))),
                "post-shot delayed Samus fade level");
        }
        // The game shows the logo's installed picture whole (#1515); the cartridge's six logo transfers, which the import
        // pictures it from, write the font, the logo's characters and its map.
        font.AsSpan(0x1000, 0x400).CopyTo(expected.AsSpan(0x9000));
        tiles.AsSpan(0, 0x2000).CopyTo(expected.AsSpan(0xc000));
        map.AsSpan(0, 0x800).CopyTo(expected.AsSpan(0xa800));
        byte[] logo = EndingScreenMemory.Compose(EndingScreenLoads.None with { PostShotParts = EndingScreenLoads.PostShotPartCount },
            null, null, objects, fontAtlas, EndingReward.Armored);
        AssertTrue(expected.AsSpan().SequenceEqual(logo), "the logo's native transfers change exactly their intended VRAM bytes");
        AssertEqual(0, guardedBus.ForbiddenReadAttempts,
            "post-shot uploads never reread the fixed cartridge transfer table");
        Console.WriteLine("  Post-shot: 36 rotation frames, delayed palette fades, 180-frame hold, and the logo's six exact transfers.");
    }

    /// <summary>Rejects runtime rereads of the fixed post-shot transfer table after import.</summary>
    private static CartridgeReadGuard NewPostShotUploadTableReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .ForbidRange(EndingPostShotUploadDefinitions.TableAddress,
                EndingPostShotUploadDefinitions.Count * EndingPostShotUploadDefinitions.RecordBytes,
                address => $"Post-shot upload reread fixed cartridge table at ${address:X6}.");
}
