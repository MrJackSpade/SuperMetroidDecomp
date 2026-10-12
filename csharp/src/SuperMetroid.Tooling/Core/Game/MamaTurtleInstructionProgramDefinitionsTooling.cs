using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="MamaTurtleInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(MamaTurtleInstructionProgramDefinitions))]
internal abstract class MamaTurtleInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Gets presentation word count.</summary>
    public static int PresentationWordCount => MamaTurtleInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Presentation word address.</summary>
    public static ushort PresentationWordAddress(int index) => MamaTurtleInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Gets mechanics word count.</summary>
    public static int MechanicsWordCount => 117;
    /// <summary>Mechanics word.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        for (int address = MamaTurtleInstructionProgramDefinitions.BabyCrawlingLeft; address < MamaTurtleInstructionProgramDefinitions.AdjacentMovementDefinitions; address += 2)
            if (MamaTurtleInstructionProgramDefinitions.TryControl((ushort)address, out ushort value) && index-- == 0)
                return new((ushort)address, value);
        throw new InvalidDataException("Turtle control layout is incomplete.");
    }
    /// <summary>Checks whether compiled mechanics byte.</summary>
    public static bool IsCompiledMechanicsByte(int address) =>
        (SnesAddressMath.BankBase(address)) == 0xa20000 && MamaTurtleInstructionProgramDefinitions.TryControl((ushort)(address & 0xfffe), out _);
}
