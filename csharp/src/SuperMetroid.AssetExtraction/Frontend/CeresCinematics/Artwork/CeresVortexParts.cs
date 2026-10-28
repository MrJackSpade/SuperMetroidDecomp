using SuperMetroid.Core.Frontend.Intro;
using System.Collections;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;

/// <summary>Shared thirteen-part vortex drawing embedded in two star-field frames.
/// Scene placement is authored content; see ceresStarPointContentReview in the lookup review inventory.</summary>
internal sealed class CeresVortexParts(bool odd, Offset16<SpritePixels> anchor, IReadOnlyList<CompiledSpritePart> stars)
    : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Calculates stock vortex grid parts only when a recognized frame and its supplied part count match.</summary>
    internal static SpriteComposition CalculateIfMatching(CinematicSpritePose shown, SpriteComposition supplied, SpriteComposition? reflectionSource = null)
    {
        bool odd;
        if (shown == CeresFlightSpritePoses.VortexPose(CeresVortexFrame.Even)) odd = false;
        else if (shown == CeresFlightSpritePoses.VortexPose(CeresVortexFrame.Odd)) odd = true;
        else return supplied;
        if (supplied.PartCount != (odd ? 33 : 36)) return supplied;
        int tip = odd ? 3 : 0, core = odd ? 12 : 18;
        var anchorPart = supplied.Part(core + 10);
        var anchor = new Offset16<SpritePixels>(anchorPart.Part.Offset.X, anchorPart.Part.Offset.Y);
        if (anchor.X > 215) return supplied;
        var stars = new CompiledSpritePart[supplied.PartCount - 13];
        int next = 0;
        for (int index = 0; index < supplied.PartCount; index++)
            if (!(index >= tip && index < tip + 2) && !(index >= core && index < core + 11))
                stars[next++] = supplied.Part(index);
        IReadOnlyList<CompiledSpritePart> selectedStars = odd ? CeresStarPointParts.CalculateIfMatching(stars, TileFlip.None) : CeresReflectedStarParts.CalculateIfMatching(reflectionSource, stars);
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
            Offset16<SpritePixels> offset;
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
            Offset16<SpritePixels> position = anchor + offset;
            return new(SpritePart.Create(Offset16<ScreenPixels>.Of(position.X, position.Y), large,
                tile, 0, 0, 0), true);
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
