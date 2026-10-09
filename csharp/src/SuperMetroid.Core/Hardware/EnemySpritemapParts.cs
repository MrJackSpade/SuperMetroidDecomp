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

    /// <summary>Shared zero-part composition used when an enemy has no visible pieces.</summary>
    internal static EnemySpritemapParts Empty { get; } = new StoredParts([]);
    // The loader transfers its newly compiled, unexposed array to this immutable view.
    /// <summary>Wraps a newly compiled array whose ownership is transferred to this immutable collection.</summary>
    internal static EnemySpritemapParts FromOwnedArray(EnemySpritemapPart[] parts) => new StoredParts(parts);
    /// <summary>Array-backed representation for spritemaps that do not need computed parts.</summary>
    /// <param name="parts">The privately retained parts in native OAM insertion order.</param>
    private sealed class StoredParts(EnemySpritemapPart[] parts) : EnemySpritemapParts
    {
        /// <summary>Number of retained hardware OBJ parts.</summary>
        public override int Count => parts.Length;
        /// <summary>Gets one retained part in native OAM draw order.</summary>
        public override EnemySpritemapPart this[int index] => parts[index];
    }
}
