using System.Collections;

namespace SuperMetroid.Core.Hardware;

/// <summary>Immutable indexed enemy OBJ composition; parts may calculate instead of occupying a complete stored array.</summary>
public abstract class EnemySpritemapParts : IReadOnlyList<SpritemapPart>
{
    /// <summary>Restricts construction to immutable composition implementations.</summary>
    private protected EnemySpritemapParts() { }

    /// <summary>The number of hardware OBJ parts in the immutable spritemap composition.</summary>
    public abstract int Count { get; }

    /// <summary>Returns the part at its native draw-order index.</summary>
    /// <param name="index">Zero-based index less than <see cref="Count"/>.</param>
    public abstract SpritemapPart this[int index] { get; }

    /// <summary>Enumerates parts in the same order that the enemy draw routine emits them to OAM.</summary>
    public IEnumerator<SpritemapPart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Uses the public draw-order iterator for non-generic consumers.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>Shared composition with no OAM parts.</summary>
    internal static EnemySpritemapParts Empty { get; } = new StoredParts([]);
    /// <summary>Takes ownership of a newly compiled array without copying its parts.</summary>
    /// <remarks>The loader transfers its newly compiled, unexposed array to this immutable view.</remarks>
    internal static EnemySpritemapParts FromOwnedArray(SpritemapPart[] parts) => new StoredParts(parts);
    /// <summary>Immutable view over an array whose ownership the loader has transferred.</summary>
    private sealed class StoredParts(SpritemapPart[] parts) : EnemySpritemapParts
    {
        /// <summary>Uses the owned array's exact part count for OAM emission.</summary>
        public override int Count => parts.Length;
        /// <summary>Preserves the loader's native part order during indexed access.</summary>
        public override SpritemapPart this[int index] => parts[index];
    }
}
