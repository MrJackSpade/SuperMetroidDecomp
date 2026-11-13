namespace SuperMetroid.Core.Enemies.WreckedShip;

/// <summary>
/// The Wrecked Ship ghost's (Coven) floating loop as a typed program (#641), one per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="WreckedShipGhostAnimations"/>.
/// </summary>
internal enum WreckedShipGhostSegment
{
    /// <summary>The floating animation.</summary>
    Floating,
}
