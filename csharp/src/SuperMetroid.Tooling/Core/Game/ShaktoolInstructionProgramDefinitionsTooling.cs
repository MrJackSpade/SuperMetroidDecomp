using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="ShaktoolInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(ShaktoolInstructionProgramDefinitions))]
internal abstract class ShaktoolInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Gets the number of compiled Shaktool instruction words, excluding interleaved presentation selectors.</summary>
    public static int MechanicsWordCount => 110;

    /// <summary>Gets the number of spritemap-selector operands extracted from the Shaktool instruction programs.</summary>
    public static int PresentationWordCount => 15;

    /// <summary>Gets a non-presentation word by its ordinal in the compiled Shaktool program address range.</summary>
    /// <param name="index">Zero-based position in the mechanics-word sequence.</param>
    /// <returns>The bank-relative address and value of the selected instruction or timing operand.</returns>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is outside the compiled mechanics-word range.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        for (int address = ShaktoolInstructionProgramDefinitions.SawHandAttackPrimaryPiece; address < ShaktoolInstructionProgramDefinitions.FirstAdjacentCodeRoutine; address += 2)
        {
            int value = ShaktoolInstructionProgramDefinitions.ProgramWord((ushort)address);
            if (value != ShaktoolInstructionProgramDefinitions.PresentationOperand && index-- == 0) return new((ushort)address, (ushort)value);
        }
        throw new InvalidOperationException("Shaktool mechanics-word index is inconsistent.");
    }

    /// <summary>Maps an extracted spritemap-selector ordinal to its operand address in bank $AA.</summary>
    /// <param name="index">Zero-based position in the presentation-word sequence.</param>
    /// <returns>The address of the selector operand in its native Shaktool instruction list.</returns>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is outside the presentation-word range.</exception>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        if (index < 6) return (ushort)((index < 3 ? ShaktoolInstructionProgramDefinitions.SawHandPrimaryPiece : ShaktoolInstructionProgramDefinitions.SawHandFinalPiece) + (index % 3) * 4 + 2);
        if (index == 6) return ShaktoolInstructionProgramDefinitions.ArmPieceNormal + 2;
        return (ushort)(ShaktoolInstructionProgramDefinitions.HeadAimingLeft + (index - 7) * 8 + 2);
    }

    /// <summary>Determines whether a bank-$AA byte belongs to a compiled mechanics word rather than a selector operand.</summary>
    /// <param name="address">24-bit SNES address to check.</param>
    /// <returns><see langword="true"/> when the address is either byte of a compiled mechanics word; otherwise, <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xaa0000) return false;
        ushort bankAddress = (ushort)(address & 0xfffe);
        return bankAddress >= ShaktoolInstructionProgramDefinitions.SawHandAttackPrimaryPiece && bankAddress < ShaktoolInstructionProgramDefinitions.FirstAdjacentCodeRoutine && !ShaktoolInstructionProgramDefinitions.IsPresentationWord(bankAddress);
    }
}
