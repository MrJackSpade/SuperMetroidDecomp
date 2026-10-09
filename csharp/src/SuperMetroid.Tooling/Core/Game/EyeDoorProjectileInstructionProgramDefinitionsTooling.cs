using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="EyeDoorProjectileInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(EyeDoorProjectileInstructionProgramDefinitions))]
internal abstract class EyeDoorProjectileInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of compiled mechanics entries across the projectile's four instruction lists.</summary>
    public static int MechanicsWordCount => EyeDoorProjectileInstructionProgramDefinitions.MechanicsWordCount;

    /// <summary>Resolves one flattened mechanics-list position to its native address and compiled value.</summary>
    /// <param name="index">Zero-based position across the initial, flight, impact, and shot lists.</param>
    /// <returns>The address and instruction value for the selected mechanics word.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the compiled mechanics entries.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index) => EyeDoorProjectileInstructionProgramDefinitions.MechanicsWord(index);

    /// <summary>Number of spritemap pointer operands used by the compiled projectile lists.</summary>
    public static int PresentationWordCount => EyeDoorProjectileInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Returns the native address containing one compiled spritemap pointer.</summary>
    /// <param name="index">Zero-based position among the presentation operands.</param>
    /// <returns>The address of the selected operand word.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the presentation operands.</exception>
    public static ushort PresentationWordAddress(int index) => EyeDoorProjectileInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Checks whether an absolute address identifies either byte of a compiled mechanics word in the projectile bank.</summary>
    /// <param name="address">24-bit SNES address to classify.</param>
    /// <returns><see langword="true"/> when the address belongs to a compiled mechanics word; otherwise, <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase) return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < EyeDoorProjectileInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = EyeDoorProjectileInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress || bankAddress == wordAddress + 1) return true;
        }
        return false;
    }
}
