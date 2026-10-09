using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Imports drawing coordinates while verifying the native save-index validity map.</summary>
public static class MapSaveMarkerExtractor
{
    /// <summary>Exports drawing anchors for all usable area/save-index slots while checking the native unused-slot validity pattern.</summary>
    /// <param name="bus">Import-capable cartridge source for bank-$82 save-point map pointer tables and four-byte X/Y records.</param>
    /// <returns>A new UTF-8 JSON buffer keyed by compiled area/slot identities, with area-map pixel positions before scroll subtraction.</returns>
    /// <remarks>Usable slots must have real coordinates; unusable slots must retain the $FFFE X sentinel. Output does not alter which slots are valid or any saved-state/load placement.</remarks>
    /// <exception cref="ArgumentException"><paramref name="bus"/> does not provide cartridge import access, including null.</exception>
    /// <exception cref="InvalidDataException">A usable marker is missing or an unused slot differs from its expected sentinel.</exception>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        var points = new Dictionary<string, MapLabelPoint>();
        for (int area = 0; area < FileSelectMapRomData.AreaCount; area++)
        {
            var typedArea = (AreaId)area;
            int pointer = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), FileSelectMapRomData.SavePointMapPointers + area * 2);
            for (int index = 0; index < MapSaveMarkerDefinitions.SlotsPerArea; index++)
            {
                int address = FileSelectMapRomData.MenuObjectBank | (pointer + index * 4);
                ushort x = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address);
                bool usable = MapSaveMarkerDefinitions.IsUsable(typedArea, index);
                if (usable)
                {
                    if (x >= ushort.MaxValue - 1) throw new InvalidDataException($"Missing save marker {typedArea}/{index}.");
                    points.Add(MapSaveMarkerDefinitions.Id(typedArea, index), new(x, RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address + 2)));
                }
                else if (x != ushort.MaxValue - 1) throw new InvalidDataException($"Expected unused save marker {typedArea}/{index}.");
            }
        }
        using var stream = new MemoryStream();
        MapSaveMarkerLayout.Write(stream, new() { Version = MapSaveMarkerFormat.Version, Markers = points });
        return stream.ToArray();
    }
}
