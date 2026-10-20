using System.Collections;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

/// <summary>Shared thirteen-part vortex drawing embedded in two star-field frames.
/// Scene placement is authored content; see ceresStarPointContentReview in the lookup review inventory.</summary>
internal sealed class CeresVortexParts(bool odd, Offset16<OamPixels> anchor, IReadOnlyList<CompiledSpritePart> stars)
    : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>The two alternating bank-$8C vortex drawings this calculator owns among the Ceres flight visuals.</summary>
    private enum VortexFrame : ushort
    {
        /// <summary>8C:8FE7, SpaceSpritemaps_CeresPurpleVortexFrame1.</summary>
        Even = CeresFlightSpriteDefinitions.VortexEven,
        /// <summary>8C:93D1, SpaceSpritemaps_CeresPurpleVortexFrame2.</summary>
        Odd = CeresFlightSpriteDefinitions.VortexOdd,
    }

    /// <summary>Calculates stock vortex grid parts only when a recognized frame and its supplied part count match.</summary>
    internal static SpriteComposition CalculateIfMatching(ushort pointer, SpriteComposition supplied, SpriteComposition? reflectionSource = null)
    {
        if (!Enum.IsDefined((VortexFrame)pointer)) return supplied;
        bool odd = (VortexFrame)pointer switch
        {
            VortexFrame.Even => false,
            VortexFrame.Odd => true,
            var frame => throw new InvalidOperationException($"Undefined {nameof(VortexFrame)} {(int)frame}."),
        };
        if (supplied.PartCount != (odd ? 33 : 36)) return supplied;
        int tip = odd ? 3 : 0, core = odd ? 12 : 18;
        var anchorPart = supplied.Part(core + 10);
        var anchor = new Offset16<OamPixels>(checked((short)anchorPart.X.SignedOffset), (short)anchorPart.Y.SignedOffset);
        if (anchor.X > 215) return supplied;
        var stars = new CompiledSpritePart[supplied.PartCount - 13];
        int next = 0;
        for (int index = 0; index < supplied.PartCount; index++)
            if (!(index >= tip && index < tip + 2) && !(index >= core && index < core + 11))
                stars[next++] = supplied.Part(index);
        IReadOnlyList<CompiledSpritePart> selectedStars = odd ? CeresStarPointParts.CalculateIfMatching(stars, SnesTileFlipFlags.None) : CeresReflectedStarParts.CalculateIfMatching(reflectionSource, stars);
        return supplied.CalculateIfMatching(new CeresVortexParts(odd, anchor, selectedStars));
    }
    /// <summary>Counts thirteen vortex parts plus the remaining stars for the selected frame.</summary>
    public int Count => odd ? 33 : 36;
    /// <summary>Calculates a vortex grid part or returns a preserved star part in native OAM order.</summary>
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            int tip = odd ? 3 : 0, core = odd ? 12 : 18;
            Offset16<OamPixels> offset;
            int tile;
            bool large;
            if (index >= tip && index < tip + 2)
            {
                large = false;
                offset = new(checked((short)(32 + 8 * (1 - (index - tip)))), 48);
                tile = CeresVortexCellDefinitions.LowerEdge;
            }
            else if (index >= core && index < core + 11)
            {
                large = true;
                int cell = 10 - (index - core);
                offset = new(checked((short)(16 * (cell % 3))), checked((short)(16 * (cell / 3))));
                tile = CeresVortexCellDefinitions.Core + 32 * (cell / 8) + 2 * (cell % 8);
            }
            else return stars[index - (index >= tip + 2 ? 2 : 0) - (index >= core + 11 ? 11 : 0)];
            Offset16<OamPixels> position = anchor + offset;
            return new(SnesSpritemapXWord.Create(position.X, large), SnesSpritemapYByte.Create(position.Y),
                SnesObjAttributeWord.Create(tile, 0, 0, 0), true);
        }
    }
    /// <summary>Enumerates the alternating frame's vortex and star parts in OAM order.</summary>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Exposes the same frame parts through the non-generic collection interface.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
