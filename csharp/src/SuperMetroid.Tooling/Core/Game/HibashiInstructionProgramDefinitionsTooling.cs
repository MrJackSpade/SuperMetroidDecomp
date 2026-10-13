using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="HibashiInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(HibashiInstructionProgramDefinitions))]
internal abstract class HibashiInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Bounds the visual operands interleaved with Hibashi graphics instructions.</summary>
    public static int PresentationWordCount => HibashiInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates a presentation operand for the native instruction audit.</summary>
    public static ushort PresentationWordAddress(int index) => HibashiInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Covers the graphics instruction sequence and its trailing hitbox commands.</summary>
    public static int MechanicsWordCount => 50;
    /// <summary>Walks the paired graphics commands before the separate hitbox program words.</summary>
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
    /// <summary>Checks either byte of a compiled mechanic word within Hibashi's bank A6 program.</summary>
    public static bool IsCompiledMechanicsByte(int address) =>
        (SnesAddressMath.BankBase(address)) == 0xa60000 &&
        (HibashiInstructionProgramDefinitions.TryRead((ushort)address, out _) || HibashiInstructionProgramDefinitions.TryRead((ushort)address - 1, out _));
}
