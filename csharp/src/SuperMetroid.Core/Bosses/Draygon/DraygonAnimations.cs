using SuperMetroid.Core.Animation;
using F = SuperMetroid.Core.Enemies.Artwork.EnemyFrameId;
using SuperMetroid.Core.Enemies.Artwork;
using P = SuperMetroid.Core.Bosses.Draygon.DraygonSegment;
using SuperMetroid.Core.Enemies.EffectActors;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// Draygon's typed animations (#641, #1600): each body, eye, tail and arms segment's beats, the animation starting at
/// each, which plays each of its segments whole (#2296), and his tail's final whips, which play its whip four times and
/// which his grab starts by name (#2399). The idle eye shows its open and narrowing pictures twice in a row
/// each, and the two showings stay two beats: his body shows the eye's picture as a showing begins on an update the draw
/// pass reaches the eye, so the second showing shows it again where the first was missed. His tail's lashes are written
/// once over a facing's poses (#2465): his whips and his fake whip lash out shifting his body's picture, his flail lashes
/// with it still, and every lash recoils and swings back the same way.
/// </summary>
internal static class DraygonAnimations
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
        [P.ArmsFacingLeftIdle] =
        [
            Show(F.DraygonArmsFacingLeftForward, 5),
            Show(F.DraygonArmsFacingLeftLowering, 5),
            Show(F.DraygonArmsFacingLeftDown, 5),
            Show(F.DraygonArmsFacingLeftDownStretched, 5),
            Show(F.DraygonArmsFacingLeftRaising, 5),
            Show(F.DraygonArmsFacingLeftReaching, 5),
        ],
        [P.ArmsFacingLeftNearSwoopApex] =
        [
            Show(F.DraygonArmsFacingLeftSpreadWide, 1),
            Show(F.DraygonArmsFacingLeftClosing, 1),
            Show(F.DraygonArmsFacingLeftGathering, 1),
            Show(F.DraygonArmsFacingLeftTucked, 64),
        ],
        [P.ArmsFacingLeftGrab] =
        [
            Show(F.DraygonArmsFacingLeftSpreadWide, 1),
            Show(F.DraygonArmsFacingLeftClosing, 1),
            Show(F.DraygonArmsFacingLeftGathering, 1),
            Show(F.DraygonArmsFacingLeftTucked, 8),
            Show(F.DraygonArmsFacingLeftGathering, 1),
            Show(F.DraygonArmsFacingLeftClosing, 1),
            Show(F.DraygonArmsFacingLeftSpreadWide, 1),
            Show(F.DraygonArmsFacingLeftClosing, 1),
        ],
        [P.BodyFacingLeftDying] =
        [
            Show(F.DraygonLimpArmFacingLeftClawRaised, 5),
            Show(F.DraygonLimpArmFacingLeftClawDropping, 5),
            Show(F.DraygonLimpArmFacingLeftClawDropped, 5),
            Show(F.DraygonLimpArmFacingLeftClawCurled, 5),
        ],
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
        [P.BodyDyingExplosions] =
        [
            Wait(12),
            Cue(new DraygonCue.SpawnDyingEffect(RoomEffectActorKind.BigExplosion)),
            Cue(new DraygonCue.SpawnDyingEffect(RoomEffectActorKind.SmallExplosion)),
            Cue(new DraygonCue.SpawnDyingEffect(RoomEffectActorKind.BigDustCloud)),
            Cue(new DraygonCue.SpawnDyingEffect(RoomEffectActorKind.DraygonBreathBubbles)),
            Cue(new DraygonCue.QueueSound(DraygonSoundDefinitions.Explosion)),
        ],
        [P.BodyDyingParalysis] =
        [
            Wait(1),
            Cue(DraygonCue.ParalyseTailAndArms.Instance),
        ],
        [P.BodyDyingExplosionLoop] =
        [
            Wait(16),
            Cue(new DraygonCue.SpawnDyingEffect(RoomEffectActorKind.BigExplosion)),
            Cue(new DraygonCue.SpawnDyingEffect(RoomEffectActorKind.SmallExplosion)),
            Cue(new DraygonCue.SpawnDyingEffect(RoomEffectActorKind.BigDustCloud)),
            Cue(new DraygonCue.SpawnDyingEffect(RoomEffectActorKind.DraygonBreathBubbles)),
            Cue(new DraygonCue.QueueSound(DraygonSoundDefinitions.Explosion)),
        ],
        [P.Delete] =
        [
        ],
        [P.BodyFacingLeftFireGoop] =
        [
            Show(F.DraygonBodyFacingLeftJawShut, 1),
            Show(F.DraygonBodyFacingLeftJawParting, 2),
            Show(F.DraygonBodyFacingLeftJawOpen, 3),
            Cue(new DraygonCue.SpawnGoop(HorizontalDirection.Left)),
            Cue(new DraygonCue.QueueSound(DraygonSoundDefinitions.GoopSpit)),
            Show(F.DraygonBodyFacingLeftJawWide, 3),
            Show(F.DraygonBodyFacingLeftJawOpen, 2),
            Show(F.DraygonBodyFacingLeftJawParting, 2),
            Show(F.DraygonBodyFacingLeftJawShut, 1),
        ],
        [P.BodyFacingLeftRoar] =
        [
            Cue(new DraygonCue.QueueSound(DraygonSoundDefinitions.Roar)),
            Show(F.DraygonBodyFacingLeftJawShut, 6),
            Show(F.DraygonBodyFacingLeftJawParting, 6),
            Show(F.DraygonBodyFacingLeftJawOpen, 6),
            Show(F.DraygonBodyFacingLeftJawWide, 6),
            Show(F.DraygonBodyFacingLeftJawOpen, 6),
            Show(F.DraygonBodyFacingLeftJawParting, 6),
            Show(F.DraygonBodyFacingLeftJawShut, 6),
        ],
        [P.EyeFacingLeftIdle] =
        [
            Show(F.DraygonEyeFacingLeftShut, 21),
            Show(F.DraygonEyeFacingLeftOpening, 5),
            Show(F.DraygonEyeFacingLeftHalfOpen, 5),
            Show(F.DraygonEyeFacingLeftOpening, 10),
            Show(F.DraygonEyeFacingLeftOpen, 10),
            Show(F.DraygonEyeFacingLeftOpen, 10),
            Show(F.DraygonEyeFacingLeftNarrowing, 10),
            Show(F.DraygonEyeFacingLeftNarrowing, 10),
            Show(F.DraygonEyeFacingLeftClosing, 10),
            Show(F.DraygonEyeFacingLeftHalfOpen, 5),
            Show(F.DraygonEyeFacingLeftOpening, 5),
            Show(F.DraygonEyeFacingLeftShut, 5),
            Cue(new DraygonCue.TrackWithEye(HorizontalDirection.Left)),
        ],
        [P.EyeFacingLeftDying] =
        [
            Show(F.DraygonEyeFacingLeftOpen, 4),
            Show(F.DraygonEyeFacingLeftNarrowing, 4),
            Show(F.DraygonEyeFacingLeftBloodshot, 4),
            Show(F.DraygonEyeFacingLeftClosing, 4),
        ],
        [P.EyeFacingLeftDyingStare] =
        [
            Show(F.DraygonEyeFacingLeftWide, 32),
            Show(F.DraygonEyeFacingLeftHalfOpen, 16),
        ],
        [P.EyeFacingLeftDead] =
        [
            Show(F.DraygonEyeFacingLeftWide, 32),
            Show(F.DraygonEyeFacingLeftHalfOpen, 32),
            Show(F.DraygonEyeFacingLeftOpening, 32),
            Show(F.DraygonEyeFacingLeftShut, 1),
        ],
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
        [P.TailFacingLeftIdle] =
        [
            Show(F.DraygonTailFacingLeftTipCurledIn, 8),
            Show(F.DraygonTailFacingLeftTipSwingingOut, 7),
            Show(F.DraygonTailFacingLeftTipOut, 6),
            Show(F.DraygonTailFacingLeftTipFurtherOut, 6),
            Show(F.DraygonTailFacingLeftTipFarthestOut, 6),
            Show(F.DraygonTailFacingLeftTipReturning, 6),
            Show(F.DraygonTailFacingLeftTipBack, 6),
            Show(F.DraygonTailFacingLeftTipReturning, 6),
            Show(F.DraygonTailFacingLeftTipFarthestOut, 6),
            Show(F.DraygonTailFacingLeftTipFurtherOut, 6),
            Show(F.DraygonTailFacingLeftTipOut, 6),
            Show(F.DraygonTailFacingLeftTipSwingingOut, 7),
        ],
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
        [P.ArmsFacingRightIdle] =
        [
            Show(F.DraygonArmsFacingRightForward, 5),
            Show(F.DraygonArmsFacingRightLowering, 5),
            Show(F.DraygonArmsFacingRightDown, 5),
            Show(F.DraygonArmsFacingRightDownStretched, 5),
            Show(F.DraygonArmsFacingRightRaising, 5),
            Show(F.DraygonArmsFacingRightReaching, 5),
        ],
        [P.ArmsFacingRightNearSwoopApex] =
        [
            Show(F.DraygonArmsFacingRightSpreadWide, 1),
            Show(F.DraygonArmsFacingRightClosing, 1),
            Show(F.DraygonArmsFacingRightGathering, 1),
            Show(F.DraygonArmsFacingRightTucked, 64),
        ],
        [P.ArmsFacingRightGrab] =
        [
            Show(F.DraygonArmsFacingRightSpreadWide, 1),
            Show(F.DraygonArmsFacingRightClosing, 1),
            Show(F.DraygonArmsFacingRightGathering, 1),
            Show(F.DraygonArmsFacingRightTucked, 8),
            Show(F.DraygonArmsFacingRightGathering, 1),
            Show(F.DraygonArmsFacingRightClosing, 1),
            Show(F.DraygonArmsFacingRightSpreadWide, 1),
            Show(F.DraygonArmsFacingRightClosing, 1),
        ],
        [P.BodyFacingRightDying] =
        [
            Show(F.DraygonLimpArmFacingRightClawRaised, 5),
            Show(F.DraygonLimpArmFacingRightClawDropping, 5),
            Show(F.DraygonLimpArmFacingRightClawDropped, 5),
            Show(F.DraygonLimpArmFacingRightClawCurled, 5),
        ],
        [P.BodyFacingRightIdle] =
        [
            Cue(new DraygonCue.TrackWithEye(HorizontalDirection.Right)),
            Show(F.DraygonBodyFacingRightJawShut, 1),
        ],
        [P.BodyFacingRightFireGoop] =
        [
            Show(F.DraygonBodyFacingRightJawShut, 1),
            Show(F.DraygonBodyFacingRightJawParting, 2),
            Show(F.DraygonBodyFacingRightJawOpen, 3),
            Cue(new DraygonCue.SpawnGoop(HorizontalDirection.Right)),
            Cue(new DraygonCue.QueueSound(DraygonSoundDefinitions.GoopSpit)),
            Show(F.DraygonBodyFacingRightJawWide, 3),
            Show(F.DraygonBodyFacingRightJawOpen, 2),
            Show(F.DraygonBodyFacingRightJawParting, 2),
            Show(F.DraygonBodyFacingRightJawShut, 1),
        ],
        [P.BodyFacingRightRoar] =
        [
            Cue(new DraygonCue.QueueSound(DraygonSoundDefinitions.Roar)),
            Show(F.DraygonBodyFacingRightJawShut, 6),
            Show(F.DraygonBodyFacingRightJawParting, 6),
            Show(F.DraygonBodyFacingRightJawOpen, 6),
            Show(F.DraygonBodyFacingRightJawWide, 6),
            Show(F.DraygonBodyFacingRightJawOpen, 6),
            Show(F.DraygonBodyFacingRightJawParting, 6),
            Show(F.DraygonBodyFacingRightJawShut, 6),
        ],
        [P.EyeFacingRightIdle] =
        [
            Show(F.DraygonEyeFacingRightShut, 21),
            Show(F.DraygonEyeFacingRightOpening, 5),
            Show(F.DraygonEyeFacingRightHalfOpen, 5),
            Show(F.DraygonEyeFacingRightOpening, 10),
            Show(F.DraygonEyeFacingRightOpen, 10),
            Show(F.DraygonEyeFacingRightOpen, 10),
            Show(F.DraygonEyeFacingRightNarrowing, 10),
            Show(F.DraygonEyeFacingRightNarrowing, 10),
            Show(F.DraygonEyeFacingRightClosing, 10),
            Show(F.DraygonEyeFacingRightHalfOpen, 5),
            Show(F.DraygonEyeFacingRightOpening, 5),
            Show(F.DraygonEyeFacingRightShut, 5),
            Cue(new DraygonCue.TrackWithEye(HorizontalDirection.Left)),
        ],
        [P.EyeFacingRightDying] =
        [
            Show(F.DraygonEyeFacingRightOpen, 4),
            Show(F.DraygonEyeFacingRightNarrowing, 4),
            Show(F.DraygonEyeFacingRightBloodshot, 4),
            Show(F.DraygonEyeFacingRightClosing, 4),
        ],
        [P.EyeFacingRightDyingStare] =
        [
            Show(F.DraygonEyeFacingRightWide, 32),
            Show(F.DraygonEyeFacingRightHalfOpen, 16),
        ],
        [P.EyeFacingRightDead] =
        [
            Show(F.DraygonEyeFacingRightWide, 32),
            Show(F.DraygonEyeFacingRightHalfOpen, 32),
            Show(F.DraygonEyeFacingRightOpening, 32),
            Show(F.DraygonEyeFacingRightShut, 1),
        ],
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
        [P.TailFacingRightIdle] =
        [
            Show(F.DraygonTailFacingRightTipCurledIn, 8),
            Show(F.DraygonTailFacingRightTipSwingingOut, 7),
            Show(F.DraygonTailFacingRightTipOut, 6),
            Show(F.DraygonTailFacingRightTipFurtherOut, 6),
            Show(F.DraygonTailFacingRightTipFarthestOut, 6),
            Show(F.DraygonTailFacingRightTipReturning, 6),
            Show(F.DraygonTailFacingRightTipBack, 6),
            Show(F.DraygonTailFacingRightTipReturning, 6),
            Show(F.DraygonTailFacingRightTipFarthestOut, 6),
            Show(F.DraygonTailFacingRightTipFurtherOut, 6),
            Show(F.DraygonTailFacingRightTipOut, 6),
            Show(F.DraygonTailFacingRightTipSwingingOut, 7),
        ],
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
    private static IReadOnlyDictionary<DraygonSegment, Animation<DraygonSegment>> StartingAt { get; } = new Dictionary<DraygonSegment, Animation<DraygonSegment>>
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
