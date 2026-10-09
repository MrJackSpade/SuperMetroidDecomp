using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="TorizoChozoOrbInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(TorizoChozoOrbInstructionProgramDefinitions))]
internal abstract class TorizoChozoOrbInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Gets the number of address/value words compiled from the orb's movement and impact instruction lists.</summary>
    public static int MechanicsWordCount => TorizoChozoOrbInstructionProgramDefinitions.MechanicsWordCount;

    /// <summary>Gets one mechanics address/value word in the catalog's stable instruction-list order.</summary>
    /// <param name="index">Zero-based word index; it must be less than <see cref="MechanicsWordCount"/>.</param>
    public static InstructionMechanicsWord MechanicsWord(int index) => TorizoChozoOrbInstructionProgramDefinitions.MechanicsWord(index);

    /// <summary>Gets the number of extracted sprite-frame operands retained for presentation audits.</summary>
    public static int PresentationWordCount => TorizoChozoOrbInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Gets the bank-local instruction address for one extracted sprite-frame operand.</summary>
    /// <param name="index">Zero-based presentation operand index; it must be less than <see cref="PresentationWordCount"/>.</param>
    public static ushort PresentationWordAddress(int index) => TorizoChozoOrbInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Tests whether a full SNES address identifies either byte of one compiled mechanics word.</summary>
    /// <param name="address">Full address to classify; only the enemy-projectile program bank can match.</param>
    /// <returns><see langword="true"/> when the address is a byte owned by one of this catalog's mechanics words.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase) return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < TorizoChozoOrbInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            var word = TorizoChozoOrbInstructionProgramDefinitions.MechanicsWord(index);
            if (bankAddress == word.Address || bankAddress == unchecked((ushort)(word.Address + 1)))
                return true;
        }
        return false;
    }
}
