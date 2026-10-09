using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="NuclearWaffleInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(NuclearWaffleInstructionProgramDefinitions))]
internal abstract class NuclearWaffleInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Gets the number of live spritemap operands in the twelve-frame body loop.</summary>
    public static int PresentationWordCount => NuclearWaffleInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Returns the bank-$A6 address of a frame's interleaved spritemap operand.</summary>
    /// <param name="index">Zero-based frame index in the body loop.</param>
    /// <returns>Address of the selected presentation word.</returns>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is outside the twelve frames.</exception>
    public static ushort PresentationWordAddress(int index) => NuclearWaffleInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Gets the number of mechanics words: one duration per frame plus the terminal loop command and target.</summary>
    public static int MechanicsWordCount => NuclearWaffleInstructionProgramDefinitions.FrameCount + 2;

    /// <summary>Returns a frame duration or one of the loop's terminal control words.</summary>
    /// <param name="index">Zero-based index through the twelve durations, goto command, and loop target.</param>
    /// <returns>The address and value of the selected mechanics word.</returns>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is outside the mechanics-word sequence.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        ushort address = (ushort)(NuclearWaffleInstructionProgramDefinitions.BodyLoop + (index < NuclearWaffleInstructionProgramDefinitions.FrameCount ? index * 4 : NuclearWaffleInstructionProgramDefinitions.FrameCount * 4 + (index - NuclearWaffleInstructionProgramDefinitions.FrameCount) * 2));
        return new(address, NuclearWaffleInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    /// <summary>Checks whether a bank-$A6 byte is loop timing or control data rather than a spritemap operand.</summary>
    /// <param name="address">24-bit cartridge address to classify.</param>
    /// <returns><see langword="true"/> for a byte of a compiled mechanics word; otherwise, <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa60000) return false;
        int offset = (ushort)address - NuclearWaffleInstructionProgramDefinitions.BodyLoop;
        return (uint)offset < NuclearWaffleInstructionProgramDefinitions.FrameCount * 4 + 4 && (offset >= NuclearWaffleInstructionProgramDefinitions.FrameCount * 4 || offset % 4 < 2);
    }
}
