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

    private readonly int elementBytes;
    private readonly CollisionRecordRun<TElement>[] runs;

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

    internal int Count { get; }

    /// <summary>Every record address, in run order.</summary>
    internal IEnumerable<ushort> Pointers
    {
        get
        {
            foreach (CollisionRecordRun<TElement> run in runs)
            {
                ushort cursor = run.Start;
                foreach (TElement[] record in run.Records)
                {
                    yield return cursor;
                    cursor = Next(cursor, record.Length);
                }
            }
        }
    }

    /// <summary>Address of the <paramref name="index"/>th record in run order.</summary>
    internal ushort PointerAt(int index) => Locate(index, out _);

    /// <summary>Elements of the <paramref name="index"/>th record in run order.</summary>
    internal TElement[] RecordAt(int index)
    {
        Locate(index, out TElement[] record);
        return record;
    }

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

    private ushort Next(ushort pointer, int elements) =>
        (ushort)(pointer + sizeof(ushort) + elementBytes * elements);
}
