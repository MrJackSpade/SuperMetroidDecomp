using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="WreckedShipGhostInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(WreckedShipGhostInstructionProgramDefinitions))]
internal abstract class WreckedShipGhostInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Gets the number of compiled pose-duration and loop-control words in the ghost animation.</summary>
    public static int MechanicsWordCount => WreckedShipGhostInstructionProgramDefinitions.MechanicsWordCount;

    /// <summary>Returns a pose duration, terminal goto command, or loop target from the compiled animation.</summary>
    /// <param name="index">Zero-based position among the three pose durations and two loop-control words.</param>
    /// <returns>The address and value of the selected mechanics word.</returns>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is outside the five compiled words.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index) => WreckedShipGhostInstructionProgramDefinitions.MechanicsWord(index);

    /// <summary>Gets the number of pose spritemap operands installed from artwork.</summary>
    public static int PresentationWordCount => WreckedShipGhostInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Returns the bank-$A8 address of an installed ghost-pose spritemap operand.</summary>
    /// <param name="index">Zero-based pose index in the animation.</param>
    /// <returns>The cartridge address of the selected presentation word.</returns>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is outside the three poses.</exception>
    public static ushort PresentationWordAddress(int index) => WreckedShipGhostInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Checks whether a bank-$A8 byte belongs to compiled mechanics rather than a presentation operand.</summary>
    /// <param name="address">24-bit cartridge address to classify.</param>
    /// <returns><see langword="true"/> for either byte of a mechanics word; otherwise, <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa80000)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < WreckedShipGhostInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = WreckedShipGhostInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}
