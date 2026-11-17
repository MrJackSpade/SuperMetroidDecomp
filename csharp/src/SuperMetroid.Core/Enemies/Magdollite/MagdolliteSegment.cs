namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>
/// The Magdollite head's idle, submerge and emerge animations, its body's pillar stacks and its overlay's throw and cap animations (#641): one segment per animation a spawn, branch or wake enters, in authored order.
/// Their steps are <see cref="MagdolliteAnimations"/>.
/// </summary>
internal enum MagdolliteSegment
{
    /// <summary>The head facing left bobbing high, mid and low, repeating.</summary>
    LeftIdle,
    /// <summary>
    /// Facing left, rearing with the throw sound, leaning and reaching to fling three gobs of lava, then throwing.
    /// </summary>
    LeftThrow,
    /// <summary>The head facing left sinking into the lava until its pillar shows.</summary>
    LeftSubmerge,
    /// <summary>Its mound facing left rising a pixel with its pillar.</summary>
    LeftRaisePillar,
    /// <summary>Facing left, its parts back in place as it starts to lower its pillar and emerge.</summary>
    LeftEmerge,
    /// <summary>
    /// Its mound facing left lowering with its pillar, then its head surfacing from the lava as its pillar hides.
    /// </summary>
    LeftLowerPillar,
    /// <summary>The head facing right bobbing high, mid and low, repeating.</summary>
    RightIdle,
    /// <summary>
    /// Facing right, rearing with the throw sound, leaning and reaching to fling three gobs of lava, then throwing.
    /// </summary>
    RightThrow,
    /// <summary>The head facing right sinking into the lava until its pillar shows.</summary>
    RightSubmerge,
    /// <summary>Its mound facing right rising a pixel with its pillar.</summary>
    RightRaisePillar,
    /// <summary>Facing right, its parts back in place as it starts to lower its pillar and emerge.</summary>
    RightEmerge,
    /// <summary>
    /// Its mound facing right lowering with its pillar, then its head surfacing from the lava as its pillar hides.
    /// </summary>
    RightLowerPillar,
    /// <summary>The pillar one segment high.</summary>
    OneSegmentPillar,
    /// <summary>The pillar two segments high.</summary>
    TwoSegmentPillar,
    /// <summary>The pillar three segments high.</summary>
    ThreeSegmentPillar,
    /// <summary>The pillar four segments high.</summary>
    FourSegmentPillar,
    /// <summary>The pillar five segments high.</summary>
    FiveSegmentPillar,
    /// <summary>The pillar six segments high.</summary>
    SixSegmentPillar,
    /// <summary>The pillar seven segments high.</summary>
    SevenSegmentPillar,
    /// <summary>The pillar eight segments high.</summary>
    EightSegmentPillar,
    /// <summary>The cap on top of its pillar, held.</summary>
    PillarCap,
}
