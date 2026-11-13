using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.KiHunters.KiHunterCueKind;
using P = SuperMetroid.Core.Enemies.KiHunters.KiHunterProgram;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>
/// The Ki-Hunter body's flight, swoop, hop, landing and spit animations and its wings' animations as typed programs (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each callback becomes a <see cref="KiHunterCue"/> with its typed operands.
/// </summary>
internal static class KiHunterAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<KiHunterProgram, EnemyFrameId, KiHunterCue> Segments { get; } = new(
        new Dictionary<KiHunterProgram, AnimationBeat<EnemyFrameId, KiHunterCue>[]>
    {
        [P.FlyingLeft] =
        [
            Show(EnemyFrameId.KiHunterPose0, 2),
            Show(EnemyFrameId.KiHunterPose1, 2),
            Show(EnemyFrameId.KiHunterPose2, 1),
            Cue(new(K.ReturnToFlying)),
        ],
        [P.SwoopLeft] =
        [
            Show(EnemyFrameId.KiHunterPose14, 2),
            Show(EnemyFrameId.KiHunterPose15, 6),
            Show(EnemyFrameId.KiHunterPose14, 2),
            Show(EnemyFrameId.KiHunterPose13, 2),
            Show(EnemyFrameId.KiHunterPose16, 2),
            Show(EnemyFrameId.KiHunterPose17, 32),
        ],
        [P.FlyingRight] =
        [
            Show(EnemyFrameId.KiHunterPose6, 2),
            Show(EnemyFrameId.KiHunterPose7, 2),
            Show(EnemyFrameId.KiHunterPose8, 1),
            Cue(new(K.ReturnToFlying)),
        ],
        [P.SwoopRight] =
        [
            Show(EnemyFrameId.KiHunterPose19, 2),
            Show(EnemyFrameId.KiHunterPose20, 6),
            Show(EnemyFrameId.KiHunterPose19, 2),
            Show(EnemyFrameId.KiHunterPose18, 2),
            Show(EnemyFrameId.KiHunterPose21, 2),
            Show(EnemyFrameId.KiHunterPose22, 32),
        ],
        [P.WingsLeft] =
        [
            Show(EnemyFrameId.KiHunterPose3, 2),
            Show(EnemyFrameId.KiHunterPose4, 2),
            Show(EnemyFrameId.KiHunterPose5, 1),
        ],
        [P.WingsRight] =
        [
            Show(EnemyFrameId.KiHunterPose9, 2),
            Show(EnemyFrameId.KiHunterPose10, 2),
            Show(EnemyFrameId.KiHunterPose11, 1),
        ],
        [P.DetachedWings] =
        [
            Show(EnemyFrameId.KiHunterPose12, 1),
        ],
        [P.JumpLeft] =
        [
            Show(EnemyFrameId.KiHunterPose23, 8),
            Show(EnemyFrameId.KiHunterPose24, 8),
            Show(EnemyFrameId.KiHunterPose25, 11),
            Show(EnemyFrameId.KiHunterPose24, 2),
            Show(EnemyFrameId.KiHunterPose23, 2),
            Cue(new(K.StartHop)),
            Show(EnemyFrameId.KiHunterPose26, 1),
        ],
        [P.JumpRight] =
        [
            Show(EnemyFrameId.KiHunterPose27, 8),
            Show(EnemyFrameId.KiHunterPose28, 8),
            Show(EnemyFrameId.KiHunterPose29, 11),
            Show(EnemyFrameId.KiHunterPose28, 2),
            Show(EnemyFrameId.KiHunterPose27, 2),
            Cue(new(K.StartHop)),
            Show(EnemyFrameId.KiHunterPose30, 1),
        ],
        [P.LandLeft] =
        [
            Show(EnemyFrameId.KiHunterPose23, 8),
            Show(EnemyFrameId.KiHunterPose24, 8),
            Show(EnemyFrameId.KiHunterPose25, 11),
            Show(EnemyFrameId.KiHunterPose24, 8),
            Cue(new(K.StartWinglessThinking)),
            Show(EnemyFrameId.KiHunterPose23, 1),
        ],
        [P.LandRight] =
        [
            Show(EnemyFrameId.KiHunterPose27, 8),
            Show(EnemyFrameId.KiHunterPose28, 8),
            Show(EnemyFrameId.KiHunterPose29, 11),
            Show(EnemyFrameId.KiHunterPose28, 8),
            Cue(new(K.StartWinglessThinking)),
            Show(EnemyFrameId.KiHunterPose27, 1),
        ],
        [P.SpitLeft] =
        [
            Show(EnemyFrameId.KiHunterPose31, 32),
            Show(EnemyFrameId.KiHunterPose32, 6),
            Show(EnemyFrameId.KiHunterPose33, 16),
            Show(EnemyFrameId.KiHunterPose34, 2),
            Cue(new(K.SpitLeft)),
            Show(EnemyFrameId.KiHunterPose35, 24),
            Cue(new(K.StartWinglessThinking)),
            Show(EnemyFrameId.KiHunterPose31, 1),
        ],
        [P.SpitRight] =
        [
            Show(EnemyFrameId.KiHunterPose36, 32),
            Show(EnemyFrameId.KiHunterPose37, 6),
            Show(EnemyFrameId.KiHunterPose38, 16),
            Show(EnemyFrameId.KiHunterPose39, 2),
            Cue(new(K.SpitRight)),
            Show(EnemyFrameId.KiHunterPose40, 24),
            Cue(new(K.StartWinglessThinking)),
            Show(EnemyFrameId.KiHunterPose36, 1),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<KiHunterProgram, Animation<KiHunterProgram>> StartingAt { get; } = new Dictionary<KiHunterProgram, Animation<KiHunterProgram>>
    {
        [P.FlyingLeft] = new([new(P.FlyingLeft, 0, 4, 1), new(P.SwoopLeft, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SwoopLeft] = new([new(P.SwoopLeft, 0, 6, 1), new(P.FlyingLeft, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.FlyingRight] = new([new(P.FlyingRight, 0, 4, 1), new(P.SwoopRight, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SwoopRight] = new([new(P.SwoopRight, 0, 6, 1), new(P.FlyingRight, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.WingsLeft] = new([new(P.WingsLeft, 0, 3, 1)], AnimationEnd.LoopsFrom(0)),
        [P.WingsRight] = new([new(P.WingsRight, 0, 3, 1)], AnimationEnd.LoopsFrom(0)),
        [P.DetachedWings] = new([new(P.DetachedWings, 0, 1, 1)], AnimationEnd.Holds),
        [P.JumpLeft] = new([new(P.JumpLeft, 0, 7, 1)], AnimationEnd.Holds),
        [P.JumpRight] = new([new(P.JumpRight, 0, 7, 1)], AnimationEnd.Holds),
        [P.LandLeft] = new([new(P.LandLeft, 0, 6, 1)], AnimationEnd.Holds),
        [P.LandRight] = new([new(P.LandRight, 0, 6, 1)], AnimationEnd.Holds),
        [P.SpitLeft] = new([new(P.SpitLeft, 0, 8, 1)], AnimationEnd.Holds),
        [P.SpitRight] = new([new(P.SpitRight, 0, 8, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<KiHunterProgram> StartingWith(KiHunterProgram segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, KiHunterCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, KiHunterCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, KiHunterCue> Cue(KiHunterCue cue) => new AnimationBeat<EnemyFrameId, KiHunterCue>.Event(cue);
}
