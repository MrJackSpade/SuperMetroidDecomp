using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static SuiteTimingVerification;

/// <summary>Represents fake kraid spit velocities verification.</summary>
internal static class FakeKraidSpitVelocitiesVerification
{
    /// <summary>Verifies fake kraid spit horizontal velocity.</summary>
    internal static void VerifyFakeKraidSpitHorizontalVelocity(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyFakeKraidSpitVelocityField), () => VerifyFakeKraidSpitVelocityField(rom, 0, launch => launch.X.RawValue));

    /// <summary>Verifies fake kraid spit vertical velocity.</summary>
    internal static void VerifyFakeKraidSpitVerticalVelocity(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyFakeKraidSpitVelocityField), () => VerifyFakeKraidSpitVelocityField(rom, 2, launch => launch.Y.RawValue));

    /// <summary>Verifies fake kraid spit velocity field.</summary>
    private static void VerifyFakeKraidSpitVelocityField(
        SuperMetroidAddressSpace rom, int fieldOffset, Func<EightEightVector<RoomPixels>, ushort> select)
    {
        foreach (bool right in new[] { false, true })
        {
            for (int projectile = 0; projectile < 2; projectile++)
            {
                int address = 0xa69a48 + (right ? 8 : 0) + projectile * 4 + fieldOffset;
                ushort expected = (ushort)(SnesLittleEndian.Word(FakeKraidSpitVelocityFieldRomByte(rom, address), FakeKraidSpitVelocityFieldRomByte(rom, address + 1)));
                AssertEqual(expected, select(FakeKraidProjectileDefinitions.SpitLaunch(right, projectile)),
                    $"Fake Kraid spit field {fieldOffset}, facing {right}, projectile {projectile}");
            }
            foreach (int projectile in new[] { int.MinValue, -1, 2, int.MaxValue })
                AssertThrows<InvalidDataException>(
                    () => FakeKraidProjectileDefinitions.SpitLaunch(right, projectile),
                    "Fake Kraid spit rejects invalid projectile ordinal before calculating velocity");
        }
    }

    /// <summary>Fake kraid spit velocity field rom byte.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for fake Kraid spit velocity field")]
    private static byte FakeKraidSpitVelocityFieldRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
