using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus;

/// <summary>
/// One gameplay frame of the bank-$90 Samus handler: the values alpha samples before movement and the ones
/// beta and the animation transitions update, shared by <see cref="SuperMetroidRuntime"/> and
/// <see cref="SamusFramePhases"/>. Fields, so the phases can pass them by reference as the native code reuses
/// its scratch words.
/// </summary>
internal sealed class SamusHandlerFrame
{
    /// <summary>Samus's camera point carried across the frame boundary; movement and the frame tail advance it.</summary>
    internal FixedPosition<RoomPixels> PreviousCameraPoint;

    /// <summary>The pose alpha sampled before any movement.</summary>
    internal SamusPoseId PoseAtFrameStart;

    /// <summary>The pose beta dispatches on, which EnemyMain can replace after alpha.</summary>
    internal SamusPoseId PoseAtBetaMovement;

    /// <summary>The movement type of the pose at frame start.</summary>
    internal SamusMovementType MovementTypeAtFrameStart;

    /// <summary>Whether the death sequence owns Samus this frame.</summary>
    internal bool DeathOwnsSamus;

    /// <summary>Whether X-ray took control of Samus during this frame.</summary>
    internal bool XrayActivatedThisFrame;

    /// <summary>The movement type read before X-ray pose input ran.</summary>
    internal SamusMovementType MovementBeforeXrayAdmission;

    /// <summary>Whether a bomb jump locks pose input this frame.</summary>
    internal bool BombJumpLocksPoseInput;

    /// <summary>Whether the input lookup fell through to the pose-definition fallback ($91:82D9).</summary>
    internal bool UsePoseDefinitionFallback;

    /// <summary>
    /// The alpha installed at the frame's start and the beta the frame dispatches, read from the installed pair when
    /// alpha begins and again before beta; beta-handler gates read <see cref="SamusControlMode.Beta"/>.
    /// </summary>
    internal SamusControlMode Mode;

    /// <summary>Whether horizontal momentum remained when the fallback pose was selected.</summary>
    internal bool DeceleratingFallbackHasMomentum;

    /// <summary>
    /// Whether Samus movement was enabled when alpha's head finished; alpha's tail, the post-EnemyMain resample and beta
    /// movement run only then.
    /// </summary>
    internal bool MovementEnabled;

    /// <summary>Whether EnemyMain ran before Samus because this frame has no movement to wait for.</summary>
    internal bool EnemyMainAlreadyRan;

    /// <summary>
    /// Whether that hoisted EnemyMain restored the normal handler pair (gunship $A2:A987). The locked alpha has
    /// already been installed for this frame, so only alpha's checks still see the lock.
    /// </summary>
    internal bool EnemyMainReleasedAlphaLock;

    /// <summary>Whether X-ray owns pose input this frame.</summary>
    internal bool XrayOwnsPoseInput;

    /// <summary>Whether the locked alpha, a bomb jump, Shinespark or Crystal Flash, or the elevator locks pose input.</summary>
    internal bool ActorLocksPoseInput;

    /// <summary>Whether the posture-transition input dispatcher, an RTS ($91:8146), skips pose input.</summary>
    internal bool PostureTransitionSkipsPoseInput;

    /// <summary>Whether ordinary alpha's HUD, bomb and projectile owners run.</summary>
    internal bool AlphaRuns;

    /// <summary>Whether grapple owns beta movement.</summary>
    internal bool GrappleOwnsMovement;

    /// <summary>Whether the grapple-release mover keeps alpha's pending aim or turn transition.</summary>
    internal bool GrappleReleaseAcceptsPoseInput;

    /// <summary>
    /// Whether this frame's alpha handler is the locked one installed at its start. Read live at each check: X-ray
    /// admission can change the lock between them.
    /// </summary>
    internal bool AlphaInputLocked(SamusState samus) => samus.Control.AlphaLocked || EnemyMainReleasedAlphaLock;
}
