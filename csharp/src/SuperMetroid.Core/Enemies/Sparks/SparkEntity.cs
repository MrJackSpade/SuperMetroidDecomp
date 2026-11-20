namespace SuperMetroid.Core.Enemies.Sparks;

/// <summary>One Spark occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Spark species owner.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">What the enemy's population entry tells its species.</param>
internal sealed class SparkEntity(SparkBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, SparkSpawnSettings settings)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <summary>The enemy's state, which its initialization builds as the enemy is built (#2554).</summary>
    internal SparkEnemyState State { get; } = SparkBehavior.InitializeSpark(place, settings);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunSparkMain(Place, State);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => SparkBehavior.AdvanceSparkTypedAnimation(Place, State, context);

    /// <summary>The pass-through shot reaction where the Spark's definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction ShotReaction => Occupant.Definition.Shot == SparkReactions.Shot ? SparkBehavior.PassThroughShot : base.ShotReaction;

    /// <summary>
    /// Passes a bomb that selects the Spark's own shot reaction through it in place of common bomb damage, once the
    /// ignoring check has passed.
    /// </summary>
    /// <param name="context">The bomb contact.</param>
    public override bool TryReplaceCommonBombDamage(EnemyBombContext context)
    {
        if (context.SelectedShot is not null)
            return base.TryReplaceCommonBombDamage(context);
        ProjectilePassThrough.Bomb(context);
        return true;
    }

    /// <summary>The Spark's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects; none where the definition leaves shots to the species.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction reaction) => reaction == SparkReactions.Shot;
}
