using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend;

/// <summary>Non-debug map landmarks and elevator destinations from the cartridge icon lists.</summary>
/// <param name="system">Saved exploration and boss state used to select visible map markers.</param>
/// <param name="area">Area whose map icons and discovered locations are drawn.</param>
public sealed class FileSelectMapIcons(Bank80SystemState system, AreaId area)
{
    /// <summary>Artwork catalog used to resolve map icon sprite IDs into OAM parts.</summary>
    [NonSerialized] private SuperMetroid.Core.Assets.MapSpriteCatalog? sprites;

    /// <summary>Installs the artwork catalog used by subsequent icon drawing.</summary>
    /// <param name="catalog">Catalog containing the installed map icon sprite artwork, or <see langword="null"/> when unavailable.</param>
    internal void BindSprites(SuperMetroid.Core.Assets.MapSpriteCatalog? catalog) => sprites = catalog;

    /// <summary>Room-layout coordinates for missile, energy, and map station icons.</summary>
    [NonSerialized] private SuperMetroid.Core.Assets.MapStationLayout? stations;

    /// <summary>Installs the station-coordinate layout used to position discovered station icons.</summary>
    /// <param name="layout">Coordinates for this area's map stations, or <see langword="null"/> when unavailable.</param>
    internal void BindStations(SuperMetroid.Core.Assets.MapStationLayout? layout) => stations = layout;

    /// <summary>Room-layout coordinates for bosses, the gunship, and elevator landmarks.</summary>
    [NonSerialized] private SuperMetroid.Core.Assets.MapLandmarkLayout? landmarks;

    /// <summary>Installs the landmark-coordinate layout used by boss, gunship, and elevator markers.</summary>
    /// <param name="layout">Coordinates for this area's landmarks, or <see langword="null"/> when unavailable.</param>
    internal void BindLandmarks(SuperMetroid.Core.Assets.MapLandmarkLayout? layout) => landmarks = layout;
    // Reuse the saved exploration owner already retained by these icons. Adding
    // another serialized owner to the menu would invalidate older debugger graphs.
    /// <summary>Gets the exploration and boss-state owner shared with pause-map rendering.</summary>
    internal Bank80SystemState MapSystem => system;

    /// <summary>Gets the area whose exploration state controls this icon set.</summary>
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
    /// <summary>Resolves a map sprite and appends its parts at scroll-adjusted screen coordinates.</summary>
    /// <param name="oam">OAM buffer receiving the sprite parts.</param>
    /// <param name="id">Catalog identifier of the map sprite.</param>
    /// <param name="x">World-space horizontal position of the icon.</param>
    /// <param name="y">World-space vertical position of the icon.</param>
    /// <param name="scrollX">Horizontal map scroll offset subtracted from the icon position.</param>
    /// <param name="scrollY">Vertical map scroll offset subtracted from the icon position.</param>
    /// <param name="palette">Palette bits supplied to each emitted OAM part.</param>
    private void Add(OamBuffer oam, ushort id, ushort x, ushort y, ushort scrollX, ushort scrollY, ushort palette)
    {
        (sprites ?? throw new InvalidOperationException(
            "Map icons require installed sprite artwork."))
            .Draw(id, oam, unchecked((ushort)(x - scrollX)),
                unchecked((ushort)(y - scrollY)), palette);
    }
}
