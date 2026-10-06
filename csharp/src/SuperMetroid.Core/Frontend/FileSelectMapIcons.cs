using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

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
    // Reuse the saved exploration owner already retained by these icons. Adding
    // another serialized owner to the menu would invalidate older debugger graphs.
    internal Bank80SystemState MapSystem => system;
    internal AreaId MapArea => area;

    /// <summary>Shared $82:B892 boss-marker drawing used by pause and file-select maps.</summary>
    public void DrawBossMarkers(OamBuffer oam, ushort scrollX, ushort scrollY)
    {
        var layout = landmarks ?? throw new InvalidOperationException(
            "Map boss markers require installed landmark layout.");
        int remainingBits = system.GetBossBitsRaw(area);
        foreach (string? id in MapLandmarkDefinitions.Bosses(area))
        {
            if (id is not null)
            {
                var point = layout.Get(id);
                bool dead = (remainingBits & 1) != 0;
                remainingBits >>= 1;
                if (dead)
                {
                    Draw(FileSelectMapIconRomData.DefeatedBoss, (ushort)point.X, (ushort)point.Y, FileSelectMapRomData.StationMarkerPalette);
                    Draw(FileSelectMapIconRomData.Boss, (ushort)point.X, (ushort)point.Y, FileSelectMapIconRomData.DefeatedBossPalette);
                    continue;
                }
                if (system.HasAreaMap(area))
                {
                    Draw(FileSelectMapIconRomData.Boss, (ushort)point.X, (ushort)point.Y, FileSelectMapRomData.StationMarkerPalette);
                    continue;
                }
            }
            remainingBits >>= 1;
        }
        void Draw(ushort id, ushort x, ushort y, ushort palette) =>
            Add(oam, id, x, y, scrollX, scrollY, palette);
    }

    /// <summary>Native $82:B6DD draws these objects before Samus's selected-station indicator.</summary>
    public void DrawBeforeMarker(OamBuffer oam, ushort scrollX, ushort scrollY)
    {
        DrawBossMarkers(oam, scrollX, scrollY);
        Simple(MapStationKind.Missile, FileSelectMapIconRomData.MissileLists, FileSelectMapIconRomData.Missile);
        Simple(MapStationKind.Energy, FileSelectMapIconRomData.EnergyLists, FileSelectMapIconRomData.Energy);
        Simple(MapStationKind.Map, FileSelectMapIconRomData.MapStationLists, FileSelectMapIconRomData.MapStation);

        void Simple(MapStationKind kind, int table, ushort id)
        {
            var layout = stations ?? throw new InvalidOperationException(
                "Map station icons require installed station layout.");
            foreach (var rule in MapStationDiscoveryRules.Get(area, kind))
                if (system.IsMapTileExplored(area, rule.CellX, rule.CellY))
                {
                    var point = layout.Get(rule.Id);
                    Draw(id, (ushort)point.X, (ushort)point.Y, FileSelectMapRomData.StationMarkerPalette);
                }
        }
        void Draw(ushort id, ushort x, ushort y, ushort palette) =>
            Add(oam, id, x, y, scrollX, scrollY, palette);
    }

    /// <summary>Normal file select adds the gunship and downloaded elevator labels, not debug save icons.</summary>
    public void DrawAfterMarker(OamBuffer oam, ushort scrollX, ushort scrollY, Action? drawArrows = null)
    {
        if (area == AreaId.Crateria)
        {
            var point = (landmarks ?? throw new InvalidOperationException(
                "Gunship icon requires installed landmark layout.")).Get(MapLandmarkDefinitions.Gunship);
            Add(oam, FileSelectMapIconRomData.Gunship, (ushort)point.X, (ushort)point.Y,
                scrollX, scrollY, FileSelectMapRomData.StationMarkerPalette);
        }
        drawArrows?.Invoke();
        DrawElevatorLabels(oam, scrollX, scrollY);
    }

    /// <summary>$82:BB30: destination lettering is visible only after downloading this area's map.</summary>
    public void DrawElevatorLabels(OamBuffer oam, ushort scrollX, ushort scrollY)
    {
        if (!system.HasAreaMap(area)) return;
        var elevatorLayout = landmarks ?? throw new InvalidOperationException(
            "Elevator labels require installed landmark layout.");
        foreach (var label in MapLandmarkDefinitions.Elevators(area))
        {
            var point = elevatorLayout.Get(label.Id);
            Add(oam, MapLandmarkDefinitions.ElevatorSpritemap(label.Destination),
                (ushort)point.X, (ushort)point.Y, scrollX, scrollY, 0);
        }
    }
    private void Add(OamBuffer oam, ushort id, ushort x, ushort y, ushort scrollX, ushort scrollY, ushort palette)
    {
        (sprites ?? throw new InvalidOperationException(
            "Map icons require installed sprite artwork."))
            .Draw(id, oam, unchecked((ushort)(x - scrollX)),
                unchecked((ushort)(y - scrollY)), palette);
    }
}
