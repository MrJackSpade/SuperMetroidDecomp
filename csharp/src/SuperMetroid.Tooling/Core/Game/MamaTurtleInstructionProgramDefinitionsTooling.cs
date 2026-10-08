using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="MamaTurtleInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(MamaTurtleInstructionProgramDefinitions))]
internal abstract class MamaTurtleInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int PresentationWordCount => MamaTurtleInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => MamaTurtleInstructionProgramDefinitions.PresentationWordAddress(index);
    public static int MechanicsWordCount => 117;
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        for (int address = MamaTurtleInstructionProgramDefinitions.BabyCrawlingLeft; address < MamaTurtleInstructionProgramDefinitions.AdjacentMovementDefinitions; address += 2)
            if (MamaTurtleInstructionProgramDefinitions.TryControl((ushort)address, out ushort value) && index-- == 0)
                return new((ushort)address, value);
        throw new InvalidDataException("Turtle control layout is incomplete.");
    }
    public static bool IsCompiledMechanicsByte(int address) =>
        (address & 0xff0000) == 0xa20000 && MamaTurtleInstructionProgramDefinitions.TryControl((ushort)(address & 0xfffe), out _);
}
