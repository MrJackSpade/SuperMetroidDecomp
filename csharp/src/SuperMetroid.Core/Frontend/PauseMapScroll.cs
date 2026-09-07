using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Frontend;

/// <summary>Pause-specific bounds from Handle_MapScrollArrows ($82:B934).</summary>
internal static class PauseMapScrollLayout
{
    /// <summary>$82:B942: leftmost visible map margin.</summary>
    public const int LeftMargin = 24;
    /// <summary>$82:B954–B958: rightmost visible map coordinate, 256 minus 24.</summary>
    public const int RightMargin = 232;
    /// <summary>$82:B96A: upper map-holder coordinate.</summary>
    public const int TopMargin = 56;
    /// <summary>$82:B97C: lower scroll cutoff.</summary>
    public const int BottomMargin = 177;
}

/// <summary>
/// Pause-page held-input arbitration and the shared $82:925D eight-update scroll pulse.
/// File select uses different vertical bounds; pause must not inherit those limits.
/// </summary>
internal sealed class PauseMapScroll(ushort minimumX, ushort maximumX, ushort minimumY, ushort maximumY)
{
    private MapScrollDirection direction;
    private int tick;

    public bool Step(ushort heldInput, ref ushort horizontal, ref ushort vertical)
    {
        bool left = Signed(minimumX - PauseMapScrollLayout.LeftMargin - horizontal) < 0;
        bool right = Signed(maximumX - PauseMapScrollLayout.RightMargin - horizontal) >= 0;
        bool up = Signed(minimumY - PauseMapScrollLayout.TopMargin - vertical) < 0;
        bool down = Signed(maximumY - PauseMapScrollLayout.BottomMargin - vertical) >= 0;
        SnesButton held = (SnesButton)heldInput;
        if (direction == MapScrollDirection.None)
        {
            if (left && held.HasFlag(SnesButton.Left)) direction = MapScrollDirection.Left;
            else if (right && held.HasFlag(SnesButton.Right)) direction = MapScrollDirection.Right;
            else if (up && held.HasFlag(SnesButton.Up)) direction = MapScrollDirection.Up;
            else if (down && held.HasFlag(SnesButton.Down)) direction = MapScrollDirection.Down;
        }
        // Native explicitly cancels a downward step at its lower limit. Other directions
        // finish the accepted pulse even when their button has since been released.
        if (direction == MapScrollDirection.Down && !down)
        {
            direction = MapScrollDirection.None;
            tick = 0;
        }
        if (direction == MapScrollDirection.None) return false;
        if (++tick == FileSelectMapRomData.ScrollPulseTick)
        {
            if (direction == MapScrollDirection.Left) horizontal = Wrap(horizontal - FileSelectMapRomData.ScrollStepPixels);
            if (direction == MapScrollDirection.Right) horizontal = Wrap(horizontal + FileSelectMapRomData.ScrollStepPixels);
            if (direction == MapScrollDirection.Up) vertical = Wrap(vertical - FileSelectMapRomData.ScrollStepPixels);
            if (direction == MapScrollDirection.Down) vertical = Wrap(vertical + FileSelectMapRomData.ScrollStepPixels);
        }
        if (tick != FileSelectMapRomData.ScrollStepTicks) return false;
        tick = 0;
        direction = MapScrollDirection.None;
        return true;
    }

    private static short Signed(int value) => unchecked((short)value);
    private static ushort Wrap(int value) => unchecked((ushort)value);
}
