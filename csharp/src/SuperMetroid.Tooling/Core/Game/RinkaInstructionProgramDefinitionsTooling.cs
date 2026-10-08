using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="RinkaInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(RinkaInstructionProgramDefinitions))]
internal abstract class RinkaInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int PresentationWordCount => RinkaInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => RinkaInstructionProgramDefinitions.PresentationWordAddress(index);
    internal const int WordsPerList = 3 + RinkaInstructionProgramDefinitions.PoseCount + 2;
    public static int MechanicsWordCount => 2 * WordsPerList;
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
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa20000) return false;
        int relative = (address & 0xfffe) - RinkaInstructionProgramDefinitions.OrdinaryInitial;
        return (uint)relative < 2 * RinkaInstructionProgramDefinitions.ListBytes && !RinkaInstructionProgramDefinitions.IsPresentationOffset(relative % RinkaInstructionProgramDefinitions.ListBytes);
    }
}
