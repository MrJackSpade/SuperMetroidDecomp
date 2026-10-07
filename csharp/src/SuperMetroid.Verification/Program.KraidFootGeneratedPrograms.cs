using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    // Independent decoder of the pinned A7:86E7-893C byte stream: duration/frame
    // pairs, no-operand callbacks, and the terminal $80ED goto with one operand.
    private static (List<InstructionMechanicsWord> Mechanics, List<ushort> Presentation)
        ReadOriginalKraidFootPrograms(SuperMetroidAddressSpace rom)
    {
        var mechanics = new List<InstructionMechanicsWord>();
        var presentation = new List<ushort>();
        for (int address = 0x86e7; address < 0x893d;)
        {
            ushort word = ReadKraidFootInstructionWord(rom, (ushort)address);
            mechanics.Add(new((ushort)address, word));
            if (word < 0x8000)
            {
                presentation.Add((ushort)(address + 2));
                address += 4;
            }
            else if (word == 0x80ed)
            {
                mechanics.Add(new((ushort)(address + 2), ReadKraidFootInstructionWord(rom, (ushort)(address + 2))));
                address += 4;
            }
            else address += 2;
        }
        return (mechanics, presentation);
    }

    private static void VerifyKraidFootGeneratedMechanics(SuperMetroidAddressSpace rom)
    {
        var original = ReadOriginalKraidFootPrograms(rom).Mechanics;
        AssertEqual(193, original.Count, "native Kraid foot mechanics count");
        AssertEqual(original.Count, KraidFootInstructionProgramDefinitions.MechanicsWordCount, "generated mechanics count");
        var bytes = new HashSet<int>();
        for (int index = 0; index < original.Count; index++)
        {
            var word = original[index];
            AssertEqual(word, KraidFootInstructionProgramDefinitions.MechanicsWord(index), "original indexed mechanics");
            AssertEqual(word.Value, KraidFootInstructionProgramDefinitions.ReadMechanicsWord(word.Address), "original mechanics read");
            bytes.Add(word.Address);
            bytes.Add(word.Address + 1);
            AssertThrows<InvalidDataException>(() => KraidFootInstructionProgramDefinitions.ReadMechanicsWord((ushort)(word.Address + 1)),
                "high mechanics byte is not a word address");
        }
        for (int address = 0; address <= ushort.MaxValue; address++)
            AssertEqual(bytes.Contains(address), KraidFootInstructionProgramDefinitions.IsCompiledMechanicsByte(0xa70000 | address),
                "complete native mechanics byte domain");
        AssertTrue(!KraidFootInstructionProgramDefinitions.IsCompiledMechanicsByte(0xa686e7), "wrong mechanics bank");
        AssertThrows<InvalidDataException>(() => KraidFootInstructionProgramDefinitions.ReadMechanicsWord(0x893d), "unused adjacent program");
        AssertThrows<IndexOutOfRangeException>(() => KraidFootInstructionProgramDefinitions.MechanicsWord(-1), "negative mechanics index");
        AssertThrows<IndexOutOfRangeException>(() => KraidFootInstructionProgramDefinitions.MechanicsWord(193), "mechanics index past end");
    }

    private static void VerifyKraidFootGeneratedPresentation(SuperMetroidAddressSpace rom)
    {
        var original = ReadOriginalKraidFootPrograms(rom).Presentation;
        AssertEqual(106, original.Count, "native foot presentation count");
        AssertEqual(original.Count, KraidFootInstructionProgramDefinitions.PresentationWordCount, "generated presentation count");
        for (int index = 0; index < original.Count; index++)
        {
            ushort address = original[index];
            AssertEqual(address, KraidFootInstructionProgramDefinitions.PresentationWordAddress(index), "native presentation address");
            AssertThrows<InvalidDataException>(() => KraidFootInstructionProgramDefinitions.ReadMechanicsWord(address),
                "presentation operand is not mechanics");
        }
        AssertThrows<IndexOutOfRangeException>(() => KraidFootInstructionProgramDefinitions.PresentationWordAddress(-1), "negative presentation index");
        AssertThrows<IndexOutOfRangeException>(() => KraidFootInstructionProgramDefinitions.PresentationWordAddress(106), "presentation index past end");
    }
}
