using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.OldTourianWall;

/// <summary>One beat of the old Tourian escape-shaft wall's explosion burst (#2044): the frame it shows and for how long.</summary>
/// <param name="Frame">The small-explosion frame shown.</param>
/// <param name="Duration">How long it shows, in updates.</param>
internal readonly partial record struct OldTourianWallExplosionBeat(EnemyProjectileFrameId Frame, int Duration);
