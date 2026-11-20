using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>The state of a Chozo statue.</summary>
public sealed class ChozoStatueState
{
    /// <summary>What the enemy's population entry told its species, which it starts from and reads for its life (#2554).</summary>
    internal ChozoStatueSpawnSettings Settings { get; }

    /// <summary>Creates the state of an enemy whose entry told its species <paramref name="settings"/>.</summary>
    /// <param name="settings">What the enemy's population entry told its species.</param>
    internal ChozoStatueState(ChozoStatueSpawnSettings settings) => Settings = Ensure.NotNull(settings);

    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<ChozoStatueSegment> Playback { get; set; }

    /// <summary>Where the statue stands in its wait for Samus at its hand and its activation (#2476).</summary>
    public ChozoStatuePhase Phase { get; internal set; }

    /// <summary>Samus has triggered the statue's hand: a waiting statue's next update starts its activation.</summary>
    /// <exception cref="InvalidOperationException">The phase is undefined.</exception>
    internal void TriggerHand() => Phase = Phase switch
    {
        ChozoStatuePhase.Placed => ChozoStatuePhase.HandTriggeredBeforeWaiting,
        ChozoStatuePhase.AwaitingHand => ChozoStatuePhase.HandTriggered,
        // A statue the hand has already woken keeps on with its wake.
        ChozoStatuePhase.HandTriggeredBeforeWaiting or ChozoStatuePhase.HandTriggered or ChozoStatuePhase.Activated => Phase,
        _ => throw new InvalidOperationException($"Undefined {nameof(ChozoStatuePhase)} {Phase}."),
    };

    /// <summary>The statue's idle animation starts its wait for Samus at its hand, or its wake if she is already there.</summary>
    /// <exception cref="InvalidOperationException">The statue has already waited, or the phase is undefined.</exception>
    internal void AwaitHand() => Phase = Phase switch
    {
        ChozoStatuePhase.Placed => ChozoStatuePhase.AwaitingHand,
        ChozoStatuePhase.HandTriggeredBeforeWaiting => ChozoStatuePhase.HandTriggered,
        ChozoStatuePhase.AwaitingHand or ChozoStatuePhase.HandTriggered or ChozoStatuePhase.Activated =>
            throw new InvalidOperationException($"A Chozo statue {Phase} started its wait for Samus again."),
        _ => throw new InvalidOperationException($"Undefined {nameof(ChozoStatuePhase)} {Phase}."),
    };
}
