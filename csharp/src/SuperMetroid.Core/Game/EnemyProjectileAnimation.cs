using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Game;

/// <summary>
/// A projectile family's typed animation, run in place of the bank-$86 instruction interpreter (#641).
/// The slot keeps the cartridge's timer gate, so each frame lasts exactly as long as the native frame did.
/// </summary>
internal abstract class EnemyProjectileAnimation
{
    /// <summary>
    /// Advances one projectile pass: the frame expires on the update its native timer would. A cue published
    /// on the way may end the animation (deleting the projectile or handing it to another program); the pass
    /// then stops, as the native instruction that did so would.
    /// </summary>
    internal void Advance(RoomEnemyProjectileSlot projectile, EnemyProjectileHandlerTable handlers, SamusState? samus)
    {
        ushort oldTimer = projectile.InstructionTimer.RawValue;
        projectile.InstructionTimer = NativeCountdown.FromRaw(unchecked((ushort)(oldTimer - 1)));
        if (oldTimer != 1)
            return;
        bool shown = TryShowNextFrame(projectile, handlers, samus, out int duration, out ushort presentationKey);
        if (projectile.Animation != this)
            return;
        if (!shown)
        {
            // A held timeline is the native sleep: the timer stays zero and wraps, never expiring again.
            projectile.InstructionTimer = NativeCountdown.Start(0);
            return;
        }
        projectile.InstructionTimer = NativeCountdown.FromRaw((ushort)duration);
        projectile.PresentationOperandAddress = presentationKey;
        projectile.SpritemapPointer = EnemyProjectileSpritemapDefinitions.BlankSpritemap;
    }

    /// <summary>Advances the timeline to its next frame, or reports that it holds.</summary>
    /// <param name="projectile">The slot the animation runs on, for cues that act on it.</param>
    /// <param name="handlers">The engine's handlers, which carry out published cues.</param>
    /// <param name="samus">Samus, for cues that act on her; null in fixtures without her.</param>
    /// <param name="duration">How many projectile passes the frame lasts.</param>
    /// <param name="presentationKey">The installed artwork key of the frame.</param>
    private protected abstract bool TryShowNextFrame(
        RoomEnemyProjectileSlot projectile, EnemyProjectileHandlerTable handlers, SamusState? samus, out int duration, out ushort presentationKey);
}

/// <summary>
/// A projectile animation that plays one <see cref="AnimationTimeline{TFrame, TCue}"/>: each cue goes to the
/// handler its family registered with <see cref="EnemyProjectileHandlerTable.RegisterCue{TCue}"/>.
/// </summary>
/// <typeparam name="TFrame">The family's frame identity.</typeparam>
/// <typeparam name="TCue">The family's cue type.</typeparam>
internal abstract class EnemyProjectileTimelineAnimation<TFrame, TCue> : EnemyProjectileAnimation
{
    /// <summary>The next step of the timeline.</summary>
    internal int Position { get; private set; }

    /// <summary>The timeline this animation plays.</summary>
    private protected abstract AnimationTimeline<TFrame, TCue> Timeline { get; }

    /// <summary>The installed artwork key of <paramref name="frame"/>.</summary>
    private protected abstract ushort PresentationKey(TFrame frame);

    /// <summary>Plays the timeline, publishing each cue to the family's registered handler.</summary>
    private protected sealed override bool TryShowNextFrame(
        RoomEnemyProjectileSlot projectile, EnemyProjectileHandlerTable handlers, SamusState? samus, out int duration, out ushort presentationKey)
    {
        int position = Position;
        AnimationAdvance<TFrame> advance = Timeline.Advance(ref position, cue => handlers.PublishCue(projectile, samus, cue));
        Position = position;
        duration = advance.Duration;
        presentationKey = advance.Held ? (ushort)0 : PresentationKey(advance.Frame);
        return !advance.Held;
    }
}

/// <summary>
/// A projectile animation that runs programs of an <see cref="AnimationProgramSet{TProgram, TFrame, TCue}"/>: for
/// families whose native lists run on into, or jump between, one another. Each cue goes to the handler its family
/// registered with <see cref="EnemyProjectileHandlerTable.RegisterCue{TCue}"/>; projectile programs never redirect.
/// </summary>
/// <typeparam name="TProgram">The family's program identity.</typeparam>
/// <typeparam name="TFrame">The family's frame identity.</typeparam>
/// <typeparam name="TCue">The family's cue type.</typeparam>
internal abstract class EnemyProjectileProgramAnimation<TProgram, TFrame, TCue> : EnemyProjectileAnimation
    where TProgram : struct, Enum
{
    /// <summary>Starts <paramref name="program"/>.</summary>
    private protected EnemyProjectileProgramAnimation(TProgram program) => Cursor = AnimationProgramCursor<TProgram>.Start(program);

    /// <summary>Where the program resumes.</summary>
    internal AnimationProgramCursor<TProgram> Cursor { get; private set; }

    /// <summary>The program set this animation runs.</summary>
    private protected abstract AnimationProgramSet<TProgram, TFrame, TCue> Programs { get; }

    /// <summary>The installed artwork key of <paramref name="frame"/>.</summary>
    private protected abstract ushort PresentationKey(TFrame frame);

    /// <summary>Runs the program, publishing each cue to the family's registered handler.</summary>
    private protected sealed override bool TryShowNextFrame(
        RoomEnemyProjectileSlot projectile, EnemyProjectileHandlerTable handlers, SamusState? samus, out int duration, out ushort presentationKey)
    {
        AnimationProgramCursor<TProgram> cursor = Cursor;
        // The projectile interpreter has no loop timer, so projectile programs hold no counter steps.
        NativeCountdown counter = NativeCountdown.Start(0);
        AnimationProgramAdvance<TFrame> advance = Programs.Advance(ref cursor, ref counter, (cue, continuation) =>
        {
            handlers.PublishCue(projectile, samus, cue);
            return null;
        });
        Cursor = cursor;
        duration = advance.Duration;
        switch (advance.Kind)
        {
            case AnimationProgramAdvanceKind.Show:
                presentationKey = PresentationKey(advance.Frame);
                return true;
            case AnimationProgramAdvanceKind.Hold:
                presentationKey = 0;
                return false;
            default:
                throw new InvalidOperationException($"A projectile program ended an update with {advance.Kind}.");
        }
    }
}

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
