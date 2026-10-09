using SuperMetroid.Core.Assets;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="PhantoonInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(PhantoonInstructionProgramDefinitions))]
internal abstract class PhantoonInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of compiled address/value entries in Phantoon's mechanics instruction stream.</summary>
    public static int MechanicsWordCount => PhantoonInstructionProgramDefinitions.MechanicsWordCount;

    /// <summary>Resolves an ordinal mechanics entry to its native instruction address and encoded value.</summary>
    /// <param name="index">Zero-based index across Phantoon's compiled mechanics instructions.</param>
    /// <returns>The native instruction address and value for that entry.</returns>
    public static InstructionMechanicsWord MechanicsWord(int index) => PhantoonInstructionProgramDefinitions.MechanicsWord(index);

    /// <summary>Number of visual frame operands embedded in Phantoon's compiled instruction programs.</summary>
    public static int PresentationWordCount => PhantoonInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Returns the bank-relative address of an embedded visual frame operand.</summary>
    /// <param name="index">Zero-based index among Phantoon's compiled presentation operands.</param>
    /// <returns>The native instruction address containing the operand.</returns>
    public static ushort PresentationWordAddress(int index) => PhantoonInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Tests whether a full SNES address points to either byte of a compiled mechanics word.</summary>
    /// <param name="address">Full address in the SNES address space.</param>
    /// <returns><see langword="true"/> for either byte of a mechanics word in bank $A7; otherwise, <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa70000)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < PhantoonInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = PhantoonInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}
