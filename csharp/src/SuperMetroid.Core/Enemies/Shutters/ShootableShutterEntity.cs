namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>
/// A shootable vertical shutter (#2146), which the Kamer reuses (<see cref="KamerEntity"/>): its own shot reaction, and
/// its own reaction to a bomb that selects it.
/// </summary>
/// <param name="species">The Shutters species owner.</param>
/// <param name="place">The place the shutter occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="pose">The pose the shutter shows from its setup: the fully grown shutter, or a Kamer's platform.</param>
internal class ShootableShutterEntity(ShuttersBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common, ShutterSegment pose)
    : VerticalShutterEntity(species, place, common, pose)
{
    /// <summary>The shootable shutter's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction? ShotReaction => LeavesShotToSpecies ? Species.ShootableShutterShotReaction : base.ShotReaction;

    /// <summary>
    /// Runs the shootable shutter's own reaction to a bomb that selects its own shot reaction, before the ignoring check.
    /// </summary>
    /// <param name="context">The bomb contact.</param>
    public override bool TryReactToBomb(EnemyBombContext context)
    {
        if (context.SelectedShot is not null)
            return base.TryReactToBomb(context);
        Species.ReactToShootableShutterBomb(context);
        return true;
    }
}
