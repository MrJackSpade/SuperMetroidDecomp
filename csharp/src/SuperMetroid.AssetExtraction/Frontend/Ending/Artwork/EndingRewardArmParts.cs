using SuperMetroid.Core.Frontend.Intro;
using System.Collections;
using Pose = SuperMetroid.AssetExtraction.Frontend.Ending.Artwork.EndingRewardSpriteFrame;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>Reward arms calculate a shared shoulder and stock tile selection.
/// Independent positions place hand-drawn pieces in authored thumbs-up poses;
/// the fixed-origin pose consumer supplies no joint parameters to derive them.
/// Shoulder geometry and the split-pose grid are calculated separately.</summary>
internal sealed class EndingRewardArmParts : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Independent authored offsets when the pieces do not match the calculated split grid.</summary>
    private readonly Offset16<SpritePixels>[]? arm;
    /// <summary>Anchor for the three-piece split pose's two-column arrangement.</summary>
    private readonly Offset16<SpritePixels> splitOrigin;
    /// <summary>Tile choices that differ from stock arm pieces.</summary>
    private readonly int[]? editedTiles;
    /// <summary>Reward pose selecting the stock arm tile variant.</summary>
    private readonly Pose pose;
    /// <summary>Authored shoulder anchor shared by its large body and two caps.</summary>
    private readonly Offset16<SpritePixels> shoulder;
    /// <summary>Whether the second reward frame uses three arm pieces instead of two.</summary>
    private readonly bool splitArm;
    /// <summary>Separates authored part placement from calculable split-grid and stock-tile relationships.</summary>
    private EndingRewardArmParts(SpriteComposition supplied, Pose pose)
    {
        this.pose = pose;
        splitArm = pose == Pose.SamusArmFromEndingFrame2;
        int count = splitArm ? 3 : 2;
        var positions = new Offset16<SpritePixels>[count];
        var tiles = new int[count];
        bool stock = true;
        for (int index = 0; index < count; index++)
        {
            CompiledSpritePart part = supplied.Part(index);
            positions[index] = new(part.Part.Offset.X, part.Part.Offset.Y);
            tiles[index] = part.Part.Character;
            stock &= tiles[index] == StockTile(pose, index);
        }
        bool grid = splitArm;
        if (splitArm)
        {
            splitOrigin = positions[1];
            for (int index = 0; grid && index < count; index++)
            {
                grid = positions[index] == splitOrigin + SplitOffset(index);
            }
        }
        arm = grid ? null : positions;
        editedTiles = stock ? null : tiles;
        CompiledSpritePart shoulderPart = supplied.Part(count);
        shoulder = new(shoulderPart.Part.Offset.X, shoulderPart.Part.Offset.Y);
    }

    /// <summary>Places the split pose's two small column pieces and adjacent large piece.</summary>
    internal static Offset16<SpritePixels> SplitOffset(int index)
    {
        if ((uint)index >= 3) throw new ArgumentOutOfRangeException(nameof(index));
        // Small column bottom-to-top, then the adjacent large piece.
        return new(checked((short)(index == 2 ? 8 : 0)), checked((short)(index == 0 ? 8 : 0)));
    }

    /// <summary>Selects the arm tile for an initial, split, or paired-variant reward pose.</summary>
    internal static int StockTile(Pose pose, int index)
    {
        int stage = pose - Pose.SamusArmFromEndingFrame1;
        if ((uint)stage >= 8) throw new ArgumentOutOfRangeException(nameof(pose));
        if ((uint)index >= (stage == 1 ? 3 : 2)) throw new ArgumentOutOfRangeException(nameof(index));
        if (stage == 0)
            return index == 0 ? EndingRewardArmCellDefinitions.InitialLower : EndingRewardArmCellDefinitions.InitialUpper;
        if (stage == 1)
            return index == 2 ? EndingRewardArmCellDefinitions.SplitLarge : EndingRewardArmCellDefinitions.SplitSmallTop + 16 * (1 - index);
        // Two adjacent two-column variants; each is used by three consecutive poses.
        int variant = (stage - 2) / 3;
        return EndingRewardArmCellDefinitions.PairedUpper + 2 * variant + (index == 0 ? 32 : 0);
    }
    /// <summary>Uses calculated arm and shoulder parts when a supplied reward frame matches their native form.</summary>
    internal static SpriteComposition CalculateIfMatching(CinematicSpritePose shown, SpriteComposition supplied)
    {
        foreach (Pose pose in EndingRewardSpriteFrameSeries.Arm)
        {
            if (shown != EndingRewardSpritePoses.FramePose(pose)) continue;
            bool split = pose == Pose.SamusArmFromEndingFrame2;
            int armCount = split ? 3 : 2;
            if (supplied.PartCount != armCount + 3) return supplied;
            CompiledSpritePart anchor = supplied.Part(armCount);
            if (anchor.Part.Offset.X > 247 || anchor.Part.Offset.Y < -120) return supplied;
            return supplied.CalculateIfMatching(new EndingRewardArmParts(supplied, pose));
        }
        return supplied;
    }
    /// <summary>Number of authored arm pieces before the three shoulder parts.</summary>
    private int ArmCount => splitArm ? 3 : 2;
    /// <summary>Arm and shoulder part count for the selected reward pose.</summary>
    public int Count => ArmCount + 3;
    /// <summary>Calculates one native OAM part, preserving independent arm offsets and tile edits.</summary>
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            int tile;
            Offset16<SpritePixels> position;
            bool large;
            if (index < ArmCount)
            {
                position = arm is null ? splitOrigin + SplitOffset(index) : arm[index];
                tile = editedTiles is null ? StockTile(pose, index) : editedTiles[index];
                large = !splitArm || index == 2;
            }
            else
            {
                int piece = index - ArmCount;
                large = piece == 0;
                int column = piece == 1 ? 1 : 0;
                tile = EndingRewardArmCellDefinitions.ShoulderCap + (large ? 16 : column);
                position = shoulder + new Offset16<SpritePixels>(checked((short)(8 * column)), checked((short)(large ? 0 : -8)));
            }
            return new(SpritePart.Create(Offset16<ScreenPixels>.Of(position.X, position.Y), large,
                tile, 0, 3, 0), true);
        }
    }
    /// <summary>Enumerates the arm pieces followed by the large shoulder and two caps.</summary>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Uses the typed part enumeration for nongeneric callers.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
