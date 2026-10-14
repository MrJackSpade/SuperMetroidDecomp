using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Pause-page held-input arbitration and the shared $82:925D eight-update scroll pulse.
/// File select uses different vertical bounds; pause must not inherit those limits.
/// </summary>
internal sealed class PauseMapScroll(Position16<MapPixels> minimum, Position16<MapPixels> maximum)
{
    /// <summary>Keeps the accepted arrow direction active until its eight-update pulse completes.</summary>
    private MapScrollDirection direction;
    /// <summary>Counts updates within the current pulse, including the single position update.</summary>
    private int tick;

    /// <summary>Checks the pause map's signed cartridge bounds for an arrow at the current map offset.</summary>
    public bool CanScroll(MapScrollDirection candidate, Position16<MapPixels> scroll) => candidate switch
    {
        MapScrollDirection.Left => Signed(minimum.X - PauseMapScrollLayout.LeftMargin - scroll.X) < 0,
        MapScrollDirection.Right => Signed(maximum.X - PauseMapScrollLayout.RightMargin - scroll.X) >= 0,
        MapScrollDirection.Up => Signed(minimum.Y - PauseMapScrollLayout.TopMargin - scroll.Y) < 0,
        MapScrollDirection.Down => Signed(maximum.Y - PauseMapScrollLayout.BottomMargin - scroll.Y) >= 0,
        MapScrollDirection.None => false,
        _ => throw new ArgumentOutOfRangeException(nameof(candidate), candidate, "Undefined map scroll direction."),
    };

    /// <summary>Arbitrates held arrows left-first, advances one map offset during each eight-update pulse, and reports pulse completion.</summary>
    public bool Step(ushort heldInput, ref Position16<MapPixels> scroll)
    {
        bool left = CanScroll(MapScrollDirection.Left, scroll);
        bool right = CanScroll(MapScrollDirection.Right, scroll);
        bool up = CanScroll(MapScrollDirection.Up, scroll);
        bool down = CanScroll(MapScrollDirection.Down, scroll);
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
            scroll += MapScrollControls.StepOffset(direction);
        }
        if (tick != FileSelectMapRomData.ScrollStepTicks) return false;
        tick = 0;
        direction = MapScrollDirection.None;
        return true;
    }

    /// <summary>Applies the native signed 16-bit comparison to a map-bound subtraction.</summary>
    private static short Signed(int value) => unchecked((short)value);
}
