using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Builds the one run of a room object's authored frames (<see cref="RoomObjectFrameRun"/>, #2480).</summary>
internal static partial class RoomObjectFrameRuns
{
    /// <summary>The run of <paramref name="frames"/>, each shown for the updates <paramref name="updates"/> gives it.</summary>
    internal static AnimationSegments<RoomObjectFrameRun, TFrame, NoAnimationCue> Of<TFrame>(IReadOnlyList<TFrame> frames, Func<TFrame, int> updates)
        where TFrame : struct
    {
        Ensure.NotNull(frames);
        Ensure.NotNull(updates);
        return new(new Dictionary<RoomObjectFrameRun, AnimationBeat<TFrame, NoAnimationCue>[]>
        {
            [RoomObjectFrameRun.Frames] = [.. frames.Select(frame => new AnimationBeat<TFrame, NoAnimationCue>.Frame(frame, updates(frame)))],
        });
    }

    /// <summary>The run of <paramref name="segments"/> played once through, then over.</summary>
    internal static Animation<RoomObjectFrameRun> Once<TFrame>(AnimationSegments<RoomObjectFrameRun, TFrame, NoAnimationCue> segments)
        where TFrame : struct =>
        new([Ensure.NotNull(segments).Whole(RoomObjectFrameRun.Frames)], AnimationEnd.Stops);
}
