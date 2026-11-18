using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Polyp;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Polyp;

/// <summary>Checks that compiled Polyp launch selectors preserve the cartridge tables for every possible RNG word.</summary>
internal static class PolypLaunchDefinitionsVerification
{
    /// <summary>Compares cooldown, launch place along the speed curve, and signed X velocity against their native bank-$A2 table selections.</summary>
    internal static void VerifyCompiledPolypLaunchDefinitions(CartridgeImportAddressSpace rom)
    {
        for (int random = 0; random <= ushort.MaxValue; random++)
        {
            // The cartridge's cooldown runs out on the step below zero; the port holds its true duration, one update more (#2281).
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa2b520 + (random & 0x0e))) + 1, PolypLaunchDefinitions.Cooldown.Choose((ushort)random), "Polyp native cooldown selection");
            AssertEqual(QuadraticSpeedCurvePlace.FromMarks(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa2b530 + (random & 0x1e)))), PolypLaunches.LaunchPlace((ushort)random), "Polyp native launch place, the speed-curve mark of the native Y-index selection");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa2b550 + (random & 0x1e))), PolypLaunchDefinitions.XVelocity.Choose((ushort)random).RawValue, "Polyp native signed X selection");
        }
        Console.WriteLine("Polyp launch definitions: all 65536 RNG words match each of three native table selectors, covering all 40 authored words.");
    }
}
