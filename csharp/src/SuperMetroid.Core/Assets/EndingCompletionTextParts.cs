using System.Collections;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>$8C:A69D..B6F2: three progressively revealed messages, ten digits,
/// and a colon. Each glyph draws bottom then top; prefixes draw newest letters first.
/// Font3 large glyphs occupy the OBJ atlas at tile$100 plus their reviewed font index.</summary>
internal sealed class EndingCompletionTextParts : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Completion-text reveal frame that determines the visible glyphs and their OAM order.</summary>
    private readonly int frame;

    /// <summary>Creates the sprite-part view for one frame of the completion-text sequence.</summary>
    /// <param name="frame">Reveal frame from zero through fifty-five.</param>
    /// <exception cref="ArgumentOutOfRangeException">The frame is outside the authored sequence.</exception>
    internal EndingCompletionTextParts(int frame)
    {
        if ((uint)frame >= 56) throw new ArgumentOutOfRangeException(nameof(frame));
        this.frame = frame;
    }

    /// <summary>Gets the number of top-and-bottom glyph parts visible on this reveal frame.</summary>
    public int Count => frame < 15 ? 2 * (frame + 1) : frame < 36 ? 2 * (frame - 14) : frame < 45 ? 2 * (frame - 35) : 2;

    /// <summary>Gets one glyph half at its position in the frame's native sprite ordering.</summary>
    /// <param name="index">Zero-based index among the visible top-and-bottom sprite parts.</param>
    /// <returns>The compiled OAM part for that glyph half.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside the parts visible on this frame.</exception>
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            bool bottom = index % 2 == 0;
            int x, bottomY = 0, tile;
            SnesTileFlipFlags flips = 0;
            if (frame >= 45)
            {
                x = -4;
                if (frame == 55)
                {
                    // Native $B66F uses the same colon dot tile for both halves.
                    tile = 0x15a;
                    if (bottom) flips = SnesTileFlipFlags.Vertical;
                }
                else tile = 0x100 + EndingTextDefinitions.CompileGlyph((char)('0' + frame - 45), EndingTextStyle.CopyrightLarge, bottom);
            }
            else
            {
                string text;
                int start;
                if (frame < 15) { text = "THE OPERATION WAS"; start = -72; }
                else if (frame < 36) { text = "COMPLETED SUCCESSFULLY"; start = -88; bottomY = 24; }
                else { text = "CLEAR TIME"; start = -64; }
                int letter = Count / 2 - 1 - index / 2;
                int column = 0;
                for (; column < text.Length; column++)
                {
                    if (text[column] == ' ') continue;
                    if (letter-- == 0) break;
                }
                x = start + column * 8;
                tile = 0x100 + EndingTextDefinitions.CompileGlyph(text[column], EndingTextStyle.CopyrightLarge, bottom);
            }
            return new(SnesSpritemapXWord.Create(x, false), unchecked((byte)(bottom ? bottomY : bottomY - 8)),
                SnesObjAttributeWord.Create(tile, 0, 3, flips), true);
        }
    }

    /// <summary>Enumerates the visible glyph halves in their native sprite ordering.</summary>
    /// <returns>An enumerator that yields each compiled part for this reveal frame.</returns>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
