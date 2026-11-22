using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// The steps Torizo's body animations are written in, and the runs of steps both Torizos' animations share, each written once
/// over the poses of a side, a stance or an arm (#2411).
/// </summary>
internal static partial class TorizoAnimationSteps
{
    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    internal static AnimationBeat<EnemyFrameId, TorizoCue> Show(EnemyFrameId frame, int duration) =>
        new AnimationBeat<EnemyFrameId, TorizoCue>.Frame(frame, duration);

    /// <summary>Raises <paramref name="cue"/>.</summary>
    internal static AnimationBeat<EnemyFrameId, TorizoCue> Cue(TorizoCue cue) =>
        new AnimationBeat<EnemyFrameId, TorizoCue>.Event(cue);

    /// <summary>
    /// Marks the moment <paramref name="kind"/> names (#2301, #2411): the owner reacts to it, and plays nothing, as a looping
    /// run's end tells it the Torizo's state may have changed.
    /// </summary>
    internal static AnimationBeat<EnemyFrameId, TorizoCue> Mark(TorizoCueKind kind) =>
        new AnimationBeat<EnemyFrameId, TorizoCue>.Marker(new TorizoCue.Signal(kind));

    /// <summary>Shows artwork look <paramref name="look"/> under the body's poses.</summary>
    internal static AnimationBeat<EnemyFrameId, TorizoCue> ShowArtwork(TorizoArtworkLook look) =>
        Cue(new TorizoCue.ShowArtwork(look));

    /// <summary>Waits <paramref name="updates"/> updates on the current frame.</summary>
    internal static AnimationBeat<EnemyFrameId, TorizoCue> Wait(int updates) =>
        new AnimationBeat<EnemyFrameId, TorizoCue>.Pause(updates);

    /// <summary>Stands the Torizo up by a Torizo facing right's <paramref name="move"/>, mirrored for <paramref name="side"/>.</summary>
    internal static AnimationBeat<EnemyFrameId, TorizoCue> StandUp(TorizoSide side, Offset16<RoomPixels> move) =>
        Cue(new TorizoCue.Posture(TorizoPostureKind.StandUp, Displacement: side.Forward(move)));

    /// <summary>Sits the Torizo down by a Torizo facing right's <paramref name="move"/>, mirrored for <paramref name="side"/>.</summary>
    internal static AnimationBeat<EnemyFrameId, TorizoCue> SitDown(TorizoSide side, Offset16<RoomPixels> move) =>
        Cue(new TorizoCue.Posture(TorizoPostureKind.SitDown, Displacement: side.Forward(move)));

    /// <summary>
    /// A seated Torizo facing left lighting its eyes up as it wakes: its eyes dark, glowing faintly, brightly and fully in
    /// turn, the seat held between them.
    /// </summary>
    internal static AnimationBeat<EnemyFrameId, TorizoCue>[] SeatedEyesLightingUp =>
    [
        Show(TorizoSide.Left.Seated, 48),
        ShowArtwork(TorizoArtworkLook.EyesDark),
        Show(TorizoSide.Left.Seated, 32),
        ShowArtwork(TorizoArtworkLook.EyesGlowingFaintly),
        Show(TorizoSide.Left.Seated, 16),
        ShowArtwork(TorizoArtworkLook.EyesGlowingBrightly),
        Show(TorizoSide.Left.Seated, 8),
        ShowArtwork(TorizoArtworkLook.EyesGlowingFully),
        Show(TorizoSide.Left.Seated, 32),
    ];

    /// <summary>
    /// A Torizo facing <paramref name="side"/>'s way standing up from its seat: each pose from the seat up to its stepping
    /// stance shows for its updates, and then the posture's move stands it into the next.
    /// </summary>
    internal static AnimationBeat<EnemyFrameId, TorizoCue>[] StandingUp(TorizoSide side, int seated, int rising, int crouched, int halfRisen,
        int nearlyUpright, int stepping) =>
    [
        Show(side.Seated, seated), StandUp(side, TorizoPostureDefinitions.SeatedToRising),
        Show(side.RisingFromSeat, rising), StandUp(side, TorizoPostureDefinitions.RisingToCrouched),
        Show(side.Crouched, crouched), StandUp(side, TorizoPostureDefinitions.CrouchedToHalfRisen),
        Show(side.HalfRisen, halfRisen), StandUp(side, TorizoPostureDefinitions.HalfRisenToNearlyUpright),
        Show(side.NearlyUpright, nearlyUpright), StandUp(side, TorizoPostureDefinitions.NearlyUprightToStepping),
        Show(side.UprightStepping, stepping), StandUp(side, TorizoPostureDefinitions.SteppingToWideStance),
    ];

    /// <summary>
    /// A Torizo facing <paramref name="side"/>'s way sitting down to its seat from its wide stance: each pose shows for its
    /// updates, and then the posture's move sits it into the next lower, the last three as <see cref="SittingDownFromHalfRisen"/>.
    /// </summary>
    internal static AnimationBeat<EnemyFrameId, TorizoCue>[] SittingDown(TorizoSide side, int wideStance, int stepping, int nearlyUpright,
        int halfRisen, int crouched, int rising) =>
    [
        Show(side.UprightWideStance, wideStance), SitDown(side, TorizoPostureDefinitions.SteppingToWideStance),
        Show(side.UprightStepping, stepping), SitDown(side, TorizoPostureDefinitions.NearlyUprightToStepping),
        Show(side.NearlyUpright, nearlyUpright), SitDown(side, TorizoPostureDefinitions.HalfRisenToNearlyUpright),
        .. SittingDownFromHalfRisen(side, halfRisen, crouched, rising),
    ];

    /// <summary>
    /// A Torizo facing <paramref name="side"/>'s way sitting down to its seat from half risen: its half-risen, crouched and
    /// rising poses each show for their updates, and then the posture's move sits it into the next lower.
    /// </summary>
    internal static AnimationBeat<EnemyFrameId, TorizoCue>[] SittingDownFromHalfRisen(TorizoSide side, int halfRisen, int crouched, int rising) =>
    [
        Show(side.HalfRisen, halfRisen), SitDown(side, TorizoPostureDefinitions.CrouchedToHalfRisen),
        Show(side.Crouched, crouched), SitDown(side, TorizoPostureDefinitions.RisingToCrouched),
        Show(side.RisingFromSeat, rising), SitDown(side, TorizoPostureDefinitions.SeatedToRising),
    ];

    /// <summary>Moves a Torizo walking as <paramref name="walker"/> <paramref name="pace"/> pixels forward the way <paramref name="side"/> faces.</summary>
    internal static AnimationBeat<EnemyFrameId, TorizoCue> Walk(TorizoWalker walker, TorizoSide side, int pace) =>
        Cue(new TorizoCue.Walk(walker, Velocity: side.Forward(pace)));

    /// <summary>Ends an attack chosen on landing: the Torizo starts walking and walks on in <paramref name="walk"/>.</summary>
    internal static AnimationBeat<EnemyFrameId, TorizoCue>[] WalkingOn(TorizoAnimation walk) =>
    [
        Cue(new TorizoCue.Signal(TorizoCueKind.StartWalking)),
        Cue(new TorizoCue.WalkOn(Walking: walk)),
    ];

    /// <summary>
    /// One arm's sonic boom (#2411): from the lowered head the arm lifts through its poses, each shown
    /// <paramref name="poseUpdates"/> updates, overhead, snaps back down, each pose <paramref name="snapUpdates"/> updates,
    /// throwing the boom <paramref name="spawn"/> spawns as it passes level, and rests lowered <paramref name="loweredUpdates"/>
    /// updates.
    /// </summary>
    internal static AnimationBeat<EnemyFrameId, TorizoCue>[] SonicBoomThrow(
        EnemyFrameId headLowered, TorizoArmPoses arm, int poseUpdates, int snapUpdates, int loweredUpdates, TorizoCueKind spawn) =>
    [
        Show(headLowered, poseUpdates),
        Show(arm.Down, poseUpdates),
        Show(arm.Lifting, poseUpdates),
        Show(arm.Level, poseUpdates),
        Show(arm.Raised, poseUpdates),
        Show(arm.Overhead, poseUpdates),
        Show(arm.Raised, snapUpdates),
        Show(arm.Level, snapUpdates),
        Cue(new TorizoCue.Signal(spawn)),
        Show(arm.Lifting, snapUpdates),
        Show(arm.Down, loweredUpdates),
    ];
}
