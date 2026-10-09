using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="KiHunterAcidSpitInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(KiHunterAcidSpitInstructionProgramDefinitions))]
internal abstract class KiHunterAcidSpitInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Gets the number of compiled control words in the two spit introductions and shared splash.</summary>
    public static int MechanicsWordCount => KiHunterAcidSpitInstructionProgramDefinitions.MechanicsWordCount;

    /// <summary>Returns a compiled hold, callback, or control-flow word for the spit projectiles.</summary>
    /// <param name="index">Zero-based index through the left introduction, floor splash, and right introduction.</param>
    /// <returns>The address and value of the selected mechanics word.</returns>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is outside the 27 compiled words.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index) => KiHunterAcidSpitInstructionProgramDefinitions.MechanicsWord(index);

    /// <summary>Gets the number of live spritemap operands in the spit and splash instruction lists.</summary>
    public static int PresentationWordCount => KiHunterAcidSpitInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Returns the bank-relative address of an interleaved live spritemap operand.</summary>
    /// <param name="index">Zero-based operand ordinal through the left, shared-splash, and right lists.</param>
    /// <returns>Address of the selected presentation word.</returns>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is outside the 19 presentation operands.</exception>
    public static ushort PresentationWordAddress(int index) => KiHunterAcidSpitInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Checks whether a bank-$86 byte is part of a compiled control word.</summary>
    /// <param name="address">24-bit cartridge address to classify.</param>
    /// <returns><see langword="true"/> for either byte of a mechanics word; otherwise, <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < KiHunterAcidSpitInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = KiHunterAcidSpitInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }

        return false;
    }
}
