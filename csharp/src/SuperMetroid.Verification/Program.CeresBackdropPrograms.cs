using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyCeresBackdropPrograms(ISnesAddressSpace rom)
    {
        Suite(nameof(VerifyCeresBackdropProgram), () => VerifyCeresBackdropProgram(rom, 0xcc3f, 0xcc47, false));
        Suite(nameof(VerifyCeresBackdropProgram), () => VerifyCeresBackdropProgram(rom, 0xccab, 0xccb3, false));
        Suite(nameof(VerifyCeresBackdropProgram), () => VerifyCeresBackdropProgram(rom, 0xccbb, 0xccd5, true));
        Suite(nameof(VerifyCeresBackdropProgram), () => VerifyCeresBackdropProgram(rom, 0xcd83, 0xcd8b, false));
        Suite(nameof(VerifyCeresBackdropProgram), () => VerifyCeresBackdropProgram(rom, 0xcd8b, 0xcd93, false));
        Suite(nameof(VerifyCeresBackdropProgram), () => VerifyCeresBackdropProgram(rom, 0xcd93, 0xcd9b, false));
        Suite(nameof(VerifyCeresBackdropProgram), () => VerifyCeresBackdropProgram(rom, 0xcd9b, 0xcda3, false));
    }

    private static void VerifyCeresBackdropProgram(ISnesAddressSpace rom, ushort start, ushort end, bool title)
    {
        ushort OriginalWord(ushort p) => (ushort)(rom.ReadByte(0x8b0000 | p) |
            rom.ReadByte(0x8b0000 | unchecked((ushort)(p + 1))) << 8);
        for (int p = start; p < end; p++)
        {
            AssertEqual(rom.ReadByte(0x8b0000 | p), CeresDestructionSpriteInstructionDefinitions.ReadByte((ushort)p), "original backdrop program byte");
            if (p + 1 < end)
                AssertEqual(OriginalWord((ushort)p), CeresDestructionSpriteInstructionDefinitions.ReadWord((ushort)p), "original aligned or unaligned backdrop word");
        }
        AssertThrows<InvalidDataException>(() => CeresDestructionSpriteInstructionDefinitions.ReadWord((ushort)(end - 1)), "word cannot cross backdrop list boundary");
        var native = new IntroDiscoverySprite(0, 0, 0, start);
        var compiled = new IntroDiscoverySprite(0, 0, 0, start);
        var nativeCalls = new List<(ushort Opcode, ushort Cursor)>();
        var compiledCalls = new List<(ushort Opcode, ushort Cursor)>();
        for (int frame = 0; frame < (title ? 400 : 32); frame++)
        {
            nativeCalls.Clear();
            compiledCalls.Clear();
            native.Step((ZebesTitleInstruction opcode, ushort cursor) => { nativeCalls.Add(((ushort)opcode, cursor)); return cursor; }, OriginalWord);
            compiled.Step((ZebesTitleInstruction opcode, ushort cursor) => { compiledCalls.Add(((ushort)opcode, cursor)); return cursor; }, CeresDestructionSpriteInstructionDefinitions.ReadWord);
            AssertEqual(native.InstructionPointer, compiled.InstructionPointer, "backdrop cursor each frame");
            AssertEqual(native.SpriteMapPointer, compiled.SpriteMapPointer, "backdrop visibility each frame");
            AssertEqual(native.IsActive, compiled.IsActive, "backdrop lifetime each frame");
            AssertEqual(nativeCalls.Count, compiledCalls.Count, "title callback count each frame");
            for (int i = 0; i < nativeCalls.Count; i++)
                AssertEqual(nativeCalls[i], compiledCalls[i], "title callback identity and continuation each frame");
        }
        AssertEqual(!title, compiled.IsActive, "static loops persist and title deletes");
    }
}
