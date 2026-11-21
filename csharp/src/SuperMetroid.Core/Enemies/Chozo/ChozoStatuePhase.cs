namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>
/// Where a Chozo statue stands in its wait for Samus at its hand (#2476). Its idle animation's first beat starts the wait,
/// Samus's arrival at the hand advances it, and the statue's next update starts its activation. Lower Norfair's hand admits
/// Samus once she has Space Jump, the Wrecked Ship's once Phantoon is defeated, so an admitted hand always wakes its statue.
/// </summary>
/// <remarks>
/// The five phases are the only ones (#2554), and each names its successor for the hand's trigger and for the start of the
/// wait: the hand's trigger is never undone, and the wait's start makes the statue wait for its hand whatever it did before.
/// </remarks>
public abstract class ChozoStatuePhase
{
    /// <summary>Only the five phases below derive from this.</summary>
    private ChozoStatuePhase() { }

    /// <summary>Placed, before its idle animation's first beat starts its wait.</summary>
    public static ChozoStatuePhase Placed { get; } = new PlacedPhase();

    /// <summary>Placed, with Samus already at its hand: its idle animation's first beat moves it on to <see cref="HandTriggered"/>.</summary>
    private static ChozoStatuePhase HandTriggeredBeforeWaiting { get; } = new TriggeredBeforeWaitingPhase();

    /// <summary>Waits for Samus at its hand, holding its cupped hands.</summary>
    private static ChozoStatuePhase AwaitingHand { get; } = new AwaitingHandPhase();

    /// <summary>Samus has triggered its hand: its next update starts its activation.</summary>
    public static ChozoStatuePhase HandTriggered { get; } = new HandTriggeredPhase();

    /// <summary>Has started its activation, which its animation plays to the end; the hand wakes it no more.</summary>
    public static ChozoStatuePhase Activated { get; } = new ActivatedPhase();

    /// <summary>
    /// The phase once Samus triggers the hand: a statue not yet woken records the trigger, waiting or not; a woken one
    /// keeps on with its wake.
    /// </summary>
    internal abstract ChozoStatuePhase AfterHandTriggered { get; }

    /// <summary>
    /// The phase once the idle animation's first beat starts the wait: the statue waits for its hand, and a hand already
    /// triggered starts its activation on the next update. The beat starts the wait whatever the statue did before, as the
    /// game's does.
    /// </summary>
    internal abstract ChozoStatuePhase AfterWaitStarts { get; }

    /// <summary>The placed statue.</summary>
    private sealed class PlacedPhase : ChozoStatuePhase
    {
        /// <inheritdoc/>
        internal override ChozoStatuePhase AfterHandTriggered => HandTriggeredBeforeWaiting;

        /// <inheritdoc/>
        internal override ChozoStatuePhase AfterWaitStarts => AwaitingHand;
    }

    /// <summary>The placed statue whose hand Samus has already triggered.</summary>
    private sealed class TriggeredBeforeWaitingPhase : ChozoStatuePhase
    {
        /// <inheritdoc/>
        internal override ChozoStatuePhase AfterHandTriggered => this;

        /// <inheritdoc/>
        internal override ChozoStatuePhase AfterWaitStarts => HandTriggered;
    }

    /// <summary>The statue waiting for Samus at its hand.</summary>
    private sealed class AwaitingHandPhase : ChozoStatuePhase
    {
        /// <inheritdoc/>
        internal override ChozoStatuePhase AfterHandTriggered => HandTriggered;

        /// <inheritdoc/>
        internal override ChozoStatuePhase AfterWaitStarts => this;
    }

    /// <summary>The waiting statue whose hand Samus has triggered.</summary>
    private sealed class HandTriggeredPhase : ChozoStatuePhase
    {
        /// <inheritdoc/>
        internal override ChozoStatuePhase AfterHandTriggered => this;

        /// <inheritdoc/>
        internal override ChozoStatuePhase AfterWaitStarts => this;
    }

    /// <summary>The activated statue.</summary>
    private sealed class ActivatedPhase : ChozoStatuePhase
    {
        /// <inheritdoc/>
        internal override ChozoStatuePhase AfterHandTriggered => this;

        /// <inheritdoc/>
        internal override ChozoStatuePhase AfterWaitStarts => HandTriggered;
    }
}
