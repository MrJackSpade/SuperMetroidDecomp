using SuperMetroid.Core.Fx;

namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>Cartridge values and helpers of <see cref="RoomFxBlockAnimationLoops"/> that only verification reads.</summary>
internal static class RoomFxBlockAnimationLoopsConstants
{
    /// <summary>Enumerates the seven loops whose every picture the atlas holds as its own, in atlas order.</summary>
    public static IEnumerable<BlockAnimation> All
    {
        get
        {
            yield return RoomFxBlockAnimationLoops.Require(BlockAnimation.HorizontalSpikes);
            yield return RoomFxBlockAnimationLoops.Require(BlockAnimation.MaridiaSandCeiling);
            yield return RoomFxBlockAnimationLoops.Require(BlockAnimation.MaridiaSandFalling);
            yield return RoomFxBlockAnimationLoops.Require(BlockAnimation.Lava);
            yield return RoomFxBlockAnimationLoops.Require(BlockAnimation.Acid);
            yield return RoomFxBlockAnimationLoops.Require(BlockAnimation.Rain);
            yield return RoomFxBlockAnimationLoops.Require(BlockAnimation.Spores);
        }
    }
}
