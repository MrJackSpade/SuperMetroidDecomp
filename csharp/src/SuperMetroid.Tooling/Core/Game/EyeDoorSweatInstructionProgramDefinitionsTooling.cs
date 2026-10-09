using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="EyeDoorSweatInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(EyeDoorSweatInstructionProgramDefinitions))]
internal abstract class EyeDoorSweatInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Gets the number of compiled eye-door sweat mechanics words.</summary>
    public static int MechanicsWordCount => EyeDoorSweatInstructionProgramDefinitions.MechanicsWordCount;

    /// <summary>Gets a mechanics word from the compiled eye-door sweat instruction program.</summary>
    /// <param name="index">Zero-based position in the mechanics-word sequence.</param>
    /// <returns>The native instruction address and compiled word value.</returns>
    public static InstructionMechanicsWord MechanicsWord(int index) => EyeDoorSweatInstructionProgramDefinitions.MechanicsWord(index);

    /// <summary>Gets the number of spritemap operands extracted from the eye-door sweat program.</summary>
    public static int PresentationWordCount => EyeDoorSweatInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Gets the native address of an extracted spritemap operand.</summary>
    /// <param name="index">Zero-based position in the presentation-operand sequence.</param>
    /// <returns>The operand's address in the instruction program.</returns>
    public static ushort PresentationWordAddress(int index) => EyeDoorSweatInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Determines whether an address identifies either byte of a compiled eye-door sweat mechanics word.</summary>
    /// <param name="address">Full bus address to classify.</param>
    /// <returns><see langword="true"/> for a compiled mechanics byte in the program bank; otherwise, <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < EyeDoorSweatInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = EyeDoorSweatInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }

        return false;
    }
}
