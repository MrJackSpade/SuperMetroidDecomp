namespace SuperMetroid.Core.Enemies.Sparks;

/// <summary>One Spark occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Spark species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class SparkEntity(SparkBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyPrivateShotCallback
{
    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => species.InitializeSpark;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => species.RunSparkMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => SparkBehavior.AdvanceSparkTypedAnimation;

    /// <summary>The pass-through shot reaction, ahead of the shared ones (only other species' precede it there).</summary>
    public override EnemyShotReaction? FindShot() =>
        SparkBehavior.PassThroughShot.Matches(Slot) ? SparkBehavior.PassThroughShot : base.FindShot();

    /// <summary>The bomb pass-through after the no-op check, ahead of the shared ones.</summary>
    public override Action<EnemyBombContext>? FindBombSpecialAfterNoOp(EnemyBombContext context) =>
        context.SelectedShotAi == EnemyInteractionCallback.SparkShot ? ProjectilePassThrough.Bomb : base.FindBombSpecialAfterNoOp(context);

    /// <summary>The Spark's own shot callback, which normal bombs dispatch like the common one.</summary>
    /// <param name="callback">The named callback the selected pointer decodes to, or null.</param>
    public bool OwnsPrivateShotCallback(EnemyInteractionCallback? callback) =>
        callback == EnemyInteractionCallback.SparkShot;
}
