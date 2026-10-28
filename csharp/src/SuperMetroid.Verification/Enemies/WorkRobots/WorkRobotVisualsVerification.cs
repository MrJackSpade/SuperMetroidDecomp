using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Hardware.ImportedSpritemapsVerification;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.WorkRobots;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.VerificationOracles;

namespace SuperMetroid.Verification.Enemies.WorkRobots;

/// <summary>Checks native Work Robot visual selectors, installed editable
/// OAM frames, and instruction-driven frame selection.</summary>
internal static class WorkRobotVisualsVerification
{
    /// <summary>Checks every pose the Work Robot's programs show against editable stock OAM equal to the cartridge's.</summary>
    internal static void VerifyInstalledWorkRobotVisuals(
        SuperMetroidAddressSpace rom, EnemyTileArtworkCatalog stock)
    {
        HashSet<ushort> shown = WorkRobotAnimationsConstants.Spritemaps(Enum.GetValues<WorkRobotProgram>()).ToHashSet();
        foreach (ushort pointer in shown)
        {
            AssertTrue((stock.Spritemaps ?? throw new InvalidOperationException("Verification requires stock.Spritemaps.")).TryGetDisplay(
                    WorkRobotVisualDefinitions.Bank, pointer, out EnemySpritemapParts installedParts),
                $"Work Robot pose $A8:{pointer:X4} has editable OAM");
            var nativeOam = new OamBuffer();
            var installedOam = new OamBuffer();
            DrawImportedEnemySpritemap(rom, nativeOam, new NativeEnemySpritemapDraw { Bank = WorkRobotVisualDefinitions.Bank, Pointer = pointer, Origin = new(128, 128), PaletteBits = 0, BaseTileIndex = 0 });
            installedOam.AddEnemySpritemap(installedParts, new Position16<SpritePixels>(128, 128), 0, 0);
            AssertTrue(nativeOam.ImageEquals(installedOam) &&
                       nativeOam.NextByteOffset == installedOam.NextByteOffset,
                $"Work Robot pose $A8:{pointer:X4} installed OAM matches cartridge");
        }
        AssertTrue(WorkRobotVisualDefinitions.Frames().Select(frame => frame.Pointer).ToHashSet().SetEquals(shown),
            "the Work Robot's programs show exactly its 27 installed frames");
        Console.WriteLine("  Work Robot visuals: 27 editable OAM frames shown by its programs match the cartridge.");
    }
}
