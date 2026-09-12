using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Splits station drawing positions from the cartridge's mixed drawing/discovery records.</summary>
public static class MapStationLayoutExtractor
{
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        var markers = new Dictionary<string, MapLabelPoint>();
        foreach (AreaId area in Enum.GetValues<AreaId>())
        foreach (MapStationKind kind in Enum.GetValues<MapStationKind>())
        {
            int table = kind switch
            {
                MapStationKind.Missile => FileSelectMapIconRomData.MissileLists,
                MapStationKind.Energy => FileSelectMapIconRomData.EnergyLists,
                MapStationKind.Map => FileSelectMapIconRomData.MapStationLists,
                _ => throw new ArgumentOutOfRangeException(nameof(kind))
            };
            ushort pointer = RomDataReader.ReadWordFixedBank(bus, table + AreaIds.ToIndex(area) * 2);
            var rules = MapStationDiscoveryRules.Get(area, kind).ToArray();
            int count = 0;
            if (pointer != 0)
                for (; count <= rules.Length; count++)
                {
                    int address = FileSelectMapRomData.MenuObjectBank | (pointer + count * 4);
                    ushort x = RomDataReader.ReadWordFixedBank(bus, address);
                    if ((short)x < 0) break;
                    if (count == rules.Length) throw new InvalidDataException($"Unexpected {area}/{kind} station record.");
                    ushort y = RomDataReader.ReadWordFixedBank(bus, address + 2);
                    var rule = rules[count];
                    if ((x >> 3) != rule.CellX || (y >> 3) != rule.CellY)
                        throw new InvalidDataException($"Station discovery definition differs from cartridge: {rule.Id}.");
                    markers.Add(rule.Id, new(x, y));
                }
            if (count != rules.Length) throw new InvalidDataException($"Missing {area}/{kind} station records.");
        }
        using var stream = new MemoryStream();
        MapStationLayout.Write(stream, new() { Version = MapStationLayoutFormat.Version, Markers = markers });
        return stream.ToArray();
    }
}
