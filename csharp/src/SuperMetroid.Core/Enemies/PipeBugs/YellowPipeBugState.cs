using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>
/// A yellow Pipe Bug's state, the Geega's (#1845): it waits hidden for Samus ahead of it, flies straight, arcs once after
/// passing her and flies on until it leaves the screen.
/// </summary>
public sealed class YellowPipeBugState
{
    /// <summary>What the enemy's population entry told its species, which it starts from and reads for its life (#2554).</summary>
    internal YellowPipeBugSpawnSettings Settings { get; }

    /// <summary>Starts the state in <paramref name="action"/>.</summary>
    internal YellowPipeBugState(YellowPipeBugAction action, YellowPipeBugSpawnSettings settings)
    {
        Settings = Ensure.NotNull(settings);
        Action = action;
    }

    /// <summary>The animation the bug plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<PipeBugSegment> Playback { get; set; }

    /// <summary>The current action, one of the yellow actions.</summary>
    public YellowPipeBugAction Action { get; internal set; }

    /// <summary>The whole-pixel spawn position restored when the bug leaves the screen; its Y ends the arc.</summary>
    public Position16<RoomPixels> SpawnPosition { get; internal set; }

    /// <summary>The countdown before straight flight begins.</summary>
    public UpdateCountdown EmergenceDelay { get; internal set; } = UpdateCountdown.Ended();

    /// <summary>
    /// The whole-mark place along the speed curve the bug's arc reads (#2473), from the curve's start through the curve's
    /// last mark. The dip moves it back from <see cref="PipeBugDefinitions.YellowArcStart"/> and ends when it would pass the curve's start; the climb back moves
    /// it on.
    /// </summary>
    public QuadraticSpeedCurvePlace ArcCurvePlace { get; internal set; }

    /// <summary>Whether the bug has completed its one authored arc.</summary>
    public bool ArcCompleted { get; internal set; }
}
