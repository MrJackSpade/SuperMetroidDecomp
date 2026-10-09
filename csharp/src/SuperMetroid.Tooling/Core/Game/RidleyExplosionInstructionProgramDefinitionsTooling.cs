using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="RidleyExplosionInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(RidleyExplosionInstructionProgramDefinitions))]
internal abstract class RidleyExplosionInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, IDeclaredProgramBank
{
    /// <summary>Bank $A6 owns the Ridley explosion enemy's ordinary spritemap programs.</summary>
    internal const byte Bank = 0xa6;

    /// <summary>Supplies the bank that owns the compiled Ridley explosion programs.</summary>
    static int IDeclaredProgramBank.Bank => Bank;

    /// <summary>Number of Ridley explosion programs with presentation operands.</summary>
    public static int PresentationWordCount => RidleyExplosionInstructionProgramDefinitions.ProgramCount;

    /// <summary>Gets the address of one program's interleaved spritemap selector.</summary>
    /// <param name="index">The zero-based explosion-program index.</param>
    /// <returns>The bank-$A6 address of that program's presentation word.</returns>
    public static ushort PresentationWordAddress(int index) => checked((ushort)(RidleyExplosionInstructionProgramDefinitions.First + index * 6 + 2));

    /// <summary>Number of fixed duration and sleep operands across the explosion programs.</summary>
    public static int MechanicsWordCount => RidleyExplosionInstructionProgramDefinitions.ProgramCount * 2;

    /// <summary>Maps a mechanics slot to its per-program duration or terminal sleep word.</summary>
    /// <param name="index">The zero-based mechanics-word index.</param>
    /// <returns>The instruction address and fixed operand for the selected program slot.</returns>
    public static InstructionMechanicsWord MechanicsWord(int index) => new(
        checked((ushort)(RidleyExplosionInstructionProgramDefinitions.First + index / 2 * 6 + (index % 2 == 0 ? 0 : 4))),
        index % 2 == 0 ? (ushort)1 : CommonEnemyInstructionCodes.Sleep);
}
