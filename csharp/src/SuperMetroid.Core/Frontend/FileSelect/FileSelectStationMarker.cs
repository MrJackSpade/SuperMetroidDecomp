using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>Saved-station marker and its pulse.</summary>
public sealed class FileSelectStationMarker
{
    /// <summary>The marker's pulse (<see cref="PauseMapIndicatorAnimation"/>).</summary>
    private readonly MapIndicatorPulse pulse = new();

    /// <summary>Binds a saved station's drawing anchor and starts a fresh marker pulse; this object neither chooses the gameplay load target nor initializes map scrolling.</summary>
    /// <param name="station">The saved station; it must have a map marker.</param>
    /// <param name="layout">The installed save-marker layout; its independently edited X/Y coordinates affect drawing only.</param>
    /// <remarks>The pulse starts on its smallest picture; the first <see cref="Step"/> shows its middle one.</remarks>
    public FileSelectStationMarker(LoadStation station,
        SuperMetroid.Core.Frontend.Map.Artwork.MapSaveMarkerPositions layout)
        => BindPosition(station, Ensure.NotNull(layout));

    /// <summary>Rebinds drawing position without changing the existing marker animation or selected station.</summary>
    internal void BindPosition(LoadStation station, SuperMetroid.Core.Frontend.Map.Artwork.MapSaveMarkerPositions layout)
    {
        MapPosition = Ensure.NotNull(layout).Get(station);
    }

    /// <summary>Station's area-map drawing anchor in whole pixels before scroll subtraction, independent of its load coordinates.</summary>
    public Position16<MapPixels> MapPosition { get; private set; }
    /// <summary>The marker's picture, following its pulse; querying does not advance it.</summary>
    public MapSpriteId SpriteId => pulse.Sprite;
    /// <summary>Whether the marker's backing frame is drawn before the marker: shown through the first pulse, then hidden and shown again on alternate pulses.</summary>
    public bool ShowBacking { get; private set; } = true;

    /// <summary>One menu update of the pulse, before drawing; each completed pulse shows or hides the backing.</summary>
    public void Step() => pulse.Step(_ => ShowBacking = !ShowBacking);

    /// <summary>Appends the current marker without advancing time; repainting cannot speed up the animation.</summary>
    public void Draw(ScreenSprites entries, Offset16<MapPixels> scroll, SpriteSheet sheet)
    {
        Ensure.NotNull(sheet);
        Offset16<ScreenPixels> origin = MapPosition.RelativeToScroll(scroll);
        // The backing is drawn behind the station marker on even animation loops.
        if (ShowBacking) Add(MapSpriteId.IndicatorBacking);
        Add(SpriteId);

        void Add(MapSpriteId id)
        {
            MapSpriteSheet.Draw(sheet, id, entries, origin, MapSpriteScreen.FileSelect, MapSpriteColouring.Markers);
        }
    }
}
