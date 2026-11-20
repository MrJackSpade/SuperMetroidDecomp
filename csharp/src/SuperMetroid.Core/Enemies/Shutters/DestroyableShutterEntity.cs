using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>
/// A destroyable vertical shutter (#2146): its own shot reaction, and its own stages around common bomb damage for a bomb
/// that selects it.
/// </summary>
/// <param name="species">The Shutters species owner.</param>
/// <param name="place">The place the shutter occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">What the enemy's population entry tells its species.</param>
internal sealed class DestroyableShutterEntity(ShuttersBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common, ShutterSpawnSettings settings)
    : VerticalShutterEntity(species, place, common, ShutterSegment.FullyGrown, settings)
{
    /// <summary>
    /// Shows <paramref name="flash"/>, a frame of the escape's red shutter flash, while the shutter holds the place it was
    /// set up in (#2380): a destroyed shutter, and any enemy that took its place since, shows none.
    /// </summary>
    internal void ShowEscapeShutterFlash(PictureShade flash)
    {
        if (ReferenceEquals(Place.Entity, this))
            ShuttersBehavior.LayEscapeShutterFlash(Place, flash);
    }

    /// <summary>The destroyable shutter's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction? ShotReaction => LeavesShotToSpecies ? OwnShotReaction : base.ShotReaction;

    /// <summary>The destroyable shutter's own shot reaction, over its own state.</summary>
    private EnemyShotReaction OwnShotReaction => field ??= Species.DestroyableShutterShotReactionOf(State);

    /// <summary>The destroyable shutter's own stages around common bomb damage, over its own state.</summary>
    private EnemyBombReaction OwnBombReaction => field ??= Species.DestroyableShutterBombReactionOf(State);

    /// <summary>The destroyable shutter's own stages around common bomb damage, for a bomb that selects its own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction? CommonBombDamageReaction(EnemyBombContext context) =>
        context.SelectedShot is null ? OwnBombReaction : base.CommonBombDamageReaction(context);
}
