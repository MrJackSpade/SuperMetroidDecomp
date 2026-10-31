using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Verification.Enemies;

/// <summary>
/// How a family whose typed programs publish only <see cref="EnemyProjectileCommonCue"/>s matches its native lists: each
/// general bank-$86 command, its operand bytes and the shared cue it becomes.
/// </summary>
internal static class NativeEnemyProjectileCommonCues
{
    /// <summary>Each general command a common-cue family's lists use and how the cue it becomes is checked.</summary>
    private static readonly Dictionary<ushort, NativeCueCommand<EnemyProjectileCommonCue>> Commands = new()
    {
        [(ushort)NativeEnemyProjectileInstruction.Delete] = NativeCueCommand<EnemyProjectileCommonCue>.Exactly(new EnemyProjectileCommonCue.Command(EnemyProjectileCommonCueKind.Delete)),
        [(ushort)NativeEnemyProjectileInstruction.FrameBehaviorInY] = NativeCueCommand<EnemyProjectileCommonCue>.Of<EnemyProjectileCommonCue.SetFrameBehavior>(2, (operands, cue, at) =>
            AssertEqual(cue.FrameBehavior, EnemyProjectileFrameBehaviorAccess.FromWord(LittleEndianBytes.Word(operands[0], operands[1])), $"{at}: pre-instruction")),
        [(ushort)NativeEnemyProjectileInstruction.ClearFrameBehavior] = NativeCueCommand<EnemyProjectileCommonCue>.Exactly(new EnemyProjectileCommonCue.Command(EnemyProjectileCommonCueKind.ClearFrameBehavior)),
        [(ushort)NativeEnemyProjectileInstruction.MoveRandomlyWithinXRadius_YRadius] = NativeCueCommand<EnemyProjectileCommonCue>.Of<EnemyProjectileCommonCue.Scatter>(4, (operands, cue, at) =>
        {
            // The scatter's operands pack each axis's range in the low byte and its center in the high byte.
            AssertEqual(new EnemyProjectileScatter(new(operands[0], operands[2]), new(operands[1], operands[3])), cue.Spread, $"{at}: scatter");
        }),
        [(ushort)NativeEnemyProjectileInstruction.TourianStatueResetDustPosition] = NativeCueCommand<EnemyProjectileCommonCue>.Exactly(new EnemyProjectileCommonCue.Command(EnemyProjectileCommonCueKind.ReturnToOrigin)),
    };

    /// <summary>The oracle rules for a common-cue family.</summary>
    internal static NativeEnemyProjectileCueRules<EnemyProjectileCommonCue> Rules { get; } = NativeCueCommand<EnemyProjectileCommonCue>.Rules(
        Commands, word => word,
        cue => cue is EnemyProjectileCommonCue.Command { Kind: EnemyProjectileCommonCueKind.Delete });
}
