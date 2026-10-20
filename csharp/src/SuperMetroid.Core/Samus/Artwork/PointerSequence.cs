namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Allocation-free immutable view of the 417 bank-$93 spritemap offsets selected by native timed projectile records, calculated from composition-group layouts.</summary>
public readonly struct PointerSequence : IReadOnlyList<ushort>
{
    /// <summary>Number of required sprite identities, 417, including the zero-part Nothing composition rather than only visible frames.</summary>
    public int Count => 417;
    /// <summary>Required identity count, equivalent to <see cref="Count"/>, for callers using array-style traversal.</summary>
    public int Length => Count;
    /// <summary>Returns one sprite identity in the original sorted native-pointer order, not an instruction-list pointer or animation-stage number.</summary>
    /// <param name="index">Zero-based identity index 0..416.</param>
    /// <returns>Bank-relative spritemap offset in bank $93.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside 0..416.</exception>
    public ushort this[int index] => ProjectileSpriteDefinitions.PointerAt(index);
    /// <summary>Enumerates the complete required identity set in ascending native-pointer order without retaining a backing pointer array.</summary>
    /// <returns>An enumerator yielding all 417 bank-relative spritemap offsets.</returns>
    public IEnumerator<ushort> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Exposes all 417 native pointer offsets to a non-generic catalog consumer.</summary>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}
