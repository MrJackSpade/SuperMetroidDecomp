using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="SaveStationElectricityInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(SaveStationElectricityInstructionProgramDefinitions))]
internal abstract class SaveStationElectricityInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of timer, frame-duration, loop-control, and deletion words in the electricity program.</summary>
    public static int MechanicsWordCount => SaveStationElectricityInstructionProgramDefinitions.MechanicsWordCount;

    /// <summary>Resolves a position in the compiled electricity program to its native address and mechanics value.</summary>
    /// <param name="index">Zero-based position among timer setup, animation, and terminal-control words.</param>
    /// <returns>The bank-local address and value stored at that mechanics slot.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the compiled mechanics sequence.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index) => SaveStationElectricityInstructionProgramDefinitions.MechanicsWord(index);

    /// <summary>Number of animation spritemap operands supplied by extracted save-station presentation art.</summary>
    public static int PresentationWordCount => SaveStationElectricityInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Returns the address of one animation frame's spritemap operand.</summary>
    /// <param name="index">Zero-based index among the presentation operands.</param>
    /// <returns>The bank-local instruction address containing the selected pointer.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the presentation operands.</exception>
    public static ushort PresentationWordAddress(int index) => SaveStationElectricityInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Checks whether an absolute projectile-bank address is either byte of a compiled mechanics word.</summary>
    /// <param name="address">24-bit SNES address to classify.</param>
    /// <returns><see langword="true"/> when the address belongs to the electricity program's mechanics words; otherwise, <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < SaveStationElectricityInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = SaveStationElectricityInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }

        return false;
    }
}
