using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.SpacePirates.WalkingSpacePirateCueKind;
using P = SuperMetroid.Core.Enemies.SpacePirates.WalkingSpacePirateSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// The walking Space Pirates' walk, look, flinch and laser animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="WalkingSpacePirateCue"/> with the values it acts on.
/// </summary>
internal static class WalkingSpacePirateAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<WalkingSpacePirateSegment, EnemyFrameId, WalkingSpacePirateCue> Segments { get; } = new(
        new Dictionary<WalkingSpacePirateSegment, AnimationBeat<EnemyFrameId, WalkingSpacePirateCue>[]>
    {
        [P.FlinchFacingLeft] =
        [
            Cue(new WalkingSpacePirateCue.SetFunction(Function: WalkingSpacePirateFunction.NoOperation)),
            Show(EnemyFrameId.WalkingPirateFlinchLeft, 16),
        ],
        [P.FlinchFacingRight] =
        [
            Cue(new WalkingSpacePirateCue.SetFunction(Function: WalkingSpacePirateFunction.NoOperation)),
            Show(EnemyFrameId.WalkingPirateFlinchRight, 16),
        ],
        [P.WalkingLeft] =
        [
            Cue(new WalkingSpacePirateCue.SetFunction(Function: WalkingSpacePirateFunction.WalkingLeft)),
        ],
        [P.WalkingLeftLoop] =
        [
            Show(EnemyFrameId.WalkingPirateWalkingLeftLoopFirst, 10),
            Show(EnemyFrameId.WalkingPirateWalkingLeftLoopSecond, 10),
            Show(EnemyFrameId.WalkingPirateWalkingLeftLoopThird, 10),
            Show(EnemyFrameId.WalkingPirateWalkingLeftLoopFourth, 10),
            Show(EnemyFrameId.WalkingPirateWalkingLeftLoopFifth, 10),
            Show(EnemyFrameId.WalkingPirateWalkingLeftLoopSixth, 10),
            Show(EnemyFrameId.WalkingPirateWalkingLeftLoopSeventh, 10),
            Show(EnemyFrameId.WalkingPirateWalkingLeftLoopEighth, 10),
        ],
        [P.FireLasersLeft] =
        [
            Cue(new WalkingSpacePirateCue.SetFunction(Function: WalkingSpacePirateFunction.AnimationOwnedNoOperation)),
            Show(EnemyFrameId.WalkingPirateFireLasersLeftFirst, 24),
            Show(EnemyFrameId.WalkingPirateFireLasersLeftSecond, 8),
            Show(EnemyFrameId.WalkingPirateFireLasersLeftThird, 8),
            Show(EnemyFrameId.WalkingPirateFireLasersLeftFourth, 8),
            Cue(new WalkingSpacePirateCue.FireLaserLeft(LaserHeight: 8)),
            Show(EnemyFrameId.WalkingPirateFireLasersLeftFifth, 8),
            Cue(new WalkingSpacePirateCue.FireLaserLeft(LaserHeight: 2)),
            Show(EnemyFrameId.WalkingPirateFireLasersLeftSixth, 24),
            Cue(new WalkingSpacePirateCue.FireLaserLeft(LaserHeight: -8)),
            Show(EnemyFrameId.WalkingPirateFireLasersLeftFifth, 8),
            Show(EnemyFrameId.WalkingPirateFireLasersLeftFourth, 8),
            Show(EnemyFrameId.WalkingPirateFireLasersLeftThird, 8),
            Show(EnemyFrameId.WalkingPirateFireLasersLeftSecond, 8),
            Cue(new WalkingSpacePirateCue.Signal(K.ChooseMovement)),
        ],
        [P.LookingFacingLeft] =
        [
            Cue(new WalkingSpacePirateCue.SetFunction(Function: WalkingSpacePirateFunction.AnimationOwnedNoOperation)),
            Show(EnemyFrameId.WalkingPirateLookingFacingLeftFirst, 32),
            Show(EnemyFrameId.WalkingPirateLookingFacingLeftSecond, 10),
            Show(EnemyFrameId.WalkingPirateLookingFacingLeftThird, 32),
            Show(EnemyFrameId.WalkingPirateLookingFacingLeftSecond, 10),
            Show(EnemyFrameId.WalkingPirateLookingFacingLeftFirst, 32),
            Show(EnemyFrameId.WalkingPirateLookShared, 8),
        ],
        [P.WalkingRight] =
        [
            Cue(new WalkingSpacePirateCue.SetFunction(Function: WalkingSpacePirateFunction.WalkingRight)),
        ],
        [P.WalkingRightLoop] =
        [
            Show(EnemyFrameId.WalkingPirateWalkingRightLoopFirst, 10),
            Show(EnemyFrameId.WalkingPirateWalkingRightLoopSecond, 10),
            Show(EnemyFrameId.WalkingPirateWalkingRightLoopThird, 10),
            Show(EnemyFrameId.WalkingPirateWalkingRightLoopFourth, 10),
            Show(EnemyFrameId.WalkingPirateWalkingRightLoopFifth, 10),
            Show(EnemyFrameId.WalkingPirateWalkingRightLoopSixth, 10),
            Show(EnemyFrameId.WalkingPirateWalkingRightLoopSeventh, 10),
            Show(EnemyFrameId.WalkingPirateWalkingRightLoopEighth, 10),
        ],
        [P.FireLasersRight] =
        [
            Cue(new WalkingSpacePirateCue.SetFunction(Function: WalkingSpacePirateFunction.AnimationOwnedNoOperation)),
            Show(EnemyFrameId.WalkingPirateFireLasersRightFirst, 24),
            Show(EnemyFrameId.WalkingPirateFireLasersRightSecond, 8),
            Show(EnemyFrameId.WalkingPirateFireLasersRightThird, 8),
            Show(EnemyFrameId.WalkingPirateFireLasersRightFourth, 8),
            Cue(new WalkingSpacePirateCue.FireLaserRight(LaserHeight: 8)),
            Show(EnemyFrameId.WalkingPirateFireLasersRightFifth, 8),
            Cue(new WalkingSpacePirateCue.FireLaserRight(LaserHeight: 2)),
            Show(EnemyFrameId.WalkingPirateFireLasersRightSixth, 24),
            Cue(new WalkingSpacePirateCue.FireLaserRight(LaserHeight: -8)),
            Show(EnemyFrameId.WalkingPirateFireLasersRightFifth, 8),
            Show(EnemyFrameId.WalkingPirateFireLasersRightFourth, 8),
            Show(EnemyFrameId.WalkingPirateFireLasersRightThird, 8),
            Show(EnemyFrameId.WalkingPirateFireLasersRightSecond, 8),
            Cue(new WalkingSpacePirateCue.Signal(K.ChooseMovement)),
        ],
        [P.LookingFacingRight] =
        [
            Cue(new WalkingSpacePirateCue.SetFunction(Function: WalkingSpacePirateFunction.AnimationOwnedNoOperation)),
            Show(EnemyFrameId.WalkingPirateLookingFacingRightFirst, 32),
            Show(EnemyFrameId.WalkingPirateLookingFacingRightSecond, 10),
            Show(EnemyFrameId.WalkingPirateLookingFacingRightThird, 32),
            Show(EnemyFrameId.WalkingPirateLookingFacingRightSecond, 10),
            Show(EnemyFrameId.WalkingPirateLookingFacingRightFirst, 32),
            Show(EnemyFrameId.WalkingPirateLookShared, 8),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<WalkingSpacePirateSegment, Animation<WalkingSpacePirateSegment>> StartingAt { get; } = new Dictionary<WalkingSpacePirateSegment, Animation<WalkingSpacePirateSegment>>
    {
        [P.FlinchFacingLeft] = new([new(P.FlinchFacingLeft, 0, 2, 1), new(P.WalkingLeft, 0, 1, 1), new(P.WalkingLeftLoop, 0, 8, 1)], AnimationEnd.LoopsFrom(2)),
        [P.FlinchFacingRight] = new([new(P.FlinchFacingRight, 0, 2, 1), new(P.WalkingRight, 0, 1, 1), new(P.WalkingRightLoop, 0, 8, 1)], AnimationEnd.LoopsFrom(2)),
        [P.WalkingLeft] = new([new(P.WalkingLeft, 0, 1, 1), new(P.WalkingLeftLoop, 0, 8, 1)], AnimationEnd.LoopsFrom(1)),
        [P.WalkingLeftLoop] = new([new(P.WalkingLeftLoop, 0, 8, 1)], AnimationEnd.LoopsFrom(0)),
        [P.FireLasersLeft] = new([new(P.FireLasersLeft, 0, 15, 1), new(P.LookingFacingLeft, 0, 7, 1), new(P.WalkingRight, 0, 1, 1), new(P.WalkingRightLoop, 0, 8, 1)], AnimationEnd.LoopsFrom(3)),
        [P.LookingFacingLeft] = new([new(P.LookingFacingLeft, 0, 7, 1), new(P.WalkingRight, 0, 1, 1), new(P.WalkingRightLoop, 0, 8, 1)], AnimationEnd.LoopsFrom(2)),
        [P.WalkingRight] = new([new(P.WalkingRight, 0, 1, 1), new(P.WalkingRightLoop, 0, 8, 1)], AnimationEnd.LoopsFrom(1)),
        [P.WalkingRightLoop] = new([new(P.WalkingRightLoop, 0, 8, 1)], AnimationEnd.LoopsFrom(0)),
        [P.FireLasersRight] = new([new(P.FireLasersRight, 0, 15, 1), new(P.LookingFacingRight, 0, 7, 1), new(P.WalkingLeft, 0, 1, 1), new(P.WalkingLeftLoop, 0, 8, 1)], AnimationEnd.LoopsFrom(3)),
        [P.LookingFacingRight] = new([new(P.LookingFacingRight, 0, 7, 1), new(P.WalkingLeft, 0, 1, 1), new(P.WalkingLeftLoop, 0, 8, 1)], AnimationEnd.LoopsFrom(2)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<WalkingSpacePirateSegment> StartingWith(WalkingSpacePirateSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, WalkingSpacePirateCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, WalkingSpacePirateCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, WalkingSpacePirateCue> Cue(WalkingSpacePirateCue cue) => new AnimationBeat<EnemyFrameId, WalkingSpacePirateCue>.Event(cue);
}
