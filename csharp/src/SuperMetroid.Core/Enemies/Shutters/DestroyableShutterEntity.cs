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
/// <param name="state">What the enemy's setup built, which its base holds (#2554).</param>
internal sealed partial class DestroyableShutterEntity(ShuttersBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
    ShutterSpawnSettings settings, VerticalShutterEnemyState state)
    : VerticalShutterEntity(species, occupant, place, common, settings, state), IEnemyInitialization
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
    internal DestroyableShutterEntity(ShuttersBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
        ShutterSpawnSettings settings)
        : this(species, occupant, place, common, settings, ShuttersBehavior.InitializeVerticalShutter(settings, place, ShutterSegment.FullyGrown))
    {
    }

    /// <summary>The shutter's own shot reaction, built once for this shutter.</summary>
    private EnemyShotReaction OwnShot => field ??= ShuttersBehavior.DestroyableShutterShotReaction(this);

    /// <summary>The shutter's own stages around common bomb damage, built once for this shutter.</summary>
    private EnemyBombReaction OwnBomb => field ??= ShuttersBehavior.DestroyableShutterBombReaction(this);

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
