using System.Collections;
using SuperMetroid.Core.Hardware;
using Pose = SuperMetroid.Core.Assets.EndingRewardSpriteFrame;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Assets;

/// <summary>Reward arms calculate a shared shoulder and stock tile selection.
/// Independent positions place hand-drawn pieces in authored thumbs-up poses;
/// the fixed-origin pose consumer supplies no joint parameters to derive them.
/// Shoulder geometry and the split-pose grid are calculated separately.</summary>
internal sealed class EndingRewardArmParts : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Independent authored offsets when the pieces do not match the calculated split grid.</summary>
    private readonly Offset16<OamPixels>[]? arm;
    /// <summary>Anchor for the three-piece split pose's two-column arrangement.</summary>
    private readonly Offset16<OamPixels> splitOrigin;
    /// <summary>Tile choices that differ from stock arm pieces.</summary>
    private readonly int[]? editedTiles;
    /// <summary>Reward pose selecting the stock arm tile variant.</summary>
    private readonly Pose pose;
    /// <summary>Authored shoulder anchor shared by its large body and two caps.</summary>
    private readonly Offset16<OamPixels> shoulder;
    /// <summary>Whether the second reward frame uses three arm pieces instead of two.</summary>
    private readonly bool splitArm;
    /// <summary>Separates authored part placement from calculable split-grid and stock-tile relationships.</summary>
    private EndingRewardArmParts(SpriteComposition supplied, Pose pose)
    {
        this.pose = pose;
        splitArm = pose == Pose.SamusArmFromEndingFrame2;
        int count = splitArm ? 3 : 2;
        var positions = new Offset16<OamPixels>[count];
        var tiles = new int[count];
        bool stock = true;
        for (int index = 0; index < count; index++)
        {
            CompiledSpritePart part = supplied.Part(index);
            positions[index] = new(checked((short)part.X.SignedOffset), unchecked((sbyte)part.Y));
            tiles[index] = part.Attributes.TileNumber;
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
        shoulder = new(checked((short)shoulderPart.X.SignedOffset), unchecked((sbyte)shoulderPart.Y));
    }

    /// <summary>Places the split pose's two small column pieces and adjacent large piece.</summary>
    internal static Offset16<OamPixels> SplitOffset(int index)
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
            return index == 0 ? EndingRewardArmAtlas.InitialLower : EndingRewardArmAtlas.InitialUpper;
        if (stage == 1)
            return index == 2 ? EndingRewardArmAtlas.SplitLarge : EndingRewardArmAtlas.SplitSmallTop + 16 * (1 - index);
        // Two adjacent two-column variants; each is used by three consecutive poses.
        int variant = (stage - 2) / 3;
        return EndingRewardArmAtlas.PairedUpper + 2 * variant + (index == 0 ? 32 : 0);
    }
    /// <summary>Uses calculated arm and shoulder parts when a supplied reward frame matches their native form.</summary>
    internal static SpriteComposition CalculateIfMatching(ushort pointer, SpriteComposition supplied)
    {
        foreach (Pose pose in EndingRewardSpriteFrameSeries.Arm)
        {
            if (pointer != EndingRewardSpriteDefinitions.FramePointer(pose)) continue;
            bool split = pose == Pose.SamusArmFromEndingFrame2;
            int armCount = split ? 3 : 2;
            if (supplied.PartCount != armCount + 3) return supplied;
            CompiledSpritePart anchor = supplied.Part(armCount);
            if (anchor.X.SignedOffset > 247 || unchecked((sbyte)anchor.Y) < -120) return supplied;
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
            Offset16<OamPixels> position;
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
                tile = EndingRewardArmAtlas.ShoulderCap + (large ? 16 : column);
                position = shoulder + new Offset16<OamPixels>(checked((short)(8 * column)), checked((short)(large ? 0 : -8)));
            }
            return new(SnesSpritemapXWord.Create(position.X, large), unchecked((byte)position.Y),
                SnesObjAttributeWord.Create(tile, 0, 3, 0), true);
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

/// <summary>Native OBJ tile anchors shared by the ending reward arm poses.</summary>
internal static class EndingRewardArmAtlas
{
    /// <summary>Tile$EE, initial pose's lower large arm piece.</summary>
    internal const int InitialLower = 0xee;
    /// <summary>Tile$E9, initial pose's upper large arm piece.</summary>
    internal const int InitialUpper = 0xe9;
    /// <summary>Tile$10D, top small piece of the split pose; next row$11D is below it.</summary>
    internal const int SplitSmallTop = 0x10d;
    /// <summary>Tile$10E, large piece adjacent to the split pose's small column.</summary>
    internal const int SplitLarge = 0x10e;
    /// <summary>Tile$109, first upper piece in the paired variants; lower piece
    /// is two rows later and the second variant is two columns to the right.</summary>
    internal const int PairedUpper = 0x109;
    /// <summary>Tile$149, left small shoulder cap; adjacent$14A is the right cap
    /// and the next atlas row$159 is the large shoulder body.</summary>
    internal const int ShoulderCap = 0x149;
}
