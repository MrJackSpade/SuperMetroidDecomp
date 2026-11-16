namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>
/// A destroyable vertical shutter (#2146): its own shot reaction, and its own stages around common bomb damage for a bomb
/// that selects it.
/// </summary>
/// <param name="species">The Shutters species owner.</param>
/// <param name="place">The place the shutter occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class DestroyableShutterEntity(ShuttersBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : VerticalShutterEntity(species, place, common)
{
    /// <summary>The destroyable shutter's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction? ShotReaction => LeavesShotToSpecies ? Species.DestroyableShutterShotReaction : base.ShotReaction;

    /// <summary>The destroyable shutter's own stages around common bomb damage, for a bomb that selects its own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction? CommonBombDamageReaction(EnemyBombContext context) =>
        context.SelectedShot is null ? Species.DestroyableShutterBombReaction : base.CommonBombDamageReaction(context);
}
