namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>
/// The Magdollite head's idle, submerge and emerge animations (#641, #2554).
/// Their steps are <see cref="MagdolliteHeadAnimations"/>.
/// </summary>
internal enum MagdolliteHeadSegment
{
    /// <summary>The head facing left bobbing high, mid and low, repeating.</summary>
    LeftIdle,
    /// <summary>The head facing left sinking into the lava until its pillar shows.</summary>
    LeftSubmerge,
    /// <summary>Its mound facing left rising a pixel with its pillar.</summary>
    LeftRaisePillar,
    /// <summary>The head facing left free to act again once its pillar has risen.</summary>
    LeftPillarRaised,
    /// <summary>Facing left, its parts back in place as it starts to lower its pillar and emerge.</summary>
    LeftEmerge,
    /// <summary>Its mound facing left lowering a pixel with its pillar.</summary>
    LeftLowerPillar,
    /// <summary>The head facing left surfacing from the lava as its pillar hides, then free to act again.</summary>
    LeftSurfacing,
    /// <summary>The head facing right bobbing high, mid and low, repeating.</summary>
    RightIdle,
    /// <summary>The head facing right sinking into the lava until its pillar shows.</summary>
    RightSubmerge,
    /// <summary>Its mound facing right rising a pixel with its pillar.</summary>
    RightRaisePillar,
    /// <summary>The head facing right free to act again once its pillar has risen.</summary>
    RightPillarRaised,
    /// <summary>Facing right, its parts back in place as it starts to lower its pillar and emerge.</summary>
    RightEmerge,
    /// <summary>Its mound facing right lowering a pixel with its pillar.</summary>
    RightLowerPillar,
    /// <summary>The head facing right surfacing from the lava as its pillar hides, then free to act again.</summary>
    RightSurfacing,
}
