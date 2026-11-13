using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Kago.KagoBugProjectileCueKind;
using P = SuperMetroid.Core.Enemies.Kago.KagoBugProjectileProgram;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Kago;

/// <summary>
/// The Kago bugs' landing, falling, jumping and shot programs (#641). Each
/// frame shows a <see cref="EnemyProjectileFrameId"/> and each command becomes a <see cref="KagoBugProjectileCue"/> with its typed operands.
/// </summary>
internal static class KagoBugProjectileAnimations
{
    /// <summary>
    /// The updates a landed bug's idle pose shows before its animation starts the landing again (#1607), kept exact (#1627):
    /// when it runs out the landing replays and raises its idle cue again, which gameplay sees. The falling and jumping poses
    /// are held instead: their lists show the same pose again, so they rest on it for good.
    /// </summary>
    private const int LongestPose = 32767;

    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<KagoBugProjectileProgram, EnemyProjectileFrameId, KagoBugProjectileCue> Segments { get; } = new(
        new Dictionary<KagoBugProjectileProgram, AnimationBeat<EnemyProjectileFrameId, KagoBugProjectileCue>[]>
    {
        [P.SharedDelete] =
        [
            Cue(new(K.Delete)),
        ],
        [P.Landed] =
        [
            Show(EnemyProjectileFrameId.KagoBugLandedFirst, 5),
            Cue(new(K.StartIdle)),
            Show(EnemyProjectileFrameId.KagoBugLandedSecond, LongestPose),
        ],
        [P.Falling] =
        [
            Hold(EnemyProjectileFrameId.KagoBugFalling),
        ],
        [P.JumpStart] =
        [
            Show(EnemyProjectileFrameId.KagoBugJumpStartFirst, 16),
            Show(EnemyProjectileFrameId.KagoBugJumpStartSecond, 5),
            Cue(new(K.StartJump)),
        ],
        [P.JumpLoop] =
        [
            Hold(EnemyProjectileFrameId.KagoBugJumpLoop),
        ],
        [P.Shot] =
        [
            Cue(new(K.DrawFromSharedSheet)),
            Show(EnemyProjectileFrameId.KagoBugShotFirst, 4),
            Show(EnemyProjectileFrameId.KagoBugShotSecond, 4),
            Show(EnemyProjectileFrameId.KagoBugShotThird, 4),
            Show(EnemyProjectileFrameId.KagoBugShotFourth, 4),
            Show(EnemyProjectileFrameId.KagoBugShotFifth, 4),
            Cue(new(K.SpawnDrop)),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<KagoBugProjectileProgram, Animation<KagoBugProjectileProgram>> StartingAt { get; } = new Dictionary<KagoBugProjectileProgram, Animation<KagoBugProjectileProgram>>
    {
        [P.SharedDelete] = new([new(P.SharedDelete, 0, 1, 1)], AnimationEnd.Holds),
        [P.Landed] = new([new(P.Landed, 0, 3, 1)], AnimationEnd.LoopsFrom(0)),
        [P.Falling] = new([new(P.Falling, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.JumpStart] = new([new(P.JumpStart, 0, 3, 1), new(P.JumpLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(1)),
        [P.JumpLoop] = new([new(P.JumpLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.Shot] = new([new(P.Shot, 0, 7, 1), new(P.SharedDelete, 0, 1, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<KagoBugProjectileProgram> StartingWith(KagoBugProjectileProgram segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, KagoBugProjectileCue> Show(EnemyProjectileFrameId frame, int duration) => new AnimationBeat<EnemyProjectileFrameId, KagoBugProjectileCue>.Frame(frame, duration);

    /// <summary>Shows <paramref name="frame"/> for good: the animation rests on it until its owner plays another (#1607).</summary>
    private static AnimationBeat<EnemyProjectileFrameId, KagoBugProjectileCue> Hold(EnemyProjectileFrameId frame) => new AnimationBeat<EnemyProjectileFrameId, KagoBugProjectileCue>.HeldFrame(frame);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, KagoBugProjectileCue> Cue(KagoBugProjectileCue cue) => new AnimationBeat<EnemyProjectileFrameId, KagoBugProjectileCue>.Event(cue);
}
