using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Imports drawing coordinates while verifying the native save-index validity map.</summary>
public static class MapSaveMarkerExtractor
{
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        var points = new Dictionary<string, MapLabelPoint>();
        for (int area = 0; area < FileSelectMapRomData.AreaCount; area++)
        {
            var typedArea = (AreaId)area;
            int pointer = RomDataReader.ReadWordFixedBank(bus, FileSelectMapRomData.SavePointMapPointers + area * 2);
            for (int index = 0; index < MapSaveMarkerDefinitions.SlotsPerArea; index++)
            {
                int address = FileSelectMapRomData.MenuObjectBank | (pointer + index * 4);
                ushort x = RomDataReader.ReadWordFixedBank(bus, address);
                bool usable = MapSaveMarkerDefinitions.Indices(typedArea).Contains(index);
                if (usable)
                {
                    if (x >= ushort.MaxValue - 1) throw new InvalidDataException($"Missing save marker {typedArea}/{index}.");
                    points.Add(MapSaveMarkerDefinitions.Id(typedArea, index), new(x, RomDataReader.ReadWordFixedBank(bus, address + 2)));
                }
                else if (x != ushort.MaxValue - 1) throw new InvalidDataException($"Expected unused save marker {typedArea}/{index}.");
            }
        }
        using var stream = new MemoryStream();
        MapSaveMarkerLayout.Write(stream, new() { Version = MapSaveMarkerFormat.Version, Markers = points });
        return stream.ToArray();
    }
}
