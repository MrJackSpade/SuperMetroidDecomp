using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// Verification's view of the import pipeline's FX header pointers (<see cref="RoomFxSources"/>) and of the bank-$83
/// pointer each compiled record occupies: a header's records sit sixteen bytes apart from the header.
/// </summary>
internal static class RoomFxDefinitionsAccess
{
    /// <summary>Each record pointer's header and index, first header first.</summary>
    private static readonly SortedDictionary<ushort, IndexedFx> RecordsByPointer = IndexRecords();

    /// <summary>Each record instance's pointer.</summary>
    private static readonly Dictionary<RoomFxRecordDefinition, ushort> PointersByRecord = IndexPointers();

    /// <summary>Places every record of every compiled header at its bank-$83 pointer.</summary>
    private static SortedDictionary<ushort, IndexedFx> IndexRecords()
    {
        var result = new SortedDictionary<ushort, IndexedFx>();
        foreach (ushort header in RoomFxSources.Pointers)
        {
            RoomFxDefinition fx = RoomFxSources.Fx(header);
            for (int index = 0; index < fx.Records.Count; index++)
                result.TryAdd(checked((ushort)(header + index * RoomFxToolingRomData.Record.ByteCount)), new IndexedFx(fx, index));
        }
        return result;
    }

    /// <summary>Maps every record instance of every compiled header to its pointer.</summary>
    private static Dictionary<RoomFxRecordDefinition, ushort> IndexPointers()
    {
        var result = new Dictionary<RoomFxRecordDefinition, ushort>(ReferenceEqualityComparer.Instance);
        foreach (ushort header in RoomFxSources.Pointers)
        {
            RoomFxDefinition fx = RoomFxSources.Fx(header);
            for (int index = 0; index < fx.Records.Count; index++)
                result.Add(fx.Records[index], checked((ushort)(header + index * RoomFxToolingRomData.Record.ByteCount)));
        }
        return result;
    }

    /// <summary>Resolves compiled headers and records by pointer.</summary>
    extension(RoomFxDefinitions)
    {
        /// <inheritdoc cref="RoomFxSources.Fx"/>
        internal static RoomFxDefinition Get(ushort pointer) => RoomFxSources.Fx(pointer);

        /// <inheritdoc cref="RoomFxSourcesConstants.PointerOf"/>
        internal static ushort PointerOf(RoomFxDefinition fx) => RoomFxSourcesConstants.PointerOf(fx);

        /// <summary>Every compiled record's pointer, in ascending order.</summary>
        internal static IEnumerable<ushort> RecordPointers => RecordsByPointer.Keys;

        /// <summary>Every compiled record, one per pointer, in pointer order.</summary>
        internal static IEnumerable<RoomFxRecordDefinition> AllRecords => RecordsByPointer.Values.Select(entry => entry.Fx.Records[entry.Index]);

        /// <summary>The compiled record at <paramref name="pointer"/>.</summary>
        internal static RoomFxRecordDefinition RecordAt(ushort pointer) => RecordsByPointer.TryGetValue(pointer, out var entry)
            ? entry.Fx.Records[entry.Index]
            : throw new ArgumentOutOfRangeException(nameof(pointer), pointer, "Pointer is not a compiled room-FX record.");

        /// <summary>The pointer of a compiled record instance.</summary>
        internal static ushort RecordPointerOf(RoomFxRecordDefinition record) => PointersByRecord[record];

        /// <summary>The records from <paramref name="root"/> to the end of its header, as a header of their own.</summary>
        internal static RoomFxDefinition Suffix(ushort root) => RecordsByPointer.TryGetValue(root, out var entry)
            ? new RoomFxDefinition($"$83:{root:X4}", entry.Fx.Records.Skip(entry.Index).ToArray())
            : throw new ArgumentOutOfRangeException(nameof(root), root, "Pointer is not a compiled room-FX record.");

        /// <summary>The pointer of the record <paramref name="fx"/> selects for <paramref name="door"/>, or zero for none.</summary>
        internal static ushort SelectedRecordPointer(RoomFxDefinition? fx, DoorDefinition? door) =>
            fx?.Select(door) is { } record ? PointersByRecord[record] : (ushort)0;
    }

    /// <summary>The FX header pointer of a room state.</summary>
    extension(CartridgeRoomState self)
    {
        /// <summary>The bank-$83 pointer of this state's FX header, or zero for none.</summary>
        internal ushort FxPointer => self.Fx is { } fx ? RoomFxSourcesConstants.PointerOf(fx) : (ushort)0;
    }
}
