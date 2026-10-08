using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="NuclearWaffleInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(NuclearWaffleInstructionProgramDefinitions))]
internal abstract class NuclearWaffleInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int PresentationWordCount => NuclearWaffleInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => NuclearWaffleInstructionProgramDefinitions.PresentationWordAddress(index);
    public static int MechanicsWordCount => NuclearWaffleInstructionProgramDefinitions.FrameCount + 2;
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        ushort address = (ushort)(NuclearWaffleInstructionProgramDefinitions.BodyLoop + (index < NuclearWaffleInstructionProgramDefinitions.FrameCount ? index * 4 : NuclearWaffleInstructionProgramDefinitions.FrameCount * 4 + (index - NuclearWaffleInstructionProgramDefinitions.FrameCount) * 2));
        return new(address, NuclearWaffleInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa60000) return false;
        int offset = (ushort)address - NuclearWaffleInstructionProgramDefinitions.BodyLoop;
        return (uint)offset < NuclearWaffleInstructionProgramDefinitions.FrameCount * 4 + 4 && (offset >= NuclearWaffleInstructionProgramDefinitions.FrameCount * 4 || offset % 4 < 2);
    }
}
