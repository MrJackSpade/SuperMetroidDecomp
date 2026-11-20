namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>
/// A shootable vertical shutter (#2146), which the Kamer reuses (<see cref="KamerEntity"/>): its own shot reaction, and
/// its own reaction to a bomb that selects it.
/// </summary>
/// <param name="species">The Shutters species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the shutter occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="pose">The pose the shutter shows from its setup: the fully grown shutter, or a Kamer's platform.</param>
/// <param name="settings">What the enemy's population entry tells its species.</param>
internal class ShootableShutterEntity(ShuttersBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, ShutterSegment pose, ShutterSpawnSettings settings)
    : VerticalShutterEntity(species, occupant, place, common, pose, settings)
{
    /// <summary>The shutter's own shot reaction, built for this occupant.</summary>
    private readonly EnemyShotReaction ownShot = species.ShootableShutterShotReaction(occupant);

    /// <summary>The shootable shutter's own shot reaction, built over its state (#2554).</summary>
    private EnemyShotReaction OwnShotReaction => field ??= species.ShootableShutterShotReactionOf(State);

    /// <summary>The shootable shutter's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction? ShotReaction => LeavesShotToSpecies ? OwnShotReaction : base.ShotReaction;

    /// <summary>
    /// Runs the shootable shutter's own reaction to a bomb that selects its own shot reaction, before the ignoring check.
    /// </summary>
    /// <param name="context">The bomb contact.</param>
    public override bool TryReactToBomb(EnemyBombContext context)
    {
        if (context.SelectedShot != ShutterReactions.Shot)
            return base.TryReactToBomb(context);
        Species.ReactToShootableShutterBomb(Occupant, context, State);
        return true;
    }
}
