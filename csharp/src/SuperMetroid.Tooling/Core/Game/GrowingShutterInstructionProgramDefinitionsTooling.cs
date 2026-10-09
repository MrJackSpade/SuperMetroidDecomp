using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="GrowingShutterInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(GrowingShutterInstructionProgramDefinitions))]
internal abstract class GrowingShutterInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of compiled duration and sleep words across the four shutter-height programs.</summary>
    public static int MechanicsWordCount => 8;

    /// <summary>Number of live spritemap operands, one for each compiled shutter-height program.</summary>
    public static int PresentationWordCount => 4;

    /// <summary>Resolves an ordinal duration or sleep entry to its address and compiled value.</summary>
    /// <param name="index">Zero-based index across the four shutter-height programs.</param>
    /// <returns>The bank-relative instruction address and mechanics value.</returns>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is outside the eight compiled words.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        ushort address = (ushort)(GrowingShutterInstructionProgramDefinitions.TenPixels + 6 * (index / 2) + 4 * (index % 2));
        return new(address, GrowingShutterInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    /// <summary>Returns the address of a height program's interleaved spritemap operand.</summary>
    /// <param name="index">Zero-based shutter-height program index.</param>
    /// <returns>The bank-relative address of the live presentation word.</returns>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is outside the four compiled programs.</exception>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(GrowingShutterInstructionProgramDefinitions.TenPixels + 6 * index + 2);
    }
    /// <summary>Recognizes mechanics bytes in the bank-$A2 shutter programs while excluding their spritemap operands.</summary>
    /// <param name="address">Full SNES address of the byte to classify.</param>
    /// <returns><see langword="true"/> for a byte belonging to a compiled duration or sleep word.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa20000) return false;
        int offset = unchecked((ushort)address) - GrowingShutterInstructionProgramDefinitions.TenPixels;
        return (uint)offset < 24 && (offset % 6 < 2 || offset % 6 >= 4);
    }
}
