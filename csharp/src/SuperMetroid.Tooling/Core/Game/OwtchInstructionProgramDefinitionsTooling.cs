using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="OwtchInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(OwtchInstructionProgramDefinitions))]
internal abstract class OwtchInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Counts six compiled control words in each of Owtch's two movement programs.</summary>
    public static int MechanicsWordCount => 12;
    /// <summary>Counts three presentation operands in each Owtch movement program.</summary>
    public static int PresentationWordCount => 6;
    /// <summary>Maps a control-word index across Owtch's two fixed-size movement records.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        int word = index % 6;
        int offset = word == 0 ? 0 : word <= 4 ? 2 + (word - 1) * 4 : 16;
        ushort address = (ushort)(OwtchInstructionProgramDefinitions.MovingLeft + index / 6 * OwtchInstructionProgramDefinitions.ProgramBytes + offset);
        return new(address, OwtchInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    /// <summary>Locates one of the three presentation words within an Owtch movement record.</summary>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(OwtchInstructionProgramDefinitions.MovingLeft + index / 3 * OwtchInstructionProgramDefinitions.ProgramBytes + 4 + index % 3 * 4);
    }
    /// <summary>Checks the six control-word positions in either bank-$A2 Owtch movement record.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xa20000) return false;
        int offset = (ushort)address - OwtchInstructionProgramDefinitions.MovingLeft;
        return (uint)offset < 2 * OwtchInstructionProgramDefinitions.ProgramBytes &&
            ((offset % OwtchInstructionProgramDefinitions.ProgramBytes & ~1) is 0 or 2 or 6 or 10 or 14 or 16);
    }
}
