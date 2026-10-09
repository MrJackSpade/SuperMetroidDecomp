using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend;

/// <summary>Saved-station marker from $82:B6DD and animation timer from $82:B9FC.</summary>
public sealed class FileSelectStationMarker
{
    private int frame;
    private int timer;
    private ushort loops;

    /// <summary>Binds a saved station's drawing anchor and starts a fresh $82:B9FC indicator animation; this object neither chooses the gameplay load target nor initializes map scrolling.</summary>
    /// <param name="bus">Required address-space context retained by the API contract; marker positions are resolved from the installed layout rather than read from cartridge memory.</param>
    /// <param name="area">One of the six Zebes map areas; Ceres is unsupported.</param>
    /// <param name="stationIndex">Native load-station slot 0..15, not an ordinal among usable stations; the slot must have a supported marker anchor.</param>
    /// <param name="layout">Required installed save-marker layout; its independently edited X/Y coordinates affect drawing only, despite the optional parameter syntax.</param>
    /// <remarks>The frame and timer begin at zero. The first <see cref="Step"/> advances to frame one; subsequent frame holds use the native 8/4/8/4 menu-update sequence.</remarks>
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

    /// <summary>Station's area-map X drawing anchor in whole pixels before horizontal scroll subtraction, independent of its compiled load coordinates.</summary>
    public ushort MapX { get; private set; }
    /// <summary>Station's area-map Y drawing anchor in whole pixels before vertical scroll subtraction, independent of its compiled load coordinates.</summary>
    public ushort MapY { get; private set; }
    /// <summary>Current native $82:C569 spritemap-table identity, following $82:BA2D's $5F/$60/$61/$60 pulse; querying does not advance the timer.</summary>
    public ushort SpritemapId => PauseMapIndicatorAnimation.SpritemapId(frame);
    /// <summary>Whether $82:B73C-$B747's backing spritemap $12 is drawn before the marker: visible on even four-frame animation loops, including the initial loop, and hidden on odd loops.</summary>
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
    public void Draw(OamBuffer oam, ushort horizontalScroll, ushort verticalScroll, SuperMetroid.Core.Assets.MapSpriteCatalog? sprites = null)
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
