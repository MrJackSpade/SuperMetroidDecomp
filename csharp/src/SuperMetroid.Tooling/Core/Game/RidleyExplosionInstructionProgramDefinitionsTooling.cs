using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="RidleyExplosionInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(RidleyExplosionInstructionProgramDefinitions))]
internal abstract class RidleyExplosionInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, IDeclaredProgramBank
{
    /// <summary>Bank $A6 owns the Ridley explosion enemy's ordinary spritemap programs.</summary>
    internal const byte Bank = 0xa6;
    static int IDeclaredProgramBank.Bank => Bank;
    public static int PresentationWordCount => RidleyExplosionInstructionProgramDefinitions.ProgramCount;
    public static ushort PresentationWordAddress(int index) => checked((ushort)(RidleyExplosionInstructionProgramDefinitions.First + index * 6 + 2));
    public static int MechanicsWordCount => RidleyExplosionInstructionProgramDefinitions.ProgramCount * 2;
    public static InstructionMechanicsWord MechanicsWord(int index) => new(
        checked((ushort)(RidleyExplosionInstructionProgramDefinitions.First + index / 2 * 6 + (index % 2 == 0 ? 0 : 4))),
        index % 2 == 0 ? (ushort)1 : CommonEnemyInstructionCodes.Sleep);
}
