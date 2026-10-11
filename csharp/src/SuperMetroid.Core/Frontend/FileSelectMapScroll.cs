using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Frontend;

/// <summary>$82:925D's mutually exclusive map-scrolling dispatcher indices.</summary>
public enum MapScrollDirection
{
    /// <summary>No eight-tick scroll step is active.</summary>
    None,
    /// <summary>Scroll the map viewport toward smaller horizontal offsets.</summary>
    Left,
    /// <summary>Scroll the map viewport toward larger horizontal offsets.</summary>
    Right,
    /// <summary>Scroll the map viewport toward smaller vertical offsets.</summary>
    Up,
    /// <summary>Scroll the map viewport toward larger vertical offsets.</summary>
    Down
}

/// <summary>Initial room-select positioning and $81:AECA/$82:925D scroll ownership.</summary>
public sealed class FileSelectMapScroll
{
    private int tick;

    /// <summary>Gets the current wrapped map scroll offset in map pixels.</summary>
    public Position16<MapPixels> Scroll { get; private set; }

    /// <summary>Gets the leftmost and adjusted upper revealed-map coordinates used by the native arrow boundaries.</summary>
    public Position16<MapPixels> Minimum { get; }

    /// <summary>Gets the rightmost and lower revealed-map coordinates used by the native arrow boundaries.</summary>
    public Position16<MapPixels> Maximum { get; }

    /// <summary>Gets the direction of the accepted eight-tick scroll step, or <see cref="MapScrollDirection.None"/>.</summary>
    public MapScrollDirection Direction { get; private set; }

    /// <summary>Calculates the initial viewport and scrolling bounds from the visible map plane.</summary>
    /// <param name="bus">Installed address space required by the file-select host contract.</param>
    /// <param name="map">Area map whose station or explored cells define the visible bounds.</param>
    /// <param name="system">Persistent area-map and explored-cell state.</param>
    /// <param name="player">Player marker's map coordinate in pixels.</param>
    public FileSelectMapScroll(ISnesAddressSpace bus, IAreaMapView map,
        Bank80SystemState system, Position16<MapPixels> player)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(map);
        ArgumentNullException.ThrowIfNull(system);
        // Native selects one plane, not the union: a downloaded map uses its station
        // mask even if an explored secret cell lies outside that mask.
        bool Visible(int x, int y) => system.HasAreaMap(map.Area)
            ? map.IsRevealedByMapStation(new(checked((ushort)x), checked((ushort)y))) : system.IsMapTileExplored(map.Area, new(checked((ushort)x), checked((ushort)y)));
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
        ushort minimumX = Wrap(left * 8 - (map.Area == AreaId.Maridia ? 24 : 0));
        ushort maximumX = Wrap(right * 8);
        ushort nativeMinimumY = Wrap(top * 8);
        ushort maximumY = Wrap(bottom * 8);
        ushort horizontal = Wrap(minimumX + Wrap(maximumX - minimumX) / 2 - 128);
        ushort screenX = Wrap(player.X - horizontal);
        if (Signed(224 - screenX) < 0)
            horizontal = Wrap(horizontal - Signed(224 - screenX));
        else if (Signed(32 - screenX) >= 0)
            horizontal = Wrap(horizontal - Wrap(32 - screenX));
        ushort middle = Wrap(nativeMinimumY + Wrap(maximumY - nativeMinimumY) / 2 + 16);
        ushort offset = Wrap((112 - middle) & ~7);
        ushort vertical = Wrap(-offset);
        short distance = Signed(64 - Wrap(player.Y + offset));
        if (distance >= 0)
        {
            vertical = Wrap(vertical - distance);
            if (Signed(vertical + 40) < 0) vertical = Wrap(-40);
        }
        Scroll = new(horizontal, vertical);
        Maximum = new(maximumX, maximumY);
        // $81:AD17 adjusts the upper arrow boundary only after positioning.
        Minimum = new(minimumX, Wrap(nativeMinimumY + 24));
    }

    /// <summary>Determines whether another eight-pixel step fits within the native boundary for a direction.</summary>
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
    /// Processes held input in native arrow order. Returns true at the scroll-sound
    /// boundary. A released button does not cancel an already accepted eight-tick step.
    /// </summary>
    public bool Step(ushort heldInput)
    {
        for (int index = 0; index < MapScrollControls.DirectionCount; index++)
            if (Direction == MapScrollDirection.None && CanScroll(MapScrollControls.ArrowAt(index)) &&
                (heldInput & MapScrollControls.ButtonFor(MapScrollControls.ArrowAt(index))) != 0)
                Direction = MapScrollControls.ArrowAt(index);
        // Native has this explicit cancellation only for the lower boundary.
        if (Direction == MapScrollDirection.Down && !CanScroll(Direction))
        {
            Direction = MapScrollDirection.None;
            tick = 0;
        }
        if (Direction == MapScrollDirection.None) return false;
        // Both halves of the MapScrolling speed data contain the same eight-tick pulse:
        // move eight pixels on tick four, then finish and queue sound on tick eight.
        if (++tick == FileSelectMapRomData.ScrollPulseTick)
        {
            Scroll += MapScrollControls.StepOffset(Direction);
        }
        if (tick != FileSelectMapRomData.ScrollStepTicks) return false;
        tick = 0;
        Direction = MapScrollDirection.None;
        return true;
    }

    private static ushort Wrap(int value) => unchecked((ushort)value);
    private static short Signed(int value) => unchecked((short)value);
}
