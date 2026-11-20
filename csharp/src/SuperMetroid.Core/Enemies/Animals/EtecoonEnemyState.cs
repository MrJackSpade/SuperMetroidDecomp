using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// The state of an Etecoon, which keeps the route its placement names (#2531) for when it takes it after its first landing.
/// </summary>
public sealed class EtecoonEnemyState
{
    /// <summary>What the enemy's population entry told its species, which it starts from and reads for its life (#2554).</summary>
    internal EtecoonSpawnSettings Settings { get; }

    /// <summary>Starts the state in <paramref name="action"/>.</summary>
    internal EtecoonEnemyState(EtecoonAction action, EtecoonSpawnSettings settings)
    {
        Settings = Ensure.NotNull(settings);
        Action = action;
        Route = route;
        Heading = HorizontalDirection.Left;
    }

    /// <summary>The route the Etecoon takes after its first landing, as its placement names it.</summary>
    internal EtecoonRoute Route { get; }

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
