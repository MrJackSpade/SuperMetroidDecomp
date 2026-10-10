using System.Collections;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Upper S at $8C:B97F: packed atlas regions traverse the outline in
/// native overlap order. The lower S uses the existing half-turn calculation.</summary>
internal sealed class EndingLogoUpperParts : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>The number of packed sprite parts used to draw the upper S outline.</summary>
    public int Count => 14;

    /// <summary>Gets an upper-S sprite part at its native overlap-order position.</summary>
    /// <param name="index">The zero-based part position, from 0 through <see cref="Count"/> minus one.</param>
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            int x, y, tile;
            bool large = true;
            if (index < 2)
            {
                // Lower edge, right to left.
                x = -22 - 16 * index; y = 9; tile = 0x64 - 2 * index;
            }
            else if (index < 4)
            {
                // Narrow upper tip, bottom to top.
                int row = 3 - index;
                x = 26; y = -55 + 8 * row; tile = 0x08 + 16 * row; large = false;
            }
            else if (index < 6)
            {
                // Inner base, right to left.
                int column = 5 - index;
                x = -22 + 16 * column; y = -7; tile = 0x80 + 2 * column;
            }
            else if (index == 6)
            {
                // Upper-right cap.
                x = 10; y = -55; tile = 0x06;
            }
            else if (index < 10)
            {
                // Shoulder, right to left.
                int column = 9 - index;
                x = -22 + 16 * column; y = -39; tile = 0x22 + 2 * column;
            }
            else if (index < 12)
            {
                // Middle bar, right to left.
                int column = 11 - index;
                x = -22 + 16 * column; y = -23; tile = 0x42 + 2 * column;
            }
            else
            {
                // Left stem, bottom to top.
                int row = 13 - index;
                x = -38; y = -23 + 16 * row; tile = 0x40 + 32 * row;
            }
            return new(SnesSpritemapXWord.Create(x, large), unchecked((byte)y),
                SnesObjAttributeWord.Create(tile, 0, 3, 0), true);
        }
    }

    /// <summary>Enumerates the upper-S sprite parts in the order required to preserve their native overlaps.</summary>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Returns a non-generic enumerator over this sequence.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
