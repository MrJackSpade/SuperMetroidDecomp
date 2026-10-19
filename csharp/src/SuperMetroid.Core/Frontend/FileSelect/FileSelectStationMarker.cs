using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>Saved-station marker from $82:B6DD and animation timer from $82:B9FC.</summary>
public sealed class FileSelectStationMarker
{
    /// <summary>Current phase of the four-frame station indicator pulse.</summary>
    private int frame;
    /// <summary>Remaining menu ticks in the current animation phase.</summary>
    private int timer;

    /// <summary>Binds a saved station's drawing anchor and starts a fresh $82:B9FC indicator animation; this object neither chooses the gameplay load target nor initializes map scrolling.</summary>
    /// <param name="area">One of the six Zebes map areas; Ceres is unsupported.</param>
    /// <param name="stationIndex">Native load-station slot 0..15, not an ordinal among usable stations; the slot must have a supported marker anchor.</param>
    /// <param name="layout">Required installed save-marker layout; its independently edited X/Y coordinates affect drawing only, despite the optional parameter syntax.</param>
    /// <remarks>The frame and timer begin at zero. The first <see cref="Step"/> advances to frame one; subsequent frame holds use the native 8/4/8/4 menu-update sequence.</remarks>
    public FileSelectStationMarker(AreaId area, int stationIndex,
        SuperMetroid.Core.Frontend.Map.Artwork.MapSaveMarkerLayout? layout = null)
        => BindPosition(area, stationIndex, layout);

    /// <summary>Rebinds drawing position without changing the existing marker animation or selected load index.</summary>
    internal void BindPosition(AreaId area, int stationIndex, SuperMetroid.Core.Frontend.Map.Artwork.MapSaveMarkerLayout? layout)
    {
        int areaIndex = AreaIds.ToIndex(area);
        if ((uint)areaIndex >= FileSelectMapRomData.AreaCount)
            throw new ArgumentOutOfRangeException(nameof(area));
        if ((uint)stationIndex >= MapSaveMarkerDefinitions.SlotsPerArea)
            throw new ArgumentOutOfRangeException(nameof(stationIndex));
        MapPosition = (layout ?? throw new InvalidOperationException(
            "Save marker requires installed station positions."))
            .Get(area, stationIndex);
    }

    /// <summary>Station's area-map drawing anchor in whole pixels before scroll subtraction, independent of its compiled load coordinates.</summary>
    public Position16<MapPixels> MapPosition { get; private set; }
    /// <summary>Current native $82:C569 spritemap-table identity, following $82:BA2D's $5F/$60/$61/$60 pulse; querying does not advance the timer.</summary>
    public MapSpriteId SpritemapId => PauseMapIndicatorAnimation.SpritemapId(frame);
    /// <summary>Whether $82:B73C-$B747's backing spritemap $12 is drawn before the marker: visible on even four-frame animation loops, including the initial loop, and hidden on odd loops.</summary>
    public bool ShowBacking { get; private set; } = true;

    /// <summary>One menu tick, before drawing; the initial zero timer advances immediately to frame one.</summary>
    public void Step()
    {
        if (timer == 0)
        {
            frame++;
            if (frame == PauseMapIndicatorAnimation.FrameCount)
            {
                frame = 0;
                // Only the parity of completed pulses is observable, so each pulse toggles the backing.
                ShowBacking = !ShowBacking;
            }
            timer = PauseMapIndicatorAnimation.FrameDelay(frame);
        }
        timer--;
    }

    /// <summary>Appends the current marker without advancing time; repainting cannot speed up the animation.</summary>
    public void Draw(ScreenSprites entries, Position16<MapPixels> scroll, SuperMetroid.Core.Frontend.Map.Artwork.MapSpriteCatalog? sprites = null)
    {
        Position16<ScreenPixels> origin = MapPosition.ToScreen(scroll);
        // $82:B6DD draws the backing behind the station marker on even animation loops.
        if (ShowBacking) Add(MapSpriteId.IndicatorBacking);
        Add(SpritemapId);

        void Add(MapSpriteId id)
        {
            (sprites ?? throw new InvalidOperationException(
                "Save marker requires installed sprite artwork."))
                .Draw(id, entries, origin.ToOam(), FileSelectMapRomData.StationMarkerPalette);
        }
    }
}
