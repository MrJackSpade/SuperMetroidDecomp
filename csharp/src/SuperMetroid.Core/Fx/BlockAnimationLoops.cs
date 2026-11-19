using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// Plays the looping block animations (<see cref="BlockAnimationLoopDefinitions"/>, #1406, #1988, #2422): each is an
/// animation that loops its one idle segment, played by the shared animation player. A pass of a playing loop counts the
/// frame shown down and, when its time is up, shows the loop's next frame for that frame's duration, returning from the
/// last frame to the first; a loop starts due on its first pass. Also resolves the animation a room effect's type shows on
/// the effect overlay and the treadmill a Wrecked Ship entrance door starts. An undefined animation, effect type or
/// direction is rejected.
/// </summary>
/// <remarks>
/// Lava, acid, rain, and spores do not show pictures of the room tileset's. Their effect starts one looping animation,
/// each with its own named frames and durations. Playing the whole loop matters: showing one convenient frame would make
/// the first still image look plausible while discarding the game's cadence and preventing replacement graphics from
/// following the same frames.
/// </remarks>
public static class BlockAnimationLoops
{
    /// <summary>The idle segments of every animation with a loop.</summary>
    internal static AnimationSegments<BlockAnimation, ArtworkFrame, NoAnimationCue> Segments { get; } =
        new(OwnTimedFrames(BlockAnimationLoopDefinitions.IdleLoops), BlockAnimationLoopDefinitions.IdleLoops.Keys);

    /// <summary>
    /// <paramref name="loops"/>, each of which shows at least one frame, every one of its own animation's and shown for at
    /// least one pass.
    /// </summary>
    /// <exception cref="ArgumentException">A loop shows no frame, or a frame belongs to another animation or is shown for no pass.</exception>
    private static IReadOnlyDictionary<BlockAnimation, AnimationBeat<ArtworkFrame, NoAnimationCue>[]> OwnTimedFrames(
        IReadOnlyDictionary<BlockAnimation, AnimationBeat<ArtworkFrame, NoAnimationCue>[]> loops)
    {
        foreach ((BlockAnimation animation, AnimationBeat<ArtworkFrame, NoAnimationCue>[] beats) in loops)
        {
            if (beats.Length == 0)
                throw new ArgumentException($"The {animation} loop shows at least one frame.", nameof(loops));
            foreach (AnimationBeat<ArtworkFrame, NoAnimationCue> beat in beats)
            {
                if (beat is not AnimationBeat<ArtworkFrame, NoAnimationCue>.Frame { Duration: > 0 } frame
                    || frame.Shown.Animation != BlockAnimationFrames.Of(animation))
                    throw new ArgumentException($"The {animation} loop shows {beat}, not a frame of its own shown for a pass or more.", nameof(loops));
            }
        }
        return loops;
    }

    /// <summary>Whether <paramref name="animation"/> has a loop: the Tourian statues and no animation have none.</summary>
    /// <exception cref="InvalidOperationException"><paramref name="animation"/> is undefined.</exception>
    public static bool HasLoop(BlockAnimation animation)
    {
        if (!Enum.IsDefined(animation))
            throw new InvalidOperationException($"Undefined {nameof(BlockAnimation)} {animation}.");
        return BlockAnimationLoopDefinitions.IdleLoops.ContainsKey(animation);
    }

    /// <summary>
    /// The loop of every animation with one, built once with the segments, so every start of a loop plays the one loop
    /// this definition holds (#1716).
    /// </summary>
    private static Dictionary<BlockAnimation, Animation<BlockAnimation>> IdleAnimations { get; } =
        BlockAnimationLoopDefinitions.IdleLoops.Keys.ToDictionary(
            animation => animation, animation => Animation<BlockAnimation>.Looping([Segments.Whole(animation)]));

    /// <summary>The loop of <paramref name="animation"/>: its idle segment, over and over.</summary>
    /// <exception cref="InvalidOperationException"><paramref name="animation"/> is undefined or has no loop.</exception>
    internal static Animation<BlockAnimation> Idle(BlockAnimation animation) => IdleAnimations[Require(animation)];

    /// <summary>Starts playing the loop of <paramref name="animation"/>, showing nothing until its first pass shows its first frame.</summary>
    /// <exception cref="InvalidOperationException"><paramref name="animation"/> is undefined or has no loop.</exception>
    internal static AnimationPlayer<BlockAnimation, ArtworkFrame, NoAnimationCue> Start(BlockAnimation animation) =>
        AnimationPlayer<BlockAnimation, ArtworkFrame, NoAnimationCue>.Starting(Segments, Idle(animation));

    /// <summary>
    /// Runs one pass of a playing loop: when the frame shown has run its passes, the loop's next frame is shown for its
    /// duration, selected in <paramref name="frames"/> at this update, as this update's frame boundary shows it.
    /// </summary>
    internal static void Pass(AnimationPlayer<BlockAnimation, ArtworkFrame, NoAnimationCue> loop, ArtworkFrameSelections frames)
    {
        Ensure.NotNull(frames);
        if (Ensure.NotNull(loop).StepToNewFrame() is { } shown)
            frames.Select(shown);
    }

    /// <summary>
    /// The frames the loop of <paramref name="animation"/> shows, in order, each with the passes it is shown for: what the
    /// import pictures under each frame's name.
    /// </summary>
    /// <exception cref="InvalidOperationException"><paramref name="animation"/> is undefined or has no loop.</exception>
    public static IReadOnlyList<TimedArtworkFrame> Frames(BlockAnimation animation) =>
        [.. Segments.Beats(Require(animation)).Select(beat => beat is AnimationBeat<ArtworkFrame, NoAnimationCue>.Frame frame
            ? new TimedArtworkFrame(frame.Shown, frame.Duration)
            : throw new InvalidOperationException($"The {animation} loop holds a beat that is not a timed frame."))];

    /// <summary>
    /// Whether the loop of <paramref name="animation"/> waits for the area's boss to be defeated before its first frame
    /// (<see cref="BlockAnimationLoopDefinitions.AwaitingAreaBoss"/>): its owner holds the wait.
    /// </summary>
    /// <exception cref="InvalidOperationException"><paramref name="animation"/> is undefined or has no loop.</exception>
    public static bool WaitsForAreaBoss(BlockAnimation animation) =>
        BlockAnimationLoopDefinitions.AwaitingAreaBoss.Contains(Require(animation));

    /// <summary>The animation a room effect of <paramref name="type"/> shows on the effect overlay, or null for one that shows none.</summary>
    /// <exception cref="InvalidOperationException"><paramref name="type"/> is undefined.</exception>
    public static BlockAnimation? OnEffectOverlay(RoomFxType type)
    {
        if (!Enum.IsDefined(type))
            throw new InvalidOperationException($"Undefined {nameof(RoomFxType)} {type}.");
        return BlockAnimationLoopDefinitions.EffectOverlayAnimations.TryGetValue(type, out BlockAnimation animation)
            ? Require(animation) : null;
    }

    /// <summary>The treadmill animation a Wrecked Ship entrance door starts for a belt pushing <paramref name="direction"/>.</summary>
    /// <exception cref="InvalidDataException"><paramref name="direction"/> is undefined.</exception>
    public static BlockAnimation Treadmill(WreckedShipTreadmillDirection direction) =>
        BlockAnimationLoopDefinitions.TreadmillAnimations.TryGetValue(direction, out BlockAnimation animation) ? animation
            : throw new InvalidDataException($"Unknown Wrecked Ship treadmill direction {direction}.");

    /// <summary><paramref name="animation"/>, which must have a loop.</summary>
    /// <exception cref="InvalidOperationException"><paramref name="animation"/> is undefined or has no loop.</exception>
    private static BlockAnimation Require(BlockAnimation animation) => HasLoop(animation)
        ? animation
        : throw new InvalidOperationException($"{animation} has no block animation loop.");
}
