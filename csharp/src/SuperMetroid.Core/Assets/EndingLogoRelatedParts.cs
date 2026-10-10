using System.Collections;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Ending logo halves rotate by180 degrees; wrap stages reveal portions
/// of the complete right wrap. References preserve independently supplied artwork.</summary>
internal sealed class EndingLogoRelatedParts : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Composition whose parts provide the source artwork for this calculated view.</summary>
    private readonly SpriteComposition source;

    /// <summary>First part in <see cref="source"/> included in this view.</summary>
    private readonly int offset;

    /// <summary>Whether each selected part is transformed by the 180-degree wrap rotation.</summary>
    private readonly bool rotate;

    /// <summary>Creates a bounded view over a composition, optionally rotating its selected parts.</summary>
    /// <param name="source">Composition supplying the sprite parts.</param>
    /// <param name="offset">Index of the first source part exposed by this view.</param>
    /// <param name="count">Number of consecutive source parts exposed.</param>
    /// <param name="rotate">Whether to rotate positions and flip flags for the selected wrap stage.</param>
    private EndingLogoRelatedParts(SpriteComposition source, int offset, int count, bool rotate)
    {
        this.source = source;
        this.offset = offset;
        Count = count;
        this.rotate = rotate;
    }

    /// <summary>
    /// Reuses the supplied composition for nonmatching frames or unsupported source geometry;
    /// otherwise selects the native upper or right-wrap part window and applies its required rotation.
    /// </summary>
    /// <param name="frame">Ending-logo animation frame, from 0 through 7.</param>
    /// <param name="supplied">Original frame composition returned when no derived view is appropriate.</param>
    /// <param name="upper">Candidate 14-part upper-wrap source used by frame 1.</param>
    /// <param name="completeRight">Candidate 25-part complete-right-wrap source used by the later reveal stages.</param>
    /// <returns>The supplied composition, or a calculated view that preserves its independent source artwork.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="frame"/> is outside the eight-frame sequence.</exception>
    internal static SpriteComposition CalculateIfMatching(int frame, SpriteComposition supplied,
        SpriteComposition upper, SpriteComposition completeRight)
    {
        if ((uint)frame >= 8) throw new ArgumentOutOfRangeException(nameof(frame));
        if (frame is 0 or 4) return supplied;
        SpriteComposition source = frame == 1 ? upper : completeRight;
        if (source.PartCount != (frame == 1 ? 14 : 25)) return supplied;
        bool rotate = frame == 1 || frame >= 5;
        int stage = frame == 1 ? -1 : (frame - 2) % 3;
        int offset = stage is 0 or 1 ? 7 : 0;
        int count = stage switch { -1 => 14, 0 => 12, 1 => 18, _ => 25 };
        if (rotate)
            for (int index = offset; index < offset + count; index++)
            {
                CompiledSpritePart part = source.Part(index);
                int size = part.X.IsLarge ? 16 : 8;
                if (-part.X.SignedOffset - size is < -256 or > 255 ||
                    -unchecked((sbyte)part.Y) - size is < -128 or > 127) return supplied;
            }
        return supplied.CalculateIfMatching(new EndingLogoRelatedParts(source, offset, count, rotate));
    }

    /// <summary>Gets the number of parts selected for this wrap stage.</summary>
    public int Count { get; }

    /// <summary>Gets a selected source part, applying the stage's half-turn transform when required.</summary>
    /// <param name="index">Zero-based position within this view.</param>
    /// <value>The selected part with its calculated screen position and flip flags.</value>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is outside this view.</exception>
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            CompiledSpritePart part = source.Part(offset + index);
            if (!rotate) return part;
            int size = part.X.IsLarge ? 16 : 8;
            return new(SnesSpritemapXWord.Create(-part.X.SignedOffset - size, part.X.IsLarge),
                unchecked((byte)(-unchecked((sbyte)part.Y) - size)),
                SnesObjAttributeWord.Create(part.Attributes.TileNumber, part.Attributes.PaletteIndex,
                    part.Attributes.Priority, part.Attributes.FlipFlags ^
                    (SnesTileFlipFlags.Horizontal | SnesTileFlipFlags.Vertical)), part.InheritPalette);
        }
    }
    /// <summary>Enumerates this stage's selected parts in source order.</summary>
    /// <returns>An enumerator over the calculated sprite-part view.</returns>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Returns a non-generic enumerator over this sequence.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
