namespace SuperMetroid.Core.Game;

/// <summary>One debugger-visible packed atmospheric slot.</summary>
public sealed class SamusAtmosphericEffectSlot
{
    /// <summary>Low byte animation frame, high byte graphics type.</summary>
    public ushort FrameAndType { get; internal set; }

    /// <summary>Low byte of the $0AEC array's packed word: the zero-based frame within the current graphics type.</summary>
    public byte AnimationFrame => unchecked((byte)FrameAndType);

    /// <summary>High byte of the packed word: 1/2 footstep splashes, 3 diving splash, 4 lava spray, 5 bubbles, or 6/7 dust; zero is reserved for inactive state.</summary>
    public byte Type => SnesLittleEndian.HighByte(FrameAndType);

    /// <summary>$0AD4 array countdown in update calls; negative words suppress motion/drawing until decrement reaches $8000 and reloads the current frame duration.</summary>
    public NativeCountdown AnimationTimer { get; internal set; }

    /// <summary>
    /// $0ADC/$0AE4 array anchor in whole room pixels; camera subtraction occurs only when
    /// drawing, motion wraps as native words, diving splashes follow the live FX surface and
    /// lava spray/dust rise one pixel per active update.
    /// </summary>
    public Position16<RoomPixels> Position { get; internal set; }

    /// <summary>Resets the packed word, delayed timer, and anchor during room or Samus initialization.</summary>
    internal void Clear()
    {
        FrameAndType = 0;
        AnimationTimer = NativeCountdown.Start(0);
        Position = Position16<RoomPixels>.Zero;
    }
}
