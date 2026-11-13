using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.SpacePirates.WalkingSpacePirateCueKind;
using P = SuperMetroid.Core.Enemies.SpacePirates.WalkingSpacePirateProgram;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// The walking Space Pirates' walk, look, flinch and laser animations as typed programs (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each callback becomes a <see cref="WalkingSpacePirateCue"/> with its typed operands.
/// </summary>
internal static class WalkingSpacePirateAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<WalkingSpacePirateProgram, EnemyFrameId, WalkingSpacePirateCue> Segments { get; } = new(
        new Dictionary<WalkingSpacePirateProgram, AnimationBeat<EnemyFrameId, WalkingSpacePirateCue>[]>
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
            Show(EnemyFrameId.WalkingPirateWalkLeft0, 10),
            Show(EnemyFrameId.WalkingPirateWalkLeft1, 10),
            Show(EnemyFrameId.WalkingPirateWalkLeft2, 10),
            Show(EnemyFrameId.WalkingPirateWalkLeft3, 10),
            Show(EnemyFrameId.WalkingPirateWalkLeft4, 10),
            Show(EnemyFrameId.WalkingPirateWalkLeft5, 10),
            Show(EnemyFrameId.WalkingPirateWalkLeft6, 10),
            Show(EnemyFrameId.WalkingPirateWalkLeft7, 10),
        ],
        [P.FireLasersLeft] =
        [
            Cue(new WalkingSpacePirateCue.SetFunction(Function: WalkingSpacePirateFunction.AnimationOwnedNoOperation)),
            Show(EnemyFrameId.WalkingPirateFireLeft0, 24),
            Show(EnemyFrameId.WalkingPirateFireLeft1, 8),
            Show(EnemyFrameId.WalkingPirateFireLeft2, 8),
            Show(EnemyFrameId.WalkingPirateFireLeft3, 8),
            Cue(new WalkingSpacePirateCue.FireLaserLeft(LaserHeight: 8)),
            Show(EnemyFrameId.WalkingPirateFireLeft4, 8),
            Cue(new WalkingSpacePirateCue.FireLaserLeft(LaserHeight: 2)),
            Show(EnemyFrameId.WalkingPirateFireLeft5, 24),
            Cue(new WalkingSpacePirateCue.FireLaserLeft(LaserHeight: -8)),
            Show(EnemyFrameId.WalkingPirateFireLeft4, 8),
            Show(EnemyFrameId.WalkingPirateFireLeft3, 8),
            Show(EnemyFrameId.WalkingPirateFireLeft2, 8),
            Show(EnemyFrameId.WalkingPirateFireLeft1, 8),
            Cue(new WalkingSpacePirateCue.Command(K.ChooseMovement)),
        ],
        [P.LookingFacingLeft] =
        [
            Cue(new WalkingSpacePirateCue.SetFunction(Function: WalkingSpacePirateFunction.AnimationOwnedNoOperation)),
            Show(EnemyFrameId.WalkingPirateLookLeft0, 32),
            Show(EnemyFrameId.WalkingPirateLookLeft1, 10),
            Show(EnemyFrameId.WalkingPirateLookLeft2, 32),
            Show(EnemyFrameId.WalkingPirateLookLeft1, 10),
            Show(EnemyFrameId.WalkingPirateLookLeft0, 32),
            Show(EnemyFrameId.WalkingPirateLookShared, 8),
        ],
        [P.WalkingRight] =
        [
            Cue(new WalkingSpacePirateCue.SetFunction(Function: WalkingSpacePirateFunction.WalkingRight)),
        ],
        [P.WalkingRightLoop] =
        [
            Show(EnemyFrameId.WalkingPirateWalkRight0, 10),
            Show(EnemyFrameId.WalkingPirateWalkRight1, 10),
            Show(EnemyFrameId.WalkingPirateWalkRight2, 10),
            Show(EnemyFrameId.WalkingPirateWalkRight3, 10),
            Show(EnemyFrameId.WalkingPirateWalkRight4, 10),
            Show(EnemyFrameId.WalkingPirateWalkRight5, 10),
            Show(EnemyFrameId.WalkingPirateWalkRight6, 10),
            Show(EnemyFrameId.WalkingPirateWalkRight7, 10),
        ],
        [P.FireLasersRight] =
        [
            Cue(new WalkingSpacePirateCue.SetFunction(Function: WalkingSpacePirateFunction.AnimationOwnedNoOperation)),
            Show(EnemyFrameId.WalkingPirateFireRight0, 24),
            Show(EnemyFrameId.WalkingPirateFireRight1, 8),
            Show(EnemyFrameId.WalkingPirateFireRight2, 8),
            Show(EnemyFrameId.WalkingPirateFireRight3, 8),
            Cue(new WalkingSpacePirateCue.FireLaserRight(LaserHeight: 8)),
            Show(EnemyFrameId.WalkingPirateFireRight4, 8),
            Cue(new WalkingSpacePirateCue.FireLaserRight(LaserHeight: 2)),
            Show(EnemyFrameId.WalkingPirateFireRight5, 24),
            Cue(new WalkingSpacePirateCue.FireLaserRight(LaserHeight: -8)),
            Show(EnemyFrameId.WalkingPirateFireRight4, 8),
            Show(EnemyFrameId.WalkingPirateFireRight3, 8),
            Show(EnemyFrameId.WalkingPirateFireRight2, 8),
            Show(EnemyFrameId.WalkingPirateFireRight1, 8),
            Cue(new WalkingSpacePirateCue.Command(K.ChooseMovement)),
        ],
        [P.LookingFacingRight] =
        [
            Cue(new WalkingSpacePirateCue.SetFunction(Function: WalkingSpacePirateFunction.AnimationOwnedNoOperation)),
            Show(EnemyFrameId.WalkingPirateLookRight0, 32),
            Show(EnemyFrameId.WalkingPirateLookRight1, 10),
            Show(EnemyFrameId.WalkingPirateLookRight2, 32),
            Show(EnemyFrameId.WalkingPirateLookRight1, 10),
            Show(EnemyFrameId.WalkingPirateLookRight0, 32),
            Show(EnemyFrameId.WalkingPirateLookShared, 8),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<WalkingSpacePirateProgram, Animation<WalkingSpacePirateProgram>> StartingAt { get; } = new Dictionary<WalkingSpacePirateProgram, Animation<WalkingSpacePirateProgram>>
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
    internal static Animation<WalkingSpacePirateProgram> StartingWith(WalkingSpacePirateProgram segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, WalkingSpacePirateCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, WalkingSpacePirateCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, WalkingSpacePirateCue> Cue(WalkingSpacePirateCue cue) => new AnimationBeat<EnemyFrameId, WalkingSpacePirateCue>.Event(cue);
}
