using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="FirefleaInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(FirefleaInstructionProgramDefinitions))]
internal abstract class FirefleaInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of duration/control words in the loop, including its command and target.</summary>
    public static int MechanicsWordCount => FirefleaInstructionProgramDefinitions.FrameCount + 2;

    /// <summary>Number of frame spritemap operands available for presentation overrides.</summary>
    public static int PresentationWordCount => FirefleaInstructionProgramDefinitions.FrameCount;
    /// <summary>Calculates alternating timed records followed by the loop command and its target.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount)
            throw new IndexOutOfRangeException();
        if (index < FirefleaInstructionProgramDefinitions.FrameCount)
            return new((ushort)(FirefleaInstructionProgramDefinitions.Loop + index * 4), (ushort)(2 - (index & 1)));
        return new((ushort)(FirefleaInstructionProgramDefinitions.Loop + FirefleaInstructionProgramDefinitions.FrameCount * 4 + (index - FirefleaInstructionProgramDefinitions.FrameCount) * 2),
            index == FirefleaInstructionProgramDefinitions.FrameCount ? CommonEnemyInstructionCodes.Goto : FirefleaInstructionProgramDefinitions.Loop);
    }
    /// <summary>Returns the bank-relative address of one frame's spritemap operand.</summary>
    /// <param name="index">Zero-based Fireflea frame index.</param>
    /// <returns>Instruction-stream address of that frame's presentation word.</returns>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= FirefleaInstructionProgramDefinitions.FrameCount)
            throw new IndexOutOfRangeException();
        return (ushort)(FirefleaInstructionProgramDefinitions.Loop + index * 4 + 2);
    }
    /// <summary>Tests whether a cartridge address names a duration or loop-control byte in the compiled list.</summary>
    /// <param name="address">24-bit cartridge address to classify.</param>
    /// <returns><see langword="true"/> for a compiled mechanics byte, excluding frame sprite operands.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa30000)
            return false;
        int offset = unchecked((ushort)address) - FirefleaInstructionProgramDefinitions.Loop;
        return offset >= 0 &&
            (offset < FirefleaInstructionProgramDefinitions.FrameCount * 4 ? (offset & 3) < 2 : offset < FirefleaInstructionProgramDefinitions.FrameCount * 4 + 4);
    }
}
