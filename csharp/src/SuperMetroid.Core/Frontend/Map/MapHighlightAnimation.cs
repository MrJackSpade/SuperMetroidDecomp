using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Map.Artwork;

namespace SuperMetroid.Core.Frontend.Map;

/// <summary>
/// The map highlight cycle shared by the pause and file-select maps (#1512, #1597): its frame and hold, and the shade the
/// highlighted sprites show, which the import pre-colours the map sprite sheet in. The highlighted sprites always show the
/// shade of the cycle's current frame. Only the highlighted sprites show the cycle (#1544): the pause screen's other
/// sprites show their own colours.
/// </summary>
public sealed class MapHighlightAnimation
{
    /// <summary>
    /// Updates remaining before the next highlight frame; starts with one update. Zero holds the animation: a step
    /// decrements only a positive count, so the value stays within zero through the longest authored hold.
    /// </summary>
    internal int Timer { get; private set; } = 1;
    /// <summary>The cycle's current frame; the highlighted sprites show its shade.</summary>
    internal MapHighlightFrame Frame { get; private set; } = MapHighlightFrame.First;
    /// <summary>The installed cycle's timing.</summary>
    private MapHighlightCycle content;

    /// <summary>Starts the highlight cycle at its first frame, one update from its next, over the installed cycle's timing.</summary>
    /// <param name="cycle">The installed cycle's timing.</param>
    public MapHighlightAnimation(MapHighlightCycle cycle) => content = Ensure.NotNull(cycle);

    /// <summary>Binds the installed cycle's timing when the game is bound to its installed content, without resetting the cycle's timer.</summary>
    public void Bind(MapHighlightCycle cycle) => content = Ensure.NotNull(cycle);

    /// <summary>Restores the cycle's initial frame and one-tick delay.</summary>
    public void Reset() { Timer = 1; Frame = MapHighlightFrame.First; }

    /// <summary>The shading the map's highlighted sprites show (#1512, #1597): the highlight shade of the cycle's current frame.</summary>
    public PictureShading Shading => PictureShading.Shaded(MapSpriteShades.Highlight(Frame));

    /// <summary>Advances to the next highlight frame when its hold ends; true requests the room library's map highlight-loop sound.</summary>
    public bool Step()
    {
        if (Timer == 0) return false;
        Timer--;
        if (Timer != 0) return false;
        MapHighlightCycle cycle = content;
        // Preserve the game's increment-before-read and loop sound ownership. A
        // shorter replacement cycle wraps at the next frame boundary, not bind.
        MapHighlightFrame? next = cycle.After(Frame);
        bool wrapped = next is null;
        Frame = next is { } following ? following : MapHighlightFrame.First;
        Timer = cycle.Duration(Frame);
        return wrapped;
    }
}
