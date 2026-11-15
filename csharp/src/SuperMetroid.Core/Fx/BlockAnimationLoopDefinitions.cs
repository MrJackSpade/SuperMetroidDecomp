namespace SuperMetroid.Core.Fx;

/// <summary>
/// The looping block animations (#1988): how many frames each loop has, how many passes each frame is shown for and when
/// the loop starts; the animation a room effect's type shows on the effect overlay; and the treadmill animation a Wrecked
/// Ship entrance door starts for each direction.
/// </summary>
/// <remarks>
/// What each frame pictures is installed artwork, which asset extraction pictures under the frame's name; this catalog
/// owns only each loop's frames, their durations and its start. The vertical spikes, the Crateria lake, the Brinstar
/// plant and the Wrecked Ship screen have no loop: their blocks show their still pictures.
/// </remarks>
public static class BlockAnimationLoopDefinitions
{
    /// <summary>The nine loops, by animation (<see cref="BlockAnimationLoops"/>).</summary>
    internal static readonly IReadOnlyDictionary<BlockAnimation, BlockAnimationLoop> Loops =
        new Dictionary<BlockAnimation, BlockAnimationLoop>
    {
        [BlockAnimation.HorizontalSpikes] = new(BlockAnimation.HorizontalSpikes, frameCount: 4, frameDuration: 8, BlockAnimationStart.Immediately),
        [BlockAnimation.MaridiaSandCeiling] = new(BlockAnimation.MaridiaSandCeiling, frameCount: 4, frameDuration: 10, BlockAnimationStart.Immediately),
        [BlockAnimation.MaridiaSandFalling] = new(BlockAnimation.MaridiaSandFalling, frameCount: 4, frameDuration: 10, BlockAnimationStart.Immediately),
        [BlockAnimation.WreckedShipTreadmillRightwards] = new(BlockAnimation.WreckedShipTreadmillRightwards, frameCount: 4, frameDuration: 1, BlockAnimationStart.OnceAreaBossIsDefeated),
        [BlockAnimation.WreckedShipTreadmillLeftwards] = new(BlockAnimation.WreckedShipTreadmillLeftwards, frameCount: 4, frameDuration: 1, BlockAnimationStart.OnceAreaBossIsDefeated),
        [BlockAnimation.Lava] = new(BlockAnimation.Lava, frameCount: 5, frameDuration: 13, BlockAnimationStart.Immediately),
        [BlockAnimation.Acid] = new(BlockAnimation.Acid, frameCount: 5, frameDuration: 10, BlockAnimationStart.Immediately),
        [BlockAnimation.Rain] = new(BlockAnimation.Rain, frameCount: 5, frameDuration: 10, BlockAnimationStart.Immediately),
        [BlockAnimation.Spores] = new(BlockAnimation.Spores, frameCount: 3, frameDuration: 10, BlockAnimationStart.Immediately),
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
