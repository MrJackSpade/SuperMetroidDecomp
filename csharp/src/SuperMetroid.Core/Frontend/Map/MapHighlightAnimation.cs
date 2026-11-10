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
    internal int Timer { get; set; } = 1;
    /// <summary>The cycle's current frame index, below the installed cycle's frame count; the highlighted sprites show its shade.</summary>
    internal int Frame { get; set; }
    /// <summary>The installed cycle's timing.</summary>
    private MapHighlightCycle? content;

    /// <summary>Binds the installed cycle's timing when the game is bound to its installed content, without resetting the cycle's timer.</summary>
    public void Bind(MapHighlightCycle? cycle) => content = cycle;

    /// <summary>Restores the cycle's initial frame and one-tick delay.</summary>
    public void Reset() { Timer = 1; Frame = 0; }

    /// <summary>The shading the map's highlighted sprites show (#1512, #1597): the highlight shade of the cycle's current frame.</summary>
    public PictureShading Shading => PictureShading.Shaded(MapSpriteShades.Highlight(Frame));

    /// <summary>Advances to the next highlight frame when its hold ends; true requests the room library's map highlight-loop sound.</summary>
    /// <exception cref="InvalidOperationException">The installed cycle is not bound.</exception>
    public bool Step()
    {
        if (Timer == 0) return false;
        Timer--;
        if (Timer != 0) return false;
        MapHighlightCycle cycle = content ?? throw new InvalidOperationException(
            "The map highlight animation requires the installed highlight cycle.");
        // Preserve the game's increment-before-read and loop sound ownership. A
        // shorter replacement cycle wraps at the next frame boundary, not bind.
        int next = Frame + 1;
        bool wrapped = next >= cycle.FrameCount;
        Frame = wrapped ? 0 : next;
        Timer = cycle.Duration(Frame);
        return wrapped;
    }
}
