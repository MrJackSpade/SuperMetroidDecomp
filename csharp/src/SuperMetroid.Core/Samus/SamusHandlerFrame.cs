using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.World;

namespace SuperMetroid.Core.Samus;

/// <summary>
/// One gameplay frame of the Samus handler: the values the input phase samples before movement and the ones
/// the movement phase and the animation transitions update, shared by <see cref="SuperMetroidRuntime"/> and
/// <see cref="SamusFramePhases"/>. Each value changes only through the sampling operation that names its point in
/// the handler sequence.
/// </summary>
internal sealed class SamusHandlerFrame
{
    /// <summary>Samus's camera point carried across the frame boundary; movement and the frame tail advance it.</summary>
    internal FixedPosition<RoomPixels> PreviousCameraPoint { get; private set; }

    /// <summary>The pose the input phase sampled before any movement.</summary>
    internal SamusPoseId PoseAtFrameStart { get; private set; }

    /// <summary>The pose the movement phase dispatches on, which the enemy update can replace after the input phase.</summary>
    internal SamusPoseId PoseAtMovement { get; private set; }

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

    /// <summary>Whether the input lookup fell through to the pose-definition fallback.</summary>
    internal bool UsePoseDefinitionFallback { get; private set; }

    /// <summary>
    /// The input mode installed at the frame's start and the movement mode the frame runs, read from the installed modes when
    /// the input phase begins and again before the movement phase; movement-mode gates read <see cref="SamusControlMode.Movement"/>.
    /// </summary>
    internal SamusControlMode Mode { get; private set; }

    /// <summary>
    /// Who moves Samus in this frame's movement phase under <see cref="Mode"/>, resolved as the movement phase begins
    /// (<see cref="SampleMovementOwner"/>).
    /// </summary>
    internal SamusMovementOwner MovementOwner { get; private set; }

    /// <summary>Whether horizontal momentum remained when the fallback pose was selected.</summary>
    internal bool DeceleratingFallbackHasMomentum { get; private set; }

    /// <summary>
    /// Whether Samus movement was enabled when the input phase's head finished; the input phase's tail, the resample after the enemy update and the movement phase
    /// movement run only then.
    /// </summary>
    internal bool MovementEnabled { get; private set; }

    /// <summary>Whether the enemy update ran before Samus because this frame has no movement to wait for.</summary>
    internal bool EnemyMainAlreadyRan { get; private set; }

    /// <summary>
    /// Whether that hoisted enemy update restored the normal modes (the gunship). The locked input mode has
    /// already been installed for this frame, so only the input phase's checks still see the lock.
    /// </summary>
    internal bool EnemyMainReleasedInputLock { get; private set; }

    /// <summary>Whether X-ray owns pose input this frame.</summary>
    internal bool XrayOwnsPoseInput { get; private set; }

    /// <summary>Whether the locked input mode, a bomb jump, Shinespark or Crystal Flash, or the elevator locks pose input.</summary>
    internal bool ActorLocksPoseInput { get; private set; }

    /// <summary>Whether the posture-transition input step, which reads no input, skips pose input.</summary>
    internal bool PostureTransitionSkipsPoseInput { get; private set; }

    /// <summary>Whether the ordinary input phase's HUD, bomb and projectile owners run.</summary>
    internal bool InputHandlingRuns { get; private set; }

    /// <summary>Whether grapple owns movement.</summary>
    internal bool GrappleOwnsMovement { get; private set; }

    /// <summary>Whether the grapple-release mover keeps the input phase's pending aim or turn transition.</summary>
    internal bool GrappleReleaseAcceptsPoseInput { get; private set; }

    /// <summary>Samples Samus's camera point carried across the frame boundary.</summary>
    internal void SampleCameraPoint(FixedPosition<RoomPixels> point) => PreviousCameraPoint = point;

    /// <summary>Replaces the camera point's whole pixels with a grapple snap's corrected position, keeping its subpositions.</summary>
    internal void SnapCameraPoint(Position16<RoomPixels> whole) => PreviousCameraPoint = PreviousCameraPoint.WithWhole(whole);

    /// <summary>Samples the pose the input phase dispatches on, which the movement phase also dispatches on until the enemy update replaces it.</summary>
    internal void SampleFrameStartPose(SamusPoseId pose)
    {
        PoseAtFrameStart = pose;
        PoseAtMovement = pose;
    }

    /// <summary>Samples the movement type of the pose at frame start.</summary>
    internal void SampleMovementType(SamusMovementType type) => MovementTypeAtFrameStart = type;

    /// <summary>Samples whether the death sequence owns Samus this frame.</summary>
    internal void SampleDeathOwnership(bool owns) => DeathOwnsSamus = owns;

    /// <summary>Records the modes the frame runs and X-ray's hold on pose input, before X-ray admission runs.</summary>
    internal void BeginControl(SamusControlMode mode, bool xrayOwnsPoseInput, SamusMovementType movementBeforeXray)
    {
        Mode = mode;
        XrayOwnsPoseInput = xrayOwnsPoseInput;
        XrayActivatedThisFrame = false;
        MovementBeforeXrayAdmission = movementBeforeXray;
    }

    /// <summary>
    /// Samples what locks pose input: Samus's bomb jump, the live input lock, <paramref name="otherActorLocks"/> (Shinespark,
    /// Crystal Flash or the elevator), and the posture-transition step, which reads no input.
    /// </summary>
    internal void SamplePoseInputLocks(SamusState samus, bool otherActorLocks)
    {
        BombJumpLocksPoseInput = samus.BombJumpPoseInputLocked;
        ActorLocksPoseInput = InputHandlingLocked(samus) || BombJumpLocksPoseInput || otherActorLocks;
        PostureTransitionSkipsPoseInput = MovementTypeAtFrameStart == SamusMovementType.PostureTransition;
    }

    /// <summary>Samples whether <paramref name="lookup"/> fell through to the pose-definition fallback, and whether Samus kept horizontal momentum.</summary>
    internal void SamplePoseLookup(SamusPoseTransitionLookup lookup, SamusState samus)
    {
        UsePoseDefinitionFallback = lookup.UsesPoseDefinitionFallback;
        DeceleratingFallbackHasMomentum = samus.HorizontalSpeed.HasBaseSpeed;
    }

    /// <summary>Samples whether Samus movement is enabled as the input phase's head finishes.</summary>
    internal void SampleMovementEnabled(bool enabled) => MovementEnabled = enabled;

    /// <summary>Samples whether the ordinary input phase's HUD, bomb and projectile owners run.</summary>
    internal void SampleInputHandlingRuns(bool runs) => InputHandlingRuns = runs;

    /// <summary>Records that X-ray took control of Samus during this frame.</summary>
    internal void MarkXrayActivated() => XrayActivatedThisFrame = true;

    /// <summary>Records whether grapple owns movement.</summary>
    internal void SampleGrappleOwnership(bool owns) => GrappleOwnsMovement = owns;

    /// <summary>Samples who moves Samus in the movement phase, which then runs that owner.</summary>
    internal void SampleMovementOwner(SamusMovementOwner owner) => MovementOwner = owner;

    /// <summary>Replaces the movement mode the frame runs, which an actor's control setup does before it runs.</summary>
    internal void ReplaceMovementMode(SamusMovementMode movement) => Mode = Mode with { Movement = movement };

    /// <summary>Records whether the grapple-release mover keeps the input phase's pending aim or turn transition.</summary>
    internal void SampleGrappleReleasePoseInput(bool accepts) => GrappleReleaseAcceptsPoseInput = accepts;

    /// <summary>Samples the pose movement dispatches on.</summary>
    internal void SampleMovementPose(SamusPoseId pose) => PoseAtMovement = pose;

    /// <summary>Records whether the enemy update ran before Samus because this frame has no movement to wait for.</summary>
    internal void RecordHoistedEnemyMain(bool hoisted) => EnemyMainAlreadyRan = hoisted;

    /// <summary>Records whether that hoisted enemy update restored the normal modes.</summary>
    internal void RecordEnemyMainReleasedInputLock(bool released) => EnemyMainReleasedInputLock = released;

    /// <summary>
    /// Whether this frame's input mode is the locked one installed at its start. Read live at each check: X-ray
    /// admission can change the lock between them.
    /// </summary>
    internal bool InputHandlingLocked(SamusState samus) => samus.Control.InputLocked || EnemyMainReleasedInputLock;
}
