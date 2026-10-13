using SuperMetroid.Core.Assets;
using P = SuperMetroid.Core.Game.DraygonProjectileProgram;

namespace SuperMetroid.Core.Game;

/// <summary>Draygon's goop and wall-turret projectile programs.</summary>
public enum DraygonProjectileProgram
{
    /// <summary><c>InstList_EnemyProjectile_DraygonGoop_Touch</c> ($86:8C38): sticks the goop to Samus, then runs on into the goop loop.</summary>
    GoopTouch,
    /// <summary><c>InstList_EnemyProjectile_DraygonGoop</c> ($86:8C3A): six ten-update frames, looping.</summary>
    Goop,
    /// <summary><c>InstList_EnemyProjectile_DraygonGoop_Shot</c> ($86:8C58): two eight-update frames, a drop, then deletion.</summary>
    GoopShot,
    /// <summary><c>InstList_EnemyProjectile_DraygonsWallTurretProjectile_0</c> ($86:8CA4): the sixteen-frame bloom, then the shot is fired.</summary>
    WallTurretBloom,
    /// <summary><c>InstList_EnemyProjectile_DraygonsWallTurretProjectile_1</c> ($86:8CE6): three eight-update frames, looping.</summary>
    WallTurretFlight,
}

/// <summary>What a Draygon projectile frame belongs to.</summary>
public enum DraygonProjectileFrameKind
{
    /// <summary>The goop in flight.</summary>
    Goop,
    /// <summary>The goop bursting after a Samus shot.</summary>
    GoopShot,
    /// <summary>The wall turret's shot forming.</summary>
    WallTurretBloom,
    /// <summary>The wall turret's shot in flight.</summary>
    WallTurretFlight,
}

/// <summary>One Draygon projectile frame: its program and its position there.</summary>
/// <param name="Kind">The program the frame belongs to.</param>
/// <param name="Phase">The frame's position in that program.</param>
public readonly record struct DraygonProjectileFrame(DraygonProjectileFrameKind Kind, int Phase);

/// <summary>Gameplay cues of Draygon's projectile programs.</summary>
internal enum DraygonProjectileCue
{
    /// <summary>Sticks the goop to Samus.</summary>
    StickToSamus,
    /// <summary>Spawns an enemy drop with Draygon's eye's drop chances where the goop burst.</summary>
    SpawnDrops,
    /// <summary>Deletes the projectile.</summary>
    Delete,
    /// <summary>Gives the wall turret's shot its fired pre-instruction.</summary>
    Fire,
}

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

/// <summary>A Draygon goop's or wall-turret shot's typed animation.</summary>
internal sealed class DraygonProjectileAnimation(DraygonProjectileProgram program)
    : EnemyProjectileProgramAnimation<DraygonProjectileProgram, DraygonProjectileFrame, DraygonProjectileCue>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<DraygonProjectileProgram, DraygonProjectileFrame, DraygonProjectileCue> Programs =>
        DraygonProjectileAnimations.Programs;

    /// <inheritdoc/>
    private protected override ushort PresentationKey(DraygonProjectileFrame frame) => DraygonProjectileVisualDefinitions.PresentationKey(frame);
}
