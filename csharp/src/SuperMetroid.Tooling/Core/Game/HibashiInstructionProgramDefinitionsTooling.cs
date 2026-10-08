using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="HibashiInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(HibashiInstructionProgramDefinitions))]
internal abstract class HibashiInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int PresentationWordCount => HibashiInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => HibashiInstructionProgramDefinitions.PresentationWordAddress(index);
    public static int MechanicsWordCount => 50;
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        int address = index switch
        {
            0 => HibashiInstructionProgramDefinitions.GraphicsProgram,
            < 47 => HibashiInstructionProgramDefinitions.GraphicsProgram + 2 + (index - 1) / 2 * 6 + (index - 1) % 2 * 4,
            47 => HibashiInstructionProgramDefinitions.HitboxProgram - 2,
            48 => HibashiInstructionProgramDefinitions.HitboxProgram,
            _ => HibashiInstructionProgramDefinitions.HitboxProgram + 4,
        };
        return new((ushort)address, HibashiInstructionProgramDefinitions.ReadMechanicsWord((ushort)address));
    }
    public static bool IsCompiledMechanicsByte(int address) =>
        (address & 0xff0000) == 0xa60000 &&
        (HibashiInstructionProgramDefinitions.TryRead((ushort)address, out _) || HibashiInstructionProgramDefinitions.TryRead((ushort)address - 1, out _));
}
