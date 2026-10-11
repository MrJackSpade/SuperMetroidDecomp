using System.Collections;
using SuperMetroid.Core.Hardware;
using Pose = SuperMetroid.Core.Assets.EndingRewardSpriteFrame;

namespace SuperMetroid.Core.Assets;

/// <summary>Small falling/landing/shooting figures use pose-specific origins
/// on a common atlas grid. Native priority is zero for these four compositions.</summary>
internal sealed class EndingRewardShootingSceneParts(Pose pose) : IReadOnlyList<CompiledSpritePart>
{
    internal static SpriteComposition CalculateIfMatching(ushort pointer, SpriteComposition supplied)
    {
        foreach (Pose pose in EndingRewardSpriteFrameSeries.ShootingScene)
            if (pointer == EndingRewardSpriteDefinitions.FramePointer(pose))
                return supplied.CalculateIfMatching(new EndingRewardShootingSceneParts(pose));
        return supplied;
    }
    public int Count => pose switch
    {
        Pose.SamusFalling or Pose.SamusShooting => 15,
        Pose.SamusLanding => 13,
        Pose.SamusLanded => 21,
        _ => throw new ArgumentOutOfRangeException(nameof(pose)),
    };
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            int tile, originX, originY;
            bool large;
            switch (pose)
            {
                case Pose.SamusFalling:
                    if (index < 3) tile = 0x54 - 16 * index; // right edge
                    else if (index < 6) tile = 0x83 - 16 * (index - 3); // leg edge
                    else if (index < 10)
                    {
                        int cell = index - 6;
                        tile = 0x21 + 4 * (cell / 2) - cell % 2; // left/right cap pairs
                    }
                    else if (index < 12) tile = 4 * (index - 10); // outer upper pieces
                    else tile = 0x42 - 32 * (index - 12); // central column
                    large = index >= 10; originX = -24; originY = -32;
                    break;
                case Pose.SamusLanding:
                    if (index == 0) tile = EndingRewardShootingAtlas.LandingLeftLegEdge;
                    else if (index < 3) tile = 0x8a - (index - 1); // lower edge
                    else if (index < 5) tile = 0x69 - 32 * (index - 3); // right body
                    else if (index < 7) tile = 0x3b - (index - 5); // right cap
                    else if (index == 7) tile = EndingRewardShootingAtlas.LandingLeftOuter;
                    else if (index == 8) tile = EndingRewardShootingAtlas.LandingLeftCap;
                    else if (index == 9) tile = EndingRewardShootingAtlas.LandingRightUpper;
                    else tile = 0x58 - 32 * (index - 10); // central column
                    large = index is 3 or 4 or 7 or >= 9; originX = -72; originY = -33;
                    break;
                case Pose.SamusLanded:
                    if (index < 2) tile = 0xb0 - 16 * index; // left cap
                    else if (index == 2) tile = EndingRewardShootingAtlas.LandedRightCap;
                    else if (index == 3) tile = EndingRewardShootingAtlas.LandedRightUpper;
                    else if (index < 6) tile = 0x122 - (index - 4); // left foot edge
                    else if (index < 9) tile = 0x125 - (index - 6); // right foot edge
                    else if (index < 11) tile = 0x103 - 2 * (index - 9); // feet
                    else if (index < 15) tile = 0xf4 - (index - 11); // waist edge
                    else
                    {
                        int cell = index - 15;
                        tile = 0xd3 - 32 * (cell / 2) - 2 * (cell % 2); // paired body rows
                    }
                    large = index is 3 or 9 or 10 or >= 15; originX = -24; originY = -112;
                    break;
                case Pose.SamusShooting:
                    if (index == 0) tile = EndingRewardShootingAtlas.ShootingRightFootTip;
                    else if (index < 4) tile = 0x11a - 2 * (index - 1); // lower row
                    else if (index == 4) tile = EndingRewardShootingAtlas.ShootingLeftHipEdge;
                    else if (index < 7) tile = 0x10a - 3 * (index - 5); // outer leg pair
                    else if (index < 9) tile = 0xea - 2 * (index - 7); // waist pair
                    else if (index == 9) tile = EndingRewardShootingAtlas.ShootingRightUpper;
                    else if (index == 10) tile = EndingRewardShootingAtlas.ShootingRightEdge;
                    else
                    {
                        int cell = index - 11;
                        tile = 0xc9 - 32 * (cell / 2) - 2 * (cell % 2); // upper-body pairs
                    }
                    large = index is not (0 or 2 or 4 or 10); originX = -76; originY = -112;
                    break;
                default:
                    throw new InvalidOperationException($"{pose} is not a falling, landing or shooting figure.");
            }
            return new(SnesSpritemapXWord.Create(originX + 8 * (tile & 15), large),
                unchecked((byte)(originY + 8 * (tile / 16))), SnesObjAttributeWord.Create(tile, 0, 0, 0), true);
        }
    }
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>Individually ordered boundary pieces of the small reward figures.</summary>
internal static class EndingRewardShootingAtlas
{
    /// <summary>Tile$78, small left leg edge while landing.</summary>
    internal const int LandingLeftLegEdge = 0x78;
    /// <summary>Tile$26, large left outer piece while landing.</summary>
    internal const int LandingLeftOuter = 0x26;
    /// <summary>Tile$17, small left cap while landing.</summary>
    internal const int LandingLeftCap = 0x17;
    /// <summary>Tile$1A, large upper-right landing piece.</summary>
    internal const int LandingRightUpper = 0x1a;
    /// <summary>Tile$95, small upper-right cap after landing.</summary>
    internal const int LandedRightCap = 0x95;
    /// <summary>Tile$A4, large upper-right piece after landing.</summary>
    internal const int LandedRightUpper = 0xa4;
    /// <summary>Tile$12C, small right foot tip while shooting.</summary>
    internal const int ShootingRightFootTip = 0x12c;
    /// <summary>Tile$F7, small left hip edge while shooting.</summary>
    internal const int ShootingLeftHipEdge = 0xf7;
    /// <summary>Tile$AA, large upper-right piece while shooting.</summary>
    internal const int ShootingRightUpper = 0xaa;
    /// <summary>Tile$CB, small right edge while shooting.</summary>
    internal const int ShootingRightEdge = 0xcb;
}
