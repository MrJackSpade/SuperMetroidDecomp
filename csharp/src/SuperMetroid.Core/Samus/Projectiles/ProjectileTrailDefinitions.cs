namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The trail animations the trail spawner selects for projectiles other than ordinary beam shots.</summary>
public static class ProjectileTrailDefinitions
{
    /// <summary>The trail a missile or Super Missile leaves: the missile animation on the left, none on the right.</summary>
    internal static readonly ProjectileTrailAnimations MissileTrail =
        new(ProjectileTrailSegment.Missile, ProjectileTrailSegment.Empty);

    /// <summary>The trail the Spazer special attack's trailing pair leaves: the left ice animation on both sides.</summary>
    internal static readonly ProjectileTrailAnimations SpazerAttackTrail =
        new(ProjectileTrailSegment.LeftIce, ProjectileTrailSegment.LeftIce);
}
