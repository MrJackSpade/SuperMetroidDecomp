using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Map.Artwork;

namespace SuperMetroid.Core.Frontend.Map;

/// <summary>
/// The map highlight cycle shared by the pause and file-select maps (#1512): its frame and hold, and the shade the
/// highlighted sprites show at that frame, which the import pre-colours the map sprite sheet in.
/// </summary>
public sealed class MapPaletteAnimation
{
    /// <summary>
    /// Updates remaining before the next palette frame is applied; starts with one update. Zero holds the animation:
    /// a step decrements only a positive count, so the value stays within zero through the longest authored hold.
    /// </summary>
    internal int Timer { get; set; } = 1;
    /// <summary>Index of the most recently applied map palette frame, below the installed cycle's frame count once stepped.</summary>
    internal int Frame { get; set; }
    /// <summary>The installed cycle's timing.</summary>
    private MapHighlightCycle? content;

    /// <summary>Binds the installed cycle's timing when the game is bound to its installed content, without resetting the cycle's timer.</summary>
    public void Bind(MapHighlightCycle? cycle) => content = cycle;

    /// <summary>Restores ResetPauseMenuAnimations' initial frame and one-tick delay.</summary>
    public void Reset() { Timer = 1; Frame = 0; }

    /// <summary>The shading the map's highlighted sprites show at the current frame (#1512).</summary>
    public PictureShading Shading => PictureShading.Shaded(MapSpriteShades.Highlight(Frame));

    /// <summary>Advances to the next highlight frame when its hold ends; true requests library-three MapPaletteLoop.</summary>
    /// <exception cref="InvalidOperationException">The installed cycle is not bound.</exception>
    public bool Step()
    {
        if (Timer == 0) return false;
        Timer--;
        if (Timer != 0) return false;
        MapHighlightCycle cycle = content ?? throw new InvalidOperationException(
            "Map palette animation requires installed palette-cycle assets.");
        // Preserve the game's increment-before-read and loop sound ownership. A
        // shorter replacement cycle wraps at the next frame boundary, not bind.
        int next = Frame + 1;
        bool wrapped = next >= cycle.FrameCount;
        Frame = wrapped ? 0 : next;
        Timer = cycle.Duration(Frame);
        return wrapped;
    }
}
