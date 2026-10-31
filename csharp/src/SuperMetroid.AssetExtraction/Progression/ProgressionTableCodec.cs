using SuperMetroid.Core;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Progression;

/// <summary>
/// The cartridge's stored form of persistent progress (#1475): the event, boss, Chozo-orb, item, door, station and
/// map-station tables of its save mirror, and each area's explored-map plane. The import pipeline reads SpaceTime's copy
/// through it, and the hosts' legacy save-image codec and the checks' snapshot imports read and write saves through it;
/// nothing in the game knows this form.
/// </summary>
/// <remarks>
/// Bit tables store flag <c>n</c> in bit <c>n % 8</c> of byte <c>n / 8</c>. The boss table holds a byte per area slot,
/// eight slots for seven areas; the station table holds a little-endian word per area slot, station <c>n</c> in its bit
/// <c>n</c>; the map-station table holds a nonzero byte for each area whose map is acquired, then padding. Reading keeps
/// what the game reads: the eighth area slot, boss bits no routine tests and map-station padding hold nothing the game
/// can observe and are not kept, and writing leaves them clear. An explored-map plane stores an area's 64-by-32 cells as
/// two 32-column pages, most significant bit first.
/// </remarks>
[EncodingOwner("cartridge persistent progress tables")]
public static class ProgressionTableCodec
{
    /// <summary>Bytes of the story-event table.</summary>
    public const int EventByteCount = PersistentProgress.EventCount / BitsPerByte;

    /// <summary>Bytes of the area-boss table: one per area slot.</summary>
    public const int AreaBossByteCount = 8;

    /// <summary>Bytes of each Chozo-orb, item and door table.</summary>
    public const int RoomFlagByteCount = PersistentProgress.RoomFlagCount / BitsPerByte;

    /// <summary>Bytes of the used-station table: a word per area slot.</summary>
    public const int UsedStationByteCount = AreaBossByteCount * sizeof(ushort);

    /// <summary>Bytes of the map-station table, including its padding.</summary>
    public const int MapStationByteCount = 12;

    /// <summary>Bytes of one area's explored-map plane.</summary>
    public const int ExploredPlaneByteCount = AreaMapLayout.BitPlaneBytesPerPage * (AreaMapLayout.WidthInTiles / AreaMapLayout.PageWidthInTiles);

    /// <summary>The value the game stores for an acquired area map.</summary>
    private const byte AcquiredMap = byte.MaxValue;

    /// <summary>Flags one table byte holds.</summary>
    private const int BitsPerByte = 8;

    /// <summary>The proven boss flags in the order of their bits, lowest first.</summary>
    private static readonly BossBits[] BossFlagBits = [BossBits.AreaBoss, BossBits.AreaMiniBoss, BossBits.AreaTorizo];

    /// <summary>Turns on exactly the flags <paramref name="table"/> stores.</summary>
    /// <exception cref="ArgumentException">The table's size does not match the flags' range.</exception>
    public static void ReadFlags(ReadOnlySpan<byte> table, NumberedFlagSet flags)
    {
        Ensure.NotNull(flags);
        Ensure.LengthEqual(table, flags.Capacity / BitsPerByte);
        for (int number = 0; number < flags.Capacity; number++)
            flags.Set(number, IsBitSet(table, number));
    }

    /// <summary>Stores <paramref name="flags"/> in <paramref name="table"/>.</summary>
    /// <exception cref="ArgumentException">The table's size does not match the flags' range.</exception>
    public static void WriteFlags(NumberedFlagSet flags, Span<byte> table)
    {
        Ensure.NotNull(flags);
        Ensure.LengthEqual(table, flags.Capacity / BitsPerByte);
        table.Clear();
        foreach (int number in flags.Members)
            table[number / BitsPerByte] |= (byte)(1 << (number % BitsPerByte));
    }

    /// <summary>What writing <paramref name="copied"/> over a bit table's leading bytes decides.</summary>
    public static FlagOverwrite PrefixOverwrite(ReadOnlySpan<byte> copied)
    {
        var set = new List<int>();
        for (int number = 0; number < copied.Length * BitsPerByte; number++)
        {
            if (IsBitSet(copied, number))
                set.Add(number);
        }
        return new FlagOverwrite(copied.Length * BitsPerByte, set);
    }

    /// <summary>Each area's defeated bosses as the area-boss table stores them.</summary>
    /// <exception cref="ArgumentException">The table is not eight bytes.</exception>
    public static void ReadAreaBosses(ReadOnlySpan<byte> table, PersistentProgress progress)
    {
        Ensure.NotNull(progress);
        Ensure.LengthEqual(table, AreaBossByteCount);
        foreach (KeyValuePair<AreaId, BossBits> area in AreaBossesPrefix(table))
            progress.SetDefeatedBosses(area.Key, area.Value);
    }

    /// <summary>Stores each area's defeated bosses in <paramref name="table"/>.</summary>
    /// <exception cref="ArgumentException">The table is not eight bytes.</exception>
    public static void WriteAreaBosses(PersistentProgress progress, Span<byte> table)
    {
        Ensure.NotNull(progress);
        Ensure.LengthEqual(table, AreaBossByteCount);
        table.Clear();
        for (int index = 0; index < AreaIds.RetailCount; index++)
        {
            BossBits bosses = progress.DefeatedBosses(AreaIds.At(index));
            for (int bit = 0; bit < BossFlagBits.Length; bit++)
            {
                if (bosses.HasFlag(BossFlagBits[bit]))
                    table[index] |= (byte)(1 << bit);
            }
        }
    }

    /// <summary>The defeated bosses of each area <paramref name="copied"/>, the area-boss table's leading bytes, covers.</summary>
    public static IReadOnlyDictionary<AreaId, BossBits> AreaBossesPrefix(ReadOnlySpan<byte> copied)
    {
        var areas = new Dictionary<AreaId, BossBits>();
        for (int index = 0; index < Math.Min(copied.Length, AreaIds.RetailCount); index++)
        {
            BossBits bosses = BossBits.None;
            for (int bit = 0; bit < BossFlagBits.Length; bit++)
            {
                if (IsBitSet(copied.Slice(index, 1), bit))
                    bosses |= BossFlagBits[bit];
            }
            areas.Add(AreaIds.At(index), bosses);
        }
        return areas;
    }

    /// <summary>Each area's used stations as the used-station table stores them.</summary>
    /// <exception cref="ArgumentException">The table is not sixteen bytes.</exception>
    public static void ReadUsedStations(ReadOnlySpan<byte> table, PersistentProgress progress)
    {
        Ensure.NotNull(progress);
        Ensure.LengthEqual(table, UsedStationByteCount);
        foreach (KeyValuePair<AreaId, FlagOverwrite> area in UsedStationsPrefix(table))
            progress.UsedStations(area.Key).Overwrite(area.Value);
    }

    /// <summary>Stores each area's used stations in <paramref name="table"/>.</summary>
    /// <exception cref="ArgumentException">The table is not sixteen bytes.</exception>
    public static void WriteUsedStations(PersistentProgress progress, Span<byte> table)
    {
        Ensure.NotNull(progress);
        Ensure.LengthEqual(table, UsedStationByteCount);
        table.Clear();
        for (int index = 0; index < AreaIds.RetailCount; index++)
        {
            foreach (int station in progress.UsedStations(AreaIds.At(index)).Members)
                table[index * sizeof(ushort) + station / BitsPerByte] |= (byte)(1 << (station % BitsPerByte));
        }
    }

    /// <summary>
    /// The used stations of each area <paramref name="copied"/>, the used-station table's leading bytes, covers: an
    /// area's first eight stations from the low byte of its word, the rest from the high byte.
    /// </summary>
    public static IReadOnlyDictionary<AreaId, FlagOverwrite> UsedStationsPrefix(ReadOnlySpan<byte> copied)
    {
        var areas = new Dictionary<AreaId, FlagOverwrite>();
        for (int index = 0; index < AreaIds.RetailCount && index * sizeof(ushort) < copied.Length; index++)
        {
            int start = index * sizeof(ushort);
            areas.Add(AreaIds.At(index), PrefixOverwrite(copied.Slice(start, Math.Min(sizeof(ushort), copied.Length - start))));
        }
        return areas;
    }

    /// <summary>Each area's acquired map as the map-station table stores them.</summary>
    /// <exception cref="ArgumentException">The table is not twelve bytes.</exception>
    public static void ReadAreaMaps(ReadOnlySpan<byte> table, PersistentProgress progress)
    {
        Ensure.NotNull(progress);
        Ensure.LengthEqual(table, MapStationByteCount);
        foreach (KeyValuePair<AreaId, bool> area in AreaMapsPrefix(table))
            progress.SetAreaMap(area.Key, area.Value);
    }

    /// <summary>Stores each area's acquired map in <paramref name="table"/>.</summary>
    /// <exception cref="ArgumentException">The table is not twelve bytes.</exception>
    public static void WriteAreaMaps(PersistentProgress progress, Span<byte> table)
    {
        Ensure.NotNull(progress);
        Ensure.LengthEqual(table, MapStationByteCount);
        table.Clear();
        for (int index = 0; index < AreaIds.RetailCount; index++)
        {
            if (progress.HasAreaMap(AreaIds.At(index)))
                table[index] = AcquiredMap;
        }
    }

    /// <summary>Whether each area <paramref name="copied"/>, the map-station table's leading bytes, covers has its map acquired.</summary>
    public static IReadOnlyDictionary<AreaId, bool> AreaMapsPrefix(ReadOnlySpan<byte> copied)
    {
        var areas = new Dictionary<AreaId, bool>();
        for (int index = 0; index < Math.Min(copied.Length, AreaIds.RetailCount); index++)
            areas.Add(AreaIds.At(index), copied[index] != 0);
        return areas;
    }

    /// <summary>Records exactly the cells <paramref name="plane"/> stores as <paramref name="area"/>'s explored cells.</summary>
    /// <exception cref="ArgumentException">The plane is not 256 bytes.</exception>
    public static void ReadExploredPlane(ReadOnlySpan<byte> plane, AreaId area, PersistentProgress progress)
    {
        Ensure.NotNull(progress);
        Ensure.LengthEqual(plane, ExploredPlaneByteCount);
        for (int y = 0; y < AreaMapLayout.HeightInTiles; y++)
        {
            for (int x = 0; x < AreaMapLayout.WidthInTiles; x++)
            {
                var cell = new Position16<MapTiles>(checked((ushort)x), checked((ushort)y));
                progress.SetExplored(area, cell, (plane[AreaMapPages.GetBitByteIndex(cell)] & AreaMapPages.GetBitMask(x)) != 0);
            }
        }
    }

    /// <summary>Stores <paramref name="area"/>'s explored cells in <paramref name="plane"/>.</summary>
    /// <exception cref="ArgumentException">The plane is not 256 bytes.</exception>
    public static void WriteExploredPlane(PersistentProgress progress, AreaId area, Span<byte> plane)
    {
        Ensure.NotNull(progress);
        Ensure.LengthEqual(plane, ExploredPlaneByteCount);
        plane.Clear();
        foreach (Position16<MapTiles> cell in progress.ExploredCells(area))
            plane[AreaMapPages.GetBitByteIndex(cell)] |= AreaMapPages.GetBitMask(cell.X);
    }

    /// <summary>Whether flag <paramref name="number"/> of a bit table is set.</summary>
    private static bool IsBitSet(ReadOnlySpan<byte> table, int number) =>
        (table[number / BitsPerByte] & (1 << (number % BitsPerByte))) != 0;
}
