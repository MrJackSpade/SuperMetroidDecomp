using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.Frontend.Cinematics;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Frontend.Map.MapSpritesVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction;

namespace SuperMetroid.Verification.Frontend.Ending;

/// <summary>Retail and live-scene checks for ending explosion sprite frames, programs, and compositions.</summary>
internal static class EndingExplosionSpriteArtworkVerification
{
    /// <summary>Compares sixteen program-selected frame pointers, part counts, and stable asset keys with bank-$8C sprite maps.</summary>
    internal static void VerifyEndingExplosionFrameCatalog(ISnesAddressSpace bus)
    {
        Suite(nameof(VerifyEndingExplosionCompositions), () => VerifyEndingExplosionCompositions(bus));
        // Independent original list operands select every distinct visual frame.
        ushort[] operands = [0xeb15, 0xeb19, 0xeb1d, 0xeb21, 0xeb2b, 0xeb2f, 0xeb33, 0xeb37,
            0xeb5f, 0xeb63, 0xeb3f, 0xeb43, 0xeb47, 0xeb53, 0xeb6b, 0xeb8b];
        // Published asset keys are compatibility evidence, not generated expected values.
        string[] names = ["planet-damage-0", "planet-damage-1", "planet-damage-2", "planet-damage-3",
            "planet-flash-0", "planet-flash-1", "planet-flash-2", "planet-flash-3", "lava-0", "lava-1",
            "glow-0", "glow-1", "glow-2", "starfield", "silhouette", "afterglow"];
        var frames = EndingExplosionSpriteDefinitions.Frames;
        AssertEqual(16, frames.Count, "explosion catalog frame count");
        for (int i = 0; i < operands.Length; i++)
        {
            int address = 0x8b0000 | operands[i];
            ushort pointer = CartridgeImportSource.Require(bus).ReadCartridgeWord(address);
            int header = 0x8c0000 | pointer;
            int parts = CartridgeImportSource.Require(bus).ReadCartridgeWord(header);
            AssertEqual(pointer, CinematicSpritemapSources.Of(frames[i].Pose), $"explosion catalog native pointer {i}");
            AssertEqual(parts, frames[i].StockPartCount, $"explosion catalog native part count {i}");
            AssertEqual(names[i], frames[i].Name, $"explosion catalog asset key {i}");
        }
        AssertTrue(frames.Select(frame => frame.Name).SequenceEqual(names), "explosion catalog enumeration order");
        AssertEqual(frames.Count, frames.Select(frame => frame.Pose).Distinct().Count(), "explosion frames name distinct compositions");
    }

    /// <summary>Checks the compositions compiled from the native maps draw the native OAM.</summary>
    private static void VerifyEndingExplosionCompositions(ISnesAddressSpace bus)
    {
        for (int pose = 0; pose < 16; pose++)
        {
            ushort pointer = pose < 10 ? (ushort)(0xa396 + pose * 22) : pose == 10 ? (ushort)0xa472 : pose == 11 ? (ushort)0xa4b0 : pose == 12 ? (ushort)0xa516 : pose == 13 ? (ushort)0xa28b : pose == 14 ? (ushort)0xa5e2 : (ushort)0xa57c;
            AssertTrue(EndingExplosionSpriteDefinitions.Frames.Count(frame => CinematicSpritemapSources.Of(frame.Pose) == pointer) == 1,
                "each native explosion map is one catalog frame");
            int count = CartridgeImportSource.Require(bus).ReadCartridgeWordFixedBank(0x8c0000 | pointer);
            var visual = new SpriteVisualPart[count];
            for (int index = 0; index < count; index++)
            {
                int address = 0x8c0000 + pointer + 2 + index * 5;
                var x = new SnesSpritemapXWord(CartridgeImportSource.Require(bus).ReadCartridgeWord(address));
                var attributes = new SnesObjAttributeWord(CartridgeImportSource.Require(bus).ReadCartridgeWord(address + 3));
                visual[index] = new SpriteVisualPart
                {
                    OffsetX = x.SignedOffset, OffsetY = unchecked((sbyte)bus.ReadCartridgeByte(address + 2)),
                    TileColumn = attributes.TileNumber % 16, TileRow = attributes.TileNumber / 16,
                    Size = x.IsLarge ? 16 : 8, Priority = attributes.Priority, Palette = null,
                    FlipX = attributes.FlipHorizontally, FlipY = attributes.FlipVertically,
                };
            }
            SpriteComposition original = IntroCinematicSpriteCompiler.Compile(visual, "native grid");
            foreach (ushort y in new ushort[] { 72, 0xfff8 })
            {
                var native = new OamBuffer();
                var generated = new OamBuffer();
                native.BeginFrame(); generated.BeginFrame();
                DrawImportedSpritemap(bus, native, 0x8c0000 | pointer, new(120, y), 0x0800, originIsOnScreen: y == 72);
                if (y == 72) original.DrawOnScreen(generated, new(120, y), 0x0800);
                else original.DrawOffScreen(generated, new(120, y), 0x0800);
                native.FinalizeFrame(); generated.FinalizeFrame();
                AssertTrue(native.ImageEquals(generated),
                    "compiled explosion composition preserves native OAM and clipping");
            }
        }
    }

    /// <summary>Compares installed explosion OAM with native maps for visible and wrapped offscreen origins after checking frame and program catalogs.</summary>
    internal static void VerifyEndingExplosionActorArtwork(
        ISnesAddressSpace bus, EndingObjectSourceArtwork stock)
    {
        Suite(nameof(VerifyEndingExplosionFrameCatalog), () => VerifyEndingExplosionFrameCatalog(bus));

        foreach (CinematicSpriteFrameDefinition frame in EndingExplosionSpriteDefinitions.Frames)
        {
            foreach (ushort y in new ushort[] { 0x0048, 0xfff8 })
            {
                bool onScreen =
                    (short)y >= 0;
                int source = (int)new SnesAddress(
                    IntroCinematicSourceDefinitions.SpritemapBank, CinematicSpritemapSources.Of(frame.Pose));
                var nativeOam = new OamBuffer();
                nativeOam.BeginFrame();
                if (onScreen)
                    DrawImportedSpritemap(bus, nativeOam, source, new(120, y), 0x0800);
                else
                    DrawImportedSpritemap(bus, nativeOam, source, new(120, y), 0x0800, originIsOnScreen: false);
                nativeOam.FinalizeFrame();
                var installedOam = ScreenSpritesFixtures.Blank();
                installedOam.BeginFrame();
                CinematicSheetFixtures.Drawer(stock.ExplosionSprites).Draw(frame.Pose, installedOam,
                    new(120, y), EndingShades.Drawn(EndingSpriteColouring.ShootingStars));
                installedOam.FinalizeFrame();
                AssertTrue(nativeOam.ImageEquals(installedOam) &&
                        nativeOam.LastFinalizedSpriteCount == installedOam.LastFinalizedSpriteCount,
                    $"ending explosion {frame.Name} at Y=${y:X4} preserves cartridge OAM");
            }
        }
    }

    /// <summary>Checks that an explosion sprite document missing a named frame is rejected on read.</summary>
    internal static void VerifyEndingExplosionSpritesRejectMissingFrame()
    {
        EndingExplosionSpriteDocument document =
            JsonSerializer.Deserialize<EndingExplosionSpriteDocument>(
                File.ReadAllBytes(Path.Combine(SuperMetroid.TestSupport.RepositoryInstallation.Verification.CartridgeImportContent.ImportData(SuperMetroid.AssetExtraction.Installation.GameInstallationLayout.EndingObjectDirectoryName), EndingExplosionSpriteFormat.FileName)), ArtworkJson.Options) ??
            throw new InvalidDataException("Stock ending explosion sprite JSON is empty.");
        document.Frames.Remove("starfield");
        AssertThrows<InvalidDataException>(() =>
        {
            using var invalid = new MemoryStream();
            EndingExplosionSpritePresentation.Write(invalid, document);
            _ = EndingExplosionSpritePresentation.Load(new MemoryStream(invalid.ToArray(), writable: false));
        }, "ending explosion sprites reject missing named art on read");
    }
}
