using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Bosses.Botwoon.Artwork;

namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>A Botwoon body segment's or spit's typed animation.</summary>
internal sealed class BotwoonProjectileAnimation : EnemyProjectileAnimation
{
    /// <summary>Uses a segment program when present; null selects the looping spit timeline.</summary>
    private BotwoonProjectileAnimation(BotwoonSegmentProgram? segment) => Segment = segment;

    /// <summary>The segment program shown, or null for a spit.</summary>
    internal BotwoonSegmentProgram? Segment { get; private set; }

    /// <summary>The next step of the timeline.</summary>
    internal int Position { get; private set; }

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
        Position = 0;
        return true;
    }

    /// <summary>Advances the segment or spit timeline and maps its frame to installed projectile artwork.</summary>
    private protected override bool TryShowNextFrame(
        RoomEnemyProjectileSlot projectile, EnemyProjectileHandlerTable handlers, SamusState? samus, out int duration, out ushort presentationKey)
    {
        var timeline = Segment is { } segment ? BotwoonAnimations.Segment(segment) : BotwoonAnimations.Spit;
        int position = Position;
        AnimationAdvance<BotwoonProjectileFrame> advance = timeline.Advance(ref position, NoCues);
        Position = position;
        duration = advance.Duration;
        presentationKey = advance.Held ? (ushort)0 : BotwoonVisualDefinitions.ProjectilePresentationKey(advance.Frame);
        return !advance.Held;
    }

    /// <summary>Rejects unexpected timeline cues; Botwoon projectile frames carry only pose and duration.</summary>
    private static void NoCues(NoAnimationCue cue) =>
        throw new InvalidOperationException($"A Botwoon projectile timeline published cue {cue}.");
}
