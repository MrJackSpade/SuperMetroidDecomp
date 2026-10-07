using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    // Independent decoder of the pinned four arm instruction regions: duration/frame
    // pairs, no-operand callbacks, and the terminal $80ED goto with one operand.
    private static (List<InstructionMechanicsWord> Mechanics, List<ushort> Presentation)
        ReadOriginalKraidArmPrograms(SuperMetroidAddressSpace rom)
    {
        var mechanics = new List<InstructionMechanicsWord>();
        var presentation = new List<ushort>();
        foreach (var (start, end) in new[] { (0x89f3, 0x8a41), (0x8a41, 0x8a8f), (0x8aa4, 0x8af0), (0x8af0, 0x8afe) })
        for (int address = start; address < end;)
        {
            ushort word = ReadKraidArmInstructionWord(rom, (ushort)address);
            mechanics.Add(new((ushort)address, word));
            if (word < 0x8000)
            {
                presentation.Add((ushort)(address + 2));
                address += 4;
            }
            else if (word == 0x80ed)
            {
                mechanics.Add(new((ushort)(address + 2), ReadKraidArmInstructionWord(rom, (ushort)(address + 2))));
                address += 4;
            }
            else address += 2;
        }
        return (mechanics, presentation);
    }

    private static void VerifyKraidArmGeneratedMechanics(SuperMetroidAddressSpace rom)
    {
        var original = ReadOriginalKraidArmPrograms(rom).Mechanics;
        AssertEqual(66, original.Count, "native Kraid arm mechanics count");
        AssertEqual(original.Count, KraidArmInstructionProgramDefinitions.MechanicsWordCount, "generated mechanics count");
        var bytes = new HashSet<int>();
        for (int index = 0; index < original.Count; index++)
        {
            var word = original[index];
            AssertEqual(word, KraidArmInstructionProgramDefinitions.MechanicsWord(index), "original indexed mechanics");
            AssertEqual(word.Value, KraidArmInstructionProgramDefinitions.ReadMechanicsWord(word.Address), "original mechanics read");
            bytes.Add(word.Address);
            bytes.Add(word.Address + 1);
            AssertThrows<InvalidDataException>(() => KraidArmInstructionProgramDefinitions.ReadMechanicsWord((ushort)(word.Address + 1)),
                "high mechanics byte is not a word address");
        }
        for (int address = 0; address <= ushort.MaxValue; address++)
            AssertEqual(bytes.Contains(address), KraidArmInstructionProgramDefinitions.IsCompiledMechanicsByte(0xa70000 | address),
                "complete native mechanics byte domain");
        AssertTrue(!KraidArmInstructionProgramDefinitions.IsCompiledMechanicsByte(0xa689f3), "wrong mechanics bank");
        AssertThrows<InvalidDataException>(() => KraidArmInstructionProgramDefinitions.ReadMechanicsWord(0x8a8f), "native callback body is not list data");
        AssertThrows<InvalidDataException>(() => KraidArmInstructionProgramDefinitions.ReadMechanicsWord(0x8afe), "adjacent lint program");
        AssertThrows<IndexOutOfRangeException>(() => KraidArmInstructionProgramDefinitions.MechanicsWord(-1), "negative mechanics index");
        AssertThrows<IndexOutOfRangeException>(() => KraidArmInstructionProgramDefinitions.MechanicsWord(66), "mechanics index past end");
    }

    private static void VerifyKraidArmGeneratedPresentation(SuperMetroidAddressSpace rom)
    {
        var original = ReadOriginalKraidArmPrograms(rom).Presentation;
        AssertEqual(57, original.Count, "native arm presentation count");
        AssertEqual(original.Count, KraidArmInstructionProgramDefinitions.PresentationWordCount, "generated presentation count");
        for (int index = 0; index < original.Count; index++)
        {
            ushort address = original[index];
            AssertEqual(address, KraidArmInstructionProgramDefinitions.PresentationWordAddress(index), "native presentation address");
            AssertThrows<InvalidDataException>(() => KraidArmInstructionProgramDefinitions.ReadMechanicsWord(address),
                "presentation operand is not mechanics");
        }
        AssertThrows<IndexOutOfRangeException>(() => KraidArmInstructionProgramDefinitions.PresentationWordAddress(-1), "negative presentation index");
        AssertThrows<IndexOutOfRangeException>(() => KraidArmInstructionProgramDefinitions.PresentationWordAddress(57), "presentation index past end");
    }
}
