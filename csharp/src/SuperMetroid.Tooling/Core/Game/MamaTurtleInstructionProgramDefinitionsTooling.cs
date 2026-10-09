using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="MamaTurtleInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(MamaTurtleInstructionProgramDefinitions))]
internal abstract class MamaTurtleInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of compiled spritemap-selector operands across the Mama and Baby Turtle instruction programs.</summary>
    public static int PresentationWordCount => MamaTurtleInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Gets the bank-local address of an indexed spritemap-selector operand.</summary>
    /// <param name="index">Zero-based ordinal among the compiled presentation operands.</param>
    /// <returns>Address of the instruction word containing the selected operand.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside <see cref="PresentationWordCount"/>.</exception>
    /// <exception cref="InvalidDataException">The compiled instruction streams do not contain the requested operand.</exception>
    public static ushort PresentationWordAddress(int index) => MamaTurtleInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Number of timing and control words compiled from the supported Mama and Baby Turtle programs.</summary>
    public static int MechanicsWordCount => 117;

    /// <summary>Gets an indexed compiled timing or control word and its instruction address.</summary>
    /// <param name="index">Zero-based position in the mechanics-word sequence.</param>
    /// <returns>The address and value of the selected mechanics word.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside <see cref="MechanicsWordCount"/>.</exception>
    /// <exception cref="InvalidDataException">The compiled instruction streams do not contain the requested word.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        for (int address = MamaTurtleInstructionProgramDefinitions.BabyCrawlingLeft; address < MamaTurtleInstructionProgramDefinitions.AdjacentMovementDefinitions; address += 2)
            if (MamaTurtleInstructionProgramDefinitions.TryControl((ushort)address, out ushort value) && index-- == 0)
                return new((ushort)address, value);
        throw new InvalidDataException("Turtle control layout is incomplete.");
    }

    /// <summary>Checks whether a byte address belongs to a compiled Mama or Baby Turtle mechanics word.</summary>
    /// <param name="address">24-bit ROM address to classify; either byte of a recognized word is accepted.</param>
    /// <returns><see langword="true"/> for a mechanics byte in bank $A2; otherwise, <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address) =>
        (address & 0xff0000) == 0xa20000 && MamaTurtleInstructionProgramDefinitions.TryControl((ushort)(address & 0xfffe), out _);
}
