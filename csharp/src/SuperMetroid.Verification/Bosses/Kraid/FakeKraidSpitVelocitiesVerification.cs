using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>Checks both axes of Fake Kraid's two spit launches for each facing direction.</summary>
internal static class FakeKraidSpitVelocitiesVerification
{
    /// <summary>Compares the native X velocity words across both facings and projectile ordinals.</summary>
    internal static void VerifyFakeKraidSpitHorizontalVelocity(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyFakeKraidSpitVelocityField), () => VerifyFakeKraidSpitVelocityField(rom, 0, launch => launch.X.RawValue));

    /// <summary>Compares the native Y velocity words across both facings and projectile ordinals.</summary>
    internal static void VerifyFakeKraidSpitVerticalVelocity(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyFakeKraidSpitVelocityField), () => VerifyFakeKraidSpitVelocityField(rom, 2, launch => launch.Y.RawValue));

    /// <summary>Checks one velocity field against bank-$A6 words and rejects unsupported projectile ordinals.</summary>
    private static void VerifyFakeKraidSpitVelocityField(
        SuperMetroidAddressSpace rom, int fieldOffset, Func<EightEightVector<RoomPixels>, ushort> select)
    {
        foreach (bool right in new[] { false, true })
        {
            for (int projectile = 0; projectile < 2; projectile++)
            {
                int address = 0xa69a48 + (right ? 8 : 0) + projectile * 4 + fieldOffset;
                ushort expected = LittleEndianBytes.Word(FakeKraidSpitVelocityFieldRomByte(rom, address), FakeKraidSpitVelocityFieldRomByte(rom, address + 1));
                AssertEqual(expected, select(FakeKraidProjectileDefinitions.SpitLaunch(right, projectile)),
                    $"Fake Kraid spit field {fieldOffset}, facing {right}, projectile {projectile}");
            }
            foreach (int projectile in new[] { int.MinValue, -1, 2, int.MaxValue })
                AssertThrows<InvalidDataException>(
                    () => FakeKraidProjectileDefinitions.SpitLaunch(right, projectile),
                    "Fake Kraid spit rejects invalid projectile ordinal before calculating velocity");
        }
    }

    /// <summary>Reads a byte from the native launch-vector table for the axis comparison.</summary>
    private static byte FakeKraidSpitVelocityFieldRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
