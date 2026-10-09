using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="YellowPipeBugInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(YellowPipeBugInstructionProgramDefinitions))]
internal abstract class YellowPipeBugInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int MechanicsWordCount => 24;
    public static int PresentationWordCount => 16;
    /// <summary>Each loop displays four timed records followed by Goto and its target; all records calculate on demand.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount)
            throw new IndexOutOfRangeException();
        int program = index / 6;
        int record = index % 6;
        ushort address = (ushort)(YellowPipeBugInstructionProgramDefinitions.FlyingLeft + 20 * program + (record < 4 ? 4 * record : 16 + 2 * (record - 4)));
        return new(address, YellowPipeBugInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount)
            throw new IndexOutOfRangeException();
        return (ushort)(YellowPipeBugInstructionProgramDefinitions.FlyingLeft + 20 * (index / 4) + 4 * (index % 4) + 2);
    }
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xb30000)
            return false;
        int offset = unchecked((ushort)address) - YellowPipeBugInstructionProgramDefinitions.FlyingLeft;
        if (offset is < 0 or >= 80)
            return false;
        int local = offset % 20;
        return local >= 16 || (local & 3) < 2;
    }
}
