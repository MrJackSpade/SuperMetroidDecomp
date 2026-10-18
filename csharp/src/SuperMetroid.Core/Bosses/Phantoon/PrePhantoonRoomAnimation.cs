using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Bosses.Phantoon.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>The pre-Phantoon room projectile's typed animation: one blank frame held 32 passes, then deletion (#641).</summary>
internal sealed class PrePhantoonRoomAnimation : EnemyProjectileTimelineAnimation<PrePhantoonRoomFrame, PrePhantoonRoomCue>
{
    /// <summary>How long the blank frame lasts ($86:A3AA).</summary>
    private const int HoldDuration = 32;

    /// <summary>The blank frame, its deletion, and the hold that follows it.</summary>
    private static readonly AnimationTimeline<PrePhantoonRoomFrame, PrePhantoonRoomCue> Program = new(
        AnimationStep<PrePhantoonRoomFrame, PrePhantoonRoomCue>.Show(default, HoldDuration),
        AnimationStep<PrePhantoonRoomFrame, PrePhantoonRoomCue>.Publish(PrePhantoonRoomCue.Delete),
        AnimationStep<PrePhantoonRoomFrame, PrePhantoonRoomCue>.Hold);

    /// <inheritdoc/>
    private protected override AnimationTimeline<PrePhantoonRoomFrame, PrePhantoonRoomCue> Timeline => Program;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(PrePhantoonRoomFrame frame) =>
        EnemyProjectileFrameId.Blank;
}