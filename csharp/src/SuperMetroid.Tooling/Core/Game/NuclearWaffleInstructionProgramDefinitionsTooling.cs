using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="NuclearWaffleInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(NuclearWaffleInstructionProgramDefinitions))]
internal abstract class NuclearWaffleInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Gets presentation word count.</summary>
    public static int PresentationWordCount => NuclearWaffleInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Presentation word address.</summary>
    public static ushort PresentationWordAddress(int index) => NuclearWaffleInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Gets mechanics word count.</summary>
    public static int MechanicsWordCount => NuclearWaffleInstructionProgramDefinitions.FrameCount + 2;
    /// <summary>Mechanics word.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        ushort address = (ushort)(NuclearWaffleInstructionProgramDefinitions.BodyLoop + (index < NuclearWaffleInstructionProgramDefinitions.FrameCount ? index * 4 : NuclearWaffleInstructionProgramDefinitions.FrameCount * 4 + (index - NuclearWaffleInstructionProgramDefinitions.FrameCount) * 2));
        return new(address, NuclearWaffleInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    /// <summary>Checks whether an address identifies a compiled mechanics byte.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xa60000) return false;
        int offset = (ushort)address - NuclearWaffleInstructionProgramDefinitions.BodyLoop;
        return (uint)offset < NuclearWaffleInstructionProgramDefinitions.FrameCount * 4 + 4 && (offset >= NuclearWaffleInstructionProgramDefinitions.FrameCount * 4 || offset % 4 < 2);
    }
}
