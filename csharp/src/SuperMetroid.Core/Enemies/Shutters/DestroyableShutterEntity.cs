using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>
/// A destroyable vertical shutter (#2146): its own shot reaction, and its own stages around common bomb damage for a bomb
/// that selects it.
/// </summary>
/// <param name="species">The Shutters species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the shutter occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">What the shutter's population entry tells its species.</param>
internal sealed class DestroyableShutterEntity(ShuttersBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
    ShutterSpawnSettings settings)
    : VerticalShutterEntity(species, occupant, place, common, settings, ShutterSegment.FullyGrown), IEnemyInitialization
{
    /// <summary>The shutter's own shot reaction, built once for this shutter.</summary>
    private EnemyShotReaction OwnShot => field ??= Species.DestroyableShutterShotReaction(Settings, State);

    /// <summary>The shutter's own stages around common bomb damage, built once for this shutter.</summary>
    private EnemyBombReaction OwnBomb => field ??= Species.DestroyableShutterBombReaction(Settings, State);

    /// <summary>Keeps the shutter, set up as vertical shutters are as it was built, among the shutters the escape's flash shows on (#2380).</summary>
    public void Initialize() => Species.KeepDestroyableShutter(this);

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
    public override EnemyShotReaction ShotReaction => Occupant.Definition.Shot == ShutterReactions.Shot ? OwnShot : base.ShotReaction;

    /// <summary>The destroyable shutter's own stages around common bomb damage, for a bomb that selects its own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction CommonBombDamageReaction(EnemyBombContext context) =>
        context.SelectedShot == ShutterReactions.Shot ? OwnBomb : base.CommonBombDamageReaction(context);
}
