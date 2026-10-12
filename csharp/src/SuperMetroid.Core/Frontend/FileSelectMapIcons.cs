using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Frontend;

/// <summary>Non-debug map landmarks and elevator destinations from the cartridge icon lists.</summary>
public sealed class FileSelectMapIcons(Bank80SystemState system, AreaId area)
{
    [NonSerialized] private SuperMetroid.Core.Assets.MapSpriteCatalog? sprites;
    internal void BindSprites(SuperMetroid.Core.Assets.MapSpriteCatalog? catalog) => sprites = catalog;
    [NonSerialized] private SuperMetroid.Core.Assets.MapStationLayout? stations;
    internal void BindStations(SuperMetroid.Core.Assets.MapStationLayout? layout) => stations = layout;
    [NonSerialized] private SuperMetroid.Core.Assets.MapLandmarkLayout? landmarks;
    internal void BindLandmarks(SuperMetroid.Core.Assets.MapLandmarkLayout? layout) => landmarks = layout;
    // The exploration owner these icons already retain; the menu reads it rather than holding a second copy.
    internal Bank80SystemState MapSystem => system;
    internal AreaId MapArea => area;

    /// <summary>Shared $82:B892 boss-marker drawing used by pause and file-select maps.</summary>
    public void DrawBossMarkers(OamBuffer oam, Position16<MapPixels> scroll)
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
            Add(oam, id, position, scroll, palette);
    }

    /// <summary>Native $82:B6DD draws these objects before Samus's selected-station indicator.</summary>
    public void DrawBeforeMarker(OamBuffer oam, Position16<MapPixels> scroll)
    {
        DrawBossMarkers(oam, scroll);
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
            Add(oam, id, position, scroll, palette);
    }

    /// <summary>Normal file select adds the gunship and downloaded elevator labels, not debug save icons.</summary>
    public void DrawAfterMarker(OamBuffer oam, Position16<MapPixels> scroll, Action? drawArrows = null)
    {
        if (area == AreaId.Crateria)
        {
            var point = (landmarks ?? throw new InvalidOperationException(
                "Gunship icon requires installed landmark layout.")).Get(MapLandmarkDefinitions.Gunship);
            Add(oam, MapSpriteId.MarkerGunship, point, scroll, FileSelectMapRomData.StationMarkerPalette);
        }
        drawArrows?.Invoke();
        DrawElevatorLabels(oam, scroll);
    }

    /// <summary>$82:BB30: destination lettering is visible only after downloading this area's map.</summary>
    public void DrawElevatorLabels(OamBuffer oam, Position16<MapPixels> scroll)
    {
        if (!system.HasAreaMap(area)) return;
        var elevatorLayout = landmarks ?? throw new InvalidOperationException(
            "Elevator labels require installed landmark layout.");
        foreach (var label in MapLandmarkDefinitions.Elevators(area))
        {
            Add(oam, MapLandmarkDefinitions.ElevatorSpritemap(label.Destination),
                elevatorLayout.Get(label.Id), scroll, 0);
        }
    }
    private void Add(OamBuffer oam, MapSpriteId id, Position16<MapPixels> position, Position16<MapPixels> scroll, ushort palette)
    {
        (sprites ?? throw new InvalidOperationException(
            "Map icons require installed sprite artwork."))
            .Draw(id, oam, (position.ToScreen(scroll)).ToOam(), palette);
    }
}
