namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Where a trail's two sparkles start, as offsets from the projectile; both are drawn four pixels up and left of them.</summary>
/// <param name="Left">The left sparkle's offset.</param>
/// <param name="Right">The right sparkle's offset.</param>
internal readonly record struct ProjectileTrailPlacement(Offset16<RoomPixels> Left, Offset16<RoomPixels> Right);
