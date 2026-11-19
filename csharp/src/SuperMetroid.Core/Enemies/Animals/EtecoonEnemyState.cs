using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// The state of an Etecoon. The route it takes after its first landing is its spawn settings', which it reads where it
/// takes it (#2531).
/// </summary>
public sealed class EtecoonEnemyState
{
    /// <summary>Starts the state in <paramref name="action"/>.</summary>
    internal EtecoonEnemyState(EtecoonAction action) => Action = action;

    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<EtecoonSegment> Playback { get; set; }

    /// <summary>
    /// The Etecoon's velocity in room pixels per update, positive right and down.
    /// </summary>
    public FixedVector<RoomPixels> Velocity { get; internal set; }

    /// <summary>
    /// The current behaviour's wait, at most <see cref="EtecoonTimingDefinitions.FlexDuration"/>: every behaviour that
    /// waits starts its wait on entry and moves on in the update its wait ends, so the wait is never counted once over.
    /// </summary>
    public UpdateCountdown ActionTimer { get; internal set; } = UpdateCountdown.Ended();

    /// <summary>The main-AI behaviour.</summary>
    public EtecoonAction Action { get; internal set; }

    /// <summary>
    /// The updates an earthquake still suspends the Etecoon for: an earthquake starts
    /// <see cref="EtecoonQuakeDefinitions.QuakePauseFrames"/>, and an Etecoon is placed with none.
    /// </summary>
    public UpdateCountdown QuakePauseFrames { get; internal set; } = UpdateCountdown.Ended();

    /// <summary>
    /// The way the Etecoon is heading (#2277): it is placed heading left, until it first takes a heading of its own.
    /// </summary>
    /// <remarks>
    /// The teaching hops never change which way the Etecoon heads: every state that tests the heading is reached only
    /// after the Etecoon has taken a heading (<see cref="Head"/>), which empties <see cref="TeachingHops"/>, and the
    /// teaching hops that count again always end in the return run, whose wall takes a heading before the next test.
    /// </remarks>
    public HorizontalDirection Heading { get; internal set; }

    /// <summary>
    /// Teaching hops taken since the count was last emptied, none when the Etecoon is placed: the count empties on reaching
    /// <see cref="EtecoonRouteDefinitions.TeachingHopsPerLesson"/>.
    /// </summary>
    public int TeachingHops
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, EtecoonRouteDefinitions.TeachingHopsPerLesson, value);
    }

    /// <summary>Takes a heading, which also empties the teaching-hop count.</summary>
    internal void Head(HorizontalDirection side)
    {
        Heading = side;
        TeachingHops = 0;
    }
}
