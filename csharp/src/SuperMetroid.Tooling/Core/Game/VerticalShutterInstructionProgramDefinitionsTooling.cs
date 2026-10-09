using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="VerticalShutterInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(VerticalShutterInstructionProgramDefinitions))]
internal abstract class VerticalShutterInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of duration and control-flow words for the plain shutter and Kamer platform programs.</summary>
    public static int MechanicsWordCount => 8;

    /// <summary>Number of separately installed spritemap operands in the shutter and platform programs.</summary>
    public static int PresentationWordCount => 5;

    /// <summary>Resolves one mechanics index to its native instruction address and compiled value.</summary>
    /// <param name="index">Zero-based position, with the plain-shutter words before the Kamer platform loop.</param>
    /// <returns>The address and compiled duration or control-flow value for that mechanics slot.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the eight mechanics words.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        int frame = index - 2;
        ushort address = (ushort)(index < 2 ? VerticalShutterInstructionProgramDefinitions.Plain + 4 * index
            : VerticalShutterInstructionProgramDefinitions.KamerPlatform + (frame < 4 ? 4 * frame : 16 + 2 * (frame - 4)));
        return new(address, VerticalShutterInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    /// <summary>Returns the address of one installed-art spritemap operand in the two program layouts.</summary>
    /// <param name="index">Zero-based position among the plain-shutter and Kamer platform presentation slots.</param>
    /// <returns>The bank-$A2 address containing the selected pointer.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the five presentation operands.</exception>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(index == 0 ? VerticalShutterInstructionProgramDefinitions.Plain + 2 : VerticalShutterInstructionProgramDefinitions.KamerPlatform + 2 + 4 * (index - 1));
    }
    /// <summary>Checks whether an absolute bank-$A2 address is a byte of compiled mechanics rather than a spritemap operand.</summary>
    /// <param name="address">24-bit SNES address to classify.</param>
    /// <returns><see langword="true"/> for a byte belonging to a shutter or platform mechanics word; otherwise, <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa20000) return false;
        int bankAddress = unchecked((ushort)address);
        int plainOffset = bankAddress - VerticalShutterInstructionProgramDefinitions.Plain;
        if ((uint)plainOffset < 6) return plainOffset < 2 || plainOffset >= 4;
        int kamerOffset = bankAddress - VerticalShutterInstructionProgramDefinitions.KamerPlatform;
        return (uint)kamerOffset < 20 && (kamerOffset >= 16 || kamerOffset % 4 < 2);
    }
}
