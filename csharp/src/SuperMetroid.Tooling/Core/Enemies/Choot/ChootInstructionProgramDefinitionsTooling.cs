using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Choot;

/// <summary>Development-tool members of <see cref="ChootInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(ChootInstructionProgramDefinitions))]
internal abstract class ChootInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Accounts for idle setup and four mechanic words in each jumping and falling list.</summary>
    public static int MechanicsWordCount => 11;
    /// <summary>Accounts for one idle pose and two poses each while jumping and falling.</summary>
    public static int PresentationWordCount => 5;
    /// <summary>Maps an index through the idle, jumping, and falling instruction words.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount)
            throw new IndexOutOfRangeException();
        ushort address;
        if (index < 3)
            address = (ushort)(ChootInstructionProgramDefinitions.Idle + (index == 2 ? 6 : index * 2));
        else
        {
            int frame = (index - 3) % 4;
            int program = (index - 3) / 4;
            address = (ushort)(ChootInstructionProgramDefinitions.Jumping + (ChootInstructionProgramDefinitions.Falling - ChootInstructionProgramDefinitions.Jumping) * program + (frame == 0 ? 0 : frame * 4 - 2));
        }
        return new(address, ChootInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    /// <summary>Locates a pose operand interleaved with Choot movement instructions.</summary>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount)
            throw new IndexOutOfRangeException();
        return index == 0 ? (ushort)(ChootInstructionProgramDefinitions.Idle + 4) :
            (ushort)(ChootInstructionProgramDefinitions.Jumping + (ChootInstructionProgramDefinitions.Falling - ChootInstructionProgramDefinitions.Jumping) * ((index - 1) / 2) + 4 + 4 * ((index - 1) % 2));
    }
    /// <summary>Checks either byte of a compiled Choot mechanic word in bank A2.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xa20000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        return ChootInstructionProgramDefinitions.TryRead(bankAddress, out _) || ChootInstructionProgramDefinitions.TryRead(unchecked((ushort)(bankAddress - 1)), out _);
    }
}
