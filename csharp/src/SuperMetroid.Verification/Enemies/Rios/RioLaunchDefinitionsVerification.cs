using SuperMetroid.Core.Enemies.Rios;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Rios;

/// <summary>Compares compiled Rio launch words and Norfair's random selector with the retail cartridge.</summary>
internal static class RioLaunchDefinitionsVerification
{
    /// <summary>
    /// Checks five fixed velocities, all 65,536 random inputs for Norfair's two Y-launch words, and the chance that holds a
    /// Norfair and a Lower Norfair Rio back from attacking against each one's test of its random number (#2380).
    /// </summary>
    internal static void VerifyCompiledRioLaunches(CartridgeImportAddressSpace rom)
    {
        AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(0xa2bbbb), RioLaunchDefinitions.RioYVelocity.RawValue, "Rio NTSC Y launch");
        AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(0xa2bbbf), RioLaunchDefinitions.RioXVelocity.RawValue, "Rio X launch");
        AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(0xa2c1c5), RioLaunchDefinitions.NorfairXVelocity.RawValue, "Norfair Rio X launch");
        AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(0xa2c6ca), RioLaunchDefinitions.LowerNorfairYVelocity.RawValue, "Lower Norfair Rio Y launch");
        AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(0xa2c6ce), RioLaunchDefinitions.LowerNorfairXVelocity.RawValue, "Lower Norfair Rio X launch");
        for (int random = 0; random <= ushort.MaxValue; random++)
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa2c1c1 + ((random >> 1) & 2))), RioLaunches.NorfairYVelocity((ushort)random).RawValue, "Norfair Rio RNG selector");
        // Each idle function masks its random number with an AND operand and stays idle when nothing is left.
        foreach ((int operand, string rio) in new[] { (0xa2c2eb, "Norfair Rio"), (0xa2c775, "Lower Norfair Rio") })
        {
            ushort mask = CartridgeImportSource.Require(rom).ReadCartridgeWord(operand);
            for (int random = 0; random <= ushort.MaxValue; random++)
                AssertEqual((random & mask) == 0, RioLaunchDefinitions.AttackHeldBack.IsTakenBy(random), $"{rio} holds back on random {random:X4}");
        }
        Console.WriteLine("Rio launch definitions: seven native words, all 65536 Norfair RNG selectors and both Rios' held-back chances match.");
    }
}
