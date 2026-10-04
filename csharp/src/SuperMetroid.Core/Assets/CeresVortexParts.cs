using System.Collections;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Shared thirteen-part vortex drawing embedded in two star-field frames.
/// Scene anchor and independent star placements remain separately unresolved.</summary>
internal sealed class CeresVortexParts(bool odd, int anchorX, int anchorY, CompiledSpritePart[] stars)
    : IReadOnlyList<CompiledSpritePart>
{
    internal static SpriteComposition CalculateIfMatching(ushort pointer, SpriteComposition supplied)
    {
        if (pointer is not (CeresFlightSpriteDefinitions.VortexEven or CeresFlightSpriteDefinitions.VortexOdd)) return supplied;
        bool odd = pointer == CeresFlightSpriteDefinitions.VortexOdd;
        if (supplied.PartCount != (odd ? 33 : 36)) return supplied;
        int tip = odd ? 3 : 0, core = odd ? 12 : 18;
        var anchor = supplied.Part(core + 10);
        int anchorX = anchor.X.SignedOffset;
        if (anchorX > 215) return supplied;
        var stars = new CompiledSpritePart[supplied.PartCount - 13];
        int next = 0;
        for (int index = 0; index < supplied.PartCount; index++)
            if (!(index >= tip && index < tip + 2) && !(index >= core && index < core + 11))
                stars[next++] = supplied.Part(index);
        return supplied.CalculateIfMatching(new CeresVortexParts(odd, anchorX, unchecked((sbyte)anchor.Y), stars));
    }
    public int Count => odd ? 33 : 36;
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            int tip = odd ? 3 : 0, core = odd ? 12 : 18;
            int x, y, tile;
            bool large;
            if (index >= tip && index < tip + 2)
            {
                large = false;
                x = 32 + 8 * (1 - (index - tip)); y = 48;
                tile = CeresVortexAtlas.LowerEdge;
            }
            else if (index >= core && index < core + 11)
            {
                large = true;
                int cell = 10 - (index - core);
                x = 16 * (cell % 3); y = 16 * (cell / 3);
                tile = CeresVortexAtlas.Core + 32 * (cell / 8) + 2 * (cell % 8);
            }
            else return stars[index - (index >= tip + 2 ? 2 : 0) - (index >= core + 11 ? 11 : 0)];
            return new(SnesSpritemapXWord.Create(anchorX + x, large), unchecked((byte)(anchorY + y)),
                SnesObjAttributeWord.Create(tile, 0, 0, 0), true);
        }
    }
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
internal static class CeresVortexAtlas
{
    /// <summary>Tile1C0, first large vortex tile; eleven large pieces are packed
    /// eight per two-row atlas strip and rendered as a three-column grid.</summary>
    internal const int Core = 0x1c0;
    /// <summary>Tile1E6, small lower-edge drawing repeated across the final grid cell.</summary>
    internal const int LowerEdge = 0x1e6;
}