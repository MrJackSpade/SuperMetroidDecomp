using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Bosses.Botwoon.Artwork;

namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>A Botwoon body segment's or spit's typed animation.</summary>
internal sealed class BotwoonProjectileAnimation : EnemyProjectileTypedAnimation<BotwoonSegmentProgram, BotwoonProjectileFrame, NoAnimationCue>
{
    /// <summary>Starts the animation of <paramref name="program"/>.</summary>
    private BotwoonProjectileAnimation(BotwoonSegmentProgram program)
        : base(BotwoonAnimations.ProjectileStartingWith(program))
    {
    }

    /// <summary>The segment program shown, or null for a spit.</summary>
    internal BotwoonSegmentProgram? Segment => PlayingSegment == BotwoonSegmentProgram.Spit ? null : PlayingSegment;

    /// <summary>A spit's cycling animation.</summary>
    internal static BotwoonProjectileAnimation Spit() => new(BotwoonSegmentProgram.Spit);

    /// <summary>A body segment showing <paramref name="program"/>.</summary>
    internal static BotwoonProjectileAnimation ForSegment(BotwoonSegmentProgram program) =>
        program == BotwoonSegmentProgram.Spit
            ? throw new ArgumentOutOfRangeException(nameof(program), program, "A body segment does not run the spit program.")
            : new(program);

    /// <summary>
    /// Switches a segment to <paramref name="program"/>'s animation from its first frame. An unchanged program keeps
    /// running, as the game only restarts an animation that differs from the one it is showing.
    /// </summary>
    /// <returns>Whether the program changed.</returns>
    internal bool Install(BotwoonSegmentProgram program)
    {
        if (Segment is not { } segment)
            throw new InvalidOperationException("A Botwoon spit animation has no segment program to replace.");
        if (program == BotwoonSegmentProgram.Spit)
            throw new ArgumentOutOfRangeException(nameof(program), program, "A body segment does not run the spit program.");
        if (segment == program)
            return false;
        Playback = new(BotwoonAnimations.ProjectileStartingWith(program), AnimationPlace.StartOfAnimation);
        return true;
    }

    /// <inheritdoc/>
    private protected override AnimationSegments<BotwoonSegmentProgram, BotwoonProjectileFrame, NoAnimationCue> Segments =>
        BotwoonAnimations.ProjectileSegments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<BotwoonSegmentProgram, Animation<BotwoonSegmentProgram>> StartingAt =>
        BotwoonAnimations.ProjectileStartingAt;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(BotwoonProjectileFrame frame) =>
        BotwoonVisualDefinitions.ShownProjectileFrames[frame];
}
