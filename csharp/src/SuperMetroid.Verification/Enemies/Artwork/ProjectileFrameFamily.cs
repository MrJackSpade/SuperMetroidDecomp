using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>A projectile family the installed-artwork checks draw: its name, the kind it is drawn as and the frames it shows.</summary>
/// <param name="Name">The family's name, for failure messages.</param>
/// <param name="Kind">The kind its frames are drawn as.</param>
/// <param name="Frames">The frames it shows.</param>
internal readonly record struct ProjectileFrameFamily(string Name, RoomEnemyProjectileKind Kind, IReadOnlyList<EnemyProjectileFrameId> Frames);
