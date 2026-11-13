using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Enemies.OldTourianWall;

/// <summary>Development-tool members of <see cref="OldTourianEscapeShaftWallExplosionAnimationDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(OldTourianEscapeShaftWallExplosionAnimationDefinitions))]
internal abstract class OldTourianEscapeShaftWallExplosionInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, IDeclaredProgramBank
{
    /// <summary>The six pose operands, in list order.</summary>
    private static readonly ushort[] FrameOperands =
    [
        OldTourianEscapeShaftWallExplosionInstructionProgramToolingDefinitions.FirstFrameOperand,
        OldTourianEscapeShaftWallExplosionInstructionProgramToolingDefinitions.SecondFrameOperand,
        OldTourianEscapeShaftWallExplosionInstructionProgramToolingDefinitions.ThirdFrameOperand,
        OldTourianEscapeShaftWallExplosionInstructionProgramToolingDefinitions.FourthFrameOperand,
        OldTourianEscapeShaftWallExplosionInstructionProgramToolingDefinitions.FifthFrameOperand,
        OldTourianEscapeShaftWallExplosionInstructionProgramToolingDefinitions.SixthFrameOperand,
    ];

    /// <summary>
    /// The control words of $86:B443-$B46F and each pose's duration, with the values the typed program plays.
    /// $86:B451 is the sound instruction's one-byte operand and each pose's following word is its spritemap.
    /// </summary>
    private static readonly InstructionMechanicsWord[] Words =
    [
        new(0xb443, (ushort)EnemyProjectileInstruction.ClearFrameBehavior),
        new(0xb445, (ushort)EnemyProjectileInstruction.TimerInY),
        new(0xb447, checked(OldTourianEscapeShaftWallExplosionAnimationDefinitions.BurstCount)),
        new(OldTourianEscapeShaftWallExplosionInstructionProgramToolingDefinitions.Loop, (ushort)EnemyProjectileInstruction.MoveRandomlyWithinXRadius_YRadius),
        new(0xb44b, ScatterWord(OldTourianEscapeShaftWallExplosionAnimationDefinitions.Scatter.XRange, OldTourianEscapeShaftWallExplosionAnimationDefinitions.Scatter.Center.X)),
        new(0xb44d, ScatterWord(OldTourianEscapeShaftWallExplosionAnimationDefinitions.Scatter.YRange, OldTourianEscapeShaftWallExplosionAnimationDefinitions.Scatter.Center.Y)),
        new(0xb44f, (ushort)EnemyProjectileInstruction.QueueSoundInY_Lib2_Max6),
        .. FrameOperands.Select((operand, frame) => new InstructionMechanicsWord(
            (ushort)(operand - 2), (ushort)OldTourianEscapeShaftWallExplosionAnimationDefinitions.FrameDurations[frame])),
        new(0xb46a, (ushort)EnemyProjectileInstruction.DecrementTimer_GotoYIfNonZero),
        new(0xb46c, OldTourianEscapeShaftWallExplosionInstructionProgramToolingDefinitions.Loop),
        new(0xb46e, (ushort)EnemyProjectileInstruction.Delete),
    ];

    /// <summary>
    /// The scatter command's packed operand word for one axis: the mask of the bits its range field reads in the low
    /// byte, its centre in the high.
    /// </summary>
    private static ushort ScatterWord(RandomSampleField range, int center) =>
        LittleEndianBytes.Word(checked((byte)((range.Outcomes - 1) << range.Shift)), checked((byte)center));

    /// <summary>Includes fixed control words and one duration before each wall-explosion pose.</summary>
    public static int MechanicsWordCount => Words.Length;
    /// <summary>Pairs a selected $86 instruction address with the value the typed program plays.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => Words[index];
    /// <summary>Bounds pose spritemap operands, excluding the sound operand at $86:B451.</summary>
    public static int PresentationWordCount => FrameOperands.Length;
    /// <summary>The program lives in bank $86, the enemy-projectile code bank.</summary>
    static int IDeclaredProgramBank.Bank => EnemyProjectileCodePointers.BankBase;
    /// <summary>Locates the spritemap word following a wall-explosion pose duration.</summary>
    public static ushort PresentationWordAddress(int index) =>
        (uint)index < (uint)FrameOperands.Length ? FrameOperands[index] : throw new ArgumentOutOfRangeException(nameof(index));

}
