using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

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
        var point = (layout ?? throw new InvalidOperationException(
            "Save marker requires installed station positions."))
            .Get(area, stationIndex);
        MapX = (ushort)point.X;
        MapY = (ushort)point.Y;
    }

    public ushort MapX { get; private set; }
    public ushort MapY { get; private set; }
    public ushort SpritemapId => PauseMapIndicatorAnimation.SpritemapId(frame);
    public bool ShowBacking => (loops & 1) == 0;

    /// <summary>One menu tick, before drawing; the initial zero timer advances immediately to frame one.</summary>
    public void Step()
    {
        if (timer == 0)
        {
            frame++;
            if (frame == PauseMapIndicatorAnimation.FrameCount)
            {
                frame = 0;
                loops = unchecked((ushort)(loops + 1));
            }
            timer = PauseMapIndicatorAnimation.FrameDelay(frame);
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
            (sprites ?? throw new InvalidOperationException(
                "Save marker requires installed sprite artwork."))
                .Draw(id, oam, x, y, FileSelectMapRomData.StationMarkerPalette);
        }
    }
}
