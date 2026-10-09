using System.Collections;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Shared thirteen-part vortex drawing embedded in two star-field frames.
/// Scene placement is authored content; see ceresStarPointContentReview in the lookup review inventory.</summary>
/// <param name="odd">Selects the odd-frame arrangement, whose tip, core, and total part count differ from the even frame.</param>
/// <param name="anchorX">Signed horizontal origin shared by the generated tip and core tiles.</param>
/// <param name="anchorY">Signed vertical origin shared by the generated tip and core tiles.</param>
/// <param name="stars">The already-selected star parts interleaved with the generated vortex geometry.</param>
internal sealed class CeresVortexParts(bool odd, int anchorX, int anchorY, IReadOnlyList<CompiledSpritePart> stars)
    : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Replaces a recognized vortex frame's hard-coded center with generated atlas geometry and selected star parts.</summary>
    /// <param name="pointer">The native spritemap pointer identifying an even or odd vortex frame.</param>
    /// <param name="supplied">The decoded frame whose tip, core anchor, and remaining stars are used as source data.</param>
    /// <param name="reflectionSource">The source composition needed to select reflected stars for the even frame.</param>
    /// <returns>The generated composition for a recognized, matching vortex frame; otherwise, <paramref name="supplied"/> unchanged.</returns>
    internal static SpriteComposition CalculateIfMatching(ushort pointer, SpriteComposition supplied, SpriteComposition? reflectionSource = null)
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
        IReadOnlyList<CompiledSpritePart> selectedStars = odd ? CeresStarPointParts.CalculateIfMatching(stars, false) : CeresReflectedStarParts.CalculateIfMatching(reflectionSource, stars);
        return supplied.CalculateIfMatching(new CeresVortexParts(odd, anchorX, unchecked((sbyte)anchor.Y), selectedStars));
    }

    /// <summary>Gets the number of parts in the selected even or odd vortex frame.</summary>
    public int Count => odd ? 33 : 36;

    /// <summary>Gets the indexed part, synthesizing vortex tiles and drawing the remaining selected stars in source order.</summary>
    /// <param name="index">Zero-based position in this frame's ordered sprite-part sequence.</param>
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

    /// <summary>Enumerates all parts in the order expected by the decoded vortex frame.</summary>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>Tile indices used by the generated Ceres vortex core and lower edge.</summary>
internal static class CeresVortexAtlas
{
    /// <summary>Tile1C0, first large vortex tile; eleven large pieces are packed
    /// eight per two-row atlas strip and rendered as a three-column grid.</summary>
    internal const int Core = 0x1c0;
    /// <summary>Tile1E6, small lower-edge drawing repeated across the final grid cell.</summary>
    internal const int LowerEdge = 0x1e6;
}
