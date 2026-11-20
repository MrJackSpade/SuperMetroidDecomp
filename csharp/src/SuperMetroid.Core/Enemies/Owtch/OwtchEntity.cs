namespace SuperMetroid.Core.Enemies.Owtch;

/// <summary>One Owtch occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Owtch species owner.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">What the enemy's population entry tells its species.</param>
internal sealed class OwtchEntity(OwtchBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common, OwtchSpawnSettings settings)
    : CommonParticipationEnemyEntity(place, common), IEnemyAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <summary>The enemy's own bomb reaction, built over its state (#2554).</summary>
    private EnemyBombReaction OwnOwtchBombReaction => field ??= OwtchBehavior.OwtchBombReactionOf(State);

    /// <summary>The enemy's own shot reaction, built over its state (#2554).</summary>
    private EnemyShotReaction OwnOwtchShotReaction => field ??= OwtchBehavior.OwtchShotReactionOf(State);

    /// <summary>The enemy's state, which its initialization builds as the enemy is built (#2554).</summary>
    internal OwtchEnemyState State { get; } = OwtchBehavior.InitializeOwtch(place, settings);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunOwtchMain(Place, State);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => OwtchBehavior.AdvanceOwtchTypedAnimation(Place, State, context);

    /// <summary>The Owtch's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction? ShotReaction => LeavesShotToSpecies ? OwnOwtchShotReaction : base.ShotReaction;

    /// <summary>The Owtch's own stages around common bomb damage for a bomb that selects its own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction? CommonBombDamageReaction(EnemyBombContext context) =>
        context.SelectedShot is null ? OwnOwtchBombReaction : base.CommonBombDamageReaction(context);

    /// <summary>The Owtch's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects; none where the definition leaves shots to the species.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) => reaction is null && LeavesShotToSpecies;
}
