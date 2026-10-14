using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>Development-tool members of <see cref="YellowPipeBugInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(YellowPipeBugInstructionProgramDefinitions))]
internal abstract class YellowPipeBugInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Counts four timed control words and two loop-control words in each pipe-bug program.</summary>
    public static int MechanicsWordCount => 24;
    /// <summary>Counts four visual operands in each of the four yellow pipe-bug programs.</summary>
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
    /// <summary>Locates the visual word after an indexed timed record in a twenty-byte program.</summary>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount)
            throw new IndexOutOfRangeException();
        return (ushort)(YellowPipeBugInstructionProgramDefinitions.FlyingLeft + 20 * (index / 4) + 4 * (index % 4) + 2);
    }
    /// <summary>Accepts timed control and loop-control bytes across the four bank-$B3 programs.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xb30000)
            return false;
        int offset = unchecked((ushort)address) - YellowPipeBugInstructionProgramDefinitions.FlyingLeft;
        if (offset is < 0 or >= 80)
            return false;
        int local = offset % 20;
        return local >= 16 || (local & 3) < 2;
    }
}
