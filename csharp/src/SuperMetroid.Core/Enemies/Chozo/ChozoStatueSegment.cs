namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>
/// The Lower Norfair and Wrecked Ship Chozo statues' idle and activation animations (#641): one segment per animation a spawn, branch or wake enters, in authored order.
/// Their steps are <see cref="ChozoStatueAnimations"/>.
/// </summary>
internal enum ChozoStatueSegment
{
    /// <summary>The lower Norfair initial animation.</summary>
    LowerNorfairInitial,
    /// <summary>The lower Norfair activated animation.</summary>
    LowerNorfairActivated,
    /// <summary>The Lower Norfair statue's finger flex, which its activation repeats.</summary>
    LowerNorfairFlexing,
    /// <summary>The Lower Norfair statue's release of Samus and the lowered acid.</summary>
    LowerNorfairRelease,
    /// <summary>The wrecked ship initial animation.</summary>
    WreckedShipInitial,
    /// <summary>The wrecked ship activated animation.</summary>
    WreckedShipActivated,
    /// <summary>The Wrecked Ship statue's finger flex, which its activation repeats.</summary>
    WreckedShipFlexing,
    /// <summary>The end of the Wrecked Ship statue's flexing: it stops awaiting the hand.</summary>
    WreckedShipFlexEnd,
    /// <summary>The Wrecked Ship statue's four strides with Samus in its hand (#2061), which its walk repeats.</summary>
    WreckedShipWalkingWithSamus,
    /// <summary>The Wrecked Ship statue's last strides, lowering Samus and letting her go.</summary>
    WreckedShipSettingSamusDown,
}
