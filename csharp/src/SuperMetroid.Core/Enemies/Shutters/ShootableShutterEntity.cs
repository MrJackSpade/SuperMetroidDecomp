namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>
/// A shootable vertical shutter, or the Kamer that reuses it (#2146): its own shot reaction, and its own reaction to a
/// bomb that selects it.
/// </summary>
/// <param name="species">The Shutters species owner.</param>
/// <param name="place">The place the shutter occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class ShootableShutterEntity(ShuttersBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : VerticalShutterEntity(species, place, common)
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
