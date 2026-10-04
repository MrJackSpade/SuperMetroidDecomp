using System.Collections;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>$8C:A69D..B6F2: three progressively revealed messages, ten digits,
/// and a colon. Each glyph draws bottom then top; prefixes draw newest letters first.
/// Font3 large glyphs occupy the OBJ atlas at tile$100 plus their reviewed font index.</summary>
internal sealed class EndingCompletionTextParts : IReadOnlyList<CompiledSpritePart>
{
    private readonly int frame;
    internal EndingCompletionTextParts(int frame)
    {
        if ((uint)frame >= 56) throw new ArgumentOutOfRangeException(nameof(frame));
        this.frame = frame;
    }
    public int Count => frame < 15 ? 2 * (frame + 1) : frame < 36 ? 2 * (frame - 14) : frame < 45 ? 2 * (frame - 35) : 2;

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
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
