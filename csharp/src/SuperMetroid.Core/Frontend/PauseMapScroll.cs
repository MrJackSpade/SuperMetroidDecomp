using SuperMetroid.Core.Input;

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
/// <param name="minimumX">Left boundary of the pause map's scrollable extent.</param>
/// <param name="maximumX">Right boundary of the pause map's scrollable extent.</param>
/// <param name="minimumY">Upper boundary of the pause map's scrollable extent.</param>
/// <param name="maximumY">Lower boundary of the pause map's scrollable extent.</param>
internal sealed class PauseMapScroll(ushort minimumX, ushort maximumX, ushort minimumY, ushort maximumY)
{
    /// <summary>Direction accepted for the current multi-update scroll pulse.</summary>
    private MapScrollDirection direction;
    /// <summary>Number of updates elapsed in the currently accepted scroll pulse.</summary>
    private int tick;

    /// <summary>Reports whether the map can continue scrolling in the requested direction.</summary>
    /// <param name="candidate">Direction whose pause-map boundary is being checked.</param>
    /// <param name="horizontal">Current horizontal map offset.</param>
    /// <param name="vertical">Current vertical map offset.</param>
    /// <returns><see langword="true"/> when that direction has not reached its pause-map cutoff.</returns>
    public bool CanScroll(MapScrollDirection candidate, ushort horizontal, ushort vertical) => candidate switch
    {
        MapScrollDirection.Left => Signed(minimumX - PauseMapScrollLayout.LeftMargin - horizontal) < 0,
        MapScrollDirection.Right => Signed(maximumX - PauseMapScrollLayout.RightMargin - horizontal) >= 0,
        MapScrollDirection.Up => Signed(minimumY - PauseMapScrollLayout.TopMargin - vertical) < 0,
        MapScrollDirection.Down => Signed(maximumY - PauseMapScrollLayout.BottomMargin - vertical) >= 0,
        _ => false,
    };

    /// <summary>Arbitrates held directions and advances one update of the pause-map scroll pulse.</summary>
    /// <param name="heldInput">Controller-button mask held on this update.</param>
    /// <param name="horizontal">Current horizontal offset, updated when the pulse reaches its movement tick.</param>
    /// <param name="vertical">Current vertical offset, updated when the pulse reaches its movement tick.</param>
    /// <returns><see langword="true"/> when the accepted pulse completes; otherwise, <see langword="false"/>.</returns>
    public bool Step(ushort heldInput, ref ushort horizontal, ref ushort vertical)
    {
        bool left = CanScroll(MapScrollDirection.Left, horizontal, vertical);
        bool right = CanScroll(MapScrollDirection.Right, horizontal, vertical);
        bool up = CanScroll(MapScrollDirection.Up, horizontal, vertical);
        bool down = CanScroll(MapScrollDirection.Down, horizontal, vertical);
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

    /// <summary>Interprets wrapped 16-bit coordinate arithmetic as a signed displacement.</summary>
    /// <param name="value">Integer difference to narrow to SNES signed-coordinate width.</param>
    /// <returns>The low 16 bits interpreted as a signed value.</returns>
    private static short Signed(int value) => unchecked((short)value);
    /// <summary>Applies 16-bit wrapping to a map coordinate update.</summary>
    /// <param name="value">Integer coordinate after applying the scroll step.</param>
    /// <returns>The low 16 bits of the coordinate.</returns>
    private static ushort Wrap(int value) => unchecked((ushort)value);
}
