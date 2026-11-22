using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>
/// The selected slot's helmet turn (#2359), played by the shared animation player: it turns to face the player and its visor
/// shines, each picture for the installed hold (#1615), and rests on its last. The turn starts on its first picture, one
/// update from its second.
/// </summary>
internal sealed partial class FileSelectHelmetAnimation
{
    /// <summary>The pictures of the turn, in the order it shows them; it rests on the last.</summary>
    internal static IReadOnlyList<FileSelectHelmetFrame> Turn { get; } =
    [
        FileSelectHelmetFrame.TurnedAway, FileSelectHelmetFrame.Turning, FileSelectHelmetFrame.ShineLeft,
        FileSelectHelmetFrame.ShineLeftOfMiddle, FileSelectHelmetFrame.ShineRightOfMiddle, FileSelectHelmetFrame.ShineRight,
        FileSelectHelmetFrame.SparkleTall, FileSelectHelmetFrame.SparkleWide,
    ];

    /// <summary>The turn as it plays.</summary>
    private readonly AnimationPlayer<FileSelectHelmetSegment, FileSelectHelmetFrame, NoAnimationCue> player;

    /// <summary>Readies the turn on its first picture, each picture held for <paramref name="frameDuration"/> updates.</summary>
    internal FileSelectHelmetAnimation(int frameDuration)
    {
        AnimationSegments<FileSelectHelmetSegment, FileSelectHelmetFrame, NoAnimationCue> segments = Segments(frameDuration);
        player = AnimationPlayer<FileSelectHelmetSegment, FileSelectHelmetFrame, NoAnimationCue>.StartShowing(segments, Turning(segments), 1);
    }

    /// <summary>The picture the selected helmet shows, which it keeps through the fade into the game options.</summary>
    internal FileSelectHelmetFrame Frame => player.Showing;

    /// <summary>Whether the turn has shown its last picture for its whole hold and rests on it.</summary>
    internal bool HasFinished => player.IsResting;

    /// <summary>Starts the turn again on its first picture, one update from its second.</summary>
    internal void Restart() => player.Show(player.Playback.Animation, 1);

    /// <summary>Runs one update of the turn.</summary>
    internal void Step() => player.Step();

    /// <summary>
    /// Holds each picture the turn moves on to for the rebound installed <paramref name="frameDuration"/>, keeping the
    /// picture shown and its updates left.
    /// </summary>
    internal void Rebind(int frameDuration)
    {
        AnimationSegments<FileSelectHelmetSegment, FileSelectHelmetFrame, NoAnimationCue> segments = Segments(frameDuration);
        player.Rebind(segments, Turning(segments));
    }

    /// <summary>The turn's one segment: each picture in turn, held for <paramref name="frameDuration"/> updates.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="frameDuration"/> is below one.</exception>
    private static AnimationSegments<FileSelectHelmetSegment, FileSelectHelmetFrame, NoAnimationCue> Segments(int frameDuration)
    {
        if (frameDuration < 1)
            throw new ArgumentOutOfRangeException(nameof(frameDuration), frameDuration, "The helmet holds each picture for at least one update.");
        return new(new Dictionary<FileSelectHelmetSegment, AnimationBeat<FileSelectHelmetFrame, NoAnimationCue>[]>
        {
            [FileSelectHelmetSegment.Turn] =
                [.. Turn.Select(frame => new AnimationBeat<FileSelectHelmetFrame, NoAnimationCue>.Frame(frame, frameDuration))],
        });
    }

    /// <summary>The turn played once, resting on its last picture.</summary>
    private static Animation<FileSelectHelmetSegment> Turning(AnimationSegments<FileSelectHelmetSegment, FileSelectHelmetFrame, NoAnimationCue> segments) =>
        new([segments.Whole(FileSelectHelmetSegment.Turn)], AnimationEnd.Holds);
}
