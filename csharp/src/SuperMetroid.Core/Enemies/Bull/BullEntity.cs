namespace SuperMetroid.Core.Enemies.Bull;

/// <summary>
/// One Bull occupant: its initialization, main AI, typed animation and immune-hit recoil after shots and bombs, with the
/// shared reactions otherwise. Its state inherits the shot-reaction guard of the previous Bull in its place.
/// </summary>
/// <param name="species">The Bull species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the Bull occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">What the enemy's population entry tells its species.</param>
internal sealed class BullEntity(BullBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, BullSpawnSettings settings)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <summary>What the enemy's population entry tells its species, which its factory checked as it built the enemy.</summary>
    internal BullSpawnSettings Settings { get; } = settings;

    /// <summary>The enemy's state, which its setup builds as the enemy is built (#2554).</summary>
    internal BullEnemyState State { get; } = species.InitializeBull(settings, place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) =>
        BullBehavior.RunBullMain(Settings, Place, State, frame.Samus);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => BullBehavior.AdvanceBullAnimation(Place, State);

    /// <summary>The Bull's own recoil reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction ShotReaction => Occupant.Definition.Shot == BullReactions.Shot ? OwnShot : base.ShotReaction;

    /// <summary>The Bull's own shot reaction, built once for this Bull with its state (#2554).</summary>
    private EnemyShotReaction OwnShot => field ??= BullBehavior.ShotReaction(State);

    /// <summary>The Bull's own bomb stages, built once for this Bull with its state (#2554).</summary>
    private EnemyBombReaction OwnBomb => field ??= BullBehavior.BombReaction(State);

    /// <summary>The Bull's own bomb recoil around common bomb damage, for a bomb that selects its own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction CommonBombDamageReaction(EnemyBombContext context) =>
        context.SelectedShot == BullReactions.Shot ? OwnBomb : base.CommonBombDamageReaction(context);

    /// <summary>The Bull's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects: the species' own where its definition names it.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction reaction) => reaction == BullReactions.Shot;
}
