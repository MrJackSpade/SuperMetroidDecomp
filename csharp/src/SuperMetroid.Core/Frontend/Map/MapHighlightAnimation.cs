using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Map.Artwork;

namespace SuperMetroid.Core.Frontend.Map;

/// <summary>
/// The map highlight cycle shared by the pause and file-select maps (#1512, #1597, #2359), played by the shared animation
/// player: each installed step shows for its installed hold, and past the last the cycle starts again from the first,
/// which requests the map's highlight-loop sound. The highlighted sprites always show the shade of the step shown, which
/// the import pre-colours the map sprite sheet in. Only the highlighted sprites show the cycle (#1544): the pause screen's
/// other sprites show their own colours.
/// </summary>
public sealed class MapHighlightAnimation
{
    /// <summary>The cycle as it plays.</summary>
    private readonly AnimationPlayer<MapHighlightSegment, MapHighlightStep, MapHighlightCue> player;

    /// <summary>Starts the highlight cycle at its first step, one update from its next, over the installed cycle's timing.</summary>
    /// <param name="cycle">The installed cycle's timing.</param>
    public MapHighlightAnimation(MapHighlightCycle cycle)
    {
        AnimationSegments<MapHighlightSegment, MapHighlightStep, MapHighlightCue> segments = Segments(cycle);
        player = AnimationPlayer<MapHighlightSegment, MapHighlightStep, MapHighlightCue>.StartShowing(segments, Cycling(segments), 1);
    }

    /// <summary>
    /// Binds the installed cycle's timing when the game is bound to its installed content, keeping the step shown and its
    /// updates left: a shorter replacement cycle wraps at the next step's boundary, not at the bind.
    /// </summary>
    public void Bind(MapHighlightCycle cycle)
    {
        AnimationSegments<MapHighlightSegment, MapHighlightStep, MapHighlightCue> segments = Segments(cycle);
        player.Rebind(segments, Cycling(segments));
    }

    /// <summary>Restores the cycle's first step, one update from its next.</summary>
    public void Reset() => player.Show(player.Playback.Animation, 1);

    /// <summary>The step the highlighted sprites show.</summary>
    internal MapHighlightStep Shown => player.Showing;

    /// <summary>The shading the map's highlighted sprites show (#1512, #1597): the highlight shade of the step shown.</summary>
    public PictureShading Shading => PictureShading.Shaded(MapSpriteShades.Highlight(Shown));

    /// <summary>Runs one update of the cycle; true requests the room library's map highlight-loop sound.</summary>
    public bool Step()
    {
        bool wrapped = false;
        player.Step(_ => wrapped = true);
        return wrapped;
    }

    /// <summary>The cycle's segments: its steps, each for its hold, and the wrap that starts it again.</summary>
    private static AnimationSegments<MapHighlightSegment, MapHighlightStep, MapHighlightCue> Segments(MapHighlightCycle cycle)
    {
        Ensure.NotNull(cycle);
        return new(new Dictionary<MapHighlightSegment, AnimationBeat<MapHighlightStep, MapHighlightCue>[]>
        {
            [MapHighlightSegment.Steps] =
                [.. cycle.Steps.Select(step => new AnimationBeat<MapHighlightStep, MapHighlightCue>.Frame(step, cycle.Duration(step)))],
            [MapHighlightSegment.Wrap] = [new AnimationBeat<MapHighlightStep, MapHighlightCue>.Event(MapHighlightCue.Wrapped)],
        });
    }

    /// <summary>The cycle played over and over, each pass ending in its wrap.</summary>
    private static Animation<MapHighlightSegment> Cycling(AnimationSegments<MapHighlightSegment, MapHighlightStep, MapHighlightCue> segments) =>
        new([segments.Whole(MapHighlightSegment.Steps), segments.Whole(MapHighlightSegment.Wrap)], AnimationEnd.LoopsFrom(0));
}
