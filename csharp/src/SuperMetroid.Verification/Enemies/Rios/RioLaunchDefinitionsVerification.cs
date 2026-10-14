using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies.Rios;

namespace SuperMetroid.Verification.Enemies.Rios;

/// <summary>Compares compiled Rio launch words and Norfair's random selector with the retail cartridge.</summary>
internal static class RioLaunchDefinitionsVerification
{
    /// <summary>Checks five fixed velocities and all 65,536 random inputs for Norfair's two Y-launch words.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Rio launches")]
    internal static void VerifyCompiledRioLaunches(SuperMetroidAddressSpace rom)
    {
        AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(0xa2bbbb), RioLaunchDefinitions.RioYVelocity, "Rio NTSC Y launch");
        AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(0xa2bbbf), RioLaunchDefinitions.RioXVelocity, "Rio X launch");
        AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(0xa2c1c5), RioLaunchDefinitions.NorfairXVelocity, "Norfair Rio X launch");
        AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(0xa2c6ca), RioLaunchDefinitions.LowerNorfairYVelocity, "Lower Norfair Rio Y launch");
        AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(0xa2c6ce), RioLaunchDefinitions.LowerNorfairXVelocity, "Lower Norfair Rio X launch");
        for (int random = 0; random <= ushort.MaxValue; random++)
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa2c1c1 + ((random >> 1) & 2))), RioLaunchDefinitions.NorfairYVelocity((ushort)random), "Norfair Rio RNG selector");
        Console.WriteLine("Rio launch definitions: seven native words and all 65536 Norfair RNG selectors match.");
    }
}
