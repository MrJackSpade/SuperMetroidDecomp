using static SuperMetroid.TestSupport.MovieReplay.WorkRobotLaserNativeListAddresses;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.WorkRobots;

/// <summary>#641: the Work Robot's laser typed programs, each matched step by step to its native bank-$86 list.</summary>
internal static class WorkRobotLaserAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyWorkRobotLaserAnimations(CartridgeImportAddressSpace rom)
    {
        int steps = Family.CompareAll(CartridgeImportSource.Require(rom));
        Console.WriteLine($"  Work Robot laser programs: {Entries.Length} programs ({steps} steps) match bank $86.");
    }
}
