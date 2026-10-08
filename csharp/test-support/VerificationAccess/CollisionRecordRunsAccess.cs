using SuperMetroid.Core.Game;

/// <summary>Verification enumeration of compiled collision records, in run order.</summary>
internal static class CollisionRecordRunsAccess
{
    extension<TElement>(CollisionRecordRuns<TElement> runs)
    {
        /// <summary>Every record address, in run order.</summary>
        internal IEnumerable<ushort> Pointers => Enumerable.Range(0, runs.Count).Select(runs.PointerAt);

        /// <summary>Elements of the <paramref name="index"/>th record in run order.</summary>
        internal TElement[] RecordAt(int index)
        {
            ushort pointer = runs.PointerAt(index);
            return runs.TryGet(pointer, out TElement[] record) ? record
                : throw new InvalidDataException($"Collision record {index} at ${pointer:X4} does not resolve.");
        }
    }
}
