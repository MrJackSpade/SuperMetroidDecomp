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
internal abstract partial class VerticalShutterEntity(ShuttersBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
    ShutterSpawnSettings settings, ShutterSegment pose)
    : ShuttersEntity(species, occupant, place, common)
{
    /// <summary>The <c>pose</c> the object was made with, which it keeps so it can be made again from what it holds (#2602).</summary>
    private readonly ShutterSegment pose = pose;

    /// <summary>What the shutter's population entry tells its species, which its factory checked as it built the enemy.</summary>
    internal ShutterSpawnSettings Settings { get; } = settings;

    /// <summary>The shutter's state, which its setup builds as the enemy is built (#2554).</summary>
    internal VerticalShutterEnemyState State { get; } = ShuttersBehavior.InitializeVerticalShutter(settings, place, pose);

    /// <summary>The vertical shutters' own Power Bomb reaction, built once for this shutter.</summary>
    private EnemyPowerBombReaction OwnPowerBomb => field ??= ShuttersBehavior.VerticalShutterPowerBombReaction(this);

    /// <summary>Sets the shutter off on a Power Bomb, in place of its common damage.</summary>
    /// <param name="context">The Power Bomb's blast meeting the shutter.</param>
    internal void ReactToPowerBomb(EnemyPowerBombContext context) => Species.ReactVerticalShutter(Settings, context.Enemy, State, context.Camera);

    /// <summary>
    /// Marks a shot's collision with the shootable shutter and sets it off, after the Super Missile quake: a shot deals the
    /// shutter no damage.
    /// </summary>
    /// <param name="c">The shot meeting the shutter.</param>
    /// <returns>Always true: setting the shutter off replaces the rest of the shot reaction.</returns>
    internal bool ReactToShootableShot(in EnemyShotContext c)
    {
        // A shot deals the shutter no damage and starts it moving; the shot's collision is still marked, a Super
        // Missile's quake included.
        c.Projectiles.EnemyImpacts.TouchEnemyWithoutImpact(c.Shot, c.MarksCollision);
        Species.ReactVerticalShutter(Settings, c.Enemy, State, c.Camera);
        return true;
    }

    /// <summary>Sets the destroyable shutter off after a bomb's common damage.</summary>
    /// <param name="context">The bomb's hit.</param>
    internal void ReactDestroyableAfterCommon(EnemyBombHitContext context) =>
        Species.ReactDestroyableShutterAfterCommon(Settings, Ensure.NotNull(context).Enemy, State, context.Camera);

    /// <summary>Sets the destroyable shutter off after a shot's common damage.</summary>
    /// <param name="context">The shot's hit.</param>
    internal void ReactDestroyableAfterCommon(EnemyShotHitContext context) =>
        Species.ReactDestroyableShutterAfterCommon(Settings, Ensure.NotNull(context).Enemy, State, context.Camera);

    /// <summary>Whether the vertical shutters' own touch, where its definition leaves Samus's touch to it, or a shared one applies.</summary>
    public override bool ReactsToTouch => Occupant.Definition.Touch == ShutterReactions.Touch || base.ReactsToTouch;

    /// <summary>Runs the vertical shutters' own touch where its definition leaves Samus's touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (Occupant.Definition.Touch == ShutterReactions.Touch)
            Species.ReactToVerticalShutterTouch(Settings, context, State);
        else
            base.ReactToTouch(context);
    }

    /// <summary>The vertical shutters' own Power Bomb reaction where the definition leaves the blast to them, or the shared one.</summary>
    public override EnemyPowerBombReaction PowerBombReaction =>
        LeavesPowerBombToSpecies ? OwnPowerBomb : base.PowerBombReaction;

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => Species.RunVerticalShutterMainAi(Settings, Place, State, frame);

    /// <inheritdoc/>
    public override void AdvanceTypedAnimation(EnemyAnimationCueContext context) => ShuttersBehavior.AdvanceVerticalShutterTypedAnimation(Place, State);
}
