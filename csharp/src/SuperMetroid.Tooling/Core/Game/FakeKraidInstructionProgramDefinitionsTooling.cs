using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="FakeKraidInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(FakeKraidInstructionProgramDefinitions))]
internal abstract class FakeKraidInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int MechanicsWordCount => 48;
    public static int PresentationWordCount => 24;
    /// <summary>Enumerates control words in native program order, skipping visual operands.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount)
            throw new IndexOutOfRangeException();
        for (int address = FakeKraidInstructionProgramDefinitions.ChooseActionFacingLeft; address < FakeKraidInstructionProgramDefinitions.FireSpitFacingRight + 24; address += 2)
        {
            if (FakeKraidInstructionProgramDefinitions.TryReadMechanicsWord((ushort)address, out ushort value) && index-- == 0)
                return new((ushort)address, value);
        }
        throw new InvalidOperationException("Fake Kraid instruction enumeration is incomplete.");
    }
    /// <summary>Visual operands follow each frame delay in the paired walking and firing programs.</summary>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount)
            throw new IndexOutOfRangeException();
        int facing = index / 12;
        int frame = index % 12;
        int offset = frame switch
        {
            < 4 => 4 + 4 * frame,
            4 => 0x1c,
            < 8 => 0x22 + 4 * (frame - 5),
            < 11 => 0x32 + 6 * (frame - 8),
            _ => 0x42,
        };
        return (ushort)(FakeKraidInstructionProgramDefinitions.ChooseActionFacingLeft +
            facing * (FakeKraidInstructionProgramDefinitions.ChooseActionFacingRight - FakeKraidInstructionProgramDefinitions.ChooseActionFacingLeft) + offset);
    }
    public static bool IsCompiledMechanicsByte(int address) =>
        (address & 0xff0000) == 0xa60000 &&
        FakeKraidInstructionProgramDefinitions.TryReadMechanicsWord(unchecked((ushort)(address & ~1)), out _);
}
