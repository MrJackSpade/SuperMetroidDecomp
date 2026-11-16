using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// The game's layout of Samus's animations (#1473, #1535, #2048), which only the import knows. The game numbers an
/// animation's frames from its start, one after another through its segments, each as long as its frames, its cue and the
/// values the cue carries; it lays the animations' programs out one after another in the cartridge; and it reads a frame
/// as a byte of that layout, so a frame on a value a cue carries reads the value as a step and a frame past an animation's
/// end reads a later animation. The import decodes that reading once into the named outcomes the game holds
/// (<see cref="DocumentCueValueSteps"/>, <see cref="ReadsPast"/>, <see cref="PlaceLandings"/>), and the analysis of the
/// frames Samus's body can show follows it here.
/// </summary>
internal sealed class SamusAnimationLayout
{
    /// <summary>A step read for a pose: the step, the pose naming the animation it was read in, its frame there and how far that animation lies after the pose's.</summary>
    /// <param name="Step">The step read.</param>
    /// <param name="Animation">A pose that animates by the animation the step was read in.</param>
    /// <param name="Frame">The frame of that animation the step lies at, counted from its start.</param>
    /// <param name="Shift">How many frames after the read pose's animation that animation starts; zero for the pose's own.</param>
    internal readonly record struct StepRead(SamusPoseAnimationStep Step, SamusPoseId Animation, int Frame, int Shift);

    /// <summary>A place of a pose's layout: the pose and how far into the game's layout of its animation the place lies.</summary>
    /// <param name="Pose">The pose.</param>
    /// <param name="Offset">How far into the layout of the pose's animation the place lies.</param>
    private readonly record struct LayoutPlace(SamusPoseId Pose, int Offset);

    /// <summary>A program laid out after another: a pose that animates by it and how many frames later it starts.</summary>
    private readonly record struct Successor(SamusPoseId Pose, int Shift);

    /// <summary>
    /// The pose naming each program, its lowest-numbered pose, in the game's numbering: the installation holds each
    /// program's outcomes once, under that pose, and every pose of the program reads them.
    /// </summary>
    private readonly SamusPoseId[] poses;

    /// <summary>Each pose's program's successor, or none for the last program laid out.</summary>
    private readonly Dictionary<SamusPoseId, Successor?> successors;

    /// <summary>The step each cue value's frame of each pose's program reads, by frame counted from the program's start.</summary>
    private readonly Dictionary<SamusPoseId, IReadOnlyDictionary<int, SamusPoseAnimationStep>> cueValueSteps;

    /// <summary>The installed form of each pose's cue-value steps, by the frame's name.</summary>
    private readonly Dictionary<SamusPoseId, IReadOnlyDictionary<string, SamusOverreadDocument.CueValueStep>> documentCueValueSteps;

    /// <summary>Keeps the layout read from the cartridge.</summary>
    private SamusAnimationLayout(SamusPoseId[] poses, Dictionary<SamusPoseId, Successor?> successors,
        Dictionary<SamusPoseId, IReadOnlyDictionary<int, SamusPoseAnimationStep>> cueValueSteps,
        Dictionary<SamusPoseId, IReadOnlyDictionary<string, SamusOverreadDocument.CueValueStep>> documentCueValueSteps)
    {
        this.poses = poses;
        this.successors = successors;
        this.cueValueSteps = cueValueSteps;
        this.documentCueValueSteps = documentCueValueSteps;
    }

    /// <summary>Reads the layout of every program and decodes the step every cue value's frame reads.</summary>
    /// <exception cref="InvalidDataException">Two poses of one program start apart, two programs start together, or a cue value names no frame.</exception>
    internal static SamusAnimationLayout Read(IImportCartridgeSource cartridge)
    {
        Ensure.NotNull(cartridge);
        SamusAnimationProgramLayout.LaidOutProgram[] laidOut = SamusAnimationProgramLayout.Read(cartridge);
        var programSuccessors = new Dictionary<SamusPoseAnimation, Successor?>();
        for (int index = 0; index < laidOut.Length; index++)
        {
            programSuccessors.Add(laidOut[index].Program, index + 1 < laidOut.Length
                ? new Successor(laidOut[index + 1].NamedBy, laidOut[index + 1].Start - laidOut[index].Start)
                : null);
        }
        Dictionary<SamusPoseAnimation, IReadOnlyDictionary<int, SamusAnimationCueValueDecoding.DecodedStep>> decoded =
            SamusAnimationCueValueDecoding.CueValueSteps(cartridge, laidOut);
        var successors = new Dictionary<SamusPoseId, Successor?>();
        var steps = new Dictionary<SamusPoseId, IReadOnlyDictionary<int, SamusPoseAnimationStep>>();
        var documentSteps = new Dictionary<SamusPoseId, IReadOnlyDictionary<string, SamusOverreadDocument.CueValueStep>>();
        foreach (SamusPoseId pose in SamusAnimationProgramLayout.PosesWithRecords())
        {
            SamusPoseAnimation animation = SamusAnimationDelays.AnimationForPose(pose);
            successors.Add(pose, programSuccessors[animation]);
            Dictionary<int, SamusOverreadDocument.CueValueStep> named = decoded[animation].ToDictionary(pair => pair.Key, pair => Named(animation, pair.Value));
            documentSteps.Add(pose, named.ToDictionary(pair => animation.NameOf(FrameAt(animation, pair.Key)), pair => pair.Value, StringComparer.Ordinal));
            steps.Add(pose, named.ToDictionary(pair => pair.Key, pair => SamusOverreadServicesReader.CueValueStep(pose, pair.Value)));
        }
        SamusPoseId[] namingPoses = [.. laidOut.Select(program => program.NamedBy).OrderBy(SamusPoseNumbers.Number)];
        return new SamusAnimationLayout(namingPoses, successors, steps, documentSteps);
    }

    /// <summary>How far into the game's layout of <paramref name="animation"/> <paramref name="frame"/> lies.</summary>
    /// <exception cref="ArgumentException">The animation has no such frame.</exception>
    internal static int Offset(SamusPoseAnimation animation, SamusAnimationFrame frame)
    {
        Ensure.NotNull(animation);
        _ = animation.SegmentName(frame);
        int start = 0;
        for (int segment = 0; segment < frame.Segment; segment++)
            start += animation.Segments[segment].Length;
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
            int length = animation.Segments[segment].Length;
            if (offset < start + length)
                return new SamusAnimationFrame(segment, offset - start);
            start += length;
        }
        return new SamusAnimationFrame(animation.Segments.Count - 1, offset - start);
    }

    /// <summary>How many frames <paramref name="animation"/>'s own steps span in its layout, its last end cue included.</summary>
    internal static int StepCount(SamusPoseAnimation animation)
    {
        Ensure.NotNull(animation);
        SamusAnimationSegment last = animation.Segments[^1];
        return Offset(animation, new SamusAnimationFrame(animation.Segments.Count - 1, last.CueFrame)) + 1;
    }

    /// <summary>How many frames <paramref name="animation"/>'s segments span in its layout, the values its last cue carries included.</summary>
    internal static int Length(SamusPoseAnimation animation) => Ensure.NotNull(animation).Segments.Sum(segment => segment.Length);

    /// <summary>The step of <paramref name="animation"/>'s own at <paramref name="offset"/>, or null for a cue's value or a frame past it.</summary>
    internal static SamusPoseAnimationStep? OwnStepAt(SamusPoseAnimation animation, int offset) =>
        offset >= 0 ? Ensure.NotNull(animation).TryStepAt(FrameAt(animation, offset)) : null;

    /// <summary>
    /// The step at <paramref name="offset"/> frames into the game's layout of <paramref name="pose"/>'s animation, reading a
    /// cue's value or past the animation's end as the game does; null where the game's animation fails.
    /// </summary>
    internal StepRead? Read(SamusPoseId pose, int offset)
    {
        SamusPoseId read = pose;
        int frame = offset;
        int shift = 0;
        SamusPoseAnimationStep? step = StepOf(read, frame);
        while (step is null)
        {
            if (successors[read] is not { } next || frame < next.Shift)
                return null;
            read = next.Pose;
            frame -= next.Shift;
            shift += next.Shift;
            step = StepOf(read, frame);
        }
        return new StepRead(step, read, frame, shift);
    }

    /// <summary>
    /// How far into <paramref name="pose"/>'s layout a cue read at <paramref name="offset"/> moves its animation when it
    /// names <paramref name="target"/>: the target's place in the animation the cue was read in, shifted by as much as that
    /// animation lies after the pose's.
    /// </summary>
    /// <exception cref="InvalidDataException">The frame reads no step.</exception>
    internal int TargetOffset(SamusPoseId pose, int offset, SamusAnimationTarget target)
    {
        StepRead read = Read(pose, offset) ?? throw new InvalidDataException(
            $"Samus animation frame {offset} of pose {pose} reads no step whose cue could name a frame.");
        SamusPoseAnimation readAnimation = SamusAnimationDelays.AnimationForPose(read.Animation);
        return Offset(readAnimation, readAnimation.Resolve(target)) + read.Shift;
    }

    /// <summary>Every program's cue-value steps as the installation holds them, by the name of the pose naming it and frame name.</summary>
    internal Dictionary<string, IReadOnlyDictionary<string, SamusOverreadDocument.CueValueStep>> DocumentCueValueSteps() =>
        poses.ToDictionary(pose => pose.ToString(), pose => documentCueValueSteps[pose], StringComparer.Ordinal);

    /// <summary>
    /// What every frame past an animation's end that Samus's animation can reach reads, by the name of the pose naming the
    /// animation's program and the frame's name: the frames reached from every frame of every animation and from every named
    /// place landing in it, followed on as the animation plays (#2048).
    /// </summary>
    internal Dictionary<string, IReadOnlyDictionary<string, SamusOverreadDocument.ReadPast>> ReadsPast()
    {
        var reads = poses.ToDictionary(pose => pose, _ => new SortedDictionary<int, SamusOverreadDocument.ReadPast>());
        var visited = new HashSet<LayoutPlace>();
        var pending = new Queue<LayoutPlace>();
        void Visit(SamusPoseId pose, int offset)
        {
            if (offset >= 0 && visited.Add(new LayoutPlace(pose, offset)))
                pending.Enqueue(new LayoutPlace(pose, offset));
        }
        // A path that writes a frame from outside the animation may write it with a timer that runs out before the
        // animation reads it, so the animation goes on from the frame after it as well as reading it.
        foreach (SamusPoseId pose in poses)
        {
            for (int offset = 0; offset <= Length(SamusAnimationDelays.AnimationForPose(pose)); offset++)
                Visit(pose, offset);
            foreach (SamusAnimationPlace place in SamusAnimationPlaces.All)
            {
                Visit(pose, Offset(place));
                Visit(pose, Offset(place) + 1);
            }
        }
        while (pending.TryDequeue(out LayoutPlace at))
        {
            if (Read(at.Pose, at.Offset) is not { } read)
                continue;
            if (read.Shift > 0)
                reads[at.Pose].Add(at.Offset, ReadPast(at.Pose, read));
            foreach (int next in Following(at.Pose, at.Offset, read.Step))
                Visit(at.Pose, next);
        }
        return poses.ToDictionary(pose => pose.ToString(), pose =>
        {
            SamusPoseAnimation animation = SamusAnimationDelays.AnimationForPose(pose);
            return (IReadOnlyDictionary<string, SamusOverreadDocument.ReadPast>)reads[pose].ToDictionary(
                pair => animation.NameOf(FrameAt(animation, pair.Key)), pair => pair.Value, StringComparer.Ordinal);
        }, StringComparer.Ordinal);
    }

    /// <summary>
    /// The frame each named place lands on in every animation other than the place's own: the frame lying as far into the
    /// animation's layout as the place lies into its own, by place name and the name of the pose naming the animation's
    /// program (#2048).
    /// </summary>
    internal Dictionary<string, IReadOnlyDictionary<string, string>> PlaceLandings() =>
        SamusAnimationPlaces.All.ToDictionary(place => place.Name.ToString(), place => (IReadOnlyDictionary<string, string>)poses
            .Where(pose => !ReferenceEquals(SamusAnimationDelays.AnimationForPose(pose), place.Animation))
            .ToDictionary(pose => pose.ToString(), pose =>
            {
                SamusPoseAnimation animation = SamusAnimationDelays.AnimationForPose(pose);
                return animation.NameOf(FrameAt(animation, Offset(place)));
            }, StringComparer.Ordinal), StringComparer.Ordinal);

    /// <summary>
    /// The places of <paramref name="pose"/>'s layout the animation can go on to from <paramref name="offset"/>, where it
    /// reads <paramref name="step"/>: the next frame after a delay, a hold, the breathing loop and the drained fall's
    /// handover, the start after a restart and the breathing loop, and the frames a repeat or the wall-jump selection names.
    /// A transition ends the pose's animation.
    /// </summary>
    /// <exception cref="InvalidDataException">The step has no playback.</exception>
    private int[] Following(SamusPoseId pose, int offset, SamusPoseAnimationStep step) => step switch
    {
        SamusAnimationDelay or SamusAnimationHold or SamusAnimationInstallDrainedFall => [offset + 1],
        SamusAnimationLoopUnlessLowEnergy => [offset + 1, 0],
        SamusAnimationRestart => [0],
        SamusAnimationRepeat repeat => [TargetOffset(pose, offset, repeat.Target)],
        SamusAnimationWallJumpSelect select =>
        [
            TargetOffset(pose, offset, select.OrdinaryFrame),
            TargetOffset(pose, offset, select.SpaceJumpFrame),
            TargetOffset(pose, offset, select.ScrewAttackFrame),
        ],
        SamusAnimationTransition or SamusAnimationTurnTransition or SamusAnimationItemTransition or
            SamusAnimationItemAirborneTransition or SamusAnimationVerticalSpeedTransition => [],
        _ => throw new InvalidDataException($"Samus animation step {step} has no playback."),
    };

    /// <summary>
    /// What <paramref name="read"/>, a read of <paramref name="pose"/>'s animation past its end, reaches as the installation
    /// names it: the later animation's frame, and the frame of the pose's animation each frame the cue there names moves to.
    /// </summary>
    private static SamusOverreadDocument.ReadPast ReadPast(SamusPoseId pose, StepRead read)
    {
        SamusPoseAnimation animation = SamusAnimationDelays.AnimationForPose(pose);
        SamusPoseAnimation readAnimation = SamusAnimationDelays.AnimationForPose(read.Animation);
        SamusAnimationTarget[] targets = read.Step switch
        {
            SamusAnimationRepeat repeat => [repeat.Target],
            SamusAnimationWallJumpSelect select => [select.OrdinaryFrame, select.SpaceJumpFrame, select.ScrewAttackFrame],
            _ => [],
        };
        var moves = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (SamusAnimationTarget target in targets)
        {
            SamusAnimationFrame named = readAnimation.Resolve(target);
            moves[readAnimation.NameOf(named)] = animation.NameOf(FrameAt(animation, Offset(readAnimation, named) + read.Shift));
        }
        return new SamusOverreadDocument.ReadPast(read.Animation.ToString(), readAnimation.NameOf(FrameAt(readAnimation, read.Frame)), moves);
    }

    /// <summary>The step <paramref name="frame"/> frames into <paramref name="pose"/>'s program names itself: one of its own, or a value one of its cues carries.</summary>
    private SamusPoseAnimationStep? StepOf(SamusPoseId pose, int frame) =>
        OwnStepAt(SamusAnimationDelays.AnimationForPose(pose), frame) ??
        (cueValueSteps[pose].TryGetValue(frame, out SamusPoseAnimationStep? step) ? step : null);

    /// <summary>A decoded cue value's step as the installation holds it, the frames it selects named in <paramref name="animation"/>.</summary>
    private static SamusOverreadDocument.CueValueStep Named(SamusPoseAnimation animation, SamusAnimationCueValueDecoding.DecodedStep step) =>
        new(step.Kind.ToString(), step.Ticks,
            step.Frames is { } frames
                ? [.. frames.Select(frame => frame >= 0
                    ? animation.NameOf(FrameAt(animation, frame))
                    : throw new InvalidDataException($"A Samus animation cue value selects frame {frame}, before its program's start."))]
                : null,
            step.Items, step.Poses);
}
