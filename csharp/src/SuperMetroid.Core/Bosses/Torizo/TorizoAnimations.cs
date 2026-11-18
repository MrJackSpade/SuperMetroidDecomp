using System.Collections.Frozen;
using SuperMetroid.Core.Animation;
using A = SuperMetroid.Core.Bosses.Torizo.TorizoAnimation;
using S = SuperMetroid.Core.Bosses.Torizo.TorizoBodySegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Bomb and Golden Torizo's body animations (#641, #1600, #1788, #2235, #2411): each segment's steps, each frame showing an
/// <see cref="EnemyFrameId"/>, each cue a <see cref="TorizoCue"/> with the values it acts on, each artwork change naming
/// the <see cref="TorizoArtworkLook"/> it shows; the segments each animation plays whole, once or a number of times; and
/// whether it then stops, loops or holds. A cue that pauses for a fixed time is followed by a wait of that length, one
/// wait with any that follows it. The runs several animations play alike (a turn, a walk's steps, a fall's pose, a volley's
/// burst) are one segment each plays, and a run the Torizo holds until its state changes (a jump's rise, Golden Torizo's
/// drop, its open chest) is the segment its animation loops on, whose mark tells the owner to start the next animation
/// once that change has come.
/// </summary>
internal static class TorizoAnimations
{
    /// <summary>Every segment's steps.</summary>
    internal static AnimationSegments<TorizoBodySegment, EnemyFrameId, TorizoCue> Segments { get; } = new(
        new Dictionary<TorizoBodySegment, AnimationBeat<EnemyFrameId, TorizoCue>[]>(
            [.. TorizoEncounterAnimations.Segments, .. TorizoFacedAnimations.Segments, .. GoldenTorizoAnimations.Segments]));

    /// <summary>
    /// Each animation (#2235, #2411): the segments it plays whole, once or a number of times, and whether it then stops,
    /// loops or holds.
    /// </summary>
    internal static IReadOnlyDictionary<TorizoAnimation, Animation<TorizoBodySegment>> StartingAt { get; } = new Dictionary<TorizoAnimation, Animation<TorizoBodySegment>>
    {
        [A.BombTorizoGutBlowingUp] = new([Segments.Whole(S.BombTorizoGutBlowingUp)], AnimationEnd.Holds),
        [A.BombTorizoFaceBlowingUp] = new([Segments.Whole(S.BombTorizoFaceBlowingUp)], AnimationEnd.Holds),
        [A.Dying] = new([Segments.Whole(S.Dying), Segments.Repeated(S.DyingExplosions, 8), Segments.Whole(S.DyingFadeToBlack), Segments.Repeated(S.DyingFlickering, 14), Segments.Whole(S.DyingFinish)], AnimationEnd.Stops),
        [A.BombTorizoDormant] = new([Segments.Whole(S.BombTorizoDormant)], AnimationEnd.Holds),
        [A.BombTorizoAwakening] = Animation<S>.LeadingInto([Segments.Whole(S.BombTorizoAwakening), Segments.Repeated(S.SeatedEyesLightingAgain, 2), Segments.Whole(S.BombTorizoStandingUp), Segments.Repeated(S.StandingColourFade, 16), Segments.Whole(S.BombTorizoFightStart)], Walk(S.FacingLeftWalkingLeftLegMoving, S.FacingLeftWalkingRightLegMoving)),
        [A.Stunned] = new([Segments.Whole(S.Stunned), Segments.Repeated(S.StunnedEyesLightingAgain, 2), Segments.Whole(S.StunnedRecovery)], AnimationEnd.Holds),
        [A.TurningLeft] = ThenWalking(S.Turning, S.FacingLeftWalkingRightLegMoving, S.FacingLeftWalkingLeftLegMoving),
        [A.FacingLeftSpewingChozoOrbsRightFootForward] = Spew(S.FacingLeftSpewingChozoOrbsRightFootForward, S.BombChozoOrbBurst, 3, S.FacingLeftSpewingChozoOrbsRightFootForwardEnd),
        [A.FacingLeftSpewingChozoOrbsLeftFootForward] = Spew(S.FacingLeftSpewingChozoOrbsLeftFootForward, S.BombChozoOrbBurst, 3, S.FacingLeftSpewingChozoOrbsLeftFootForwardEnd),
        [A.FacingLeftSonicBoomsRightFootForward] = SonicBooms(S.FacingLeftSonicBoomsRightFootForwardThrowing, S.FacingLeftSonicBoomsRightFootForwardEnd),
        [A.FacingLeftSonicBoomsLeftFootForward] = SonicBooms(S.FacingLeftSonicBoomsLeftFootForwardThrowing, S.FacingLeftSonicBoomsLeftFootForwardEnd),
        [A.FacingLeftExplosiveSwipeRightFootForward] = new([Segments.Whole(S.FacingLeftExplosiveSwipeRightFootForward)], AnimationEnd.Holds),
        [A.FacingLeftExplosiveSwipeLeftFootForward] = new([Segments.Whole(S.FacingLeftExplosiveSwipeLeftFootForward)], AnimationEnd.Holds),
        [A.FacingLeftJumpingForward] = Jump(S.FacingLeftJumpingForward, S.FacingLeftRising),
        [A.FacingLeftFalling] = Fall(S.FacingLeftFalling, S.FacingLeftFallingPose),
        [A.FacingLeftLanding] = ThenWalking(S.FacingLeftLanding, S.FacingLeftWalkingLeftLegMoving, S.FacingLeftWalkingRightLegMoving),
        [A.FacingLeftJumpingBackwardToLeftFoot] = Jump(S.FacingLeftJumpingBackwardToLeftFoot, S.FacingLeftRising),
        [A.FacingLeftFallingBackwardToLeftFoot] = Fall(S.FacingLeftFallingBackwardToLeftFoot, S.FacingLeftFallingPose),
        [A.FacingLeftLandingBackwardOnLeftFoot] = ThenWalking(S.FacingLeftLandingBackwardOnLeftFoot, S.FacingLeftWalkingRightLegMoving, S.FacingLeftWalkingLeftLegMoving),
        [A.FacingLeftJumpingBackwardToRightFoot] = Jump(S.FacingLeftJumpingBackwardToRightFoot, S.FacingLeftRising),
        [A.FacingLeftFallingBackwardToRightFoot] = Fall(S.FacingLeftFallingBackwardToRightFoot, S.FacingLeftFallingPose),
        [A.FacingLeftLandingBackwardOnRightFoot] = ThenWalking(S.FacingLeftLandingBackwardOnRightFoot, S.FacingLeftWalkingLeftLegMoving, S.FacingLeftWalkingRightLegMoving),
        [A.FacelessTurningLeft] = ThenWalking(S.TurningQuickly, S.FacingLeftFacelessWalkingRightLegMoving, S.FacingLeftFacelessWalkingLeftLegMoving),
        [A.FacingLeftFacelessWalkingRightLegMoving] = Walking(S.FacingLeftFacelessWalkingRightLegMoving, S.FacingLeftFacelessWalkingLeftLegMoving),
        [A.FacingLeftFacelessWalkingLeftLegMoving] = Walking(S.FacingLeftFacelessWalkingLeftLegMoving, S.FacingLeftFacelessWalkingRightLegMoving),
        [A.TurningRight] = ThenWalking(S.Turning, S.FacingRightWalkingLeftLegMoving, S.FacingRightWalkingRightLegMoving),
        [A.FacingRightSpewingChozoOrbsLeftFootForward] = Spew(S.FacingRightSpewingChozoOrbsLeftFootForward, S.BombChozoOrbBurst, 3, S.FacingRightSpewingChozoOrbsLeftFootForwardEnd),
        [A.FacingRightSpewingChozoOrbsRightFootForward] = Spew(S.FacingRightSpewingChozoOrbsRightFootForward, S.BombChozoOrbBurst, 3, S.FacingRightSpewingChozoOrbsRightFootForwardEnd),
        [A.FacingRightSonicBoomsLeftFootForward] = SonicBooms(S.FacingRightSonicBoomsLeftFootForwardThrowing, S.FacingRightSonicBoomsLeftFootForwardEnd),
        [A.FacingRightSonicBoomsRightFootForward] = SonicBooms(S.FacingRightSonicBoomsRightFootForwardThrowing, S.FacingRightSonicBoomsRightFootForwardEnd),
        [A.FacingRightExplosiveSwipeLeftFootForward] = new([Segments.Whole(S.FacingRightExplosiveSwipeLeftFootForward)], AnimationEnd.Holds),
        [A.FacingRightExplosiveSwipeRightFootForward] = new([Segments.Whole(S.FacingRightExplosiveSwipeRightFootForward)], AnimationEnd.Holds),
        [A.FacingRightJumpingForward] = Jump(S.FacingRightJumpingForward, S.FacingRightRising),
        [A.FacingRightFalling] = Fall(S.FacingRightFalling, S.FacingRightFallingPose),
        [A.FacingRightLanding] = ThenWalking(S.FacingRightLanding, S.FacingRightWalkingRightLegMoving, S.FacingRightWalkingLeftLegMoving),
        [A.FacingRightJumpingBackwardToRightFoot] = Jump(S.FacingRightJumpingBackwardToRightFoot, S.FacingRightRising),
        [A.FacingRightFallingBackwardToRightFoot] = Fall(S.FacingRightFallingBackwardToRightFoot, S.FacingRightFallingPose),
        [A.FacingRightLandingBackwardOnRightFoot] = ThenWalking(S.FacingRightLandingBackwardOnRightFoot, S.FacingRightWalkingLeftLegMoving, S.FacingRightWalkingRightLegMoving),
        [A.FacingRightJumpingBackwardToLeftFoot] = Jump(S.FacingRightJumpingBackwardToLeftFoot, S.FacingRightRising),
        [A.FacingRightFallingBackwardToLeftFoot] = Fall(S.FacingRightFallingBackwardToLeftFoot, S.FacingRightFallingPose),
        [A.FacingRightLandingBackwardOnLeftFoot] = ThenWalking(S.FacingRightLandingBackwardOnLeftFoot, S.FacingRightWalkingRightLegMoving, S.FacingRightWalkingLeftLegMoving),
        [A.FacelessTurningRight] = ThenWalking(S.TurningQuickly, S.FacingRightFacelessWalkingLeftLegMoving, S.FacingRightFacelessWalkingRightLegMoving),
        [A.FacingRightFacelessWalkingLeftLegMoving] = Walking(S.FacingRightFacelessWalkingLeftLegMoving, S.FacingRightFacelessWalkingRightLegMoving),
        [A.FacingRightFacelessWalkingRightLegMoving] = Walking(S.FacingRightFacelessWalkingRightLegMoving, S.FacingRightFacelessWalkingLeftLegMoving),
        [A.GoldenTorizoDormant] = new([Segments.Whole(S.GoldenTorizoDormant)], AnimationEnd.Holds),
        [A.GoldenTorizoAwakening] = Animation<S>.LeadingInto([Segments.Whole(S.GoldenTorizoAwakening)], [Segments.Whole(S.GoldenTorizoDropping)]),
        [A.GoldenTorizoSettling] = Animation<S>.LeadingInto([Segments.Whole(S.GoldenTorizoSettling), Segments.Repeated(S.SeatedEyesLightingAgain, 2), Segments.Whole(S.GoldenTorizoStandingUp), Segments.Repeated(S.StandingColourFade, 16), Segments.Whole(S.GoldenTorizoFightStart)], Walk(S.GoldenWalkingLeftLeftLegMoving, S.GoldenWalkingLeftRightLegMoving)),
        [A.GoldenFacingLeftSpewingChozoOrbsRightFootForward] = Spew(S.GoldenFacingLeftSpewingChozoOrbsRightFootForward, S.GoldenChozoOrbBurst, 6, S.GoldenFacingLeftSpewingChozoOrbsRightFootForwardEnd),
        [A.GoldenFacingLeftSpewingChozoOrbsLeftFootForward] = Spew(S.GoldenFacingLeftSpewingChozoOrbsLeftFootForward, S.GoldenChozoOrbBurst, 6, S.GoldenFacingLeftSpewingChozoOrbsLeftFootForwardEnd),
        [A.GoldenFacingLeftSonicBoomsRightFootForward] = SonicBooms(S.GoldenFacingLeftSonicBoomsRightFootForwardThrowing, S.GoldenFacingLeftSonicBoomsRightFootForwardEnd),
        [A.GoldenFacingLeftSonicBoomsLeftFootForward] = SonicBooms(S.GoldenFacingLeftSonicBoomsLeftFootForwardThrowing, S.GoldenFacingLeftSonicBoomsLeftFootForwardEnd),
        [A.GoldenFacingRightSpewingChozoOrbsLeftFootForward] = Spew(S.GoldenFacingRightSpewingChozoOrbsLeftFootForward, S.GoldenChozoOrbBurst, 6, S.GoldenFacingRightSpewingChozoOrbsLeftFootForwardEnd),
        [A.GoldenFacingRightSpewingChozoOrbsRightFootForward] = Spew(S.GoldenFacingRightSpewingChozoOrbsRightFootForward, S.GoldenChozoOrbBurst, 6, S.GoldenFacingRightSpewingChozoOrbsRightFootForwardEnd),
        [A.GoldenFacingRightSonicBoomsLeftFootForward] = SonicBooms(S.GoldenFacingRightSonicBoomsLeftFootForwardThrowing, S.GoldenFacingRightSonicBoomsLeftFootForwardEnd),
        [A.GoldenFacingRightSonicBoomsRightFootForward] = SonicBooms(S.GoldenFacingRightSonicBoomsRightFootForwardThrowing, S.GoldenFacingRightSonicBoomsRightFootForwardEnd),
        [A.GoldenFacingLeftLandedBackwardOnLeftFoot] = ThenWalking(S.GoldenFacingLeftLandedBackwardOnLeftFoot, S.GoldenWalkingLeftRightLegMoving, S.GoldenWalkingLeftLeftLegMoving),
        [A.GoldenFacingLeftLandedBackwardOnRightFoot] = ThenWalking(S.GoldenFacingLeftLandedBackwardOnRightFoot, S.GoldenWalkingLeftLeftLegMoving, S.GoldenWalkingLeftRightLegMoving),
        [A.GoldenFacingRightLandedBackwardOnRightFoot] = ThenWalking(S.GoldenFacingRightLandedBackwardOnRightFoot, S.GoldenWalkingRightLeftLegMoving, S.GoldenWalkingRightRightLegMoving),
        [A.GoldenFacingRightLandedBackwardOnLeftFoot] = ThenWalking(S.GoldenFacingRightLandedBackwardOnLeftFoot, S.GoldenWalkingRightRightLegMoving, S.GoldenWalkingRightLeftLegMoving),
        [A.GoldenFacingLeftCatchingSuperMissileLeftLegForward] = ThenWalking(S.GoldenFacingLeftCatchingSuperMissileLeftLegForward, S.GoldenWalkingLeftRightLegMoving, S.GoldenWalkingLeftLeftLegMoving),
        [A.GoldenFacingLeftCatchingSuperMissileRightLegForward] = ThenWalking(S.GoldenFacingLeftCatchingSuperMissileRightLegForward, S.GoldenWalkingLeftLeftLegMoving, S.GoldenWalkingLeftRightLegMoving),
        [A.GoldenFacingRightCatchingSuperMissileRightLegForward] = ThenWalking(S.GoldenFacingRightCatchingSuperMissileRightLegForward, S.GoldenWalkingRightLeftLegMoving, S.GoldenWalkingRightRightLegMoving),
        [A.GoldenFacingRightCatchingSuperMissileLeftLegForward] = ThenWalking(S.GoldenFacingRightCatchingSuperMissileLeftLegForward, S.GoldenWalkingRightRightLegMoving, S.GoldenWalkingRightLeftLegMoving),
        [A.GoldenFacingLeftSittingDownAttack] = ThenWalking(S.GoldenFacingLeftSittingDownAttack, S.GoldenWalkingLeftLeftLegMoving, S.GoldenWalkingLeftRightLegMoving),
        [A.GoldenFacingRightSittingDownAttack] = ThenWalking(S.GoldenFacingRightSittingDownAttack, S.GoldenWalkingRightRightLegMoving, S.GoldenWalkingRightLeftLegMoving),
        [A.GoldenReleasingEggs] = Animation<S>.LeadingInto([Segments.Whole(S.GoldenReleasingEggs), Segments.Repeated(S.GoldenEggLaying, 6)], [Segments.Whole(S.GoldenChestHeldOpen)]),
        [A.GoldenChestClosing] = new([Segments.Whole(S.GoldenChestClosing)], AnimationEnd.Holds),
        [A.GoldenEyeBeamAttack] = new([Segments.Whole(S.GoldenEyeBeamAttack), Segments.Repeated(S.GoldenEyeBeamFiring, 4), Segments.Whole(S.GoldenEyeBeamPause), Segments.Repeated(S.GoldenEyeBeamGlow, 2), Segments.Whole(S.GoldenEyeBeamEnd)], AnimationEnd.Holds),
        [A.GoldenDodgeTurningLeft] = ThenWalking(S.DodgeTurning, S.GoldenWalkingLeftRightLegMoving, S.GoldenWalkingLeftLeftLegMoving),
        [A.GoldenTurningLeft] = ThenWalking(S.TurningQuickly, S.GoldenWalkingLeftRightLegMoving, S.GoldenWalkingLeftLeftLegMoving),
        [A.GoldenWalkingLeftLeftLegMoving] = Walking(S.GoldenWalkingLeftLeftLegMoving, S.GoldenWalkingLeftRightLegMoving),
        [A.GoldenDodgeTurningRight] = ThenWalking(S.DodgeTurning, S.GoldenWalkingRightLeftLegMoving, S.GoldenWalkingRightRightLegMoving),
        [A.GoldenTurningRight] = ThenWalking(S.TurningQuickly, S.GoldenWalkingRightLeftLegMoving, S.GoldenWalkingRightRightLegMoving),
        [A.GoldenWalkingRightRightLegMoving] = Walking(S.GoldenWalkingRightRightLegMoving, S.GoldenWalkingRightLeftLegMoving),
        [A.FacingLeftWalkingRightLegMoving] = Walking(S.FacingLeftWalkingRightLegMoving, S.FacingLeftWalkingLeftLegMoving),
        [A.FacingLeftWalkingLeftLegMoving] = Walking(S.FacingLeftWalkingLeftLegMoving, S.FacingLeftWalkingRightLegMoving),
        [A.FacingRightWalkingLeftLegMoving] = Walking(S.FacingRightWalkingLeftLegMoving, S.FacingRightWalkingRightLegMoving),
        [A.FacingRightWalkingRightLegMoving] = Walking(S.FacingRightWalkingRightLegMoving, S.FacingRightWalkingLeftLegMoving),
        [A.GoldenWalkingLeftRightLegMoving] = Walking(S.GoldenWalkingLeftRightLegMoving, S.GoldenWalkingLeftLeftLegMoving),
        [A.GoldenWalkingRightLeftLegMoving] = Walking(S.GoldenWalkingRightLeftLegMoving, S.GoldenWalkingRightRightLegMoving),
    }.ToFrozenDictionary();

    /// <summary>A walk's two steps, <paramref name="first"/> then <paramref name="second"/>, each played once.</summary>
    private static AnimationPhase<TorizoBodySegment>[] Walk(TorizoBodySegment first, TorizoBodySegment second) => [Segments.Whole(first), Segments.Whole(second)];

    /// <summary>The walk that steps <paramref name="first"/>, then <paramref name="second"/>, and loops.</summary>
    private static Animation<TorizoBodySegment> Walking(TorizoBodySegment first, TorizoBodySegment second) =>
        Animation<TorizoBodySegment>.Looping(Walk(first, second));

    /// <summary><paramref name="lead"/> played once, then the walk that steps <paramref name="first"/>, then <paramref name="second"/>, looping (#2331).</summary>
    private static Animation<TorizoBodySegment> ThenWalking(TorizoBodySegment lead, TorizoBodySegment first, TorizoBodySegment second) =>
        Animation<TorizoBodySegment>.LeadingInto([Segments.Whole(lead)], Walk(first, second));

    /// <summary>A spew: its lead, <paramref name="plays"/> bursts, then its end, holding on its last frame.</summary>
    private static Animation<TorizoBodySegment> Spew(TorizoBodySegment lead, TorizoBodySegment burst, int plays, TorizoBodySegment end) =>
        new([Segments.Whole(lead), Segments.Repeated(burst, plays), Segments.Whole(end)], AnimationEnd.Holds);

    /// <summary>A volley of sonic booms: the Torizo stands its ground, throws four times, then ends, holding on its last frame.</summary>
    private static Animation<TorizoBodySegment> SonicBooms(TorizoBodySegment throwing, TorizoBodySegment end) =>
        new([Segments.Whole(S.StandingToThrowSonicBooms), Segments.Repeated(throwing, 4), Segments.Whole(end)], AnimationEnd.Holds);

    /// <summary>
    /// A jump: its lead, then the rising pose it loops on until its owner starts the fall its lead named, once the Torizo no
    /// longer rises (#2411).
    /// </summary>
    private static Animation<TorizoBodySegment> Jump(TorizoBodySegment lead, TorizoBodySegment rising) =>
        Animation<TorizoBodySegment>.LeadingInto([Segments.Whole(lead)], [Segments.Whole(rising)]);

    /// <summary>A fall: its start, then the pose it loops on.</summary>
    private static Animation<TorizoBodySegment> Fall(TorizoBodySegment start, TorizoBodySegment pose) =>
        Animation<TorizoBodySegment>.LeadingInto([Segments.Whole(start)], [Segments.Whole(pose)]);

    /// <summary>How <paramref name="animation"/> plays.</summary>
    internal static Animation<TorizoBodySegment> StartingWith(TorizoAnimation animation) => StartingAt[animation];

    /// <summary>A playback of <paramref name="animation"/>, before its first play.</summary>
    internal static AnimationPlayback<TorizoBodySegment> Start(TorizoAnimation animation) => new(StartingWith(animation), AnimationPlace.StartOfAnimation);

    /// <summary>A cue's continuation into <paramref name="animation"/>, from its start.</summary>
    internal static AnimationContinuation<TorizoBodySegment> ContinueWith(TorizoAnimation animation) => new(StartingWith(animation), AnimationPlace.StartOfAnimation);

    /// <summary>A cue's continuation where <paramref name="playback"/> stood, as a resumed walk or interrupted animation continues there.</summary>
    internal static AnimationContinuation<TorizoBodySegment> ContinueAt(AnimationPlayback<TorizoBodySegment> playback) =>
        new(Ensure.NotNull(playback.Animation), playback.Place);
}
