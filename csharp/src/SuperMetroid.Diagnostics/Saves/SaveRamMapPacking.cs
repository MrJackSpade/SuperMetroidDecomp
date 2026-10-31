using SuperMetroid.AssetExtraction.Progression;
using SuperMetroid.Core;
using SuperMetroid.Core.Map;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Diagnostics.Saves;

/// <summary>
/// The packed explored-map field of a legacy save image (#1322, #1475): bank $81's <c>SaveMap</c> and <c>LoadMap</c> copy
/// each Zebes area's explored-map plane byte by byte through a list of the bytes the area's stock map occupies, in the
/// plane's page-first order, the areas one after another; Ceres is not packed. The lists are calculated from stock map
/// occupancy (<see cref="ExploredMapStorage.IsKept"/>), so installed artwork overrides never move a byte.
/// </summary>
[EncodingOwner("legacy save image packed explored-map field")]
public static class SaveRamMapPacking
{
    /// <summary>The Zebes areas whose explored maps the field packs, in packing order.</summary>
    public const int PackedAreaCount = 6;

    /// <summary>Map cells one plane byte holds.</summary>
    private const int CellsPerByte = 8;

    /// <summary>Plane bytes per row of one page.</summary>
    private const int BytesPerPageRow = AreaMapLayout.PageWidthInTiles / CellsPerByte;

    /// <summary>
    /// Where <paramref name="area"/>'s bytes start in the packed field, the sum of the earlier areas' byte counts, and the
    /// plane bytes it packs, in packing order.
    /// </summary>
    /// <param name="area">The packed area's index, 0 through 5.</param>
    /// <param name="maps">Installed stock map rules.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="area"/> is not a packed area.</exception>
    public static SaveRamPackedArea Area(int area, IAreaMapSource maps)
    {
        if ((uint)area >= PackedAreaCount)
            throw new ArgumentOutOfRangeException(nameof(area), area, "Only the six Zebes areas' explored maps are packed.");
        Ensure.NotNull(maps);
        int offset = 0;
        for (int previous = 0; previous < area; previous++)
            offset += OccupiedBytes(maps.Get(AreaIds.At(previous))).Count;
        return new SaveRamPackedArea(offset, OccupiedBytes(maps.Get(AreaIds.At(area))));
    }

    /// <summary>The packed field holding <paramref name="progress"/>'s Zebes exploration.</summary>
    /// <exception cref="InvalidDataException">The packed lists escape the field.</exception>
    public static byte[] Pack(PersistentProgress progress, IAreaMapSource maps)
    {
        Ensure.NotNull(progress);
        var field = new byte[SaveRamLayout.CompressedMapDataByteCount];
        var plane = new byte[ProgressionTableCodec.ExploredPlaneByteCount];
        for (int area = 0; area < PackedAreaCount; area++)
        {
            SaveRamPackedArea packed = Area(area, maps);
            ProgressionTableCodec.WriteExploredPlane(progress, AreaIds.At(area), plane);
            for (int index = 0; index < packed.PlaneBytes.Count; index++)
                field[FieldIndex(packed, index)] = plane[packed.PlaneBytes[index]];
        }
        return field;
    }

    /// <summary>Records the exploration <paramref name="field"/> packs into <paramref name="progress"/>; Ceres is left unexplored.</summary>
    /// <exception cref="ArgumentException">The field is not the packed field's size.</exception>
    /// <exception cref="InvalidDataException">The packed lists escape the field.</exception>
    public static void Unpack(ReadOnlySpan<byte> field, IAreaMapSource maps, PersistentProgress progress)
    {
        Ensure.LengthEqual(field, SaveRamLayout.CompressedMapDataByteCount);
        Ensure.NotNull(progress);
        var plane = new byte[ProgressionTableCodec.ExploredPlaneByteCount];
        for (int area = 0; area < AreaIds.RetailCount; area++)
        {
            Array.Clear(plane);
            if (area < PackedAreaCount)
            {
                SaveRamPackedArea packed = Area(area, maps);
                for (int index = 0; index < packed.PlaneBytes.Count; index++)
                    plane[packed.PlaneBytes[index]] = field[FieldIndex(packed, index)];
            }
            ProgressionTableCodec.ReadExploredPlane(plane, AreaIds.At(area), progress);
        }
    }

    /// <summary>The field index of a packed area's <paramref name="index"/>th byte.</summary>
    private static int FieldIndex(SaveRamPackedArea packed, int index)
    {
        int fieldIndex = packed.SaveOffset + index;
        return fieldIndex < SaveRamLayout.CompressedMapDataByteCount
            ? fieldIndex
            : throw new InvalidDataException("The packed explored-map lists escape the saved map field.");
    }

    /// <summary>The plane bytes the stock map occupies, in the plane's page-first order.</summary>
    private static List<int> OccupiedBytes(IAreaMapView stockRules)
    {
        var occupied = new List<int>();
        for (int planeByte = 0; planeByte < ProgressionTableCodec.ExploredPlaneByteCount; planeByte++)
        {
            if (ExploredMapStorage.IsKept(stockRules, FirstCell(planeByte)))
                occupied.Add(planeByte);
        }
        return occupied;
    }

    /// <summary>The first, leftmost cell a plane byte holds.</summary>
    private static Position16<MapTiles> FirstCell(int planeByte)
    {
        int page = planeByte / AreaMapLayout.BitPlaneBytesPerPage;
        int local = planeByte % AreaMapLayout.BitPlaneBytesPerPage;
        int x = page * AreaMapLayout.PageWidthInTiles + local % BytesPerPageRow * CellsPerByte;
        return new Position16<MapTiles>(checked((ushort)x), checked((ushort)(local / BytesPerPageRow)));
    }
}
