using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>
/// A yellow Pipe Bug's state, the Geega's (#1845): it waits hidden for Samus ahead of it, flies straight, arcs once after
/// passing her and flies on until it leaves the screen.
/// </summary>
public sealed class YellowPipeBugState
{
    /// <summary>Starts the state in <paramref name="action"/>.</summary>
    internal YellowPipeBugState(PipeBugAction action) => Action = action;

    /// <summary>The animation the bug plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<PipeBugSegment> Playback { get; set; }

    /// <summary>The current action, one of the yellow actions.</summary>
    public PipeBugAction Action { get; internal set; }

    /// <summary>The whole-pixel spawn position restored when the bug leaves the screen; its Y ends the arc.</summary>
    public Position16<RoomPixels> SpawnPosition { get; internal set; }

    /// <summary>The bug's flight speed in pixels per update, the spawn settings' speed: added flying right and negated flying left.</summary>
    public SignedSixteenSixteen FlightSpeed { get; internal set; }

    /// <summary>The countdown before straight flight begins.</summary>
    public int EmergenceDelay { get; internal set; }

    /// <summary>
    /// The whole-mark place along the speed curve the bug's arc reads, from the curve's start through the curve's last
    /// mark. The rising phase moves it back from mark 40 and ends when it would pass the curve's start; the returning
    /// phase moves it back on.
    /// </summary>
    public QuadraticSpeedCurvePlace ArcCounter { get; internal set; }

    /// <summary>Whether the arc is in its rising phase rather than its returning one.</summary>
    public bool ArcRising { get; internal set; }

    /// <summary>Whether the bug has completed its one authored arc.</summary>
    public bool ArcCompleted { get; internal set; }
}
