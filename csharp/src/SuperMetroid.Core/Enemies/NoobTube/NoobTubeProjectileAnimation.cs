using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.NoobTube;

/// <summary>
/// A n00b tube piece's typed animation: the crack's or a bubble's, which start at their segments, or a glass shard's, which
/// its spawn starts by the shard (<see cref="NoobTubeProjectileAnimations.ShardAnimations"/>, #2044).
/// </summary>
internal sealed partial class NoobTubeProjectileAnimation
    : EnemyProjectileTypedAnimation<NoobTubeProjectileSegment, NoobTubeProjectileFrame, NoobTubeProjectileCue>
{
    /// <summary>Whether the animation shows its mirrored frames, which its owner sets as the shard's flicker reflects it.</summary>
    private readonly NoobTubeShardMirroring _mirroring;

    /// <summary>A piece's animation from <paramref name="animation"/>, whose events <paramref name="owner"/> carries out.</summary>
    /// <param name="owner">The tube pieces' behaviour, which carries out the events the animation raises (#2571).</param>
    /// <param name="animation">The animation the piece plays from its start.</param>
    internal NoobTubeProjectileAnimation(NoobTubeBehavior.NoobTubeProjectileBehavior owner, Animation<NoobTubeProjectileSegment> animation)
        : this(owner, animation, new NoobTubeShardMirroring())
    {
    }

    /// <summary>A piece's animation whose runner hands <paramref name="owner"/> the animation's <paramref name="mirroring"/>.</summary>
    private NoobTubeProjectileAnimation(
        NoobTubeBehavior.NoobTubeProjectileBehavior owner, Animation<NoobTubeProjectileSegment> animation, NoobTubeShardMirroring mirroring)
        : base(animation.Phases[0].Segment, animation, new PieceCues(Ensure.NotNull(owner), mirroring)) => _mirroring = mirroring;
    /// <inheritdoc/>
    private protected override IUnpausedAnimationPlay<NoobTubeProjectileSegment, NoobTubeProjectileFrame, NoobTubeProjectileCue> Segments => NoobTubeProjectileAnimations.Segments;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(NoobTubeProjectileFrame frame)
    {
        Showing = frame;
        return ShowsMirroredFrame && frame.Mirrored is { } mirrored ? mirrored : frame.Shown;
    }

    /// <summary>
    /// The beat the animation shows now: the shard's frame and, for a shard with one, its mirror image's, which a shard drawn
    /// whole draws both of on every update (#2639).
    /// </summary>
    internal NoobTubeProjectileFrame? Showing { get; private set; }

    /// <summary>
    /// Whether the last flicker cue chose the shard's mirror image: a mirroring flicker shows it on even updates and the
    /// shard itself on odd ones.
    /// </summary>
    internal bool ShowsMirroredFrame => _mirroring.ShowsMirroredFrame;

    /// <summary>Hands each event the piece's animation raises to the tube pieces' behaviour with the animation's mirroring.</summary>
    /// <param name="owner">The tube pieces' behaviour.</param>
    /// <param name="mirroring">The animation's mirroring.</param>
    private sealed partial class PieceCues(NoobTubeBehavior.NoobTubeProjectileBehavior owner, NoobTubeShardMirroring mirroring)
        : EnemyProjectileCueRunner<NoobTubeProjectileSegment, NoobTubeProjectileCue>
    {
        /// <inheritdoc/>
        internal override IUnpausedHidingAnimationShown<NoobTubeProjectileSegment, TFrame> PlayToNextFrame<TFrame>(
            IUnpausedAnimationPlay<NoobTubeProjectileSegment, TFrame, NoobTubeProjectileCue> segments, EnemyProjectileCueContext context,
            ref Animation<NoobTubeProjectileSegment> animation, ref AnimationPlace place) =>
            Ensure.NotNull(segments).PlayToNextFrame(ref animation, ref place, (cue, _) =>
            {
                owner.RunCue(context, cue, mirroring);
                return null;
            });
    }
}
