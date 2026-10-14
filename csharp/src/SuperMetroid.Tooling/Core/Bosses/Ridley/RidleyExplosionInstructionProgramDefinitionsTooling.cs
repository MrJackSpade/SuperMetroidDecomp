using SuperMetroid.Tooling;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Development-tool members of <see cref="RidleyExplosionInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(RidleyExplosionInstructionProgramDefinitions))]
internal abstract class RidleyExplosionInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, IDeclaredProgramBank
{
    /// <summary>Bank $A6 owns the Ridley explosion enemy's ordinary spritemap programs.</summary>
    internal const byte Bank = 0xa6;
    /// <summary>Publishes the declared bank to the program catalog audit.</summary>
    static int IDeclaredProgramBank.Bank => Bank;
    /// <summary>Each explosion program contributes one spritemap operand at byte offset two.</summary>
    public static int PresentationWordCount => RidleyExplosionInstructionProgramDefinitions.ProgramCount;
    /// <summary>Locates the spritemap operand in the indexed six-byte explosion instruction.</summary>
    public static ushort PresentationWordAddress(int index) => checked((ushort)(RidleyExplosionInstructionProgramDefinitions.First + index * 6 + 2));
    /// <summary>Each program has a frame-duration word and a terminal sleep instruction.</summary>
    public static int MechanicsWordCount => RidleyExplosionInstructionProgramDefinitions.ProgramCount * 2;
    /// <summary>Returns the duration or sleep operand, alternating within each six-byte program.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => new(
        checked((ushort)(RidleyExplosionInstructionProgramDefinitions.First + index / 2 * 6 + (index % 2 == 0 ? 0 : 4))),
        index % 2 == 0 ? (ushort)1 : (ushort)CommonEnemyInstruction.Sleep);
}
