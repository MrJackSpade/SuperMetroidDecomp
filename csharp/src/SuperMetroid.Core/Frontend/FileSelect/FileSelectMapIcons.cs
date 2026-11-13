using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>Non-debug map landmarks and elevator destinations from the game's icon lists.</summary>
public sealed class FileSelectMapIcons(SharedGameState system, AreaId area)
{
    /// <summary>The installed map sprite sheet, rebound when a frontend save state is restored.</summary>
    private SpriteSheet? sheet;
    /// <summary>Binds the installed map sprite sheet every icon draws from.</summary>
    internal void BindSprites(SpriteSheet? mapSheet) => sheet = mapSheet;
    /// <summary>Installed station positions rebound after frontend restoration.</summary>
    private SuperMetroid.Core.Frontend.Map.Artwork.MapStationPositions? stations;
    /// <summary>Binds installed station positions for explored map tiles.</summary>
    internal void BindStations(SuperMetroid.Core.Frontend.Map.Artwork.MapStationPositions? layout) => stations = layout;
    /// <summary>Installed boss, gunship, and elevator landmark positions.</summary>
    private SuperMetroid.Core.Frontend.Map.Artwork.MapLandmarkPositions? landmarks;
    /// <summary>Binds landmark positions used for boss, gunship, and elevator icons.</summary>
    internal void BindLandmarks(SuperMetroid.Core.Frontend.Map.Artwork.MapLandmarkPositions? layout) => landmarks = layout;
    /// <summary>Exposes the retained exploration owner to the menu without duplicating its state.</summary>
    internal SharedGameState MapSystem => system;
    /// <summary>Area whose map exploration and icons this view draws.</summary>
    internal AreaId MapArea => area;

    /// <summary>Shared boss-marker drawing used by pause and file-select maps.</summary>
    public void DrawBossMarkers(ScreenSprites entries, Position16<MapPixels> scroll)
    {
        var layout = landmarks ?? throw new InvalidOperationException(
            "Map boss markers require installed landmark layout.");
        // Each listed marker consumes the next defeated-boss flag in marker order, and a marker that is neither defeated
        // nor shown by the area's map consumes one more; an empty entry consumes one.
        DefeatedBosses defeated = system.BossesDefeatedIn(area);
        int flag = 0;
        foreach (string? id in MapLandmarks.Bosses(area))
        {
            if (id is not null)
            {
                Position16<MapPixels> point = layout.Get(id);
                bool dead = flag < DefeatedBossKinds.InMarkerOrder.Count && defeated.HasFlag(DefeatedBossKinds.InMarkerOrder[flag]);
                flag++;
                if (dead)
                {
                    Draw(MapSpriteId.MarkerDefeatedBoss, point, MapSpriteColouring.Markers);
                    Draw(MapSpriteId.MarkerBoss, point, MapSpriteColouring.DefeatedBossMarker);
                    continue;
                }
                if (system.HasAreaMap(area))
                {
                    Draw(MapSpriteId.MarkerBoss, point, MapSpriteColouring.Markers);
                    continue;
                }
            }
            flag++;
        }
        void Draw(MapSpriteId id, Position16<MapPixels> position, MapSpriteColouring colouring) =>
            Add(entries, id, position, scroll, colouring);
    }

    /// <summary>The game draws these objects before Samus's selected-station indicator.</summary>
    public void DrawBeforeMarker(ScreenSprites entries, Position16<MapPixels> scroll)
    {
        DrawBossMarkers(entries, scroll);
        Simple(MapStationKind.Missile, MapSpriteId.StationMissile);
        Simple(MapStationKind.Energy, MapSpriteId.StationEnergy);
        Simple(MapStationKind.Map, MapSpriteId.StationMap);

        void Simple(MapStationKind kind, MapSpriteId id)
        {
            var layout = stations ?? throw new InvalidOperationException(
                "Map station icons require installed station layout.");
            foreach (var rule in MapStationDiscoveryRules.Get(area, kind))
                if (system.IsMapTileExplored(area, rule.Cell))
                    Draw(id, layout.Get(rule.Id), MapSpriteColouring.Markers);
        }
        void Draw(MapSpriteId id, Position16<MapPixels> position, MapSpriteColouring colouring) =>
            Add(entries, id, position, scroll, colouring);
    }

    /// <summary>Normal file select adds the gunship and downloaded elevator labels, not debug save icons.</summary>
    public void DrawAfterMarker(ScreenSprites entries, Position16<MapPixels> scroll, Action? drawArrows = null)
    {
        if (area == AreaId.Crateria)
        {
            var point = (landmarks ?? throw new InvalidOperationException(
                "Gunship icon requires installed landmark layout.")).Get(MapLandmarkDefinitions.Gunship);
            Add(entries, MapSpriteId.MarkerGunship, point, scroll, MapSpriteColouring.Markers);
        }
        drawArrows?.Invoke();
        DrawElevatorLabels(entries, scroll);
    }

    /// <summary>Destination lettering is visible only after downloading this area's map.</summary>
    public void DrawElevatorLabels(ScreenSprites entries, Position16<MapPixels> scroll)
    {
        if (!system.HasAreaMap(area)) return;
        var elevatorLayout = landmarks ?? throw new InvalidOperationException(
            "Elevator labels require installed landmark layout.");
        foreach (var label in MapLandmarks.Elevators(area))
        {
            Add(entries, MapLandmarks.ElevatorPicture(label.Destination),
                elevatorLayout.Get(label.Id), scroll, MapSpriteColouring.Labels);
        }
    }
    /// <summary>Converts a map-space landmark to screen space and draws it with installed sprite art.</summary>
    private void Add(ScreenSprites entries, MapSpriteId id, Position16<MapPixels> position, Position16<MapPixels> scroll,
        MapSpriteColouring colouring)
    {
        MapSpriteSheet.Draw(sheet ?? throw new InvalidOperationException("Map icons require the installed map sprite sheet."),
            id, entries, position.ToScreen(scroll).ToSpritePosition(), MapSpriteScreen.FileSelect, colouring);
    }
}
