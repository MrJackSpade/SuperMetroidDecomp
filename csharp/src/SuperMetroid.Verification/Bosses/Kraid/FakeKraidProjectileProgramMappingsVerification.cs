using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Bosses.Kraid.FakeKraidProjectileInstructionProgramDefinitionsVerification;
using SuperMetroid.Core.Bosses.Kraid;
namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>Checks Fake Kraid spit and spike control ownership separately from visual operands.</summary>
internal static class FakeKraidProjectileProgramMappingsVerification
{
    /// <summary>Compares six native control words and their complete bank-$86 byte domain, rejecting adjacent visual or unrelated data.</summary>
    internal static void VerifyFakeKraidProjectileMechanicsMapping(SuperMetroidAddressSpace rom)
    {
        ushort[] addresses = [0x9dda, 0x9dde, 0x9de0, 0x9de4, 0x9de6, 0x9dea];
        AssertEqual(addresses.Length, FakeKraidProjectileInstructionProgramDefinitionsTooling.MechanicsWordCount,
            "Fake Kraid projectile control word count");
        for (int index = 0; index < addresses.Length; index++)
        {
            ushort address = addresses[index];
            ushort expected = ReadFakeKraidProjectileInstructionWord(rom, address);
            var actual = FakeKraidProjectileInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertEqual(address, actual.Address, "Fake Kraid projectile control enumeration address");
            AssertEqual(expected, actual.Value, "Fake Kraid projectile enumerated native control");
            AssertEqual(expected, FakeKraidProjectileInstructionProgramDefinitions.ReadMechanicsWord(address),
                "Fake Kraid projectile direct native control");
        }
        var words = addresses.ToHashSet();
        for (int address = 0; address <= ushort.MaxValue; address++)
        {
            bool expected = words.Contains((ushort)(address & ~1));
            AssertEqual(expected,
                FakeKraidProjectileInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(0x860000 | address),
                "Fake Kraid projectile complete bank ownership");
            AssertEqual(expected,
                FakeKraidProjectileInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(0x12860000 | address),
                "Fake Kraid projectile preserves high-address-bit masking");
            AssertTrue(!FakeKraidProjectileInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(0xa60000 | address),
                "Fake Kraid projectile rejects other banks");
        }
        for (int address = 0x9dd8; address <= 0x9dee; address++)
            if (!words.Contains((ushort)address))
                AssertThrows<InvalidDataException>(
                    () => FakeKraidProjectileInstructionProgramDefinitions.ReadMechanicsWord((ushort)address),
                    "Fake Kraid projectile rejects visual operands, odd bytes and adjacent routines");
        foreach (ushort address in new ushort[] { 0, ushort.MaxValue })
            AssertThrows<InvalidDataException>(
                () => FakeKraidProjectileInstructionProgramDefinitions.ReadMechanicsWord(address),
                "Fake Kraid projectile rejects distant control addresses");
        foreach (int index in new[] { int.MinValue, -1, 6, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(
                () => FakeKraidProjectileInstructionProgramDefinitionsTooling.MechanicsWord(index),
                "Fake Kraid projectile control enumeration bounds");
    }

    /// <summary>Confirms the three middle visual operands occur two bytes into their six-byte projectile poses.</summary>
    internal static void VerifyFakeKraidProjectilePresentationAddresses()
    {
        ushort[] addresses = [0x9ddc, 0x9de2, 0x9de8];
        AssertEqual(addresses.Length, FakeKraidProjectileInstructionProgramDefinitions.PresentationWordCount,
            "Fake Kraid projectile visual operand count");
        for (int index = 0; index < addresses.Length; index++)
            AssertEqual(addresses[index], FakeKraidProjectileInstructionProgramDefinitions.PresentationWordAddress(index),
                "Fake Kraid projectile native visual operand position");
        foreach (int index in new[] { int.MinValue, -1, 3, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(
                () => FakeKraidProjectileInstructionProgramDefinitions.PresentationWordAddress(index),
                "Fake Kraid projectile visual enumeration bounds");
    }
}
