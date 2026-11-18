using SuperMetroid.Core.Animation;
using A = SuperMetroid.Core.Rooms.Doors.EyeDoorAction;
using F = SuperMetroid.Core.Rooms.Doors.EyeDoorObjectFrame;
using S = SuperMetroid.Core.Rooms.Doors.EyeDoorSegment;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The eye doors' authored data (#1554): how near Samus must come, the hits the eye takes, and the animations of the eye
/// and of the body and bottom it watches over (#2480), each action an event raised just before the picture after it
/// draws. Both facings play the same animations, mirrored; the right-facing body and bottom pulse faster.
/// </summary>
internal static class EyeDoorSequenceDefinitions
{
    /// <summary>The sound each hit on the eye makes.</summary>
    internal static readonly SoundEffectId HitSound = EnemySound.EnemyKilled;

    /// <summary>How crowded the sound queue may be for the hit sound to join it.</summary>
    internal static readonly SoundRequestPriority HitSoundPriority = SoundRequestPriority.Normal;

    /// <summary>The missile hits the eye takes before the door opens; a Super Missile destroys it at once (#1907).</summary>
    internal const int HitThreshold = 3;

    /// <summary>How many times the destroyed eye flashes to the blue cap before it turns into it.</summary>
    internal const int OpeningFlashes = 10;

    /// <summary>How near Samus must stand, across and up or down, to open the closed eye or keep it watching.</summary>
    internal static Extent16<RoomBlocks> WakeReach { get; } = new(6, 4);

    /// <summary>How near Samus must stand, across and up or down, for the open eye to blink.</summary>
    internal static Extent16<RoomBlocks> InFrontReach { get; } = new(1, 4);

    /// <summary>How near Samus must stand, across and up or down, to make the body and bottom pulse.</summary>
    internal static Extent16<RoomBlocks> PulseReach { get; } = new(6, 16);

    /// <summary>Every run's pictures, each for its updates, and the actions taken just before the picture after each draws.</summary>
    internal static AnimationSegments<EyeDoorSegment, EyeDoorObjectFrame, EyeDoorAction> Segments { get; } = new(
        new Dictionary<EyeDoorSegment, AnimationBeat<EyeDoorObjectFrame, EyeDoorAction>[]>
        {
            [S.Closed] = [Show(F.ClosedEye, 4)],
            [S.Opening] = [Show(F.HalfOpenEye, 8)],
            [S.Watching] =
            [
                Show(F.OpenEye, 64),
                Act(A.Shoot), Show(F.OpenEye, 32),
                Act(A.Shoot), Show(F.OpenEye, 32),
                Act(A.Shoot), Show(F.OpenEye, 64),
                Show(F.HalfOpenEye, 6),
                Show(F.ClosedEye, 96),
                Show(F.HalfOpenEye, 6),
            ],
            [S.Blinking] = [Show(F.ClosedEye, 4)],
            [S.Flinching] =
            [
                Show(F.FlinchingEye, 2),
                Show(F.OpenSolidEye, 2),
                Act(A.SpawnTwoSmoke), Show(F.FlinchingEye, 2),
                Show(F.OpenSolidEye, 2),
                Show(F.FlinchingEye, 2),
                Act(A.SpawnTwoSmoke), Show(F.OpenSolidEye, 2),
                Show(F.HalfOpenEye, 4),
                Show(F.ClosedEye, 8),
                Act(A.SpawnSweat), Show(F.ClosedEye, 56),
                Show(F.HalfOpenEye, 4),
                Show(F.OpenSolidEye, 4),
            ],
            [S.OpeningFlash] = [Show(F.OpeningClear, 3), Show(F.BlueCap, 4)],
        });

    /// <summary>
    /// The body's and bottom's runs of poses, each for its updates (#2481): a part only draws, so its animations raise
    /// nothing.
    /// </summary>
    internal static AnimationSegments<EyeDoorPartSegment, EyeDoorPartPose, NoAnimationCue> PartSegments { get; } = new(
        new Dictionary<EyeDoorPartSegment, AnimationBeat<EyeDoorPartPose, NoAnimationCue>[]>
        {
            [EyeDoorPartSegment.Still] = [Pose(EyeDoorPartPose.Resting, 8)],
            [EyeDoorPartSegment.LeftPulse] =
            [
                Pose(EyeDoorPartPose.Resting, 8), Pose(EyeDoorPartPose.Swelling, 8), Pose(EyeDoorPartPose.Swollen, 8),
                Pose(EyeDoorPartPose.Swelling, 8),
            ],
            [EyeDoorPartSegment.RightPulse] =
            [
                Pose(EyeDoorPartPose.Resting, 6), Pose(EyeDoorPartPose.Swelling, 6), Pose(EyeDoorPartPose.Swollen, 6),
                Pose(EyeDoorPartPose.Swelling, 6),
            ],
        });

    /// <summary>The closed eye, for four updates before it looks for Samus again.</summary>
    internal static Animation<EyeDoorSegment> Closed { get; } = Once(S.Closed);

    /// <summary>The eye opening as Samus nears.</summary>
    internal static Animation<EyeDoorSegment> Opening { get; } = Once(S.Opening);

    /// <summary>The open eye watching Samus.</summary>
    internal static Animation<EyeDoorSegment> Watching { get; } = Once(S.Watching);

    /// <summary>The eye blinking while Samus stands right in front of it.</summary>
    internal static Animation<EyeDoorSegment> Blinking { get; } = Once(S.Blinking);

    /// <summary>The eye flinching from a hit short of the threshold.</summary>
    internal static Animation<EyeDoorSegment> Flinching { get; } = Once(S.Flinching);

    /// <summary>The destroyed eye flashing to the blue cap, <see cref="OpeningFlashes"/> times, before it turns into it.</summary>
    internal static Animation<EyeDoorSegment> Destroyed { get; } = new([Segments.Repeated(S.OpeningFlash, OpeningFlashes)], AnimationEnd.Stops);

    /// <summary>The still body or bottom, before it looks for Samus again.</summary>
    internal static Animation<EyeDoorPartSegment> Still { get; } = Once(EyeDoorPartSegment.Still);

    /// <summary>Each facing's body and bottom pulsing while Samus is near.</summary>
    internal static IReadOnlyDictionary<EyeDoorOrientation, Animation<EyeDoorPartSegment>> Pulse { get; } =
        new Dictionary<EyeDoorOrientation, Animation<EyeDoorPartSegment>>
    {
        [EyeDoorOrientation.Left] = Once(EyeDoorPartSegment.LeftPulse),
        [EyeDoorOrientation.Right] = Once(EyeDoorPartSegment.RightPulse),
    };

    /// <summary>The whole of <paramref name="segment"/>, once.</summary>
    private static Animation<EyeDoorSegment> Once(EyeDoorSegment segment) => new([Segments.Whole(segment)], AnimationEnd.Stops);

    /// <summary>The whole of the part's <paramref name="segment"/>, once.</summary>
    private static Animation<EyeDoorPartSegment> Once(EyeDoorPartSegment segment) => new([PartSegments.Whole(segment)], AnimationEnd.Stops);

    /// <summary>Draws the part in <paramref name="pose"/> for <paramref name="updates"/> updates.</summary>
    private static AnimationBeat<EyeDoorPartPose, NoAnimationCue> Pose(EyeDoorPartPose pose, int updates) =>
        new AnimationBeat<EyeDoorPartPose, NoAnimationCue>.Frame(pose, updates);

    /// <summary>Draws <paramref name="frame"/> for <paramref name="updates"/> updates.</summary>
    private static AnimationBeat<EyeDoorObjectFrame, EyeDoorAction> Show(EyeDoorObjectFrame frame, int updates) =>
        new AnimationBeat<EyeDoorObjectFrame, EyeDoorAction>.Frame(frame, updates);

    /// <summary>Takes <paramref name="action"/> just before the picture after it draws.</summary>
    private static AnimationBeat<EyeDoorObjectFrame, EyeDoorAction> Act(EyeDoorAction action) =>
        new AnimationBeat<EyeDoorObjectFrame, EyeDoorAction>.Event(action);
}
