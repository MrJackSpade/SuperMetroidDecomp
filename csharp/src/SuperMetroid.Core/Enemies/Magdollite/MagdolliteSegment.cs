namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>
/// The Magdollite head's idle, submerge and emerge animations, its body's pillar stacks and its overlay's throw and cap animations (#641): one segment per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="MagdolliteAnimations"/>.
/// </summary>
internal enum MagdolliteSegment
{
    /// <summary>The left idle animation.</summary>
    LeftIdle,
    /// <summary>The left throw animation.</summary>
    LeftThrow,
    /// <summary>The left submerge animation.</summary>
    LeftSubmerge,
    /// <summary>The left raise pillar animation.</summary>
    LeftRaisePillar,
    /// <summary>The left emerge animation.</summary>
    LeftEmerge,
    /// <summary>The left lower pillar animation.</summary>
    LeftLowerPillar,
    /// <summary>The right idle animation.</summary>
    RightIdle,
    /// <summary>The right throw animation.</summary>
    RightThrow,
    /// <summary>The right submerge animation.</summary>
    RightSubmerge,
    /// <summary>The right raise pillar animation.</summary>
    RightRaisePillar,
    /// <summary>The right emerge animation.</summary>
    RightEmerge,
    /// <summary>The right lower pillar animation.</summary>
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
    /// <summary>The pillar cap animation.</summary>
    PillarCap,
}
