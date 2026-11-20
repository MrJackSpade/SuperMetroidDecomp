namespace SuperMetroid.Core.Enemies.Platforms;

/// <summary>
/// The Tripper's and the suspensor platform's vertically moving and still platform loops (#641): one segment per animation a spawn, branch or resumed rest enters, in authored order.
/// Their steps are <see cref="PlatformAnimations"/>.
/// </summary>
internal enum PlatformSegment
{
    /// <summary>The suspensor platform heading left, moving or still: it moves left and plays its loop (#1842).</summary>
    SuspensorPlatformHeadingLeft,
    /// <summary>The suspensor platform's loop, the same whichever way it heads and whether it moves or stays still (#1842).</summary>
    SuspensorPlatformLights,
    /// <summary>The suspensor platform heading right, moving or still: it moves right and plays its loop (#1842).</summary>
    SuspensorPlatformHeadingRight,
    /// <summary>The Tripper turning to face left as it moves, then hovering facing left.</summary>
    TripperMovingLeft,
    /// <summary>The Tripper hovering facing left, its jet flickering short then long, played over each loop (<see cref="PlatformAnimations.HoverFlickers"/>, #2276, #2439).</summary>
    TripperHoveringLeft,
    /// <summary>The Tripper turning to face right as it moves, then hovering facing right.</summary>
    TripperMovingRight,
    /// <summary>The Tripper hovering facing right, its jet flickering short then long, played over each loop (<see cref="PlatformAnimations.HoverFlickers"/>, #2276, #2439).</summary>
    TripperHoveringRight,
    /// <summary>The Tripper facing left as it keeps moving, then thrusting facing left.</summary>
    TripperStillMovingLeft,
    /// <summary>The Tripper thrusting left, its jet flickering, looped (#2276).</summary>
    TripperThrustingLeft,
    /// <summary>The Tripper facing right as it keeps moving, then thrusting facing right.</summary>
    TripperStillMovingRight,
    /// <summary>The Tripper thrusting right, its jet flickering, looped (#2276).</summary>
    TripperThrustingRight,
}
