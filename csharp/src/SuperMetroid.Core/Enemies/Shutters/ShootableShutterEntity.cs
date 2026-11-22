namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>
/// A shootable vertical shutter (#2146), which the Kamer reuses (<see cref="KamerEntity"/>): its own shot reaction, and
/// its own reaction to a bomb that selects it.
/// </summary>
/// <param name="species">The Shutters species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the shutter occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">What the shutter's population entry tells its species.</param>
/// <param name="state">What the enemy's setup built, which its base holds (#2554).</param>
internal partial class ShootableShutterEntity(ShuttersBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
    ShutterSpawnSettings settings, VerticalShutterEnemyState state)
    : VerticalShutterEntity(species, occupant, place, common, settings, state)
{
    /// <summary>
    /// Builds the enemy, running its setup as the game builds it (#2554). A restore of the game's state builds it through the
    /// primary constructor from what it holds, so the setup never runs again.
    /// </summary>
    /// <param name="species">The Shutters species owner.</param>
    /// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
    /// <param name="place">The place the shutter occupies.</param>
    /// <param name="common">The shared participation.</param>
    /// <param name="settings">What the shutter's population entry tells its species.</param>
    /// <param name="pose">The pose the shutter shows: the fully grown shutter, or the Kamer's platform.</param>
    internal ShootableShutterEntity(ShuttersBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
        ShutterSpawnSettings settings, ShutterSegment pose)
        : this(species, occupant, place, common, settings, ShuttersBehavior.InitializeVerticalShutter(settings, place, pose))
    {
    }

    /// <summary>The shutter's own shot reaction, built once for this shutter.</summary>
    private EnemyShotReaction OwnShot => field ??= ShuttersBehavior.ShootableShutterShotReaction(this);
    /// <summary>The shootable shutter's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction ShotReaction => Occupant.Definition.Shot == ShutterReactions.Shot ? OwnShot : base.ShotReaction;

    /// <summary>
    /// Runs the shootable shutter's own reaction to a bomb that selects its own shot reaction, before the ignoring check.
    /// </summary>
    /// <param name="context">The bomb contact.</param>
    public override bool TryReactToBomb(EnemyBombContext context)
    {
        if (context.SelectedShot != ShutterReactions.Shot)
            return base.TryReactToBomb(context);
        Species.ReactToShootableShutterBomb(Settings, context, State);
        return true;
    }
}
