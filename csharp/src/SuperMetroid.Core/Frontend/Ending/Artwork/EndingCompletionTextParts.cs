using System.Collections;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>$8C:A69D..B6F2: three progressively revealed messages, ten digits,
/// and a colon. Each glyph draws bottom then top; prefixes draw newest letters first.
/// Font3 large glyphs occupy the OBJ atlas at tile$100 plus their reviewed font index.</summary>
internal sealed class EndingCompletionTextParts : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Native composition ordinal: revealed message prefix, digit, or colon.</summary>
    private readonly int frame;
    /// <summary>Accepts one of the fifty-six ending text compositions.</summary>
    internal EndingCompletionTextParts(int frame)
    {
        if ((uint)frame >= 56) throw new ArgumentOutOfRangeException(nameof(frame));
        this.frame = frame;
    }
    /// <summary>Revealed messages add paired glyph halves; a digit or colon always uses two parts.</summary>
    public int Count => frame < 15 ? 2 * (frame + 1) : frame < 36 ? 2 * (frame - 14) : frame < 45 ? 2 * (frame - 35) : 2;

    /// <summary>Selects the bottom or top glyph half in reverse-letter OAM order for this reveal frame.</summary>
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            bool bottom = index % 2 == 0;
            int x, bottomY = 0, tile;
            TileFlip flips = 0;
            if (frame >= 45)
            {
                x = -4;
                if (frame == 55)
                {
                    // Native $B66F uses the same colon dot tile for both halves.
                    tile = CharacterSheetGrid.Cell(21, 10);
                    if (bottom) flips = TileFlip.Vertical;
                }
                else tile = CharacterSheetGrid.Cell(16, 0) + EndingTextDefinitions.CompileGlyph((char)('0' + frame - 45), EndingTextStyle.CopyrightLarge, bottom);
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
                tile = CharacterSheetGrid.Cell(16, 0) + EndingTextDefinitions.CompileGlyph(text[column], EndingTextStyle.CopyrightLarge, bottom);
            }
            return new(SpritePart.Create(x, bottom ? bottomY : bottomY - 8, false,
                tile, 0, 3, flips), true);
        }
    }
    /// <summary>Yields all visible glyph halves in their native OAM order.</summary>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Exposes the same glyph halves to non-generic collection consumers.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
