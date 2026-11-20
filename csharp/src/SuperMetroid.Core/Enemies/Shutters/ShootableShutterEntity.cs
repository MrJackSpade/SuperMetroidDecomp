namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>
/// A shootable vertical shutter (#2146), which the Kamer reuses (<see cref="KamerEntity"/>): its own shot reaction, and
/// its own reaction to a bomb that selects it.
/// </summary>
/// <param name="species">The Shutters species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the shutter occupies.</param>
/// <param name="common">The shared participation.</param>
internal class ShootableShutterEntity(ShuttersBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : VerticalShutterEntity(species, occupant, place, common)
{
    /// <summary>The shutter's own shot reaction, built for this occupant.</summary>
    private readonly EnemyShotReaction ownShot = species.ShootableShutterShotReaction(occupant);

    /// <summary>A shootable shutter shows the fully grown shutter.</summary>
    protected override ShutterSegment Pose => ShutterSegment.FullyGrown;

    /// <summary>The shootable shutter's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction ShotReaction => Occupant.Definition.Shot == ShutterReactions.Shot ? ownShot : base.ShotReaction;

    /// <summary>
    /// Runs the shootable shutter's own reaction to a bomb that selects its own shot reaction, before the ignoring check.
    /// </summary>
    /// <param name="context">The bomb contact.</param>
    public override bool TryReactToBomb(EnemyBombContext context)
    {
        if (context.SelectedShot != ShutterReactions.Shot)
            return base.TryReactToBomb(context);
        Species.ReactToShootableShutterBomb(Occupant, context);
        return true;
    }
}
