namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// One picture a projectile shows on an update (#2639): its frame, drawn from a place in the room. A projectile shows its
/// frame at its own place, none while its flicker hides it, or, for a n00b-tube shard, the shard and its mirror image.
/// </summary>
/// <param name="At">The room place the picture is drawn from, before the frame's own placement.</param>
/// <param name="Frame">The frame shown.</param>
internal readonly partial record struct EnemyProjectileShownPicture(Position16<RoomPixels> At, EnemyProjectileFrameId Frame);
