using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>
/// A Brinstar Pipe Bug's state, the ordinary Zeb's or the strong Zebbo's (#1845): it waits in its pipe, rises out of it
/// toward Samus, flies across the screen and waits hidden to emerge again.
/// </summary>
public sealed class BrinstarPipeBugState
{
    /// <summary>Starts the state in <paramref name="action"/>.</summary>
    internal BrinstarPipeBugState(BrinstarPipeBugAction action) => Action = action;

    /// <summary>The animation the bug plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<PipeBugSegment> Playback { get; set; }

    /// <summary>The current action, one of the Brinstar actions.</summary>
    public BrinstarPipeBugAction Action { get; internal set; }

    /// <summary>The whole-pixel spawn position the bug returns to after leaving the screen.</summary>
    public Position16<RoomPixels> SpawnPosition { get; internal set; }

    /// <summary>
    /// The room Y of the highest point of the bug's emergence, above its spawn point. The bug keeps rising
    /// while it is still below this point.
    /// </summary>
    public int EmergenceTopY { get; internal set; }

    /// <summary>
    /// The way the bug flies, set from Samus's side of the bug every update it waits with Samus in its vertical band (#2278).
    /// </summary>
    public HorizontalDirection Flight { get; internal set; }

    /// <summary>
    /// The respawn countdown, in AI updates: started at its respawn delay when the bug leaves the screen, and expiring when a decrement
    /// reaches zero.
    /// </summary>
    public UpdateCountdown RespawnDelay { get; internal set; } = UpdateCountdown.Ended();
}
