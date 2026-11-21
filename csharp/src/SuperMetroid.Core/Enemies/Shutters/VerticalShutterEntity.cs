namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>
/// A vertical shutter, shootable (<see cref="ShootableShutterEntity"/>, or the Kamer that reuses it,
/// <see cref="KamerEntity"/>) or destroyable (<see cref="DestroyableShutterEntity"/>): each sets up, moves and animates as
/// vertical shutters do, showing its own pose.
/// </summary>
/// <param name="species">The Shutters species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the shutter occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">What the shutter's population entry tells its species.</param>
/// <param name="pose">The pose the shutter shows.</param>
internal abstract class VerticalShutterEntity(ShuttersBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
    ShutterSpawnSettings settings, ShutterSegment pose)
    : ShuttersEntity(species, occupant, place, common)
{
    /// <summary>What the shutter's population entry tells its species, which its factory checked as it built the enemy.</summary>
    internal ShutterSpawnSettings Settings { get; } = settings;

    /// <summary>The shutter's state, which its setup builds as the enemy is built (#2554).</summary>
    internal VerticalShutterEnemyState State { get; } = ShuttersBehavior.InitializeVerticalShutter(occupant, settings, place, pose);

    /// <summary>The vertical shutters' own Power Bomb reaction, built once for this shutter.</summary>
    private EnemyPowerBombReaction OwnPowerBomb => field ??= Species.VerticalShutterPowerBombReaction(Occupant, Settings, State);

    /// <summary>Whether the vertical shutters' own touch, where its definition leaves Samus's touch to it, or a shared one applies.</summary>
    public override bool ReactsToTouch => Occupant.Definition.Touch == ShutterReactions.Touch || base.ReactsToTouch;

    /// <summary>Runs the vertical shutters' own touch where its definition leaves Samus's touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (Occupant.Definition.Touch == ShutterReactions.Touch)
            Species.ReactToVerticalShutterTouch(Occupant, Settings, context, State);
        else
            base.ReactToTouch(context);
    }

    /// <summary>The vertical shutters' own Power Bomb reaction where the definition leaves the blast to them, or the shared one.</summary>
    public override EnemyPowerBombReaction PowerBombReaction =>
        LeavesPowerBombToSpecies ? OwnPowerBomb : base.PowerBombReaction;

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => Species.RunVerticalShutterMainAi(Occupant, Settings, Place, State, frame);

    /// <inheritdoc/>
    public override void AdvanceTypedAnimation(EnemyAnimationCueContext context) => ShuttersBehavior.AdvanceVerticalShutterTypedAnimation(Place, State);
}
