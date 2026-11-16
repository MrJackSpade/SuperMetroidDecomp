using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Bosses.Crocomire;

/// <summary>
/// Checks the rows Crocomire's poses lie lower on his body's area (#1802) against the cartridge, which lowers his body layer
/// by them as it shows each pose.
/// </summary>
internal static class CrocomireBg2ScrollVerification
{
    /// <summary>Runs the lowered-pose check for all seventeen poses his body layer's placement searches.</summary>
    internal static void VerifyCrocomireBg2ScrollDefinitions(CartridgeImportAddressSpace rom)
    {
        Suite(nameof(VerifyCrocomireLoweredPoses), () => VerifyCrocomireLoweredPoses(rom));
    }

    /// <summary>
    /// The cartridge searches seventeen pose frames, listed at $A4:8B79, and adds the matching frame's fourth component's Y
    /// offset to his body layer's vertical scroll: a pose with offset -n lies n pixels lower. The import places exactly those
    /// poses that much lower and every other frame on its cells. This asserts the import's lowered frames, where the
    /// cartridge-scroll test before #1802 asserted the scroll.
    /// </summary>
    private static void VerifyCrocomireLoweredPoses(CartridgeImportAddressSpace rom)
    {
        var searched = new HashSet<EnemyFrameId>();
        for (int index = 0; index < 17; index++)
        {
            ushort pointer = CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa48b79 + 2 * index);
            ushort offset = CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa40000 | unchecked((ushort)(pointer + 0x1c)));
            EnemyFrameId frame = RoomEnemySlotFrameAccess.TryFrameAt(CrocomireBodyCollision.Bank, pointer)
                ?? throw new InvalidDataException($"Crocomire's searched pose ${pointer:X4} is not a named frame.");
            AssertTrue(searched.Add(frame), "Crocomire's searched poses are distinct");
            AssertEqual(-(short)offset, BossBodySourceDefinitions.FramesLowered.GetValueOrDefault(frame),
                $"Crocomire's {frame} lies as much lower as the cartridge lowers his body layer for it");
        }
        foreach (EnemyFrameId lowered in BossBodySourceDefinitions.FramesLowered.Keys)
            AssertTrue(searched.Contains(lowered), $"{lowered} lies lower only because the cartridge searches it");
    }
}
