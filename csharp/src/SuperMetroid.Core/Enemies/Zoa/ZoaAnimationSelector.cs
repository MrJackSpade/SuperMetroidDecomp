namespace SuperMetroid.Core.Enemies.Zoa;

/// <summary>A Zoa's animation: rising toward Samus's height or shooting across the room, facing left or right.</summary>
public enum ZoaAnimationSelector
{
    /// <summary>Shooting across the room facing left: the resting Zoa's animation.</summary>
    ShootingFacingLeft,
    /// <summary>Rising facing left, toward a Samus on its left.</summary>
    RisingFacingLeft,
    /// <summary>Shooting across the room facing right.</summary>
    ShootingFacingRight,
    /// <summary>Rising facing right, toward a Samus on its right.</summary>
    RisingFacingRight,
}
