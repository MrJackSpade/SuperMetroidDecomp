using SuperMetroid.Core;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Saves;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Progression;

/// <summary>
/// The cartridge's stored form of persistent progress (#1475): the event, boss, Chozo-orb, item, door, station and
/// map-station tables of its save mirror, and each area's explored-map plane. Story events, doors and item locations
/// are numbered through <see cref="StoryEventNumbers"/> and <see cref="RoomPlacementNumbers"/>. The import pipeline
/// decodes SpaceTime's copy through it into the story events, item locations, doors and stations it decides, which the
/// installation holds by name (#1709); the hosts' legacy save-image codec and the checks' snapshot imports read and write
/// saves through it. Neither the game nor the installation it loads knows this form.
/// </summary>
/// <remarks>
/// Bit tables store flag <c>n</c> in bit <c>n % 8</c> of byte <c>n / 8</c>. The boss table holds a byte per area slot,
/// eight slots for seven areas; the station table holds a little-endian word per area slot, station <c>n</c> in its bit
/// <c>n</c>; the map-station table holds a nonzero byte for each area whose map is acquired, then padding. Reading keeps
/// what the game reads: the eighth area slot, boss bits no routine tests and map-station padding hold nothing the game
/// can observe and are not kept, and writing leaves them clear; so do the event table's bits no story event uses
/// (<see cref="StoryEventNumbers"/>), the door and item tables' bits no retail door or item location has, and the whole
/// Chozo-orb table, which only unused room objects write and nothing reads. An explored-map plane stores an area's 64-by-32 cells as
/// two 32-column pages, most significant bit first. The event table keeps the Zebetite count as a three-bit counter in
/// the bits of events three, four and five, lowest bit first (#1527); reading takes those bits out of the story events
/// into <see cref="PersistentProgress.ZebetitesDestroyed"/>, and writing puts the count back into them.
/// </remarks>
[EncodingOwner("cartridge persistent progress tables")]
public static class ProgressionTableCodec
{
    /// <summary>Bytes of the story-event table.</summary>
    public const int EventByteCount = StoryEventNumbers.NumberCount / BitsPerByte;

    /// <summary>Bytes of the area-boss table: one per area slot.</summary>
    public const int AreaBossByteCount = 8;

    /// <summary>Bytes of each Chozo-orb, item and door table.</summary>
    public const int RoomFlagByteCount = RoomPlacementNumbers.NumberCount / BitsPerByte;

    /// <summary>Bytes of the used-station table: a word per area slot.</summary>
    public const int UsedStationByteCount = AreaBossByteCount * sizeof(ushort);

    /// <summary>Bytes of the map-station table, including its padding.</summary>
    public const int MapStationByteCount = 12;

    /// <summary>Bytes of one area's explored-map plane.</summary>
    public const int ExploredPlaneByteCount = AreaMapPages.BitPlaneBytesPerPage * (AreaMapSize.WidthInMapTiles / AreaMapPages.PageWidthInTiles);

    /// <summary>The value the game stores for an acquired area map.</summary>
    private const byte AcquiredMap = byte.MaxValue;

    /// <summary>Flags one table byte holds.</summary>
    private const int BitsPerByte = 8;

    /// <summary>The event whose bit holds the Zebetite counter's lowest bit; the next events hold its higher bits.</summary>
    private const int FirstZebetiteCounterEvent = 3;

    /// <summary>The bits of the Zebetite counter.</summary>
    private const int ZebetiteCounterBits = 3;

    /// <summary>The proven boss flags in the order of their bits, lowest first.</summary>
    private static readonly DefeatedBosses[] BossFlagBits = [DefeatedBosses.AreaBoss, DefeatedBosses.AreaMiniBoss, DefeatedBosses.AreaTorizo];

    /// <summary>Records exactly the item locations the collected-item <paramref name="table"/> stores as collected.</summary>
    /// <exception cref="ArgumentException">The table is not sixty-four bytes.</exception>
    public static void ReadCollectedItems(ReadOnlySpan<byte> table, PersistentProgress progress)
    {
        Ensure.NotNull(progress);
        Ensure.LengthEqual(table, RoomFlagByteCount);
        foreach (ItemLocation location in ItemLocationDefinitions.All)
            progress.CollectedItems.Set(location, IsBitSet(table, location.Number()));
    }

    /// <summary>Stores <paramref name="progress"/>'s collected item locations in the collected-item <paramref name="table"/>.</summary>
    /// <exception cref="ArgumentException">The table is not sixty-four bytes.</exception>
    public static void WriteCollectedItems(PersistentProgress progress, Span<byte> table)
    {
        Ensure.NotNull(progress);
        Ensure.LengthEqual(table, RoomFlagByteCount);
        table.Clear();
        foreach (ItemLocation location in progress.CollectedItems.Members)
            SetBit(table, location.Number());
    }

    /// <summary>Records exactly the doors the opened-door <paramref name="table"/> stores as opened.</summary>
    /// <exception cref="ArgumentException">The table is not sixty-four bytes.</exception>
    public static void ReadOpenedDoors(ReadOnlySpan<byte> table, PersistentProgress progress)
    {
        Ensure.NotNull(progress);
        Ensure.LengthEqual(table, RoomFlagByteCount);
        foreach (LockedDoor door in LockedDoorDefinitions.All)
            progress.OpenedDoors.Set(door, IsBitSet(table, door.Number()));
    }

    /// <summary>Stores <paramref name="progress"/>'s opened doors in the opened-door <paramref name="table"/>.</summary>
    /// <exception cref="ArgumentException">The table is not sixty-four bytes.</exception>
    public static void WriteOpenedDoors(PersistentProgress progress, Span<byte> table)
    {
        Ensure.NotNull(progress);
        Ensure.LengthEqual(table, RoomFlagByteCount);
        table.Clear();
        foreach (LockedDoor door in progress.OpenedDoors.Members)
            SetBit(table, door.Number());
    }

    /// <summary>The item locations <paramref name="itemTable"/>, an overwrite of the collected-item table, decides: those whose bits it reaches.</summary>
    public static DecidedFlags<ItemLocation> CollectedItemsOf(BitTablePrefix itemTable) =>
        Decided(Ensure.NotNull(itemTable), ItemLocationDefinitions.All, location => location.Number());

    /// <summary>The doors <paramref name="doorTable"/>, an overwrite of the opened-door table, decides: those whose bits it reaches.</summary>
    public static DecidedFlags<LockedDoor> OpenedDoorsOf(BitTablePrefix doorTable) =>
        Decided(Ensure.NotNull(doorTable), LockedDoorDefinitions.All, door => door.Number());

    /// <summary>The members of a kind whose numbered bits <paramref name="table"/> reaches, and those it sets.</summary>
    private static DecidedFlags<T> Decided<T>(BitTablePrefix table, IReadOnlyList<T> kind, Func<T, int> number)
        where T : notnull
    {
        T[] decided = [.. kind.Where(member => number(member) < table.Decided)];
        return new DecidedFlags<T>(decided.ToHashSet(), decided.Where(member => table.Set.Contains(number(member))).ToHashSet());
    }

    /// <summary>Sets bit <paramref name="number"/> of a bit table.</summary>
    private static void SetBit(Span<byte> table, int number) => table[number / BitsPerByte] |= (byte)(1 << (number % BitsPerByte));

    /// <summary>What writing <paramref name="copied"/> over a bit table's leading bytes decides.</summary>
    public static BitTablePrefix PrefixOverwrite(ReadOnlySpan<byte> copied)
    {
        var set = new List<int>();
        for (int number = 0; number < copied.Length * BitsPerByte; number++)
        {
            if (IsBitSet(copied, number))
                set.Add(number);
        }
        return new BitTablePrefix(copied.Length * BitsPerByte, set);
    }

    /// <summary>
    /// Records the story events <paramref name="table"/> stores in <paramref name="progress"/>'s events, and the Zebetite
    /// counter it keeps in the bits of events three through five as <paramref name="progress"/>'s Zebetite count.
    /// </summary>
    /// <exception cref="ArgumentException">The table is not eight bytes.</exception>
    public static void ReadEvents(ReadOnlySpan<byte> table, PersistentProgress progress)
    {
        Ensure.NotNull(progress);
        Ensure.LengthEqual(table, EventByteCount);
        foreach (StoryEvent storyEvent in Enum.GetValues<StoryEvent>())
            progress.Events.Set(storyEvent, IsBitSet(table, storyEvent.Number()));
        int counter = 0;
        for (int bit = 0; bit < ZebetiteCounterBits; bit++)
        {
            if (IsBitSet(table, FirstZebetiteCounterEvent + bit))
                counter |= 1 << bit;
        }
        progress.ZebetitesDestroyed = ZebetitesDestroyed(counter);
    }

    /// <summary>
    /// Stores <paramref name="progress"/>'s story events in <paramref name="table"/>, and its Zebetite count in the bits of
    /// events three through five.
    /// </summary>
    /// <exception cref="ArgumentException">The table is not eight bytes.</exception>
    public static void WriteEvents(PersistentProgress progress, Span<byte> table)
    {
        Ensure.NotNull(progress);
        Ensure.LengthEqual(table, EventByteCount);
        table.Clear();
        foreach (StoryEvent storyEvent in progress.Events.Members)
        {
            int number = storyEvent.Number();
            table[number / BitsPerByte] |= (byte)(1 << (number % BitsPerByte));
        }
        for (int bit = 0; bit < ZebetiteCounterBits; bit++)
        {
            if ((progress.ZebetitesDestroyed & (1 << bit)) != 0)
            {
                int counterEvent = FirstZebetiteCounterEvent + bit;
                table[counterEvent / BitsPerByte] |= (byte)(1 << (counterEvent % BitsPerByte));
            }
        }
    }

    /// <summary>
    /// The story events <paramref name="eventTable"/>, an overwrite of the event table, decides: those whose bits it
    /// reaches. The Zebetite counter's bits are <see cref="ZebetitesDestroyedOf"/>'s, and bits no story event uses hold
    /// nothing the game reads.
    /// </summary>
    public static DecidedFlags<StoryEvent> StoryEventsOf(BitTablePrefix eventTable)
    {
        Ensure.NotNull(eventTable);
        StoryEvent[] decided = [.. Enum.GetValues<StoryEvent>().Where(storyEvent => storyEvent.Number() < eventTable.Decided)];
        return new DecidedFlags<StoryEvent>(
            decided.ToHashSet(),
            decided.Where(storyEvent => eventTable.Set.Contains(storyEvent.Number())).ToHashSet());
    }

    /// <summary>
    /// The Zebetite count <paramref name="eventTable"/>, an overwrite of the event table, leaves; null when it stops before
    /// the Zebetite counter's bits.
    /// </summary>
    /// <exception cref="InvalidDataException">The overwrite decides only some of the counter's bits, which no count expresses.</exception>
    public static int? ZebetitesDestroyedOf(BitTablePrefix eventTable)
    {
        Ensure.NotNull(eventTable);
        if (eventTable.Decided <= FirstZebetiteCounterEvent)
            return null;
        if (eventTable.Decided < FirstZebetiteCounterEvent + ZebetiteCounterBits)
            throw new InvalidDataException("The overwrite decides only part of the Zebetite counter.");
        int counter = 0;
        for (int bit = 0; bit < ZebetiteCounterBits; bit++)
        {
            if (eventTable.Set.Contains(FirstZebetiteCounterEvent + bit))
                counter |= 1 << bit;
        }
        return ZebetitesDestroyed(counter);
    }

    /// <summary>
    /// The Zebetites destroyed a stored Zebetite counter means. The game itself stores 0 through 4, but SpaceTime's
    /// overwrite can leave any value of the three bits. The Zebetites' initialization, the counter's only reader, keeps
    /// every Zebetite away for every value from four up, so such a value means all four are destroyed.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="counter"/> does not fit the counter's three bits.</exception>
    public static int ZebetitesDestroyed(int counter) =>
        Math.Min(Ensure.BetweenInclusive(0, (1 << ZebetiteCounterBits) - 1, counter), PersistentProgress.ZebetiteCount);

    /// <summary>Each area's defeated bosses as the area-boss table stores them.</summary>
    /// <exception cref="ArgumentException">The table is not eight bytes.</exception>
    public static void ReadAreaBosses(ReadOnlySpan<byte> table, PersistentProgress progress)
    {
        Ensure.NotNull(progress);
        Ensure.LengthEqual(table, AreaBossByteCount);
        foreach (KeyValuePair<AreaId, DefeatedBosses> area in AreaBossesPrefix(table))
            progress.SetDefeatedBosses(area.Key, area.Value);
    }

    /// <summary>Stores each area's defeated bosses in <paramref name="table"/>.</summary>
    /// <exception cref="ArgumentException">The table is not eight bytes.</exception>
    public static void WriteAreaBosses(PersistentProgress progress, Span<byte> table)
    {
        Ensure.NotNull(progress);
        Ensure.LengthEqual(table, AreaBossByteCount);
        table.Clear();
        for (int index = 0; index < SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.Count; index++)
            table[index] = EncodeBossMask(progress.BossesDefeatedIn(SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.At(index)));
    }

    /// <summary>The defeated bosses of each area <paramref name="copied"/>, the area-boss table's leading bytes, covers.</summary>
    public static IReadOnlyDictionary<AreaId, DefeatedBosses> AreaBossesPrefix(ReadOnlySpan<byte> copied)
    {
        var areas = new Dictionary<AreaId, DefeatedBosses>();
        for (int index = 0; index < Math.Min(copied.Length, SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.Count); index++)
            areas.Add(SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.At(index), StoredBosses(copied[index]));
        return areas;
    }

    /// <summary>
    /// The bosses an area's stored boss byte records. Bits no routine tests stand for nothing and are not kept, so
    /// re-encoding the byte writes them as zero.
    /// </summary>
    /// <param name="value">An area's boss byte, as the table or a copy of it holds it.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is not a byte.</exception>
    public static DefeatedBosses StoredBosses(int value)
    {
        _ = Ensure.BetweenInclusive(0, byte.MaxValue, value);
        DefeatedBosses bosses = DefeatedBosses.None;
        for (int bit = 0; bit < BossFlagBits.Length; bit++)
        {
            if ((value & (1 << bit)) != 0)
                bosses |= BossFlagBits[bit];
        }
        return bosses;
    }

    /// <summary>The boss byte that records <paramref name="bosses"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="bosses"/> holds a value that names no boss.</exception>
    public static byte EncodeBossMask(DefeatedBosses bosses)
    {
        ProvenDefeatedBosses.Validate(bosses, "Encoded boss mask");
        int value = 0;
        for (int bit = 0; bit < BossFlagBits.Length; bit++)
        {
            if ((bosses & BossFlagBits[bit]) != DefeatedBosses.None)
                value |= 1 << bit;
        }
        return checked((byte)value);
    }

    /// <summary>
    /// Records exactly the stations the used-station <paramref name="table"/> stores as used: each area's word holds a bit
    /// per number its station list can give a station, and the bits of numbers that name no station Samus uses are never
    /// read, by the game or by the file-select map.
    /// </summary>
    /// <exception cref="ArgumentException">The table is not sixteen bytes.</exception>
    public static void ReadUsedStations(ReadOnlySpan<byte> table, PersistentProgress progress)
    {
        Ensure.NotNull(progress);
        Ensure.LengthEqual(table, UsedStationByteCount);
        foreach (LoadStation station in LoadStationDefinitions.Usable)
            progress.UsedStations.Set(station, IsBitSet(table, StationBit(station)));
    }

    /// <summary>Stores <paramref name="progress"/>'s used stations in <paramref name="table"/>.</summary>
    /// <exception cref="ArgumentException">The table is not sixteen bytes.</exception>
    public static void WriteUsedStations(PersistentProgress progress, Span<byte> table)
    {
        Ensure.NotNull(progress);
        Ensure.LengthEqual(table, UsedStationByteCount);
        table.Clear();
        foreach (LoadStation station in progress.UsedStations.Members)
            SetBit(table, StationBit(station));
    }

    /// <summary>
    /// The stations <paramref name="areas"/>, an overwrite of the used-station table by area, decides: those whose numbers
    /// their area's overwrite reaches.
    /// </summary>
    public static DecidedFlags<LoadStation> UsedStationsOf(IReadOnlyDictionary<AreaId, BitTablePrefix> areas)
    {
        Ensure.NotNull(areas);
        LoadStation[] decided = [.. LoadStationDefinitions.Usable.Where(station =>
            areas.TryGetValue(station.Area, out BitTablePrefix? area) && LoadStationNumberCodec.Number(station) < area.Decided)];
        return new DecidedFlags<LoadStation>(decided.ToHashSet(),
            decided.Where(station => areas[station.Area].Set.Contains(LoadStationNumberCodec.Number(station))).ToHashSet());
    }

    /// <summary>
    /// The bit of the used-station table that records <paramref name="station"/>: its number's bit in its area's word.
    /// </summary>
    private static int StationBit(LoadStation station) =>
        AreaNumberCodec.Encode(station.Area) * sizeof(ushort) * BitsPerByte + LoadStationNumberCodec.Number(station);

    /// <summary>
    /// The used stations of each area <paramref name="copied"/>, the used-station table's leading bytes, covers: an
    /// area's first eight stations from the low byte of its word, the rest from the high byte.
    /// </summary>
    public static IReadOnlyDictionary<AreaId, BitTablePrefix> UsedStationsPrefix(ReadOnlySpan<byte> copied)
    {
        var areas = new Dictionary<AreaId, BitTablePrefix>();
        for (int index = 0; index < SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.Count && index * sizeof(ushort) < copied.Length; index++)
        {
            int start = index * sizeof(ushort);
            areas.Add(SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.At(index), PrefixOverwrite(copied.Slice(start, Math.Min(sizeof(ushort), copied.Length - start))));
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
        for (int index = 0; index < SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.Count; index++)
        {
            if (progress.HasAreaMap(SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.At(index)))
                table[index] = AcquiredMap;
        }
    }

    /// <summary>Whether each area <paramref name="copied"/>, the map-station table's leading bytes, covers has its map acquired.</summary>
    public static IReadOnlyDictionary<AreaId, bool> AreaMapsPrefix(ReadOnlySpan<byte> copied)
    {
        var areas = new Dictionary<AreaId, bool>();
        for (int index = 0; index < Math.Min(copied.Length, SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.Count); index++)
            areas.Add(SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.At(index), copied[index] != 0);
        return areas;
    }

    /// <summary>Records exactly the cells <paramref name="plane"/> stores as <paramref name="area"/>'s explored cells.</summary>
    /// <exception cref="ArgumentException">The plane is not 256 bytes.</exception>
    public static void ReadExploredPlane(ReadOnlySpan<byte> plane, AreaId area, PersistentProgress progress)
    {
        Ensure.NotNull(progress);
        Ensure.LengthEqual(plane, ExploredPlaneByteCount);
        for (int y = 0; y < AreaMapSize.HeightInMapTiles; y++)
        {
            for (int x = 0; x < AreaMapSize.WidthInMapTiles; x++)
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
            plane[AreaMapPages.GetBitByteIndex(cell)] |= checked((byte)AreaMapPages.GetBitMask(cell.X));
    }

    /// <summary>Whether flag <paramref name="number"/> of a bit table is set.</summary>
    private static bool IsBitSet(ReadOnlySpan<byte> table, int number) =>
        (table[number / BitsPerByte] & (1 << (number % BitsPerByte))) != 0;
}
