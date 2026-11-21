namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>
/// Where a Chozo statue stands in its wait for Samus at its hand (#2476). Its idle animation's first beat starts the wait,
/// Samus's arrival at the hand advances it, and the statue's next update starts its activation. Lower Norfair's hand admits
/// Samus once she has Space Jump, the Wrecked Ship's once Phantoon is defeated, so an admitted hand always wakes its statue.
/// </summary>
/// <remarks>
/// The five phases are the only ones (#2554): each is the pairing of whether the statue's update waits for its hand with
/// whether Samus has triggered the hand, which the game holds apart and the hand never clears, so every phase has its
/// successor for the hand's trigger and for the start of the wait.
/// </remarks>
public sealed class ChozoStatuePhase
{
    /// <summary>Whether the statue's update waits for its hand's trigger: from the idle animation's first beat until its activation starts.</summary>
    private readonly bool _waitsForHand;

    /// <summary>Whether Samus has triggered the statue's hand, which nothing undoes.</summary>
    private readonly bool _handTriggered;

    /// <summary>Builds the phase whose update waits for the hand or not, its hand triggered or not.</summary>
    private ChozoStatuePhase(bool waitsForHand, bool handTriggered)
    {
        _waitsForHand = waitsForHand;
        _handTriggered = handTriggered;
    }

    /// <summary>Placed, before its idle animation's first beat starts its wait.</summary>
    public static ChozoStatuePhase Placed { get; } = new(waitsForHand: false, handTriggered: false);

    /// <summary>Placed, with Samus already at its hand: its idle animation's first beat moves it on to <see cref="HandTriggered"/>.</summary>
    public static ChozoStatuePhase HandTriggeredBeforeWaiting { get; } = new(waitsForHand: false, handTriggered: true);

    /// <summary>Waits for Samus at its hand, holding its cupped hands.</summary>
    public static ChozoStatuePhase AwaitingHand { get; } = new(waitsForHand: true, handTriggered: false);

    /// <summary>Samus has triggered its hand: its next update starts its activation.</summary>
    public static ChozoStatuePhase HandTriggered { get; } = new(waitsForHand: true, handTriggered: true);

    /// <summary>Has started its activation, which its animation plays to the end; the hand wakes it no more.</summary>
    public static ChozoStatuePhase Activated { get; } = new(waitsForHand: false, handTriggered: true);

    /// <summary>
    /// The phase once Samus triggers the hand: a statue not yet woken records the trigger, waiting or not; a woken one
    /// keeps on with its wake.
    /// </summary>
    internal ChozoStatuePhase AfterHandTriggered =>
        _handTriggered ? this
        : _waitsForHand ? HandTriggered
        : HandTriggeredBeforeWaiting;

    /// <summary>
    /// The phase once the idle animation's first beat starts the wait: the statue's update waits for the hand, and a hand
    /// already triggered starts its activation on the next update. The beat installs the wait whatever the statue's
    /// update was, as the game's does.
    /// </summary>
    internal ChozoStatuePhase AfterWaitStarts => _handTriggered ? HandTriggered : AwaitingHand;
}
