namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>
/// A vertical shutter, shootable (<see cref="ShootableShutterEntity"/>, or the Kamer that reuses it,
/// <see cref="KamerEntity"/>) or destroyable (<see cref="DestroyableShutterEntity"/>): each sets up, moves and animates as
/// vertical shutters do, showing its own pose, and runs the vertical shutters' own touch and Power Bomb reaction where its
/// definition leaves them to it.
/// </summary>
/// <param name="species">The Shutters species owner.</param>
/// <param name="place">The place the shutter occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="pose">The pose the shutter shows from its setup.</param>
/// <param name="settings">What the enemy's population entry tells its species.</param>
internal abstract class VerticalShutterEntity(ShuttersBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, ShutterSegment pose, ShutterSpawnSettings settings)
    : ShuttersEntity(species, occupant, place, common), IEnemyPrivateShotReaction
{
    /// <summary>The vertical shutter's own Power Bomb reaction, built over its state (#2554).</summary>
    private EnemyPowerBombReaction OwnPowerBombReaction => field ??= species.VerticalShutterPowerBombReactionOf(State);

    /// <summary>The shutter's state, which its setup builds as it is built (#2554).</summary>
    internal VerticalShutterEnemyState State { get; } = ShuttersBehavior.InitializeVerticalShutter(place, pose, settings);

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => Species.RunVerticalShutterMainAi(Place, State, frame);

    /// <inheritdoc/>
    public override void AdvanceTypedAnimation(EnemyAnimationCueContext context) => ShuttersBehavior.AdvanceVerticalShutterTypedAnimation(Place, State, context);

    /// <summary>Whether the vertical shutter's own touch, where its definition leaves Samus's touch to it, or a shared one applies.</summary>
    public override bool ReactsToTouch => Occupant.Definition.Touch == ShutterReactions.Touch || base.ReactsToTouch;

    /// <summary>Runs the vertical shutter's own touch where its definition leaves Samus's touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (Occupant.Definition.Touch == ShutterReactions.Touch)
            Species.ReactToVerticalShutterTouch(context, State);
        else
            base.ReactToTouch(context);
    }

    /// <summary>The vertical shutters' own Power Bomb reaction where the definition leaves the blast to them, or the shared one.</summary>
    public override EnemyPowerBombReaction PowerBombReaction =>
        LeavesPowerBombToSpecies ? OwnPowerBombReaction : base.PowerBombReaction;

    /// <summary>
    /// The shootable and destroyable vertical shutters' shot reactions, which a normal bomb triggers as it does the common one.
    /// </summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects; none where the definition leaves shots to the species.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction reaction) => reaction == ShutterReactions.Shot;
}
