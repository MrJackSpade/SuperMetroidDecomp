using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="RidleyExplosionInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(RidleyExplosionInstructionProgramDefinitions))]
internal abstract class RidleyExplosionInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, IDeclaredProgramBank
{
    /// <summary>Bank $A6 owns the Ridley explosion enemy's ordinary spritemap programs.</summary>
    internal const byte Bank = 0xa6;
    /// <summary>Gets bank.</summary>
    static int IDeclaredProgramBank.Bank => Bank;
    /// <summary>Gets presentation word count.</summary>
    public static int PresentationWordCount => RidleyExplosionInstructionProgramDefinitions.ProgramCount;
    /// <summary>Presentation word address.</summary>
    public static ushort PresentationWordAddress(int index) => checked((ushort)(RidleyExplosionInstructionProgramDefinitions.First + index * 6 + 2));
    /// <summary>Gets mechanics word count.</summary>
    public static int MechanicsWordCount => RidleyExplosionInstructionProgramDefinitions.ProgramCount * 2;
    /// <summary>Mechanics word.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => new(
        checked((ushort)(RidleyExplosionInstructionProgramDefinitions.First + index / 2 * 6 + (index % 2 == 0 ? 0 : 4))),
        index % 2 == 0 ? (ushort)1 : (ushort)CommonEnemyInstruction.Sleep);
}
