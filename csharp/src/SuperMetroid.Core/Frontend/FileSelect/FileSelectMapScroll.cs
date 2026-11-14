using SuperMetroid.Core.Input;
using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Map;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>Initial room-select positioning and scroll ownership.</summary>
public sealed class FileSelectMapScroll
{
    /// <summary>Elapsed menu ticks in the accepted eight-tick directional step.</summary>
    private int tick;

    /// <summary>Gets the current wrapped map scroll offset in map pixels.</summary>
    public Position16<MapPixels> Scroll { get; private set; }

    /// <summary>Gets the leftmost and adjusted upper revealed-map coordinates used by the arrow boundaries.</summary>
    public Position16<MapPixels> Minimum { get; }

    /// <summary>Gets the rightmost and lower revealed-map coordinates used by the arrow boundaries.</summary>
    public Position16<MapPixels> Maximum { get; }

    /// <summary>Gets the direction of the accepted eight-tick scroll step, or <see cref="MapScrollDirection.None"/>.</summary>
    public MapScrollDirection Direction { get; private set; }

    /// <summary>Calculates the initial viewport and scrolling bounds from the visible map plane.</summary>
    /// <param name="map">Area map whose station or explored cells define the visible bounds.</param>
    /// <param name="system">Persistent area-map and explored-cell state.</param>
    /// <param name="player">Player marker's map coordinate in pixels.</param>
    public FileSelectMapScroll(IAreaMapView map,
        SharedGameState system, Position16<MapPixels> player)
    {
        Ensure.NotNull(map);
        Ensure.NotNull(system);
        // The game selects one plane, not the union: a downloaded map uses its station
        // mask even if an explored secret cell lies outside that mask.
        bool Visible(int x, int y) => system.HasAreaMap(map.Area)
            ? map.IsRevealedByMapStation(new(x, y)) : system.IsMapTileExplored(map.Area, new(x, y));
        int left = 26, right = 28, top = 1, bottom = 11;
        for (int x = 0; x < 64; x++)
            if (Enumerable.Range(0, 32).Any(y => Visible(x, y))) { left = x; break; }
        for (int x = 63; x >= 0; x--)
            if (Enumerable.Range(0, 32).Any(y => Visible(x, y))) { right = x; break; }
        // The retail scans return their defaults before inspecting the last row in
        // either direction. Preserve that boundary behavior for sparse/empty maps.
        for (int y = 0; y < 31; y++)
            if (Enumerable.Range(0, 64).Any(x => Visible(x, y))) { top = y; break; }
        for (int y = 31; y > 0; y--)
            if (Enumerable.Range(0, 64).Any(x => Visible(x, y))) { bottom = y; break; }
        // Every value below is a map pixel or a difference of two, within a few hundred pixels of the 512 by 256 map:
        // the scans keep the left column at or before the right and the top row at or above the bottom, so the
        // halved spans are never negative and no value comes near the coordinate range. Only a negative result (Maridia's
        // margin left of the first column, a scroll above the map) is stored as its wrapped coordinate.
        int minimumX = left * 8 - (map.Area == AreaId.Maridia ? 24 : 0);
        int maximumX = right * 8;
        int minimumY = top * 8;
        int maximumY = bottom * 8;
        int horizontal = minimumX + (maximumX - minimumX) / 2 - 128;
        int screenX = player.X - horizontal;
        if (224 - screenX < 0)
            horizontal -= 224 - screenX;
        else if (32 - screenX >= 0)
            horizontal -= 32 - screenX;
        int middle = minimumY + (maximumY - minimumY) / 2 + 16;
        // The centring offset is rounded down to whole eight-pixel map rows.
        int offset = MapScreenConversions.RoundDownToMapTile(112 - middle);
        int vertical = -offset;
        int distance = 64 - (player.Y + offset);
        if (distance >= 0)
        {
            vertical -= distance;
            if (vertical + 40 < 0) vertical = -40;
        }
        Scroll = Position16<MapPixels>.Wrapped(horizontal, vertical);
        Maximum = Position16<MapPixels>.Wrapped(maximumX, maximumY);
        // The game adjusts the upper arrow boundary only after positioning.
        Minimum = Position16<MapPixels>.Wrapped(minimumX, minimumY + 24);
    }

    /// <summary>Determines whether another eight-pixel step fits within the boundary for a direction.</summary>
    /// <param name="direction">Direction to test.</param>
    /// <returns><see langword="true"/> when the corresponding scroll arrow may accept a step.</returns>
    public bool CanScroll(MapScrollDirection direction) => direction switch
    {
        MapScrollDirection.Left => Signed(Minimum.X - 24 - Scroll.X) < 0,
        MapScrollDirection.Right => Signed(Maximum.X - 232 - Scroll.X) >= 0,
        MapScrollDirection.Up => Signed(Minimum.Y - 64 - Scroll.Y) < 0,
        MapScrollDirection.Down => Signed(Maximum.Y - 145 - Scroll.Y) >= 0,
        MapScrollDirection.None => false,
        _ => throw new ArgumentOutOfRangeException(nameof(direction)),
    };

    /// <summary>
    /// Processes held input in arrow order. Returns true at the scroll-sound
    /// boundary. A released button does not cancel an already accepted eight-tick step.
    /// </summary>
    public bool Step(ControllerButton heldInput)
    {
        for (int index = 0; index < MapScrollControls.DirectionCount; index++)
            if (Direction == MapScrollDirection.None && CanScroll(MapScrollControls.ArrowAt(index)) &&
                (heldInput & MapScrollControls.ButtonFor(MapScrollControls.ArrowAt(index))) != ControllerButton.None)
                Direction = MapScrollControls.ArrowAt(index);
        // The game has this explicit cancellation only for the lower boundary.
        if (Direction == MapScrollDirection.Down && !CanScroll(Direction))
        {
            Direction = MapScrollDirection.None;
            tick = 0;
        }
        if (Direction == MapScrollDirection.None) return false;
        // Both halves of the map scrolling speed data contain the same eight-tick pulse:
        // move eight pixels on tick four, then finish and queue sound on tick eight.
        if (++tick == MapScrollDefinitions.PulseTick)
        {
            Scroll += MapScrollControls.StepOffset(Direction);
        }
        if (tick != MapScrollDefinitions.StepTicks) return false;
        tick = 0;
        Direction = MapScrollDirection.None;
        return true;
    }

    /// <summary>A map coordinate or boundary difference read as a signed offset, as the scroll's bounds checks read it.</summary>
    private static int Signed(int value) => CoordinateAxis.AsSigned(value);
}
