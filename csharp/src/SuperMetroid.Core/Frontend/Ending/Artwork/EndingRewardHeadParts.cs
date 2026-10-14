using System.Collections;
using Pose = SuperMetroid.Core.Frontend.Ending.Artwork.EndingRewardSpriteFrame;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>Reward head compositions: atlas rows, shared upper head and packed
/// jump/helmet caps. Native draw order is preserved independently of tile order.</summary>
internal sealed class EndingRewardHeadParts(Pose pose) : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Limits calculated compositions to the large helmet, jumping head, and four face frames.</summary>
    private static bool IsHead(Pose pose) => pose is >= Pose.LargeSamusHelmetFromEndingFrame1 and <= Pose.JumpingSamusHeadFromEnding
        or >= Pose.SamusHeadFromEndingFrame1 and <= Pose.SamusHeadWithHelmetFromEnding;

    /// <summary>Recalculates a reward head only when its native frame pointer belongs to this head family.</summary>
    internal static SpriteComposition CalculateIfMatching(ushort pointer, SpriteComposition supplied)
    {
        foreach (Pose pose in Enum.GetValues<Pose>())
            if (IsHead(pose) && EndingRewardSpriteDefinitions.FramePointer(pose) == pointer)
                return supplied.CalculateIfMatching(new EndingRewardHeadParts(pose));
        return supplied;
    }

    /// <summary>Face frames use two pieces; helmet and jump poses need their own cap and head part counts.</summary>
    public int Count => pose switch
    {
        Pose.LargeSamusHelmetFromEndingFrame1 or Pose.LargeSamusHelmetFromEndingFrame2 => 5,
        Pose.JumpingSamusHeadFromEnding => 3,
        Pose.SamusHeadWithHelmetFromEnding => 4,
        >= Pose.SamusHeadFromEndingFrame1 and <= Pose.SamusHeadFromEndingFrame4 => 2,
        _ => throw new ArgumentOutOfRangeException(nameof(pose)),
    };

    /// <summary>Builds the authored head or helmet piece at its draw-order index using the shared atlas rows.</summary>
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            int x, y, tile;
            bool large;
            if (pose is >= Pose.SamusHeadFromEndingFrame1 and <= Pose.SamusHeadFromEndingFrame4)
            {
                x = -8; y = index == 0 ? -8 : -16; large = true;
                tile = index == 0 ? 0x188 + 2 * (pose - Pose.SamusHeadFromEndingFrame1) : 0x178; // magic-number-audit: allow(PoseOrMovement) - tile atlas indices and strides for cinematic sprite composition.
            }
            else if (pose == Pose.SamusHeadWithHelmetFromEnding)
            {
                int column = 1 - index % 2, row = index / 2;
                x = -12 + 8 * column; y = 4 - 16 * row; large = row == 1;
                tile = 0x1c8 + column - 32 * row;
            }
            else if (pose == Pose.JumpingSamusHeadFromEnding)
            {
                large = index == 2;
                int column = large ? 0 : 1 - index;
                x = -8 + 8 * column; y = large ? -6 : -14;
                tile = (large ? 0x157 : 0x147) + column;
            }
            else if (pose == Pose.LargeSamusHelmetFromEndingFrame1)
            {
                large = index >= 3;
                int column = large ? index - 3 : 2 - index;
                x = -12 + 8 * column; y = large ? -13 : 3;
                tile = (large ? 0x76 : 0x96) + column;
            }
            else
            {
                // Right large piece precedes the three small cap tiles, then left large piece.
                large = index is 0 or 4;
                int column = large ? (index == 0 ? 1 : 0) : 3 - index;
                x = -12 + 8 * column; y = large ? -6 : -14;
                tile = (large ? 0x56 : 0x46) + column;
            }
            return new(SnesSpritemapXWord.Create(x, large), unchecked((byte)y),
                SnesObjAttributeWord.Create(tile, 0, 3, 0), true);
        }
    }
    /// <summary>Enumerates reward head pieces in native sprite draw order.</summary>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Uses the same draw order for non-generic collection consumers.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
