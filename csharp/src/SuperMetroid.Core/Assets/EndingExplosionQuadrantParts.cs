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
    /// <summary>Explicit first-quadrant artwork when the compiled source differs from the stock atlas layout; null selects stock parts.</summary>
    private readonly CompiledSpritePart[]? basis;
    /// <summary>Ending-explosion pose whose stock atlas layout or reflected variations are being exposed.</summary>
    private readonly Pose pose;
    /// <summary>Number of source parts in one quadrant before the four-way reflection is applied.</summary>
    private readonly int basisCount;
    /// <summary>Native quadrant ordinal that determines the orientation and ordering of the reflected copies.</summary>
    private readonly int firstQuadrant;

    /// <summary>Creates a four-quadrant view over one supplied source quadrant.</summary>
    /// <param name="suppliedBasis">Parts from the compiled first quadrant, retained only when they differ from stock geometry.</param>
    /// <param name="pose">Glow or supernova composition, used to select stock tiles and quadrant order.</param>
    /// <param name="firstQuadrant">Native quadrant index occupied by the supplied source parts.</param>
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

    /// <summary>Atlas roles of the three large overlapping pieces that form a glow quadrant.</summary>
    private enum GlowPiece
    {
        /// <summary>Upper tile that caps the glow shape.</summary>
        TopCap,
        /// <summary>Outer lower tile that forms the body's border.</summary>
        OuterBody,
        /// <summary>Inner lower tile overlapping the outer body.</summary>
        InnerBody
    }

    /// <summary>Atlas roles of the four small border pieces and large center tile in a supernova quadrant.</summary>
    private enum SupernovaPiece
    {
        /// <summary>Lower outside edge tile of the quadrant.</summary>
        LowerOuterEdge,
        /// <summary>Upper outside edge tile of the quadrant.</summary>
        UpperOuterEdge,
        /// <summary>Inner edge along the top of the quadrant.</summary>
        InnerTopEdge,
        /// <summary>Outer edge along the top of the quadrant.</summary>
        OuterTopEdge,
        /// <summary>Large interior tile shared by the quadrant's four border pieces.</summary>
        Interior
    }

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

    /// <summary>Replaces a matching glow or supernova composition with a four-quadrant reflected view when its parts are mirrorable.</summary>
    /// <param name="pointer">Bank-$8C frame pointer used to select the pose and native first-quadrant order.</param>
    /// <param name="supplied">Compiled sprite composition whose part count and coordinates must match the quadrant layout.</param>
    /// <returns>A composition that generates the reflected quadrants, or <paramref name="supplied"/> when the frame is unrelated or outside the mirror domain.</returns>
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

    /// <summary>Total parts across all four reflected quadrants.</summary>
    public int Count => basisCount * 4;

    /// <summary>Gets one compiled sprite part in native quadrant order, reflecting position and flip flags as needed.</summary>
    /// <param name="index">Zero-based part index across all four quadrants.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is outside this composition.</exception>
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
    /// <summary>Enumerates the four quadrants in the order used by the native frame.</summary>
    /// <returns>An iterator over reflected compiled sprite parts.</returns>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Returns a non-generic enumerator over this sequence.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
