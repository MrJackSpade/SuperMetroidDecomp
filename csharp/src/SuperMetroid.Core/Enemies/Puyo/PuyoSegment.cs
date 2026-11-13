namespace SuperMetroid.Core.Enemies.Puyo;

/// <summary>
/// Puyo's grounded loops and airborne hop poses as typed animations (#641): one segment per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="PuyoAnimations"/>.
/// </summary>
internal enum PuyoSegment
{
    /// <summary>The grounded fast animation.</summary>
    GroundedFast,
    /// <summary>The grounded medium animation.</summary>
    GroundedMedium,
    /// <summary>The grounded slow animation.</summary>
    GroundedSlow,
    /// <summary>The right frame 0 left frame 4 animation.</summary>
    RightFrame0LeftFrame4,
    /// <summary>The right frame 1 left frame 3 animation.</summary>
    RightFrame1LeftFrame3,
    /// <summary>The frame 2 animation.</summary>
    Frame2,
    /// <summary>The right frame 3 left frame 1 animation.</summary>
    RightFrame3LeftFrame1,
    /// <summary>The right frame 4 left frame 0 animation.</summary>
    RightFrame4LeftFrame0,
}
