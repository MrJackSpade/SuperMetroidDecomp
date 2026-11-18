using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// The steps Torizo's body animations are written in, and the runs of steps both Torizos' animations share, each written once
/// over the poses of a side, a stance or an arm (#2411).
/// </summary>
internal static class TorizoAnimationSteps
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

    /// <summary>Moves a walking Torizo of <paramref name="kind"/> <paramref name="pace"/> pixels forward the way <paramref name="side"/> faces.</summary>
    internal static AnimationBeat<EnemyFrameId, TorizoCue> Walk(TorizoWalkKind kind, TorizoSide side, int pace) =>
        Cue(new TorizoCue.Walk(kind, Velocity: side.Forward(pace)));

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
