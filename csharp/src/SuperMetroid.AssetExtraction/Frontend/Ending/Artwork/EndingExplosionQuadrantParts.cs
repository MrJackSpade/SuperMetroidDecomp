using SuperMetroid.Core.Frontend.Intro;
using System.Collections;
using Pose = SuperMetroid.AssetExtraction.Frontend.Ending.Artwork.EndingExplosionSpriteDefinitions.Pose;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>$8C:A472/A4B0/A516: glow and two supernova compositions mirror one
/// supplied quadrant about the actor origin. Reflection subtracts sprite size and
/// toggles the corresponding flip. Native quadrant order starts top-right for glow
/// and supernova one, top-left for supernova two, then proceeds clockwise.</summary>
internal sealed class EndingExplosionQuadrantParts : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Edited first-quadrant parts, absent when the supplied basis matches calculated stock.</summary>
    private readonly CompiledSpritePart[]? basis;
    /// <summary>Glow or supernova pose selecting the corresponding atlas arrangement.</summary>
    private readonly Pose pose;
    /// <summary>Number of pieces mirrored into each of four quadrants.</summary>
    private readonly int basisCount;
    /// <summary>Starting quadrant that preserves native clockwise OAM order.</summary>
    private readonly int firstQuadrant;
    /// <summary>Stores supplied basis parts only when they differ from calculated stock geometry.</summary>
    private EndingExplosionQuadrantParts(CompiledSpritePart[] suppliedBasis, Pose pose, int firstQuadrant)
    {
        this.pose = pose;
        basisCount = suppliedBasis.Length;
        bool stock = true;
        for (int index = 0; stock && index < basisCount; index++)
            stock = suppliedBasis[index] == StockBasis(pose, index);
        basis = stock ? null : suppliedBasis;
        this.firstQuadrant = firstQuadrant;
    }

    /// <summary>Three overlapping pieces in the glow's source quadrant.</summary>
    private enum GlowPiece
    {
        /// <summary>Upper glow cap.</summary>
        TopCap,
        /// <summary>Outer lower glow body.</summary>
        OuterBody,
        /// <summary>Inner lower glow body.</summary>
        InnerBody,
    }
    /// <summary>Four edge cells and one large interior in a supernova quadrant.</summary>
    private enum SupernovaPiece
    {
        /// <summary>Lower part of the outer edge.</summary>
        LowerOuterEdge,
        /// <summary>Upper part of the outer edge.</summary>
        UpperOuterEdge,
        /// <summary>Inner cell along the top edge.</summary>
        InnerTopEdge,
        /// <summary>Outer cell along the top edge.</summary>
        OuterTopEdge,
        /// <summary>Large central supernova tile.</summary>
        Interior,
    }

    /// <summary>The glow quadrant's pieces in part order.</summary>
    private static readonly GlowPiece[] GlowPieces = [GlowPiece.TopCap, GlowPiece.OuterBody, GlowPiece.InnerBody];

    /// <summary>A supernova quadrant's pieces in part order.</summary>
    private static readonly SupernovaPiece[] SupernovaPieces =
    [
        SupernovaPiece.LowerOuterEdge, SupernovaPiece.UpperOuterEdge, SupernovaPiece.InnerTopEdge,
        SupernovaPiece.OuterTopEdge, SupernovaPiece.Interior,
    ];

    /// <summary>Original base-quadrant packing at atlas origins$80/$83/$86.
    /// A24-pixel quadrant uses three overlapping large glow pieces, or four small
    /// edge pieces around one large supernova interior. Native first quadrant is
    /// reflected to the right except for supernova two.</summary>
    internal static CompiledSpritePart StockBasis(Pose pose, int index)
    {
        int atlasOrigin = pose switch
        {
            Pose.Glow => 0x80, // magic-number-audit: allow(PoseOrMovement) - tile atlas indices and strides for cinematic sprite composition.
            Pose.SupernovaFirst => 0x83, // magic-number-audit: allow(PoseOrMovement) - tile atlas indices and strides for cinematic sprite composition.
            Pose.SupernovaSecond => 0x86, // magic-number-audit: allow(PoseOrMovement) - tile atlas indices and strides for cinematic sprite composition.
            Pose.Stars or
                Pose.Silhouette or
                Pose.Afterglow => throw new ArgumentOutOfRangeException(nameof(pose)),
            _ => throw new ArgumentOutOfRangeException(nameof(pose)),
        };
        if ((uint)index >= (pose == Pose.Glow ? GlowPieces.Length : SupernovaPieces.Length)) throw new ArgumentOutOfRangeException(nameof(index));
        Position16<TilemapCells> cell = pose == Pose.Glow ? GlowPieces[index] switch
        {
            GlowPiece.TopCap => new Position16<TilemapCells>(1, 0),
            GlowPiece.OuterBody => new Position16<TilemapCells>(0, 1),
            GlowPiece.InnerBody => new Position16<TilemapCells>(1, 1),
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        } : SupernovaPieces[index] switch
        {
            SupernovaPiece.LowerOuterEdge => new Position16<TilemapCells>(0, 2),
            SupernovaPiece.UpperOuterEdge => new Position16<TilemapCells>(0, 1),
            SupernovaPiece.InnerTopEdge => new Position16<TilemapCells>(2, 0),
            SupernovaPiece.OuterTopEdge => new Position16<TilemapCells>(1, 0),
            SupernovaPiece.Interior => new Position16<TilemapCells>(1, 1),
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };
        int column = cell.X, row = cell.Y;
        bool large = pose == Pose.Glow || SupernovaPieces[index] == SupernovaPiece.Interior;
        bool right = pose != Pose.SupernovaSecond;
        int x = column * 8 - 24, y = row * 8 - 24;
        if (right) x = -x - (large ? 16 : 8);
        return new(SpritePart.Create(Offset16<ScreenPixels>.Of(x, y), large,
            atlasOrigin + row * 16 + column, 0, 0, right ? TileFlip.Horizontal : 0), true);
    }

    /// <summary>Uses a supplied first quadrant to calculate matching glow or supernova reflections.</summary>
    internal static SpriteComposition CalculateIfMatching(CinematicSpritePose shown, SpriteComposition supplied)
    {
        int count, first;
        Pose pose;
        if (shown == EndingExplosionSpritePoses.PoseOf(Pose.Glow)) { count = 3; first = 0; pose = Pose.Glow; }
        else if (shown == EndingExplosionSpritePoses.PoseOf(Pose.SupernovaFirst)) { count = 5; first = 0; pose = Pose.SupernovaFirst; }
        else if (shown == EndingExplosionSpritePoses.PoseOf(Pose.SupernovaSecond)) { count = 5; first = 3; pose = Pose.SupernovaSecond; }
        else return supplied;
        if (supplied.PartCount != count * 4) return supplied;
        var basis = new CompiledSpritePart[count];
        for (int index = 0; index < count; index++)
        {
            CompiledSpritePart part = supplied.Part(index);
            int size = part.Part.Large ? 16 : 8;
            // Arbitrary edits outside the signed coordinate mirror domain stay explicit.
            if (-part.Part.Offset.X - size is < -256 or > 255 ||
                -part.Part.Offset.Y - size is < -128 or > 127) return supplied;
            basis[index] = part;
        }
        return supplied.CalculateIfMatching(new EndingExplosionQuadrantParts(basis, pose, first));
    }

    /// <summary>Four times the number of source-quadrant pieces.</summary>
    public int Count => basisCount * 4;
    /// <summary>Reflects a source piece about the actor origin for its native OAM quadrant.</summary>
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            int quadrant = (firstQuadrant + index / basisCount) % 4;
            bool reflectX = (quadrant < 2) != (firstQuadrant < 2);
            bool reflectY = quadrant is 1 or 2;
            CompiledSpritePart part = basis is null ? StockBasis(pose, index % basisCount) : basis[index % basisCount];
            int size = part.Part.Large ? 16 : 8;
            int x = part.Part.Offset.X, y = part.Part.Offset.Y;
            TileFlip flips = part.Part.Flip ^
                (reflectX ? TileFlip.Horizontal : 0) ^ (reflectY ? TileFlip.Vertical : 0);
            return new(SpritePart.Create(Offset16<ScreenPixels>.Of(reflectX ? -x - size : x, reflectY ? -y - size : y), part.Part.Large,
                part.Part.Character, part.Part.Palette, part.Part.Priority, flips),
                part.InheritPalette);
        }
    }
    /// <summary>Enumerates all four reflected quadrants in native draw order.</summary>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Exposes the same quadrant order through the non-generic collection interface.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
