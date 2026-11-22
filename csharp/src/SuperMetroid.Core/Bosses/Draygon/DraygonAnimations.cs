using SuperMetroid.Core.Animation;
using F = SuperMetroid.Core.Enemies.Artwork.EnemyFrameId;
using SuperMetroid.Core.Enemies.Artwork;
using P = SuperMetroid.Core.Bosses.Draygon.DraygonSegment;
using SuperMetroid.Core.Enemies.EffectActors;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// Draygon's typed animations (#641, #1600): each body, eye, tail and arms segment's beats, the animation starting at
/// each, which plays each of its segments whole (#2296), and his tail's final whips, which play its whip four times and
/// which his grab starts by name (#2399). The idle eye shows each of its pictures for one beat of the picture's whole
/// duration, and his body shows the eye's picture as it changes (#2573). His tail's lashes are written
/// once over a facing's poses (#2465): his whips and his fake whip lash out shifting his body's picture, his flail lashes
/// with it still, and every lash recoils and swings back the same way.
/// </summary>
internal static partial class DraygonAnimations
{
    /// <summary>Every body, eye, tail and arms segment's beats.</summary>
    internal static AnimationSegments<DraygonSegment, EnemyFrameId, DraygonCue> Segments { get; } = new(
        new Dictionary<DraygonSegment, AnimationBeat<EnemyFrameId, DraygonCue>[]>
    {
        [P.Rest] =
        [
        ],
        [P.BodyFacingLeftReset] =
        [
            Cue(new DraygonCue.StartPartAnimations(P.BodyFacingLeftIdle, P.EyeFacingLeftIdle, P.TailFacingLeftIdle, P.ArmsFacingLeftIdle)),
            Cue(new DraygonCue.TrackWithEye(HorizontalDirection.Left)),
            Show(F.DraygonBodyFacingLeftJawShut, 1),
        ],
        [P.BodyFacingRightReset] =
        [
            Cue(new DraygonCue.StartPartAnimations(P.BodyFacingRightIdle, P.EyeFacingRightIdle, P.TailFacingRightIdle, P.ArmsFacingRightIdle)),
            Cue(new DraygonCue.TrackWithEye(HorizontalDirection.Right)),
            Show(F.DraygonBodyFacingRightJawShut, 1),
        ],
        [P.ArmsFacingLeftIdle] = ArmsIdle(DraygonFacingPoses.Left),
        [P.ArmsFacingLeftNearSwoopApex] = ArmsNearSwoopApex(DraygonFacingPoses.Left),
        [P.ArmsFacingLeftGrab] = ArmsGrab(DraygonFacingPoses.Left),
        [P.BodyFacingLeftDying] = BodyDying(DraygonFacingPoses.Left),
        [P.BodyFacingLeftIdle] =
        [
            Cue(new DraygonCue.TrackWithEye(HorizontalDirection.Left)),
            Show(F.DraygonBodyFacingLeftJawShut, 1),
        ],
        [P.BodyDyingCry] =
        [
            Cue(new DraygonCue.QueueSound(DraygonSoundDefinitions.DyingCry)),
            Cue(DraygonCue.SetBodyIntangible.Instance),
        ],
        [P.BodyDyingExplosions] = DyingExplosions(12),
        [P.BodyDyingParalysis] =
        [
            Wait(1),
            Cue(DraygonCue.ParalyseTailAndArms.Instance),
        ],
        [P.BodyDyingExplosionLoop] = DyingExplosions(16),
        [P.Delete] =
        [
        ],
        [P.BodyFacingLeftFireGoop] = BodyFireGoop(DraygonFacingPoses.Left),
        [P.BodyFacingLeftRoar] = BodyRoar(DraygonFacingPoses.Left),
        [P.EyeFacingLeftIdle] = EyeIdle(DraygonFacingPoses.Left),
        [P.EyeFacingLeftDying] = EyeDying(DraygonFacingPoses.Left),
        [P.EyeFacingLeftDyingStare] = EyeDyingStare(DraygonFacingPoses.Left),
        [P.EyeFacingLeftDead] = EyeDead(DraygonFacingPoses.Left),
        [P.EyeFacingLeftLookingLeft] =
        [
            Show(F.DraygonEyeFacingLeftOpen, 1),
        ],
        [P.EyeFacingLeftLookingRight] =
        [
            Show(F.DraygonEyeFacingLeftBloodshot, 1),
        ],
        [P.EyeFacingLeftLookingUp] =
        [
            Show(F.DraygonEyeFacingLeftNarrowing, 1),
        ],
        [P.EyeFacingLeftLookingDown] =
        [
            Show(F.DraygonEyeFacingLeftClosing, 1),
        ],
        [P.TailFacingLeftIdle] = TailIdle(DraygonTailPoses.Left),
        [P.TailFacingLeftInitialFakeWhip] =
        [
            .. ShiftingLash(DraygonTailPoses.Left, FakeWhipTipOutUpdates),
            .. Recoil(DraygonTailPoses.Left, FakeWhipRecoilUpdates),
        ],
        [P.TailFacingLeftReleaseSamus] =
        [
            Cue(new DraygonCue.SetBodyPhase(DraygonPhase.ReleaseSamus)),
        ],
        [P.TailFacingLeftWhip] =
        [
            .. ShiftingLash(DraygonTailPoses.Left, LashTipOutUpdates),
            Cue(DraygonCue.TailWhipHit.Instance),
            Cue(new DraygonCue.QueueSound(DraygonSoundDefinitions.Explosion)),
            .. Recoil(DraygonTailPoses.Left, LashRecoilUpdates),
        ],
        [P.TailFacingLeftFlail] =
        [
            .. Lash(DraygonTailPoses.Left),
            Cue(new DraygonCue.QueueSound(DraygonSoundDefinitions.Explosion)),
            .. Recoil(DraygonTailPoses.Left, LashRecoilUpdates),
        ],
        [P.ArmsFacingRightIdle] = ArmsIdle(DraygonFacingPoses.Right),
        [P.ArmsFacingRightNearSwoopApex] = ArmsNearSwoopApex(DraygonFacingPoses.Right),
        [P.ArmsFacingRightGrab] = ArmsGrab(DraygonFacingPoses.Right),
        [P.BodyFacingRightDying] = BodyDying(DraygonFacingPoses.Right),
        [P.BodyFacingRightIdle] =
        [
            Cue(new DraygonCue.TrackWithEye(HorizontalDirection.Right)),
            Show(F.DraygonBodyFacingRightJawShut, 1),
        ],
        [P.BodyFacingRightFireGoop] = BodyFireGoop(DraygonFacingPoses.Right),
        [P.BodyFacingRightRoar] = BodyRoar(DraygonFacingPoses.Right),
        [P.EyeFacingRightIdle] = EyeIdle(DraygonFacingPoses.Right),
        [P.EyeFacingRightDying] = EyeDying(DraygonFacingPoses.Right),
        [P.EyeFacingRightDyingStare] = EyeDyingStare(DraygonFacingPoses.Right),
        [P.EyeFacingRightDead] = EyeDead(DraygonFacingPoses.Right),
        [P.EyeFacingRightLookingRight] =
        [
            Show(F.DraygonEyeFacingRightOpen, 1),
        ],
        [P.EyeFacingRightLookingLeft] =
        [
            Show(F.DraygonEyeFacingRightBloodshot, 1),
        ],
        [P.EyeFacingRightLookingUp] =
        [
            Show(F.DraygonEyeFacingRightNarrowing, 1),
        ],
        [P.EyeFacingRightLookingDown] =
        [
            Show(F.DraygonEyeFacingRightClosing, 1),
        ],
        [P.TailFacingRightIdle] = TailIdle(DraygonTailPoses.Right),
        [P.TailFacingRightReleaseSamus] =
        [
            Cue(new DraygonCue.SetBodyPhase(DraygonPhase.ReleaseSamus)),
        ],
        [P.TailFacingRightWhip] =
        [
            .. ShiftingLash(DraygonTailPoses.Right, LashTipOutUpdates),
            Cue(DraygonCue.TailWhipHit.Instance),
            Cue(new DraygonCue.QueueSound(DraygonSoundDefinitions.Explosion)),
            .. Recoil(DraygonTailPoses.Right, LashRecoilUpdates),
        ],
        [P.TailFacingRightFlail] =
        [
            .. Lash(DraygonTailPoses.Right),
            Cue(new DraygonCue.QueueSound(DraygonSoundDefinitions.Explosion)),
            .. Recoil(DraygonTailPoses.Right, LashRecoilUpdates),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static Dictionary<DraygonSegment, Animation<DraygonSegment>> StartingAt { get; } = new Dictionary<DraygonSegment, Animation<DraygonSegment>>
    {
        [P.Rest] = new([], AnimationEnd.Holds),
        [P.BodyFacingLeftReset] = new([Segments.Whole(P.BodyFacingLeftReset)], AnimationEnd.Holds),
        [P.BodyFacingRightReset] = new([Segments.Whole(P.BodyFacingRightReset)], AnimationEnd.Holds),
        [P.ArmsFacingLeftIdle] = Animation<DraygonSegment>.Looping([Segments.Whole(P.ArmsFacingLeftIdle)]),
        [P.ArmsFacingLeftNearSwoopApex] = new([Segments.Whole(P.ArmsFacingLeftNearSwoopApex)], AnimationEnd.Holds),
        [P.ArmsFacingLeftGrab] = new([Segments.Whole(P.ArmsFacingLeftGrab)], AnimationEnd.Holds),
        [P.BodyFacingLeftDying] = Animation<DraygonSegment>.LeadingInto([Segments.Whole(P.BodyFacingLeftDying), Segments.Whole(P.BodyDyingCry), Segments.Repeated(P.BodyDyingExplosions, 8), Segments.Whole(P.BodyDyingParalysis)], [Segments.Whole(P.BodyDyingExplosionLoop)]),
        [P.BodyFacingLeftIdle] = new([Segments.Whole(P.BodyFacingLeftIdle)], AnimationEnd.Holds),
        [P.Delete] = new([], AnimationEnd.Stops),
        [P.BodyFacingLeftFireGoop] = new([Segments.Whole(P.BodyFacingLeftFireGoop)], AnimationEnd.Holds),
        [P.BodyFacingLeftRoar] = new([Segments.Whole(P.BodyFacingLeftRoar)], AnimationEnd.Holds),
        [P.EyeFacingLeftIdle] = new([Segments.Whole(P.EyeFacingLeftIdle)], AnimationEnd.Holds),
        [P.EyeFacingLeftDying] = new([Segments.Repeated(P.EyeFacingLeftDying, 4), Segments.Whole(P.EyeFacingLeftDyingStare)], AnimationEnd.Holds),
        [P.EyeFacingLeftDead] = new([Segments.Whole(P.EyeFacingLeftDead)], AnimationEnd.Holds),
        [P.EyeFacingLeftLookingLeft] = new([Segments.Whole(P.EyeFacingLeftLookingLeft)], AnimationEnd.Holds),
        [P.EyeFacingLeftLookingRight] = new([Segments.Whole(P.EyeFacingLeftLookingRight)], AnimationEnd.Holds),
        [P.EyeFacingLeftLookingUp] = new([Segments.Whole(P.EyeFacingLeftLookingUp)], AnimationEnd.Holds),
        [P.EyeFacingLeftLookingDown] = new([Segments.Whole(P.EyeFacingLeftLookingDown)], AnimationEnd.Holds),
        [P.TailFacingLeftIdle] = Animation<DraygonSegment>.Looping([Segments.Whole(P.TailFacingLeftIdle)]),
        [P.TailFacingLeftInitialFakeWhip] = Animation<DraygonSegment>.LeadingInto([Segments.Whole(P.TailFacingLeftInitialFakeWhip)], [Segments.Whole(P.TailFacingLeftIdle)]),
        [P.TailFacingLeftWhip] = Animation<DraygonSegment>.LeadingInto([Segments.Whole(P.TailFacingLeftWhip)], [Segments.Whole(P.TailFacingLeftIdle)]),
        [P.TailFacingLeftFlail] = Animation<DraygonSegment>.LeadingInto([Segments.Whole(P.TailFacingLeftFlail)], [Segments.Whole(P.TailFacingLeftIdle)]),
        [P.ArmsFacingRightIdle] = Animation<DraygonSegment>.Looping([Segments.Whole(P.ArmsFacingRightIdle)]),
        [P.ArmsFacingRightNearSwoopApex] = new([Segments.Whole(P.ArmsFacingRightNearSwoopApex)], AnimationEnd.Holds),
        [P.ArmsFacingRightGrab] = new([Segments.Whole(P.ArmsFacingRightGrab)], AnimationEnd.Holds),
        [P.BodyFacingRightDying] = Animation<DraygonSegment>.LeadingInto([Segments.Whole(P.BodyFacingRightDying), Segments.Whole(P.BodyDyingCry), Segments.Repeated(P.BodyDyingExplosions, 8), Segments.Whole(P.BodyDyingParalysis)], [Segments.Whole(P.BodyDyingExplosionLoop)]),
        [P.BodyFacingRightIdle] = new([Segments.Whole(P.BodyFacingRightIdle)], AnimationEnd.Holds),
        [P.BodyFacingRightFireGoop] = new([Segments.Whole(P.BodyFacingRightFireGoop)], AnimationEnd.Holds),
        [P.BodyFacingRightRoar] = new([Segments.Whole(P.BodyFacingRightRoar)], AnimationEnd.Holds),
        [P.EyeFacingRightIdle] = new([Segments.Whole(P.EyeFacingRightIdle)], AnimationEnd.Holds),
        [P.EyeFacingRightDying] = new([Segments.Repeated(P.EyeFacingRightDying, 4), Segments.Whole(P.EyeFacingRightDyingStare)], AnimationEnd.Holds),
        [P.EyeFacingRightDead] = new([Segments.Whole(P.EyeFacingRightDead)], AnimationEnd.Holds),
        [P.EyeFacingRightLookingRight] = new([Segments.Whole(P.EyeFacingRightLookingRight)], AnimationEnd.Holds),
        [P.EyeFacingRightLookingLeft] = new([Segments.Whole(P.EyeFacingRightLookingLeft)], AnimationEnd.Holds),
        [P.EyeFacingRightLookingUp] = new([Segments.Whole(P.EyeFacingRightLookingUp)], AnimationEnd.Holds),
        [P.EyeFacingRightLookingDown] = new([Segments.Whole(P.EyeFacingRightLookingDown)], AnimationEnd.Holds),
        [P.TailFacingRightIdle] = Animation<DraygonSegment>.Looping([Segments.Whole(P.TailFacingRightIdle)]),
        [P.TailFacingRightWhip] = Animation<DraygonSegment>.LeadingInto([Segments.Whole(P.TailFacingRightWhip)], [Segments.Whole(P.TailFacingRightIdle)]),
        [P.TailFacingRightFlail] = Animation<DraygonSegment>.LeadingInto([Segments.Whole(P.TailFacingRightFlail)], [Segments.Whole(P.TailFacingRightIdle)]),
    };

    /// <summary>
    /// The look his eye takes facing each way toward each straight direction Samus lies nearest (#2557): up, right, down or
    /// left, each facing's own.
    /// </summary>
    internal static IReadOnlyDictionary<DraygonEyeLook, DraygonSegment> EyeLooks { get; } = new Dictionary<DraygonEyeLook, DraygonSegment>
    {
        [new(HorizontalDirection.Left, CompassDirection.Up)] = P.EyeFacingLeftLookingUp,
        [new(HorizontalDirection.Left, CompassDirection.Right)] = P.EyeFacingLeftLookingRight,
        [new(HorizontalDirection.Left, CompassDirection.Down)] = P.EyeFacingLeftLookingDown,
        [new(HorizontalDirection.Left, CompassDirection.Left)] = P.EyeFacingLeftLookingLeft,
        [new(HorizontalDirection.Right, CompassDirection.Up)] = P.EyeFacingRightLookingUp,
        [new(HorizontalDirection.Right, CompassDirection.Right)] = P.EyeFacingRightLookingRight,
        [new(HorizontalDirection.Right, CompassDirection.Down)] = P.EyeFacingRightLookingDown,
        [new(HorizontalDirection.Right, CompassDirection.Left)] = P.EyeFacingRightLookingLeft,
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<DraygonSegment> StartingWith(DraygonSegment segment) => StartingAt[segment];

    /// <summary>How many times his tail whips in its final whips before it releases Samus.</summary>
    private const int FinalWhips = 4;

    /// <summary>
    /// His tail's final whips facing left (#2399): his whip played <see cref="FinalWhips"/> times, the release of Samus, then
    /// his idle tail for good. His grab starts it by this name.
    /// </summary>
    internal static Animation<DraygonSegment> TailFacingLeftFinalWhips { get; } = Animation<DraygonSegment>.LeadingInto(
        [Segments.Repeated(P.TailFacingLeftWhip, FinalWhips), Segments.Whole(P.TailFacingLeftReleaseSamus)], [Segments.Whole(P.TailFacingLeftIdle)]);

    /// <summary>His tail's final whips facing right (#2399), as <see cref="TailFacingLeftFinalWhips"/> plays them facing left.</summary>
    internal static Animation<DraygonSegment> TailFacingRightFinalWhips { get; } = Animation<DraygonSegment>.LeadingInto(
        [Segments.Repeated(P.TailFacingRightWhip, FinalWhips), Segments.Whole(P.TailFacingRightReleaseSamus)], [Segments.Whole(P.TailFacingRightIdle)]);

    /// <summary>The updates his tail's tip shows swung out before a whip or a flail lashes.</summary>
    private const int LashTipOutUpdates = 2;

    /// <summary>The updates his tail shows recoiling after a whip or a flail lashes.</summary>
    private const int LashRecoilUpdates = 3;

    /// <summary>The updates his tail's tip shows swung out before his fake whip lashes, a long wind-up.</summary>
    private const int FakeWhipTipOutUpdates = 16;

    /// <summary>The updates his tail shows recoiling after his fake whip lashes, a long hold.</summary>
    private const int FakeWhipRecoilUpdates = 16;

    /// <summary>
    /// His tail lashing out on <paramref name="poses"/> (#2465): its tip swung out for <paramref name="tipOutUpdates"/>, then
    /// from cocked to curled, each pose a beat shorter, while his body's picture shifts back from the way he faces and
    /// down a pixel further each pose, two at the curl, and settles as the lash ends. His whips and his fake whip lash so.
    /// </summary>
    private static AnimationBeat<EnemyFrameId, DraygonCue>[] ShiftingLash(DraygonTailPoses poses, int tipOutUpdates) =>
    [
        Cue(new DraygonCue.ShiftBodyPicture(BackFrom(poses.Facing, 1))),
        Show(poses.TipOut, tipOutUpdates),
        Cue(new DraygonCue.ShiftBodyPicture(BackFrom(poses.Facing, 2))),
        Show(poses.Cocking, 6),
        Cue(new DraygonCue.ShiftBodyPicture(BackFrom(poses.Facing, 3))),
        Show(poses.Raising, 5),
        Cue(new DraygonCue.ShiftBodyPicture(BackFrom(poses.Facing, 4))),
        Show(poses.HangingLong, 4),
        Cue(new DraygonCue.ShiftBodyPicture(BackFrom(poses.Facing, 5))),
        Show(poses.Swinging, 3),
        Cue(new DraygonCue.ShiftBodyPicture(BackFrom(poses.Facing, 6))),
        Show(poses.LashedOut, 2),
        Cue(new DraygonCue.ShiftBodyPicture(BackFrom(poses.Facing, 8))),
        Show(poses.Curling, 1),
        Cue(new DraygonCue.ShiftBodyPicture(new(0, 0))),
    ];

    /// <summary>
    /// His tail lashing out on <paramref name="poses"/> as <see cref="ShiftingLash"/> lashes it after a short wind-up, but
    /// with his body's picture still (#2465): his flail lashes so.
    /// </summary>
    private static AnimationBeat<EnemyFrameId, DraygonCue>[] Lash(DraygonTailPoses poses) =>
    [
        Show(poses.TipOut, LashTipOutUpdates),
        Show(poses.Cocking, 6),
        Show(poses.Raising, 5),
        Show(poses.HangingLong, 4),
        Show(poses.Swinging, 3),
        Show(poses.LashedOut, 2),
        Show(poses.Curling, 1),
    ];

    /// <summary>
    /// His tail recoiling on <paramref name="poses"/> (#2465): recoiling for <paramref name="recoilingUpdates"/>, then
    /// swinging back from curled to cocked through the lash's poses, each a beat longer. Every lash ends so.
    /// </summary>
    private static AnimationBeat<EnemyFrameId, DraygonCue>[] Recoil(DraygonTailPoses poses, int recoilingUpdates) =>
    [
        Show(poses.Recoiling, recoilingUpdates),
        Show(poses.Curling, 1),
        Show(poses.LashedOut, 2),
        Show(poses.Swinging, 3),
        Show(poses.HangingLong, 4),
        Show(poses.Raising, 5),
        Show(poses.Cocking, 6),
    ];

    /// <summary>
    /// The shift of his body's picture <paramref name="pixels"/> back from the way he faces, <paramref name="facing"/>, and
    /// as many down.
    /// </summary>
    private static Offset16<RoomPixels> BackFrom(HorizontalDirection facing, int pixels) =>
        DraygonParts.TowardFacing(new(-pixels, pixels), facing);

    /// <summary>His arms' idle sway over a facing's poses (#2557).</summary>
    private static AnimationBeat<EnemyFrameId, DraygonCue>[] ArmsIdle(DraygonFacingPoses poses) =>
    [
        Show(poses.ArmsForward, 5),
        Show(poses.ArmsLowering, 5),
        Show(poses.ArmsDown, 5),
        Show(poses.ArmsDownStretched, 5),
        Show(poses.ArmsRaising, 5),
        Show(poses.ArmsReaching, 5),
    ];

    /// <summary>His arms closing and tucking near the top of a swoop, over a facing's poses (#2557).</summary>
    private static AnimationBeat<EnemyFrameId, DraygonCue>[] ArmsNearSwoopApex(DraygonFacingPoses poses) =>
    [
        Show(poses.ArmsSpreadWide, 1),
        Show(poses.ArmsClosing, 1),
        Show(poses.ArmsGathering, 1),
        Show(poses.ArmsTucked, 64),
    ];

    /// <summary>His arms' grab, over a facing's poses (#2557).</summary>
    private static AnimationBeat<EnemyFrameId, DraygonCue>[] ArmsGrab(DraygonFacingPoses poses) =>
    [
        Show(poses.ArmsSpreadWide, 1),
        Show(poses.ArmsClosing, 1),
        Show(poses.ArmsGathering, 1),
        Show(poses.ArmsTucked, 8),
        Show(poses.ArmsGathering, 1),
        Show(poses.ArmsClosing, 1),
        Show(poses.ArmsSpreadWide, 1),
        Show(poses.ArmsClosing, 1),
    ];

    /// <summary>His limp arm dropping as he dies, over a facing's poses (#2557).</summary>
    private static AnimationBeat<EnemyFrameId, DraygonCue>[] BodyDying(DraygonFacingPoses poses) =>
    [
        Show(poses.LimpArmClawRaised, 5),
        Show(poses.LimpArmClawDropping, 5),
        Show(poses.LimpArmClawDropped, 5),
        Show(poses.LimpArmClawCurled, 5),
    ];

    /// <summary>His jaw opening to spit goop the way he faces and closing again, over a facing's poses (#2557).</summary>
    private static AnimationBeat<EnemyFrameId, DraygonCue>[] BodyFireGoop(DraygonFacingPoses poses) =>
    [
        Show(poses.BodyJawShut, 1),
        Show(poses.BodyJawParting, 2),
        Show(poses.BodyJawOpen, 3),
        Cue(new DraygonCue.SpawnGoop(poses.Facing)),
        Cue(new DraygonCue.QueueSound(DraygonSoundDefinitions.GoopSpit)),
        Show(poses.BodyJawWide, 3),
        Show(poses.BodyJawOpen, 2),
        Show(poses.BodyJawParting, 2),
        Show(poses.BodyJawShut, 1),
    ];

    /// <summary>His roar: the sound and his jaw opening and closing, six updates a picture, over a facing's poses (#2557).</summary>
    private static AnimationBeat<EnemyFrameId, DraygonCue>[] BodyRoar(DraygonFacingPoses poses) =>
    [
        Cue(new DraygonCue.QueueSound(DraygonSoundDefinitions.Roar)),
        Show(poses.BodyJawShut, 6),
        Show(poses.BodyJawParting, 6),
        Show(poses.BodyJawOpen, 6),
        Show(poses.BodyJawWide, 6),
        Show(poses.BodyJawOpen, 6),
        Show(poses.BodyJawParting, 6),
        Show(poses.BodyJawShut, 6),
    ];

    /// <summary>
    /// His eye's idle blink over a facing's poses (#2557), tracking with it as it ends as if facing left, whichever way he
    /// faces, as the game's right-facing blink does.
    /// </summary>
    private static AnimationBeat<EnemyFrameId, DraygonCue>[] EyeIdle(DraygonFacingPoses poses) =>
    [
        Show(poses.EyeShut, 21),
        Show(poses.EyeOpening, 5),
        Show(poses.EyeHalfOpen, 5),
        Show(poses.EyeOpening, 10),
        Show(poses.EyeOpen, 20),
        Show(poses.EyeNarrowing, 20),
        Show(poses.EyeClosing, 10),
        Show(poses.EyeHalfOpen, 5),
        Show(poses.EyeOpening, 5),
        Show(poses.EyeShut, 5),
        Cue(new DraygonCue.TrackWithEye(HorizontalDirection.Left)),
    ];

    /// <summary>His eye rolling as he dies, over a facing's poses (#2557).</summary>
    private static AnimationBeat<EnemyFrameId, DraygonCue>[] EyeDying(DraygonFacingPoses poses) =>
    [
        Show(poses.EyeOpen, 4),
        Show(poses.EyeNarrowing, 4),
        Show(poses.EyeBloodshot, 4),
        Show(poses.EyeClosing, 4),
    ];

    /// <summary>His dying eye's stare, over a facing's poses (#2557).</summary>
    private static AnimationBeat<EnemyFrameId, DraygonCue>[] EyeDyingStare(DraygonFacingPoses poses) =>
    [
        Show(poses.EyeWide, 32),
        Show(poses.EyeHalfOpen, 16),
    ];

    /// <summary>His dead eye closing for good, over a facing's poses (#2557).</summary>
    private static AnimationBeat<EnemyFrameId, DraygonCue>[] EyeDead(DraygonFacingPoses poses) =>
    [
        Show(poses.EyeWide, 32),
        Show(poses.EyeHalfOpen, 32),
        Show(poses.EyeOpening, 32),
        Show(poses.EyeShut, 1),
    ];

    /// <summary>His tail's idle swing, out to its farthest and back, over a facing's poses (#2557).</summary>
    private static AnimationBeat<EnemyFrameId, DraygonCue>[] TailIdle(DraygonTailPoses poses) =>
    [
        Show(poses.TipCurledIn, 8),
        Show(poses.TipSwingingOut, 7),
        Show(poses.TipOut, 6),
        Show(poses.TipFurtherOut, 6),
        Show(poses.TipFarthestOut, 6),
        Show(poses.TipReturning, 6),
        Show(poses.TipBack, 6),
        Show(poses.TipReturning, 6),
        Show(poses.TipFarthestOut, 6),
        Show(poses.TipFurtherOut, 6),
        Show(poses.TipOut, 6),
        Show(poses.TipSwingingOut, 7),
    ];

    /// <summary>
    /// One round of his dying explosions after <paramref name="wait"/> updates (#2557): a big and a small explosion, a dust
    /// cloud and breath bubbles, with the explosion sound.
    /// </summary>
    private static AnimationBeat<EnemyFrameId, DraygonCue>[] DyingExplosions(int wait) =>
    [
        Wait(wait),
        Cue(new DraygonCue.SpawnDyingEffect(RoomEffectActorKind.BigExplosion)),
        Cue(new DraygonCue.SpawnDyingEffect(RoomEffectActorKind.SmallExplosion)),
        Cue(new DraygonCue.SpawnDyingEffect(RoomEffectActorKind.BigDustCloud)),
        Cue(new DraygonCue.SpawnDyingEffect(RoomEffectActorKind.DraygonBreathBubbles)),
        Cue(new DraygonCue.QueueSound(DraygonSoundDefinitions.Explosion)),
    ];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, DraygonCue> Show(EnemyFrameId frame, int duration) =>
        new AnimationBeat<EnemyFrameId, DraygonCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, DraygonCue> Cue(DraygonCue cue) =>
        new AnimationBeat<EnemyFrameId, DraygonCue>.Event(cue);

    /// <summary>Keeps the current frame for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, DraygonCue> Wait(int duration) =>
        new AnimationBeat<EnemyFrameId, DraygonCue>.Pause(duration);
}
