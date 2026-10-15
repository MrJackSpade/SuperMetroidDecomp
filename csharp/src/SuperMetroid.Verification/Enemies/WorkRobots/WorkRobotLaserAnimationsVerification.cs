using static SuperMetroid.Verification.Enemies.WorkRobots.WorkRobotLaserNativeListAddresses;
using SuperMetroid.Core.Enemies.WorkRobots;
using SuperMetroid.Core.Enemies.WorkRobots.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Verification.Enemies;

namespace SuperMetroid.Verification.Enemies.WorkRobots;

/// <summary>#641: the Work Robot's laser typed programs, each matched step by step to its native bank-$86 list.</summary>
internal static class WorkRobotLaserAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyWorkRobotLaserAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProjectileProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Entries, WorkRobotLaserAnimations.Programs,
            WorkRobotLaserFrameDefinitions.PresentationKey, NativeEnemyProjectileCommonCues.Rules);
        Console.WriteLine($"  Work Robot laser programs: {Entries.Length} programs ({steps} steps) match bank $86.");
    }
}
