namespace SuperMetroid.Core.Enemies.Sparks;

/// <summary>One Spark occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Spark species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class SparkEntity(SparkBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => SparkBehavior.InitializeSpark;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => species.RunSparkMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => SparkBehavior.AdvanceSparkTypedAnimation;

    /// <summary>The pass-through shot reaction, ahead of the shared ones (only other species' precede it there).</summary>
    public override EnemyShotReaction? FindShot() =>
        SparkBehavior.PassThroughShot.Matches(Slot) ? SparkBehavior.PassThroughShot : base.FindShot();

    /// <summary>The bomb pass-through after the no-op check, ahead of the shared ones.</summary>
    public override Action<EnemyBombContext>? FindBombSpecialAfterNoOp(EnemyBombContext context) =>
        context.SelectedShotAi == EnemyReaction.SparkShot ? ProjectilePassThrough.Bomb : base.FindBombSpecialAfterNoOp(context);

    /// <summary>The Spark's own shot reaction, which normal bombs dispatch like the common one.</summary>
    /// <param name="reaction">The reaction the species definition or the selected hitbox names, or null.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) =>
        reaction == EnemyReaction.SparkShot;
}
