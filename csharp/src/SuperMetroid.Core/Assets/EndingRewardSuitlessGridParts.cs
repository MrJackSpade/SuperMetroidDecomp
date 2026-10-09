using System.Collections;
using SuperMetroid.Core.Hardware;
using Pose = SuperMetroid.Core.Assets.EndingRewardSpriteFrame;

namespace SuperMetroid.Core.Assets;

/// <summary>Suitless lower-body and jump compositions use atlas grids, with
/// separately positioned arm strips in the jumping pose.</summary>
/// <param name="pose">Suitless pose whose ordered sprite parts are exposed by this grid.</param>
internal sealed class EndingRewardSuitlessGridParts(Pose pose) : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Replaces matching suitless-pose compositions with their atlas-grid part ordering.</summary>
    /// <param name="pointer">Native spritemap pointer used to select a supported pose.</param>
    /// <param name="supplied">Composition to retain when the pointer does not identify a supported pose.</param>
    /// <returns>The recalculated composition for a matching pose, or <paramref name="supplied"/> unchanged.</returns>
    internal static SpriteComposition CalculateIfMatching(ushort pointer, SpriteComposition supplied)
    {
        if (pointer == EndingRewardSpriteDefinitions.FramePointer(Pose.SuitlessSamusLowerBody))
            return supplied.CalculateIfMatching(new EndingRewardSuitlessGridParts(Pose.SuitlessSamusLowerBody));
        if (pointer == EndingRewardSpriteDefinitions.FramePointer(Pose.SuitlessSamusPreparingToJump))
            return supplied.CalculateIfMatching(new EndingRewardSuitlessGridParts(Pose.SuitlessSamusPreparingToJump));
        if (pointer == EndingRewardSpriteDefinitions.FramePointer(Pose.SuitlessSamusJumping))
            return supplied.CalculateIfMatching(new EndingRewardSuitlessGridParts(Pose.SuitlessSamusJumping));
        return supplied;
    }
    /// <summary>Number of ordered parts in the selected pose's grid composition.</summary>
    public int Count => pose switch
    {
        Pose.SuitlessSamusLowerBody => 14,
        Pose.SuitlessSamusPreparingToJump => 20,
        Pose.SuitlessSamusJumping => 19,
        _ => throw new ArgumentOutOfRangeException(nameof(pose)),
    };

    /// <summary>Gets the indexed tile and screen offset in the selected pose's composition order.</summary>
    /// <param name="index">Zero-based part index, less than <see cref="Count"/>.</param>
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            int tile, x, y;
            bool large;
            if (pose == Pose.SuitlessSamusLowerBody)
            {
                if (index == 0) tile = EndingRewardSuitlessAtlas.InnerHipEdge;
                else if (index < 3) tile = 0xd0 + 5 * (index - 1); // outer foot tips, left then right
                else if (index < 5) tile = 0xc1 + 2 * (index - 3); // foot pair, left then right
                else if (index < 8) tile = 0x83 - 16 * (index - 5); // inner edge, bottom to top
                else if (index < 10) tile = 0xb3 - 32 * (index - 8); // right leg
                else tile = 0xb1 - 32 * (index - 10); // left leg and hip
                x = 8 * (tile & 15) - 16; y = 8 * (tile / 16) - 40;
                large = index is 3 or 4 or >= 8;
            }
            else if (pose == Pose.SuitlessSamusPreparingToJump)
            {
                if (index < 5) tile = 0x56 - 16 * index; // left edge, bottom to top
                else if (index == 5) tile = EndingRewardSuitlessAtlas.CrouchRightCap;
                else if (index == 6) tile = EndingRewardSuitlessAtlas.CrouchLeftLegEdge;
                else if (index < 10) tile = 0xc8 - 32 * (index - 7); // right leg
                else if (index < 12) tile = 0x69 - 4 * (index - 10); // outer hips, right then left
                else if (index < 14) tile = 0x49 - 32 * (index - 12); // right upper edge
                else tile = 0xa7 - 32 * (index - 14); // central column, bottom to top
                x = 8 * ((tile & 15) - 6) - 16; y = 8 * (tile / 16) - 32;
                large = index >= 7;
            }
            else if (index < 7)
            {
                bool right = index < 4;
                int row = right ? index : index - 4;
                tile = (right ? 0xbb : 0x2b) - 16 * row;
                x = right ? 16 : -24; y = (right ? -16 : -24) - 8 * row;
                large = false;
            }
            else
            {
                if (index < 9) tile = 0xce - 32 * (index - 7); // lower right leg
                else
                {
                    int cell = index - 9;
                    tile = 0x8e - 32 * (cell / 2) - 2 * (cell % 2); // paired body rows
                }
                x = 8 * ((tile & 15) - 12) - 16; y = 8 * (tile / 16) - 40;
                large = true;
            }
            return new(SnesSpritemapXWord.Create(x, large), unchecked((byte)y),
                SnesObjAttributeWord.Create(tile, 0, 3, 0), true);
        }
    }
    /// <summary>Enumerates the pose's compiled sprite parts in the same order as the indexer.</summary>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>Individually ordered edges outside the suitless body strip traversals.</summary>
internal static class EndingRewardSuitlessAtlas
{
    /// <summary>Tile$53, small inner hip edge in the lower-body composition.</summary>
    internal const int InnerHipEdge = 0x53;
    /// <summary>Tile$19, small upper-right cap of the jump-preparation pose.</summary>
    internal const int CrouchRightCap = 0x19;
    /// <summary>Tile$86, small left leg edge of the jump-preparation pose.</summary>
    internal const int CrouchLeftLegEdge = 0x86;
}
