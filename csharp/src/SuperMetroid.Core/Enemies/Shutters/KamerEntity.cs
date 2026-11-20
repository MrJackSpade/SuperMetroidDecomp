namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>
/// The Kamer (#2179), a vertical platform that reuses the shootable shutter: it moves, reacts to shots and bombs as the
/// shootable shutter does, and shows its platform.
/// </summary>
/// <param name="species">The Shutters species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the Kamer occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">What the enemy's population entry tells its species.</param>
internal sealed class KamerEntity(ShuttersBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, ShutterSpawnSettings settings)
    : ShootableShutterEntity(species, occupant, place, common, ShutterSegment.KamerPlatform, settings)
{
}
