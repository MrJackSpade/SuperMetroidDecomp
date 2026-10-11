using SuperMetroid.Core.Input;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Frontend;

/// <summary>Pause-specific bounds from Handle_MapScrollArrows ($82:B934).</summary>
internal static class PauseMapScrollLayout
{
    /// <summary>$82:B9A0-B9BE pause arrow anchors are 24 pixels below the matching $81:AF32 file-select records.</summary>
    public const int ArrowVerticalOffset = 24;

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
internal sealed class PauseMapScroll(Position16<MapPixels> minimum, Position16<MapPixels> maximum)
{
    private MapScrollDirection direction;
    private int tick;

    public bool CanScroll(MapScrollDirection candidate, Position16<MapPixels> scroll) => candidate switch
    {
        MapScrollDirection.Left => Signed(minimum.X - PauseMapScrollLayout.LeftMargin - scroll.X) < 0,
        MapScrollDirection.Right => Signed(maximum.X - PauseMapScrollLayout.RightMargin - scroll.X) >= 0,
        MapScrollDirection.Up => Signed(minimum.Y - PauseMapScrollLayout.TopMargin - scroll.Y) < 0,
        MapScrollDirection.Down => Signed(maximum.Y - PauseMapScrollLayout.BottomMargin - scroll.Y) >= 0,
        MapScrollDirection.None => false,
        _ => throw new ArgumentOutOfRangeException(nameof(candidate), candidate, "Undefined map scroll direction."),
    };

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

    private static short Signed(int value) => unchecked((short)value);
}
