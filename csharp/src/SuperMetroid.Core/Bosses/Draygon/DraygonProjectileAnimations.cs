using SuperMetroid.Core.Animation;
using P = SuperMetroid.Core.Bosses.Draygon.DraygonProjectileSegment;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Draygon's typed projectile animations (#641, #1600): the goop's and wall-turret shot's segments, and the animation starting at each.</summary>
internal static class DraygonProjectileAnimations
{
    /// <summary>Frames in the goop loop.</summary>
    internal const int GoopPhaseCount = 6;
    /// <summary>Frames in the goop burst.</summary>
    internal const int GoopShotPhaseCount = 2;
    /// <summary>Frames in the wall turret's bloom.</summary>
    internal const int BloomPhaseCount = 16;
    /// <summary>Frames in the wall turret's flight loop.</summary>
    internal const int FlightPhaseCount = 3;

    /// <summary>The bloom's frame durations: two forming ramps, the second a tick faster, then four charging frames.</summary>
    private static readonly int[] BloomDurations = [5, 4, 3, 3, 3, 3, 4, 3, 2, 2, 2, 2, 10, 10, 10, 10];

    /// <summary>Every goop and wall-turret segment's beats.</summary>
    internal static AnimationSegments<DraygonProjectileSegment, DraygonProjectileFrame, DraygonProjectileCue> Segments { get; } = new(
        new Dictionary<DraygonProjectileSegment, AnimationBeat<DraygonProjectileFrame, DraygonProjectileCue>[]>
        {
            [P.GoopTouch] = [Cue(DraygonProjectileCue.StickToSamus)],
            [P.Goop] = [.. Frames(DraygonProjectileFrameKind.Goop, GoopPhaseCount, _ => 10)],
            [P.GoopShot] =
            [
                .. Frames(DraygonProjectileFrameKind.GoopShot, GoopShotPhaseCount, _ => 8),
                Cue(DraygonProjectileCue.SpawnDrops),
                Cue(DraygonProjectileCue.Delete),
            ],
            [P.WallTurretBloom] =
            [
                .. Frames(DraygonProjectileFrameKind.WallTurretBloom, BloomPhaseCount, phase => BloomDurations[phase]),
                Cue(DraygonProjectileCue.Fire),
            ],
            [P.WallTurretFlight] = [.. Frames(DraygonProjectileFrameKind.WallTurretFlight, FlightPhaseCount, _ => 8)],
        });

    /// <summary>
    /// The animation that starts at each segment: the touch runs on into the goop loop, the burst holds after its deletion,
    /// and the bloom fires and runs on into the flight loop.
    /// </summary>
    internal static IReadOnlyDictionary<DraygonProjectileSegment, Animation<DraygonProjectileSegment>> StartingAt { get; } =
        new Dictionary<DraygonProjectileSegment, Animation<DraygonProjectileSegment>>
        {
            [P.GoopTouch] = new([new(P.GoopTouch, 0, 1, 1), new(P.Goop, 0, GoopPhaseCount, 1)], AnimationEnd.LoopsFrom(1)),
            [P.Goop] = new([new(P.Goop, 0, GoopPhaseCount, 1)], AnimationEnd.LoopsFrom(0)),
            [P.GoopShot] = new([new(P.GoopShot, 0, GoopShotPhaseCount + 2, 1)], AnimationEnd.Holds),
            [P.WallTurretBloom] = new([new(P.WallTurretBloom, 0, BloomPhaseCount + 1, 1), new(P.WallTurretFlight, 0, FlightPhaseCount, 1)], AnimationEnd.LoopsFrom(1)),
            [P.WallTurretFlight] = new([new(P.WallTurretFlight, 0, FlightPhaseCount, 1)], AnimationEnd.LoopsFrom(0)),
        };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<DraygonProjectileSegment> StartingWith(DraygonProjectileSegment segment) => StartingAt[segment];

    /// <summary>Raises <paramref name="cue"/>.</summary>
    private static AnimationBeat<DraygonProjectileFrame, DraygonProjectileCue> Cue(DraygonProjectileCue cue) =>
        new AnimationBeat<DraygonProjectileFrame, DraygonProjectileCue>.Event(cue);

    /// <summary>Shows <paramref name="count"/> consecutive frames of <paramref name="kind"/>.</summary>
    private static IEnumerable<AnimationBeat<DraygonProjectileFrame, DraygonProjectileCue>> Frames(
        DraygonProjectileFrameKind kind, int count, Func<int, int> duration) =>
        Enumerable.Range(0, count).Select(phase =>
            (AnimationBeat<DraygonProjectileFrame, DraygonProjectileCue>)new AnimationBeat<DraygonProjectileFrame, DraygonProjectileCue>.Frame(new(kind, phase), duration(phase)));
}
