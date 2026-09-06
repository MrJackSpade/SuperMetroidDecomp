using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.Core.Frontend;

/// <summary>Shared $82:A92B palette animation used by pause and file-select maps.</summary>
public sealed class MapPaletteAnimation(ISnesAddressSpace bus)
{
    private byte timer = 1;
    private byte frame;

    /// <summary>Restores ResetPauseMenuAnimations' initial frame and one-tick delay.</summary>
    public void Reset() { timer = 1; frame = 0; }

    /// <summary>Copies the native palette frame; true requests library-three MapPaletteLoop.</summary>
    public bool Step(SnesCgram cgram)
    {
        if (timer == 0 || --timer != 0) return false;
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
