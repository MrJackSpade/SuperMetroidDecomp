using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// The game's layout of Samus's animations (#1473, #1535, #2048), which only the import knows. The game numbers an
/// animation's frames from its start, one after another through its segments, each as long as its frames, its cue and the
/// values the cue carries; it lays the animations' programs out one after another in the cartridge; and it reads a frame
/// as a byte of that layout, so a frame on a value a cue carries reads the value as a step and a frame past an animation's
/// end reads a later animation. The import decodes that reading once into the named outcomes the game holds, each keyed
/// by the animation's own name (<see cref="DocumentCueValueSteps"/>, <see cref="ReadsPast"/>, <see cref="PlaceLandings"/>),
/// and the analysis of the frames Samus's body can show follows it here.
/// </summary>
internal sealed class SamusAnimationLayout
{
    /// <summary>A step read for an animation: the step, the animation it was read in, its frame there and how far that animation lies after the one read.</summary>
    /// <param name="Step">The step read.</param>
    /// <param name="Animation">The animation the step was read in.</param>
    /// <param name="Frame">The frame of that animation the step lies at, counted from its start.</param>
    /// <param name="Shift">How many frames after the read animation that animation starts; zero for the read animation itself.</param>
    internal readonly record struct StepRead(SamusPoseAnimationStep Step, SamusPoseAnimation Animation, int Frame, int Shift);

    /// <summary>A place of an animation's layout: the animation and how far into the game's layout of it the place lies.</summary>
    /// <param name="Animation">The animation.</param>
    /// <param name="Offset">How far into the layout of the animation the place lies.</param>
    private readonly record struct LayoutPlace(SamusPoseAnimation Animation, int Offset);

    /// <summary>A program laid out after another: its animation and how many frames later it starts.</summary>
    private readonly record struct Successor(SamusPoseAnimation Animation, int Shift);

    /// <summary>Every program's animation, in the order of their names: the installation holds each animation's outcomes once, under its name.</summary>
    private readonly SamusPoseAnimation[] programs;

    /// <summary>Each animation's program's successor, or none for the last program laid out.</summary>
    private readonly Dictionary<SamusPoseAnimation, Successor?> successors;

    /// <summary>The step each cue value's frame of each animation reads, by frame counted from the program's start.</summary>
    private readonly Dictionary<SamusPoseAnimation, IReadOnlyDictionary<int, SamusPoseAnimationStep>> cueValueSteps;

    /// <summary>The installed form of each animation's cue-value steps, by the frame's name.</summary>
    private readonly Dictionary<SamusPoseAnimation, IReadOnlyDictionary<string, SamusOverreadDocument.CueValueStep>> documentCueValueSteps;

    /// <summary>Keeps the layout read from the cartridge.</summary>
    private SamusAnimationLayout(SamusPoseAnimation[] programs, Dictionary<SamusPoseAnimation, Successor?> successors,
        Dictionary<SamusPoseAnimation, IReadOnlyDictionary<int, SamusPoseAnimationStep>> cueValueSteps,
        Dictionary<SamusPoseAnimation, IReadOnlyDictionary<string, SamusOverreadDocument.CueValueStep>> documentCueValueSteps)
    {
        this.programs = programs;
        this.successors = successors;
        this.cueValueSteps = cueValueSteps;
        this.documentCueValueSteps = documentCueValueSteps;
    }

    /// <summary>Reads the layout of every program and decodes the step every cue value's frame reads.</summary>
    /// <exception cref="InvalidDataException">Two records of one program start apart, two programs start together, or a cue value names no frame.</exception>
    internal static SamusAnimationLayout Read(IImportCartridgeSource cartridge)
    {
        Ensure.NotNull(cartridge);
        SamusAnimationProgramLayout.LaidOutProgram[] laidOut = SamusAnimationProgramLayout.Read(cartridge);
        var successors = new Dictionary<SamusPoseAnimation, Successor?>();
        for (int index = 0; index < laidOut.Length; index++)
        {
            successors.Add(laidOut[index].Program, index + 1 < laidOut.Length
                ? new Successor(laidOut[index + 1].Program, laidOut[index + 1].Start - laidOut[index].Start)
                : null);
        }
        Dictionary<SamusPoseAnimation, IReadOnlyDictionary<int, SamusAnimationCueValueDecoding.DecodedStep>> decoded =
            SamusAnimationCueValueDecoding.CueValueSteps(cartridge, laidOut);
        var steps = new Dictionary<SamusPoseAnimation, IReadOnlyDictionary<int, SamusPoseAnimationStep>>();
        var documentSteps = new Dictionary<SamusPoseAnimation, IReadOnlyDictionary<string, SamusOverreadDocument.CueValueStep>>();
        foreach (SamusAnimationProgramLayout.LaidOutProgram program in laidOut)
        {
            SamusPoseAnimation animation = program.Program;
            Dictionary<int, SamusOverreadDocument.CueValueStep> named = decoded[animation].ToDictionary(pair => pair.Key, pair => Named(animation, pair.Value));
            documentSteps.Add(animation, named.ToDictionary(pair => animation.NameOf(FrameAt(animation, pair.Key)), pair => pair.Value, StringComparer.Ordinal));
            steps.Add(animation, named.ToDictionary(pair => pair.Key, pair => SamusOverreadServicesReader.CueValueStep(animation, pair.Value)));
        }
        var names = new WrittenOrder<SamusAnimationName>(Ensure.EveryMemberOnce(SamusAnimationNameKeys.ListedOrder));
        SamusPoseAnimation[] programs = [.. laidOut.Select(program => program.Program).OrderBy(animation => names.PlaceOf(animation.Name))];
        return new SamusAnimationLayout(programs, successors, steps, documentSteps);
    }

    /// <summary>How far into the game's layout of <paramref name="animation"/> <paramref name="frame"/> lies.</summary>
    /// <exception cref="ArgumentException">The animation has no such frame, or the frame lies past its segment's span.</exception>
    internal static int Offset(SamusPoseAnimation animation, SamusAnimationFrame frame)
    {
        Ensure.NotNull(animation);
        int index = IndexOf(animation, animation.Require(frame).Segment);
        if (index < animation.Segments.Count - 1 && frame.Frame >= Span(animation, index))
            throw new ArgumentException($"Segment {frame.Segment} of the {animation.Name} animation spans no frame {frame.Frame}.", nameof(frame));
        int start = 0;
        for (int segment = 0; segment < index; segment++)
            start += Span(animation, segment);
        return start + frame.Frame;
    }

    /// <summary>How far into the game's layout of its own animation <paramref name="place"/> lies, which it lands at in any other.</summary>
    internal static int Offset(SamusAnimationPlace place) => Offset(place.Animation, place.Frame);

    /// <summary>The frame of <paramref name="animation"/> lying <paramref name="offset"/> frames into the game's layout of it.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The offset is negative.</exception>
    internal static SamusAnimationFrame FrameAt(SamusPoseAnimation animation, int offset)
    {
        Ensure.NotNull(animation);
        Ensure.AtLeastZero(offset);
        int start = 0;
        for (int segment = 0; segment < animation.Segments.Count - 1; segment++)
        {
            int span = Span(animation, segment);
            if (offset < start + span)
                return new SamusAnimationFrame(animation.Segments[segment].Name, offset - start);
            start += span;
        }
        return new SamusAnimationFrame(animation.Segments[^1].Name, offset - start);
    }

    /// <summary>How many frames <paramref name="animation"/>'s own steps span in its layout, its last end cue included.</summary>
    internal static int StepCount(SamusPoseAnimation animation)
    {
        Ensure.NotNull(animation);
        SamusAnimationSegment last = animation.Segments[^1];
        return Offset(animation, new SamusAnimationFrame(last.Name, last.CueFrame)) + 1;
    }

    /// <summary>How many frames <paramref name="animation"/>'s segments span in its layout, the values its last cue carries included.</summary>
    internal static int Length(SamusPoseAnimation animation) =>
        Enumerable.Range(0, Ensure.NotNull(animation).Segments.Count).Sum(segment => Span(animation, segment));

    /// <summary>
    /// How many frames of the game's layout the segment of <paramref name="animation"/> at <paramref name="index"/> spans
    /// (#2395, #2530): its delays, its end cue, the frames its cue's operands take and the runs no segment names laid out
    /// after it; a lead, which the game lays out in one run with the segment after it, spans its delays alone.
    /// </summary>
    private static int Span(SamusPoseAnimation animation, int index)
    {
        SamusAnimationSegment segment = animation.Segments[index];
        if (segment.End is not { } end)
            return segment.Delays.Count;
        return segment.CueFrame + 1 + SamusAnimationCueValueDecoding.OperandFrames(end) + SamusAnimationUnnamedRunDefinitions.All
            .Where(run => run.Animation == animation.Name && run.After == segment.Name)
            .Sum(run => run.Delays + 1 + SamusAnimationCueValueDecoding.OperandFrames(run.End));
    }

    /// <summary>The place of the segment named <paramref name="name"/> among <paramref name="animation"/>'s segments.</summary>
    /// <exception cref="ArgumentException">The animation has no such segment.</exception>
    private static int IndexOf(SamusPoseAnimation animation, SamusAnimationSegmentName name)
    {
        for (int index = 0; index < animation.Segments.Count; index++)
        {
            if (animation.Segments[index].Name == name)
                return index;
        }
        throw new ArgumentException($"The {animation.Name} animation has no segment {name}.", nameof(name));
    }

    /// <summary>
    /// The frame every frame on or past the cue ending a segment other than an animation's last goes on to, by the
    /// animation's name and the frame's name (#2395): the next value the cue carries, an unnamed run's frame, or the next
    /// segment's first frame once they are behind it. A cue that continues into the next segment goes on without one.
    /// </summary>
    /// <exception cref="InvalidDataException">A cue that continues into the next segment has frames laid out after it.</exception>
    internal Dictionary<string, IReadOnlyDictionary<string, string>> FramesAfterCues() =>
        programs.ToDictionary(animation => SamusAnimationNameKeys.Installed.Of(animation.Name), animation =>
        {
            var after = new Dictionary<string, string>(StringComparer.Ordinal);
            int start = 0;
            for (int index = 0; index < animation.Segments.Count - 1; index++)
            {
                SamusAnimationSegment segment = animation.Segments[index];
                int cue = start + segment.CueFrame;
                int next = start + Span(animation, index);
                // A lead has no cue: its last frame plays on into the next segment's first.
                if (segment.End is { } end)
                {
                    if (end.ContinuesToNextSegment && next != cue + 1)
                        throw new InvalidDataException($"The {animation.Name} animation's {segment.Name} cue continues into the next segment past frames laid out after it.");
                    for (int offset = end.ContinuesToNextSegment ? cue + 1 : cue; offset < next; offset++)
                        after.Add(animation.NameOf(FrameAt(animation, offset)), animation.NameOf(FrameAt(animation, offset + 1)));
                }
                start = next;
            }
            return (IReadOnlyDictionary<string, string>)after;
        }, StringComparer.Ordinal);

    /// <summary>The step of <paramref name="animation"/>'s own at <paramref name="offset"/>, or null for a cue's value or a frame past it.</summary>
    internal static SamusPoseAnimationStep? OwnStepAt(SamusPoseAnimation animation, int offset) =>
        offset >= 0 ? Ensure.NotNull(animation).TryStepAt(FrameAt(animation, offset)) : null;

    /// <summary>
    /// The step at <paramref name="offset"/> frames into the game's layout of <paramref name="animation"/>, reading a cue's
    /// value or past the animation's end as the game does; null where the game's animation fails.
    /// </summary>
    internal StepRead? Read(SamusPoseAnimation animation, int offset)
    {
        SamusPoseAnimation read = animation;
        int frame = offset;
        int shift = 0;
        SamusPoseAnimationStep? step = StepOf(read, frame);
        while (step is null)
        {
            if (successors[read] is not { } next || frame < next.Shift)
                return null;
            read = next.Animation;
            frame -= next.Shift;
            shift += next.Shift;
            step = StepOf(read, frame);
        }
        return new StepRead(step, read, frame, shift);
    }

    /// <summary>
    /// How far into <paramref name="animation"/>'s layout a cue read at <paramref name="offset"/> moves it when it names
    /// <paramref name="target"/>: the target's place in the animation the cue was read in, shifted by as much as that
    /// animation lies after the one read.
    /// </summary>
    /// <exception cref="InvalidDataException">The frame reads no step.</exception>
    internal int TargetOffset(SamusPoseAnimation animation, int offset, SamusAnimationFrame target)
    {
        StepRead read = Read(animation, offset) ?? throw new InvalidDataException(
            $"Samus animation frame {offset} of the {animation.Name} animation reads no step whose cue could name a frame.");
        return Offset(read.Animation, read.Animation.Require(target)) + read.Shift;
    }

    /// <summary>Every program's cue-value steps as the installation holds them, by the animation's name and frame name.</summary>
    internal Dictionary<string, IReadOnlyDictionary<string, SamusOverreadDocument.CueValueStep>> DocumentCueValueSteps() =>
        programs.ToDictionary(animation => SamusAnimationNameKeys.Installed.Of(animation.Name), animation => documentCueValueSteps[animation], StringComparer.Ordinal);

    /// <summary>
    /// What every frame past an animation's end that Samus's animation can reach reads, by the animation's name and the
    /// frame's name: the frames reached from every frame of every animation and from every named place landing in it,
    /// followed on as the animation plays (#2048).
    /// </summary>
    internal Dictionary<string, IReadOnlyDictionary<string, SamusOverreadDocument.ReadPast>> ReadsPast()
    {
        var reads = programs.ToDictionary(animation => animation, _ => new SortedDictionary<int, SamusOverreadDocument.ReadPast>());
        var visited = new HashSet<LayoutPlace>();
        var pending = new Queue<LayoutPlace>();
        void Visit(SamusPoseAnimation animation, int offset)
        {
            if (offset >= 0 && visited.Add(new LayoutPlace(animation, offset)))
                pending.Enqueue(new LayoutPlace(animation, offset));
        }
        // A path that writes a frame from outside the animation may write it with a timer that runs out before the
        // animation reads it, so the animation goes on from the frame after it as well as reading it.
        foreach (SamusPoseAnimation animation in programs)
        {
            for (int offset = 0; offset <= Length(animation); offset++)
                Visit(animation, offset);
            foreach (SamusAnimationPlace place in SamusAnimationPlaces.All)
            {
                Visit(animation, Offset(place));
                Visit(animation, Offset(place) + 1);
            }
        }
        while (pending.TryDequeue(out LayoutPlace at))
        {
            if (Read(at.Animation, at.Offset) is not { } read)
                continue;
            if (read.Shift > 0)
                reads[at.Animation].Add(at.Offset, ReadPast(at.Animation, read));
            foreach (int next in Following(at.Animation, at.Offset, read.Step))
                Visit(at.Animation, next);
        }
        return programs.ToDictionary(animation => SamusAnimationNameKeys.Installed.Of(animation.Name), animation =>
            (IReadOnlyDictionary<string, SamusOverreadDocument.ReadPast>)reads[animation].ToDictionary(
                pair => animation.NameOf(FrameAt(animation, pair.Key)), pair => pair.Value, StringComparer.Ordinal),
            StringComparer.Ordinal);
    }

    /// <summary>
    /// The frame each named place lands on in every animation other than the place's own: the frame lying as far into the
    /// animation's layout as the place lies into its own, by place name and the animation's name (#2048).
    /// </summary>
    internal Dictionary<string, IReadOnlyDictionary<string, string>> PlaceLandings() =>
        SamusAnimationPlaces.All.ToDictionary(place => SamusAnimationPlaceNameKeys.Installed.Of(place.Name), place => (IReadOnlyDictionary<string, string>)programs
            .Where(animation => !ReferenceEquals(animation, place.Animation))
            .ToDictionary(animation => SamusAnimationNameKeys.Installed.Of(animation.Name), animation => animation.NameOf(FrameAt(animation, Offset(place))),
                StringComparer.Ordinal), StringComparer.Ordinal);

    /// <summary>
    /// The places of <paramref name="animation"/>'s layout it can go on to from <paramref name="offset"/>, where it reads
    /// <paramref name="step"/>: the next frame after a delay, a hold, the breathing loop and the drained fall's handover,
    /// the start after a restart and the breathing loop, and the frames a repeat or the wall-jump selection names. A
    /// transition ends the animation.
    /// </summary>
    /// <exception cref="InvalidDataException">The step has no playback.</exception>
    private int[] Following(SamusPoseAnimation animation, int offset, SamusPoseAnimationStep step) => step switch
    {
        SamusAnimationDelay or SamusAnimationHold or SamusAnimationInstallDrainedFall => [offset + 1],
        SamusAnimationLoopUnlessLowEnergy => [offset + 1, 0],
        SamusAnimationRestart => [0],
        SamusAnimationRepeat repeat => [TargetOffset(animation, offset, repeat.Target)],
        SamusAnimationWallJumpSelect select =>
        [
            TargetOffset(animation, offset, select.OrdinaryFrame),
            TargetOffset(animation, offset, select.SpaceJumpFrame),
            TargetOffset(animation, offset, select.ScrewAttackFrame),
        ],
        SamusAnimationTransition or SamusAnimationTurnTransition or SamusAnimationItemTransition or
            SamusAnimationItemAirborneTransition or SamusAnimationVerticalSpeedTransition => [],
        _ => throw new InvalidDataException($"Samus animation step {step} has no playback."),
    };

    /// <summary>
    /// What <paramref name="read"/>, a read of <paramref name="animation"/> past its end, reaches as the installation names
    /// it: the later animation by its name and its frame, and the frame of the animation read each frame the cue there names
    /// moves to.
    /// </summary>
    private static SamusOverreadDocument.ReadPast ReadPast(SamusPoseAnimation animation, StepRead read)
    {
        SamusPoseAnimation readAnimation = read.Animation;
        SamusAnimationFrame[] targets = read.Step switch
        {
            SamusAnimationRepeat repeat => [repeat.Target],
            SamusAnimationWallJumpSelect select => [select.OrdinaryFrame, select.SpaceJumpFrame, select.ScrewAttackFrame],
            _ => [],
        };
        var moves = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (SamusAnimationFrame target in targets)
        {
            SamusAnimationFrame named = readAnimation.Require(target);
            moves[readAnimation.NameOf(named)] = animation.NameOf(FrameAt(animation, Offset(readAnimation, named) + read.Shift));
        }
        return new SamusOverreadDocument.ReadPast(SamusAnimationNameKeys.Installed.Of(readAnimation.Name), readAnimation.NameOf(FrameAt(readAnimation, read.Frame)), moves);
    }

    /// <summary>The step <paramref name="frame"/> frames into <paramref name="animation"/>'s program names itself: one of its own, or a value one of its cues carries.</summary>
    private SamusPoseAnimationStep? StepOf(SamusPoseAnimation animation, int frame) =>
        OwnStepAt(animation, frame) ??
        (cueValueSteps[animation].TryGetValue(frame, out SamusPoseAnimationStep? step) ? step : null);

    /// <summary>A decoded cue value's step as the installation holds it, the frames it selects named in <paramref name="animation"/>.</summary>
    private static SamusOverreadDocument.CueValueStep Named(SamusPoseAnimation animation, SamusAnimationCueValueDecoding.DecodedStep step) =>
        new(CueValueStepKindKeys.Installed.Of(step.Kind), step.Ticks,
            step.Frames is { } frames
                ? [.. frames.Select(frame => frame >= 0
                    ? animation.NameOf(FrameAt(animation, frame))
                    : throw new InvalidDataException($"A Samus animation cue value selects frame {frame}, before its program's start."))]
                : null,
            step.Items, step.Poses);
}
