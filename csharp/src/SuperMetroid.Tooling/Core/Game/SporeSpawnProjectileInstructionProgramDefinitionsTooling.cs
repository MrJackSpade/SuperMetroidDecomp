using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="SporeSpawnProjectileInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(SporeSpawnProjectileInstructionProgramDefinitions))]
internal abstract class SporeSpawnProjectileInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of compiled address/value pairs across the emitter, spore, stalk, and shot programs.</summary>
    public static int MechanicsWordCount => SporeSpawnProjectileInstructionProgramDefinitions.MechanicsWordCount;

    /// <summary>Resolves a flattened program-word index to its native address and mechanics value.</summary>
    /// <param name="index">Zero-based index spanning the five Spore Spawn projectile instruction lists.</param>
    /// <returns>The bank-local address and compiled value for the selected control word.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside <see cref="MechanicsWordCount"/>.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index) => SporeSpawnProjectileInstructionProgramDefinitions.MechanicsWord(index);

    /// <summary>Number of spritemap operands whose identities resolve through the compiled presentation catalog.</summary>
    public static int PresentationWordCount => SporeSpawnProjectileInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Returns the native address of an interleaved spritemap operand by flattened program order.</summary>
    /// <param name="index">Zero-based index among the presentation operands.</param>
    /// <returns>The bank-local address referenced by the compiled programs.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside <see cref="PresentationWordCount"/>.</exception>
    public static ushort PresentationWordAddress(int index) => SporeSpawnProjectileInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Checks whether a full address selects either byte of a compiled mechanics word.</summary>
    /// <param name="address">Full SNES address to classify.</param>
    /// <returns><see langword="true"/> for mechanics bytes in the projectile program bank; presentation operands and unrelated addresses return <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase) return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < SporeSpawnProjectileInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            var word = SporeSpawnProjectileInstructionProgramDefinitions.MechanicsWord(index);
            if (bankAddress == word.Address || bankAddress == unchecked((ushort)(word.Address + 1)))
                return true;
        }
        return false;
    }
}
