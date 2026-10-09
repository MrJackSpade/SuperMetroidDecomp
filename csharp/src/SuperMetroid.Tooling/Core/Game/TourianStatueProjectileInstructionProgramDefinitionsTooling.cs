using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="TourianStatueProjectileInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(TourianStatueProjectileInstructionProgramDefinitions))]
internal abstract class TourianStatueProjectileInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of compiled mechanics operands across the Tourian statue projectile programs.</summary>
    public static int MechanicsWordCount => TourianStatueProjectileInstructionProgramDefinitions.MechanicsWordCount;

    /// <summary>Returns a compiled instruction address and mechanics operand at its catalog position.</summary>
    /// <param name="index">Zero-based position among the mechanics words from all statue projectile programs.</param>
    /// <returns>The bank-$86 address and value of the selected mechanics operand.</returns>
    public static InstructionMechanicsWord MechanicsWord(int index) => TourianStatueProjectileInstructionProgramDefinitions.MechanicsWord(index);

    /// <summary>Number of pose-duration operands whose addresses select installed statue presentation data.</summary>
    public static int PresentationWordCount => TourianStatueProjectileInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Maps a pose ordinal to the address of its presentation-selection operand.</summary>
    /// <param name="index">Zero-based position among the presentation words in the compiled statue programs.</param>
    public static ushort PresentationWordAddress(int index) => TourianStatueProjectileInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Checks whether a byte address belongs to a compiled bank-$86 statue-projectile mechanics word.</summary>
    /// <param name="address">24-bit address to test against the Tourian statue programs.</param>
    /// <returns><see langword="true"/> when the address identifies either byte of a mechanics operand.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase) return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < TourianStatueProjectileInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            var word = TourianStatueProjectileInstructionProgramDefinitions.MechanicsWord(index);
            if (bankAddress == word.Address || bankAddress == unchecked((ushort)(word.Address + 1)))
                return true;
        }
        return false;
    }
}
