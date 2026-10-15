using SuperMetroid.Core.Programs;
using P = SuperMetroid.Core.Bosses.Draygon.DraygonProjectileProgram;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Draygon's typed projectile programs (#641), in place of their bank-$86 instruction lists.</summary>
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

    /// <summary>Every goop and wall-turret program.</summary>
    internal static AnimationProgramSet<DraygonProjectileProgram, DraygonProjectileFrame, DraygonProjectileCue> Programs { get; } = new(
        new Dictionary<DraygonProjectileProgram, AnimationProgramStep<DraygonProjectileProgram, DraygonProjectileFrame, DraygonProjectileCue>[]>
        {
            [P.GoopTouch] = [Cue(DraygonProjectileCue.StickToSamus), FallInto(P.Goop)],
            [P.Goop] = [.. Frames(DraygonProjectileFrameKind.Goop, GoopPhaseCount, _ => 10), GoTo(P.Goop)],
            [P.GoopShot] =
            [
                .. Frames(DraygonProjectileFrameKind.GoopShot, GoopShotPhaseCount, _ => 8),
                Cue(DraygonProjectileCue.SpawnDrops),
                Cue(DraygonProjectileCue.Delete),
                AnimationProgramStep<DraygonProjectileProgram, DraygonProjectileFrame, DraygonProjectileCue>.Hold,
            ],
            [P.WallTurretBloom] =
            [
                .. Frames(DraygonProjectileFrameKind.WallTurretBloom, BloomPhaseCount, phase => BloomDurations[phase]),
                Cue(DraygonProjectileCue.Fire),
                FallInto(P.WallTurretFlight),
            ],
            [P.WallTurretFlight] = [.. Frames(DraygonProjectileFrameKind.WallTurretFlight, FlightPhaseCount, _ => 8), GoTo(P.WallTurretFlight)],
        });

    /// <summary>Runs on into <paramref name="program"/>.</summary>
    private static AnimationProgramStep<DraygonProjectileProgram, DraygonProjectileFrame, DraygonProjectileCue> FallInto(DraygonProjectileProgram program) =>
        AnimationProgramStep<DraygonProjectileProgram, DraygonProjectileFrame, DraygonProjectileCue>.FallInto(program);

    /// <summary>Continues at the start of <paramref name="program"/>.</summary>
    private static AnimationProgramStep<DraygonProjectileProgram, DraygonProjectileFrame, DraygonProjectileCue> GoTo(DraygonProjectileProgram program) =>
        AnimationProgramStep<DraygonProjectileProgram, DraygonProjectileFrame, DraygonProjectileCue>.GoTo(program);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<DraygonProjectileProgram, DraygonProjectileFrame, DraygonProjectileCue> Cue(DraygonProjectileCue cue) =>
        AnimationProgramStep<DraygonProjectileProgram, DraygonProjectileFrame, DraygonProjectileCue>.Publish(cue);

    /// <summary>Shows <paramref name="count"/> consecutive frames of <paramref name="kind"/>.</summary>
    private static IEnumerable<AnimationProgramStep<DraygonProjectileProgram, DraygonProjectileFrame, DraygonProjectileCue>> Frames(
        DraygonProjectileFrameKind kind, int count, Func<int, int> duration) =>
        Enumerable.Range(0, count).Select(phase =>
            AnimationProgramStep<DraygonProjectileProgram, DraygonProjectileFrame, DraygonProjectileCue>.Show(new(kind, phase), duration(phase)));
}
