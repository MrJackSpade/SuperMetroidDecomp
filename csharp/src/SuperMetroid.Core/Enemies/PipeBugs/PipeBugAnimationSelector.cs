namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>
/// A Brinstar Pipe Bug's animation: rising out of its pipe or shooting across the room, facing left or right. The
/// bug's strength then picks the Zeb or Zebbo art for it.
/// </summary>
public enum PipeBugAnimationSelector
{
    /// <summary>Rising facing left: the waiting and respawned bug's animation.</summary>
    RisingFacingLeft,
    /// <summary>Shooting across the room facing left.</summary>
    ShootingFacingLeft,
    /// <summary>Rising facing right, toward a Samus on its right.</summary>
    RisingFacingRight,
    /// <summary>Shooting across the room facing right.</summary>
    ShootingFacingRight,
}
