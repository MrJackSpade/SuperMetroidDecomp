using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="MorphBallEyeInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(MorphBallEyeInstructionProgramDefinitions))]
internal abstract class MorphBallEyeInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of compiled instruction words across the eye-body and mount programs.</summary>
    public static int MechanicsWordCount => MorphBallEyeInstructionProgramDefinitions.MechanicsWordCount;

    /// <summary>Resolves an ordinal mechanics entry to its native instruction address and encoded value.</summary>
    /// <param name="index">Zero-based index in the combined eye-body and mount mechanics sequence.</param>
    /// <returns>The native instruction address and value for that entry.</returns>
    public static InstructionMechanicsWord MechanicsWord(int index) => MorphBallEyeInstructionProgramDefinitions.MechanicsWord(index);

    /// <summary>Number of spritemap operands interleaved with the compiled instruction streams.</summary>
    public static int PresentationWordCount => MorphBallEyeInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Returns the bank-relative address of an interleaved spritemap operand.</summary>
    /// <param name="index">Zero-based index among the eye-body and mount presentation operands.</param>
    /// <returns>The native instruction address containing that operand.</returns>
    public static ushort PresentationWordAddress(int index) => MorphBallEyeInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Tests whether a full SNES address points to either byte of a compiled mechanics word.</summary>
    /// <param name="address">Full address in the SNES address space.</param>
    /// <returns><see langword="true"/> for either byte of a mechanics word in bank $A8; otherwise, <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa80000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < MorphBallEyeInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = MorphBallEyeInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}
