using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyCeresInitialExplosionProgram(ISnesAddressSpace rom) => VerifyCeresExplosionProgram(rom, 0xccdb, 0xccf5);
    private static void VerifyCeresRepeatingExplosionProgram(ISnesAddressSpace rom) => VerifyCeresExplosionProgram(rom, 0xccf5, 0xcd1b);
    private static void VerifyCeresFinalWaveProgram(ISnesAddressSpace rom) => VerifyCeresExplosionProgram(rom, 0xcd1b, 0xcd39);
    private static void VerifyCeresStationBlastProgram(ISnesAddressSpace rom) => VerifyCeresExplosionProgram(rom, 0xce1b, 0xce35);

    private static void VerifyCeresExplosionProgram(ISnesAddressSpace rom, ushort start, ushort end)
    {
        for (int pointer = start; pointer < end; pointer++)
        {
            AssertEqual(rom.ReadByte(0x8b0000 | pointer), CeresDestructionSpriteInstructionDefinitions.ReadByte((ushort)pointer), "original explosion program byte");
            if (pointer + 1 < end)
            {
                ushort expected = (ushort)(rom.ReadByte(0x8b0000 | pointer) | rom.ReadByte(0x8b0000 | (pointer + 1)) << 8);
                AssertEqual(expected, CeresDestructionSpriteInstructionDefinitions.ReadWord((ushort)pointer), "original aligned or unaligned explosion word");
            }
        }
        AssertThrows<InvalidDataException>(() => CeresDestructionSpriteInstructionDefinitions.ReadWord((ushort)(end - 1)), "word cannot cross explosion program boundary");
        foreach (ushort invalid in new ushort[] { 0, 0xccda, 0xcd39, 0xce1a, 0xce35, 0xffff })
            AssertThrows<InvalidDataException>(() => CeresExplosionInstructionDefinitions.ReadByte(invalid), "explosion-only reader rejects unrelated data");

        // Confirm the actual interpreter's timing and terminal deletion, including
        // a delayed first instruction independently of its repeat-count timer.
        foreach (ushort delay in new ushort[] { 1, 16 })
        {
            var native = new IntroDiscoverySprite(0, 0, 0, start);
            var compiled = new IntroDiscoverySprite(0, 0, 0, start);
            native.DelayFirstInstruction(delay);
            compiled.DelayFirstInstruction(delay);
            for (int frame = 0; frame < 256; frame++)
            {
                native.Step(instructionWord: pointer =>
                    (ushort)(rom.ReadByte(0x8b0000 | pointer) |
                    rom.ReadByte(0x8b0000 | unchecked((ushort)(pointer + 1))) << 8));
                compiled.Step(instructionWord: CeresDestructionSpriteInstructionDefinitions.ReadWord);
                AssertEqual(native.InstructionPointer, compiled.InstructionPointer, "explosion instruction cursor");
                AssertEqual(native.SpriteMapPointer, compiled.SpriteMapPointer, "explosion frame and blank interval");
                AssertEqual(native.GeneralTimer, compiled.GeneralTimer, "explosion loop count");
                AssertEqual(native.IsActive, compiled.IsActive, "explosion lifetime");
            }
            AssertEqual(false, compiled.IsActive, "finite explosion program deletes");
        }
    }
}
