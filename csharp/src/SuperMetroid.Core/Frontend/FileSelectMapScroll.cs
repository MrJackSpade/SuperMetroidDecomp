using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.Core.Frontend;

/// <summary>$82:925D's mutually exclusive map-scrolling dispatcher indices.</summary>
public enum MapScrollDirection { None, Left, Right, Up, Down }

/// <summary>Initial room-select positioning and $81:AECA/$82:925D scroll ownership.</summary>
public sealed class FileSelectMapScroll
{
    private readonly ushort[] buttons = new ushort[4];
    private int tick;
    public ushort Horizontal { get; private set; }
    public ushort Vertical { get; private set; }
    public ushort MinimumX { get; }
    public ushort MaximumX { get; }
    public ushort MinimumY { get; }
    public ushort MaximumY { get; }
    public MapScrollDirection Direction { get; private set; }

    public FileSelectMapScroll(ISnesAddressSpace bus, AreaMapCartridgeData map,
        Bank80SystemState system, ushort playerMapX, ushort playerMapY)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(map);
        ArgumentNullException.ThrowIfNull(system);
        // Native selects one plane, not the union: a downloaded map uses its station
        // mask even if an explored secret cell lies outside that mask.
        bool Visible(int x, int y) => system.HasAreaMap(map.Area)
            ? map.IsRevealedByMapStation(x, y) : system.IsMapTileExplored(map.Area, x, y);
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
        MinimumX = Wrap(left * 8 - (map.Area == AreaId.Maridia ? 24 : 0));
        MaximumX = Wrap(right * 8);
        ushort nativeMinimumY = Wrap(top * 8);
        MaximumY = Wrap(bottom * 8);
        Horizontal = Wrap(MinimumX + Wrap(MaximumX - MinimumX) / 2 - 128);
        ushort screenX = Wrap(playerMapX - Horizontal);
        if (Signed(224 - screenX) < 0)
            Horizontal = Wrap(Horizontal - Signed(224 - screenX));
        else if (Signed(32 - screenX) >= 0)
            Horizontal = Wrap(Horizontal - Wrap(32 - screenX));
        ushort middle = Wrap(nativeMinimumY + Wrap(MaximumY - nativeMinimumY) / 2 + 16);
        ushort offset = Wrap((112 - middle) & ~7);
        Vertical = Wrap(-offset);
        short distance = Signed(64 - Wrap(playerMapY + offset));
        if (distance >= 0)
        {
            Vertical = Wrap(Vertical - distance);
            if (Signed(Vertical + 40) < 0) Vertical = Wrap(-40);
        }
        // $81:AD17 adjusts the upper arrow boundary only after positioning.
        MinimumY = Wrap(nativeMinimumY + 24);
        for (int index = 0; index < buttons.Length; index++)
        {
            int record = FileSelectMapRomData.ScrollArrows + index * 10;
            buttons[index] = RomDataReader.ReadWordFixedBank(bus, record + 6);
            ushort direction = RomDataReader.ReadWordFixedBank(bus, record + 8);
            if (direction != index + 1)
                throw new InvalidDataException("File-select map arrow direction table is inconsistent.");
        }
    }

    public bool CanScroll(MapScrollDirection direction) => direction switch
    {
        MapScrollDirection.Left => Signed(MinimumX - 24 - Horizontal) < 0,
        MapScrollDirection.Right => Signed(MaximumX - 232 - Horizontal) >= 0,
        MapScrollDirection.Up => Signed(MinimumY - 64 - Vertical) < 0,
        MapScrollDirection.Down => Signed(MaximumY - 145 - Vertical) >= 0,
        MapScrollDirection.None => false,
        _ => throw new ArgumentOutOfRangeException(nameof(direction)),
    };

    /// <summary>
    /// Processes held input in native arrow order. Returns true at the scroll-sound
    /// boundary. A released button does not cancel an already accepted eight-tick step.
    /// </summary>
    public bool Step(ushort heldInput)
    {
        for (int index = 0; index < buttons.Length; index++)
            if (Direction == MapScrollDirection.None && CanScroll((MapScrollDirection)(index + 1)) &&
                (heldInput & buttons[index]) != 0)
                Direction = (MapScrollDirection)(index + 1);
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
            if (Direction == MapScrollDirection.Left) Horizontal = Wrap(Horizontal - FileSelectMapRomData.ScrollStepPixels);
            if (Direction == MapScrollDirection.Right) Horizontal = Wrap(Horizontal + FileSelectMapRomData.ScrollStepPixels);
            if (Direction == MapScrollDirection.Up) Vertical = Wrap(Vertical - FileSelectMapRomData.ScrollStepPixels);
            if (Direction == MapScrollDirection.Down) Vertical = Wrap(Vertical + FileSelectMapRomData.ScrollStepPixels);
        }
        if (tick != FileSelectMapRomData.ScrollStepTicks) return false;
        tick = 0;
        Direction = MapScrollDirection.None;
        return true;
    }

    private static ushort Wrap(int value) => unchecked((ushort)value);
    private static short Signed(int value) => unchecked((short)value);
}
