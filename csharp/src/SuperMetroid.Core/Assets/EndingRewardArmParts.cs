using System.Collections;
using SuperMetroid.Core.Hardware;
using Pose = SuperMetroid.Core.Assets.EndingRewardSpriteFrame;

namespace SuperMetroid.Core.Assets;

/// <summary>Reward arms calculate a shared shoulder and stock tile selection.
/// Pose-specific positions remain independent inputs pending their own review.</summary>
internal sealed class EndingRewardArmParts : IReadOnlyList<CompiledSpritePart>
{
    private readonly record struct Piece(int X, int Y);
    private readonly Piece[] arm;
    private readonly int[]? editedTiles;
    private readonly Pose pose;
    private readonly int shoulderX, shoulderY;
    private readonly bool splitArm;
    private EndingRewardArmParts(SpriteComposition supplied, Pose pose)
    {
        this.pose = pose;
        splitArm = pose == Pose.SamusArmFromEndingFrame2;
        int count = splitArm ? 3 : 2;
        arm = new Piece[count];
        var tiles = new int[count];
        bool stock = true;
        for (int index = 0; index < count; index++)
        {
            CompiledSpritePart part = supplied.Part(index);
            arm[index] = new(part.X.SignedOffset, unchecked((sbyte)part.Y));
            tiles[index] = part.Attributes.TileNumber;
            stock &= tiles[index] == StockTile(pose, index);
        }
        editedTiles = stock ? null : tiles;
        CompiledSpritePart shoulder = supplied.Part(count);
        shoulderX = shoulder.X.SignedOffset;
        shoulderY = unchecked((sbyte)shoulder.Y);
    }

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
    internal static SpriteComposition CalculateIfMatching(ushort pointer, SpriteComposition supplied)
    {
        for (Pose pose = Pose.SamusArmFromEndingFrame1; pose <= Pose.SamusArmFromEndingFrame8; pose++)
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
    public int Count => arm.Length + 3;
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            int tile, x, y;
            bool large;
            if (index < arm.Length)
            {
                Piece piece = arm[index];
                tile = editedTiles is null ? StockTile(pose, index) : editedTiles[index];
                x = piece.X; y = piece.Y;
                large = !splitArm || index == 2;
            }
            else
            {
                int piece = index - arm.Length;
                large = piece == 0;
                int column = piece == 1 ? 1 : 0;
                tile = EndingRewardArmAtlas.ShoulderCap + (large ? 16 : column);
                x = shoulderX + 8 * column; y = shoulderY - (large ? 0 : 8);
            }
            return new(SnesSpritemapXWord.Create(x, large), unchecked((byte)y),
                SnesObjAttributeWord.Create(tile, 0, 3, 0), true);
        }
    }
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

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
