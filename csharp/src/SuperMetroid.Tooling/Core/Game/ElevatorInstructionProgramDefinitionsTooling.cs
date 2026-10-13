using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="ElevatorInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(ElevatorInstructionProgramDefinitions))]
internal abstract class ElevatorInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Covers the loop's two spaced mechanic words and two trailing words.</summary>
    public static int MechanicsWordCount => 4;
    /// <summary>Covers the two visual operands interleaved with the elevator loop instructions.</summary>
    public static int PresentationWordCount => 2;
    /// <summary>Reads loop mechanic words at four-byte spacing before the trailing two-byte pair.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        ushort address = (ushort)(ElevatorInstructionProgramDefinitions.Loop + (index < 2 ? 4 * index : 8 + 2 * (index - 2)));
        return new(address, ElevatorInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    /// <summary>Locates either visual operand between the loop's first two mechanic words.</summary>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(ElevatorInstructionProgramDefinitions.Loop + 2 + 4 * index);
    }
    /// <summary>Accepts the compiled mechanic byte positions in bank A3's 12-byte loop span.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xa30000) return false;
        int offset = unchecked((ushort)address) - ElevatorInstructionProgramDefinitions.Loop;
        return (uint)offset < 12 && (offset >= 8 || offset % 4 < 2);
    }
}
