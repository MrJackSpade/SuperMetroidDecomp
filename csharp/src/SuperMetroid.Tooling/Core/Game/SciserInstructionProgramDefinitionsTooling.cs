using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="SciserInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(SciserInstructionProgramDefinitions))]
internal abstract class SciserInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of timing and control words across the four Sciser surface loops.</summary>
    public static int MechanicsWordCount => SciserInstructionProgramDefinitions.SurfaceCount * 8;

    /// <summary>Number of interleaved spritemap-selector operands across the four surface loops.</summary>
    public static int PresentationWordCount => SciserInstructionProgramDefinitions.SurfaceCount * 4;

    /// <summary>Gets a compiled timing or control word and its address in the selected surface loop.</summary>
    /// <param name="index">Zero-based position in address order across the four loops.</param>
    /// <returns>The address and expected value of the selected mechanics word.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside <see cref="MechanicsWordCount"/>.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        int word = index % 8;
        int offset = word < 2 ? word * 2 : word < 6 ? 4 + (word - 2) * 4 : 20 + (word - 6) * 2;
        ushort address = (ushort)(SciserInstructionProgramDefinitions.UpsideRight + index / 8 * SciserInstructionProgramDefinitions.ProgramBytes + offset);
        return new(address, SciserInstructionProgramDefinitions.ReadMechanicsWord(address));
    }

    /// <summary>Gets the address of an interleaved spritemap-selector operand in a Sciser surface loop.</summary>
    /// <param name="index">Zero-based position in address order across the four loops.</param>
    /// <returns>The bank-local address of the selected presentation word.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside <see cref="PresentationWordCount"/>.</exception>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(SciserInstructionProgramDefinitions.UpsideRight + index / 4 * SciserInstructionProgramDefinitions.ProgramBytes + 6 + index % 4 * 4);
    }

    /// <summary>Checks whether an address identifies either byte of compiled Sciser loop mechanics.</summary>
    /// <param name="address">24-bit ROM address to classify.</param>
    /// <returns><see langword="true"/> for a mechanics byte in bank $A3; otherwise, <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa30000) return false;
        int offset = (ushort)address - SciserInstructionProgramDefinitions.UpsideRight;
        return (uint)offset < SciserInstructionProgramDefinitions.SurfaceCount * SciserInstructionProgramDefinitions.ProgramBytes &&
            ((offset % SciserInstructionProgramDefinitions.ProgramBytes & ~1) is 0 or 2 or 4 or 8 or 12 or 16 or 20 or 22);
    }
}
