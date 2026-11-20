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
        [BlockAnimation.HorizontalSpikes] = IdleLoop(BlockAnimation.HorizontalSpikes, frames: 4, updates: 8),
        [BlockAnimation.MaridiaSandCeiling] = IdleLoop(BlockAnimation.MaridiaSandCeiling, frames: 4, updates: 10),
        [BlockAnimation.MaridiaSandFalling] = IdleLoop(BlockAnimation.MaridiaSandFalling, frames: 4, updates: 10),
        [BlockAnimation.WreckedShipTreadmillRightwards] = IdleLoop(BlockAnimation.WreckedShipTreadmillRightwards, frames: 4, updates: 1),
        [BlockAnimation.WreckedShipTreadmillLeftwards] = IdleLoop(BlockAnimation.WreckedShipTreadmillLeftwards, frames: 4, updates: 1),
        [BlockAnimation.Lava] = IdleLoop(BlockAnimation.Lava, frames: 5, updates: 13),
        [BlockAnimation.Acid] = IdleLoop(BlockAnimation.Acid, frames: 5, updates: 10),
        [BlockAnimation.Rain] = IdleLoop(BlockAnimation.Rain, frames: 5, updates: 10),
        [BlockAnimation.Spores] = IdleLoop(BlockAnimation.Spores, frames: 3, updates: 10),
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

    /// <summary>
    /// An idle loop of <paramref name="animation"/>'s first <paramref name="frames"/> idle frames, "idle 0" on, each shown for
    /// <paramref name="updates"/> updates.
    /// </summary>
    private static AnimationBeat<ArtworkFrame, NoAnimationCue>[] IdleLoop(BlockAnimation animation, int frames, int updates) =>
        [.. Enumerable.Range(0, frames).Select(frame => new Idle(BlockAnimationFrames.Frame(animation, $"idle {frame}"), updates))];
}
