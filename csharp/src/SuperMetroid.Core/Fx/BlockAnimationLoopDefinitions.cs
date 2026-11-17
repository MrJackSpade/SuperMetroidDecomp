using System.Collections.Immutable;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// The looping block animations (#1988, #2361): each loop's frames, authored in the order it shows them with the passes
/// each is shown for, and when the loop starts; the animation a room effect's type shows on the effect overlay; and the treadmill animation a Wrecked
/// Ship entrance door starts for each direction.
/// </summary>
/// <remarks>
/// What each frame pictures is installed artwork, which asset extraction pictures under the frame's name; this catalog
/// owns only each loop's frames, their durations and its start.
/// </remarks>
public static class BlockAnimationLoopDefinitions
{
    /// <summary>The nine loops, by animation (<see cref="BlockAnimationLoops"/>).</summary>
    internal static readonly IReadOnlyDictionary<BlockAnimation, BlockAnimationLoop> Loops =
        new Dictionary<BlockAnimation, BlockAnimationLoop>
    {
        [BlockAnimation.HorizontalSpikes] = new(BlockAnimation.HorizontalSpikes, BlockAnimationStart.Immediately,
        [
            new(BlockAnimationFrames.Frame(BlockAnimation.HorizontalSpikes, "frame 0"), 8),
            new(BlockAnimationFrames.Frame(BlockAnimation.HorizontalSpikes, "frame 1"), 8),
            new(BlockAnimationFrames.Frame(BlockAnimation.HorizontalSpikes, "frame 2"), 8),
            new(BlockAnimationFrames.Frame(BlockAnimation.HorizontalSpikes, "frame 3"), 8),
        ]),
        [BlockAnimation.MaridiaSandCeiling] = new(BlockAnimation.MaridiaSandCeiling, BlockAnimationStart.Immediately,
        [
            new(BlockAnimationFrames.Frame(BlockAnimation.MaridiaSandCeiling, "frame 0"), 10),
            new(BlockAnimationFrames.Frame(BlockAnimation.MaridiaSandCeiling, "frame 1"), 10),
            new(BlockAnimationFrames.Frame(BlockAnimation.MaridiaSandCeiling, "frame 2"), 10),
            new(BlockAnimationFrames.Frame(BlockAnimation.MaridiaSandCeiling, "frame 3"), 10),
        ]),
        [BlockAnimation.MaridiaSandFalling] = new(BlockAnimation.MaridiaSandFalling, BlockAnimationStart.Immediately,
        [
            new(BlockAnimationFrames.Frame(BlockAnimation.MaridiaSandFalling, "frame 0"), 10),
            new(BlockAnimationFrames.Frame(BlockAnimation.MaridiaSandFalling, "frame 1"), 10),
            new(BlockAnimationFrames.Frame(BlockAnimation.MaridiaSandFalling, "frame 2"), 10),
            new(BlockAnimationFrames.Frame(BlockAnimation.MaridiaSandFalling, "frame 3"), 10),
        ]),
        [BlockAnimation.WreckedShipTreadmillRightwards] = new(BlockAnimation.WreckedShipTreadmillRightwards, BlockAnimationStart.OnceAreaBossIsDefeated,
        [
            new(BlockAnimationFrames.Frame(BlockAnimation.WreckedShipTreadmillRightwards, "frame 0"), 1),
            new(BlockAnimationFrames.Frame(BlockAnimation.WreckedShipTreadmillRightwards, "frame 1"), 1),
            new(BlockAnimationFrames.Frame(BlockAnimation.WreckedShipTreadmillRightwards, "frame 2"), 1),
            new(BlockAnimationFrames.Frame(BlockAnimation.WreckedShipTreadmillRightwards, "frame 3"), 1),
        ]),
        [BlockAnimation.WreckedShipTreadmillLeftwards] = new(BlockAnimation.WreckedShipTreadmillLeftwards, BlockAnimationStart.OnceAreaBossIsDefeated,
        [
            new(BlockAnimationFrames.Frame(BlockAnimation.WreckedShipTreadmillLeftwards, "frame 0"), 1),
            new(BlockAnimationFrames.Frame(BlockAnimation.WreckedShipTreadmillLeftwards, "frame 1"), 1),
            new(BlockAnimationFrames.Frame(BlockAnimation.WreckedShipTreadmillLeftwards, "frame 2"), 1),
            new(BlockAnimationFrames.Frame(BlockAnimation.WreckedShipTreadmillLeftwards, "frame 3"), 1),
        ]),
        [BlockAnimation.Lava] = new(BlockAnimation.Lava, BlockAnimationStart.Immediately,
        [
            new(BlockAnimationFrames.Frame(BlockAnimation.Lava, "frame 0"), 13),
            new(BlockAnimationFrames.Frame(BlockAnimation.Lava, "frame 1"), 13),
            new(BlockAnimationFrames.Frame(BlockAnimation.Lava, "frame 2"), 13),
            new(BlockAnimationFrames.Frame(BlockAnimation.Lava, "frame 3"), 13),
            new(BlockAnimationFrames.Frame(BlockAnimation.Lava, "frame 4"), 13),
        ]),
        [BlockAnimation.Acid] = new(BlockAnimation.Acid, BlockAnimationStart.Immediately,
        [
            new(BlockAnimationFrames.Frame(BlockAnimation.Acid, "frame 0"), 10),
            new(BlockAnimationFrames.Frame(BlockAnimation.Acid, "frame 1"), 10),
            new(BlockAnimationFrames.Frame(BlockAnimation.Acid, "frame 2"), 10),
            new(BlockAnimationFrames.Frame(BlockAnimation.Acid, "frame 3"), 10),
            new(BlockAnimationFrames.Frame(BlockAnimation.Acid, "frame 4"), 10),
        ]),
        [BlockAnimation.Rain] = new(BlockAnimation.Rain, BlockAnimationStart.Immediately,
        [
            new(BlockAnimationFrames.Frame(BlockAnimation.Rain, "frame 0"), 10),
            new(BlockAnimationFrames.Frame(BlockAnimation.Rain, "frame 1"), 10),
            new(BlockAnimationFrames.Frame(BlockAnimation.Rain, "frame 2"), 10),
            new(BlockAnimationFrames.Frame(BlockAnimation.Rain, "frame 3"), 10),
            new(BlockAnimationFrames.Frame(BlockAnimation.Rain, "frame 4"), 10),
        ]),
        [BlockAnimation.Spores] = new(BlockAnimation.Spores, BlockAnimationStart.Immediately,
        [
            new(BlockAnimationFrames.Frame(BlockAnimation.Spores, "frame 0"), 10),
            new(BlockAnimationFrames.Frame(BlockAnimation.Spores, "frame 1"), 10),
            new(BlockAnimationFrames.Frame(BlockAnimation.Spores, "frame 2"), 10),
        ]),
    };

    /// <summary>
    /// The animation a room effect of each type shows on the effect overlay; an effect of any other type shows none
    /// there.
    /// </summary>
    internal static readonly IReadOnlyDictionary<RoomFxType, BlockAnimation> EffectOverlayAnimations =
        new Dictionary<RoomFxType, BlockAnimation>
    {
        [RoomFxType.Lava] = BlockAnimation.Lava,
        [RoomFxType.Acid] = BlockAnimation.Acid,
        [RoomFxType.Rain] = BlockAnimation.Rain,
        [RoomFxType.Spores] = BlockAnimation.Spores,
    };

    /// <summary>The treadmill animation a Wrecked Ship entrance door starts for each belt direction.</summary>
    internal static readonly IReadOnlyDictionary<WreckedShipTreadmillDirection, BlockAnimation> TreadmillAnimations =
        new Dictionary<WreckedShipTreadmillDirection, BlockAnimation>
    {
        [WreckedShipTreadmillDirection.Rightwards] = BlockAnimation.WreckedShipTreadmillRightwards,
        [WreckedShipTreadmillDirection.Leftwards] = BlockAnimation.WreckedShipTreadmillLeftwards,
    };
}
