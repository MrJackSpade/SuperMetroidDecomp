using System.Collections;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>$8C:A5E2 afterglow: atlas rows $19..1E map to eight-pixel rows
/// around screen Y=0 at atlas row$1B, with X centered at column8. Row$18 packs
/// the four small upper-cap tiles, four large upper-body tiles, then four small
/// lower-cap tiles. The selected tile sequence remains independently supplied.</summary>
internal sealed class EndingExplosionAfterglowParts : IReadOnlyList<CompiledSpritePart>
{
    private readonly int[] tiles;
    private EndingExplosionAfterglowParts(int[] tiles) => this.tiles = tiles;

    internal static SpriteComposition CalculateIfMatching(ushort pointer, SpriteComposition supplied)
    {
        if (pointer != EndingExplosionSpriteDefinitions.Pointer(EndingExplosionSpriteDefinitions.Pose.Afterglow)) return supplied;
        var tiles = new int[supplied.PartCount];
        for (int index = 0; index < tiles.Length; index++)
        {
            int tile = supplied.Part(index).Attributes.TileNumber;
            if (tile is < 0x180 or > 0x1ef) return supplied;
            tiles[index] = tile;
        }
        return supplied.CalculateIfMatching(new EndingExplosionAfterglowParts(tiles));
    }

    public int Count => tiles.Length;
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            int tile = tiles[index], row = tile / 16, column = tile % 16;
            int x = (column - 8) * 8, y = (row - 0x1b) * 8;
            bool large = row is >= 0x1a and <= 0x1d;
            if (row == 0x18)
            {
                if (column < 4) { x = (column - 2) * 8; y = -32; }
                else if (column >= 12) { x = (column - 14) * 8; y = 32; }
                else { y = -24; large = true; }
            }
            return new(SnesSpritemapXWord.Create(x, large), unchecked((byte)y),
                SnesObjAttributeWord.Create(tile, 0, 0), true);
        }
    }
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
