using System.Collections.Frozen;
using SuperMetroid.Core.Animation;
using A = SuperMetroid.Core.Bosses.Torizo.TorizoAnimation;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Bomb and Golden Torizo's body animations (#641, #1600, #1788, #2235): each segment's steps, each frame showing a
/// <see cref="EnemyFrameId"/>, each cue a <see cref="TorizoCue"/> with the values it acts on, each artwork change naming
/// the <see cref="TorizoArtworkLook"/> it shows; the segments each animation plays whole, once or a number of times; and
/// whether it then stops, loops or holds. A cue that pauses for a fixed time is followed by a wait of that length, one
/// wait with any that follows it. The runs several animations play alike (a walk's steps, a fall's pose, a volley's
/// burst) are one segment each plays.
/// </summary>
internal static class TorizoAnimations
{
    /// <summary>Every segment's steps.</summary>
    internal static AnimationSegments<TorizoAnimation, EnemyFrameId, TorizoCue> Segments { get; } = new(
        new Dictionary<TorizoAnimation, AnimationBeat<EnemyFrameId, TorizoCue>[]>(
            [.. TorizoEncounterAnimations.Segments, .. TorizoFacingLeftAnimations.Segments, .. TorizoFacingRightAnimations.Segments,
             .. GoldenTorizoAnimations.Segments]));

    /// <summary>
    /// The animation that starts at each segment an owner or a cue starts (#2235): the segments it plays whole, once or a
    /// number of times, and whether it then stops, loops or holds.
    /// </summary>
    internal static IReadOnlyDictionary<TorizoAnimation, Animation<TorizoAnimation>> StartingAt { get; } = new Dictionary<TorizoAnimation, Animation<TorizoAnimation>>
    {
        [A.BombTorizoGutBlowingUp] = new([Whole(A.BombTorizoGutBlowingUp)], AnimationEnd.Holds),
        [A.BombTorizoFaceBlowingUp] = new([Whole(A.BombTorizoFaceBlowingUp)], AnimationEnd.Holds),
        [A.Dying] = new([Whole(A.Dying), Repeat(A.DyingExplosions, 8), Whole(A.DyingFadeToBlack), Repeat(A.DyingFlickering, 14), Whole(A.DyingFinish)], AnimationEnd.Stops),
        [A.BombTorizoDormant] = new([Whole(A.BombTorizoDormant)], AnimationEnd.Holds),
        [A.BombTorizoAwakening] = Animation<A>.LeadingInto([Whole(A.BombTorizoAwakening), Repeat(A.SeatedEyesLightingAgain, 2), Whole(A.BombTorizoStandingUp), Repeat(A.StandingColourFade, 16), Whole(A.BombTorizoFightStart)], Walk(A.FacingLeftWalkingLeftLegMoving, A.FacingLeftWalkingRightLegMoving)),
        [A.Stunned] = new([Whole(A.Stunned), Repeat(A.StunnedEyesLightingAgain, 2), Whole(A.StunnedRecovery)], AnimationEnd.Holds),
        [A.TurningLeft] = ThenWalking(A.TurningLeft, A.FacingLeftWalkingRightLegMoving, A.FacingLeftWalkingLeftLegMoving),
        [A.FacingLeftSpewingChozoOrbsRightFootForward] = Spew(A.FacingLeftSpewingChozoOrbsRightFootForward, A.BombChozoOrbBurst, 3, A.FacingLeftSpewingChozoOrbsRightFootForwardEnd),
        [A.FacingLeftSpewingChozoOrbsLeftFootForward] = Spew(A.FacingLeftSpewingChozoOrbsLeftFootForward, A.BombChozoOrbBurst, 3, A.FacingLeftSpewingChozoOrbsLeftFootForwardEnd),
        [A.FacingLeftSonicBoomsRightFootForward] = SonicBooms(A.FacingLeftSonicBoomsRightFootForward, A.FacingLeftSonicBoomsRightFootForwardThrowing, A.FacingLeftSonicBoomsRightFootForwardEnd),
        [A.FacingLeftSonicBoomsLeftFootForward] = SonicBooms(A.FacingLeftSonicBoomsLeftFootForward, A.FacingLeftSonicBoomsLeftFootForwardThrowing, A.FacingLeftSonicBoomsLeftFootForwardEnd),
        [A.FacingLeftExplosiveSwipeRightFootForward] = new([Whole(A.FacingLeftExplosiveSwipeRightFootForward)], AnimationEnd.Holds),
        [A.FacingLeftExplosiveSwipeLeftFootForward] = new([Whole(A.FacingLeftExplosiveSwipeLeftFootForward)], AnimationEnd.Holds),
        [A.FacingLeftJumpingForward] = Jump(A.FacingLeftJumpingForward, A.FacingLeftRising, A.FacingLeftFalling, A.FacingLeftFallingPose),
        [A.FacingLeftFalling] = Fall(A.FacingLeftFalling, A.FacingLeftFallingPose),
        [A.FacingLeftLanding] = ThenWalking(A.FacingLeftLanding, A.FacingLeftWalkingLeftLegMoving, A.FacingLeftWalkingRightLegMoving),
        [A.FacingLeftJumpingBackwardToLeftFoot] = Jump(A.FacingLeftJumpingBackwardToLeftFoot, A.FacingLeftRising, A.FacingLeftFallingBackwardToLeftFoot, A.FacingLeftFallingPose),
        [A.FacingLeftFallingBackwardToLeftFoot] = Fall(A.FacingLeftFallingBackwardToLeftFoot, A.FacingLeftFallingPose),
        [A.FacingLeftLandingBackwardOnLeftFoot] = ThenWalking(A.FacingLeftLandingBackwardOnLeftFoot, A.FacingLeftWalkingRightLegMoving, A.FacingLeftWalkingLeftLegMoving),
        [A.FacingLeftJumpingBackwardToRightFoot] = Jump(A.FacingLeftJumpingBackwardToRightFoot, A.FacingLeftRising, A.FacingLeftFallingBackwardToRightFoot, A.FacingLeftFallingPose),
        [A.FacingLeftFallingBackwardToRightFoot] = Fall(A.FacingLeftFallingBackwardToRightFoot, A.FacingLeftFallingPose),
        [A.FacingLeftLandingBackwardOnRightFoot] = ThenWalking(A.FacingLeftLandingBackwardOnRightFoot, A.FacingLeftWalkingLeftLegMoving, A.FacingLeftWalkingRightLegMoving),
        [A.FacelessTurningLeft] = ThenWalking(A.FacelessTurningLeft, A.FacingLeftFacelessWalkingRightLegMoving, A.FacingLeftFacelessWalkingLeftLegMoving),
        [A.FacingLeftFacelessWalkingRightLegMoving] = Walking(A.FacingLeftFacelessWalkingRightLegMoving, A.FacingLeftFacelessWalkingLeftLegMoving),
        [A.FacingLeftFacelessWalkingLeftLegMoving] = Walking(A.FacingLeftFacelessWalkingLeftLegMoving, A.FacingLeftFacelessWalkingRightLegMoving),
        [A.TurningRight] = ThenWalking(A.TurningRight, A.FacingRightWalkingLeftLegMoving, A.FacingRightWalkingRightLegMoving),
        [A.FacingRightSpewingChozoOrbsLeftFootForward] = Spew(A.FacingRightSpewingChozoOrbsLeftFootForward, A.BombChozoOrbBurst, 3, A.FacingRightSpewingChozoOrbsLeftFootForwardEnd),
        [A.FacingRightSpewingChozoOrbsRightFootForward] = Spew(A.FacingRightSpewingChozoOrbsRightFootForward, A.BombChozoOrbBurst, 3, A.FacingRightSpewingChozoOrbsRightFootForwardEnd),
        [A.FacingRightSonicBoomsLeftFootForward] = SonicBooms(A.FacingRightSonicBoomsLeftFootForward, A.FacingRightSonicBoomsLeftFootForwardThrowing, A.FacingRightSonicBoomsLeftFootForwardEnd),
        [A.FacingRightSonicBoomsRightFootForward] = SonicBooms(A.FacingRightSonicBoomsRightFootForward, A.FacingRightSonicBoomsRightFootForwardThrowing, A.FacingRightSonicBoomsRightFootForwardEnd),
        [A.FacingRightExplosiveSwipeLeftFootForward] = new([Whole(A.FacingRightExplosiveSwipeLeftFootForward)], AnimationEnd.Holds),
        [A.FacingRightExplosiveSwipeRightFootForward] = new([Whole(A.FacingRightExplosiveSwipeRightFootForward)], AnimationEnd.Holds),
        [A.FacingRightJumpingForward] = Jump(A.FacingRightJumpingForward, A.FacingRightRising, A.FacingRightFalling, A.FacingRightFallingPose),
        [A.FacingRightFalling] = Fall(A.FacingRightFalling, A.FacingRightFallingPose),
        [A.FacingRightLanding] = ThenWalking(A.FacingRightLanding, A.FacingRightWalkingRightLegMoving, A.FacingRightWalkingLeftLegMoving),
        [A.FacingRightJumpingBackwardToRightFoot] = Jump(A.FacingRightJumpingBackwardToRightFoot, A.FacingRightRising, A.FacingRightFallingBackwardToRightFoot, A.FacingRightFallingPose),
        [A.FacingRightFallingBackwardToRightFoot] = Fall(A.FacingRightFallingBackwardToRightFoot, A.FacingRightFallingPose),
        [A.FacingRightLandingBackwardOnRightFoot] = ThenWalking(A.FacingRightLandingBackwardOnRightFoot, A.FacingRightWalkingLeftLegMoving, A.FacingRightWalkingRightLegMoving),
        [A.FacingRightJumpingBackwardToLeftFoot] = Jump(A.FacingRightJumpingBackwardToLeftFoot, A.FacingRightRising, A.FacingRightFallingBackwardToLeftFoot, A.FacingRightFallingPose),
        [A.FacingRightFallingBackwardToLeftFoot] = Fall(A.FacingRightFallingBackwardToLeftFoot, A.FacingRightFallingPose),
        [A.FacingRightLandingBackwardOnLeftFoot] = ThenWalking(A.FacingRightLandingBackwardOnLeftFoot, A.FacingRightWalkingRightLegMoving, A.FacingRightWalkingLeftLegMoving),
        [A.FacelessTurningRight] = ThenWalking(A.FacelessTurningRight, A.FacingRightFacelessWalkingLeftLegMoving, A.FacingRightFacelessWalkingRightLegMoving),
        [A.FacingRightFacelessWalkingLeftLegMoving] = Walking(A.FacingRightFacelessWalkingLeftLegMoving, A.FacingRightFacelessWalkingRightLegMoving),
        [A.FacingRightFacelessWalkingRightLegMoving] = Walking(A.FacingRightFacelessWalkingRightLegMoving, A.FacingRightFacelessWalkingLeftLegMoving),
        [A.GoldenTorizoDormant] = new([Whole(A.GoldenTorizoDormant)], AnimationEnd.Holds),
        [A.GoldenTorizoAwakening] = Animation<A>.LeadingInto([Whole(A.GoldenTorizoAwakening), Whole(A.GoldenTorizoSettling), Repeat(A.SeatedEyesLightingAgain, 2), Whole(A.GoldenTorizoStandingUp), Repeat(A.StandingColourFade, 16), Whole(A.GoldenTorizoFightStart)], Walk(A.GoldenWalkingLeftLeftLegMoving, A.GoldenWalkingLeftRightLegMoving)),
        [A.GoldenFacingLeftSpewingChozoOrbsRightFootForward] = Spew(A.GoldenFacingLeftSpewingChozoOrbsRightFootForward, A.GoldenChozoOrbBurst, 6, A.GoldenFacingLeftSpewingChozoOrbsRightFootForwardEnd),
        [A.GoldenFacingLeftSpewingChozoOrbsLeftFootForward] = Spew(A.GoldenFacingLeftSpewingChozoOrbsLeftFootForward, A.GoldenChozoOrbBurst, 6, A.GoldenFacingLeftSpewingChozoOrbsLeftFootForwardEnd),
        [A.GoldenFacingLeftSonicBoomsRightFootForward] = SonicBooms(A.GoldenFacingLeftSonicBoomsRightFootForward, A.GoldenFacingLeftSonicBoomsRightFootForwardThrowing, A.GoldenFacingLeftSonicBoomsRightFootForwardEnd),
        [A.GoldenFacingLeftSonicBoomsLeftFootForward] = SonicBooms(A.GoldenFacingLeftSonicBoomsLeftFootForward, A.GoldenFacingLeftSonicBoomsLeftFootForwardThrowing, A.GoldenFacingLeftSonicBoomsLeftFootForwardEnd),
        [A.GoldenFacingRightSpewingChozoOrbsLeftFootForward] = Spew(A.GoldenFacingRightSpewingChozoOrbsLeftFootForward, A.GoldenChozoOrbBurst, 6, A.GoldenFacingRightSpewingChozoOrbsLeftFootForwardEnd),
        [A.GoldenFacingRightSpewingChozoOrbsRightFootForward] = Spew(A.GoldenFacingRightSpewingChozoOrbsRightFootForward, A.GoldenChozoOrbBurst, 6, A.GoldenFacingRightSpewingChozoOrbsRightFootForwardEnd),
        [A.GoldenFacingRightSonicBoomsLeftFootForward] = SonicBooms(A.GoldenFacingRightSonicBoomsLeftFootForward, A.GoldenFacingRightSonicBoomsLeftFootForwardThrowing, A.GoldenFacingRightSonicBoomsLeftFootForwardEnd),
        [A.GoldenFacingRightSonicBoomsRightFootForward] = SonicBooms(A.GoldenFacingRightSonicBoomsRightFootForward, A.GoldenFacingRightSonicBoomsRightFootForwardThrowing, A.GoldenFacingRightSonicBoomsRightFootForwardEnd),
        [A.GoldenFacingLeftLandedBackwardOnLeftFoot] = ThenWalking(A.GoldenFacingLeftLandedBackwardOnLeftFoot, A.GoldenWalkingLeftRightLegMoving, A.GoldenWalkingLeftLeftLegMoving),
        [A.GoldenFacingLeftLandedBackwardOnRightFoot] = ThenWalking(A.GoldenFacingLeftLandedBackwardOnRightFoot, A.GoldenWalkingLeftLeftLegMoving, A.GoldenWalkingLeftRightLegMoving),
        [A.GoldenFacingRightLandedBackwardOnRightFoot] = ThenWalking(A.GoldenFacingRightLandedBackwardOnRightFoot, A.GoldenWalkingRightLeftLegMoving, A.GoldenWalkingRightRightLegMoving),
        [A.GoldenFacingRightLandedBackwardOnLeftFoot] = ThenWalking(A.GoldenFacingRightLandedBackwardOnLeftFoot, A.GoldenWalkingRightRightLegMoving, A.GoldenWalkingRightLeftLegMoving),
        [A.GoldenFacingLeftCatchingSuperMissileLeftLegForward] = ThenWalking(A.GoldenFacingLeftCatchingSuperMissileLeftLegForward, A.GoldenWalkingLeftRightLegMoving, A.GoldenWalkingLeftLeftLegMoving),
        [A.GoldenFacingLeftCatchingSuperMissileRightLegForward] = ThenWalking(A.GoldenFacingLeftCatchingSuperMissileRightLegForward, A.GoldenWalkingLeftLeftLegMoving, A.GoldenWalkingLeftRightLegMoving),
        [A.GoldenFacingRightCatchingSuperMissileRightLegForward] = ThenWalking(A.GoldenFacingRightCatchingSuperMissileRightLegForward, A.GoldenWalkingRightLeftLegMoving, A.GoldenWalkingRightRightLegMoving),
        [A.GoldenFacingRightCatchingSuperMissileLeftLegForward] = ThenWalking(A.GoldenFacingRightCatchingSuperMissileLeftLegForward, A.GoldenWalkingRightRightLegMoving, A.GoldenWalkingRightLeftLegMoving),
        [A.GoldenFacingLeftSittingDownAttack] = ThenWalking(A.GoldenFacingLeftSittingDownAttack, A.GoldenWalkingLeftLeftLegMoving, A.GoldenWalkingLeftRightLegMoving),
        [A.GoldenFacingRightSittingDownAttack] = ThenWalking(A.GoldenFacingRightSittingDownAttack, A.GoldenWalkingRightRightLegMoving, A.GoldenWalkingRightLeftLegMoving),
        [A.GoldenReleasingEggs] = new([Whole(A.GoldenReleasingEggs), Repeat(A.GoldenEggLaying, 6), Whole(A.GoldenChestClosing)], AnimationEnd.Holds),
        [A.GoldenEyeBeamAttack] = new([Whole(A.GoldenEyeBeamAttack), Repeat(A.GoldenEyeBeamFiring, 4), Whole(A.GoldenEyeBeamPause), Repeat(A.GoldenEyeBeamGlow, 2), Whole(A.GoldenEyeBeamEnd)], AnimationEnd.Holds),
        [A.GoldenDodgeTurningLeft] = ThenWalking(A.GoldenDodgeTurningLeft, A.GoldenWalkingLeftRightLegMoving, A.GoldenWalkingLeftLeftLegMoving),
        [A.GoldenTurningLeft] = ThenWalking(A.GoldenTurningLeft, A.GoldenWalkingLeftRightLegMoving, A.GoldenWalkingLeftLeftLegMoving),
        [A.GoldenWalkingLeftLeftLegMoving] = Walking(A.GoldenWalkingLeftLeftLegMoving, A.GoldenWalkingLeftRightLegMoving),
        [A.GoldenDodgeTurningRight] = ThenWalking(A.GoldenDodgeTurningRight, A.GoldenWalkingRightLeftLegMoving, A.GoldenWalkingRightRightLegMoving),
        [A.GoldenTurningRight] = ThenWalking(A.GoldenTurningRight, A.GoldenWalkingRightLeftLegMoving, A.GoldenWalkingRightRightLegMoving),
        [A.GoldenWalkingRightRightLegMoving] = Walking(A.GoldenWalkingRightRightLegMoving, A.GoldenWalkingRightLeftLegMoving),
        [A.FacingLeftWalkingRightLegMoving] = Walking(A.FacingLeftWalkingRightLegMoving, A.FacingLeftWalkingLeftLegMoving),
        [A.FacingLeftWalkingLeftLegMoving] = Walking(A.FacingLeftWalkingLeftLegMoving, A.FacingLeftWalkingRightLegMoving),
        [A.FacingRightWalkingLeftLegMoving] = Walking(A.FacingRightWalkingLeftLegMoving, A.FacingRightWalkingRightLegMoving),
        [A.FacingRightWalkingRightLegMoving] = Walking(A.FacingRightWalkingRightLegMoving, A.FacingRightWalkingLeftLegMoving),
        [A.GoldenWalkingLeftRightLegMoving] = Walking(A.GoldenWalkingLeftRightLegMoving, A.GoldenWalkingLeftLeftLegMoving),
        [A.GoldenWalkingRightLeftLegMoving] = Walking(A.GoldenWalkingRightLeftLegMoving, A.GoldenWalkingRightRightLegMoving),
    }.ToFrozenDictionary();

    /// <summary>One play of every step of <paramref name="segment"/>.</summary>
    private static AnimationPhase<TorizoAnimation> Whole(TorizoAnimation segment) => Repeat(segment, 1);

    /// <summary><paramref name="plays"/> plays of every step of <paramref name="segment"/>.</summary>
    private static AnimationPhase<TorizoAnimation> Repeat(TorizoAnimation segment, int plays) =>
        new(segment, 0, Segments.Beats(segment).Count, plays);

    /// <summary>A walk's two steps, <paramref name="first"/> then <paramref name="second"/>, each played once.</summary>
    private static AnimationPhase<TorizoAnimation>[] Walk(TorizoAnimation first, TorizoAnimation second) => [Whole(first), Whole(second)];

    /// <summary>The walk that steps <paramref name="first"/>, then <paramref name="second"/>, and loops.</summary>
    private static Animation<TorizoAnimation> Walking(TorizoAnimation first, TorizoAnimation second) =>
        Animation<TorizoAnimation>.Looping(Walk(first, second));

    /// <summary><paramref name="lead"/> played once, then the walk that steps <paramref name="first"/>, then <paramref name="second"/>, looping (#2331).</summary>
    private static Animation<TorizoAnimation> ThenWalking(TorizoAnimation lead, TorizoAnimation first, TorizoAnimation second) =>
        Animation<TorizoAnimation>.LeadingInto([Whole(lead)], Walk(first, second));

    /// <summary>A spew: its lead, <paramref name="plays"/> bursts, then its end, holding on its last frame.</summary>
    private static Animation<TorizoAnimation> Spew(TorizoAnimation lead, TorizoAnimation burst, int plays, TorizoAnimation end) =>
        new([Whole(lead), Repeat(burst, plays), Whole(end)], AnimationEnd.Holds);

    /// <summary>A volley of sonic booms: its lead, four throws, then its end, holding on its last frame.</summary>
    private static Animation<TorizoAnimation> SonicBooms(TorizoAnimation lead, TorizoAnimation throwing, TorizoAnimation end) =>
        new([Whole(lead), Repeat(throwing, 4), Whole(end)], AnimationEnd.Holds);

    /// <summary>A jump: its lead and its rise, then its fall's start and the pose the fall loops on.</summary>
    private static Animation<TorizoAnimation> Jump(TorizoAnimation lead, TorizoAnimation rising, TorizoAnimation fallStart, TorizoAnimation pose) =>
        Animation<TorizoAnimation>.LeadingInto([Whole(lead), Whole(rising), Whole(fallStart)], [Whole(pose)]);

    /// <summary>A fall: its start, then the pose it loops on.</summary>
    private static Animation<TorizoAnimation> Fall(TorizoAnimation start, TorizoAnimation pose) =>
        Animation<TorizoAnimation>.LeadingInto([Whole(start)], [Whole(pose)]);

    /// <summary>How <paramref name="animation"/> plays.</summary>
    internal static Animation<TorizoAnimation> StartingWith(TorizoAnimation animation) => StartingAt[animation];

    /// <summary>A playback of <paramref name="animation"/>, before its first play.</summary>
    internal static AnimationPlayback<TorizoAnimation> Start(TorizoAnimation animation) => new(StartingWith(animation), AnimationPlace.StartOfAnimation);

    /// <summary>A cue's continuation into <paramref name="animation"/>, from its start.</summary>
    internal static AnimationContinuation<TorizoAnimation> ContinueWith(TorizoAnimation animation) => new(StartingWith(animation), AnimationPlace.StartOfAnimation);

    /// <summary>A cue's continuation where <paramref name="playback"/> stood, as a resumed walk or interrupted animation continues there.</summary>
    internal static AnimationContinuation<TorizoAnimation> ContinueAt(AnimationPlayback<TorizoAnimation> playback) =>
        new(Ensure.NotNull(playback.Animation), playback.Place);

    /// <summary>
    /// A replay cue's continuation (<see cref="TorizoCue.Replay"/>): the start of the phase <paramref name="continuation"/>,
    /// the place just past the cue, stands in, so the run the cue is in plays again from its first step.
    /// </summary>
    internal static AnimationContinuation<TorizoAnimation> ReplayPhase(AnimationPlayback<TorizoAnimation> continuation) =>
        new(Ensure.NotNull(continuation.Animation), AnimationPlace.Before(continuation.Place.Phase));
}