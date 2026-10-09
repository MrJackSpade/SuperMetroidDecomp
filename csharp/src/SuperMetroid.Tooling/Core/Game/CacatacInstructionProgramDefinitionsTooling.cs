using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="CacatacInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(CacatacInstructionProgramDefinitions))]
internal abstract class CacatacInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of mechanics words compiled from the upright and inverted loops, 28 per orientation.</summary>
    public static int MechanicsWordCount => 56;
    /// <summary>Number of interleaved visual-selector operands, 12 per orientation.</summary>
    public static int PresentationWordCount => 24;

    /// <summary>Reads a mechanics word from the compiled upright and inverted animation loops.</summary>
    /// <param name="index">Zero-based index in address order across both orientations.</param>
    /// <returns>The ROM address and compiled value of that mechanics word.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside <see cref="MechanicsWordCount"/>.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        int start = CacatacInstructionProgramDefinitions.UpsideUpIdle + index / 28 * 80;
        int step = index % 28;
        int offset = step switch
        {
            0 => 0,
            < 9 => 2 + (step - 1) * 4,
            < 11 => 34 + (step - 9) * 2,
            < 15 => 38 + (step - 11) * 4,
            _ => 54 + (step - 15) * 2,
        };
        ushort address = (ushort)(start + offset);
        return new(address, CacatacInstructionProgramDefinitions.ReadMechanicsWord(address));
    }

    /// <summary>Gets the bank-local ROM address of a visual-selector operand.</summary>
    /// <param name="index">Zero-based visual-operand index in address order across both orientations.</param>
    /// <returns>The address of the selected visual-selector word.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside <see cref="PresentationWordCount"/>.</exception>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        int pose = index % 12;
        return (ushort)(CacatacInstructionProgramDefinitions.UpsideUpIdle + index / 12 * 80 + (pose < 8 ? 4 + pose * 4 : 40 + (pose - 8) * 4));
    }

    /// <summary>Determines whether a ROM byte belongs to a compiled mechanics word for either orientation.</summary>
    /// <param name="address">24-bit ROM address to classify; either byte of a compiled word is accepted.</param>
    /// <returns><see langword="true"/> when the byte is part of compiled instruction mechanics in bank $A2; otherwise, <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address) =>
        (address & 0xff0000) == 0xa20000 && CacatacInstructionProgramDefinitions.TryRead((ushort)(address & ~1), out _);
}
