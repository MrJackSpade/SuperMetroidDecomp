using System.Collections;
using Pose = SuperMetroid.Core.Frontend.Ending.Artwork.EndingRewardSpriteFrame;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>Standing suited body and its headless/armless variant share a
/// thirty-part atlas grid. The full body prepends the four-tile arm strip.</summary>
internal sealed class EndingRewardStandingParts(bool includeArm) : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Replaces matching stock standing poses with calculated parts while preserving edited compositions.</summary>
    internal static SpriteComposition CalculateIfMatching(ushort pointer, SpriteComposition supplied)
    {
        if (pointer == EndingRewardSpriteDefinitions.FramePointer(Pose.LargeSamusFromEndingStanding))
            return supplied.CalculateIfMatching(new EndingRewardStandingParts(true));
        if (pointer == EndingRewardSpriteDefinitions.FramePointer(Pose.HeadlessArmlessSuitedSamus))
            return supplied.CalculateIfMatching(new EndingRewardStandingParts(false));
        return supplied;
    }

    /// <summary>Counts thirty shared body parts and four optional arm-strip parts.</summary>
    public int Count => includeArm ? 34 : 30;
    /// <summary>Calculates an indexed body or arm part from its native atlas tile and screen offset.</summary>
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            int x, y, tile;
            bool large;
            if (includeArm && index < 4)
            {
                tile = 0xbe - 32 * index;
                x = 16; y = 8 - 16 * index; large = true;
            }
            else
            {
                int piece = index - (includeArm ? 4 : 0);
                if (piece < 2) tile = 0xe6 - 16 * piece; // outer foot edge, bottom to top
                else if (piece == 2) tile = EndingRewardStandingAtlas.RightFoot;
                else if (piece < 6) tile = 0x26 - 16 * (piece - 3); // outer shoulder edge
                else if (piece < 8) tile = 0x91 - 16 * (piece - 6); // inner left hip edge
                else if (piece == 8) tile = EndingRewardStandingAtlas.OuterLeftHip;
                else if (piece == 9) tile = EndingRewardStandingAtlas.LeftLegEdge;
                else if (piece < 12) tile = 0xd2 - 2 * (piece - 10); // remaining feet, right to left
                else if (piece < 18)
                {
                    int cell = piece - 12;
                    tile = 0xc4 - 32 * (cell / 2) - 2 * (cell % 2); // two-column lower body
                }
                else
                {
                    int cell = piece - 18;
                    tile = 0x64 - 32 * (cell / 3) - 2 * (cell % 3); // three-column upper body
                }
                x = 8 * (tile & 15) - 24;
                y = 8 * (tile / 16) - 56;
                large = piece is 2 or >= 10;
            }
            return new(SnesSpritemapXWord.Create(x, large), unchecked((byte)y),
                SnesObjAttributeWord.Create(tile, 0, 3, 0), true);
        }
    }
    /// <summary>Enumerates standing parts in the OAM composition order.</summary>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Exposes the same standing-part order through the non-generic collection interface.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
