using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="TorizoExplosiveSwipeInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(TorizoExplosiveSwipeInstructionProgramDefinitions))]
internal abstract class TorizoExplosiveSwipeInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of fixed timing and control operands in the explosive-swipe programs.</summary>
    public static int MechanicsWordCount => TorizoExplosiveSwipeInstructionProgramDefinitions.MechanicsWordCount;

    /// <summary>Maps a mechanics slot to its instruction address and cartridge-defined operand.</summary>
    /// <param name="index">The zero-based mechanics-word index.</param>
    /// <returns>The address and fixed value of the selected mechanics word.</returns>
    public static InstructionMechanicsWord MechanicsWord(int index) => TorizoExplosiveSwipeInstructionProgramDefinitions.MechanicsWord(index);

    /// <summary>Number of sprite-selector operands supplied as explosive-swipe presentation data.</summary>
    public static int PresentationWordCount => TorizoExplosiveSwipeInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Gets the address of one explosive-swipe sprite selector.</summary>
    /// <param name="index">The zero-based presentation slot.</param>
    /// <returns>The address of the selected presentation word.</returns>
    public static ushort PresentationWordAddress(int index) => TorizoExplosiveSwipeInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Checks whether an address is either byte of a compiled mechanics operand.</summary>
    /// <param name="address">The full SNES address to classify.</param>
    /// <returns><see langword="true"/> when the bank address belongs to a fixed mechanics word.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase) return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < TorizoExplosiveSwipeInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            var word = TorizoExplosiveSwipeInstructionProgramDefinitions.MechanicsWord(index);
            if (bankAddress == word.Address || bankAddress == unchecked((ushort)(word.Address + 1)))
                return true;
        }
        return false;
    }
}
