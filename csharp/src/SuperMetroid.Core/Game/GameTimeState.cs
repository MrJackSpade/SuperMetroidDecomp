namespace SuperMetroid.Core.Game;

/// <summary>
/// Cartridge-compatible gameplay clock stored at WRAM <c>$09DA-$09E1</c> and mirrored in
/// each SRAM slot. This is deliberately four words rather than a host <see cref="TimeSpan"/>:
/// the ROM increments at an assumed 60 accepted gameplay calls per second and saturates at
/// 99:59:59.59, independent of wall-clock refresh rate.
/// </summary>
public sealed class GameTimeState
{
    public ushort Frames { get; private set; }
    public ushort Seconds { get; private set; }
    public ushort Minutes { get; private set; }
    public ushort Hours { get; private set; }

    /// <summary>Executes <c>$82:DB69</c>'s four-word clock tail once.</summary>
    public void Step()
    {
        // The native comparisons are signed 16-bit, but ordinary values never approach
        // that boundary. Explicit unchecked increments preserve corrupt/debug WRAM behavior
        // while the final 100-hour clamp prevents the visible clock from wrapping.
        Frames = unchecked((ushort)(Frames + 1));
        if (Frames >= 60)
        {
            Frames = 0;
            Seconds = unchecked((ushort)(Seconds + 1));
            if (Seconds >= 60)
            {
                Seconds = 0;
                Minutes = unchecked((ushort)(Minutes + 1));
                if (Minutes >= 60)
                {
                    Minutes = 0;
                    Hours = unchecked((ushort)(Hours + 1));
                }
            }
        }

        if (Hours >= 100)
        {
            Frames = 59;
            Seconds = 59;
            Minutes = 59;
            Hours = 99;
        }
    }

    /// <summary>Restores the four literal SRAM words without host-side normalization.</summary>
    public void Load(ushort frames, ushort seconds, ushort minutes, ushort hours)
    {
        Frames = frames;
        Seconds = seconds;
        Minutes = minutes;
        Hours = hours;
    }
}
