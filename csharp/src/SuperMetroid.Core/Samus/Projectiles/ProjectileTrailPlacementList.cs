namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// A list of trail placements, read by the frame the projectile shows (#2565): the list a trail family keeps for the
/// course a shot travels, so the list is the pair of the family and the course rather than a member of their product.
/// </summary>
/// <param name="Family">The trail family whose list it is.</param>
/// <param name="Course">The travel the family keeps the list for.</param>
internal readonly record struct ProjectileTrailPlacementList(ProjectileTrailFamily Family, ProjectileTrailCourse Course);
