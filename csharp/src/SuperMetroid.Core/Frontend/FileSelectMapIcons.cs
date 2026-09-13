using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.Core.Frontend;

/// <summary>Non-debug map landmarks and elevator destinations from the cartridge icon lists.</summary>
public sealed class FileSelectMapIcons(ISnesAddressSpace bus, Bank80SystemState system, AreaId area)
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
        if (landmarks is not null)
        {
            int remainingBits = system.GetBossBitsRaw(area);
            foreach (string? id in MapLandmarkDefinitions.Bosses(area))
            {
                if (id is not null)
                {
                    var point = landmarks.Get(id);
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
            return;
        }
        ushort pointer = Pointer(FileSelectMapIconRomData.BossLists);
        int bits = system.GetBossBitsRaw(area);
        if (pointer != 0)
        {
            for (int record = 0; ; record++)
            {
                ushort x = Read(pointer, record * 4);
                if (x == ushort.MaxValue) break;
                if (x != ushort.MaxValue - 1)
                {
                    ushort y = Read(pointer, record * 4 + 2);
                    bool dead = (bits & 1) != 0;
                    bits >>= 1;
                    if (dead)
                    {
                        Draw(FileSelectMapIconRomData.DefeatedBoss, x, y, FileSelectMapRomData.StationMarkerPalette);
                        Draw(FileSelectMapIconRomData.Boss, x, y, FileSelectMapIconRomData.DefeatedBossPalette);
                        continue;
                    }
                    if (system.HasAreaMap(area))
                    {
                        Draw(FileSelectMapIconRomData.Boss, x, y, FileSelectMapRomData.StationMarkerPalette);
                        continue;
                    }
                }
                // Native's undrawn/not-downloaded path falls through to a second shift;
                // an unused coordinate consumes one bit, not a visible boss record.
                bits >>= 1;
            }
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
            if (stations is not null)
            {
                foreach (var rule in MapStationDiscoveryRules.Get(area, kind))
                    if (system.IsMapTileExplored(area, rule.CellX, rule.CellY))
                    {
                        var point = stations.Get(rule.Id);
                        Draw(id, (ushort)point.X, (ushort)point.Y, FileSelectMapRomData.StationMarkerPalette);
                    }
                return;
            }
            // Cartridge-fed diagnostic control; installed hosts bind presentation above.
            ushort list = Pointer(table);
            if (list == 0) return;
            for (int record = 0; ; record++)
            {
                ushort x = Read(list, record * 4);
                if ((short)x < 0) break;
                ushort y = Read(list, record * 4 + 2);
                if (record < 16 && system.IsMapTileExplored(area, x >> 3, y >> 3))
                    Draw(id, x, y, FileSelectMapRomData.StationMarkerPalette);
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
            var point = landmarks?.Get(MapLandmarkDefinitions.Gunship);
            ushort list = point is null ? Pointer(FileSelectMapRomData.SavePointMapPointers) : (ushort)0;
            Add(oam, FileSelectMapIconRomData.Gunship, point is null ? Read(list, 0) : (ushort)point.X, point is null ? Read(list, 2) : (ushort)point.Y,
                scrollX, scrollY, FileSelectMapRomData.StationMarkerPalette);
        }
        drawArrows?.Invoke();
        if (!system.HasAreaMap(area)) return;
        if (landmarks is not null)
        {
            foreach (var label in MapLandmarkDefinitions.Elevators(area))
            {
                var point = landmarks.Get(label.Id);
                Add(oam, MapLandmarkDefinitions.ElevatorSpritemap(label.Destination), (ushort)point.X, (ushort)point.Y, scrollX, scrollY, 0);
            }
            return;
        }
        ushort pointer = Pointer(FileSelectMapIconRomData.ElevatorLists);
        for (int record = 0; ; record++)
        {
            ushort x = Read(pointer, record * 6);
            if (x == ushort.MaxValue) break;
            Add(oam, Read(pointer, record * 6 + 4), x, Read(pointer, record * 6 + 2), scrollX, scrollY, 0);
        }
    }

    private ushort Pointer(int table) => RomDataReader.ReadWordFixedBank(bus, table + AreaIds.ToIndex(area) * 2);
    private ushort Read(ushort list, int offset)
    {
        if (offset >= 65536) throw new InvalidDataException("Map icon list wrapped its bank without a terminator.");
        return RomDataReader.ReadWordFixedBank(bus, FileSelectMapRomData.MenuObjectBank | unchecked((ushort)(list + offset)));
    }
    private void Add(OamBuffer oam, ushort id, ushort x, ushort y, ushort scrollX, ushort scrollY, ushort palette)
    {
        if (sprites is not null)
        {
            sprites.Draw(id, oam, unchecked((ushort)(x - scrollX)), unchecked((ushort)(y - scrollY)), palette);
            return;
        }
        ushort pointer = RomDataReader.ReadWordFixedBank(bus, MenuPpuState.SpritemapPointerTableAddress + id * 2);
        oam.AddOnScreenSpritemap(bus, FileSelectMapRomData.MenuObjectBank | pointer,
            unchecked((ushort)(x - scrollX)), unchecked((ushort)(y - scrollY)), palette);
    }
}
