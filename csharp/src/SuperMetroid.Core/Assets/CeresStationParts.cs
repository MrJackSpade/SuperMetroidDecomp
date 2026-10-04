using System.Collections;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Thirty strip-aligned parts of the station at8C:9150.
/// Eleven lower-edge pieces remain independently unresolved supplied content.</summary>
internal sealed class CeresStationParts(CompiledSpritePart[] lowerEdge) : IReadOnlyList<CompiledSpritePart>
{
    internal static SpriteComposition CalculateIfMatching(ushort pointer, SpriteComposition supplied)
    {
        if (pointer != CeresFlightSpriteDefinitions.StationUnderAttack || supplied.PartCount != 41) return supplied;
        var edge = new CompiledSpritePart[11];
        for (int index = 0; index < edge.Length; index++) edge[index] = supplied.Part(index + 3);
        return supplied.CalculateIfMatching(new CeresStationParts(edge));
    }
    public int Count => 41;
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            if (index is >= 3 and <= 13) return lowerEdge[index - 3];
            int x, y, tile;
            if (index < 2)
            {
                // Overlapping top strip extension, then its lower-left small cell.
                x = -10 - 8 - (index == 0 ? 8 : 0); y = -48 + (index == 0 ? 8 : 0);
                tile = CeresStationAtlas.Top - 1 + (index == 0 ? 15 : 0);
            }
            else if (index == 2)
            {
                x = -50 + 8 * 9; y = 16; tile = CeresStationAtlas.Lower + 9;
            }
            else if (index < 19)
            {
                int column = 2 * (18 - index);
                x = -50 + 8 * column; y = 16; tile = CeresStationAtlas.Lower + column;
            }
            else if (index < 25)
            {
                int column = 2 * (24 - index);
                x = -50 + 8 * column; y = 0; tile = CeresStationAtlas.Middle + column;
            }
            else if (index < 32)
            {
                int piece = 31 - index;
                int column = 2 * piece - (piece >= 4 ? 1 : 0);
                x = -50 + 8 * column; y = -16;
                tile = CeresStationAtlas.TwoRowStrip(CeresStationAtlas.UpperMiddle, column);
            }
            else if (index < 38)
            {
                int column = Math.Min(2 * (37 - index), 9);
                x = -42 + 8 * column; y = -32;
                tile = CeresStationAtlas.TwoRowStrip(CeresStationAtlas.Shoulder, column);
            }
            else
            {
                int column = 2 * (40 - index);
                x = -10 + 8 * column; y = -48; tile = CeresStationAtlas.Top + column;
            }
            return new(SnesSpritemapXWord.Create(x, index != 0), unchecked((byte)y),
                SnesObjAttributeWord.Create(tile, 0, 0, 0), true);
        }
    }
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
internal static class CeresStationAtlas
{
    /// <summary>TileE6, top strip at Y=-48 in8C:9150.</summary>
    internal const int Top = 0xe6;
    /// <summary>TileEC, left end of the shoulder strip at Y=-32.</summary>
    internal const int Shoulder = 0xec;
    /// <summary>Tile107, left end of the upper-middle strip at Y=-16.</summary>
    internal const int UpperMiddle = 0x107;
    /// <summary>Tile124, left end of the middle strip at Y=0.</summary>
    internal const int Middle = 0x124;
    /// <summary>Tile140, left end of the lower strip at Y=16.</summary>
    internal const int Lower = 0x140;
    /// <summary>At a sixteen-column boundary, skip the row of lower tile halves.</summary>
    internal static int TwoRowStrip(int start, int column)
    {
        int atlasColumn = (start & 15) + column;
        return (start & ~15) + 32 * (atlasColumn / 16) + atlasColumn % 16;
    }
}