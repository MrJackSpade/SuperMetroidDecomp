namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>
/// The Kamer (#2179), a vertical platform that reuses the shootable shutter: it moves, reacts to shots and bombs as the
/// shootable shutter does, and shows its platform.
/// </summary>
/// <param name="species">The Shutters species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the Kamer occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">What the Kamer's population entry tells its species.</param>
/// <param name="state">What the enemy's setup built, which its base holds (#2554).</param>
internal sealed partial class KamerEntity(ShuttersBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
    ShutterSpawnSettings settings, VerticalShutterEnemyState state)
    : ShootableShutterEntity(species, occupant, place, common, settings, state)
{
    /// <summary>
    /// Builds the enemy, running its setup as the game builds it (#2554). A restore of the game's state builds it through the
    /// primary constructor from what it holds, so the setup never runs again.
    /// </summary>
    /// <param name="species">The Shutters species owner.</param>
    /// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
    /// <param name="place">The place the Kamer occupies.</param>
    /// <param name="common">The shared participation.</param>
    /// <param name="settings">What the Kamer's population entry tells its species.</param>
    internal KamerEntity(ShuttersBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
        ShutterSpawnSettings settings)
        : this(species, occupant, place, common, settings, ShuttersBehavior.InitializeVerticalShutter(settings, place, ShutterSegment.KamerPlatform))
    {
    }
}
