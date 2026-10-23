using SuperMetroid.Core.Frontend.Map.Artwork;

namespace SuperMetroid.Core.Frontend.Map;

/// <summary>Shared $82:A92B palette animation used by pause and file-select maps.</summary>
public sealed class MapPaletteAnimation
{
    /// <summary>
    /// Updates remaining before the next palette frame is applied; starts with one update. Zero holds the animation:
    /// a step decrements only a positive count, so the value stays within zero through the longest authored hold.
    /// </summary>
    internal int Timer { get; set; } = 1;
    /// <summary>Index of the most recently applied map palette frame, below the installed cycle's frame count once stepped.</summary>
    internal int Frame { get; set; }
    /// <summary>Installed host color cycle, excluded from serialized gameplay state.</summary>
    [NonSerialized] private MapPaletteCycle? content;

    /// <summary>Rebinds host colors without resetting a saved timer or capturing assets in the state graph.</summary>
    public void Bind(MapPaletteCycle? cycle) => content = cycle;

    /// <summary>Restores ResetPauseMenuAnimations' initial frame and one-tick delay.</summary>
    public void Reset() { Timer = 1; Frame = 0; }

    /// <summary>Copies the native palette frame; true requests library-three MapPaletteLoop.</summary>
    public bool Step(IPaletteColors colours)
    {
        if (Timer == 0) return false;
        Timer--;
        if (Timer != 0) return false;
        MapPaletteCycle cycle = content ?? throw new InvalidOperationException(
            "Map palette animation requires installed palette-cycle assets.");
        // Preserve native increment-before-read and loop sound ownership. A
        // shorter replacement cycle wraps at the next frame boundary, not bind.
        int next = Frame + 1;
        bool wrapped = next >= cycle.FrameCount;
        Frame = wrapped ? 0 : next;
        Timer = cycle.Duration(Frame);
        cycle.Apply(colours, Frame, MapAnimationRomData.PaletteDestination);
        return wrapped;
    }
}
