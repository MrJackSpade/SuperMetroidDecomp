using System.Collections;

namespace SuperMetroid.Core.Hardware;

/// <summary>Immutable indexed enemy OBJ composition; parts may calculate instead of occupying a complete stored array.</summary>
public abstract class EnemySpritemapParts : IReadOnlyList<EnemySpritemapPart>
{
    private protected EnemySpritemapParts() { }

    /// <summary>The number of hardware OBJ parts in the immutable spritemap composition.</summary>
    public abstract int Count { get; }

    /// <summary>Returns the part at its native draw-order index.</summary>
    /// <param name="index">Zero-based index less than <see cref="Count"/>.</param>
    public abstract EnemySpritemapPart this[int index] { get; }

    /// <summary>Enumerates parts in the same order that the enemy draw routine emits them to OAM.</summary>
    public IEnumerator<EnemySpritemapPart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    internal static EnemySpritemapParts Empty { get; } = new StoredParts([]);
    // The loader transfers its newly compiled, unexposed array to this immutable view.
    internal static EnemySpritemapParts FromOwnedArray(EnemySpritemapPart[] parts) => new StoredParts(parts);
    private sealed class StoredParts(EnemySpritemapPart[] parts) : EnemySpritemapParts
    {
        public override int Count => parts.Length;
        public override EnemySpritemapPart this[int index] => parts[index];
    }
}
