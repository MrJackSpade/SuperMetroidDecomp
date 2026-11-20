namespace SuperMetroid.Core.Enemies.Bull;

/// <summary>
/// One Bull occupant: its initialization, main AI, typed animation and immune-hit recoil after shots and bombs, with the
/// shared reactions otherwise. Its state inherits the shot-reaction guard of the previous Bull in its place.
/// </summary>
/// <param name="species">The Bull species owner.</param>
/// <param name="place">The place the Bull occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">What the enemy's population entry tells its species.</param>
internal sealed class BullEntity(BullBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common, BullSpawnSettings settings)
    : CommonParticipationEnemyEntity(place, common), IEnemyAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <summary>The enemy's own bomb reaction, built over its state (#2554).</summary>
    private EnemyBombReaction OwnBullBombReaction => field ??= BullBehavior.BullBombReactionOf(State);

    /// <summary>The enemy's own shot reaction, built over its state (#2554).</summary>
    private EnemyShotReaction OwnBullShotReaction => field ??= BullBehavior.BullShotReactionOf(State);

    /// <summary>The enemy's state, which its initialization builds as the enemy is built (#2554).</summary>
    internal BullEnemyState State { get; } = species.InitializeBull(place, settings);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) =>
        BullBehavior.RunBullMain(Place, State, frame.Samus);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => BullBehavior.AdvanceBullAnimation(Place, State, context);

    /// <summary>The Bull's own recoil reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction? ShotReaction => LeavesShotToSpecies ? OwnBullShotReaction : base.ShotReaction;

    /// <summary>The Bull's own bomb recoil around common bomb damage, for a bomb that selects its own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction? CommonBombDamageReaction(EnemyBombContext context) =>
        context.SelectedShot is null ? OwnBullBombReaction : base.CommonBombDamageReaction(context);

    /// <summary>The Bull's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects; none where the definition leaves shots to the species.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) => reaction is null && LeavesShotToSpecies;
}
