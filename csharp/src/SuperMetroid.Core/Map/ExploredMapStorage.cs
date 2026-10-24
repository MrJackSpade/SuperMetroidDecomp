using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Map;

/// <summary>
/// Which explored-map bytes a save keeps (#1322, #1421): each area's bytes a stock map tile occupies, packed into one
/// fixed field, so exploration of cells no stock tile occupies is lost when the game is saved.
/// </summary>
public static class ExploredMapStorage
{
    /// <summary>The areas whose explored maps a save keeps.</summary>
    public const int PackedAreaCount = 6;

    /// <summary>The bytes of the packed field that keeps them.</summary>
    public const int PackedByteCount = 1280;

    /// <summary>Packs each area's occupied explored-map bytes into the saved field.</summary>
    public static byte[] Pack(ReadOnlySpan<byte> exploredMap, IAreaMapSource? maps)
    {
        int expectedByteCount = Bank80SystemState.ExploredMapAreaCount * Bank80SystemState.ExploredMapBytesPerArea;
        if (exploredMap.Length != expectedByteCount)
            throw new InvalidDataException($"A save snapshot requires exactly {expectedByteCount} unpacked explored-map bytes.");

        var compressed = new byte[PackedByteCount];
        for (int area = 0; area < PackedAreaCount; area++)
        {
            ExploredMapPackingDefinition definition = ExploredMapPackingDefinitions.Area(area, RequireMaps(maps));
            ExploredMapPackingDefinitions.OccupiedByteIndexes areaByteIndexes = definition.AreaByteIndexes;
            for (int index = 0; index < areaByteIndexes.Count; index++)
            {
                int compressedIndex = definition.SaveOffset + index;
                if ((uint)compressedIndex >= compressed.Length)
                    throw new InvalidDataException("Compiled packed-map table escapes the saved map field.");
                compressed[compressedIndex] = exploredMap[area * Bank80SystemState.ExploredMapBytesPerArea + areaByteIndexes[index]];
            }
        }
        return compressed;
    }

    /// <summary>Expands the saved map field into area-major explored-map bytes using stock map rules.</summary>
    public static byte[] Unpack(ReadOnlySpan<byte> compressed, IAreaMapSource? maps)
    {
        Ensure.LengthEqual(compressed, PackedByteCount);

        var explored = new byte[Bank80SystemState.ExploredMapAreaCount * Bank80SystemState.ExploredMapBytesPerArea];
        for (int area = 0; area < PackedAreaCount; area++)
        {
            ExploredMapPackingDefinition definition = ExploredMapPackingDefinitions.Area(area, RequireMaps(maps));
            ExploredMapPackingDefinitions.OccupiedByteIndexes areaByteIndexes = definition.AreaByteIndexes;
            for (int index = 0; index < areaByteIndexes.Count; index++)
            {
                int compressedIndex = definition.SaveOffset + index;
                if ((uint)compressedIndex >= compressed.Length)
                    throw new InvalidDataException("Compiled packed-map table escapes the saved map field.");
                explored[area * Bank80SystemState.ExploredMapBytesPerArea + areaByteIndexes[index]] = compressed[compressedIndex];
            }
        }
        return explored;
    }

    /// <summary>The explored-map bytes a save of <paramref name="exploredMap"/> reads back.</summary>
    internal static byte[] Retained(ReadOnlySpan<byte> exploredMap, IAreaMapSource? maps) => Unpack(Pack(exploredMap, maps), maps);

    /// <summary>The bound map rules; packing fails loudly without them.</summary>
    private static IAreaMapSource RequireMaps(IAreaMapSource? maps) =>
        maps ?? throw new InvalidOperationException("Save-map packing requires bound immutable stock map rules.");
}
