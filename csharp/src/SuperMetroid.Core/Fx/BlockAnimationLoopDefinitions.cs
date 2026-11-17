namespace SuperMetroid.Core.Fx;

/// <summary>
/// The looping block animations (#1988, #2361): each loop's frames, authored in the order it shows them with the passes
/// each is shown for, and when the loop starts; the animation a room effect's type shows on the effect overlay; and the treadmill animation a Wrecked
/// Ship entrance door starts for each direction.
/// </summary>
/// <remarks>
/// Each loop is its animation's idle loop, the one a block animation always plays (#2370): its frames are named idle 0,
/// idle 1 and on, in the order it shows them. What each frame pictures is installed artwork, which asset extraction
/// pictures under the frame's name; this catalog owns only each loop's frames, their durations and its start.
/// </remarks>
public static class BlockAnimationLoopDefinitions
{
    /// <summary>The nine loops, by animation (<see cref="BlockAnimationLoops"/>).</summary>
    internal static readonly IReadOnlyDictionary<BlockAnimation, BlockAnimationLoop> Loops =
        new Dictionary<BlockAnimation, BlockAnimationLoop>
    {
        [BlockAnimation.HorizontalSpikes] = new(BlockAnimation.HorizontalSpikes, BlockAnimationStart.Immediately,
        [
            new(BlockAnimationFrames.Frame(BlockAnimation.HorizontalSpikes, "idle 0"), 8),
            new(BlockAnimationFrames.Frame(BlockAnimation.HorizontalSpikes, "idle 1"), 8),
            new(BlockAnimationFrames.Frame(BlockAnimation.HorizontalSpikes, "idle 2"), 8),
            new(BlockAnimationFrames.Frame(BlockAnimation.HorizontalSpikes, "idle 3"), 8),
        ]),
        [BlockAnimation.MaridiaSandCeiling] = new(BlockAnimation.MaridiaSandCeiling, BlockAnimationStart.Immediately,
        [
            new(BlockAnimationFrames.Frame(BlockAnimation.MaridiaSandCeiling, "idle 0"), 10),
            new(BlockAnimationFrames.Frame(BlockAnimation.MaridiaSandCeiling, "idle 1"), 10),
            new(BlockAnimationFrames.Frame(BlockAnimation.MaridiaSandCeiling, "idle 2"), 10),
            new(BlockAnimationFrames.Frame(BlockAnimation.MaridiaSandCeiling, "idle 3"), 10),
        ]),
        [BlockAnimation.MaridiaSandFalling] = new(BlockAnimation.MaridiaSandFalling, BlockAnimationStart.Immediately,
        [
            new(BlockAnimationFrames.Frame(BlockAnimation.MaridiaSandFalling, "idle 0"), 10),
            new(BlockAnimationFrames.Frame(BlockAnimation.MaridiaSandFalling, "idle 1"), 10),
            new(BlockAnimationFrames.Frame(BlockAnimation.MaridiaSandFalling, "idle 2"), 10),
            new(BlockAnimationFrames.Frame(BlockAnimation.MaridiaSandFalling, "idle 3"), 10),
        ]),
        [BlockAnimation.WreckedShipTreadmillRightwards] = new(BlockAnimation.WreckedShipTreadmillRightwards, BlockAnimationStart.OnceAreaBossIsDefeated,
        [
            new(BlockAnimationFrames.Frame(BlockAnimation.WreckedShipTreadmillRightwards, "idle 0"), 1),
            new(BlockAnimationFrames.Frame(BlockAnimation.WreckedShipTreadmillRightwards, "idle 1"), 1),
            new(BlockAnimationFrames.Frame(BlockAnimation.WreckedShipTreadmillRightwards, "idle 2"), 1),
            new(BlockAnimationFrames.Frame(BlockAnimation.WreckedShipTreadmillRightwards, "idle 3"), 1),
        ]),
        [BlockAnimation.WreckedShipTreadmillLeftwards] = new(BlockAnimation.WreckedShipTreadmillLeftwards, BlockAnimationStart.OnceAreaBossIsDefeated,
        [
            new(BlockAnimationFrames.Frame(BlockAnimation.WreckedShipTreadmillLeftwards, "idle 0"), 1),
            new(BlockAnimationFrames.Frame(BlockAnimation.WreckedShipTreadmillLeftwards, "idle 1"), 1),
            new(BlockAnimationFrames.Frame(BlockAnimation.WreckedShipTreadmillLeftwards, "idle 2"), 1),
            new(BlockAnimationFrames.Frame(BlockAnimation.WreckedShipTreadmillLeftwards, "idle 3"), 1),
        ]),
        [BlockAnimation.Lava] = new(BlockAnimation.Lava, BlockAnimationStart.Immediately,
        [
            new(BlockAnimationFrames.Frame(BlockAnimation.Lava, "idle 0"), 13),
            new(BlockAnimationFrames.Frame(BlockAnimation.Lava, "idle 1"), 13),
            new(BlockAnimationFrames.Frame(BlockAnimation.Lava, "idle 2"), 13),
            new(BlockAnimationFrames.Frame(BlockAnimation.Lava, "idle 3"), 13),
            new(BlockAnimationFrames.Frame(BlockAnimation.Lava, "idle 4"), 13),
        ]),
        [BlockAnimation.Acid] = new(BlockAnimation.Acid, BlockAnimationStart.Immediately,
        [
            new(BlockAnimationFrames.Frame(BlockAnimation.Acid, "idle 0"), 10),
            new(BlockAnimationFrames.Frame(BlockAnimation.Acid, "idle 1"), 10),
            new(BlockAnimationFrames.Frame(BlockAnimation.Acid, "idle 2"), 10),
            new(BlockAnimationFrames.Frame(BlockAnimation.Acid, "idle 3"), 10),
            new(BlockAnimationFrames.Frame(BlockAnimation.Acid, "idle 4"), 10),
        ]),
        [BlockAnimation.Rain] = new(BlockAnimation.Rain, BlockAnimationStart.Immediately,
        [
            new(BlockAnimationFrames.Frame(BlockAnimation.Rain, "idle 0"), 10),
            new(BlockAnimationFrames.Frame(BlockAnimation.Rain, "idle 1"), 10),
            new(BlockAnimationFrames.Frame(BlockAnimation.Rain, "idle 2"), 10),
            new(BlockAnimationFrames.Frame(BlockAnimation.Rain, "idle 3"), 10),
            new(BlockAnimationFrames.Frame(BlockAnimation.Rain, "idle 4"), 10),
        ]),
        [BlockAnimation.Spores] = new(BlockAnimation.Spores, BlockAnimationStart.Immediately,
        [
            new(BlockAnimationFrames.Frame(BlockAnimation.Spores, "idle 0"), 10),
            new(BlockAnimationFrames.Frame(BlockAnimation.Spores, "idle 1"), 10),
            new(BlockAnimationFrames.Frame(BlockAnimation.Spores, "idle 2"), 10),
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
