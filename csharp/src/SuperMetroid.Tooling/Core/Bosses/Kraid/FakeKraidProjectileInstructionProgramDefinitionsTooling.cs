using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Development-tool members of <see cref="FakeKraidProjectileInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(FakeKraidProjectileInstructionProgramDefinitions))]
internal abstract class FakeKraidProjectileInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, IDeclaredProgramBank
{
    /// <summary>Bytes in one pose list: its duration, visual operand and terminal sleep.</summary>
    private const int PoseListByteCount = 6;

    /// <summary>The spit, left spike and right spike visual operands, in list order.</summary>
    private static readonly ushort[] PresentationWords =
    [
        FakeKraidProjectileInstructionProgramToolingDefinitions.SpitOperand,
        FakeKraidProjectileInstructionProgramToolingDefinitions.SpikeLeftOperand,
        FakeKraidProjectileInstructionProgramToolingDefinitions.SpikeRightOperand,
    ];

    /// <summary>Each of Fake Kraid's spit and two spike poses has a live spritemap operand.</summary>
    public static int PresentationWordCount => PresentationWords.Length;
    /// <summary>The program lives in bank $86, the enemy-projectile code bank.</summary>
    static int IDeclaredProgramBank.Bank => EnemyProjectileCodePointers.BankBase;

    /// <summary>Locates the artwork selection interleaved between a pose duration and terminal sleep.</summary>
    public static ushort PresentationWordAddress(int index) =>
        (uint)index < (uint)PresentationWords.Length ? PresentationWords[index] : throw new IndexOutOfRangeException();

    /// <summary>The three six-byte projectile poses each contribute a duration and terminal sleep word to compiled control.</summary>
    public static int MechanicsWordCount => 6;

    /// <summary>A Fake Kraid pose's duration or sleep word, as the typed programs play them, skipping the visual operand.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount)
            throw new IndexOutOfRangeException();
        bool sleep = index % 2 != 0;
        ushort address = (ushort)(FakeKraidProjectileInstructionProgramToolingDefinitions.Spit + PoseListByteCount * (index / 2) + (sleep ? 4 : 0));
        return new(address, sleep
            ? (ushort)EnemyProjectileInstruction.Sleep
            : checked((ushort)FakeKraidProjectileInstructionProgramDefinitions.PoseDuration));
    }
}
