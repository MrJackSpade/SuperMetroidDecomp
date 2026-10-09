using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="MochtroidInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(MochtroidInstructionProgramDefinitions))]
internal abstract class MochtroidInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of compiled timing, Goto, and target words across the two Mochtroid instruction loops.</summary>
    public static int MechanicsWordCount => 12;
    /// <summary>Number of compiled spritemap operand words, four for each of the two instruction loops.</summary>
    public static int PresentationWordCount => 8;
    /// <summary>Each loop displays four timed records followed by Goto and its target; all records calculate on demand.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount)
            throw new IndexOutOfRangeException();
        int program = index / 6;
        int record = index % 6;
        ushort address = (ushort)(MochtroidInstructionProgramDefinitions.FreeFlight + 20 * program + (record < 4 ? 4 * record : 16 + 2 * (record - 4)));
        return new(address, MochtroidInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    /// <summary>Returns the address of one compiled spritemap operand in loop order.</summary>
    /// <param name="index">Zero-based operand index across the free-flight and attached loops.</param>
    /// <exception cref="IndexOutOfRangeException">The index is outside the eight compiled operands.</exception>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount)
            throw new IndexOutOfRangeException();
        return (ushort)(MochtroidInstructionProgramDefinitions.FreeFlight + 20 * (index / 4) + 4 * (index % 4) + 2);
    }
    /// <summary>Reports whether a bank-$A3 address points to a byte in a compiled timing, Goto, or Goto-target word.</summary>
    /// <param name="address">The full banked address to classify.</param>
    /// <returns><see langword="true"/> for compiled instruction bytes and <see langword="false"/> for other addresses, including spritemap operands.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa30000)
            return false;
        int offset = unchecked((ushort)address) - MochtroidInstructionProgramDefinitions.FreeFlight;
        if (offset < 0 || offset >= 40)
            return false;
        int local = offset % 20;
        return local >= 16 || (local & 3) < 2;
    }
}
