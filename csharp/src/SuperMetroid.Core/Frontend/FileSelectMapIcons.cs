using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.Core.Frontend;

/// <summary>Non-debug map landmarks and elevator destinations from the cartridge icon lists.</summary>
public sealed class FileSelectMapIcons(ISnesAddressSpace bus, Bank80SystemState system, AreaId area)
{
    /// <summary>Native $82:B6DD draws these objects before Samus's selected-station indicator.</summary>
    public void DrawBeforeMarker(OamBuffer oam, ushort scrollX, ushort scrollY)
    {
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
        Simple(FileSelectMapIconRomData.MissileLists, FileSelectMapIconRomData.Missile);
        Simple(FileSelectMapIconRomData.EnergyLists, FileSelectMapIconRomData.Energy);
        Simple(FileSelectMapIconRomData.MapStationLists, FileSelectMapIconRomData.MapStation);

        void Simple(int table, ushort id)
        {
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
            ushort list = Pointer(FileSelectMapRomData.SavePointMapPointers);
            Add(oam, FileSelectMapIconRomData.Gunship, Read(list, 0), Read(list, 2),
                scrollX, scrollY, FileSelectMapRomData.StationMarkerPalette);
        }
        drawArrows?.Invoke();
        if (!system.HasAreaMap(area)) return;
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
        ushort pointer = RomDataReader.ReadWordFixedBank(bus, MenuPpuState.SpritemapPointerTableAddress + id * 2);
        oam.AddOnScreenSpritemap(bus, FileSelectMapRomData.MenuObjectBank | pointer,
            unchecked((ushort)(x - scrollX)), unchecked((ushort)(y - scrollY)), palette);
    }
}
