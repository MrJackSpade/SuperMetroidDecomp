using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Game;

/// <summary>
/// A projectile family's typed animation, run in place of the bank-$86 instruction interpreter (#641).
/// The slot keeps the cartridge's timer gate, so each frame lasts exactly as long as the native frame did.
/// </summary>
internal abstract class EnemyProjectileAnimation
{
    /// <summary>Advances one projectile pass: the frame expires on the update its native timer would.</summary>
    internal void Advance(RoomEnemyProjectileSlot projectile)
    {
        ushort oldTimer = projectile.InstructionTimer;
        projectile.InstructionTimer = unchecked((ushort)(oldTimer - 1));
        if (oldTimer != 1)
            return;
        if (!TryShowNextFrame(out int duration, out ushort presentationKey))
        {
            // A held timeline is the native sleep: the timer stays zero and wraps, never expiring again.
            projectile.InstructionTimer = 0;
            return;
        }
        projectile.InstructionTimer = (ushort)duration;
        projectile.PresentationOperandAddress = presentationKey;
        projectile.SpritemapPointer = EnemyProjectileSpritemapDefinitions.BlankSpritemap;
    }

    /// <summary>Advances the timeline to its next frame, or reports that it holds.</summary>
    /// <param name="duration">How many projectile passes the frame lasts.</param>
    /// <param name="presentationKey">The installed artwork key of the frame.</param>
    private protected abstract bool TryShowNextFrame(out int duration, out ushort presentationKey);
}

/// <summary>A Botwoon body segment's or spit's typed animation.</summary>
internal sealed class BotwoonProjectileAnimation : EnemyProjectileAnimation
{
    /// <summary>Initializes a new botwoon projectile animation instance.</summary>
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

    /// <summary>try show next frame.</summary>
    private protected override bool TryShowNextFrame(out int duration, out ushort presentationKey)
    {
        var timeline = Segment is { } segment ? BotwoonAnimations.Segment(segment) : BotwoonAnimations.Spit;
        int position = Position;
        AnimationAdvance<BotwoonProjectileFrame> advance = timeline.Advance(ref position, NoCues);
        Position = position;
        duration = advance.Duration;
        presentationKey = advance.Held ? (ushort)0 : BotwoonVisualDefinitions.ProjectilePresentationKey(advance.Frame);
        return !advance.Held;
    }

    /// <summary>no cues.</summary>
    private static void NoCues(NoAnimationCue cue) =>
        throw new InvalidOperationException($"A Botwoon projectile timeline published cue {cue}.");
}
