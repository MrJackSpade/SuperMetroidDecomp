using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="RipperInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(RipperInstructionProgramDefinitions))]
internal abstract class RipperInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int MechanicsWordCount => 36;
    public static int PresentationWordCount => 24;
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount)
            throw new IndexOutOfRangeException();
        ushort start = RipperInstructionProgramDefinitions.ProgramStart(index / 6);
        int record = index % 6;
        int offset = record < 4 ? record * 4 : 16 + 2 * (record - 4);
        ushort value = record < 4 ? RipperInstructionProgramDefinitions.VisualHold(record) :
            record == 4 ? (ushort)CommonEnemyInstruction.Goto : start;
        return new((ushort)(start + offset), value);
    }
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount)
            throw new IndexOutOfRangeException();
        return (ushort)(RipperInstructionProgramDefinitions.ProgramStart(index / 4) + 4 * (index % 4) + 2);
    }
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa20000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        return RipperInstructionProgramDefinitions.TryRead(bankAddress, out _) || RipperInstructionProgramDefinitions.TryRead(unchecked((ushort)(bankAddress - 1)), out _);
    }
}
