namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>One timed record a Samus projectile program shows (#1301, #1517); see <see cref="SamusProjectileRecordDefinitions"/>.</summary>
/// <param name="Animation">The program that shows the record.</param>
/// <param name="Show">The record's place among the records the program shows, from zero.</param>
internal readonly record struct SamusProjectileFrame(SamusProjectileSegment Animation, int Show);
