namespace SuperMetroid.Core.Game;

/// <summary>One contiguous run of native collision records starting at <paramref name="Start"/>.</summary>
/// <param name="Start">Address of the run's first record.</param>
/// <param name="Records">Each record's elements in native order.</param>
internal readonly record struct CollisionRecordRun<TElement>(ushort Start, TElement[][] Records);

/// <summary>
/// Native counted collision records laid out in contiguous runs: each record is a count word
/// followed by fixed-size elements, so every record address derives from its run start and the
/// preceding record sizes rather than being stored beside its contents.
/// </summary>
internal sealed class CollisionRecordRuns<TElement>
{
    /// <summary>Bytes in one extended-frame component (X, Y, spritemap and hitbox-list words).</summary>
    internal const int FrameComponentBytes = 8;
    /// <summary>Bytes in one hitbox rectangle (left, top, right, bottom, touch and shot callbacks).</summary>
    internal const int HitboxBytes = 12;

    /// <summary>Byte width of each element stored after a record's count word.</summary>
    private readonly int elementBytes;
    /// <summary>Ordered contiguous record groups from which pointers are derived.</summary>
    private readonly CollisionRecordRun<TElement>[] runs;

    /// <summary>Initializes the record index and computes the total number of records in all runs.</summary>
    /// <param name="elementBytes">Byte width of each payload element.</param>
    /// <param name="runs">Contiguous runs in native pointer order.</param>
    private CollisionRecordRuns(int elementBytes, CollisionRecordRun<TElement>[] runs)
    {
        this.elementBytes = elementBytes;
        this.runs = runs;
        Count = runs.Sum(run => run.Records.Length);
    }

    /// <summary>Extended-frame records whose elements are eight-byte components.</summary>
    internal static CollisionRecordRuns<TElement> Frames(params CollisionRecordRun<TElement>[] runs) =>
        new(FrameComponentBytes, runs);

    /// <summary>Hitbox lists whose elements are twelve-byte rectangles.</summary>
    internal static CollisionRecordRuns<TElement> HitboxLists(params CollisionRecordRun<TElement>[] runs) =>
        new(HitboxBytes, runs);

    /// <summary>Total number of records across the configured runs.</summary>
    internal int Count { get; }

    /// <summary>Address of the <paramref name="index"/>th record in run order.</summary>
    internal ushort PointerAt(int index) => Locate(index, out _);

    /// <summary>Finds the record that begins at a native pointer.</summary>
    /// <param name="pointer">Record start address to locate.</param>
    /// <param name="elements">Receives the record payload when found, or an empty array otherwise.</param>
    /// <returns>True when a run contains a record beginning at <paramref name="pointer"/>.</returns>
    internal bool TryGet(ushort pointer, out TElement[] elements)
    {
        foreach (CollisionRecordRun<TElement> run in runs)
        {
            ushort cursor = run.Start;
            foreach (TElement[] record in run.Records)
            {
                if (cursor == pointer)
                {
                    elements = record;
                    return true;
                }
                cursor = Next(cursor, record.Length);
            }
        }
        elements = [];
        return false;
    }

    /// <summary>Maps a flattened run-order index to its native record address and payload.</summary>
    /// <param name="index">Zero-based record index across all runs.</param>
    /// <param name="record">Receives the payload at the selected index.</param>
    /// <returns>The address of the selected record's count word.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the flattened record range.</exception>
    private ushort Locate(int index, out TElement[] record)
    {
        if ((uint)index >= Count) throw new IndexOutOfRangeException();
        foreach (CollisionRecordRun<TElement> run in runs)
        {
            if (index >= run.Records.Length)
            {
                index -= run.Records.Length;
                continue;
            }
            ushort cursor = run.Start;
            for (int preceding = 0; preceding < index; preceding++)
                cursor = Next(cursor, run.Records[preceding].Length);
            record = run.Records[index];
            return cursor;
        }
        throw new IndexOutOfRangeException();
    }

    /// <summary>Advances from one record start past its count word and fixed-width payload.</summary>
    /// <param name="pointer">Address of the current record's count word.</param>
    /// <param name="elements">Number of payload elements in the current record.</param>
    /// <returns>The wrapped 16-bit address immediately after the record.</returns>
    private ushort Next(ushort pointer, int elements) =>
        (ushort)(pointer + sizeof(ushort) + elementBytes * elements);
}
