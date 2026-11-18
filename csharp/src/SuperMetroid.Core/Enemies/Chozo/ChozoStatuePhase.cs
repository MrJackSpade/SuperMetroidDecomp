namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>
/// Where a Chozo statue stands in its wait for Samus at its hand (#2476). Its idle animation's first beat starts the wait,
/// Samus's arrival at the hand advances it, and the statue's next update starts its activation. Lower Norfair's hand admits
/// Samus once she has Space Jump, the Wrecked Ship's once Phantoon is defeated, so an admitted hand always wakes its statue.
/// </summary>
public enum ChozoStatuePhase
{
    /// <summary>Placed, before its idle animation's first beat starts its wait.</summary>
    Placed,

    /// <summary>Placed, with Samus already at its hand: its idle animation's first beat moves it on to <see cref="HandTriggered"/>.</summary>
    HandTriggeredBeforeWaiting,

    /// <summary>Waits for Samus at its hand, holding its cupped hands.</summary>
    AwaitingHand,

    /// <summary>Samus has triggered its hand: its next update starts its activation.</summary>
    HandTriggered,

    /// <summary>Has started its activation, which its animation plays to the end; the hand wakes it no more.</summary>
    Activated,
}
