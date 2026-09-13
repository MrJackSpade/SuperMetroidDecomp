using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.Core.Frontend;

/// <summary>Saved-station marker from $82:B6DD and animation timer from $82:B9FC.</summary>
public sealed class FileSelectStationMarker
{
    private int frame;
    private int timer;
    private ushort loops;

    public FileSelectStationMarker(ISnesAddressSpace bus, AreaId area, int stationIndex,
        SuperMetroid.Core.Assets.MapSaveMarkerLayout? layout = null)
        => BindPosition(bus, area, stationIndex, layout);

    /// <summary>Rebinds drawing position without changing the existing marker animation or selected load index.</summary>
    internal void BindPosition(ISnesAddressSpace bus, AreaId area, int stationIndex, SuperMetroid.Core.Assets.MapSaveMarkerLayout? layout)
    {
        ArgumentNullException.ThrowIfNull(bus);
        int areaIndex = AreaIds.ToIndex(area);
        if ((uint)areaIndex >= FileSelectMapRomData.AreaCount)
            throw new ArgumentOutOfRangeException(nameof(area));
        if ((uint)stationIndex >= MapSaveMarkerDefinitions.SlotsPerArea)
            throw new ArgumentOutOfRangeException(nameof(stationIndex));
        if (layout is not null)
        {
            var point = layout.Get(area, stationIndex);
            MapX = (ushort)point.X;
            MapY = (ushort)point.Y;
            return;
        }
        ushort list = RomDataReader.ReadWordFixedBank(bus,
            FileSelectMapRomData.SavePointMapPointers + areaIndex * 2);
        // Do not walk through the end sentinel into the next area's list when a bad
        // save selects a station that does not exist. Unused entries retain their index.
        for (int station = 0; station <= stationIndex; station++)
        {
            int address = FileSelectMapRomData.MenuObjectBank | unchecked((ushort)(list + station * 4));
            ushort x = RomDataReader.ReadWordFixedBank(bus, address);
            if (x == ushort.MaxValue || (station == stationIndex && x == ushort.MaxValue - 1))
                throw new InvalidDataException($"Area {area} has no map coordinate for station {stationIndex}.");
            if (station == stationIndex)
            {
                MapX = x;
                MapY = RomDataReader.ReadWordFixedBank(bus, address + 2);
            }
        }
    }

    public ushort MapX { get; private set; }
    public ushort MapY { get; private set; }
    public ushort SpritemapId => PauseMapIndicatorAnimation.SpritemapIds[frame];
    public bool ShowBacking => (loops & 1) == 0;

    /// <summary>One menu tick, before drawing; the initial zero timer advances immediately to frame one.</summary>
    public void Step()
    {
        if (timer == 0)
        {
            frame++;
            if (frame == PauseMapIndicatorAnimation.SpritemapIds.Length)
            {
                frame = 0;
                loops = unchecked((ushort)(loops + 1));
            }
            timer = PauseMapIndicatorAnimation.FrameDelays[frame];
        }
        timer--;
    }

    /// <summary>Appends the current marker without advancing time; repainting cannot speed up the animation.</summary>
    public void Draw(ISnesAddressSpace bus, OamBuffer oam, ushort horizontalScroll, ushort verticalScroll, SuperMetroid.Core.Assets.MapSpriteCatalog? sprites = null)
    {
        ushort x = unchecked((ushort)(MapX - horizontalScroll));
        ushort y = unchecked((ushort)(MapY - verticalScroll));
        if (ShowBacking) Add(FileSelectMapRomData.StationMarkerBacking);
        Add(SpritemapId);

        void Add(ushort id)
        {
            if (sprites is not null) { sprites.Draw(id, oam, x, y, FileSelectMapRomData.StationMarkerPalette); return; }
            ushort pointer = RomDataReader.ReadWordFixedBank(bus, MenuPpuState.SpritemapPointerTableAddress + id * 2);
            oam.AddOnScreenSpritemap(bus, FileSelectMapRomData.MenuObjectBank | pointer,
                x, y, FileSelectMapRomData.StationMarkerPalette);
        }
    }
}
