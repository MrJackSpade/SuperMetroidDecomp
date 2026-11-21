using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>The file select map's landmarks and elevator destinations from the game's icon lists, as the game draws them without its debug mode.</summary>
/// <param name="system">The shared game state whose downloaded maps, explored tiles and defeated bosses the icons show.</param>
/// <param name="area">The area whose icons the map draws.</param>
/// <param name="stationLayout">The installed station positions for explored map tiles.</param>
/// <param name="landmarkLayout">The installed boss, gunship and elevator landmark positions.</param>
/// <param name="mapSheet">The installed map sprite sheet every icon draws from.</param>
public sealed class FileSelectMapIcons(SharedGameState system, AreaId area, MapStationPositions stationLayout,
    MapLandmarkPositions landmarkLayout, SpriteSheet mapSheet)
{
    /// <summary>The installed map sprite sheet, rebound when a frontend save state is restored.</summary>
    private SpriteSheet sheet = Ensure.NotNull(mapSheet);
    /// <summary>Binds the installed map sprite sheet every icon draws from.</summary>
    internal void BindSprites(SpriteSheet value) => sheet = Ensure.NotNull(value);
    /// <summary>Installed station positions rebound after frontend restoration.</summary>
    private MapStationPositions stations = Ensure.NotNull(stationLayout);
    /// <summary>Binds installed station positions for explored map tiles.</summary>
    internal void BindStations(MapStationPositions layout) => stations = Ensure.NotNull(layout);
    /// <summary>Installed boss, gunship, and elevator landmark positions.</summary>
    private MapLandmarkPositions landmarks = Ensure.NotNull(landmarkLayout);
    /// <summary>Binds landmark positions used for boss, gunship, and elevator icons.</summary>
    internal void BindLandmarks(MapLandmarkPositions layout) => landmarks = Ensure.NotNull(layout);
    /// <summary>Exposes the retained exploration owner to the menu without duplicating its state.</summary>
    internal SharedGameState MapSystem => system;
    /// <summary>Area whose map exploration and icons this view draws.</summary>
    internal AreaId MapArea => area;

    /// <summary>Shared boss-marker drawing used by pause and file-select maps.</summary>
    public void DrawBossMarkers(ScreenSprites entries, Offset16<MapPixels> scroll)
    {
        var layout = landmarks;
        // Each marker shows the boss it names: defeated once the area records that boss, alive while the map is known.
        DefeatedBosses defeated = system.BossesDefeatedIn(area);
        foreach (MapBossMarker marker in MapLandmarks.BossMarkers(area))
        {
            Position16<MapPixels> point = layout.Get(marker.Id);
            if (defeated.HasFlag(AreaBossDefinitions.Bosses[marker.Boss].RecordedAs))
            {
                Draw(MapSpriteId.MarkerDefeatedBoss, point, MapSpriteColouring.Markers);
                Draw(MapSpriteId.MarkerBoss, point, MapSpriteColouring.DefeatedBossMarker);
            }
            else if (system.HasAreaMap(area))
            {
                Draw(MapSpriteId.MarkerBoss, point, MapSpriteColouring.Markers);
            }
        }
        void Draw(MapSpriteId id, Position16<MapPixels> position, MapSpriteColouring colouring) =>
            Add(entries, id, position, scroll, colouring);
    }

    /// <summary>The game draws these objects before Samus's selected-station indicator.</summary>
    public void DrawBeforeMarker(ScreenSprites entries, Offset16<MapPixels> scroll)
    {
        DrawBossMarkers(entries, scroll);
        Simple(MapStationKind.Missile, MapSpriteId.StationMissile);
        Simple(MapStationKind.Energy, MapSpriteId.StationEnergy);
        Simple(MapStationKind.Map, MapSpriteId.StationMap);

        void Simple(MapStationKind kind, MapSpriteId id)
        {
            var layout = stations;
            foreach (var rule in MapStationDiscoveryRules.Get(area, kind))
                if (system.IsMapTileExplored(area, rule.Cell))
                    Draw(id, layout.Get(rule.Id), MapSpriteColouring.Markers);
        }
        void Draw(MapSpriteId id, Position16<MapPixels> position, MapSpriteColouring colouring) =>
            Add(entries, id, position, scroll, colouring);
    }

    /// <summary>Draws what follows the marker: the gunship in Crateria, the arrows, and the elevator labels once the area's map is downloaded; never the debug-only save icons the game's debug mode adds.</summary>
    public void DrawAfterMarker(ScreenSprites entries, Offset16<MapPixels> scroll, Action? drawArrows = null)
    {
        if (MapLandmarks.ShowsGunship(area))
        {
            var point = landmarks.Get(MapLandmarkDefinitions.Gunship);
            Add(entries, MapSpriteId.MarkerGunship, point, scroll, MapSpriteColouring.Markers);
        }
        drawArrows?.Invoke();
        DrawElevatorLabels(entries, scroll);
    }

    /// <summary>Destination lettering is visible only after downloading this area's map.</summary>
    public void DrawElevatorLabels(ScreenSprites entries, Offset16<MapPixels> scroll)
    {
        // Ceres has no elevators to label.
        if (!AreaIds.IsZebes(area, out ZebesArea zebes) || !system.HasAreaMap(area)) return;
        var elevatorLayout = landmarks;
        foreach (var label in MapLandmarks.Elevators(zebes))
        {
            Add(entries, MapLandmarks.ElevatorPicture(label.Destination),
                elevatorLayout.Get(label.Id), scroll, MapSpriteColouring.Labels);
        }
    }
    /// <summary>Converts a map-space landmark to screen space and draws it with installed sprite art.</summary>
    private void Add(ScreenSprites entries, MapSpriteId id, Position16<MapPixels> position, Offset16<MapPixels> scroll,
        MapSpriteColouring colouring)
    {
        MapSpriteSheet.Draw(sheet,
            id, entries, position.RelativeToScroll(scroll), MapSpriteScreen.FileSelect, colouring);
    }
}
