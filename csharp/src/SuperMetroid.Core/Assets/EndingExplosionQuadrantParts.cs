using System.Collections;
using SuperMetroid.Core.Hardware;
using Pose = SuperMetroid.Core.Assets.EndingExplosionSpriteDefinitions.Pose;

namespace SuperMetroid.Core.Assets;

/// <summary>$8C:A472/A4B0/A516: glow and two supernova compositions mirror one
/// supplied quadrant about the actor origin. Reflection subtracts sprite size and
/// toggles the corresponding flip. Native quadrant order starts top-right for glow
/// and supernova one, top-left for supernova two, then proceeds clockwise.</summary>
internal sealed class EndingExplosionQuadrantParts : IReadOnlyList<CompiledSpritePart>
{
    private readonly CompiledSpritePart[]? basis;
    private readonly Pose pose;
    private readonly int basisCount;
    private readonly int firstQuadrant;
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

    private enum GlowPiece { TopCap, OuterBody, InnerBody }
    private enum SupernovaPiece { LowerOuterEdge, UpperOuterEdge, InnerTopEdge, OuterTopEdge, Interior }

    /// <summary>Original base-quadrant packing at atlas origins$80/$83/$86.
    /// A24-pixel quadrant uses three overlapping large glow pieces, or four small
    /// edge pieces around one large supernova interior. Native first quadrant is
    /// reflected to the right except for supernova two.</summary>
    internal static CompiledSpritePart StockBasis(Pose pose, int index)
    {
        int atlasOrigin = pose switch
        {
            Pose.Glow => 0x80,
            Pose.SupernovaFirst => 0x83,
            Pose.SupernovaSecond => 0x86,
            _ => throw new ArgumentOutOfRangeException(nameof(pose)),
        };
        if ((uint)index >= (pose == Pose.Glow ? 3 : 5)) throw new ArgumentOutOfRangeException(nameof(index));
        (int column, int row) = pose == Pose.Glow ? (GlowPiece)index switch
        {
            GlowPiece.TopCap => (1, 0),
            GlowPiece.OuterBody => (0, 1),
            GlowPiece.InnerBody => (1, 1),
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        } : (SupernovaPiece)index switch
        {
            SupernovaPiece.LowerOuterEdge => (0, 2),
            SupernovaPiece.UpperOuterEdge => (0, 1),
            SupernovaPiece.InnerTopEdge => (2, 0),
            SupernovaPiece.OuterTopEdge => (1, 0),
            SupernovaPiece.Interior => (1, 1),
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };
        bool large = pose == Pose.Glow || (SupernovaPiece)index == SupernovaPiece.Interior;
        bool right = pose != Pose.SupernovaSecond;
        int x = column * 8 - 24, y = row * 8 - 24;
        if (right) x = -x - (large ? 16 : 8);
        return new(SnesSpritemapXWord.Create(x, large), unchecked((byte)y),
            SnesObjAttributeWord.Create(atlasOrigin + row * 16 + column, 0, 0,
                right ? SnesTileFlipFlags.Horizontal : 0), true);
    }

    internal static SpriteComposition CalculateIfMatching(ushort pointer, SpriteComposition supplied)
    {
        int count, first;
        Pose pose;
        if (pointer == EndingExplosionSpriteDefinitions.Pointer(Pose.Glow)) { count = 3; first = 0; pose = Pose.Glow; }
        else if (pointer == EndingExplosionSpriteDefinitions.Pointer(Pose.SupernovaFirst)) { count = 5; first = 0; pose = Pose.SupernovaFirst; }
        else if (pointer == EndingExplosionSpriteDefinitions.Pointer(Pose.SupernovaSecond)) { count = 5; first = 3; pose = Pose.SupernovaSecond; }
        else return supplied;
        if (supplied.PartCount != count * 4) return supplied;
        var basis = new CompiledSpritePart[count];
        for (int index = 0; index < count; index++)
        {
            CompiledSpritePart part = supplied.Part(index);
            int size = part.X.IsLarge ? 16 : 8;
            // Arbitrary edits outside the signed coordinate mirror domain stay explicit.
            if (-part.X.SignedOffset - size is < -256 or > 255 ||
                -unchecked((sbyte)part.Y) - size is < -128 or > 127) return supplied;
            basis[index] = part;
        }
        return supplied.CalculateIfMatching(new EndingExplosionQuadrantParts(basis, pose, first));
    }

    public int Count => basisCount * 4;
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            int quadrant = (firstQuadrant + index / basisCount) % 4;
            bool reflectX = (quadrant < 2) != (firstQuadrant < 2);
            bool reflectY = quadrant is 1 or 2;
            CompiledSpritePart part = basis is null ? StockBasis(pose, index % basisCount) : basis[index % basisCount];
            int size = part.X.IsLarge ? 16 : 8;
            int x = part.X.SignedOffset, y = unchecked((sbyte)part.Y);
            SnesTileFlipFlags flips = part.Attributes.FlipFlags ^
                (reflectX ? SnesTileFlipFlags.Horizontal : 0) ^ (reflectY ? SnesTileFlipFlags.Vertical : 0);
            return new(SnesSpritemapXWord.Create(reflectX ? -x - size : x, part.X.IsLarge),
                unchecked((byte)(reflectY ? -y - size : y)),
                SnesObjAttributeWord.Create(part.Attributes.TileNumber, part.Attributes.PaletteIndex, part.Attributes.Priority, flips),
                part.InheritPalette);
        }
    }
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
