using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="HibashiInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(HibashiInstructionProgramDefinitions))]
internal abstract class HibashiInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of spritemap operands across Hibashi's eruption graphics and hitbox programs.</summary>
    public static int PresentationWordCount => HibashiInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Returns the address of a spritemap operand in the compiled graphics or hitbox instruction list.</summary>
    /// <param name="index">Zero-based ordinal among the presentation operands in both programs.</param>
    public static ushort PresentationWordAddress(int index) => HibashiInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Number of timer, callback, and control words compiled from Hibashi's two instruction programs.</summary>
    public static int MechanicsWordCount => 50;

    /// <summary>Resolves an ordinal in the compiled mechanics sequence to its address and encoded value.</summary>
    /// <param name="index">Zero-based index across the graphics-program and hitbox-program mechanics words.</param>
    /// <returns>The instruction address and corresponding duration, callback, or opcode value.</returns>
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
    /// <summary>Checks whether a byte address is part of a compiled Hibashi mechanics word in bank $A6.</summary>
    /// <param name="address">24-bit address to test against the graphics and hitbox programs.</param>
    /// <returns><see langword="true"/> when the address identifies either byte of a compiled mechanics word.</returns>
    public static bool IsCompiledMechanicsByte(int address) =>
        (address & 0xff0000) == 0xa60000 &&
        (HibashiInstructionProgramDefinitions.TryRead((ushort)address, out _) || HibashiInstructionProgramDefinitions.TryRead((ushort)address - 1, out _));
}
