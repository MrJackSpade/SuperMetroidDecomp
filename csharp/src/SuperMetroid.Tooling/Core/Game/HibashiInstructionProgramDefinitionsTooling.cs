using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="HibashiInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(HibashiInstructionProgramDefinitions))]
internal abstract class HibashiInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Gets presentation word count.</summary>
    public static int PresentationWordCount => HibashiInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Presentation word address.</summary>
    public static ushort PresentationWordAddress(int index) => HibashiInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Gets mechanics word count.</summary>
    public static int MechanicsWordCount => 50;
    /// <summary>Mechanics word.</summary>
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
    /// <summary>Checks whether an address identifies a compiled mechanics byte.</summary>
    public static bool IsCompiledMechanicsByte(int address) =>
        (SnesAddressMath.BankBase(address)) == 0xa60000 &&
        (HibashiInstructionProgramDefinitions.TryRead((ushort)address, out _) || HibashiInstructionProgramDefinitions.TryRead((ushort)address - 1, out _));
}
