namespace SuperMetroid.Core.Game;

/// <summary>One immutable ordered bank-$A1 room population and its terminator quota.</summary>
public sealed class RoomEnemyPopulationDefinition
{
    private readonly RoomEnemyPopulationRecord[] records;
    internal RoomEnemyPopulationDefinition(ushort pointer, RoomEnemyPopulationRecord[] records, byte deathQuota)
    {
        Pointer = pointer;
        this.records = records;
        DeathQuota = deathQuota;
    }
    public ushort Pointer { get; }
    public ReadOnlyMemory<RoomEnemyPopulationRecord> Records => records;
    public byte DeathQuota { get; }
}

/// <summary>Application-owned ordered enemy placements selected by all retail room states.</summary>
/// <remarks>
/// The 302 pointers and 1,658 records come from the pinned NTSC J/U v1.0 bank-$A1
/// populations selected by 323 compiled room states. Record order and the byte after
/// each terminator govern linked slot allocation and the room enemy-death quota.
/// This is gameplay data, not an editable visual asset.
/// </remarks>
public static partial class RoomEnemyPopulationDefinitions
{
    private static readonly RoomEnemyPopulationDefinition[] Definitions =
    [
        ..BuildSegment0(),
        ..BuildSegment1(),
        ..BuildSegment2(),
        ..BuildSegment3(),
    ];

    public const int ListCount = 302;
    public const int RecordCount = 1658;

    static RoomEnemyPopulationDefinitions()
    {
        if (Definitions.Length != ListCount || Definitions.Sum(list => list.Records.Length) != RecordCount)
            throw new InvalidDataException("Compiled enemy-population counts changed.");
        for (int index = 1; index < Definitions.Length; index++)
            if (Definitions[index - 1].Pointer >= Definitions[index].Pointer)
                throw new InvalidDataException("Compiled enemy populations are not strictly ordered.");
    }

    /// <summary>Gets a retail population by its native bank-$A1 identity.</summary>
    public static RoomEnemyPopulationDefinition Get(ushort pointer)
    {
        int low = 0;
        int high = Definitions.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            RoomEnemyPopulationDefinition candidate = Definitions[middle];
            if (candidate.Pointer == pointer) return candidate;
            if (candidate.Pointer < pointer) low = middle + 1;
            else high = middle - 1;
        }
        throw new ArgumentOutOfRangeException(nameof(pointer), pointer,
            "Pointer is not a retail bank-$A1 enemy population.");
    }

    /// <summary>Enumerates every compiled population identity for ROM-oracle tests.</summary>
    public static IEnumerable<ushort> Pointers => Definitions.Select(list => list.Pointer);
}
