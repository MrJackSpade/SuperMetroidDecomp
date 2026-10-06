using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyCeresRearPrograms(ISnesAddressSpace rom)
    {
        Suite(nameof(VerifyCeresFlightProgram), () => VerifyCeresFlightProgram(rom, 0xcc47, 0xcc4f, true, true));
        Suite(nameof(VerifyCeresFlightProgram), () => VerifyCeresFlightProgram(rom, 0xcc4f, 0xcc57, true, true));
        Suite(nameof(VerifyCeresFlightProgram), () => VerifyCeresFlightProgram(rom, 0xcc57, 0xcc63, false, true));
    }

    private static void VerifyCeresFlightPrograms(ISnesAddressSpace rom)
    {
        Suite(nameof(VerifyCeresRearPrograms), () => VerifyCeresRearPrograms(rom));
        Suite(nameof(VerifyCeresFlightProgram), () => VerifyCeresFlightProgram(rom, 0xcda3, 0xcdab, false, false));
        Suite(nameof(VerifyCeresFlightProgram), () => VerifyCeresFlightProgram(rom, 0xce4b, 0xce53, false, false));
        foreach (ushort invalid in new ushort[] { 0, 0xcc46, 0xcc63, 0xcda2, 0xcdab, 0xce4a, 0xce53, 0xffff })
        {
            AssertThrows<InvalidDataException>(() => CeresFlightSpriteInstructionDefinitions.ReadByte(invalid), "unowned flight byte");
            AssertThrows<InvalidDataException>(() => CeresFlightSpriteInstructionDefinitions.ReadWord(invalid), "unowned flight word");
        }
    }

    private static void VerifyCeresFlightProgram(ISnesAddressSpace rom, ushort start, ushort end, bool crossesIntoNext, bool destructionAlias)
    {
        ushort OriginalWord(ushort p) => (ushort)(rom.ReadByte(0x8b0000 | p) | rom.ReadByte(0x8b0000 | (p + 1)) << 8);
        for (int p = start; p < end; p++)
        {
            byte expected = rom.ReadByte(0x8b0000 | p);
            AssertEqual(expected, CeresFlightSpriteInstructionDefinitions.ReadByte((ushort)p), "original flight program byte");
            if (destructionAlias)
                AssertEqual(expected, CeresDestructionSpriteInstructionDefinitions.ReadByte((ushort)p), "shared destruction byte alias");
            if (p + 1 < end || crossesIntoNext)
            {
                AssertEqual(OriginalWord((ushort)p), CeresFlightSpriteInstructionDefinitions.ReadWord((ushort)p), "original flight word including admitted cross-list alias");
                if (destructionAlias)
                    AssertEqual(OriginalWord((ushort)p), CeresDestructionSpriteInstructionDefinitions.ReadWord((ushort)p), "shared destruction word alias");
            }
        }
        if (!crossesIntoNext)
            AssertThrows<InvalidDataException>(() => CeresFlightSpriteInstructionDefinitions.ReadWord((ushort)(end - 1)), "flight region boundary");
        var native = new IntroDiscoverySprite(0, 0, 0, start);
        var compiled = new IntroDiscoverySprite(0, 0, 0, start);
        for (int frame = 0; frame < 32; frame++)
        {
            native.Step(rom, instructionWord: OriginalWord);
            compiled.Step(rom, instructionWord: CeresFlightSpriteInstructionDefinitions.ReadWord);
            AssertEqual(native.InstructionPointer, compiled.InstructionPointer, "flight loop cursor");
            AssertEqual(native.SpriteMapPointer, compiled.SpriteMapPointer, "flight loop visible frame");
            AssertEqual(native.IsActive, compiled.IsActive, "flight loop lifetime");
        }
        AssertEqual(true, compiled.IsActive, "flight programs loop indefinitely");
    }
}
