using System.Collections;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Immutable indexed enemy sprite composition; parts may calculate instead of occupying a complete stored array.</summary>
public abstract class EnemySpritemapParts : IReadOnlyList<SpritePart>
{
    /// <summary>Restricts construction to immutable composition implementations.</summary>
    private protected EnemySpritemapParts() { }

    /// <summary>The number of parts in the immutable composition.</summary>
    public abstract int Count { get; }

    /// <summary>Returns the part at its draw-order index, frontmost first.</summary>
    /// <param name="index">Zero-based index less than <see cref="Count"/>.</param>
    public abstract SpritePart this[int index] { get; }

    /// <summary>Enumerates parts in draw order, frontmost first.</summary>
    public IEnumerator<SpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Uses the public draw-order iterator for non-generic consumers.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>Shared composition with no parts.</summary>
    internal static EnemySpritemapParts Empty { get; } = new StoredParts([]);
    /// <summary>Takes ownership of a newly compiled array without copying its parts.</summary>
    /// <remarks>The loader transfers its newly compiled, unexposed array to this immutable view.</remarks>
    internal static EnemySpritemapParts FromOwnedArray(SpritePart[] parts) => new StoredParts(parts);
    /// <summary>Immutable view over an array whose ownership the loader has transferred.</summary>
    private sealed class StoredParts(SpritePart[] parts) : EnemySpritemapParts
    {
        /// <summary>Uses the owned array's exact part count.</summary>
        public override int Count => parts.Length;
        /// <summary>Preserves the loader's part order during indexed access.</summary>
        public override SpritePart this[int index] => parts[index];
    }
}
