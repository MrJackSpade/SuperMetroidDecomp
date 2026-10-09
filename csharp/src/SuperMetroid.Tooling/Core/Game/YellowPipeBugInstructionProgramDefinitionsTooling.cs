using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="YellowPipeBugInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(YellowPipeBugInstructionProgramDefinitions))]
internal abstract class YellowPipeBugInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Gets the total mechanics words across the four straight and arcing flight loops.</summary>
    public static int MechanicsWordCount => 24;

    /// <summary>Gets the number of frame-pointer operands interleaved with the four loops' timer words.</summary>
    public static int PresentationWordCount => 16;

    /// <summary>Each loop displays four timed records followed by Goto and its target; all records calculate on demand.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount)
            throw new IndexOutOfRangeException();
        int program = index / 6;
        int record = index % 6;
        ushort address = (ushort)(YellowPipeBugInstructionProgramDefinitions.FlyingLeft + 20 * program + (record < 4 ? 4 * record : 16 + 2 * (record - 4)));
        return new(address, YellowPipeBugInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    /// <summary>Gets the bank-local address of a frame-pointer operand in the compiled flight loops.</summary>
    /// <param name="index">The zero-based position among all sixteen presentation operands.</param>
    /// <returns>The native address of the selected frame-pointer word.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the compiled presentation-operand range.</exception>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount)
            throw new IndexOutOfRangeException();
        return (ushort)(YellowPipeBugInstructionProgramDefinitions.FlyingLeft + 20 * (index / 4) + 4 * (index % 4) + 2);
    }
    /// <summary>Tests whether a full bank-$B3 address names either byte of a compiled mechanics word.</summary>
    /// <param name="address">The full SNES address to classify.</param>
    /// <returns><see langword="true"/> for timer words, loop opcodes, or loop targets in these programs.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xb30000)
            return false;
        int offset = unchecked((ushort)address) - YellowPipeBugInstructionProgramDefinitions.FlyingLeft;
        if (offset < 0 || offset >= 80)
            return false;
        int local = offset % 20;
        return local >= 16 || (local & 3) < 2;
    }
}
