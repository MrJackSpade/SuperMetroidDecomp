using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// Verification's view of the import pipeline's FX header pointers (<see cref="RoomFxSources"/>) and of the bank-$83
/// pointer each compiled record occupies: a header's door records sit sixteen bytes apart from the header, its default
/// record after them, and Mother Brain's two records after her room's default. A header without a default ends where
/// its default would sit, at the cartridge's terminator word (#1546).
/// </summary>
internal static class RoomFxDefinitionsAccess
{
    /// <summary>Each record pointer's header, record and door word, first header first.</summary>
    private static readonly SortedDictionary<ushort, IndexedFx> RecordsByPointer = new();

    /// <summary>The header each terminator word ends, by the terminator's pointer.</summary>
    private static readonly SortedDictionary<ushort, RoomFxSelection> EndsByPointer = new();

    /// <summary>The records from each record or terminator pointer to the end of its header, as a header of their own.</summary>
    private static readonly Dictionary<ushort, RoomFxSelection> SuffixesByPointer = new();

    /// <summary>Each record instance's pointer.</summary>
    private static readonly Dictionary<RoomFxRecordDefinition, ushort> PointersByRecord = new(ReferenceEqualityComparer.Instance);

    /// <summary>Places every compiled record and terminator at its bank-$83 pointer.</summary>
    static RoomFxDefinitionsAccess()
    {
        foreach (ushort header in RoomFxSources.Pointers)
        {
            RoomFxSelection fx = RoomFxSources.Fx(header);
            for (int index = 0; index < fx.DoorRecords.Count; index++)
            {
                RoomFxDoorRecord entry = fx.DoorRecords[index];
                ushort pointer = At(header, index);
                Add(pointer, new IndexedFx(fx, entry.Record, DoorHeaderSourcesConstants.PointerOf(entry.Door)),
                    new RoomFxSelection($"$83:{pointer:X4}", [.. fx.DoorRecords.Skip(index)], fx.Default));
            }
            ushort last = At(header, fx.DoorRecords.Count);
            if (fx.Default is { } defaultRecord)
                Add(last, new IndexedFx(fx, defaultRecord, 0), new RoomFxSelection($"$83:{last:X4}", [], defaultRecord));
            else
            {
                EndsByPointer.Add(last, fx);
                SuffixesByPointer.Add(last, new RoomFxSelection($"$83:{last:X4}", [], null));
            }
        }
        RoomFxSelection motherBrain = RoomFxDefinitions.MotherBrainDefaultOrWhenMotherBrainGlassDestroyed;
        AddMotherBrainRecord(motherBrain, MotherBrainFxRecordPointers.Initial, RoomFxDefinitions.MotherBrainAcidRises);
        AddMotherBrainRecord(motherBrain, MotherBrainFxRecordPointers.FakeDeath, RoomFxDefinitions.MotherBrainAcidDrains);

        static ushort At(ushort header, int index) => checked((ushort)(header + index * RoomFxToolingRomData.Record.ByteCount));

        static void AddMotherBrainRecord(RoomFxSelection header, ushort pointer, RoomFxRecordDefinition record) =>
            Add(pointer, new IndexedFx(header, record, 0), new RoomFxSelection($"$83:{pointer:X4}", [], record));

        static void Add(ushort pointer, IndexedFx entry, RoomFxSelection suffix)
        {
            RecordsByPointer.Add(pointer, entry);
            SuffixesByPointer.Add(pointer, suffix);
            PointersByRecord.Add(entry.Record, pointer);
        }
    }

    /// <summary>The door word a compiled record's header stores for it: its entry door's pointer, or zero for a default or a Mother Brain record.</summary>
    /// <exception cref="KeyNotFoundException">The record is not one of the compiled catalog's.</exception>
    internal static ushort DoorPointerOf(RoomFxRecordDefinition record) => RecordsByPointer[PointersByRecord[record]].DoorPointer;

    /// <summary>The area of the rooms whose header holds a compiled record.</summary>
    extension(RoomFxRecordDefinition self)
    {
        /// <summary>
        /// The area of the rooms that load this compiled record's header, whose lists its effects are selected from
        /// (<see cref="RoomFxSelectionAreas"/>).
        /// </summary>
        /// <exception cref="InvalidOperationException">The record is not one of the compiled catalog's.</exception>
        internal AreaId HeaderArea => PointersByRecord.TryGetValue(self, out ushort pointer)
            ? RoomFxSelectionAreas.Of(RecordsByPointer[pointer].Fx)
            : throw new InvalidOperationException("The record is not one of the compiled room-FX catalog's, so no header names its area.");
    }

    /// <summary>Resolves compiled headers and records by pointer.</summary>
    extension(RoomFxDefinitions)
    {
        /// <inheritdoc cref="RoomFxSources.Fx"/>
        internal static RoomFxSelection Get(ushort pointer) => RoomFxSources.Fx(pointer);

        /// <inheritdoc cref="RoomFxSourcesConstants.PointerOf"/>
        internal static ushort PointerOf(RoomFxSelection fx) => RoomFxSourcesConstants.PointerOf(fx);

        /// <summary>Every compiled record's pointer, in ascending order.</summary>
        internal static IEnumerable<ushort> RecordPointers => RecordsByPointer.Keys;

        /// <summary>The pointer of every terminator word, which ends a header that has no default record, in ascending order.</summary>
        internal static IEnumerable<ushort> EndPointers => EndsByPointer.Keys;

        /// <summary>Every compiled record, one per pointer, in pointer order.</summary>
        internal static IEnumerable<RoomFxRecordDefinition> AllRecords => RecordsByPointer.Values.Select(entry => entry.Record);

        /// <summary>The compiled record at <paramref name="pointer"/>.</summary>
        internal static RoomFxRecordDefinition RecordAt(ushort pointer) => RecordsByPointer.TryGetValue(pointer, out var entry)
            ? entry.Record
            : throw new ArgumentOutOfRangeException(nameof(pointer), pointer, "Pointer is not a compiled room-FX record.");

        /// <summary>The pointer of a compiled record instance.</summary>
        internal static ushort RecordPointerOf(RoomFxRecordDefinition record) => PointersByRecord[record];

        /// <summary>The records from <paramref name="root"/>, a record or a terminator, to the end of its header, as a header of their own.</summary>
        internal static RoomFxSelection Suffix(ushort root) => SuffixesByPointer.TryGetValue(root, out RoomFxSelection? suffix)
            ? suffix
            : throw new ArgumentOutOfRangeException(nameof(root), root, "Pointer is not a compiled room-FX record or terminator.");

        /// <summary>The pointer of the record <paramref name="fx"/> selects for <paramref name="door"/>, or zero for none.</summary>
        internal static ushort SelectedRecordPointer(RoomFxSelection? fx, DoorDefinition? door) =>
            fx?.Select(door) is { } record ? PointersByRecord[record] : (ushort)0;
    }

    /// <summary>The FX header pointer of a room state.</summary>
    extension(RoomStateSettings self)
    {
        /// <summary>The bank-$83 pointer of this state's FX header, or zero for none.</summary>
        internal ushort FxPointer => self.Fx is { } fx ? RoomFxSourcesConstants.PointerOf(fx) : (ushort)0;
    }
}
