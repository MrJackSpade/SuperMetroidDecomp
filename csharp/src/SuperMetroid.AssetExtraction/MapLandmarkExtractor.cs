using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Imports only landmark positions; validates mixed-record identities against compiled definitions.</summary>
public static class MapLandmarkExtractor
{
    /// <summary>Exports named boss, elevator-destination-label, and Crateria gunship drawing positions from native area-map records.</summary>
    /// <param name="bus">Import-capable cartridge source for bank-$82 boss/elevator lists and the Crateria save-point position list.</param>
    /// <returns>A new UTF-8 JSON buffer mapping compiled landmark identities to area-map pixel anchors before scroll subtraction.</returns>
    /// <remarks>Verifies expected boss slots, unused-slot sentinels, list terminators, and elevator destination spritemaps; discovery, defeated-boss state, and elevator connectivity are not editable output.</remarks>
    /// <exception cref="ArgumentException"><paramref name="bus"/> does not provide cartridge import access, including null.</exception>
    /// <exception cref="InvalidDataException">Required records, unused slots, destination bindings, or list boundaries differ from compiled definitions.</exception>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        var points = new Dictionary<string, MapLabelPoint>();
        foreach (AreaId area in Enum.GetValues<AreaId>())
        {
            int boss = Pointer(FileSelectMapIconRomData.BossLists, area);
            var bosses = MapLandmarkDefinitions.Bosses(area);
            if (boss == 0 && !bosses.IsEmpty) throw new InvalidDataException($"Missing {area} boss list.");
            for (int i = 0; i < bosses.Length; i++)
            {
                ushort x = Read(boss + i * 4);
                if (bosses[i] is string id)
                {
                    if (x >= ushort.MaxValue - 1) throw new InvalidDataException($"Missing map boss {id}.");
                    points.Add(id, new(x, Read(boss + i * 4 + 2)));
                }
                else if (x != ushort.MaxValue - 1) throw new InvalidDataException($"Expected unused {area} boss slot {i}.");
            }
            if (boss != 0 && Read(boss + bosses.Length * 4) != ushort.MaxValue)
                throw new InvalidDataException($"Unexpected {area} boss slots.");
            if (area == AreaId.Ceres) continue;
            int elevator = Pointer(FileSelectMapIconRomData.ElevatorLists, area);
            var elevators = MapLandmarkDefinitions.Elevators(area);
            for (int i = 0; i < elevators.Length; i++)
            {
                var label = elevators[i];
                if (Read(elevator + i * 6 + 4) != MapLandmarkDefinitions.ElevatorSpritemap(label.Destination))
                    throw new InvalidDataException($"Elevator label destination differs from cartridge: {label.Id}.");
                points.Add(label.Id, new(Read(elevator + i * 6), Read(elevator + i * 6 + 2)));
            }
            if (Read(elevator + elevators.Length * 6) != ushort.MaxValue)
                throw new InvalidDataException($"Unexpected {area} elevator labels.");
        }
        int ship = Pointer(FileSelectMapRomData.SavePointMapPointers, AreaId.Crateria);
        points.Add(MapLandmarkDefinitions.Gunship, new(Read(ship), Read(ship + 2)));
        using var stream = new MemoryStream();
        MapLandmarkLayout.Write(stream, new() { Version = MapLandmarkFormat.Version, Markers = points });
        return stream.ToArray();

        ushort Pointer(int table, AreaId area) => RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), table + AreaIds.ToIndex(area) * 2);
        ushort Read(int pointer) => RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), FileSelectMapRomData.MenuObjectBank | pointer);
    }
}
