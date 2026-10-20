using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Bosses.Botwoon.Artwork;

namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>A Botwoon body segment's or spit's typed animation.</summary>
internal sealed class BotwoonProjectileAnimation : EnemyProjectileTimelineAnimation<BotwoonProjectileFrame, NoAnimationCue>
{
    /// <summary>Uses a segment program when present; null selects the looping spit timeline.</summary>
    private BotwoonProjectileAnimation(BotwoonSegmentProgram? segment) => Segment = segment;

    /// <summary>The segment program shown, or null for a spit.</summary>
    internal BotwoonSegmentProgram? Segment { get; private set; }

    /// <summary>A spit's cycling animation.</summary>
    internal static BotwoonProjectileAnimation Spit() => new(null);

    /// <summary>A body segment showing <paramref name="program"/>.</summary>
    internal static BotwoonProjectileAnimation ForSegment(BotwoonSegmentProgram program) => new(program);

    /// <summary>
    /// Switches a segment to <paramref name="program"/> from its first frame. An unchanged program keeps
    /// running, as the cartridge only reinstalls a list that differs from the one it installed.
    /// </summary>
    /// <returns>Whether the program changed.</returns>
    internal bool Install(BotwoonSegmentProgram program)
    {
        if (Segment is null)
            throw new InvalidOperationException("A Botwoon spit animation has no segment program to replace.");
        if (Segment == program)
            return false;
        Segment = program;
        Restart();
        return true;
    }

    /// <inheritdoc/>
    private protected override AnimationTimeline<BotwoonProjectileFrame, NoAnimationCue> Timeline =>
        Segment is { } segment ? BotwoonAnimations.Segment(segment) : BotwoonAnimations.Spit;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(BotwoonProjectileFrame frame) =>
        BotwoonVisualDefinitions.ShownProjectileFrames[frame];
}
