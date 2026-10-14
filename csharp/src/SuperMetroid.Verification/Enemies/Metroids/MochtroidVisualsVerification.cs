using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Hardware.ImportedSpritemapsVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Metroids;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Verification.Enemies.Metroids;

/// <summary>Compares Mochtroid selectors and installed OAM compositions with their retail source frames.</summary>
internal static class MochtroidVisualsVerification
{
    /// <summary>Checks the six frames the two loops show are editable and draw pixel-identically to the cartridge.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for Mochtroid visuals")]
    internal static void VerifyInstalledMochtroidVisuals(
        SuperMetroidAddressSpace rom, EnemyTileArtworkCatalog stock)
    {
        ushort[] poses = [.. MochtroidAnimations.Spritemaps(MochtroidProgram.FreeFlight, MochtroidProgram.Attached).Distinct()];
        AssertEqual(MochtroidVisualDefinitions.FrameCount, poses.Length, "the two loops show six distinct editable OAM frames");
        AssertTrue(poses.ToHashSet().SetEquals(MochtroidVisualDefinitions.Frames().Select(frame => frame.Pointer)),
            "the exported Mochtroid frames are exactly the poses its loops show");
        foreach (ushort pose in poses)
        {
            AssertTrue((stock.Spritemaps ?? throw new InvalidOperationException("Verification requires stock.Spritemaps.")).TryGetDisplay(
                    MochtroidVisualDefinitions.Bank, pose, out EnemySpritemapParts installedParts),
                $"Mochtroid pose ${pose:X4} has editable OAM");
            var nativeOam = new OamBuffer();
            var installedOam = new OamBuffer();
            DrawImportedEnemySpritemap(rom, nativeOam, new EnemySpritemapDraw { Bank = MochtroidVisualDefinitions.Bank, Pointer = pose, Origin = new(128, 128), PaletteBits = 0, BaseTileIndex = 0 });
            installedOam.AddEnemySpritemap(installedParts, new Position16<OamPixels>(128, 128), 0, 0);
            AssertTrue(nativeOam.ImageEquals(installedOam) && nativeOam.NextByteOffset == installedOam.NextByteOffset,
                $"Mochtroid pose ${pose:X4} installed OAM matches cartridge");
        }
        Console.WriteLine("  Mochtroid visuals: six editable OAM frames shown by the two loops and stock parity pass.");
    }
}
