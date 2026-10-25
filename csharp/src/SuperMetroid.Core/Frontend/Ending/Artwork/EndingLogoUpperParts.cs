using System.Collections;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>Upper S at $8C:B97F: packed atlas regions traverse the outline in
/// native overlap order. The lower S uses the existing half-turn calculation.</summary>
internal sealed class EndingLogoUpperParts : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>The upper S outline comprises fourteen overlapping OBJ parts.</summary>
    public int Count => 14;
    /// <summary>Calculates one atlas tile and offset in the native lower-edge, tip, shoulder, bar, and stem draw order.</summary>
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
                x = -22 - 16 * index; y = 9; tile = SnesCharacterGrid.Cell(6, 4) - 2 * index;
            }
            else if (index < 4)
            {
                // Narrow upper tip, bottom to top.
                int row = 3 - index;
                x = 26; y = -55 + 8 * row; tile = SnesCharacterGrid.Cell(0, 8) + 16 * row; large = false;
            }
            else if (index < 6)
            {
                // Inner base, right to left.
                int column = 5 - index;
                x = -22 + 16 * column; y = -7; tile = SnesCharacterGrid.Cell(8, 0) + 2 * column;
            }
            else if (index == 6)
            {
                // Upper-right cap.
                x = 10; y = -55; tile = SnesCharacterGrid.Cell(0, 6);
            }
            else if (index < 10)
            {
                // Shoulder, right to left.
                int column = 9 - index;
                x = -22 + 16 * column; y = -39; tile = SnesCharacterGrid.Cell(2, 2) + 2 * column;
            }
            else if (index < 12)
            {
                // Middle bar, right to left.
                int column = 11 - index;
                x = -22 + 16 * column; y = -23; tile = SnesCharacterGrid.Cell(4, 2) + 2 * column;
            }
            else
            {
                // Left stem, bottom to top.
                int row = 13 - index;
                x = -38; y = -23 + 16 * row; tile = SnesCharacterGrid.Cell(4, 0) + 32 * row;
            }
            return new(SpritePart.Create(x, y, large,
                tile, 0, 3, 0), true);
        }
    }
    /// <summary>Yields the complete upper logo outline in its native overlap order.</summary>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Exposes that same ordered outline through the non-generic collection interface.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
