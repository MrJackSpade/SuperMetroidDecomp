using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Bosses.Botwoon.Artwork;

namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>A Botwoon body segment's or spit's typed animation.</summary>
internal sealed class BotwoonProjectileAnimation : EnemyProjectileProgramAnimation<BotwoonSegmentProgram, BotwoonProjectileFrame, NoAnimationCue>
{
    /// <summary>Starts <paramref name="program"/>.</summary>
    private BotwoonProjectileAnimation(BotwoonSegmentProgram program)
        : base(program)
    {
    }

    /// <summary>The segment program shown, or null for a spit.</summary>
    internal BotwoonSegmentProgram? Segment => Cursor.Program == BotwoonSegmentProgram.Spit ? null : Cursor.Program;

    /// <summary>A spit's cycling animation.</summary>
    internal static BotwoonProjectileAnimation Spit() => new(BotwoonSegmentProgram.Spit);

    /// <summary>A body segment showing <paramref name="program"/>.</summary>
    internal static BotwoonProjectileAnimation ForSegment(BotwoonSegmentProgram program) =>
        program == BotwoonSegmentProgram.Spit
            ? throw new ArgumentOutOfRangeException(nameof(program), program, "A body segment does not run the spit program.")
            : new(program);

    /// <summary>
    /// Switches a segment to <paramref name="program"/> from its first frame. An unchanged program keeps
    /// running, as the cartridge only reinstalls a list that differs from the one it installed.
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
        Cursor = AnimationProgramCursor<BotwoonSegmentProgram>.Start(program);
        return true;
    }

    /// <inheritdoc/>
    private protected override AnimationProgramSet<BotwoonSegmentProgram, BotwoonProjectileFrame, NoAnimationCue> Programs =>
        BotwoonAnimations.SegmentPrograms;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(BotwoonProjectileFrame frame) =>
        BotwoonVisualDefinitions.ShownProjectileFrames[frame];
}
