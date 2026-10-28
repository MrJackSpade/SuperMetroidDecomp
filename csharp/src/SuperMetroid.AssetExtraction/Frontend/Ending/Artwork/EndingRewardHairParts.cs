using SuperMetroid.Core.Frontend.Intro;
using System.Collections;
using Pose = SuperMetroid.AssetExtraction.Frontend.Ending.Artwork.EndingRewardSpriteFrame;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>Hair-release compositions use packed atlas regions and paired forearms.
/// Three authored anchors place the hands beside the hair in held drawings;
/// opposite-side geometry is calculated separately from those pose choices.</summary>
internal sealed class EndingRewardHairParts : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Index in the seven-step hair release sequence used to choose atlas regions and part counts.</summary>
    [IntegerQuantity("hair animation step index")]
    private readonly int stage;
    /// <summary>Supplied forearm positions retained only when they do not follow the symmetric pose rule.</summary>
    private readonly Offset16<SpritePixels>[]? moving;
    /// <summary>First hand placement in a held-hair pose, used to derive the opposite arm when possible.</summary>
    private readonly Offset16<SpritePixels> forearmAnchor;
    /// <summary>Captures authored hand placement for the held poses while calculating the remaining body pieces.</summary>
    private EndingRewardHairParts(int stage, SpriteComposition supplied)
    {
        this.stage = stage;
        if (stage is >= 1 and <= 3)
        {
            var positions = new Offset16<SpritePixels>[2];
            for (int index = 0; index < 2; index++)
            {
                CompiledSpritePart part = supplied.Part(index + 2);
                positions[index] = new Offset16<SpritePixels>(part.Part.Offset.X, part.Part.Offset.Y);
            }
            forearmAnchor = positions[0];
            moving = positions[1] == OppositeForearm(forearmAnchor) ? null : positions;
        }
    }
    /// <summary>Reflects a sixteen-pixel forearm about the body center and raises the opposite cel one pixel.</summary>
    /// <remarks>Reflect the sixteen-pixel piece about x=0.5; the right cel sits one pixel higher.</remarks>
    internal static Offset16<SpritePixels> OppositeForearm(Offset16<SpritePixels> anchor)
        => new Offset16<SpritePixels>(checked((short)(1 - 16 - anchor.X)), checked((short)(anchor.Y - 1)));
    /// <summary>Replaces a matching hair-release frame with its packed atlas composition when the supplied part count agrees.</summary>
    internal static SpriteComposition CalculateIfMatching(CinematicSpritePose shown, SpriteComposition supplied)
    {
        for (int stage = 0; stage < EndingRewardSpriteFrameSeries.HairOpening.Count; stage++)
        {
            Pose pose = EndingRewardSpriteFrameSeries.HairOpening[stage];
            if (shown != EndingRewardSpritePoses.FramePose(pose)) continue;
            if (supplied.PartCount != PartCount(stage)) return supplied;
            return supplied.CalculateIfMatching(new EndingRewardHairParts(stage, supplied));
        }
        return supplied;
    }
    /// <summary>Counts head, arm, and hair pieces for each of the seven release drawings.</summary>
    private static int PartCount(int stage) => stage switch
    {
        0 or 4 => 9,
        >= 1 and <= 3 or 5 => 10,
        6 => 15,
        7 => 13,
        _ => throw new ArgumentOutOfRangeException(nameof(stage)),
    };
    /// <summary>Number of sprite parts in the selected hair-release drawing.</summary>
    public int Count => PartCount(stage);
    /// <summary>Calculates the requested part's atlas tile, anchor offset, and OBJ size for this release stage.</summary>
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
                    if (index < 2) tile = CharacterSheetGrid.Cell(25, 3) + 16 * index; // right head edge
                    else if (index < 4) tile = CharacterSheetGrid.Cell(24, 2) - (index - 2); // top cap
                    else if (index == 4) tile = EndingRewardHairCellDefinitions.InitialHeadCenter;
                    else if (index < 7) tile = CharacterSheetGrid.Cell(26, 9) - 3 * (index - 5); // outer lower edges
                    else tile = CharacterSheetGrid.Cell(24, 8) - 2 * (index - 7); // lower pair
                    originX = index < 5 ? -15 : -64;
                    originY = index < 5 ? -232 : -208;
                    large = index is 4 or >= 7;
                    break;
                case >= 1 and <= 3:
                    if (index < 2) tile = index == (stage == 2 ? 0 : 1)
                        ? EndingRewardHairCellDefinitions.TiedHeadRightEdge : EndingRewardHairCellDefinitions.TiedHead;
                    else if (index < 4) tile = CharacterSheetGrid.Cell(27, 6) + 2 * (index - 2); // paired forearms
                    else if (index == 4) tile = EndingRewardHairCellDefinitions.TiedHeadTopRight;
                    else if (index < 7) tile = CharacterSheetGrid.Cell(16, 0) + 2 * (index - 5); // upper-body pair
                    else tile = CharacterSheetGrid.Cell(18, 3) - (index - 7); // lower edge, right to left
                    originX = -16; originY = -152;
                    large = tile == EndingRewardHairCellDefinitions.TiedHead || index is 2 or 3 or 5 or 6;
                    break;
                case 4:
                    if (index == 0) tile = EndingRewardHairCellDefinitions.ReleaseLeftEdge;
                    else if (index == 1) tile = EndingRewardHairCellDefinitions.ReleaseRightOuter;
                    else if (index == 2) tile = EndingRewardHairCellDefinitions.ReleaseTopRight;
                    else if (index < 6) tile = CharacterSheetGrid.Cell(18, 7) - (index - 3); // lower edge
                    else if (index == 6) tile = EndingRewardHairCellDefinitions.ReleaseLowerLeft;
                    else tile = CharacterSheetGrid.Cell(16, 5) - 32 * (index - 7); // central column
                    originX = -48; originY = -152;
                    large = index is 1 or 2 or >= 6;
                    break;
                case 5:
                    if (index < 3) tile = CharacterSheetGrid.Cell(18, 9) - 16 * index; // lower-left edge
                    else if (index < 5) tile = CharacterSheetGrid.Cell(16, 12) - 32 * (index - 3); // right column
                    else if (index < 7) tile = CharacterSheetGrid.Cell(15, 9) - 16 * (index - 5); // upper-left edge
                    else if (index == 7) tile = EndingRewardHairCellDefinitions.LooseTop;
                    else tile = CharacterSheetGrid.Cell(16, 10) - 32 * (index - 8); // central column
                    originX = -88; originY = -144;
                    large = index is 3 or 4 or >= 7;
                    break;
                default:
                    if (index < 4)
                    {
                        tile = CharacterSheetGrid.Cell(21, 13) - 16 * index; // shared separately packed arm strip
                        originX = -120; originY = -144;
                        large = false;
                    }
                    else if (stage == 6)
                    {
                        if (index == 4) tile = EndingRewardHairCellDefinitions.SweepLowerEdge;
                        else if (index < 8) tile = CharacterSheetGrid.Cell(22, 6) - 16 * (index - 5); // far-right edge
                        else if (index < 10) tile = CharacterSheetGrid.Cell(21, 4) - 32 * (index - 8); // right column
                        else if (index == 10) tile = EndingRewardHairCellDefinitions.SweepTop;
                        else
                        {
                            int cell = index - 11;
                            tile = CharacterSheetGrid.Cell(22, 2) - 32 * (cell / 2) - 2 * (cell % 2);
                        }
                        originX = -16; originY = -192;
                        large = index >= 8;
                    }
                    else
                    {
                        if (index < 7) tile = CharacterSheetGrid.Cell(19, 10) - (index - 4); // top edge
                        else if (index < 9) tile = CharacterSheetGrid.Cell(22, 11) - 32 * (index - 7); // right column
                        else
                        {
                            int cell = index - 9;
                            tile = CharacterSheetGrid.Cell(22, 9) - 32 * (cell / 2) - 2 * (cell % 2);
                        }
                        originX = -72; originY = -192;
                        large = index >= 7;
                    }
                    break;
            }
            int x = originX + 8 * (tile & 15), y = originY + 8 * (tile / 16);
            if (stage is >= 1 and <= 3 && index is 2 or 3)
            {
                Offset16<SpritePixels> forearm = moving is not null ? moving[index - 2]
                    : index == 2 ? forearmAnchor : OppositeForearm(forearmAnchor);
                x = forearm.X;
                y = forearm.Y;
            }
            return new(SpritePart.Create(Offset16<ScreenPixels>.Of(x, y), large,
                tile, 0, 3, 0), true);
        }
    }
    /// <summary>Yields the calculated sprite pieces in native composition order.</summary>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Provides the same ordered sprite traversal through the non-generic interface.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
