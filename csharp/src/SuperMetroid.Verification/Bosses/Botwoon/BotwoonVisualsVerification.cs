using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Hardware.ImportedSpritemapsVerification;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Bosses.Botwoon.Artwork;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Verification.Bosses.Botwoon;

/// <summary>Checks installed Botwoon head frames against the cartridge's OAM.</summary>
internal static class BotwoonVisualsVerification
{
    /// <summary>Compares sixteen installed head frames to native OAM.</summary>
    internal static void VerifyInstalledBotwoonVisuals(
        CartridgeImportAddressSpace rom, EnemyTileSourceArtwork stock)
    {
        // The head animations' frames are checked against the cartridge lists by VerifyBotwoonHeadAnimations;
        // here every visible frame's installed artwork must draw the cartridge's OAM.
        var selected = new HashSet<ushort>();
        foreach (NativeEnemySpritemapDefinition frame in BotwoonVisualDefinitionsConstants.Frames())
        {
            ushort installedPointer = frame.Pointer;
            ushort native = installedPointer;
            selected.Add(installedPointer);
            AssertTrue((stock.Spritemaps ?? throw new InvalidOperationException("Verification requires stock.Spritemaps.")).TryGetDisplay(
                    BotwoonVisualDefinitionsConstants.Bank, installedPointer,
                    out EnemySpritemapParts installedParts),
                $"Botwoon head frame $B3:{installedPointer:X4} is editable");
            var nativeOam = new OamBuffer();
            var installedOam = new OamBuffer();
            DrawImportedEnemySpritemap(rom, nativeOam, new NativeEnemySpritemapDraw { Bank = BotwoonVisualDefinitionsConstants.Bank, Pointer = native, Origin = new(128, 128), PaletteBits = 0, BaseTileIndex = 0 });
            installedOam.AddEnemySpritemap(installedParts, new Position16<SpritePixels>(128, 128), 0, 0);
            AssertTrue(nativeOam.ImageEquals(installedOam) &&
                       nativeOam.NextByteOffset == installedOam.NextByteOffset,
                $"Botwoon head frame $B3:{installedPointer:X4} matches cartridge OAM");
        }
        AssertEqual(16, selected.Count,
            "Botwoon head selects sixteen distinct visible OAM frames");

        Console.WriteLine(
            "  Botwoon visuals: 16 typed head frames draw the cartridge OAM.");
    }
}
