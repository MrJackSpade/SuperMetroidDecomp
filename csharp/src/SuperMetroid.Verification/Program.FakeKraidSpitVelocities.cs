using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Compares both projectile X-velocity words for each Kraid-facing direction with the native launch table.</summary>
    /// <param name="rom">Address space used to read the retail Fake Kraid projectile table.</param>
    private static void VerifyFakeKraidSpitHorizontalVelocity(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyFakeKraidSpitVelocityField), () => VerifyFakeKraidSpitVelocityField(rom, 0, launch => launch.XVelocity));

    /// <summary>Compares both projectile Y-velocity words for each Kraid-facing direction with the native launch table.</summary>
    /// <param name="rom">Address space used to read the retail Fake Kraid projectile table.</param>
    private static void VerifyFakeKraidSpitVerticalVelocity(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyFakeKraidSpitVelocityField), () => VerifyFakeKraidSpitVelocityField(rom, 2, launch => launch.YVelocity));

    /// <summary>Checks the selected velocity word for both projectile ordinals and facing directions against native data.</summary>
    /// <param name="rom">Address space containing the native launch velocity table at bank $A6.</param>
    /// <param name="fieldOffset">Byte offset of the selected word within each four-byte projectile entry.</param>
    /// <param name="select">Accessor that reads the corresponding compiled velocity from a launch definition.</param>
    private static void VerifyFakeKraidSpitVelocityField(
        SuperMetroidAddressSpace rom, int fieldOffset, Func<FakeKraidSpitLaunch, ushort> select)
    {
        foreach (bool right in new[] { false, true })
        {
            for (int projectile = 0; projectile < 2; projectile++)
            {
                int address = 0xa69a48 + (right ? 8 : 0) + projectile * 4 + fieldOffset;
                ushort expected = (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
                AssertEqual(expected, select(FakeKraidProjectileDefinitions.SpitLaunch(right, projectile)),
                    $"Fake Kraid spit field {fieldOffset}, facing {right}, projectile {projectile}");
            }
            foreach (int projectile in new[] { int.MinValue, -1, 2, int.MaxValue })
                AssertThrows<InvalidDataException>(
                    () => FakeKraidProjectileDefinitions.SpitLaunch(right, projectile),
                    "Fake Kraid spit rejects invalid projectile ordinal before calculating velocity");
        }
    }
}
