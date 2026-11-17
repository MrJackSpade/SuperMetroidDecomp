using SuperMetroid.Core.Input;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Verification.Enemies;

/// <summary>
/// Typed entry to the enemy instruction interpreter for fixtures that keep their call as an argument row:
/// slot, Samus, level, camera and controller input. A null value-type element is its default, as it was when
/// these rows were passed to <see cref="System.Reflection.MethodBase.Invoke(object, object[])"/>; an element
/// of the wrong type is rejected.
/// </summary>
internal static class EnemyInstructionRunner
{
    /// <summary>Processes the slot's instruction list for one frame.</summary>
    internal static void Process(object?[] arguments) =>
        EnemyAnimationPlayer.AdvanceAnimation(
            arguments[0] as RoomEnemyPlace ?? throw new ArgumentException("Element 0 must be the enemy slot.", nameof(arguments)),
            Optional<SamusState>(arguments, 1) ?? RoomEnemyFrameFixtures.StandaloneSamus(),
            Optional<RoomLevelData>(arguments, 2) ?? RoomEnemyFrameFixtures.OpenRoom(new(64, 64)),
            Value<Position16<RoomPixels>>(arguments, 3),
            Value<GameplayAction>(arguments, 4));

    /// <summary>
    /// Processes <paramref name="place"/>'s animation for <paramref name="updates"/> updates with no Samus, level, camera or
    /// input, and lists each newly shown frame as <c>update:spritemap</c> (hexadecimal), comma-separated.
    /// </summary>
    internal static string ShownFrames(RoomEnemyPlace place, int updates)
    {
        object?[] arguments = [place, null, null, Position16<RoomPixels>.Zero, default(GameplayAction)];
        var shown = new List<string>();
        for (int update = 0; update < updates; update++)
        {
            Process(arguments);
            if (place.AnimationClock.ShowsNewFrame)
                shown.Add($"{update}:{place.SpritemapPointer:X4}");
        }
        return string.Join(",", shown);
    }

    /// <summary>
    /// The <see cref="ShownFrames"/> listing of <paramref name="spritemaps"/> shown in turn, each held for its duration in
    /// <paramref name="durations"/>, starting on update zero.
    /// </summary>
    internal static string ExpectedFrames(IEnumerable<ushort> spritemaps, IEnumerable<int> durations)
    {
        int update = 0;
        var shown = new List<string>();
        ushort[] maps = [.. spritemaps];
        int[] times = [.. durations];
        for (int step = 0; step < Math.Min(maps.Length, times.Length); step++)
        {
            ushort spritemap = maps[step];
            int duration = times[step];
            shown.Add($"{update}:{spritemap:X4}");
            update += duration;
        }
        return string.Join(",", shown);
    }

    /// <summary>
    /// The frames a looping segment's animation shows (#1600): one pass over <paramref name="segment"/>'s frames, then
    /// the first of the next pass.
    /// </summary>
    internal static string ExpectedLoop<TSegment, TFrame, TEvent>(AnimationSegments<TSegment, TFrame, TEvent> segments,
        TSegment segment, Func<TFrame, ushort> spritemap)
        where TSegment : struct, Enum
        where TFrame : struct
        where TEvent : notnull
    {
        var frames = segments.Beats(segment).OfType<AnimationBeat<TFrame, TEvent>.Frame>().ToArray();
        return ExpectedFrames([.. frames.Select(frame => spritemap(frame.Shown)), spritemap(frames[0].Shown)],
            [.. frames.Select(frame => frame.Duration), frames[0].Duration]);
    }

    /// <summary>The update count the typed <see cref="ExpectedLoop{TSegment, TFrame, TEvent}(AnimationSegments{TSegment, TFrame, TEvent}, TSegment, Func{TFrame, ushort})"/> covers: one pass plus the first update of the next.</summary>
    internal static int LoopUpdates<TSegment, TFrame, TEvent>(AnimationSegments<TSegment, TFrame, TEvent> segments, TSegment segment)
        where TSegment : struct, Enum
        where TFrame : struct
        where TEvent : notnull =>
        segments.Beats(segment).OfType<AnimationBeat<TFrame, TEvent>.Frame>().Sum(frame => frame.Duration) + 1;

    /// <summary>
    /// The frames a looping animation shows (#2337): one pass over its phases, each played as many times as it plays, then
    /// the first frame of the next pass.
    /// </summary>
    internal static string ExpectedAnimationLoop<TSegment, TFrame, TEvent>(AnimationSegments<TSegment, TFrame, TEvent> segments,
        Animation<TSegment> animation, Func<TFrame, ushort> spritemap)
        where TSegment : struct, Enum
        where TFrame : struct
        where TEvent : notnull
    {
        AnimationBeat<TFrame, TEvent>.Frame[] frames = PassFrames(segments, animation);
        return ExpectedFrames([.. frames.Select(frame => spritemap(frame.Shown)), spritemap(frames[0].Shown)],
            [.. frames.Select(frame => frame.Duration), frames[0].Duration]);
    }

    /// <summary>The update count <see cref="ExpectedAnimationLoop"/> covers: one pass plus the first update of the next.</summary>
    internal static int AnimationLoopUpdates<TSegment, TFrame, TEvent>(AnimationSegments<TSegment, TFrame, TEvent> segments, Animation<TSegment> animation)
        where TSegment : struct, Enum
        where TFrame : struct
        where TEvent : notnull =>
        PassFrames(segments, animation).Sum(frame => frame.Duration) + 1;

    /// <summary>The timed frames one pass of <paramref name="animation"/> shows, in order, each phase as many times as it plays.</summary>
    private static AnimationBeat<TFrame, TEvent>.Frame[] PassFrames<TSegment, TFrame, TEvent>(AnimationSegments<TSegment, TFrame, TEvent> segments,
        Animation<TSegment> animation)
        where TSegment : struct, Enum
        where TFrame : struct
        where TEvent : notnull =>
        [.. animation.Phases.SelectMany(phase => Enumerable.Repeat(segments.Beats(phase.Segment).Skip(phase.FirstBeat).Take(phase.BeatCount), phase.Plays)
            .SelectMany(beats => beats))
            .OfType<AnimationBeat<TFrame, TEvent>.Frame>()];

    /// <summary>Accepts a nullable fixture reference at an argument index and reports mismatched runtime types.</summary>
    private static T? Optional<T>(object?[] arguments, int index) where T : class => arguments[index] switch
    {
        null => null,
        T value => value,
        object other => throw new ArgumentException($"Element {index} is {other.GetType()}, not {typeof(T)}.", nameof(arguments)),
    };

    /// <summary>Restores reflection's default-value behavior for a null fixture value type while rejecting wrong types.</summary>
    private static T Value<T>(object?[] arguments, int index) where T : struct => arguments[index] switch
    {
        null => default,
        T value => value,
        object other => throw new ArgumentException($"Element {index} is {other.GetType()}, not {typeof(T)}.", nameof(arguments)),
    };
}
