using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="WalkingSpacePirateInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(WalkingSpacePirateInstructionProgramDefinitions))]
internal abstract class WalkingSpacePirateInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of compiled engine-control words across the walking Pirate programs.</summary>
    public static int MechanicsWordCount => WalkingSpacePirateInstructionProgramDefinitions.MechanicsWordCount;

    /// <summary>Resolves an indexed mechanics word to its bank-relative address and encoded value.</summary>
    /// <param name="index">Zero-based index within the compiled mechanics words.</param>
    /// <returns>The native instruction address and value for that mechanics entry.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the mechanics table.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index) => WalkingSpacePirateInstructionProgramDefinitions.MechanicsWord(index);

    /// <summary>Number of pose operands whose addresses are supplied by installed presentation data.</summary>
    public static int PresentationWordCount => WalkingSpacePirateInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Returns the instruction-stream address of an indexed pose's presentation operand.</summary>
    /// <param name="index">Zero-based index within the presentation operands.</param>
    /// <returns>The bank-relative address of the operand following that pose's duration.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the presentation table.</exception>
    public static ushort PresentationWordAddress(int index) => WalkingSpacePirateInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Tests whether a full SNES address points to either byte of a compiled mechanics word.</summary>
    /// <param name="address">Full address in the SNES address space.</param>
    /// <returns><see langword="true"/> for either byte of a mechanics word in bank $B2; otherwise, <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xb20000) return false;
        ushort offset = unchecked((ushort)address);
        for (int index = 0; index < WalkingSpacePirateInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort word = WalkingSpacePirateInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (offset == word || offset == word + 1) return true;
        }
        return false;
    }
}
