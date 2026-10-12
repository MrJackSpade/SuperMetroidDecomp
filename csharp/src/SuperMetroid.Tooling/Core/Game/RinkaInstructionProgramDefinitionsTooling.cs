using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="RinkaInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(RinkaInstructionProgramDefinitions))]
internal abstract class RinkaInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Gets the number of presentation words retained as cartridge data.</summary>
    public static int PresentationWordCount => RinkaInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Gets the cartridge address of the presentation word at the given index.</summary>
    public static ushort PresentationWordAddress(int index) => RinkaInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Defines words per list.</summary>
    internal const int WordsPerList = 3 + RinkaInstructionProgramDefinitions.PoseCount + 2;
    /// <summary>Gets the number of compiled mechanics words.</summary>
    public static int MechanicsWordCount => 2 * WordsPerList;
    /// <summary>Gets the compiled mechanics word at the given index.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        int part = index % WordsPerList;
        int offset = part switch
        {
            0 => 0,
            1 => 2,
            2 => 6,
            _ when part < 3 + RinkaInstructionProgramDefinitions.PoseCount => RinkaInstructionProgramDefinitions.SetupBytes + (part - 3) * 4,
            _ => RinkaInstructionProgramDefinitions.SetupBytes + RinkaInstructionProgramDefinitions.PoseCount * 4 + (part - 3 - RinkaInstructionProgramDefinitions.PoseCount) * 2,
        };
        ushort address = (ushort)(RinkaInstructionProgramDefinitions.OrdinaryInitial + index / WordsPerList * RinkaInstructionProgramDefinitions.ListBytes + offset);
        return new(address, RinkaInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    /// <summary>Checks whether the address belongs to compiled mechanics data.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xa20000) return false;
        int relative = (address & 0xfffe) - RinkaInstructionProgramDefinitions.OrdinaryInitial;
        return (uint)relative < 2 * RinkaInstructionProgramDefinitions.ListBytes && !RinkaInstructionProgramDefinitions.IsPresentationOffset(relative % RinkaInstructionProgramDefinitions.ListBytes);
    }
}
