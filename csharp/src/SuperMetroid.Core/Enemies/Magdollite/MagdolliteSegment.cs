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
    /// <summary>The pillar phase 0 animation.</summary>
    PillarPhase0,
    /// <summary>The pillar phase 1 animation.</summary>
    PillarPhase1,
    /// <summary>The pillar phase 2 animation.</summary>
    PillarPhase2,
    /// <summary>The pillar phase 3 animation.</summary>
    PillarPhase3,
    /// <summary>The pillar phase 4 animation.</summary>
    PillarPhase4,
    /// <summary>The pillar phase 5 animation.</summary>
    PillarPhase5,
    /// <summary>The pillar phase 6 animation.</summary>
    PillarPhase6,
    /// <summary>The pillar phase 7 animation.</summary>
    PillarPhase7,
    /// <summary>The pillar cap animation.</summary>
    PillarCap,
}
