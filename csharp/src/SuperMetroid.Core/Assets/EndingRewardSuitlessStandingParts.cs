using System.Collections;
using SuperMetroid.Core.Hardware;
using Pose = SuperMetroid.Core.Assets.EndingRewardSpriteFrame;

namespace SuperMetroid.Core.Assets;

/// <summary>Two suitless standing compositions: shared lower-body regions,
/// vertical arm strips and independently positioned head/upper-body grids.</summary>
/// <param name="armsStraight">Selects the arms-straight pose layout instead of the default standing pose.</param>
internal sealed class EndingRewardSuitlessStandingParts(bool armsStraight) : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Shared lower-body grid used by both suitless standing layouts.</summary>
    private readonly EndingRewardSuitlessGridParts lower = new(Pose.SuitlessSamusLowerBody);

    /// <summary>Uses the specialized suitless layout for either standing pose when the pointer matches.</summary>
    /// <param name="pointer">Frame pointer identifying the standing pose to match.</param>
    /// <param name="supplied">Composition decoded from the original frame.</param>
    /// <returns>A composition using the specialized layout for a recognized standing pose; otherwise, the supplied composition.</returns>
    internal static SpriteComposition CalculateIfMatching(ushort pointer, SpriteComposition supplied)
    {
        if (pointer == EndingRewardSpriteDefinitions.FramePointer(Pose.SuitlessSamusStanding))
            return supplied.CalculateIfMatching(new EndingRewardSuitlessStandingParts(false));
        if (pointer == EndingRewardSpriteDefinitions.FramePointer(Pose.SuitlessSamusStandingArmsStraight))
            return supplied.CalculateIfMatching(new EndingRewardSuitlessStandingParts(true));
        return supplied;
    }

    /// <summary>Gets the fixed number of sprite parts in either suitless standing pose.</summary>
    public int Count => 28;

    /// <summary>Gets the sprite part at its pose-specific position in the 28-part standing composition.</summary>
    /// <param name="index">Zero-based position within the standing composition.</param>
    /// <returns>The tile and placement for the selected part in the chosen pose layout.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is outside the composition.</exception>
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            int tile, x, y;
            bool large;
            if (armsStraight)
            {
                if (index is >= 5 and < 8) return lower[index];
                if (index is >= 8 and < 12) return lower[index - 7];
                if (index is >= 12 and < 18) return lower[index - 4];
                if (index is >= 1 and < 5 or >= 21 and < 25)
                {
                    bool right = index < 5;
                    int row = index - (right ? 1 : 21);
                    tile = (right ? 0x1b5 : 0x1b4) - 16 * row;
                    x = right ? 8 : -13; y = 24 - 8 * row; large = false;
                }
                else
                {
                    if (index == 0) tile = EndingRewardSuitlessStandingAtlas.HeadRightCap;
                    else if (index < 21)
                    {
                        // Left edge, right edge, then center of the head's middle row.
                        int column = index == 18 ? 0 : index == 19 ? 3 : 1;
                        tile = 0x1a0 + column;
                    }
                    else if (index < 27) tile = 0x1b2 - 2 * (index - 25);
                    else tile = EndingRewardSuitlessStandingAtlas.HeadTop;
                    x = -15 + 8 * (tile & 15); y = -40 + 8 * (tile / 16 - 24);
                    large = index is 20 or >= 25;
                }
            }
            else
            {
                if (index == 5)
                {
                    tile = EndingRewardSuitlessStandingAtlas.RightArm; x = 8; y = 0; large = true;
                }
                else if (index is >= 6 and < 10)
                {
                    tile = 0x15d - 16 * (index - 6); x = -16; y = 24 - 8 * (index - 6); large = false;
                }
                else
                {
                    if (index < 3) tile = index switch
                    {
                        0 => EndingRewardSuitlessStandingAtlas.RightKneeEdge,
                        1 => EndingRewardSuitlessStandingAtlas.LeftUpperEdge,
                        _ => EndingRewardSuitlessStandingAtlas.LeftFootTip,
                    };
                    else if (index < 5) tile = 0x03 + 16 * (index - 3);
                    else if (index < 14) tile = index switch
                    {
                        10 => EndingRewardSuitlessStandingAtlas.RightUpperBody,
                        11 => EndingRewardSuitlessStandingAtlas.RightShinEdge,
                        12 => EndingRewardSuitlessStandingAtlas.RightFootTip,
                        _ => EndingRewardSuitlessStandingAtlas.RightFoot,
                    };
                    else if (index < 17) tile = 0xa2 - 32 * (index - 14);
                    else if (index < 20) tile = 0x40 - 16 * (index - 17);
                    else if (index == 20) tile = EndingRewardSuitlessStandingAtlas.RightShoulder;
                    else tile = 0xc1 - 32 * (index - 21);
                    x = -16 + 8 * (tile & 15); y = -40 + 8 * (tile / 16);
                    large = index == 10 || index is >= 13 and < 17 or >= 20;
                }
            }
            return new(SnesSpritemapXWord.Create(x, large), unchecked((byte)y),
                SnesObjAttributeWord.Create(tile, 0, 3, 0), true);
        }
    }

    /// <summary>Enumerates all parts of the selected standing pose in composition order.</summary>
    /// <returns>An enumerator over the 28 generated sprite parts.</returns>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>Individually ordered boundary pieces in the suitless standing atlas.</summary>
internal static class EndingRewardSuitlessStandingAtlas
{
    /// <summary>Tile$193, small right cap of the arms-straight head.</summary>
    internal const int HeadRightCap = 0x193;
    /// <summary>Tile$181, large top of the arms-straight head.</summary>
    internal const int HeadTop = 0x181;
    /// <summary>Tile$12E, separately packed large right arm.</summary>
    internal const int RightArm = 0x12e;
    /// <summary>Tile$A4, small right knee edge.</summary>
    internal const int RightKneeEdge = 0xa4;
    /// <summary>Tile$10, small left upper-body edge.</summary>
    internal const int LeftUpperEdge = 0x10;
    /// <summary>Tile$D0, small left foot tip.</summary>
    internal const int LeftFootTip = 0xd0;
    /// <summary>Tile$33, large right upper-body piece.</summary>
    internal const int RightUpperBody = 0x33;
    /// <summary>Tile$B4, small right shin edge.</summary>
    internal const int RightShinEdge = 0xb4;
    /// <summary>Tile$D5, small right foot tip.</summary>
    internal const int RightFootTip = 0xd5;
    /// <summary>Tile$C3, large right foot.</summary>
    internal const int RightFoot = 0xc3;
    /// <summary>Tile$23, large right shoulder piece.</summary>
    internal const int RightShoulder = 0x23;
}
