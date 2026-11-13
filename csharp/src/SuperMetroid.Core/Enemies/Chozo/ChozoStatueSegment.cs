namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>
/// The Lower Norfair and Wrecked Ship Chozo statues' idle and activation animations (#641): one segment per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="ChozoStatueAnimations"/>.
/// </summary>
internal enum ChozoStatueSegment
{
    /// <summary>The lower Norfair initial animation.</summary>
    LowerNorfairInitial,
    /// <summary>The lower Norfair activated animation.</summary>
    LowerNorfairActivated,
    /// <summary>The loop.</summary>
    LowerNorfairActivatedLoop,
    /// <summary>The wrecked ship initial animation.</summary>
    WreckedShipInitial,
    /// <summary>The wrecked ship activated animation.</summary>
    WreckedShipActivated,
    /// <summary>The loop.</summary>
    WreckedShipActivatedLoop,
    /// <summary>The loop.</summary>
    WreckedShipActivatedLoop2,
}
