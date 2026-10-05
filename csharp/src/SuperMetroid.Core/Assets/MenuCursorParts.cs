using System.Collections;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>$82:CBCB-CBFA: two eight-pixel tiles form the centered selection missile.
/// Tile identities and native left/right part order remain independent supplied inputs.</summary>
internal sealed class MenuCursorParts(bool leftFirst, int leftTile, int rightTile) : IReadOnlyList<CompiledSpritePart>
{
    internal static SpriteComposition CalculateIfMatching(string name, SpriteComposition supplied)
    {
        if (name is not ("Cursor.0" or "Cursor.1" or "Cursor.2" or "Cursor.3") || supplied.PartCount != 2)
            return supplied;
        bool leftFirst = supplied.Part(0).X.SignedOffset < 0;
        int leftTile = supplied.Part(leftFirst ? 0 : 1).Attributes.TileNumber;
        int rightTile = supplied.Part(leftFirst ? 1 : 0).Attributes.TileNumber;
        return supplied.CalculateIfMatching(new MenuCursorParts(leftFirst, leftTile, rightTile));
    }

    public int Count => 2;
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            bool left = (index == 0) == leftFirst;
            return new(SnesSpritemapXWord.Create(left ? -8 : 0, false), unchecked((byte)-4),
                SnesObjAttributeWord.Create(left ? leftTile : rightTile, 0, 3, SnesTileFlipFlags.None), true);
        }
    }
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
