using System.Collections;
using SuperMetroid.Core.Hardware;
using Pose = SuperMetroid.Core.Assets.EndingRewardSpriteFrame;

namespace SuperMetroid.Core.Assets;

/// <summary>Reward head compositions: atlas rows, shared upper head and packed
/// jump/helmet caps. Native draw order is preserved independently of tile order.</summary>
/// <param name="pose">Ending-cinematic pose whose head sprite parts are generated on demand.</param>
internal sealed class EndingRewardHeadParts(Pose pose) : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Determines whether a cinematic pose is represented by a separately composed head spritemap.</summary>
    /// <param name="pose">Pose to classify.</param>
    /// <returns><see langword="true"/> for one of the supported helmeted or suitless head frames.</returns>
    private static bool IsHead(Pose pose) => pose is >= Pose.LargeSamusHelmetFromEndingFrame1 and <= Pose.JumpingSamusHeadFromEnding
        or >= Pose.SamusHeadFromEndingFrame1 and <= Pose.SamusHeadWithHelmetFromEnding;

    /// <summary>Uses generated head parts when the pointer identifies a supported ending-reward head frame.</summary>
    /// <param name="pointer">Compiled spritemap pointer to compare with the ending-reward frame catalog.</param>
    /// <param name="supplied">Composition used to calculate pointers outside the generated head frames.</param>
    /// <returns>The composition calculated with generated head parts for a match, or the supplied composition's normal result otherwise.</returns>
    internal static SpriteComposition CalculateIfMatching(ushort pointer, SpriteComposition supplied)
    {
        for (Pose pose = Pose.LargeSamusFromEndingStanding; pose <= Pose.SuitlessSamusLowerBody; pose++)
            if (IsHead(pose) && EndingRewardSpriteDefinitions.FramePointer(pose) == pointer)
                return supplied.CalculateIfMatching(new EndingRewardHeadParts(pose));
        return supplied;
    }

    /// <summary>Gets the number of OAM entries in the selected head spritemap.</summary>
    public int Count => pose switch
    {
        Pose.LargeSamusHelmetFromEndingFrame1 or Pose.LargeSamusHelmetFromEndingFrame2 => 5,
        Pose.JumpingSamusHeadFromEnding => 3,
        Pose.SamusHeadWithHelmetFromEnding => 4,
        >= Pose.SamusHeadFromEndingFrame1 and <= Pose.SamusHeadFromEndingFrame4 => 2,
        _ => throw new ArgumentOutOfRangeException(nameof(pose)),
    };

    /// <summary>Gets the generated head part at its native draw-order index.</summary>
    /// <param name="index">Zero-based OAM entry index for the selected pose.</param>
    /// <returns>Sprite position, size, and atlas tile attributes for that part.</returns>
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

    /// <summary>Enumerates the generated head parts in the order the native spritemap draws them.</summary>
    /// <returns>An enumerator that creates each part on demand.</returns>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Returns a non-generic enumerator over this sequence.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
