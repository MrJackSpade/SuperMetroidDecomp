using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>Saved-station marker and its animation timer.</summary>
public sealed class FileSelectStationMarker
{
    /// <summary>The marker's step in its pulse (<see cref="PauseMapIndicatorAnimation.Pulse"/>).</summary>
    private int frame;
    /// <summary>Remaining menu ticks in the current animation phase.</summary>
    private int timer;

    /// <summary>Binds a saved station's drawing anchor and starts a fresh indicator animation; this object neither chooses the gameplay load target nor initializes map scrolling.</summary>
    /// <param name="station">The saved station; it must have a map marker.</param>
    /// <param name="layout">Required installed save-marker layout; its independently edited X/Y coordinates affect drawing only, despite the optional parameter syntax.</param>
    /// <remarks>The frame and timer begin at zero. The first <see cref="Step"/> advances to frame one; subsequent frame holds use the 8/4/8/4 menu-update sequence.</remarks>
    public FileSelectStationMarker(LoadStation station,
        SuperMetroid.Core.Frontend.Map.Artwork.MapSaveMarkerPositions? layout = null)
        => BindPosition(station, layout);

    /// <summary>Rebinds drawing position without changing the existing marker animation or selected station.</summary>
    internal void BindPosition(LoadStation station, SuperMetroid.Core.Frontend.Map.Artwork.MapSaveMarkerPositions? layout)
    {
        MapPosition = (layout ?? throw new InvalidOperationException(
            "Save marker requires installed station positions."))
            .Get(station);
    }

    /// <summary>Station's area-map drawing anchor in whole pixels before scroll subtraction, independent of its compiled load coordinates.</summary>
    public Position16<MapPixels> MapPosition { get; private set; }
    /// <summary>Current marker frame identity, following its pulse; querying does not advance the timer.</summary>
    public MapSpriteId SpriteId => PauseMapIndicatorAnimation.Pulse[frame].Sprite;
    /// <summary>Whether the marker's backing frame is drawn before the marker: visible on even four-frame animation loops, including the initial loop, and hidden on odd loops.</summary>
    public bool ShowBacking { get; private set; } = true;

    /// <summary>One menu tick, before drawing; the initial zero timer advances immediately to frame one.</summary>
    public void Step()
    {
        if (timer == 0)
        {
            frame++;
            if (frame == PauseMapIndicatorAnimation.Pulse.Count)
            {
                frame = 0;
                // Only the parity of completed pulses is observable, so each pulse toggles the backing.
                ShowBacking = !ShowBacking;
            }
            timer = PauseMapIndicatorAnimation.Pulse[frame].Duration;
        }
        timer--;
    }

    /// <summary>Appends the current marker without advancing time; repainting cannot speed up the animation.</summary>
    public void Draw(ScreenSprites entries, Position16<MapPixels> scroll, SpriteSheet? sheet = null)
    {
        Position16<ScreenPixels> origin = MapPosition.ToScreen(scroll);
        // The backing is drawn behind the station marker on even animation loops.
        if (ShowBacking) Add(MapSpriteId.IndicatorBacking);
        Add(SpriteId);

        void Add(MapSpriteId id)
        {
            MapSpriteSheet.Draw(sheet ?? throw new InvalidOperationException("Save marker requires the installed map sprite sheet."),
                id, entries, origin.ToSpritePosition(), MapSpriteScreen.FileSelect, MapSpriteColouring.Markers);
        }
    }
}
