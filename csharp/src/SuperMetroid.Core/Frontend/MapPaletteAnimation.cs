using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.Core.Frontend;

/// <summary>Shared $82:A92B palette animation used by pause and file-select maps.</summary>
public sealed class MapPaletteAnimation(ISnesAddressSpace bus)
{
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
        if (content is not null)
        {
            // Preserve native increment-before-read and loop sound ownership. A
            // shorter replacement cycle wraps at the next frame boundary, not bind.
            int next = frame + 1;
            bool wrapped = next >= content.FrameCount;
            frame = (byte)(wrapped ? 0 : next);
            timer = content.Duration(frame);
            content.Apply(cgram, frame, MapAnimationRomData.PaletteDestination);
            return wrapped;
        }
        frame++;
        byte delay = bus.ReadByte(MapAnimationRomData.PaletteTiming + frame * 3);
        bool looped = delay == byte.MaxValue;
        if (looped)
        {
            frame = 0;
            delay = bus.ReadByte(MapAnimationRomData.PaletteTiming);
            if (delay == byte.MaxValue) throw new InvalidDataException("Map palette animation has no frames.");
        }
        timer = delay;
        for (int color = 0; color < 16; color++)
            cgram.SetColor(MapAnimationRomData.PaletteDestination + color,
                RomDataReader.ReadWordFixedBank(bus, MapAnimationRomData.PaletteColors + frame * 32 + color * 2));
        return looped;
    }
}
