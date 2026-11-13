using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Zoa.ZoaCueKind;
using P = SuperMetroid.Core.Enemies.Zoa.ZoaProgram;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Zoa;

/// <summary>
/// Zoa's shooting and rising animations as typed programs (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each callback becomes a <see cref="ZoaCue"/> with its typed operands.
/// </summary>
internal static class ZoaAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<ZoaProgram, EnemyFrameId, ZoaCue> Segments { get; } = new(
        new Dictionary<ZoaProgram, AnimationBeat<EnemyFrameId, ZoaCue>[]>
    {
        [P.FacingLeftShooting] =
        [
            Cue(new(K.SetSpeedStage1)),
            Show(EnemyFrameId.ZoaFacingLeftShootingFirst, 64),
            Cue(new(K.SetSpeedStage2)),
            Show(EnemyFrameId.ZoaFacingLeftShootingSecond, 8),
            Cue(new(K.SetSpeedStage3)),
            Show(EnemyFrameId.ZoaFacingLeftShootingThird, 48),
        ],
        [P.FacingLeftRising] =
        [
            Show(EnemyFrameId.ZoaFacingLeftRisingFirst, 4),
            Show(EnemyFrameId.ZoaFacingLeftRisingSecond, 4),
            Show(EnemyFrameId.ZoaFacingLeftRisingThird, 4),
        ],
        [P.FacingRightShooting] =
        [
            Cue(new(K.SetSpeedStage1)),
            Show(EnemyFrameId.ZoaFacingRightShootingFirst, 64),
            Cue(new(K.SetSpeedStage2)),
            Show(EnemyFrameId.ZoaFacingRightShootingSecond, 8),
            Cue(new(K.SetSpeedStage3)),
            Show(EnemyFrameId.ZoaFacingRightShootingThird, 48),
        ],
        [P.FacingRightRising] =
        [
            Show(EnemyFrameId.ZoaFacingRightRisingFirst, 4),
            Show(EnemyFrameId.ZoaFacingRightRisingSecond, 4),
            Show(EnemyFrameId.ZoaFacingRightRisingThird, 4),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<ZoaProgram, Animation<ZoaProgram>> StartingAt { get; } = new Dictionary<ZoaProgram, Animation<ZoaProgram>>
    {
        [P.FacingLeftShooting] = new([new(P.FacingLeftShooting, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.FacingLeftRising] = new([new(P.FacingLeftRising, 0, 3, 1)], AnimationEnd.LoopsFrom(0)),
        [P.FacingRightShooting] = new([new(P.FacingRightShooting, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.FacingRightRising] = new([new(P.FacingRightRising, 0, 3, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<ZoaProgram> StartingWith(ZoaProgram segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, ZoaCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, ZoaCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, ZoaCue> Cue(ZoaCue cue) => new AnimationBeat<EnemyFrameId, ZoaCue>.Event(cue);
}
