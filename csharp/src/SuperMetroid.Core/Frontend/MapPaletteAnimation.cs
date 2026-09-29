using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend;

/// <summary>Shared $82:A92B palette animation used by pause and file-select maps.</summary>
public sealed class MapPaletteAnimation
{
    public MapPaletteAnimation(ISnesAddressSpace bus) => ArgumentNullException.ThrowIfNull(bus);
    private byte timer = 1;
    private byte frame;
    [NonSerialized] private MapPaletteCycle? content;

    /// <summary>Rebinds host colors without resetting a saved timer or capturing assets in the state graph.</summary>
    public void Bind(MapPaletteCycle? cycle) => content = cycle;

    /// <summary>Restores ResetPauseMenuAnimations' initial frame and one-tick delay.</summary>
    public void Reset() { timer = 1; frame = 0; }

    /// <summary>Copies the native palette frame; true requests library-three MapPaletteLoop.</summary>
    public bool Step(SnesCgram cgram)
    {
        if (timer == 0 || --timer != 0) return false;
        MapPaletteCycle cycle = content ?? throw new InvalidOperationException(
            "Map palette animation requires installed palette-cycle assets.");
        // Preserve native increment-before-read and loop sound ownership. A
        // shorter replacement cycle wraps at the next frame boundary, not bind.
        int next = frame + 1;
        bool wrapped = next >= cycle.FrameCount;
        frame = (byte)(wrapped ? 0 : next);
        timer = cycle.Duration(frame);
        cycle.Apply(cgram, frame, MapAnimationRomData.PaletteDestination);
        return wrapped;
    }
}
