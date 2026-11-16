using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Progression;
using SuperMetroid.AssetExtraction.Frontend.Map.Artwork;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Saves;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>Imports drawing coordinates while verifying the native save-index validity map.</summary>
public static class MapSaveMarkerExtractor
{
    /// <summary>The marker records each Zebes area's list holds, one per number its station list can give a station.</summary>
    private const int SlotsPerArea = 16;

    /// <summary>Exports drawing anchors for all usable area/save-index slots while checking the native unused-slot validity pattern.</summary>
    /// <param name="bus">Import-capable cartridge source for bank-$82 save-point map pointer tables and four-byte X/Y records.</param>
    /// <returns>A new UTF-8 JSON buffer keyed by marked station names, with area-map pixel positions before scroll subtraction.</returns>
    /// <remarks>The record of a marked station's number must have real coordinates; every other record must retain the $FFFE X sentinel. Output does not alter which stations are marked or any saved-state/load placement.</remarks>
    /// <exception cref="ArgumentException"><paramref name="bus"/> does not provide cartridge import access, including null.</exception>
    /// <exception cref="InvalidDataException">A usable marker is missing or an unused slot differs from its expected sentinel.</exception>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        var points = new Dictionary<string, Position16<MapPixels>>();
        foreach (AreaId typedArea in AreaIds.Zebes)
        {
            int pointer = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), FrontendSourceDefinitions.SavePointMapPointers.AddWithinBank(AreaNumberCodec.Encode(typedArea) * 2));
            for (int index = 0; index < SlotsPerArea; index++)
            {
                int address = MapSourceDefinitions.Menu.ObjectBank | (pointer + index * 4);
                int x = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address);
                if (LoadStationNumberCodec.Find(typedArea, index) is { } station && MapSaveMarkers.IsMarked(station))
                {
                    if (x >= ushort.MaxValue - 1) throw new InvalidDataException($"Missing save marker {station}.");
                    points.Add(MapSaveMarkers.Id(station), Position16<MapPixels>.Of(x, RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address + 2)));
                }
                else if (x != ushort.MaxValue - 1) throw new InvalidDataException($"Expected unused save marker {typedArea}/{index}.");
            }
        }
        using var stream = new MemoryStream();
        MapSaveMarkerFile.Write(stream, new() { Version = MapSaveMarkerFormat.Version, Markers = points });
        return stream.ToArray();
    }
}
