using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>An installed program frame's picture a bank-$86 operand shows moved off the projectile's place (#2557).</summary>
/// <param name="Frame">The installed frame whose picture the operand shows.</param>
/// <param name="Placement">How far from the projectile's place the operand draws the picture.</param>
internal sealed partial record EnemyProjectilePlacedFrame(EnemyProjectileFrameId Frame, Offset16<RoomPixels> Placement);
