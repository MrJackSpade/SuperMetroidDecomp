using System.Collections.Frozen;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Assets;
using Idle = SuperMetroid.Core.Animation.AnimationBeat<SuperMetroid.Core.Assets.ArtworkFrame, SuperMetroid.Core.Animation.NoAnimationCue>.Frame;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// The looping block animations (#1988, #2361, #2422): each animation's idle loop, its frames authored in the order it shows
/// them with the passes each is shown for; the loops that wait for the area's boss; the animation a room effect's type shows
/// on the effect overlay; and the treadmill animation a Wrecked Ship entrance door starts for each direction.
/// </summary>
/// <remarks>
/// Each loop is its animation's idle loop, the one a block animation always plays (#2370): its frames are named idle 0,
/// idle 1 and on, in the order it shows them. What each frame pictures is installed artwork, which asset extraction
/// pictures under the frame's name; this catalog owns only each loop's frames, their durations and its start.
/// </remarks>
public static class BlockAnimationLoopDefinitions
{
    /// <summary>The idle loops, by animation, each the one segment its animation loops through (<see cref="BlockAnimationLoops"/>).</summary>
    internal static readonly FrozenDictionary<BlockAnimation, AnimationBeat<ArtworkFrame, NoAnimationCue>[]> IdleLoops =
        new Dictionary<BlockAnimation, AnimationBeat<ArtworkFrame, NoAnimationCue>[]>
    {
        [BlockAnimation.HorizontalSpikes] =
        [
            new Idle(BlockAnimationFrames.Frame(BlockAnimation.HorizontalSpikes, "idle 0"), 8),
            new Idle(BlockAnimationFrames.Frame(BlockAnimation.HorizontalSpikes, "idle 1"), 8),
            new Idle(BlockAnimationFrames.Frame(BlockAnimation.HorizontalSpikes, "idle 2"), 8),
            new Idle(BlockAnimationFrames.Frame(BlockAnimation.HorizontalSpikes, "idle 3"), 8),
        ],
        [BlockAnimation.MaridiaSandCeiling] =
        [
            new Idle(BlockAnimationFrames.Frame(BlockAnimation.MaridiaSandCeiling, "idle 0"), 10),
            new Idle(BlockAnimationFrames.Frame(BlockAnimation.MaridiaSandCeiling, "idle 1"), 10),
            new Idle(BlockAnimationFrames.Frame(BlockAnimation.MaridiaSandCeiling, "idle 2"), 10),
            new Idle(BlockAnimationFrames.Frame(BlockAnimation.MaridiaSandCeiling, "idle 3"), 10),
        ],
        [BlockAnimation.MaridiaSandFalling] =
        [
            new Idle(BlockAnimationFrames.Frame(BlockAnimation.MaridiaSandFalling, "idle 0"), 10),
            new Idle(BlockAnimationFrames.Frame(BlockAnimation.MaridiaSandFalling, "idle 1"), 10),
            new Idle(BlockAnimationFrames.Frame(BlockAnimation.MaridiaSandFalling, "idle 2"), 10),
            new Idle(BlockAnimationFrames.Frame(BlockAnimation.MaridiaSandFalling, "idle 3"), 10),
        ],
        [BlockAnimation.WreckedShipTreadmillRightwards] =
        [
            new Idle(BlockAnimationFrames.Frame(BlockAnimation.WreckedShipTreadmillRightwards, "idle 0"), 1),
            new Idle(BlockAnimationFrames.Frame(BlockAnimation.WreckedShipTreadmillRightwards, "idle 1"), 1),
            new Idle(BlockAnimationFrames.Frame(BlockAnimation.WreckedShipTreadmillRightwards, "idle 2"), 1),
            new Idle(BlockAnimationFrames.Frame(BlockAnimation.WreckedShipTreadmillRightwards, "idle 3"), 1),
        ],
        [BlockAnimation.WreckedShipTreadmillLeftwards] =
        [
            new Idle(BlockAnimationFrames.Frame(BlockAnimation.WreckedShipTreadmillLeftwards, "idle 0"), 1),
            new Idle(BlockAnimationFrames.Frame(BlockAnimation.WreckedShipTreadmillLeftwards, "idle 1"), 1),
            new Idle(BlockAnimationFrames.Frame(BlockAnimation.WreckedShipTreadmillLeftwards, "idle 2"), 1),
            new Idle(BlockAnimationFrames.Frame(BlockAnimation.WreckedShipTreadmillLeftwards, "idle 3"), 1),
        ],
        [BlockAnimation.Lava] =
        [
            new Idle(BlockAnimationFrames.Frame(BlockAnimation.Lava, "idle 0"), 13),
            new Idle(BlockAnimationFrames.Frame(BlockAnimation.Lava, "idle 1"), 13),
            new Idle(BlockAnimationFrames.Frame(BlockAnimation.Lava, "idle 2"), 13),
            new Idle(BlockAnimationFrames.Frame(BlockAnimation.Lava, "idle 3"), 13),
            new Idle(BlockAnimationFrames.Frame(BlockAnimation.Lava, "idle 4"), 13),
        ],
        [BlockAnimation.Acid] =
        [
            new Idle(BlockAnimationFrames.Frame(BlockAnimation.Acid, "idle 0"), 10),
            new Idle(BlockAnimationFrames.Frame(BlockAnimation.Acid, "idle 1"), 10),
            new Idle(BlockAnimationFrames.Frame(BlockAnimation.Acid, "idle 2"), 10),
            new Idle(BlockAnimationFrames.Frame(BlockAnimation.Acid, "idle 3"), 10),
            new Idle(BlockAnimationFrames.Frame(BlockAnimation.Acid, "idle 4"), 10),
        ],
        [BlockAnimation.Rain] =
        [
            new Idle(BlockAnimationFrames.Frame(BlockAnimation.Rain, "idle 0"), 10),
            new Idle(BlockAnimationFrames.Frame(BlockAnimation.Rain, "idle 1"), 10),
            new Idle(BlockAnimationFrames.Frame(BlockAnimation.Rain, "idle 2"), 10),
            new Idle(BlockAnimationFrames.Frame(BlockAnimation.Rain, "idle 3"), 10),
            new Idle(BlockAnimationFrames.Frame(BlockAnimation.Rain, "idle 4"), 10),
        ],
        [BlockAnimation.Spores] =
        [
            new Idle(BlockAnimationFrames.Frame(BlockAnimation.Spores, "idle 0"), 10),
            new Idle(BlockAnimationFrames.Frame(BlockAnimation.Spores, "idle 1"), 10),
            new Idle(BlockAnimationFrames.Frame(BlockAnimation.Spores, "idle 2"), 10),
        ],
    }.ToFrozenDictionary();

    /// <summary>
    /// The loops that wait, showing nothing, until a pass on which the area's boss is recorded defeated, and show their first
    /// frame on that pass, never testing the boss again: the Wrecked Ship treadmills wait for Phantoon. Every other loop
    /// shows its first frame on its first pass.
    /// </summary>
    internal static readonly FrozenSet<BlockAnimation> AwaitingAreaBoss =
        [BlockAnimation.WreckedShipTreadmillRightwards, BlockAnimation.WreckedShipTreadmillLeftwards];

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
