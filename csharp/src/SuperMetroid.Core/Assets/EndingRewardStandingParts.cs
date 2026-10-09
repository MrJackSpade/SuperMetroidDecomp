using System.Collections;
using SuperMetroid.Core.Hardware;
using Pose = SuperMetroid.Core.Assets.EndingRewardSpriteFrame;

namespace SuperMetroid.Core.Assets;

/// <summary>Standing suited body and its headless/armless variant share a
/// thirty-part atlas grid. The full body prepends the four-tile arm strip.</summary>
/// <param name="includeArm">Whether to prepend the standing pose's four-part arm strip.</param>
internal sealed class EndingRewardStandingParts(bool includeArm) : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Uses the matching standing-pose part layout when the supplied frame pointer names either supported pose.</summary>
    /// <param name="pointer">Native spritemap pointer identifying the pose to check.</param>
    /// <param name="supplied">Composition whose parts are reused or conditionally extended with this layout.</param>
    /// <returns>The supplied composition with this layout applied for a supported standing pose; otherwise the original composition.</returns>
    internal static SpriteComposition CalculateIfMatching(ushort pointer, SpriteComposition supplied)
    {
        if (pointer == EndingRewardSpriteDefinitions.FramePointer(Pose.LargeSamusFromEndingStanding))
            return supplied.CalculateIfMatching(new EndingRewardStandingParts(true));
        if (pointer == EndingRewardSpriteDefinitions.FramePointer(Pose.HeadlessArmlessSuitedSamus))
            return supplied.CalculateIfMatching(new EndingRewardStandingParts(false));
        return supplied;
    }

    /// <summary>Number of parts in this pose, including the optional four-part arm strip.</summary>
    public int Count => includeArm ? 34 : 30;

    /// <summary>Gets the atlas part at the requested position in draw order.</summary>
    /// <param name="index">Zero-based position within this pose's part sequence.</param>
    /// <returns>The sprite part to draw at that position.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside this pose's part sequence.</exception>
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
    /// <summary>Enumerates the pose's sprite parts in their required draw order.</summary>
    /// <returns>An enumerator over this pose's parts.</returns>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>Individual edge pieces outside the standing-body row traversals.</summary>
internal static class EndingRewardStandingAtlas
{
    /// <summary>Tile$D4, large right foot drawn between its small edge and shoulder strip.</summary>
    internal const int RightFoot = 0xd4;
    /// <summary>Tile$80, small outer left hip edge adjacent to tile$81.</summary>
    internal const int OuterLeftHip = 0x80;
    /// <summary>Tile$C1, small left leg edge above the foot row.</summary>
    internal const int LeftLegEdge = 0xc1;
}
