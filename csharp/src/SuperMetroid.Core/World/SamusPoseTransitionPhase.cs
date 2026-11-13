using SuperMetroid.Core.Input;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.World;

/// <summary>
/// The pose update after normal beta's animation: animation command three, the interruptions, the collision
/// transitions, the input transition alpha selected or its lookup-failure command, and the pose-history commit.
/// </summary>
internal sealed class SamusPoseTransitionPhase
{

    /// <summary>Supplies held and newly pressed controller bits to Samus phase handlers.</summary>
    private readonly ControllerInputState _controller1;
    /// <summary>Gates movement and animation branches during gameplay time freeze.</summary>
    private readonly GameplayTimeFreeze _freeze;
    /// <summary>Provides frame-counter cadence for movement and animation behavior.</summary>
    private readonly FrameClock _clock;
    /// <summary>Supplies player options such as configured movement controls.</summary>
    private readonly GameplayOptions _options;
    /// <summary>Owns the live Samus state and phase-specific handler selection.</summary>
    private readonly SamusFrameState _player;
    /// <summary>Provides active level data, camera, and room state for movement scans.</summary>
    private readonly ActiveRoomState _room;

    /// <summary>Creates the phase over the frame phases' collaborators.</summary>
    internal SamusPoseTransitionPhase(SamusPoseTransitionPhaseDependencies dependencies)
    {

        _controller1 = dependencies.Controller1;
        _freeze = dependencies.Freeze;
        _clock = dependencies.Clock;
        _options = dependencies.Options;
        _player = dependencies.Player;
        _room = dependencies.Room;
    }

    /// <summary>Applies post-movement pose and animation transitions after the frame's selected Samus handler.</summary>
    internal void Apply(SamusHandlerFrame frame, SamusState samus, ISamusRoomObjects roomObjects)
    {
        if (!_player.GroundedSamusMovementEnabled ||
            frame.Mode.Beta is SamusBetaHandler.Fatal or SamusBetaHandler.SuitPickup or SamusBetaHandler.ScriptLocked)
            return;

        // Hit interruption observes the old movement type before the pose update.
        // Its bomb rejection still occurs when an animation transition
        // wins pose selection; otherwise a stale request survives unmorph/morph.
        if (samus.KnockbackTimer == 0 && samus.KnockbackDirection == SuperMetroid.Core.Samus.Movement.SamusKnockbackDirection.None)
            SamusPoseMetadata.RejectUnsupportedPublishedBombJump(samus, _freeze.TimeIsFrozen);

        // The turn-transition animation cue's command-three “super-special” transition wins at this seam.
        // It publishes a turn-completion pose only if alpha did not select one
        // of its four jump exceptions. When published, it still wins here.
        // The hurt-expiry check samples hurt expiry before the pose update applies a queued
        // animation command. Preserve that decision even if Shinespark setup
        // clears the live knockback direction while installing the shinespark windup poses.
        bool hitInterruptionExpiryPending =
            SamusKnockbackMovement.IsExpiredHitInterruptionPending(samus);
        bool animationTransitionApplied =
            SamusPoseTransitions.ApplyPendingVerifiedAnimationTransition(samus);

        animationTransitionApplied = ApplyInterruptionTransitions(frame, samus, animationTransitionApplied,
            hitInterruptionExpiryPending, roomObjects);
        animationTransitionApplied = ApplyCollisionTransitions(frame, samus, animationTransitionApplied, roomObjects);

        if (!animationTransitionApplied && _player.ProspectiveSamusPose is { } inputTransition)
            ApplyInputTransition(frame, samus, inputTransition, roomObjects);
        else if (!animationTransitionApplied)
            ApplyLookupFailureCommand(frame, samus);

        // Crouching selects command two only on lookup failure. A matched
        // angle-held pose must retain boost even though movement zeroed X speed.
        if (!animationTransitionApplied && frame.UsePoseDefinitionFallback &&
            _player.ProspectiveSamusPose is null &&
            frame.MovementTypeAtFrameStart == SamusMovementType.Crouching &&
            SamusPoseMetadata.ReadMovementType(samus) == SamusMovementType.Crouching)
            samus.HorizontalSpeed.ApplyStoppedInputFallback(SamusPoseMetadata.ReadFacingDirection(samus));

        // Windup still runs the ordinary special-movement-type input lookup. Its unmatched
        // chord selects command two, just like crouching. Skipping
        // this loses the one-frame momentum cancellation before a delayed
        // midair launch. A selected launch or timeout must win instead.
        if (!animationTransitionApplied && frame.UsePoseDefinitionFallback &&
            _player.ProspectiveSamusPose is null &&
            frame.PoseAtFrameStart is SamusPoseId.ShinesparkWindupRightPose or SamusPoseId.ShinesparkWindupLeftPose &&
            samus.Shinespark.Phase == ShinesparkPhase.Windup)
            samus.HorizontalSpeed.ApplyStoppedInputFallback(SamusPoseMetadata.ReadFacingDirection(samus));

        // Falling's lookup-failure command still runs when its definition
        // retains the current pose. It follows pose initialization, and must
        // not run when a higher-priority collision or interruption won.
        if (!animationTransitionApplied && frame.UsePoseDefinitionFallback &&
            _player.ProspectiveSamusPose is null &&
            frame.MovementTypeAtFrameStart == SamusMovementType.Falling &&
            SamusPoseMetadata.ReadMovementType(samus) == SamusMovementType.Falling)
            SamusAerialPoseTransitions.ApplyFallingInputFallback(samus);

        // The normal-jump initializer publishes the fresh Shoot direction
        // for every accepted route, including aim changes within a jump. The
        // earlier projectile epilogue cleared the previous update's handoff.
        if (samus.Pose != samus.PoseHistory.PreviousPose)
            SamusAerialPoseTransitions.PublishNormalJumpPoseShotDirection(samus, _controller1.NewlyPressed);

        // The pose-history shift runs after consuming any transition slot,
        // including a self-transition. A frame with no selected transition
        // does not shift it. Keep this after final collision/pose selection.
        // The grapple launch applies its transitional
        // slot early and clears ordinary alpha input. It still reaches this
        // same history epilogue, once, after the forced launch pose.
        // X-ray setup applies interrupted-slot command five early and clears
        // ordinary alpha targets. Preserve its final history commit here.
        if (animationTransitionApplied || frame.XrayActivatedThisFrame || _player.LastGrappleMovement is { WallJumpStarted: true } ||
            _player.ProspectiveSamusPose is not null ||
            _player.ProspectiveSamusFallbackPose is not null)
            samus.CommitPoseHistory();
    }

    /// <summary>
    /// Applies the queued transitional poses and interruptions that outrank alpha's input in priority order: grapple's
    /// drop and release poses, the finished Shinespark crash, hurt expiry and a pending hit, a published bomb jump, the
    /// windup's launch pose, X-ray's activation and grapple's connection. Returns whether a transition has been applied.
    /// </summary>
    [IndependentFlag("An earlier transition this frame and a pending hit expiry are independent pose-handler states.")]
    private bool ApplyInterruptionTransitions(SamusHandlerFrame frame, SamusState samus, bool alreadyApplied,
        bool hitInterruptionExpiryPending, ISamusRoomObjects roomObjects)
    {
        bool animationTransitionApplied = alreadyApplied;
        // The animation's transition queues a transitional pose rather than replacing beta movement.
        // Commit after animation, suppressing the lower-priority input/grounding
        // pose. Its speed clears already happened before movement.
        if (!animationTransitionApplied && _player.LastGrappleMovement is { PendingDropPose: SamusPoseId dropPose })
        {
            SamusAerialPoseTransitions.ApplyGrappleDropTransition(samus, 
                _room.LevelData ?? throw new InvalidOperationException("Grapple drop requires room geometry."),
                dropPose, _clock.Counter, roomObjects, speed: GrappleDropSpeed.Kept);
            _player.ProspectiveSamusPose = null;
            _player.ProspectiveSamusFallbackPose = null;
            _player.ProspectiveSamusWallCollisionPose = null;
            animationTransitionApplied = true;
        }

        // The grapple release queues its release pose the same way, so the hit interruption
        // earlier this frame still saw the grappling movement type.
        if (!animationTransitionApplied && _player.LastGrappleMovement is { PendingReleasePose: SamusPoseId releasePose })
        {
            SamusGrappleMovement.ApplyReleasePose(samus, releasePose);
            _player.ProspectiveSamusPose = null;
            _player.ProspectiveSamusFallbackPose = null;
            _player.ProspectiveSamusWallCollisionPose = null;
            animationTransitionApplied = true;
        }

        // Crash movement queues a transitional pose; it must not replace the
        // body Samus's animation saw earlier in this frame. Command-three
        // animation transitions retain their higher priority.
        if (!animationTransitionApplied && _player.LastShinesparkMovement is { CrashSequenceFinished: true })
        {
            SamusShinesparkState.ApplyCrashFinishPose(samus);
            _player.ProspectiveSamusPose = null;
            _player.ProspectiveSamusFallbackPose = null;
            _player.ProspectiveSamusWallCollisionPose = null;
            animationTransitionApplied = true;
        }

        // Expiry still permits this frame's movement and animation. Its
        // transitional slot wins over the ordinary input selected in alpha,
        // even when a damage boost has already restored normal movement.
        // With an animation command three already pending, changes it
        // to command eight: keep the animation pose and ALSO clear hurt state.
        if (hitInterruptionExpiryPending)
        {
            SamusKnockbackMovement.FinishExpiredHitInterruption(samus);
            _player.ProspectiveSamusPose = null;
            _player.ProspectiveSamusFallbackPose = null;
            _player.ProspectiveSamusWallCollisionPose = null;
            animationTransitionApplied = true;
        }

        // The pose interruption runs after Samus's animation and before the pose update. Terrain
        // spike reactions only publish their damage request; this common interruption
        // converts that request into the same hurt pose and special movement handler
        // used by every damage producer. Animation command three retains
        // its higher pose-update priority and therefore suppresses this branch.
        if (!animationTransitionApplied &&
            SamusKnockbackMovement.TryStartPendingHitInterruption(

                samus,
                _controller1.Current,
                _freeze.TimeIsFrozen,
                _room.LevelData,
                _clock.Counter,
                roomObjects))
        {
            _player.ProspectiveSamusPose = null;
            _player.ProspectiveSamusFallbackPose = null;
            _player.ProspectiveSamusWallCollisionPose = null;
            animationTransitionApplied = true;
        }

        // Bomb overlap is published between alpha and beta, but the game's
        // interruption dispatcher considers it only AFTER movement/animation.
        // Active hurt values retain the request without arming or rejecting it;
        // expiry owns this frame even after clearing those values. This permits
        // a later bomb launch to retain the damaged pose, with input locked by
        // special command three rather than an early speculative pose change.
        if (!animationTransitionApplied && samus.KnockbackTimer == 0 &&
            samus.KnockbackDirection == SuperMetroid.Core.Samus.Movement.SamusKnockbackDirection.None &&
            SamusPoseMetadata.TrySetupPublishedBombJump(samus, 

                _room.LevelData ?? throw new InvalidOperationException(
                    "A published bomb-jump direction requires active room level data."),
                _freeze.TimeIsFrozen,
                _clock.Counter,
                _controller1.NewlyPressed,
                roomObjects))
        {
            _player.ProspectiveSamusPose = null;
            _player.ProspectiveSamusFallbackPose = null;
            _player.ProspectiveSamusWallCollisionPose = null;
            animationTransitionApplied = true;
        }

        // Windup expiry installs vertical movement immediately, but publishes
        // an interrupted pose for this later phase. Samus's animation must still see
        // the windup body; otherwise the launch animation loses its first tick.
        if (!animationTransitionApplied &&
            _player.LastShinesparkMovement is { PendingLaunchPose: SamusPoseId launchPose })
        {
            SamusShinesparkState.ApplyDirectionalLaunchPose(samus, launchPose);
            _player.ProspectiveSamusPose = null;
            _player.ProspectiveSamusFallbackPose = null;
            _player.ProspectiveSamusWallCollisionPose = null;
            animationTransitionApplied = true;
        }

        if (samus.Xray.CommitPendingActivation(samus, animationTransitionApplied))
        {
            _player.ProspectiveSamusPose = null;
            _player.ProspectiveSamusFallbackPose = null;
            _player.ProspectiveSamusWallCollisionPose = null;
            animationTransitionApplied = true;
        }

        // Grapple connection publishes a special prospective command.
        // Super-special animation/hurt owners above can suppress it without
        // undoing the already-installed beam function. This is the
        // ordering used by bounded Grapple momentum-retention setups.
        if (!animationTransitionApplied &&
            _player.LastGrappleMovement is { PendingConnection: { } pendingConnection })
        {
            Position16<RoomPixels> grapplePrevious =
                SamusGrappleMovement.ApplyPendingConnectionPose(
                    samus,
                    pendingConnection);
            frame.SnapCameraPoint(grapplePrevious);
            _player.ProspectiveSamusPose = null;
            _player.ProspectiveSamusFallbackPose = null;
            _player.ProspectiveSamusWallCollisionPose = null;
            animationTransitionApplied = true;
        }

        return animationTransitionApplied;
    }

    /// <summary>
    /// Applies the collision results that replace alpha's input target: ceilings, the bomb arc's fall, running into a
    /// wall, Morph Ball, wall-jump and Spring Ball landings, ordinary landings and walking off a floor. Returns whether
    /// a transition has been applied.
    /// </summary>
    private bool ApplyCollisionTransitions(SamusHandlerFrame frame, SamusState samus, bool alreadyApplied, ISamusRoomObjects roomObjects)
    {
        bool animationTransitionApplied = alreadyApplied;
        // The ceiling collision replaces the ordinary input target with the current pose
        // on ceiling collision. Apply the ball's deferred EFDF vertical
        // stop and consume this same-pose transition so history still shifts
        // and an alpha aim/turn selection cannot override it. Higher-priority
        // animation and hurt interruptions above retain their priority.
        // Ball movement can resume before the bomb arc reaches a ceiling;
        // its collision must also outrank the stop-rolling input fallback.
        if (!animationTransitionApplied &&
            (_player.LastAerialSamusMovement is { HitCeiling: true } ||
             _player.LastMorphBallMovement is { HitCeiling: true } ||
             _player.LastBombJumpMovement is { Vertical.Collided: true }))
        {
            if (_player.LastMorphBallMovement is { HitCeiling: true })
                SamusPosturePoseTransitions.ApplySolidCeilingCollision(samus);
            _player.ProspectiveSamusPose = null;
            _player.ProspectiveSamusFallbackPose = null;
            animationTransitionApplied = true;
        }

        // The bomb-arc check overrides the input target when the bomb arc moved down through
        // its apex with no floor below; the airborne pose is installed this frame.
        if (!animationTransitionApplied &&
            _player.LastBombJumpMovement is { FellWithoutFloor: true })
        {
            SamusPosturePoseTransitions.ApplyBombJumpFallingPose(samus);
            _player.ProspectiveSamusPose = null;
            _player.ProspectiveSamusFallbackPose = null;
            animationTransitionApplied = true;
        }

        // The wall-stop check runs inside the pose update after beta movement/animation and
        // only when no super-special animation command has already won. Its first
        // branch consumes the X-speed-killed flag produced by CURRENT type-one
        // running movement. This creates a wall-stop pose even with no input match.
        // An unobstructed upward platform carry leaves the prospective input
        // pose intact too. Requiring a vertical collision here wrongly skipped
        // the one-pixel run check while a Kamer was carrying Samus upward.
        // A special movement handler can also retain a standing/running pose
        // (suit-interrupted windup). The wall-stop check does not require normal grounded beta.
        if (!animationTransitionApplied &&
            _player.LastGroundedSamusMovement is not
                { Vertical.IsUnobstructedDownwardMovement: true })
        {
            bool currentRunHitWall =
                (SamusPoseMetadata.IsRightFacingRunningPose(frame.PoseAtFrameStart) ||
                 SamusPoseMetadata.IsLeftFacingRunningPose(frame.PoseAtFrameStart)) &&
                _player.LastGroundedSamusMovement is { Horizontal.Collided: true };
            SamusPoseId? prospectiveRunningPose =
                _player.ProspectiveSamusPose is { } prospective
                    ? prospective.ProspectivePose
                    // Alpha writes the same prospective-pose slot for a
                    // table match and a no-input fallback. A decelerating run
                    // retains its running pose and must still take EADE's real
                    // one-pixel forward move; omitting it shortens release drift.
                    : _player.ProspectiveSamusFallbackPose;
            _player.ProspectiveSamusWallCollisionPose =
                SamusGroundedPoseTransitions.CheckProspectiveRunningPoseForWall(samus, 

                    _room.LevelData ?? throw new InvalidOperationException(
                        "Ran-into-wall probe requires active room level data."),
                    prospectiveRunningPose,
                    currentRunHitWall,
                    out _,
                    roomObjects);
        }

        if (!animationTransitionApplied &&
            _player.ProspectiveSamusWallCollisionPose is { } wallCollisionPose)
        {
            // The selector result replaces the ordinary input target. Applying it
            // here also covers the no-input “running speed was killed” branch.
            SamusGroundedPoseTransitions.ApplyRanIntoWallPoseChange(samus, wallCollisionPose);
            animationTransitionApplied = true;
            // The wall-stop check replaces only ProspectivePose. When alpha's table lookup
            // failed, the pose update still runs the command
            // selected for the pre-wall movement type.
            if (_player.ProspectiveSamusPose is null && _player.ProspectiveSamusFallbackPose is not null)
            {
                SamusProspectivePoseChangeCommand command = SamusPoseChangeCommands.ForLookupFailure(
                    SamusPoseMetadata.ReadMovementType(frame.PoseAtFrameStart));
                if (command is not (SamusProspectivePoseChangeCommand.Decelerate or SamusProspectivePoseChangeCommand.Stop))
                    throw new InvalidDataException(
                        $"Ran-into-wall source {frame.PoseAtFrameStart} selected unexpected lookup-failure command {command}.");
                samus.HorizontalSpeed.ApplyDeceleratingInputFallback(
                    command == SamusProspectivePoseChangeCommand.Decelerate && frame.DeceleratingFallbackHasMomentum,
                    SamusPoseMetadata.ReadFacingDirection(samus));
            }
        }

        // Morph landing has its own prospective-pose and collision tables. A hard
        // impact retains the falling morph ball poses and launches bounce one; the next collision
        // launches bounce two; only a gentle/second-bounce collision installs
        // the grounded morph ball poses. Treat every branch as a consumed transition so input
        // selected before collision cannot override the game's bounce result.
        // The collision table ignores ground-family type four, even if hurt
        // movement left that pose with nonzero Y speed. Do not infer an airborne
        // landing dispatcher from velocity alone.
        if (!animationTransitionApplied &&
            _player.LastMorphBallMovement is { Landed: true } &&
            SamusPoseMetadata.IsAirborneMorphBallPose(frame.PoseAtFrameStart))
        {
            SamusPosturePoseTransitions.ApplyMorphBallLanding(samus);
            animationTransitionApplied = true;
        }

        // The wall-jump check publishes solid-vertical collision result five and reports
        // a collision, preventing vertical motion. The matching command
        // installs the wall-jump pose, clears old momentum, and launches from fixed constants.
        if (!animationTransitionApplied &&
            _player.LastAerialSamusMovement is { WallJumpTriggered: true })
        {
            SamusAerialPoseTransitions.ApplyWallJumpTrigger(samus, _room.LevelData, _clock.Counter, roomObjects);
            animationTransitionApplied = true;
        }

        // Spring Ball's three movement families share collision result three, but
        // the Spring Ball handler makes held Jump an immediate relaunch and stores the jump intent
        // during automatic rebounds. Keep it distinct from ordinary-ball state.
        // The grounded Spring Ball movement type likewise has the table's no-transition entry.
        if (!animationTransitionApplied &&
            _player.LastMorphBallMovement is { Landed: true } &&
            SamusPoseMetadata.IsAirborneSpringBallPose(frame.PoseAtFrameStart))
        {
            SamusPosturePoseTransitions.ApplySpringBallLanding(samus, _controller1.Current);
            animationTransitionApplied = true;
        }

        // A downward collision publishes result one. The landing check chooses normal or
        // spin landing from the movement type that executed this frame, then pose
        // command five clears both velocity axes. This ordinary prospective pose
        // is lower priority than an animation command-three transition above.
        if (!animationTransitionApplied &&
            _player.LastAerialSamusMovement is { Landed: true } &&
            !SamusPoseMetadata.IsAerialTurnPose(frame.PoseAtFrameStart))
        {
            if (SamusPoseMetadata.IsCompactAerialPose(frame.PoseAtFrameStart))
            {
                SamusPoseTransitions.TryApplyCompactAerialLanding(samus, 

                    _room.LevelData ?? throw new InvalidOperationException(
                        "Compact landing requires active room level data."),
                    _clock.Counter,
                    roomObjects);
            }
            else
            {
                bool wasSpinning = SamusPoseMetadata.IsSpinJumpPose(frame.PoseAtFrameStart) ||
                    SamusPoseMetadata.IsWallJumpPose(frame.PoseAtFrameStart);
                SamusPoseTransitions.TryApplyAerialLanding(samus, 

                    _room.LevelData ?? throw new InvalidOperationException(
                        "Aerial landing requires active room level data."),
                    wasSpinning,
                    _controller1.Current,
                    _clock.Counter,
                    roomObjects);
            }
            animationTransitionApplied = true;
        }

        // An unobstructed DOWNWARD grounding probe publishes result two. An
        // unobstructed upward platform carry publishes result zero
        // and must leave the pose alone; treating every non-collision as result two
        // made rising Kamers repeatedly alternate falling and landing. For admitted
        // standing (including landing art), running, and stable crouching types,
        // data zero
        // selects airborne family zero and the falling check chooses falling art.
        // The ground and jumping turn types contain the no-pose-change entry in that literal table;
        // they finish their turn transition, then the destination pose detects the missing floor.
        if (!animationTransitionApplied &&
            _player.LastGroundedSamusMovement is
                { Vertical.IsUnobstructedDownwardMovement: true } &&
            (SamusPoseMetadata.IsRightFacingStandingPose(frame.PoseAtBetaMovement) ||
             SamusPoseMetadata.IsLeftFacingStandingPose(frame.PoseAtBetaMovement) ||
             SamusPoseMetadata.IsRightFacingLandingPose(frame.PoseAtBetaMovement) ||
             SamusPoseMetadata.IsLeftFacingLandingPose(frame.PoseAtBetaMovement) ||
             SamusPoseMetadata.IsRightFacingRunningPose(frame.PoseAtBetaMovement) ||
             SamusPoseMetadata.IsLeftFacingRunningPose(frame.PoseAtBetaMovement) ||
             SamusPoseMetadata.IsMoonwalkingPose(frame.PoseAtBetaMovement) ||
             frame.PoseAtBetaMovement is SamusPoseId.KnockbackRightPose or SamusPoseId.KnockbackLeftPose ||
             SamusPoseMetadata.IsRanIntoWallPose(frame.PoseAtBetaMovement) ||
             SamusPoseMetadata.IsRightFacingCrouchingPose(frame.PoseAtBetaMovement) ||
             SamusPoseMetadata.IsLeftFacingCrouchingPose(frame.PoseAtBetaMovement)))
        {
            SamusPoseId fallingPose = SamusPoseTransitions.SelectFallingPoseForCurrentAim(samus);
            SamusPoseTransitions.ApplyWalkedOffFloorTransition(samus, 
                _room.LevelData ?? throw new InvalidOperationException("Walk-off requires room collision data."),
                fallingPose, _clock.Counter, roomObjects);
            animationTransitionApplied = true;
        }

        // Movement type four publishes the ordinary airborne morph ball poses when its
        // no-speed grounding probe finds no floor. The shared rolling animation is
        // preserved, while Y speed starts at zero/down exactly like the falling check.
        if (!animationTransitionApplied &&
            _player.LastMorphBallMovement is
                { Vertical.IsUnobstructedDownwardMovement: true } &&
            (SamusPoseMetadata.IsGroundedMorphBallPose(frame.PoseAtFrameStart) ||
             SamusPoseMetadata.IsGroundedSpringBallPose(frame.PoseAtFrameStart)))
        {
            SamusPosturePoseTransitions.ApplyMorphBallWalkOff(samus);
            animationTransitionApplied = true;
        }

        return animationTransitionApplied;
    }

    /// <summary>
    /// Applies the input transition alpha selected from the pose table, through the family initializer that owns the
    /// source and target pair. Self-transitions in the turn tables mean "remain in this animation".
    /// </summary>
    private void ApplyInputTransition(SamusHandlerFrame frame, SamusState samus, SamusPoseTransition inputTransition, ISamusRoomObjects roomObjects)
    {
        SamusPoseId targetPose = unchecked(inputTransition.ProspectivePose);

        // Reinitializing frame zero would prevent the turn transition from ever being reached.
        if (targetPose == frame.PoseAtFrameStart)
            return;
        if (!TryApplySpecialBodyTransition(frame, samus, targetPose, roomObjects) && !TryApplyAimRunOrTurnTransition(frame, samus, targetPose))
            ApplyJumpCrouchOrPostureTransition(frame, samus, targetPose, roomObjects);
    }

    /// <summary>
    /// Applies a Draygon-grab, moonwalk, damage-boost, aerial-turn, spin, Morph Ball or compact aerial transition.
    /// Returns false when the pair belongs to a later family.
    /// </summary>
    private bool TryApplySpecialBodyTransition(SamusHandlerFrame frame, SamusState samus, SamusPoseId targetPose, ISamusRoomObjects roomObjects)
    {
        switch (new SamusPoseChange(frame.PoseAtFrameStart, targetPose))
        {
            case var (source, target)
                when SamusPoseMetadata.IsDraygonGrabbedPose(source) &&
                     SamusPoseMetadata.IsDraygonGrabbedPose(target):
                // Both transition tables stay within one facing. The helper makes
                // that invariant explicit and initializes the target's real
                // animation.
                SamusPoseMetadata.ApplyDraygonGrabbedPoseChange(samus, 
                    targetPose);
                return true;
            case var (source, target)
                when (((SamusPoseMetadata.IsRightFacingStandingPose(source) ||
                        SamusPoseMetadata.IsRightFacingRanIntoWallPose(source) ||
                        SamusPoseMetadata.IsRightFacingLandingPose(source)) &&
                       SamusPoseMetadata.IsMoonwalkingFacingRightPose(target)) ||
                      ((SamusPoseMetadata.IsLeftFacingStandingPose(source) ||
                        SamusPoseMetadata.IsLeftFacingRanIntoWallPose(source) ||
                        SamusPoseMetadata.IsLeftFacingLandingPose(source)) &&
                       SamusPoseMetadata.IsMoonwalkingFacingLeftPose(target)) ||
                      (SamusPoseMetadata.IsMoonwalkingPose(source) &&
                       (SamusPoseMetadata.IsMoonwalkingPose(target) ||
                        target is SamusPoseId.MovingRightNormalPose or
                            SamusPoseId.MovingLeftNormalPose))):
                SamusGroundedPoseTransitions.ApplyMoonwalkPoseChange(samus, 
                    targetPose,
                    _options.MoonwalkEnabled);
                return true;
            case var (source, target)
                when SamusPoseMetadata.IsMoonwalkingPose(source) &&
                     SamusPoseMetadata.IsMoonwalkTurnJumpPose(target):
                // The stable moonwalk poses' table publishes a generic
                // reversal target. The reversal check validates it against the old
                // shot direction, folds momentum, and leaves the moonwalk turn-jump poses
                // grounded until either its input table or the turn transition starts the jump.
                SamusGroundedPoseTransitions.ApplyMoonwalkTurnJump(samus, targetPose);
                return true;
            case var (source, target)
                when SamusPoseMetadata.IsMoonwalkTurnJumpLeftPose(source) &&
                         target is SamusPoseId.SpinJumpLeftPose or
                             SamusPoseId.NeutralJumpTransitionLeftPose ||
                     SamusPoseMetadata.IsMoonwalkTurnJumpRightPose(source) &&
                         target is SamusPoseId.SpinJumpRightPose or
                             SamusPoseId.NeutralJumpTransitionRightPose:
                SamusAerialPoseTransitions.ApplyOrdinaryJumpTransition(samus, 
                    targetPose,
                    _controller1.NewlyPressed);
                return true;
            case (SamusPoseId.KnockbackRightPose, SamusPoseId.DamageBoostRightPose):
            case (SamusPoseId.KnockbackLeftPose, SamusPoseId.DamageBoostLeftPose):
                // The accepted pose restores normal movement while
                // preserving the hurt arc and remaining timer.
                SamusKnockbackMovement.ApplyDamageBoostTransition(
                    samus,
                    frame.PoseAtFrameStart,
                    targetPose);
                return true;
            case (SamusPoseId.DamageBoostRightPose,
                  SamusPoseId.NeutralJumpRightPose or SamusPoseId.NormalJumpForwardRightPose):
            case (SamusPoseId.DamageBoostLeftPose,
                  SamusPoseId.NeutralJumpLeftPose or SamusPoseId.NormalJumpForwardLeftPose):
                SamusKnockbackMovement.ApplyDamageBoostPoseTransition(
                    samus,
                    targetPose,
                    _controller1.NewlyPressed);
                return true;
            case var (source, target)
                when ((SamusPoseMetadata.IsRightFacingNormalJumpPose(source) &&
                       target == SamusPoseId.TurningRightToLeftJumpPose) ||
                      (SamusPoseMetadata.IsLeftFacingNormalJumpPose(source) &&
                       target == SamusPoseId.TurningLeftToRightJumpPose) ||
                      (SamusPoseMetadata.IsRightFacingFallingPose(source) &&
                       target == SamusPoseId.TurningRightToLeftFallingPose) ||
                      (SamusPoseMetadata.IsLeftFacingFallingPose(source) &&
                       target == SamusPoseId.TurningLeftToRightFallingPose)):
                // The generic jumping and falling turns are table outputs. The helper
                // reads the source shot-direction record, chooses the exact
                // aimed aerial turn art when needed, folds momentum, and runs compact
                // pose-expansion collision before committing the turn.
                SamusAerialPoseTransitions.TryApplyAerialTurn(samus, 

                    _room.LevelData ?? throw new InvalidOperationException(
                        "Aerial turn requires active room level data."),
                    target,
                    _clock.Counter,
                    roomObjects);
                return true;
            case var (source, target)
                when (SamusPoseMetadata.IsSpinJumpPose(source) ||
                      SamusPoseMetadata.IsWallJumpPose(source)) &&
                     SamusPoseMetadata.IsSpinJumpPose(target):
                SamusAerialPoseTransitions.ApplySpinJumpDirectionTransition(samus, targetPose);
                return true;
            case var (source, target)
                when SamusPoseMetadata.IsStableBallPose(source) &&
                     SamusPoseMetadata.IsStableBallPose(target) &&
                     !(SamusPoseMetadata.IsGroundedSpringBallPose(source) &&
                       target is SamusPoseId.SpringBallJumpRightPose or
                           SamusPoseId.SpringBallJumpLeftPose):
                // These six poses all share one animation.
                // The initializer preserves frame/timer and applies mode-one
                // reversal momentum only when direction actually changes.
                SamusPosturePoseTransitions.ApplyMorphBallPoseChange(samus, target);
                return true;
            case var (source, target)
                when SamusPoseMetadata.IsGroundedSpringBallPose(source) &&
                     target is SamusPoseId.SpringBallJumpRightPose or
                         SamusPoseId.SpringBallJumpLeftPose:
                SamusPosturePoseTransitions.ApplySpringBallJump(samus, target);
                return true;
            case var (source, target)
                when ((SamusPoseMetadata.ReadFacingDirection(source) == SamusFacingDirection.Right &&
                       target == SamusPoseId.MorphingTransitionRightPose) ||
                      (SamusPoseMetadata.ReadFacingDirection(source) == SamusFacingDirection.Left &&
                       target == SamusPoseId.MorphingTransitionLeftPose) ||
                      (SamusPoseMetadata.IsStableBallPose(source) &&
                       target is SamusPoseId.UnmorphingTransitionRightPose or
                           SamusPoseId.UnmorphingTransitionLeftPose)):
                SamusPosturePoseTransitions.TryApplyMorphTransition(samus, 

                    _room.LevelData ?? throw new InvalidOperationException(
                        "Morph transition requires active room level data."),
                    targetPose,
                    _clock.Counter,
                    roomObjects);
                return true;
            case var (source, target)
                when (SamusPoseMetadata.IsCompactAerialPose(source) ||
                      SamusPoseMetadata.IsCompactAerialPose(target)) &&
                     ((SamusPoseMetadata.IsRightFacingNormalJumpPose(source) &&
                       SamusPoseMetadata.IsRightFacingNormalJumpPose(target)) ||
                      (SamusPoseMetadata.IsLeftFacingNormalJumpPose(source) &&
                       SamusPoseMetadata.IsLeftFacingNormalJumpPose(target)) ||
                      (SamusPoseMetadata.IsRightFacingFallingPose(source) &&
                       SamusPoseMetadata.IsRightFacingFallingPose(target)) ||
                      (SamusPoseMetadata.IsLeftFacingFallingPose(source) &&
                       SamusPoseMetadata.IsLeftFacingFallingPose(target))):
                SamusAerialPoseTransitions.TryApplyCompactAerialTransition(samus, 

                    _room.LevelData ?? throw new InvalidOperationException(
                        "Compact aerial transition requires active room level data."),
                    targetPose,
                    _clock.Counter,
                    roomObjects);
                return true;
            default:
                return false;
        }
    }

    /// <summary>
    /// Applies a grounded or aerial aim change, a standing or wall-to-running start, or a grounded turn. Returns false
    /// when the pair belongs to a later family.
    /// </summary>
    private bool TryApplyAimRunOrTurnTransition(SamusHandlerFrame frame, SamusState samus, SamusPoseId targetPose)
    {
        switch (new SamusPoseChange(frame.PoseAtFrameStart, targetPose))
        {
            case var (source, target)
                when (SamusPoseMetadata.IsGroundedAimPose(source) ||
                      SamusPoseMetadata.IsGroundedAimPose(target) ||
                      SamusPoseMetadata.IsGunExtendedPose(source) ||
                      SamusPoseMetadata.IsGunExtendedPose(target)) &&
                     (((SamusPoseMetadata.IsRightFacingStandingPose(source) ||
                        SamusPoseMetadata.IsRightFacingRunningPose(source) ||
                        SamusPoseMetadata.IsRightFacingRanIntoWallPose(source) ||
                        SamusPoseMetadata.IsRightFacingLandingPose(source)) &&
                       (SamusPoseMetadata.IsRightFacingStandingPose(target) ||
                        SamusPoseMetadata.IsRightFacingRunningPose(target) ||
                        SamusPoseMetadata.IsRightFacingRanIntoWallPose(target))) ||
                      ((SamusPoseMetadata.IsLeftFacingStandingPose(source) ||
                        SamusPoseMetadata.IsLeftFacingRunningPose(source) ||
                        SamusPoseMetadata.IsLeftFacingRanIntoWallPose(source) ||
                        SamusPoseMetadata.IsLeftFacingLandingPose(source)) &&
                       (SamusPoseMetadata.IsLeftFacingStandingPose(target) ||
                        SamusPoseMetadata.IsLeftFacingRunningPose(target) ||
                        SamusPoseMetadata.IsLeftFacingRanIntoWallPose(target))) ||
                      (SamusPoseMetadata.IsRightFacingCrouchingPose(source) &&
                       SamusPoseMetadata.IsRightFacingCrouchingPose(target)) ||
                      (SamusPoseMetadata.IsLeftFacingCrouchingPose(source) &&
                       SamusPoseMetadata.IsLeftFacingCrouchingPose(target))):
                SamusGroundedPoseTransitions.ApplyGroundedAimTransition(samus, targetPose);
                return true;
            case var (source, target)
                when SamusPoseMetadata.IsSameFacingAerialAimFireOrForwardTransition(
                    source,
                    target):
                SamusAerialPoseTransitions.ApplyAerialAimTransition(samus, targetPose);
                return true;
            case (SamusPoseId.FacingRightNormalPose, SamusPoseId.MovingRightNormalPose):
                SamusGroundedPoseTransitions.ApplyStandingRightToRunningRight(samus);
                return true;
            case (SamusPoseId.FacingLeftNormalPose, SamusPoseId.MovingLeftNormalPose):
                SamusGroundedPoseTransitions.ApplyStandingLeftToRunningLeft(samus);
                return true;
            case var (source, target)
                when SamusPoseMetadata.IsRanIntoWallPose(source) &&
                     (SamusPoseMetadata.IsRightFacingRunningPose(target) ||
                      SamusPoseMetadata.IsLeftFacingRunningPose(target)):
                SamusGroundedPoseTransitions.ApplyRanIntoWallToRunning(samus, target);
                return true;
            case var (forwardSource, forwardTarget)
                when SamusPoseMetadata.IsForwardFacingPose(forwardSource) &&
                     forwardTarget is
                         SamusPoseId.TurningRightToLeftPose or
                         SamusPoseId.TurningLeftToRightPose:
                // Ceres releases controls while the forward-facing pose is still active.
                // Its own transition table maps Left/Right to generic
                // turning poses; the reversal check recognizes the old front-facing pose and
                // preserves that target without inventing a source facing.
                SamusGroundedPoseTransitions.ApplyGroundedTurn(samus, targetPose, _controller1.Current);
                return true;
            case var (rightSource, rightTarget)
                when rightTarget is
                         SamusPoseId.TurningRightToLeftPose or
                         SamusPoseId.TurningRightToLeftCrouchingPose &&
                     (SamusPoseMetadata.IsRightFacingStandingPose(rightSource) ||
                      SamusPoseMetadata.IsRightFacingRunningPose(rightSource) ||
                      SamusPoseMetadata.IsMoonwalkingFacingRightPose(rightSource) ||
                      SamusPoseMetadata.IsRightFacingRanIntoWallPose(rightSource) ||
                      SamusPoseMetadata.IsRightFacingCrouchingPose(rightSource) ||
                      SamusPoseMetadata.IsRightFacingLandingPose(rightSource)):
            case var (leftSource, leftTarget)
                when leftTarget is
                         SamusPoseId.TurningLeftToRightPose or
                         SamusPoseId.TurningLeftToRightCrouchingPose &&
                     (SamusPoseMetadata.IsLeftFacingStandingPose(leftSource) ||
                      SamusPoseMetadata.IsLeftFacingRunningPose(leftSource) ||
                      SamusPoseMetadata.IsMoonwalkingFacingLeftPose(leftSource) ||
                      SamusPoseMetadata.IsLeftFacingRanIntoWallPose(leftSource) ||
                      SamusPoseMetadata.IsLeftFacingCrouchingPose(leftSource) ||
                      SamusPoseMetadata.IsLeftFacingLandingPose(leftSource)):
                SamusGroundedPoseTransitions.ApplyGroundedTurn(samus, targetPose, _controller1.Current);
                return true;
            default:
                return false;
        }
    }

    /// <summary>
    /// Applies a jump, crouch, unmorph, landing, posture or Shinespark direction transition. Every active retail pair
    /// is owned by this or an earlier family; any other pair means the pose table or state is corrupt.
    /// </summary>
    private void ApplyJumpCrouchOrPostureTransition(SamusHandlerFrame frame, SamusState samus, SamusPoseId targetPose, ISamusRoomObjects roomObjects)
    {
        switch (new SamusPoseChange(frame.PoseAtFrameStart, targetPose))
        {
            case var (source, target)
                when ((SamusPoseMetadata.IsRightFacingStandingPose(source) ||
                       SamusPoseMetadata.IsRightFacingRanIntoWallPose(source)) &&
                      SamusPoseMetadata.IsRightFacingNormalJumpPose(target) &&
                      target is
                          SamusPoseId.NeutralJumpTransitionRightPose or
                          SamusPoseId.NormalJumpTransitionAimUpRightPose or
                          SamusPoseId.NormalJumpTransitionAimDiagonalUpRightPose or
                          SamusPoseId.NormalJumpTransitionAimDiagonalDownRightPose) ||
                     ((SamusPoseMetadata.IsLeftFacingStandingPose(source) ||
                       SamusPoseMetadata.IsLeftFacingRanIntoWallPose(source)) &&
                      SamusPoseMetadata.IsLeftFacingNormalJumpPose(target) &&
                      target is
                          SamusPoseId.NeutralJumpTransitionLeftPose or
                          SamusPoseId.NormalJumpTransitionAimUpLeftPose or
                          SamusPoseId.NormalJumpTransitionAimDiagonalUpLeftPose or
                          SamusPoseId.NormalJumpTransitionAimDiagonalDownLeftPose) ||
                     SamusPoseMetadata.IsLandingToNormalJumpTransition(source, target):
                // All the landing records expose the same
                // standing input table while their brief streams are active.
                // A fresh Jump edge can therefore preserve any authored
                // normal-jump aim target, even from the firing landings
                // before the turn-transition fallback.
            case (SamusPoseId.MovingRightNormalPose,
                  SamusPoseId.SpinJumpRightPose):
            case (SamusPoseId.MovingRightGunExtendedPose,
                  SamusPoseId.SpinJumpRightPose):
            case (SamusPoseId.RunningAimUpRightPose or
                  SamusPoseId.RunningAimDiagonalUpRightPose or
                  SamusPoseId.RunningAimDiagonalDownRightPose,
                  SamusPoseId.SpinJumpRightPose):
            case (SamusPoseId.MovingLeftNormalPose,
                  SamusPoseId.SpinJumpLeftPose):
            case (SamusPoseId.MovingLeftGunExtendedPose,
                  SamusPoseId.SpinJumpLeftPose):
            case (SamusPoseId.RunningAimUpLeftPose or
                  SamusPoseId.RunningAimDiagonalUpLeftPose or
                  SamusPoseId.RunningAimDiagonalDownLeftPose,
                  SamusPoseId.SpinJumpLeftPose):
            case var (turnSourcePose, turnTargetPose)
                when SamusPoseMetadata.IsStandingGroundTurnToJumpTransition(
                    turnSourcePose,
                    turnTargetPose):
                // Every standing turn on the ground—including the aimed turns
                // and moonwalk records—continues consulting the standing transition table.
                // That table can interrupt the art with either same-facing
                // spin or neutral jump. Preserve newly pressed Shoot as well:
                // the normal-jump initializer owns the simultaneous
                // Jump+Fire bridge when the selected body is type two.
                SamusAerialPoseTransitions.ApplyOrdinaryJumpTransition(samus, 
                    targetPose,
                    _controller1.NewlyPressed);
                break;
            case var (source, target)
                when ((SamusPoseMetadata.IsRightFacingCrouchingPose(source) &&
                       target == SamusPoseId.NeutralJumpTransitionRightPose) ||
                      (SamusPoseMetadata.IsLeftFacingCrouchingPose(source) &&
                       target == SamusPoseId.NeutralJumpTransitionLeftPose)):
                // The crouched aim records use the ordinary neutral-jump transition
                // art for every crouched aim direction. The helper preserves
                // the crouching-turn poses' literal ten-pixel adjustment.
                SamusPosturePoseTransitions.TryApplyCrouchJumpTransition(samus, 

                    _room.LevelData ?? throw new InvalidOperationException(
                        "Crouch jump requires active room level data."),
                    targetPose,
                    _clock.Counter,
                    roomObjects);
                break;
            case var (source, target)
                when (SamusPoseMetadata.IsSpinJumpPose(source) ||
                      SamusPoseMetadata.IsWallJumpPose(source)) &&
                     (SamusPoseMetadata.IsRightFacingNormalJumpPose(target) ||
                      SamusPoseMetadata.IsLeftFacingNormalJumpPose(target)):
                // Aim and Fire cancel both compact spin and wall-
                // jump bodies through the same normal-jump initializer without
                // restarting the jump arc. The wall-jump Shot record, for example, maps Shot
                // directly from the right wall jump to the right gun-extended normal jump. The shared helper performs
                // changed-pose collision for every radius expansion; a rejected
                // body leaves the compact source pose installed.
                SamusAerialPoseTransitions.TryApplySpinOrWallJumpToNormalJumpTransition(samus, 

                    _room.LevelData ?? throw new InvalidOperationException(
                        "Spin-to-normal-jump transition requires active room level data."),
                    targetPose,
                    _clock.Counter,
                    _controller1.NewlyPressed,
                    roomObjects);
                break;
            case var (source, target)
                when (source == SamusPoseId.UnmorphingTransitionRightPose &&
                      SamusPoseMetadata.IsRightFacingFallingPose(target)) ||
                     (source == SamusPoseId.UnmorphingTransitionLeftPose &&
                      SamusPoseMetadata.IsLeftFacingFallingPose(target)):
                // Route an animation/collision-selected falling body through
                // ordinary expansion collision while preserving the fall.
                // Posture transitions do not poll controller pose tables:
                // the posture-transition movement type's input handler returns before lookup.
                SamusPosturePoseTransitions.TryApplyUnmorphToFallingTransition(samus, 

                    _room.LevelData ?? throw new InvalidOperationException(
                        "Unmorph-to-falling transition requires active room level data."),
                    targetPose,
                    _clock.Counter,
                    roomObjects);
                break;
            case var (source, target)
                when ((SamusPoseMetadata.IsRightFacingCrouchingPose(source) &&
                       target == SamusPoseId.FacingRightNormalPose) ||
                      (SamusPoseMetadata.IsLeftFacingCrouchingPose(source) &&
                       target == SamusPoseId.FacingLeftNormalPose)):
                // These direct facing-normal records bypass the animated
                // aimed standing-transition family, but still run pose-expansion
                // collision before installing the final standing body.
                SamusPosturePoseTransitions.TryApplyDirectCrouchToStandingTransition(samus, 

                    _room.LevelData ?? throw new InvalidOperationException(
                        "Direct crouch exit requires active room level data."),
                    targetPose,
                    _clock.Counter,
                    roomObjects);
                break;
            case var (landingSource, landingTarget)
                when (landingTarget == SamusPoseId.MovingRightNormalPose &&
                      SamusPoseMetadata.IsRightFacingLandingPose(landingSource)) ||
                     (landingTarget == SamusPoseId.MovingLeftNormalPose &&
                      SamusPoseMetadata.IsLeftFacingLandingPose(landingSource)):
                SamusGroundedPoseTransitions.ApplyLandingToRunning(samus, targetPose);
                break;
            case var (source, postureTarget)
                when SamusPoseMetadata.IsCrouchStandTransitionPose(postureTarget) &&
                     (SamusPoseMetadata.IsRightFacingStandingPose(source) ||
                      SamusPoseMetadata.IsLeftFacingStandingPose(source) ||
                      SamusPoseMetadata.IsRightFacingRunningPose(source) ||
                      SamusPoseMetadata.IsLeftFacingRunningPose(source) ||
                      SamusPoseMetadata.IsMoonwalkingPose(source) ||
                      SamusPoseMetadata.IsRanIntoWallPose(source) ||
                      SamusPoseMetadata.IsRightFacingLandingPose(source) ||
                      SamusPoseMetadata.IsLeftFacingLandingPose(source) ||
                      SamusPoseMetadata.IsRightFacingCrouchingPose(source) ||
                      SamusPoseMetadata.IsLeftFacingCrouchingPose(source)):
                SamusPosturePoseTransitions.TryApplyPostureTransition(samus, 

                    _room.LevelData ?? throw new InvalidOperationException(
                        "Posture transition requires active room level data."),
                    targetPose,
                    _clock.Counter,
                    roomObjects);
                break;
            case var (source, sparkTarget)
                when source is
                         SamusPoseId.ShinesparkWindupRightPose or
                         SamusPoseId.ShinesparkWindupLeftPose &&
                     sparkTarget is
                         SamusPoseId.ShinesparkHorizontalRightPose or
                         SamusPoseId.ShinesparkHorizontalLeftPose or
                         SamusPoseId.ShinesparkVerticalRightPose or
                         SamusPoseId.ShinesparkVerticalLeftPose or
                         SamusPoseId.ShinesparkDiagonalRightPose or
                         SamusPoseId.ShinesparkDiagonalLeftPose:
                // These are ordinary held/new input records, but
                // their handler installs a special movement handler instead of
                // invoking a normal movement-type initializer.
                SamusAerialPoseTransitions.ApplyShinesparkDirectionTransition(samus, 
                    targetPose);
                break;
            default:
                // The movement-coverage check exhaustively audits every active retail
                // transition pair entering this dispatcher. An authored match not
                // owned by one of the explicit families above means the pose
                // table or state was mixed from an incompatible revision.
                throw new InvalidDataException(
                    $"Grounded input transition {frame.PoseAtFrameStart} -> " +
                    $"{targetPose} escaped the exhaustive retail dispatcher.");
        }
    }

    /// <summary>
    /// Applies the command alpha's lookup failure selected when no transition won: hurt and
    /// boost releases, grabbed, wall-jump and ball fallbacks, running deceleration and stops, and the aim, turn and
    /// aerial definition fallbacks.
    /// </summary>
    private void ApplyLookupFailureCommand(SamusHandlerFrame frame, SamusState samus)
    {
        if (frame.PoseAtFrameStart is SamusPoseId.KnockbackRightPose or SamusPoseId.KnockbackLeftPose &&
                 _player.ProspectiveSamusFallbackPose == frame.PoseAtFrameStart)
        {
            // The retained hurt pose still executes prospective command two.
            // Reset deceleration and cancel the momentum flag after movement.
            // Extra speed survives this command, but the next normal mover
            // consumes the cancellation through HandleExtraRunSpeed.
            samus.HorizontalSpeed.AccelerationMode = SamusAccelerationMode.Accelerating;
            samus.HorizontalSpeed.CancelRunningMomentum(SamusPoseMetadata.ReadFacingDirection(samus));
        }
        else if (frame.PoseAtFrameStart is SamusPoseId.DamageBoostRightPose or SamusPoseId.DamageBoostLeftPose &&
                 _player.ProspectiveSamusFallbackPose is { } boostFallback &&
                 boostFallback != frame.PoseAtFrameStart)
        {
            // Releasing every boost chord reaches definition entry two, the neutral jump poses:
            // the same initializer as the table's held-Jump exit.
            SamusKnockbackMovement.ApplyDamageBoostPoseTransition(
                samus,
                unchecked(boostFallback),
                _controller1.NewlyPressed);
        }
        else if (SamusPoseMetadata.IsDraygonGrabbedPose(frame.PoseAtFrameStart) &&
                 _player.ProspectiveSamusFallbackPose is { } draygonFallback)
        {
            // Zero input uses the pose's fallback pose, returning aimed/firing/
            // struggling art to the neutral Draygon-grabbed poses without touching owner placement.
            SamusPoseMetadata.ApplyDraygonGrabbedPoseChange(samus, 
                unchecked(draygonFallback));
        }
        else if (SamusPoseMetadata.IsWallJumpPose(frame.PoseAtFrameStart) &&
                 _player.ProspectiveSamusFallbackPose is
                     SamusPoseId.SpinJumpRightPose or SamusPoseId.SpinJumpLeftPose)
        {
            // The pose definition leaves the launch animation for ordinary spin
            // art. The wall-jumping movement type selects command six. The pose change
            // reports a replacement only when the pose's initialization itself replaces the
            // installed pose; then the pose update skips the command. So
            // a spin jump keeps its launch speed only when initialization promotes it
            // to Space Jump or Screw Attack; otherwise the command clears base/extra speed.
            SamusPoseId spinFallback = unchecked(_player.ProspectiveSamusFallbackPose.Value);
            SamusAerialPoseTransitions.ApplySpinJumpDirectionTransition(samus, spinFallback);
            if (samus.Pose == spinFallback)
                samus.HorizontalSpeed.ClearHorizontalMomentum(SamusPoseMetadata.ReadFacingDirection(samus));
        }
        else if (frame.PoseAtFrameStart is SamusPoseId.MorphBallFallingRightPose or SamusPoseId.MorphBallFallingLeftPose &&
                 _player.ProspectiveSamusFallbackPose == frame.PoseAtFrameStart)
        {
            samus.HorizontalSpeed.ApplyDeceleratingInputFallback(
                frame.DeceleratingFallbackHasMomentum, SamusPoseMetadata.ReadFacingDirection(samus));
        }
        else if (frame.PoseAtFrameStart is SamusPoseId.SpringBallJumpRightPose or SamusPoseId.SpringBallJumpLeftPose or
                     SamusPoseId.SpringBallFallingRightPose or SamusPoseId.SpringBallFallingLeftPose &&
                 _player.ProspectiveSamusFallbackPose == frame.PoseAtFrameStart)
        {
            // Spring Ball in air/falling (their own movement types) select command six in
            // the pose command table. After this frame's movement, command six clears mode and base
            // speed and falls through to its boost/extra-speed cancel.
            samus.HorizontalSpeed.ClearHorizontalMomentum(SamusPoseMetadata.ReadFacingDirection(samus));
        }
        else if (frame.PoseAtFrameStart is SamusPoseId.MorphBallGroundRightPose or SamusPoseId.MorphBallGroundLeftPose or
                     SamusPoseId.SpringBallGroundRightPose or SamusPoseId.SpringBallGroundLeftPose &&
                 _player.ProspectiveSamusFallbackPose == frame.PoseAtFrameStart)
        {
            // Command six clears base/mode, then cancels extra running momentum.
            // Consume the same-pose command after this frame's movement.
            samus.HorizontalSpeed.ClearHorizontalMomentum(SamusPoseMetadata.ReadFacingDirection(samus));
        }
        else if ((frame.PoseAtFrameStart is SamusPoseId.MorphBallMovingRightPose or
                     SamusPoseId.MorphBallMovingLeftPose or
                     SamusPoseId.SpringBallMovingRightPose or
                     SamusPoseId.SpringBallMovingLeftPose) &&
                 _player.ProspectiveSamusFallbackPose == frame.PoseAtFrameStart)
        {
            // Prospective command one rechecks the post-movement base speeds. A
            // residue selects mode two deceleration; exact zero falls through to
            // command two's stopped mode and definition fallback on the next frame.
            samus.HorizontalSpeed.AccelerationMode =
                samus.HorizontalSpeed.HasBaseSpeed ? SamusAccelerationMode.Decelerating : SamusAccelerationMode.Accelerating;
        }
        else if ((frame.PoseAtFrameStart is SamusPoseId.MorphBallMovingRightPose or
                     SamusPoseId.MorphBallMovingLeftPose or
                     SamusPoseId.SpringBallMovingRightPose or
                     SamusPoseId.SpringBallMovingLeftPose) &&
                 _player.ProspectiveSamusFallbackPose is
                     SamusPoseId.MorphBallGroundRightPose or SamusPoseId.MorphBallGroundLeftPose or
                     SamusPoseId.SpringBallGroundRightPose or SamusPoseId.SpringBallGroundLeftPose)
        {
            // Command six clears base and extra momentum after the
            // current movement frame for both grounded ball movement types.
            samus.HorizontalSpeed.ClearHorizontalMomentum(SamusPoseMetadata.ReadFacingDirection(samus));
            SamusPosturePoseTransitions.ApplyMorphBallPoseChange(samus, 
                unchecked(_player.ProspectiveSamusFallbackPose.Value));
        }
        else if ((SamusPoseMetadata.IsRightFacingRunningPose(frame.PoseAtFrameStart) ||
                  SamusPoseMetadata.IsLeftFacingRunningPose(frame.PoseAtFrameStart)) &&
                 _player.ProspectiveSamusFallbackPose == frame.PoseAtFrameStart)
        {
            // Retaining the running fallback selected command one in alpha.
            // After movement, fold extra dash speed into base and cancel boost
            // before decelerating; changing only the mode leaves stale dash
            // speed alive and changes the subsequent stopping trajectory.
            samus.HorizontalSpeed.ApplyDeceleratingInputFallback(
                selectedMomentumCommand: true, SamusPoseMetadata.ReadFacingDirection(samus));
        }
        else if (SamusPoseMetadata.IsRightFacingRunningPose(frame.PoseAtFrameStart) &&
                 _player.ProspectiveSamusFallbackPose == SamusPoseId.FacingRightNormalPose)
        {
            samus.HorizontalSpeed.AccelerationMode = SamusAccelerationMode.Accelerating;
            if (frame.PoseAtFrameStart == SamusPoseId.MovingRightNormalPose)
                SamusGroundedPoseTransitions.ApplyRunningRightToStandingRight(samus);
            else
                SamusGroundedPoseTransitions.ApplyGroundedAimTransition(samus, SamusPoseId.FacingRightNormalPose);
        }
        else if (SamusPoseMetadata.IsLeftFacingRunningPose(frame.PoseAtFrameStart) &&
                 _player.ProspectiveSamusFallbackPose == SamusPoseId.FacingLeftNormalPose)
        {
            samus.HorizontalSpeed.AccelerationMode = SamusAccelerationMode.Accelerating;
            if (frame.PoseAtFrameStart == SamusPoseId.MovingLeftNormalPose)
                SamusGroundedPoseTransitions.ApplyRunningLeftToStandingLeft(samus);
            else
                SamusGroundedPoseTransitions.ApplyGroundedAimTransition(samus, SamusPoseId.FacingLeftNormalPose);
        }
        else if (SamusPoseMetadata.IsMoonwalkingPose(frame.PoseAtFrameStart) &&
                 _player.ProspectiveSamusFallbackPose is { } moonwalkFallback)
        {
            // Command two selected the literal standing/aim fallback during alpha.
            // Applying it after movement preserves the one-last-step timing.
            SamusGroundedPoseTransitions.ApplyMoonwalkPoseChange(samus, 
                unchecked(moonwalkFallback),
                _options.MoonwalkEnabled);
        }
        else if (SamusPoseMetadata.IsAimedRanIntoWallPose(frame.PoseAtFrameStart) &&
                 _player.ProspectiveSamusFallbackPose is { } wallFallback)
        {
            // The aimed ran-into-wall poses name the plain ran-into-wall poses as their fallback poses. The stable
            // target shares radius, animation, and ran-into-wall cleanup physics.
            SamusGroundedPoseTransitions.ApplyGroundedAimTransition(samus, 
                unchecked(wallFallback));
        }
        else if ((frame.PoseAtFrameStart is
                      SamusPoseId.StandingAimUpRightPose or
                      SamusPoseId.StandingAimUpLeftPose or
                      SamusPoseId.StandingAimDiagonalUpRightPose or
                      SamusPoseId.StandingAimDiagonalUpLeftPose or
                      SamusPoseId.StandingAimDiagonalDownRightPose or
                      SamusPoseId.StandingAimDiagonalDownLeftPose ||
                  SamusPoseMetadata.IsAimedCrouchingPose(frame.PoseAtFrameStart)) &&
                 _player.ProspectiveSamusFallbackPose is { } aimFallback)
        {
            // Only the input fallback path above populates this value.
            // Validate and apply it through the same radius/animation seam as
            // held-input aim changes; never assign the authored value directly to Pose.
            SamusGroundedPoseTransitions.ApplyGroundedAimTransition(samus, 
                unchecked(aimFallback));
        }
        else if (frame.UsePoseDefinitionFallback && _player.ProspectiveSamusPose is null &&
                 samus.Pose == frame.PoseAtFrameStart &&
                 SamusPoseMetadata.IsAimedCrouchingTurnPose(frame.PoseAtFrameStart) &&
                 _player.ProspectiveSamusFallbackPose is { } crouchingTurnFallback)
        {
            SamusGroundedPoseTransitions.ApplyAimedCrouchingTurnInputFallback(samus, 
                unchecked(crouchingTurnFallback));
        }
        else if (frame.UsePoseDefinitionFallback && _player.ProspectiveSamusPose is null &&
                 samus.Pose == frame.PoseAtFrameStart &&
                 (SamusPoseMetadata.IsAerialTurnPose(frame.PoseAtFrameStart) ||
                  frame.MovementTypeAtFrameStart == SamusMovementType.TurningOnGround))
        {
            // A definition without a fallback pose retains the animation, not the momentum
            // command. Command two still cancels reverse acceleration,
            // for ground turns exactly as for aerial ones.
            SamusGroundedPoseTransitions.ApplyTurnInputFallback(samus, frame.MovementTypeAtFrameStart);
        }
        else if (frame.MovementTypeAtFrameStart == SamusMovementType.NormalJumping &&
                 _player.ProspectiveSamusFallbackPose is { } jumpFallback)
        {
            // An unchanged prospective pose must not reset animation or invoke
            // compact-radius expansion. Command one still folds momentum after
            // beta movement and rechecks the remaining base speed there.
            if (jumpFallback != samus.Pose)
                SamusAerialPoseTransitions.ApplyAerialAimTransition(samus, unchecked(jumpFallback));
            samus.HorizontalSpeed.ApplyDeceleratingInputFallback(
                frame.DeceleratingFallbackHasMomentum, SamusPoseMetadata.ReadFacingDirection(samus));
        }
        else if (SamusPoseMetadata.IsAimedAerialPose(frame.PoseAtFrameStart) &&
                 _player.ProspectiveSamusFallbackPose is { } aerialFallback)
        {
            // Install the fallback's pose metadata and initializer first.
            // Falling command eight below then clears only the extra component;
            // it must not be folded into base speed like running deceleration.
            // A retained compact pose selects history/momentum work only;
            // pose initialization and animation restart require a change.
            if (aerialFallback != samus.Pose)
                SamusAerialPoseTransitions.ApplyAerialAimTransition(samus, 
                    unchecked(aerialFallback));
        }
    }
}
