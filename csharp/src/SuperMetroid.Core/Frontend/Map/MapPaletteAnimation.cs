using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Map.Artwork;

namespace SuperMetroid.Core.Frontend.Map;

/// <summary>
/// The map highlight cycle shared by the pause and file-select maps (#1512): its frame and hold, and the shade the
/// highlighted sprites show, which the import pre-colours the map sprite sheet in. Until the cycle first applies a frame
/// the highlighted sprites show the screen's own colours, as the game's highlight colours are the screen's own ones
/// until its first step shows a frame over them. Only the highlighted sprites show the cycle (#1544): the pause screen's
/// other sprites show their own colours.
/// </summary>
public sealed class MapPaletteAnimation
{
    /// <summary>The map screen whose own colours the highlighted sprites show before the cycle's first applied frame.</summary>
    private readonly MapSpriteScreen screen;

    /// <summary>Starts the cycle on <paramref name="screen"/>, whose colours the highlighted sprites show until its first frame.</summary>
    public MapPaletteAnimation(MapSpriteScreen screen) => this.screen = screen;

    /// <summary>
    /// Updates remaining before the next palette frame is applied; starts with one update. Zero holds the animation:
    /// a step decrements only a positive count, so the value stays within zero through the longest authored hold.
    /// </summary>
    internal int Timer { get; set; } = 1;
    /// <summary>The cycle's current frame index, below the installed cycle's frame count once stepped.</summary>
    internal int Frame { get; set; }
    /// <summary>
    /// The frame whose colours the highlighted sprites show: the last frame a step applied, or null before the first.
    /// Resetting the cycle's frame leaves it, as the colours stay until the next step writes over them.
    /// </summary>
    internal int? ShownFrame { get; set; }
    /// <summary>The installed cycle's timing.</summary>
    private MapHighlightCycle? content;

    /// <summary>Binds the installed cycle's timing when the game is bound to its installed content, without resetting the cycle's timer.</summary>
    public void Bind(MapHighlightCycle? cycle) => content = cycle;

    /// <summary>Restores ResetPauseMenuAnimations' initial frame and one-tick delay.</summary>
    public void Reset() { Timer = 1; Frame = 0; }

    /// <summary>
    /// The shading the map's highlighted sprites show (#1512): the screen's own colours before the first applied frame,
    /// then the highlight shade of the frame last applied.
    /// </summary>
    public PictureShading Shading => PictureShading.Shaded(ShownFrame is int frame
        ? MapSpriteShades.Highlight(frame)
        : MapSpriteShades.Drawn(screen, MapSpriteColouring.Highlighted));

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
        ShownFrame = Frame;
        Timer = cycle.Duration(Frame);
        return wrapped;
    }
}
