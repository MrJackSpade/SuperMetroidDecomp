using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Imports only landmark positions; validates mixed-record identities against compiled definitions.</summary>
public static class MapLandmarkExtractor
{
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

        ushort Pointer(int table, AreaId area) => RomDataReader.ReadWordFixedBank(bus, table + AreaIds.ToIndex(area) * 2);
        ushort Read(int pointer) => RomDataReader.ReadWordFixedBank(bus, FileSelectMapRomData.MenuObjectBank | pointer);
    }
}
