namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>A trail's read of a placement list at an animation frame.</summary>
/// <param name="List">The placement list read.</param>
/// <param name="Frame">The animation frame it is read at.</param>
internal readonly record struct ProjectileTrailRead(ProjectileTrailPlacementList List, int Frame);
