using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="BoyonInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(BoyonInstructionProgramDefinitions))]
internal abstract class BoyonInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Gets the number of instruction words retained as Boyon idle and bouncing mechanics.</summary>
    public static int MechanicsWordCount => 18;

    /// <summary>Gets the number of idle and bouncing spritemap operands extracted as presentation data.</summary>
    public static int PresentationWordCount => 10;

    /// <summary>Gets an idle or bouncing mechanics word by its position in the combined compiled layout.</summary>
    /// <param name="index">Zero-based position in the mechanics-word sequence.</param>
    /// <returns>The native instruction address and its compiled word value.</returns>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is outside the mechanics-word range.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        bool bouncing = index >= 8;
        int word = bouncing ? index - 8 : index;
        int frames = bouncing ? 6 : 4;
        int offset = word < 2 ? 2 * word : word < frames + 2
            ? 4 + 4 * (word - 2) : 4 + 4 * frames + 2 * (word - frames - 2);
        ushort address = (ushort)((bouncing ? BoyonInstructionProgramDefinitions.Bouncing : BoyonInstructionProgramDefinitions.Idle) + offset);
        return new(address, BoyonInstructionProgramDefinitions.ReadMechanicsWord(address));
    }

    /// <summary>Gets the native program address of an extracted idle or bouncing spritemap operand.</summary>
    /// <param name="index">Zero-based position in the combined presentation-operand sequence.</param>
    /// <returns>The address of the operand in the program bank.</returns>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is outside the presentation-word range.</exception>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(index < 4 ? BoyonInstructionProgramDefinitions.Idle + 6 + 4 * index : BoyonInstructionProgramDefinitions.Bouncing + 6 + 4 * (index - 4));
    }

    /// <summary>Determines whether a bank-$A2 address identifies a compiled mechanics byte rather than a presentation operand.</summary>
    /// <param name="address">Full bus address to classify.</param>
    /// <returns><see langword="true"/> for a byte used by compiled mechanics; otherwise, <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa20000) return false;
        int offset = (ushort)address - BoyonInstructionProgramDefinitions.Idle;
        if ((uint)offset >= 56) return false;
        bool bouncing = offset >= 24;
        int local = bouncing ? offset - 24 : offset;
        int tail = bouncing ? 28 : 20;
        return local < 4 || local >= tail || local % 4 < 2;
    }
}
