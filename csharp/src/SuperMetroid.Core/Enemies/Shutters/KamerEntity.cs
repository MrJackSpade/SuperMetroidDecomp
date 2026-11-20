namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>
/// The Kamer (#2179), a vertical platform that reuses the shootable shutter: it moves, reacts to shots and bombs as the
/// shootable shutter does, and shows its platform.
/// </summary>
/// <param name="species">The Shutters species owner.</param>
/// <param name="place">The place the Kamer occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class KamerEntity(ShuttersBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : ShootableShutterEntity(species, place, common, ShutterSegment.KamerPlatform)
{
}
