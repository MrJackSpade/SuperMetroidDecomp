using System.Collections;
using SuperMetroid.Core.Hardware;
using Pose = SuperMetroid.Core.Assets.EndingRewardSpriteFrame;

namespace SuperMetroid.Core.Assets;

/// <summary>Hair-release compositions use packed atlas regions and paired forearms.
/// Three authored anchors place the hands beside the hair in held drawings;
/// opposite-side geometry is calculated separately from those pose choices.</summary>
internal sealed class EndingRewardHairParts : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Zero-based stage selector used to calculate the matching hair-release composition.</summary>
    private readonly int stage;
    /// <summary>Authored forearm positions when the pose's two pieces do not match the mirrored default geometry.</summary>
    private readonly (int X, int Y)[]? moving;
    /// <summary>Position of the authored forearm used as the anchor for its opposite-side counterpart.</summary>
    private readonly (int X, int Y) forearmAnchor;

    /// <summary>Creates the calculated piece list for one recognized hair-release stage.</summary>
    /// <param name="stage">Zero-based position among the eight consecutive hair-release poses.</param>
    /// <param name="supplied">Imported composition whose matching forearm geometry is preserved when it differs from the default.</param>
    private EndingRewardHairParts(int stage, SpriteComposition supplied)
    {
        this.stage = stage;
        if (stage is >= 1 and <= 3)
        {
            var positions = new (int X, int Y)[2];
            for (int index = 0; index < 2; index++)
            {
                CompiledSpritePart part = supplied.Part(index + 2);
                positions[index] = (part.X.SignedOffset, unchecked((sbyte)part.Y));
            }
            forearmAnchor = positions[0];
            moving = positions[1] == OppositeForearm(forearmAnchor) ? null : positions;
        }
    }
    /// <summary>Mirrors a sixteen-pixel forearm around the vertical axis and offsets its counterpart one pixel upward.</summary>
    /// <param name="anchor">Position of the authored forearm.</param>
    /// <returns>Position of the opposite forearm.</returns>
    internal static (int X, int Y) OppositeForearm((int X, int Y) anchor)
        // Reflect the sixteen-pixel piece about x=0.5; the right cel sits one pixel higher.
        => (1 - 16 - anchor.X, anchor.Y - 1);
    /// <summary>Replaces recognized hair-release compositions with calculated atlas parts when their supplied part count matches.</summary>
    /// <param name="pointer">Native spritemap pointer identifying the pose.</param>
    /// <param name="supplied">Imported composition retained for unrecognized poses or incompatible part counts.</param>
    /// <returns>The calculated hair composition for a matching pose, otherwise the original supplied composition.</returns>
    internal static SpriteComposition CalculateIfMatching(ushort pointer, SpriteComposition supplied)
    {
        for (int stage = 0; stage < 8; stage++)
        {
            var pose = (Pose)((int)Pose.SuitlessSamusOpeningHairFrame1 + stage);
            if (pointer != EndingRewardSpriteDefinitions.FramePointer(pose)) continue;
            if (supplied.PartCount != PartCount(stage)) return supplied;
            return supplied.CalculateIfMatching(new EndingRewardHairParts(stage, supplied));
        }
        return supplied;
    }
    /// <summary>Returns the authored number of sprite pieces for a hair-release stage.</summary>
    /// <param name="stage">Zero-based stage selector from zero through seven.</param>
    /// <returns>The number of pieces emitted by the stage's spritemap.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The stage is outside the eight recognized poses.</exception>
    private static int PartCount(int stage) => stage switch
    {
        0 or 4 => 9,
        >= 1 and <= 3 or 5 => 10,
        6 => 15,
        7 => 13,
        _ => throw new ArgumentOutOfRangeException(nameof(stage)),
    };
    /// <summary>Gets the piece count required by this stage's native spritemap.</summary>
    public int Count => PartCount(stage);

    /// <summary>Gets the calculated atlas piece at its native composition index.</summary>
    /// <param name="index">Zero-based piece index within this stage.</param>
    /// <returns>The positioned and attributed sprite piece.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside this stage's piece range.</exception>
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            int tile, originX, originY;
            bool large;
            switch (stage)
            {
                case 0:
                    if (index < 2) tile = 0x193 + 16 * index; // right head edge
                    else if (index < 4) tile = 0x182 - (index - 2); // top cap
                    else if (index == 4) tile = EndingRewardHairAtlas.InitialHeadCenter;
                    else if (index < 7) tile = 0x1a9 - 3 * (index - 5); // outer lower edges
                    else tile = 0x188 - 2 * (index - 7); // lower pair
                    originX = index < 5 ? -15 : -64;
                    originY = index < 5 ? -232 : -208;
                    large = index == 4 || index >= 7;
                    break;
                case >= 1 and <= 3:
                    if (index < 2) tile = index == (stage == 2 ? 0 : 1)
                        ? EndingRewardHairAtlas.TiedHeadRightEdge : EndingRewardHairAtlas.TiedHead;
                    else if (index < 4) tile = 0x1b6 + 2 * (index - 2); // paired forearms
                    else if (index == 4) tile = EndingRewardHairAtlas.TiedHeadTopRight;
                    else if (index < 7) tile = 0x100 + 2 * (index - 5); // upper-body pair
                    else tile = 0x123 - (index - 7); // lower edge, right to left
                    originX = -16; originY = -152;
                    large = tile == EndingRewardHairAtlas.TiedHead || index is 2 or 3 or 5 or 6;
                    break;
                case 4:
                    if (index == 0) tile = EndingRewardHairAtlas.ReleaseLeftEdge;
                    else if (index == 1) tile = EndingRewardHairAtlas.ReleaseRightOuter;
                    else if (index == 2) tile = EndingRewardHairAtlas.ReleaseTopRight;
                    else if (index < 6) tile = 0x127 - (index - 3); // lower edge
                    else if (index == 6) tile = EndingRewardHairAtlas.ReleaseLowerLeft;
                    else tile = 0x105 - 32 * (index - 7); // central column
                    originX = -48; originY = -152;
                    large = index is 1 or 2 or >= 6;
                    break;
                case 5:
                    if (index < 3) tile = 0x129 - 16 * index; // lower-left edge
                    else if (index < 5) tile = 0x10c - 32 * (index - 3); // right column
                    else if (index < 7) tile = 0xf9 - 16 * (index - 5); // upper-left edge
                    else if (index == 7) tile = EndingRewardHairAtlas.LooseTop;
                    else tile = 0x10a - 32 * (index - 8); // central column
                    originX = -88; originY = -144;
                    large = index is 3 or 4 or >= 7;
                    break;
                default:
                    if (index < 4)
                    {
                        tile = 0x15d - 16 * index; // shared separately packed arm strip
                        originX = -120; originY = -144;
                        large = false;
                    }
                    else if (stage == 6)
                    {
                        if (index == 4) tile = EndingRewardHairAtlas.SweepLowerEdge;
                        else if (index < 8) tile = 0x166 - 16 * (index - 5); // far-right edge
                        else if (index < 10) tile = 0x154 - 32 * (index - 8); // right column
                        else if (index == 10) tile = EndingRewardHairAtlas.SweepTop;
                        else
                        {
                            int cell = index - 11;
                            tile = 0x162 - 32 * (cell / 2) - 2 * (cell % 2);
                        }
                        originX = -16; originY = -192;
                        large = index >= 8;
                    }
                    else
                    {
                        if (index < 7) tile = 0x13a - (index - 4); // top edge
                        else if (index < 9) tile = 0x16b - 32 * (index - 7); // right column
                        else
                        {
                            int cell = index - 9;
                            tile = 0x169 - 32 * (cell / 2) - 2 * (cell % 2);
                        }
                        originX = -72; originY = -192;
                        large = index >= 7;
                    }
                    break;
            }
            int x = originX + 8 * (tile & 15), y = originY + 8 * (tile / 16);
            if (stage is >= 1 and <= 3 && index is 2 or 3)
                (x, y) = moving is not null ? moving[index - 2]
                    : index == 2 ? forearmAnchor : OppositeForearm(forearmAnchor);
            return new(SnesSpritemapXWord.Create(x, large), unchecked((byte)y),
                SnesObjAttributeWord.Create(tile, 0, 3, 0), true);
        }
    }
    /// <summary>Enumerates the calculated pieces in the order expected by the native spritemap composition.</summary>
    /// <returns>An enumerator over this stage's positioned sprite pieces.</returns>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Returns a non-generic enumerator over this sequence.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>Individually ordered pieces outside the hair-pose atlas traversals.</summary>
internal static class EndingRewardHairAtlas
{
    /// <summary>Tile$191, initial pose's large head center.</summary>
    internal const int InitialHeadCenter = 0x191;
    /// <summary>Tile$E1, shared tied-hair head in poses2..4.</summary>
    internal const int TiedHead = 0xe1;
    /// <summary>Tile$F3, small right head edge in poses2..4.</summary>
    internal const int TiedHeadRightEdge = 0xf3;
    /// <summary>Tile$E3, small top-right head edge in poses2..4.</summary>
    internal const int TiedHeadTopRight = 0xe3;
    /// <summary>Tile$104, small left boundary in the release pose.</summary>
    internal const int ReleaseLeftEdge = 0x104;
    /// <summary>Tile$107, large right outer piece in the release pose.</summary>
    internal const int ReleaseRightOuter = 0x107;
    /// <summary>Tile$E6, large top-right piece in the release pose.</summary>
    internal const int ReleaseTopRight = 0xe6;
    /// <summary>Tile$114, large lower-left release piece.</summary>
    internal const int ReleaseLowerLeft = 0x114;
    /// <summary>Tile$DA, top piece in the first loose-hair pose.</summary>
    internal const int LooseTop = 0xda;
    /// <summary>Tile$174, small lower edge of the outward sweep.</summary>
    internal const int SweepLowerEdge = 0x174;
    /// <summary>Tile$131, top piece of the outward sweep.</summary>
    internal const int SweepTop = 0x131;
}
