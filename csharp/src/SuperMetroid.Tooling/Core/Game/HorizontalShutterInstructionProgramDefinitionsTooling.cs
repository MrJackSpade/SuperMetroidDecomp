using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="HorizontalShutterInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(HorizontalShutterInstructionProgramDefinitions))]
internal abstract class HorizontalShutterInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of compiled mechanics words surrounding the shutter's presentation operand.</summary>
    public static int MechanicsWordCount => 2;

    /// <summary>Number of presentation operands embedded in the shutter instruction program.</summary>
    public static int PresentationWordCount => 1;

    /// <summary>Resolves one mechanics entry to its address and compiled instruction value.</summary>
    /// <param name="index">Zero-based index among the mechanics words.</param>
    /// <returns>The bank-relative address and decoded mechanics value.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the mechanics word range.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        ushort address = (ushort)(HorizontalShutterInstructionProgramDefinitions.Stationary + 4 * index);
        return new(address, HorizontalShutterInstructionProgramDefinitions.ReadMechanicsWord(address));
    }

    /// <summary>Returns the address of the shutter program's embedded presentation operand.</summary>
    /// <param name="index">Presentation operand index; only zero is defined.</param>
    /// <returns>The bank-relative address of the presentation word.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The index is not zero.</exception>
    public static ushort PresentationWordAddress(int index) => index == 0
        ? HorizontalShutterInstructionProgramDefinitions.PresentationWord
        : throw new ArgumentOutOfRangeException(nameof(index));

    /// <summary>Tests whether a full SNES address points to either byte of a compiled mechanics word.</summary>
    /// <param name="address">Full address in the SNES address space.</param>
    /// <returns><see langword="true"/> for either mechanics word's bytes in bank $A2; otherwise, <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa20000) return false;
        int offset = unchecked((ushort)address) - HorizontalShutterInstructionProgramDefinitions.Stationary;
        return (uint)offset < 6 && (offset < 2 || offset >= 4);
    }
}
