using SuperMetroid.Core.Input;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.World;

/// <summary>
/// The pose update after the normal movement phase's animation: the pose the animation's pose-transition cue left pending, the
/// interruptions, the collision transitions, the input transition the input phase selected or the momentum response its lookup
/// failure selected, and the pose-history commit.
/// </summary>
internal sealed class SamusPoseTransitionPhase
{

    /// <summary>Supplies the held and newly pressed controller buttons to Samus's frame phases.</summary>
    private readonly GameplayActionInput _playerActions;
    /// <summary>Gates movement and animation branches during gameplay time freeze.</summary>
    private readonly GameplayTimeFreeze _freeze;
    /// <summary>Provides frame-counter cadence for movement and animation behavior.</summary>
    private readonly FrameClock _clock;
    /// <summary>Supplies player options such as configured movement controls.</summary>
    private readonly GameplayOptions _options;
    /// <summary>Owns the live Samus state and the selection of her input and movement modes.</summary>
    private readonly SamusFrameState _player;

    /// <summary>Creates the phase over the frame phases' collaborators.</summary>
    internal SamusPoseTransitionPhase(SamusPoseTransitionPhaseDependencies dependencies)
    {
        _playerActions = dependencies.PlayerActions;
        _freeze = dependencies.Freeze;
        _clock = dependencies.Clock;
        _options = dependencies.Options;
        _player = dependencies.Player;
    }

    /// <summary>Applies post-movement pose and animation transitions after the frame's selected movement mode.</summary>
    internal void Apply(SamusControlFrame frame, SamusState samus, SamusTerrain terrain)
    {
        if (!_player.GroundedSamusMovementEnabled ||
            frame.Mode.Movement is SamusMovementMode.Fatal or SamusMovementMode.SuitPickup or SamusMovementMode.ScriptLocked)
            return;

        // Hit interruption observes the old movement type before the pose update.
        // Its bomb rejection still occurs when an animation transition
        // wins pose selection; otherwise a stale request survives unmorph/morph.
        if (samus.HasNoKnockback)
            SamusPoseMetadata.RejectUnsupportedPublishedBombJump(samus, _freeze.TimeIsFrozen);

        // The pose the animation's pose-transition cue left pending outranks every other transition at this seam.
        // The turn's cue leaves a turn-completion pose only if the input phase did not select one
        // of its four jump exceptions. When left pending, it still wins here.
        // The hurt-expiry check samples hurt expiry before the pose update installs the animation's
        // pending pose. That decision stands even if Shinespark setup
        // clears the live knockback direction while installing the shinespark windup poses.
        bool hitInterruptionExpiryPending =
            SamusKnockbackMovement.IsExpiredHitInterruptionPending(samus);
        bool animationTransitionApplied =
            SamusPoseTransitions.ApplyPendingAnimationTransition(samus);

        animationTransitionApplied = ApplyInterruptionTransitions(frame, samus, animationTransitionApplied,
            hitInterruptionExpiryPending, terrain);
        animationTransitionApplied = ApplyCollisionTransitions(frame, samus, animationTransitionApplied, terrain);

        if (!animationTransitionApplied && _player.ProspectiveSamusPose is { } inputTransition)
            ApplyInputTransition(frame, samus, inputTransition, terrain);
        else if (!animationTransitionApplied)
            ApplyLookupFailureResponse(frame, samus);

        // Crouching stops Samus's momentum only on lookup failure. A matched
        // angle-held pose must retain boost even though movement zeroed X speed.
        if (!animationTransitionApplied && frame.UsePoseDefinitionFallback &&
            _player.ProspectiveSamusPose is null &&
            frame.MovementTypeAtFrameStart == SamusMovementType.Crouching &&
            SamusPoseMetadata.ReadMovementType(samus) == SamusMovementType.Crouching)
            samus.HorizontalSpeed.ApplyStoppedInputFallback(SamusPoseMetadata.ReadFacingDirection(samus));

        // Windup still runs the ordinary special-movement-type input lookup. Its unmatched
        // chord stops Samus's momentum, just like crouching. Skipping
        // this loses the one-frame momentum cancellation before a delayed
        // midair launch. A selected launch or timeout must win instead.
        if (!animationTransitionApplied && frame.UsePoseDefinitionFallback &&
            _player.ProspectiveSamusPose is null &&
            frame.PoseAtFrameStart is SamusPoseId.ShinesparkWindupRightPose or SamusPoseId.ShinesparkWindupLeftPose &&
            samus.Shinespark.Phase == ShinesparkPhase.Windup)
            samus.HorizontalSpeed.ApplyStoppedInputFallback(SamusPoseMetadata.ReadFacingDirection(samus));

        // Falling's lookup-failure momentum response still runs when its definition
        // retains the current pose. It follows pose initialization, and must
        // not run when a higher-priority collision or interruption won.
        if (!animationTransitionApplied && frame.UsePoseDefinitionFallback &&
            _player.ProspectiveSamusPose is null &&
            frame.MovementTypeAtFrameStart == SamusMovementType.Falling &&
            SamusPoseMetadata.ReadMovementType(samus) == SamusMovementType.Falling)
            SamusAerialPoseTransitions.ApplyFallingInputFallback(samus);

        // The normal-jump setup publishes the fresh Shoot direction
        // for every accepted route, including aim changes within a jump. The
        // projectile step earlier in this update cleared the previous update's handoff.
        if (samus.Pose != samus.PoseHistory.PreviousPose)
            SamusAerialPoseTransitions.ArmNormalJumpTransitionShot(samus, _playerActions.NewlyPressed);

        // The pose history moves on after any chosen pose transition takes effect,
        // including a transition to the same pose. An update that chose no transition
        // does not move it. Keep this after the final collision and pose choice.
        // The grapple launch applies its transition pose early and clears the ordinary
        // input phase's input. It still reaches this same history step, once, after the
        // forced launch pose.
        // X-ray's activation installs its pose and movement early and clears
        // the ordinary input phase's targets. Its final history commit happens here.
        if (animationTransitionApplied || frame.XrayActivatedThisFrame || _player.LastGrappleMovement is { WallJumpStarted: true } ||
            _player.ProspectiveSamusPose is not null ||
            _player.ProspectiveSamusFallbackPose is not null)
            samus.CommitPoseHistory();
    }

    /// <summary>
    /// Applies the queued transitional poses and interruptions that outrank the input phase's input in priority order: grapple's
    /// drop and release poses, the finished Shinespark crash, hurt expiry and a pending hit, a published bomb jump, the
    /// windup's launch pose, X-ray's activation and grapple's connection. Returns whether a transition has been applied.
    /// </summary>
    [IndependentFlag("An earlier transition this frame and a pending hit expiry are independent pose-update states.")]
    private bool ApplyInterruptionTransitions(SamusControlFrame frame, SamusState samus, bool alreadyApplied,
        bool hitInterruptionExpiryPending, SamusTerrain terrain)
    {
        bool animationTransitionApplied = alreadyApplied;
        // The animation's transition queues a transitional pose rather than replacing movement.
        // Commit after animation, suppressing the lower-priority input/grounding
        // pose. Its speed clears already happened before movement.
        if (!animationTransitionApplied && _player.LastGrappleMovement is { PendingDropPose: SamusPoseId dropPose })
        {
            SamusAerialPoseTransitions.ApplyGrappleDropTransition(samus,
                terrain.Level,
                dropPose, _clock.Counter, terrain.RoomObjects);
            _player.ClearProspectivePoses();
            _player.ProspectiveSamusWallCollisionPose = null;
            animationTransitionApplied = true;
        }

        // The grapple release queues its release pose the same way, so the hit interruption
        // earlier this frame still saw the grappling movement type.
        if (!animationTransitionApplied && _player.LastGrappleMovement is { PendingReleasePose: SamusPoseId releasePose })
        {
            SamusGrappleMovement.ApplyReleasePose(samus, releasePose);
            _player.ClearProspectivePoses();
            _player.ProspectiveSamusWallCollisionPose = null;
            animationTransitionApplied = true;
        }

        // Crash movement queues a transitional pose; it must not replace the
        // body Samus's animation saw earlier in this frame. The animation's
        // pending transition pose retains its higher priority.
        if (!animationTransitionApplied && _player.LastShinesparkMovement is { CrashSequenceFinished: true })
        {
            SamusShinesparkState.ApplyCrashFinishPose(samus);
            _player.ClearProspectivePoses();
            _player.ProspectiveSamusWallCollisionPose = null;
            animationTransitionApplied = true;
        }

        // Expiry still permits this frame's movement and animation. Its
        // transition pose wins over the ordinary input chosen in the input phase,
        // even when a damage boost has already restored normal movement.
        // With the animation's transition pose already installed, finishing
        // the hit keeps that pose and ALSO clears the hurt state.
        if (hitInterruptionExpiryPending)
        {
            SamusKnockbackMovement.FinishExpiredHitInterruption(samus);
            _player.ClearProspectivePoses();
            _player.ProspectiveSamusWallCollisionPose = null;
            animationTransitionApplied = true;
        }

        // The pose interruption runs after Samus's animation and before the pose update. Terrain
        // spike reactions only publish their damage request; this common interruption
        // converts that request into the same hurt pose and knockback movement owner
        // used by every damage producer. The animation's pending transition pose retains
        // its higher pose-update priority and therefore suppresses this interruption.
        if (!animationTransitionApplied &&
            SamusKnockbackMovement.TryStartPendingHitInterruption(
                samus,
                _playerActions.Current,
                _freeze.TimeIsFrozen,
                new KnockbackPoseCollision(terrain, _clock.Counter)))
        {
            _player.ClearProspectivePoses();
            _player.ProspectiveSamusWallCollisionPose = null;
            animationTransitionApplied = true;
        }

        // Bomb overlap is published between the input and movement phases, but the game's
        // interruption step considers it only AFTER movement/animation.
        // Active hurt values retain the request without arming or rejecting it;
        // expiry owns this frame even after clearing those values. This permits
        // a later bomb launch to retain the damaged pose, with input locked by
        // the armed bomb jump rather than an early speculative pose change.
        if (!animationTransitionApplied && samus.HasNoKnockback &&
            SamusPoseMetadata.TrySetupPublishedBombJump(samus,
                terrain.Level,
                _freeze.TimeIsFrozen,
                _clock.Counter,
                _playerActions.NewlyPressed,
                terrain.RoomObjects))
        {
            _player.ClearProspectivePoses();
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
            _player.ClearProspectivePoses();
            _player.ProspectiveSamusWallCollisionPose = null;
            animationTransitionApplied = true;
        }

        if (samus.Xray.CommitPendingActivation(samus, animationTransitionApplied))
        {
            _player.ClearProspectivePoses();
            _player.ProspectiveSamusWallCollisionPose = null;
            animationTransitionApplied = true;
        }

        // Grapple connection publishes a pending connection pose.
        // The animation and hurt transitions above can suppress it without
        // undoing the already-entered beam phase. This is the
        // ordering used by bounded Grapple momentum-retention setups.
        if (!animationTransitionApplied &&
            _player.LastGrappleMovement is { PendingConnection: { } pendingConnection })
        {
            Position16<RoomPixels> grapplePrevious =
                SamusGrappleMovement.ApplyPendingConnectionPose(
                    samus,
                    pendingConnection);
            frame.SnapCameraPoint(grapplePrevious);
            _player.ClearProspectivePoses();
            _player.ProspectiveSamusWallCollisionPose = null;
            animationTransitionApplied = true;
        }

        return animationTransitionApplied;
    }

    /// <summary>
    /// Applies the collision results that replace the input phase's input target: ceilings, the bomb arc's fall, running into a
    /// wall, Morph Ball, wall-jump and Spring Ball landings, ordinary landings and walking off a floor. Returns whether
    /// a transition has been applied.
    /// </summary>
    private bool ApplyCollisionTransitions(SamusControlFrame frame, SamusState samus, bool alreadyApplied, SamusTerrain terrain)
    {
        bool animationTransitionApplied = alreadyApplied;
        // The ceiling collision replaces the ordinary input target with the current pose
        // on ceiling collision. Apply the ball's deferred vertical
        // stop and consume this same-pose transition so history still shifts
        // and an input-phase aim/turn selection cannot override it. Higher-priority
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
            _player.ClearProspectivePoses();
            animationTransitionApplied = true;
        }

        // The bomb-arc check overrides the input target when the bomb arc moved down through
        // its apex with no floor below; the airborne pose is installed this frame.
        if (!animationTransitionApplied &&
            _player.LastBombJumpMovement is { FellWithoutFloor: true })
        {
            SamusPosturePoseTransitions.ApplyBombJumpFallingPose(samus);
            _player.ClearProspectivePoses();
            animationTransitionApplied = true;
        }

        // The wall-stop check runs inside the pose update after movement/animation and
        // only when no animation or interruption transition has already won. It first
        // reads whether this update's running movement stopped her speed against a wall. This creates a wall-stop pose
        // even with no input match.
        // An unobstructed upward platform carry leaves the prospective input
        // pose intact too, so the one-pixel run check runs while a Kamer carries Samus upward.
        // A special movement owner can also retain a standing/running pose
        // (suit-interrupted windup). The wall-stop check does not require the normal grounded movement phase.
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
                    // The input phase chooses the same prospective pose for a
                    // matched input transition and a no-input fallback. A decelerating run
                    // retains its running pose and still takes the running wall check's one-pixel forward move, which
                    // carries on its drift after release.
                    : _player.ProspectiveSamusFallbackPose;
            _player.ProspectiveSamusWallCollisionPose =
                SamusGroundedPoseTransitions.CheckProspectiveRunningPoseForWall(samus,
                    terrain.Level,
                    prospectiveRunningPose,
                    currentRunHitWall,
                    out _,
                    terrain.RoomObjects);
        }

        if (!animationTransitionApplied &&
            _player.ProspectiveSamusWallCollisionPose is { } wallCollisionPose)
        {
            // The selector result replaces the ordinary input target. Applying it
            // here also covers the no-input “running speed was killed” branch.
            SamusGroundedPoseTransitions.ApplyRanIntoWallPoseChange(samus, wallCollisionPose);
            animationTransitionApplied = true;
            // The wall-stop check replaces only ProspectivePose. When the input phase matched no input
            // transition, the pose update still runs the momentum response
            // selected for the pre-wall movement type.
            if (_player.ProspectiveSamusPose is null && _player.ProspectiveSamusFallbackPose is not null)
            {
                SamusMomentumResponse response = SamusMomentumResponses.ForLookupFailure(
                    SamusPoseMetadata.ReadMovementType(frame.PoseAtFrameStart));
                samus.HorizontalSpeed.ApplyDeceleratingInputFallback(
                    response == SamusMomentumResponse.Decelerate && frame.DeceleratingFallbackHasMomentum,
                    SamusPoseMetadata.ReadFacingDirection(samus));
            }
        }

        // A Morph Ball landing has its own poses and landing transitions. A hard
        // impact retains the falling morph ball poses and launches the first bounce; the next collision
        // launches the second bounce; only a gentle or second-bounce collision installs
        // the grounded morph ball poses. Every one of these outcomes consumes the transition, so input
        // selected before collision cannot override the game's bounce result.
        // The landing transitions ignore the ground family's SamusMovementType.MorphBallGround, even if hurt
        // movement left that pose with nonzero Y speed. Do not infer an airborne
        // landing step from velocity alone.
        if (!animationTransitionApplied &&
            _player.LastMorphBallMovement is { Landed: true } &&
            SamusPoseMetadata.IsAirborneMorphBallPose(frame.PoseAtFrameStart))
        {
            SamusPosturePoseTransitions.ApplyMorphBallLanding(samus);
            animationTransitionApplied = true;
        }

        // The wall-jump check publishes its wall-jump collision result and reports
        // a collision, preventing vertical motion. The wall-jump trigger
        // installs the wall-jump pose, clears old momentum, and launches from fixed constants.
        if (!animationTransitionApplied &&
            _player.LastAerialSamusMovement is { WallJumpTriggered: true })
        {
            SamusAerialPoseTransitions.ApplyWallJumpTrigger(samus, terrain.Level, _clock.Counter, terrain.RoomObjects);
            animationTransitionApplied = true;
        }

        // Spring Ball's three movement families share one collision transition, but
        // Spring Ball's movement makes held Jump an immediate relaunch and stores the jump intent
        // during automatic rebounds. Keep it distinct from ordinary-ball state.
        // The grounded Spring Ball movement type likewise lands with no transition.
        if (!animationTransitionApplied &&
            _player.LastMorphBallMovement is { Landed: true } &&
            SamusPoseMetadata.IsAirborneSpringBallPose(frame.PoseAtFrameStart))
        {
            SamusPosturePoseTransitions.ApplySpringBallLanding(samus, _playerActions.Current);
            animationTransitionApplied = true;
        }

        // A downward collision publishes a landing. The landing check chooses normal or
        // spin landing from the movement type that executed this frame, then settling
        // the accepted landing clears both velocity axes. This ordinary prospective pose
        // is lower priority than the animation's pending transition pose above.
        if (!animationTransitionApplied &&
            _player.LastAerialSamusMovement is { Landed: true } &&
            !SamusPoseMetadata.IsAerialTurnPose(frame.PoseAtFrameStart))
        {
            if (SamusPoseMetadata.IsCompactAerialPose(frame.PoseAtFrameStart))
            {
                SamusPoseTransitions.TryApplyCompactAerialLanding(samus,
                    terrain.Level,
                    _clock.Counter,
                    terrain.RoomObjects);
            }
            else
            {
                bool wasSpinning = SamusPoseMetadata.IsSpinJumpPose(frame.PoseAtFrameStart) ||
                    SamusPoseMetadata.IsWallJumpPose(frame.PoseAtFrameStart);
                SamusPoseTransitions.TryApplyAerialLanding(samus,
                    terrain.Level,
                    wasSpinning,
                    _playerActions.Current,
                    _clock.Counter,
                    terrain.RoomObjects);
            }
            animationTransitionApplied = true;
        }

        // An unobstructed DOWNWARD grounding probe publishes SamusVerticalCollisionResult.Falling. An
        // unobstructed upward platform carry publishes SamusVerticalCollisionResult.None
        // and leaves the pose alone, so Samus rising on a Kamer does not fall and land over and over. For admitted
        // standing (including landing art), running, and stable crouching types,
        // losing the floor selects the first airborne family and the falling check chooses falling art.
        // The ground and jumping turns change no pose when they lose the floor;
        // they finish their turn transition, then the destination pose detects the missing floor.
        if (!animationTransitionApplied &&
            _player.LastGroundedSamusMovement is
                { Vertical.IsUnobstructedDownwardMovement: true } &&
            (SamusPoseMetadata.IsRightFacingStandingPose(frame.PoseAtMovement) ||
             SamusPoseMetadata.IsLeftFacingStandingPose(frame.PoseAtMovement) ||
             SamusPoseMetadata.IsRightFacingLandingPose(frame.PoseAtMovement) ||
             SamusPoseMetadata.IsLeftFacingLandingPose(frame.PoseAtMovement) ||
             SamusPoseMetadata.IsRightFacingRunningPose(frame.PoseAtMovement) ||
             SamusPoseMetadata.IsLeftFacingRunningPose(frame.PoseAtMovement) ||
             SamusPoseMetadata.IsMoonwalkingPose(frame.PoseAtMovement) ||
             frame.PoseAtMovement is SamusPoseId.KnockbackRightPose or SamusPoseId.KnockbackLeftPose ||
             SamusPoseMetadata.IsRanIntoWallPose(frame.PoseAtMovement) ||
             SamusPoseMetadata.IsRightFacingCrouchingPose(frame.PoseAtMovement) ||
             SamusPoseMetadata.IsLeftFacingCrouchingPose(frame.PoseAtMovement)))
        {
            SamusPoseId fallingPose = SamusPoseTransitions.SelectFallingPoseForCurrentAim(samus);
            SamusPoseTransitions.ApplyWalkedOffFloorTransition(samus,
                terrain.Level,
                fallingPose, _clock.Counter, terrain.RoomObjects);
            animationTransitionApplied = true;
        }

        // SamusMovementType.MorphBallGround movement publishes the ordinary airborne morph ball poses when its
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
    /// Applies the input transition the input phase selected from the pose's input transitions, through the setup the
    /// transition names. A transition that keeps her pose means "remain in this animation", so nothing is set up.
    /// </summary>
    /// <exception cref="NotSupportedException">The transition names the stand-up from the Draygon-held ball, whose setup is not reproduced.</exception>
    private void ApplyInputTransition(SamusControlFrame frame, SamusState samus, SamusPoseTransition inputTransition, SamusTerrain terrain)
    {
        SamusPoseId targetPose = inputTransition.ProspectivePose;
        switch (inputTransition.Setup)
        {
            case SamusPoseTransitionSetup.KeepPose:
                // Reinitializing frame zero would prevent the turn transition from ever being reached.
                break;
            case SamusPoseTransitionSetup.ChangeDraygonGrabbedPose:
                // Both sets of input transitions stay within one facing; the change keeps it and starts the target's
                // own animation.
                SamusPoseMetadata.ApplyDraygonGrabbedPoseChange(samus, targetPose);
                break;
            case SamusPoseTransitionSetup.ChangeMoonwalkPose:
                SamusGroundedPoseTransitions.ApplyMoonwalkPoseChange(samus, targetPose, _options.MoonwalkEnabled);
                break;
            case SamusPoseTransitionSetup.StartMoonwalkTurnJump:
                // The stable moonwalk poses' input transitions select a generic
                // reversal target. The reversal check validates it against the old
                // shot direction, folds momentum, and leaves the moonwalk turn-jump poses
                // grounded until either its input transitions or the turn transition starts the jump.
                SamusGroundedPoseTransitions.ApplyMoonwalkTurnJump(samus, targetPose);
                break;
            case SamusPoseTransitionSetup.StartJump:
                // Every standing turn on the ground, the aimed turns and the moonwalk turns among them, continues taking
                // the standing pose's input transitions, which can interrupt the art with either same-facing spin or
                // neutral jump; the landings expose the same standing input transitions while their brief transitions
                // are active. Preserve newly pressed Shoot as well: the normal-jump setup owns the simultaneous
                // Jump+Fire bridge when the selected body is SamusMovementType.NormalJumping.
                SamusAerialPoseTransitions.ApplyOrdinaryJumpTransition(samus, targetPose, _playerActions.NewlyPressed);
                break;
            case SamusPoseTransitionSetup.JumpFromCrouch:
                // The crouched aim poses use the ordinary neutral-jump transition
                // art for every crouched aim direction, keeping the crouching-turn poses' ten-pixel adjustment.
                SamusPosturePoseTransitions.TryApplyCrouchJumpTransition(samus, terrain.Level, targetPose, _clock.Counter,
                    terrain.RoomObjects);
                break;
            case SamusPoseTransitionSetup.StartDamageBoost:
                // The accepted pose restores normal movement while
                // preserving the hurt arc and remaining timer.
                SamusKnockbackMovement.ApplyDamageBoostTransition(samus, frame.PoseAtFrameStart, targetPose);
                break;
            case SamusPoseTransitionSetup.JumpFromDamageBoost:
                SamusKnockbackMovement.ApplyDamageBoostPoseTransition(samus, targetPose, _playerActions.NewlyPressed);
                break;
            case SamusPoseTransitionSetup.StartRunning:
                SamusGroundedPoseTransitions.StartRunning(samus, targetPose);
                break;
            case SamusPoseTransitionSetup.ChangeGroundedAim:
                SamusGroundedPoseTransitions.ApplyGroundedAimTransition(samus, targetPose);
                break;
            case SamusPoseTransitionSetup.TurnOnGround:
                // Ceres releases controls while the forward-facing pose is still active. Its own input transitions map
                // Left/Right to generic turning poses; the reversal check recognizes the old front-facing pose, which
                // faces neither way, and keeps that target.
                SamusGroundedPoseTransitions.ApplyGroundedTurn(samus, targetPose, _playerActions.Current);
                break;
            case SamusPoseTransitionSetup.CrouchOrStand:
                SamusPosturePoseTransitions.TryApplyPostureTransition(samus, terrain.Level, targetPose, _clock.Counter,
                    terrain.RoomObjects);
                break;
            case SamusPoseTransitionSetup.StandFromCrouch:
                // These direct facing-normal poses bypass the animated
                // aimed standing-transition family, but still run pose-expansion
                // collision before installing the final standing body.
                SamusPosturePoseTransitions.TryApplyDirectCrouchToStandingTransition(samus, terrain.Level, targetPose,
                    _clock.Counter, terrain.RoomObjects);
                break;
            case SamusPoseTransitionSetup.MorphOrUnmorph:
                SamusPosturePoseTransitions.TryApplyMorphTransition(samus, terrain.Level, targetPose, _clock.Counter,
                    terrain.RoomObjects);
                break;
            case SamusPoseTransitionSetup.ChangeBallPose:
                // These six poses all share one animation.
                // The setup preserves frame/timer and applies turning
                // reversal momentum only when direction actually changes.
                SamusPosturePoseTransitions.ApplyMorphBallPoseChange(samus, targetPose);
                break;
            case SamusPoseTransitionSetup.JumpSpringBall:
                SamusPosturePoseTransitions.ApplySpringBallJump(samus, targetPose);
                break;
            case SamusPoseTransitionSetup.FallFromUnmorph:
                // Route an animation/collision-selected falling body through
                // ordinary expansion collision while preserving the fall.
                // Posture transitions read no input transitions:
                // the posture-transition movement type's input lookup returns at once.
                SamusPosturePoseTransitions.TryApplyUnmorphToFallingTransition(samus, terrain.Level, targetPose,
                    _clock.Counter, terrain.RoomObjects);
                break;
            case SamusPoseTransitionSetup.TurnInAir:
                // The input transitions select the generic jumping and falling turns. The turn
                // reads the source pose's shot direction, chooses the exact
                // aimed aerial turn art when needed, folds momentum, and runs compact
                // pose-expansion collision before committing the turn.
                SamusAerialPoseTransitions.TryApplyAerialTurn(samus, terrain.Level, targetPose, _clock.Counter,
                    terrain.RoomObjects);
                break;
            case SamusPoseTransitionSetup.ChangeSpinDirection:
                SamusAerialPoseTransitions.ApplySpinJumpDirectionTransition(samus, targetPose);
                break;
            case SamusPoseTransitionSetup.AimFromSpin:
                // Aim and Fire cancel both compact spin and wall-
                // jump bodies through the same normal-jump setup without
                // restarting the jump arc. The wall-jump Shot transition, for example, maps Shot
                // directly from the right wall jump to the right gun-extended normal jump. The jump's start performs
                // changed-pose collision for every radius expansion; a rejected
                // body leaves the compact source pose installed.
                SamusAerialPoseTransitions.TryApplySpinOrWallJumpToNormalJumpTransition(samus, terrain.Level, targetPose,
                    _clock.Counter, _playerActions.NewlyPressed, terrain.RoomObjects);
                break;
            case SamusPoseTransitionSetup.ChangeAerialAim:
                SamusAerialPoseTransitions.ApplySameFacingAerialTransition(samus, targetPose);
                break;
            case SamusPoseTransitionSetup.ChangeCompactAerialPose:
                SamusAerialPoseTransitions.TryApplyCompactAerialTransition(samus, terrain.Level, targetPose, _clock.Counter,
                    terrain.RoomObjects);
                break;
            case SamusPoseTransitionSetup.AimShinespark:
                // These are ordinary held/new input transitions, but
                // their transition hands her movement to a special owner instead of
                // invoking a normal movement-type setup.
                SamusAerialPoseTransitions.ApplyShinesparkDirectionTransition(samus, targetPose);
                break;
            case SamusPoseTransitionSetup.StandUpFromDraygonHold:
                throw new NotSupportedException(
                    $"The stand-up {targetPose} from the Draygon-held ball {frame.PoseAtFrameStart} has no reproduced setup.");
            default:
                throw new InvalidDataException($"Undefined {nameof(SamusPoseTransitionSetup)} for input transition {frame.PoseAtFrameStart} -> {targetPose}.");
        }
    }

    /// <summary>
    /// Applies the setup Samus's frame-start pose, or else the movement her last pose setup left her in, names for the input
    /// phase's lookup failure when no transition won (<see cref="SamusLookupFailureSetups"/>), from the fallback pose that
    /// lookup chose.
    /// </summary>
    private void ApplyLookupFailureResponse(SamusControlFrame frame, SamusState samus)
    {
        SamusPoseId source = frame.PoseAtFrameStart;
        SamusPoseId? fallback = _player.ProspectiveSamusFallbackPose;
        switch (SamusLookupFailureSetups.For(source, frame.MovementTypeAtFrameStart))
        {
            case SamusLookupFailureSetup.None:
                break;
            case SamusLookupFailureSetup.StopKnockback:
                // The retained hurt pose still stops Samus's momentum.
                // Reset deceleration and cancel the momentum flag after movement.
                // Extra speed survives the stop, but the next normal mover
                // consumes the cancellation through HandleExtraRunSpeed.
                if (fallback == source)
                {
                    samus.HorizontalSpeed.AccelerationMode = SamusAccelerationMode.Accelerating;
                    samus.HorizontalSpeed.CancelRunningMomentum(SamusPoseMetadata.ReadFacingDirection(samus));
                }
                break;
            case SamusLookupFailureSetup.ReleaseDamageBoost:
                // Releasing every boost chord reaches the neutral jump poses:
                // the same setup as its held-Jump input transition.
                if (fallback is { } boostFallback && boostFallback != source)
                    SamusKnockbackMovement.ApplyDamageBoostPoseTransition(samus, boostFallback, _playerActions.NewlyPressed);
                break;
            case SamusLookupFailureSetup.ReturnToNeutralDraygonHold:
                // Zero input uses the pose's fallback pose, returning aimed/firing/
                // struggling art to the neutral Draygon-grabbed poses without touching owner placement.
                if (fallback is { } draygonFallback)
                    SamusPoseMetadata.ApplyDraygonGrabbedPoseChange(samus, draygonFallback);
                break;
            case SamusLookupFailureSetup.SpinFromWallJump:
                // The pose definition leaves the launch animation for ordinary spin
                // art. The wall-jumping movement type kills Samus's X speed. The pose change
                // reports a replacement only when the pose's initialization itself replaces the
                // installed pose; then the pose update skips the kill. So
                // a spin jump keeps its launch speed only when initialization promotes it
                // to Space Jump or Screw Attack; otherwise the kill clears base/extra speed.
                if (fallback is SamusPoseId.SpinJumpRightPose or SamusPoseId.SpinJumpLeftPose)
                {
                    SamusPoseId spinFallback = fallback.Value;
                    SamusAerialPoseTransitions.ApplySpinJumpDirectionTransition(samus, spinFallback);
                    if (samus.Pose == spinFallback)
                        samus.HorizontalSpeed.ClearHorizontalMomentum(SamusPoseMetadata.ReadFacingDirection(samus));
                }
                break;
            case SamusLookupFailureSetup.DecelerateFallingBall:
                if (fallback == source)
                {
                    samus.HorizontalSpeed.ApplyDeceleratingInputFallback(
                        frame.DeceleratingFallbackHasMomentum, SamusPoseMetadata.ReadFacingDirection(samus));
                }
                break;
            case SamusLookupFailureSetup.StopBall:
                // A still ball and Spring Ball in the air or falling kill Samus's X speed: after this frame's movement,
                // the kill clears mode and base speed and goes on to cancel boost and extra running speed.
                if (fallback == source)
                    samus.HorizontalSpeed.ClearHorizontalMomentum(SamusPoseMetadata.ReadFacingDirection(samus));
                break;
            case SamusLookupFailureSetup.StopRollingBall:
                // Killing X speed clears base and extra momentum after the
                // current movement frame for both grounded ball movement types.
                if (fallback is { } stillBall)
                {
                    samus.HorizontalSpeed.ClearHorizontalMomentum(SamusPoseMetadata.ReadFacingDirection(samus));
                    SamusPosturePoseTransitions.ApplyMorphBallPoseChange(samus, stillBall);
                }
                break;
            case SamusLookupFailureSetup.StopRunning:
                if (fallback == source)
                    DecelerateRetainedRun(samus);
                else if (fallback is not null)
                {
                    samus.HorizontalSpeed.AccelerationMode = SamusAccelerationMode.Accelerating;
                    SamusGroundedPoseTransitions.ApplyRunningToStanding(samus);
                }
                break;
            case SamusLookupFailureSetup.StopAimedRunning:
                if (fallback == source)
                    DecelerateRetainedRun(samus);
                else if (fallback is { } standing)
                {
                    samus.HorizontalSpeed.AccelerationMode = SamusAccelerationMode.Accelerating;
                    SamusGroundedPoseTransitions.ApplyGroundedAimTransition(samus, standing);
                }
                break;
            case SamusLookupFailureSetup.StopMoonwalk:
                // The stop selected the literal standing/aim fallback during the input phase.
                // Applying it after movement preserves the one-last-step timing.
                if (fallback is { } moonwalkFallback)
                    SamusGroundedPoseTransitions.ApplyMoonwalkPoseChange(samus, moonwalkFallback, _options.MoonwalkEnabled);
                break;
            case SamusLookupFailureSetup.LowerGroundedAim:
                // Only the input fallback path above populates this value; the aimed wall stops name the plain wall stops,
                // which share their radius, animation and cleanup. Validate and apply it through the same
                // radius/animation seam as held-input aim changes; never assign the authored value directly to Pose.
                if (fallback is { } aimFallback)
                    SamusGroundedPoseTransitions.ApplyGroundedAimTransition(samus, aimFallback);
                break;
            case SamusLookupFailureSetup.AbandonAimedCrouchingTurn:
                if (frame.UsePoseDefinitionFallback && samus.Pose == source && fallback is { } crouchingTurnFallback)
                    SamusGroundedPoseTransitions.ApplyAimedCrouchingTurnInputFallback(samus, crouchingTurnFallback);
                break;
            case SamusLookupFailureSetup.StopTurn:
                // A definition without a fallback pose retains the animation, not the momentum
                // response. The stop still cancels reverse acceleration,
                // for ground turns exactly as for aerial ones.
                if (frame.UsePoseDefinitionFallback && samus.Pose == source)
                    SamusGroundedPoseTransitions.ApplyTurnInputFallback(samus, frame.MovementTypeAtFrameStart);
                break;
            case SamusLookupFailureSetup.ContinueNormalJump:
                // An unchanged prospective pose must not reset animation or invoke
                // compact-radius expansion. Decelerating still folds momentum after
                // movement and rechecks the remaining base speed there.
                if (fallback is { } jumpFallback)
                {
                    if (jumpFallback != samus.Pose)
                        SamusAerialPoseTransitions.ApplySameFacingAerialTransition(samus, jumpFallback);
                    samus.HorizontalSpeed.ApplyDeceleratingInputFallback(
                        frame.DeceleratingFallbackHasMomentum, SamusPoseMetadata.ReadFacingDirection(samus));
                }
                break;
            case SamusLookupFailureSetup.LowerAerialAim:
                // Install the fallback's pose metadata and setup first.
                // Falling's run-speed kill, after this response, then clears only the extra component;
                // it must not be folded into base speed like running deceleration.
                // A retained compact pose selects history/momentum work only;
                // pose initialization and animation restart require a change.
                if (fallback is { } aerialFallback && aerialFallback != samus.Pose)
                    SamusAerialPoseTransitions.ApplySameFacingAerialTransition(samus, aerialFallback);
                break;
            default:
                throw new InvalidDataException($"Undefined {nameof(SamusLookupFailureSetup)} for pose {source}.");
        }
    }

    /// <summary>
    /// Retaining the running fallback selected deceleration in the input phase. After movement, fold extra dash speed into
    /// base and cancel boost before decelerating; changing only the mode leaves stale dash speed alive and changes the
    /// subsequent stopping trajectory.
    /// </summary>
    private static void DecelerateRetainedRun(SamusState samus) =>
        samus.HorizontalSpeed.ApplyDeceleratingInputFallback(selectedDeceleration: true, SamusPoseMetadata.ReadFacingDirection(samus));
}
