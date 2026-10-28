using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Progression;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>Splits station drawing positions from the cartridge's mixed drawing/discovery records.</summary>
public static class MapStationLayoutExtractor
{
    /// <summary>Exports every missile, energy, and map station's drawing anchor while checking its native area-map discovery cell and record count.</summary>
    /// <param name="bus">Import-capable cartridge source for bank-$82 per-area station-list pointers and mixed coordinate/discovery records.</param>
    /// <returns>A new UTF-8 JSON buffer mapping compiled station identities to area-map pixel X/Y positions before scroll subtraction.</returns>
    /// <remarks>Each native coordinate divided by eight must match the corresponding compiled discovery-cell coordinate. Drawing positions are exported; station kind, discovery rules, and list membership remain compiled.</remarks>
    /// <exception cref="ArgumentException"><paramref name="bus"/> does not provide cartridge import access, including null.</exception>
    /// <exception cref="InvalidDataException">Station records are missing/extra, discovery cells differ, or the generated layout fails format validation.</exception>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        var markers = new Dictionary<string, Position16<MapPixels>>();
        foreach (AreaId area in Enum.GetValues<AreaId>())
        foreach (MapStationKind kind in Enum.GetValues<MapStationKind>())
        {
            int table = kind switch
            {
                MapStationKind.Missile => MapSourceDefinitions.Landmarks.MissileLists,
                MapStationKind.Energy => MapSourceDefinitions.Landmarks.EnergyLists,
                MapStationKind.Map => MapSourceDefinitions.Landmarks.MapStationLists,
                _ => throw new ArgumentOutOfRangeException(nameof(kind))
            };
            ushort pointer = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), table + AreaIds.ToIndex(area) * 2);
            var rules = MapStationDiscoveryRules.Get(area, kind).ToArray();
            int count = 0;
            if (pointer != 0)
                for (; count <= rules.Length; count++)
                {
                    int address = MapSourceDefinitions.Menu.ObjectBank | (pointer + count * 4);
                    ushort x = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address);
                    if ((short)x < 0) break;
                    if (count == rules.Length) throw new InvalidDataException($"Unexpected {area}/{kind} station record.");
                    ushort y = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address + 2);
                    var rule = rules[count];
                    if (new Position16<MapTiles>(checked((ushort)(x >> 3)), checked((ushort)(y >> 3))) != rule.Cell)
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
