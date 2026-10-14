using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies.Polyp;

namespace SuperMetroid.Verification.Enemies.Polyp;

/// <summary>Checks that compiled Polyp launch selectors preserve the cartridge tables for every possible RNG word.</summary>
internal static class PolypLaunchDefinitionsVerification
{
    /// <summary>Compares cooldown, initial Y index, and signed X velocity against their native bank-$A2 table selections.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Polyp launch definitions")]
    internal static void VerifyCompiledPolypLaunchDefinitions(SuperMetroidAddressSpace rom)
    {
        for (int random = 0; random <= ushort.MaxValue; random++)
        {
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa2b520 + (random & 0x0e))), PolypLaunchDefinitions.Cooldown((ushort)random), "Polyp native cooldown selection");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa2b530 + (random & 0x1e))), PolypLaunchDefinitions.InitialYIndex((ushort)random), "Polyp native Y-index selection");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa2b550 + (random & 0x1e))), PolypLaunchDefinitions.XVelocity((ushort)random), "Polyp native signed X selection");
        }
        Console.WriteLine("Polyp launch definitions: all 65536 RNG words match each of three native table selectors, covering all 40 authored words.");
    }
}
