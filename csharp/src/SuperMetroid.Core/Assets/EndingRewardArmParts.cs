using System.Collections;
using SuperMetroid.Core.Hardware;
using Pose = SuperMetroid.Core.Assets.EndingRewardSpriteFrame;

namespace SuperMetroid.Core.Assets;

/// <summary>Reward arms share a three-piece shoulder. Pose-specific arm tile
/// choices and positions remain independent inputs pending their own review.</summary>
internal sealed class EndingRewardArmParts : IReadOnlyList<CompiledSpritePart>
{
    private readonly record struct Piece(int Tile, int X, int Y);
    private readonly Piece[] arm;
    private readonly int shoulderX, shoulderY;
    private readonly bool splitArm;
    private EndingRewardArmParts(SpriteComposition supplied, bool splitArm)
    {
        this.splitArm = splitArm;
        int count = splitArm ? 3 : 2;
        arm = new Piece[count];
        for (int index = 0; index < count; index++)
        {
            CompiledSpritePart part = supplied.Part(index);
            arm[index] = new(part.Attributes.TileNumber, part.X.SignedOffset, unchecked((sbyte)part.Y));
        }
        CompiledSpritePart shoulder = supplied.Part(count);
        shoulderX = shoulder.X.SignedOffset;
        shoulderY = unchecked((sbyte)shoulder.Y);
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
            return supplied.CalculateIfMatching(new EndingRewardArmParts(supplied, split));
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
                tile = piece.Tile; x = piece.X; y = piece.Y;
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
    /// <summary>Tile$149, left small shoulder cap; adjacent$14A is the right cap
    /// and the next atlas row$159 is the large shoulder body.</summary>
    internal const int ShoulderCap = 0x149;
}
