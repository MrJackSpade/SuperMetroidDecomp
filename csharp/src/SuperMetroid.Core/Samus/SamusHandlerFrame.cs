using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.World;

namespace SuperMetroid.Core.Samus;

/// <summary>
/// One gameplay frame of the bank-$90 Samus handler: the values alpha samples before movement and the ones
/// beta and the animation transitions update, shared by <see cref="SuperMetroidRuntime"/> and
/// <see cref="SamusFramePhases"/>. Each value changes only through the sampling operation that names its point in
/// the native handler sequence.
/// </summary>
internal sealed class SamusHandlerFrame
{
    /// <summary>Samus's camera point carried across the frame boundary; movement and the frame tail advance it.</summary>
    internal FixedPosition<RoomPixels> PreviousCameraPoint { get; private set; }

    /// <summary>The pose alpha sampled before any movement.</summary>
    internal SamusPoseId PoseAtFrameStart { get; private set; }

    /// <summary>The pose beta dispatches on, which EnemyMain can replace after alpha.</summary>
    internal SamusPoseId PoseAtBetaMovement { get; private set; }

    /// <summary>The movement type of the pose at frame start.</summary>
    internal SamusMovementType MovementTypeAtFrameStart { get; private set; }

    /// <summary>Whether the death sequence owns Samus this frame.</summary>
    internal bool DeathOwnsSamus { get; private set; }

    /// <summary>Whether X-ray took control of Samus during this frame.</summary>
    internal bool XrayActivatedThisFrame { get; private set; }

    /// <summary>The movement type read before X-ray pose input ran.</summary>
    internal SamusMovementType MovementBeforeXrayAdmission { get; private set; }

    /// <summary>Whether a bomb jump locks pose input this frame.</summary>
    internal bool BombJumpLocksPoseInput { get; private set; }

    /// <summary>Whether the input lookup fell through to the pose-definition fallback ($91:82D9).</summary>
    internal bool UsePoseDefinitionFallback { get; private set; }

    /// <summary>
    /// The alpha installed at the frame's start and the beta the frame dispatches, read from the installed pair when
    /// alpha begins and again before beta; beta-handler gates read <see cref="SamusControlMode.Beta"/>.
    /// </summary>
    internal SamusControlMode Mode { get; private set; }

    /// <summary>Whether horizontal momentum remained when the fallback pose was selected.</summary>
    internal bool DeceleratingFallbackHasMomentum { get; private set; }

    /// <summary>
    /// Whether Samus movement was enabled when alpha's head finished; alpha's tail, the post-EnemyMain resample and beta
    /// movement run only then.
    /// </summary>
    internal bool MovementEnabled { get; private set; }

    /// <summary>Whether EnemyMain ran before Samus because this frame has no movement to wait for.</summary>
    internal bool EnemyMainAlreadyRan { get; private set; }

    /// <summary>
    /// Whether that hoisted EnemyMain restored the normal handler pair (gunship $A2:A987). The locked alpha has
    /// already been installed for this frame, so only alpha's checks still see the lock.
    /// </summary>
    internal bool EnemyMainReleasedAlphaLock { get; private set; }

    /// <summary>Whether X-ray owns pose input this frame.</summary>
    internal bool XrayOwnsPoseInput { get; private set; }

    /// <summary>Whether the locked alpha, a bomb jump, Shinespark or Crystal Flash, or the elevator locks pose input.</summary>
    internal bool ActorLocksPoseInput { get; private set; }

    /// <summary>Whether the posture-transition input dispatcher, an RTS ($91:8146), skips pose input.</summary>
    internal bool PostureTransitionSkipsPoseInput { get; private set; }

    /// <summary>Whether ordinary alpha's HUD, bomb and projectile owners run.</summary>
    internal bool AlphaRuns { get; private set; }

    /// <summary>Whether grapple owns beta movement.</summary>
    internal bool GrappleOwnsMovement { get; private set; }

    /// <summary>Whether the grapple-release mover keeps alpha's pending aim or turn transition.</summary>
    internal bool GrappleReleaseAcceptsPoseInput { get; private set; }

    /// <summary>Samples Samus's camera point carried across the frame boundary.</summary>
    internal void SampleCameraPoint(FixedPosition<RoomPixels> point) => PreviousCameraPoint = point;

    /// <summary>Replaces the camera point's whole pixels with a grapple snap's corrected words, keeping its subpositions.</summary>
    internal void SnapCameraPoint(Position16<RoomPixels> whole) => PreviousCameraPoint = PreviousCameraPoint.WithWhole(whole);

    /// <summary>Samples the pose alpha dispatches on, which beta also dispatches on until EnemyMain replaces it.</summary>
    internal void SampleFrameStartPose(SamusPoseId pose)
    {
        PoseAtFrameStart = pose;
        PoseAtBetaMovement = pose;
    }

    /// <summary>Samples the movement type of the pose at frame start.</summary>
    internal void SampleMovementType(SamusMovementType type) => MovementTypeAtFrameStart = type;

    /// <summary>Samples whether the death sequence owns Samus this frame.</summary>
    internal void SampleDeathOwnership(bool owns) => DeathOwnsSamus = owns;

    /// <summary>Records the handler pair the frame dispatches and X-ray's hold on pose input, before X-ray admission runs.</summary>
    internal void BeginControl(SamusControlMode mode, bool xrayOwnsPoseInput, SamusMovementType movementBeforeXray)
    {
        Mode = mode;
        XrayOwnsPoseInput = xrayOwnsPoseInput;
        XrayActivatedThisFrame = false;
        MovementBeforeXrayAdmission = movementBeforeXray;
    }

    /// <summary>
    /// Samples what locks pose input: Samus's bomb jump, the live alpha lock, <paramref name="otherActorLocks"/> (Shinespark,
    /// Crystal Flash or the elevator), and the posture-transition dispatcher's RTS.
    /// </summary>
    internal void SamplePoseInputLocks(SamusState samus, bool otherActorLocks)
    {
        BombJumpLocksPoseInput = samus.BombJumpPoseInputLocked;
        ActorLocksPoseInput = AlphaInputLocked(samus) || BombJumpLocksPoseInput || otherActorLocks;
        PostureTransitionSkipsPoseInput = MovementTypeAtFrameStart == SamusMovementType.PostureTransition;
    }

    /// <summary>Samples whether <paramref name="lookup"/> fell through to the pose-definition fallback, and whether Samus kept horizontal momentum.</summary>
    internal void SamplePoseLookup(SamusPoseTransitionLookup lookup, SamusState samus)
    {
        UsePoseDefinitionFallback = lookup.UsesPoseDefinitionFallback;
        DeceleratingFallbackHasMomentum = samus.HorizontalSpeed.BaseFixed != 0;
    }

    /// <summary>Samples whether Samus movement is enabled as alpha's head finishes.</summary>
    internal void SampleMovementEnabled(bool enabled) => MovementEnabled = enabled;

    /// <summary>Samples whether ordinary alpha's HUD, bomb and projectile owners run.</summary>
    internal void SampleAlphaRuns(bool runs) => AlphaRuns = runs;

    /// <summary>Records that X-ray took control of Samus during this frame.</summary>
    internal void MarkXrayActivated() => XrayActivatedThisFrame = true;

    /// <summary>Records whether grapple owns beta movement.</summary>
    internal void SampleGrappleOwnership(bool owns) => GrappleOwnsMovement = owns;

    /// <summary>Replaces the beta handler the frame dispatches, which an actor command does before dispatch.</summary>
    internal void RedispatchBeta(SamusBetaHandler beta) => Mode = Mode with { Beta = beta };

    /// <summary>Records whether the grapple-release mover keeps alpha's pending aim or turn transition.</summary>
    internal void SampleGrappleReleasePoseInput(bool accepts) => GrappleReleaseAcceptsPoseInput = accepts;

    /// <summary>Samples the pose beta movement dispatches on.</summary>
    internal void SampleBetaMovementPose(SamusPoseId pose) => PoseAtBetaMovement = pose;

    /// <summary>Records whether EnemyMain ran before Samus because this frame has no movement to wait for.</summary>
    internal void RecordHoistedEnemyMain(bool hoisted) => EnemyMainAlreadyRan = hoisted;

    /// <summary>Records whether that hoisted EnemyMain restored the normal handler pair.</summary>
    internal void RecordEnemyMainReleasedAlphaLock(bool released) => EnemyMainReleasedAlphaLock = released;

    /// <summary>
    /// Whether this frame's alpha handler is the locked one installed at its start. Read live at each check: X-ray
    /// admission can change the lock between them.
    /// </summary>
    internal bool AlphaInputLocked(SamusState samus) => samus.Control.AlphaLocked || EnemyMainReleasedAlphaLock;
}
