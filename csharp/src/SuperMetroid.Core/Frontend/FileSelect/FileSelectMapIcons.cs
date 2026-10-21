using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>Non-debug map landmarks and elevator destinations from the cartridge icon lists.</summary>
public sealed class FileSelectMapIcons(Bank80SystemState system, AreaId area)
{
    /// <summary>Installed sprite artwork rebound when a frontend save state is restored.</summary>
    [NonSerialized] private SuperMetroid.Core.Frontend.Map.Artwork.MapSpriteCatalog? sprites;
    /// <summary>Binds the installed map-icon compositions used by all draw paths.</summary>
    internal void BindSprites(SuperMetroid.Core.Frontend.Map.Artwork.MapSpriteCatalog? catalog) => sprites = catalog;
    /// <summary>Installed station positions rebound after frontend restoration.</summary>
    [NonSerialized] private SuperMetroid.Core.Frontend.Map.Artwork.MapStationLayout? stations;
    /// <summary>Binds installed station positions for explored map tiles.</summary>
    internal void BindStations(SuperMetroid.Core.Frontend.Map.Artwork.MapStationLayout? layout) => stations = layout;
    /// <summary>Installed boss, gunship, and elevator landmark positions.</summary>
    [NonSerialized] private SuperMetroid.Core.Frontend.Map.Artwork.MapLandmarkLayout? landmarks;
    /// <summary>Binds landmark positions used for boss, gunship, and elevator icons.</summary>
    internal void BindLandmarks(SuperMetroid.Core.Frontend.Map.Artwork.MapLandmarkLayout? layout) => landmarks = layout;
    /// <summary>Exposes the retained exploration owner to the menu without duplicating its state.</summary>
    internal Bank80SystemState MapSystem => system;
    /// <summary>Area whose map exploration and icons this view draws.</summary>
    internal AreaId MapArea => area;

    /// <summary>Shared $82:B892 boss-marker drawing used by pause and file-select maps.</summary>
    public void DrawBossMarkers(ScreenSprites entries, Position16<MapPixels> scroll)
    {
        var layout = landmarks ?? throw new InvalidOperationException(
            "Map boss markers require installed landmark layout.");
        int remainingBits = system.GetBossBitsRaw(area);
        foreach (string? id in MapLandmarkDefinitions.Bosses(area))
        {
            if (id is not null)
            {
                Position16<MapPixels> point = layout.Get(id);
                bool dead = (remainingBits & 1) != 0;
                remainingBits >>= 1;
                if (dead)
                {
                    Draw(MapSpriteId.MarkerDefeatedBoss, point, FileSelectMapRomData.StationMarkerPalette);
                    Draw(MapSpriteId.MarkerBoss, point, FileSelectMapIconRomData.DefeatedBossPalette);
                    continue;
                }
                if (system.HasAreaMap(area))
                {
                    Draw(MapSpriteId.MarkerBoss, point, FileSelectMapRomData.StationMarkerPalette);
                    continue;
                }
            }
            remainingBits >>= 1;
        }
        void Draw(MapSpriteId id, Position16<MapPixels> position, ushort palette) =>
            Add(entries, id, position, scroll, palette);
    }

    /// <summary>Native $82:B6DD draws these objects before Samus's selected-station indicator.</summary>
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
                    Draw(id, layout.Get(rule.Id), FileSelectMapRomData.StationMarkerPalette);
        }
        void Draw(MapSpriteId id, Position16<MapPixels> position, ushort palette) =>
            Add(entries, id, position, scroll, palette);
    }

    /// <summary>Normal file select adds the gunship and downloaded elevator labels, not debug save icons.</summary>
    public void DrawAfterMarker(ScreenSprites entries, Position16<MapPixels> scroll, Action? drawArrows = null)
    {
        if (area == AreaId.Crateria)
        {
            var point = (landmarks ?? throw new InvalidOperationException(
                "Gunship icon requires installed landmark layout.")).Get(MapLandmarkDefinitions.Gunship);
            Add(entries, MapSpriteId.MarkerGunship, point, scroll, FileSelectMapRomData.StationMarkerPalette);
        }
        drawArrows?.Invoke();
        DrawElevatorLabels(entries, scroll);
    }

    /// <summary>$82:BB30: destination lettering is visible only after downloading this area's map.</summary>
    public void DrawElevatorLabels(ScreenSprites entries, Position16<MapPixels> scroll)
    {
        if (!system.HasAreaMap(area)) return;
        var elevatorLayout = landmarks ?? throw new InvalidOperationException(
            "Elevator labels require installed landmark layout.");
        foreach (var label in MapLandmarkDefinitions.Elevators(area))
        {
            Add(entries, MapLandmarkDefinitions.ElevatorSpritemap(label.Destination),
                elevatorLayout.Get(label.Id), scroll, 0);
        }
    }
    /// <summary>Converts a map-space landmark to OAM and draws it with installed sprite art.</summary>
    private void Add(ScreenSprites entries, MapSpriteId id, Position16<MapPixels> position, Position16<MapPixels> scroll, ushort palette)
    {
        (sprites ?? throw new InvalidOperationException(
            "Map icons require installed sprite artwork."))
            .Draw(id, entries, (position.ToScreen(scroll)).ToOam(), palette);
    }
}
