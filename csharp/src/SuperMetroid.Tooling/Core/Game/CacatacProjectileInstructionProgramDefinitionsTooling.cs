using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="CacatacProjectileInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(CacatacProjectileInstructionProgramDefinitions))]
internal abstract class CacatacProjectileInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Gets the number of live spritemap operands embedded in the ten direction programs.</summary>
    public static int PresentationWordCount => CacatacProjectileInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Returns the bank-$86 address of a direction program's live spritemap operand.</summary>
    /// <param name="index">Zero-based direction-program index.</param>
    /// <returns>Address of the indexed program's presentation word.</returns>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is outside the ten programs.</exception>
    public static ushort PresentationWordAddress(int index) => CacatacProjectileInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Gets the number of compiled control words: a wait and sleep command for each direction program.</summary>
    public static int MechanicsWordCount => 2 * CacatacProjectileInstructionProgramDefinitions.ProgramCount;

    /// <summary>Returns one compiled wait or sleep word from the interleaved direction programs.</summary>
    /// <param name="index">Zero-based index into the wait/sleep word sequence.</param>
    /// <returns>The address and value of the selected mechanics word.</returns>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is outside the compiled word sequence.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        bool sleep = (index & 1) != 0;
        return new((ushort)(CacatacProjectileInstructionProgramDefinitions.LeftFacingUp + 6 * (index / 2) + (sleep ? 4 : 0)),
            sleep ? EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep : (ushort)1);
    }

    /// <summary>Tests whether a 24-bit address names either byte of a compiled mechanics word.</summary>
    /// <param name="address">24-bit cartridge address to classify.</param>
    /// <returns><see langword="true"/> for either byte of a compiled word in bank $86; otherwise, <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0x860000)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < MechanicsWordCount; index++)
        {
            ushort wordAddress = MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }

        return false;
    }
}
