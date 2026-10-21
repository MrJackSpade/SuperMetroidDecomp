using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>The first, second, third and fourth of one verify installed enemy projectile spritemaps case.</summary>
/// <param name="First">The first.</param>
/// <param name="Second">The second.</param>
/// <param name="Third">The third.</param>
/// <param name="Fourth">The fourth.</param>
internal readonly record struct VerifyInstalledEnemyProjectileSpritemapsCase(string First, RoomEnemyProjectileKind Second, int Third, Func<int, ushort> Fourth);
