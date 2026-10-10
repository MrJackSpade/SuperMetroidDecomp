using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Verifies the 35 Kraid foot roots selected by the native instruction stream against catalog identity, two-component records, ordering, materialization, and bounds.</summary>
    /// <param name="rom">Cartridge address space used to read native instruction operands and frame records.</param>
    private static void VerifyKraidFootFrameGeometry(SuperMetroidAddressSpace rom)
    {
        var selected = new SortedSet<ushort>();
        for (int index = 0; index < KraidFootInstructionProgramDefinitionsTooling.PresentationWordCount; index++)
        {
            int operand = 0xa70000 | KraidFootInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
            selected.Add((ushort)(rom.ReadByte(operand) | rom.ReadByte(operand + 1) << 8));
        }
        AssertEqual(35, selected.Count, "original selected Kraid foot roots");
        var expected = selected.Select(pointer => new EnemyExtendedFrameDefinition(0xa7, pointer,
            $"kraid_foot_oam_{pointer:X4}")).ToArray();
        AssertEqual(expected.Length, KraidFootVisualDefinitions.Frames.Length, "Kraid foot sequence length");
        for (int index = 0; index < expected.Length; index++)
        {
            AssertEqual(expected[index], KraidFootVisualDefinitions.Frame(index), "native Kraid foot identity and key");
            AssertEqual(expected[index], KraidFootVisualDefinitions.Frames[index], "Kraid foot indexed sequence");
            int address = 0xa70000 | expected[index].Pointer;
            AssertEqual(2, rom.ReadByte(address) | rom.ReadByte(address + 1) << 8, "native two-component frame size");
        }
        int ordinal = 0;
        foreach (var frame in KraidFootVisualDefinitions.Frames)
            AssertEqual(expected[ordinal++], frame, "Kraid foot enumeration order");
        AssertEqual(35, ordinal, "Kraid foot enumeration count");
        AssertTrue(expected.SequenceEqual(KraidFootVisualDefinitions.Frames.ToArray()), "Kraid foot materialization");
        AssertThrows<IndexOutOfRangeException>(() => KraidFootVisualDefinitions.Frame(-1), "negative foot index");
        AssertThrows<IndexOutOfRangeException>(() => KraidFootVisualDefinitions.Frame(35), "foot index past end");
    }
}
