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
            Show(EnemyFrameId.ZoaShootLeft0, 64),
            Cue(new(K.SetSpeedStage2)),
            Show(EnemyFrameId.ZoaShootLeft1, 8),
            Cue(new(K.SetSpeedStage3)),
            Show(EnemyFrameId.ZoaShootLeft2, 48),
        ],
        [P.FacingLeftRising] =
        [
            Show(EnemyFrameId.ZoaRiseLeft0, 4),
            Show(EnemyFrameId.ZoaRiseLeft1, 4),
            Show(EnemyFrameId.ZoaRiseLeft2, 4),
        ],
        [P.FacingRightShooting] =
        [
            Cue(new(K.SetSpeedStage1)),
            Show(EnemyFrameId.ZoaShootRight0, 64),
            Cue(new(K.SetSpeedStage2)),
            Show(EnemyFrameId.ZoaShootRight1, 8),
            Cue(new(K.SetSpeedStage3)),
            Show(EnemyFrameId.ZoaShootRight2, 48),
        ],
        [P.FacingRightRising] =
        [
            Show(EnemyFrameId.ZoaRiseRight0, 4),
            Show(EnemyFrameId.ZoaRiseRight1, 4),
            Show(EnemyFrameId.ZoaRiseRight2, 4),
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
