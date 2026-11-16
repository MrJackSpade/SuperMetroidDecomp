using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>Checks both axes of Fake Kraid's two spit launches for each facing direction.</summary>
internal static class FakeKraidSpitVelocitiesVerification
{
    /// <summary>Compares the native X velocity words across both facings and both shots.</summary>
    internal static void VerifyFakeKraidSpitHorizontalVelocity(CartridgeImportAddressSpace rom) =>
        Suite(nameof(VerifyFakeKraidSpitVelocityField), () => VerifyFakeKraidSpitVelocityField(rom, 0, launch => launch.X.RawValue));

    /// <summary>Compares the native Y velocity words across both facings and both shots.</summary>
    internal static void VerifyFakeKraidSpitVerticalVelocity(CartridgeImportAddressSpace rom) =>
        Suite(nameof(VerifyFakeKraidSpitVelocityField), () => VerifyFakeKraidSpitVelocityField(rom, 2, launch => launch.Y.RawValue));

    /// <summary>Checks one velocity field against bank-$A6 words and rejects a shot that is neither of the two.</summary>
    private static void VerifyFakeKraidSpitVelocityField(
        CartridgeImportAddressSpace rom, int fieldOffset, Func<EightEightVector<RoomPixels>, ushort> select)
    {
        foreach (bool right in new[] { false, true })
        {
            HorizontalDirection direction = right ? HorizontalDirection.Right : HorizontalDirection.Left;
            // The native table lists each facing's near shot, then its far one, the order a spit launches them.
            for (int projectile = 0; projectile < 2; projectile++)
            {
                FakeKraidSpit spit = FakeKraidProjectileDefinitions.SpitsInLaunchOrder[projectile];
                int address = 0xa69a48 + (right ? 8 : 0) + projectile * 4 + fieldOffset;
                ushort expected = LittleEndianBytes.Word(FakeKraidSpitVelocityFieldRomByte(rom, address), FakeKraidSpitVelocityFieldRomByte(rom, address + 1));
                AssertEqual(expected, select(FakeKraidProjectiles.SpitLaunch(direction, spit)),
                    $"Fake Kraid spit field {fieldOffset}, facing {direction}, {spit} shot");
            }
            foreach (FakeKraidSpit spit in new[] { (FakeKraidSpit)(-1), (FakeKraidSpit)2 })
                AssertThrows<InvalidDataException>(
                    () => FakeKraidProjectiles.SpitLaunch(direction, spit),
                    "Fake Kraid spit rejects a shot that is neither of its two before calculating velocity");
        }
    }

    /// <summary>Reads a byte from the native launch-vector table for the axis comparison.</summary>
    private static byte FakeKraidSpitVelocityFieldRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
