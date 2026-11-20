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
/// interruptions, the collision transitions, the input transition the input phase selected or the momentum response
/// chosen when her input chooses no new pose, and the pose-history commit.
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
    /// <summary>The steps the pose setups and the responses to no new pose run.</summary>
    private readonly SamusPoseSetupSteps _setups;

    /// <summary>Creates the phase over the frame phases' collaborators.</summary>
    internal SamusPoseTransitionPhase(SamusPoseTransitionPhaseDependencies dependencies)
    {
        _playerActions = dependencies.PlayerActions;
        _freeze = dependencies.Freeze;
        _clock = dependencies.Clock;
        _options = dependencies.Options;
        _player = dependencies.Player;
        _setups = new SamusPoseSetupSteps(_playerActions, _clock, () => _options.MoonwalkEnabled);
    }

    /// <summary>Applies post-movement pose and animation transitions after the frame's selected movement mode.</summary>
    internal void Apply(SamusControlFrame frame, SamusState samus, SamusTerrain terrain)
    {
        if (!_player.GroundedSamusMovementEnabled ||
            frame.Mode.Movement is SamusMovementMode.Fatal or SamusMovementMode.SuitPickup or SamusMovementMode.ScriptLocked)
            return;

        // The hit interruption sees the old movement type before the pose update, and its bomb rejection comes before
        // every pose transition, an animation's included.
        if (samus.HasNoKnockback)
            SamusPoseMetadata.RejectUnsupportedPublishedBombJump(samus, _freeze.TimeIsFrozen);

        // The pose the animation's pose-transition cue left pending outranks every other transition at this seam.
        // The turn's cue leaves a turn-completion pose only if the input phase did not select one
        // of its four jump exceptions. When left pending, it still wins here.
        // The hurt-expiry check reads hurt expiry before the pose update takes the animation's pending pose. That
        // decision stands even if the Shinespark's setup clears the live knockback direction while it sets up the
        // shinespark windup poses.
        bool hitInterruptionExpiryPending =
            SamusKnockbackMovement.IsExpiredHitInterruptionPending(samus);
        bool transitionApplied =
            SamusPoseTransitions.ApplyPendingAnimationTransition(samus);

        transitionApplied = ApplyInterruptionTransitions(frame, samus, transitionApplied,
            hitInterruptionExpiryPending, terrain);
        transitionApplied = ApplyCollisionTransitions(frame, samus, transitionApplied, terrain);

        if (!transitionApplied && _player.ProspectiveSamusPose is { } inputTransition)
            ApplyInputTransition(frame, samus, inputTransition, terrain);
        else if (!transitionApplied)
            ApplyNoTransitionResponse(frame, samus);

        // Crouching stops Samus's momentum only when her input chooses no new pose. A matched
        // angle-held pose must retain boost even though movement zeroed X speed.
        if (!transitionApplied && frame.InputChoseNoPose &&
            _player.ProspectiveSamusPose is null &&
            frame.MovementTypeAtFrameStart == SamusMovementType.Crouching &&
            SamusPoseMetadata.ReadMovementType(samus) == SamusMovementType.Crouching)
            samus.HorizontalSpeed.StopForNoTransition(SamusPoseMetadata.ReadFacingDirection(samus));

        // The windup still looks its input up as any special movement does: input none of its transitions matches stops
        // Samus's momentum on this update, as crouching does, unless a launch or the timeout comes first.
        if (!transitionApplied && frame.InputChoseNoPose &&
            _player.ProspectiveSamusPose is null &&
            frame.PoseAtFrameStart is SamusPoseId.ShinesparkWindupRightPose or SamusPoseId.ShinesparkWindupLeftPose &&
            samus.Shinespark.Phase == ShinesparkPhase.Windup)
            samus.HorizontalSpeed.StopForNoTransition(SamusPoseMetadata.ReadFacingDirection(samus));

        // Falling's no-transition momentum response still runs when its definition
        // retains the current pose. It follows pose initialization, and must
        // not run when a higher-priority collision or interruption won.
        if (!transitionApplied && frame.InputChoseNoPose &&
            _player.ProspectiveSamusPose is null &&
            frame.MovementTypeAtFrameStart == SamusMovementType.Falling &&
            SamusPoseMetadata.ReadMovementType(samus) == SamusMovementType.Falling)
            SamusAerialPoseTransitions.StopRunWhenFallingWithoutTransition(samus);

        // The normal-jump setup publishes the fresh Shoot direction
        // for every accepted route, including aim changes within a jump. The
        // projectile step earlier in this update cleared the previous update's handoff.
        if (samus.Pose != samus.PoseHistory.PreviousPose)
            SamusAerialPoseTransitions.ArmNormalJumpTransitionShot(samus, _playerActions.NewlyPressed);

        // The pose history moves on after any chosen pose transition takes effect,
        // including a transition to the same pose. An update that chose no transition
        // does not move it. It moves on after the update's final collision and pose choice.
        // The grapple launch applies its transition pose early and clears the ordinary
        // input phase's input. It still reaches this same history step, once, after the
        // forced launch pose.
        // X-ray's activation installs its pose and movement early and clears
        // the ordinary input phase's targets. Its final history commit happens here.
        if (transitionApplied || frame.XrayActivatedThisFrame || frame.GrappleMovement is { WallJumpStarted: true } ||
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
        bool transitionApplied = alreadyApplied;
        // The animation's transition queues a transitional pose rather than replacing movement.
        // Commit after animation, suppressing the lower-priority input/grounding
        // pose. Its speed clears already happened before movement.
        if (!transitionApplied && frame.GrappleMovement is { PendingDropPose: SamusPoseId dropPose })
        {
            SamusAerialPoseTransitions.ApplyGrappleDropTransition(samus,
                terrain.Level,
                dropPose, _clock.Counter, terrain.RoomObjects);
            _player.ClearProspectivePoses();
            _player.ProspectiveSamusWallCollisionPose = null;
            transitionApplied = true;
        }

        // The grapple release queues its release pose the same way, so the hit interruption
        // earlier this frame still saw the grappling movement type.
        if (!transitionApplied && frame.GrappleMovement is { PendingReleasePose: SamusPoseId releasePose })
        {
            SamusGrappleMovement.ApplyReleasePose(samus, releasePose);
            _player.ClearProspectivePoses();
            _player.ProspectiveSamusWallCollisionPose = null;
            transitionApplied = true;
        }

        // Crash movement queues a transitional pose; it must not replace the
        // body Samus's animation saw earlier in this frame. The animation's
        // pending transition pose retains its higher priority.
        if (!transitionApplied && frame.ShinesparkMovement is { CrashSequenceFinished: true })
        {
            SamusShinesparkState.ApplyCrashFinishPose(samus);
            _player.ClearProspectivePoses();
            _player.ProspectiveSamusWallCollisionPose = null;
            transitionApplied = true;
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
            transitionApplied = true;
        }

        // The pose interruption runs after Samus's animation and before the pose update. Terrain
        // spike reactions only publish their damage request; this common interruption
        // converts that request into the same hurt pose and knockback movement owner
        // used by every damage producer. The animation's pending transition pose retains
        // its higher pose-update priority and therefore suppresses this interruption.
        if (!transitionApplied &&
            SamusKnockbackMovement.TryStartPendingHitInterruption(
                samus,
                _playerActions.Current,
                _freeze.TimeIsFrozen,
                new KnockbackPoseCollision(terrain, _clock.Counter)))
        {
            _player.ClearProspectivePoses();
            _player.ProspectiveSamusWallCollisionPose = null;
            transitionApplied = true;
        }

        // Bomb overlap is published between the input and movement phases, but the game's
        // interruption step considers it only AFTER movement/animation.
        // Active hurt values retain the request without arming or rejecting it;
        // expiry owns this frame even after clearing those values. This permits
        // a later bomb launch to retain the damaged pose, with input locked by
        // the armed bomb jump rather than an early speculative pose change.
        if (!transitionApplied && samus.HasNoKnockback &&
            SamusPoseMetadata.TrySetupPublishedBombJump(samus,
                terrain.Level,
                _freeze.TimeIsFrozen,
                _clock.Counter,
                _playerActions.NewlyPressed,
                terrain.RoomObjects))
        {
            _player.ClearProspectivePoses();
            _player.ProspectiveSamusWallCollisionPose = null;
            transitionApplied = true;
        }

        // The windup's end starts her vertical movement at once; Samus's animation sees the windup body on this update,
        // and the launch pose is installed afterwards.
        if (!transitionApplied &&
            frame.ShinesparkMovement is { PendingLaunchPose: SamusPoseId launchPose })
        {
            SamusShinesparkState.ApplyDirectionalLaunchPose(samus, launchPose);
            _player.ClearProspectivePoses();
            _player.ProspectiveSamusWallCollisionPose = null;
            transitionApplied = true;
        }

        if (frame.XrayActivation is { Pose: { } xrayPose } && samus.Xray.CommitActivation(samus, xrayPose, transitionApplied))
        {
            _player.ClearProspectivePoses();
            _player.ProspectiveSamusWallCollisionPose = null;
            transitionApplied = true;
        }

        // Grapple connection publishes a pending connection pose.
        // The animation and hurt transitions above can suppress it without
        // undoing the beam phase already entered.
        if (!transitionApplied &&
            frame.GrappleMovement is { PendingConnection: { } pendingConnection })
        {
            Position16<RoomPixels> grapplePrevious =
                SamusGrappleMovement.ApplyPendingConnectionPose(
                    samus,
                    pendingConnection);
            frame.SnapCameraPoint(grapplePrevious);
            _player.ClearProspectivePoses();
            _player.ProspectiveSamusWallCollisionPose = null;
            transitionApplied = true;
        }

        return transitionApplied;
    }

    /// <summary>
    /// Applies the collision results that replace the input phase's input target: ceilings, the bomb arc's fall, running into a
    /// wall, Morph Ball, wall-jump and Spring Ball landings, ordinary landings and walking off a floor. Returns whether
    /// a transition has been applied.
    /// </summary>
    private bool ApplyCollisionTransitions(SamusControlFrame frame, SamusState samus, bool alreadyApplied, SamusTerrain terrain)
    {
        bool transitionApplied = alreadyApplied;
        // The ceiling collision replaces the ordinary input target with the current pose
        // on ceiling collision. Apply the ball's deferred vertical
        // stop and consume this same-pose transition so history still shifts
        // and an input-phase aim/turn selection cannot override it. Higher-priority
        // animation and hurt interruptions above retain their priority.
        // Ball movement can resume before the bomb arc reaches a ceiling;
        // its collision must also outrank the stop-rolling input fallback.
        if (!transitionApplied &&
            (frame.AerialMovement is { HitCeiling: true } ||
             frame.MorphBallMovement is { HitCeiling: true } ||
             frame.BombJumpMovement is { Vertical.Collided: true }))
        {
            if (frame.MorphBallMovement is { HitCeiling: true })
                SamusPosturePoseTransitions.ApplySolidCeilingCollision(samus);
            _player.ClearProspectivePoses();
            transitionApplied = true;
        }

        // The bomb-arc check overrides the input target when the bomb arc moved down through
        // its apex with no floor below; the airborne pose is installed this frame.
        if (!transitionApplied &&
            frame.BombJumpMovement is { FellWithoutFloor: true })
        {
            SamusPosturePoseTransitions.ApplyBombJumpFallingPose(samus, terrain, _clock.Counter);
            _player.ClearProspectivePoses();
            transitionApplied = true;
        }

        // The wall-stop check runs inside the pose update after movement/animation and
        // only when no animation or interruption transition has already won. It first
        // reads whether this update's running movement stopped her speed against a wall. This creates a wall-stop pose
        // even with no input match.
        // An unobstructed upward platform carry leaves the prospective input
        // pose intact too, so the one-pixel run check runs while a Kamer carries Samus upward.
        // A special movement owner can also retain a standing/running pose
        // (suit-interrupted windup). The wall-stop check does not require the normal grounded movement phase.
        if (!transitionApplied &&
            frame.GroundedMovement is not
                { Vertical.IsUnobstructedDownwardMovement: true })
        {
            bool currentRunHitWall =
                (SamusPoseMetadata.IsRightFacingRunningPose(frame.PoseAtFrameStart) ||
                 SamusPoseMetadata.IsLeftFacingRunningPose(frame.PoseAtFrameStart)) &&
                frame.GroundedMovement is { Horizontal.Collided: true };
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
                    terrain.RoomObjects);
        }

        if (!transitionApplied &&
            _player.ProspectiveSamusWallCollisionPose is { } wallCollisionPose)
        {
            // The wall-stop pose replaces the input transition, including when no input was held and the run's speed
            // was stopped.
            SamusGroundedPoseTransitions.ApplyRanIntoWallPoseChange(samus, wallCollisionPose);
            transitionApplied = true;
            // The wall-stop check replaces only ProspectivePose. When the input phase matched no input
            // transition, the pose update still runs the momentum response
            // selected for the pre-wall movement type.
            if (_player.ProspectiveSamusPose is null && _player.ProspectiveSamusFallbackPose is not null)
            {
                SamusMomentumResponse response = SamusMomentumResponses.ForNoTransition(
                    SamusPoseMetadata.ReadMovementType(frame.PoseAtFrameStart));
                samus.HorizontalSpeed.DecelerateForNoTransition(
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
        // movement left that pose with Y speed.
        if (!transitionApplied &&
            frame.MorphBallMovement is { Landed: true } &&
            SamusPoseMetadata.IsAirborneMorphBallPose(frame.PoseAtFrameStart))
        {
            SamusPosturePoseTransitions.ApplyMorphBallLanding(samus);
            transitionApplied = true;
        }

        // The wall-jump check publishes its wall-jump collision result and reports
        // a collision, preventing vertical motion. The wall-jump trigger
        // installs the wall-jump pose, clears old momentum, and launches from fixed constants.
        if (!transitionApplied &&
            frame.AerialMovement is { WallJumpTriggered: true })
        {
            SamusAerialPoseTransitions.ApplyWallJumpTrigger(samus, terrain.Level, _clock.Counter, terrain.RoomObjects);
            transitionApplied = true;
        }

        // Spring Ball's three movement families share one collision transition, but
        // Spring Ball's movement makes held Jump an immediate relaunch and stores the jump intent
        // during automatic rebounds, unlike the ordinary ball.
        // The grounded Spring Ball movement type likewise lands with no transition.
        if (!transitionApplied &&
            frame.MorphBallMovement is { Landed: true } &&
            SamusPoseMetadata.IsAirborneSpringBallPose(frame.PoseAtFrameStart))
        {
            SamusPosturePoseTransitions.ApplySpringBallLanding(samus, _playerActions.Current);
            transitionApplied = true;
        }

        // A downward collision publishes a landing. The landing check chooses normal or
        // spin landing from the movement type that executed this frame, then settling
        // the accepted landing clears both velocity axes. This ordinary prospective pose
        // is lower priority than the animation's pending transition pose above.
        if (!transitionApplied &&
            frame.AerialMovement is { Landed: true } &&
            !SamusPoseMetadata.IsAerialTurnPose(frame.PoseAtFrameStart))
        {
            if (SamusPoseMetadata.IsCompactAerialPose(frame.PoseAtFrameStart))
            {
                SamusPoseTransitions.ApplyCompactAerialLanding(samus,
                    terrain.Level,
                    _clock.Counter,
                    terrain.RoomObjects);
            }
            else
            {
                bool wasSpinning = SamusPoseMetadata.IsSpinJumpPose(frame.PoseAtFrameStart) ||
                    SamusPoseMetadata.IsWallJumpPose(frame.PoseAtFrameStart);
                SamusPoseTransitions.ApplyAerialLandingInRoom(samus,
                    terrain.Level,
                    wasSpinning,
                    _playerActions.Current,
                    _clock.Counter,
                    terrain.RoomObjects);
            }
            transitionApplied = true;
        }

        // An unobstructed DOWNWARD grounding probe publishes SamusVerticalCollisionResult.Falling. An
        // unobstructed upward platform carry publishes SamusVerticalCollisionResult.None
        // and leaves the pose alone, so Samus rising on a Kamer does not fall and land over and over. For admitted
        // standing (including landing art), running, and stable crouching types,
        // losing the floor selects the first airborne family and the falling check chooses falling art.
        // The ground and jumping turns change no pose when they lose the floor;
        // they finish their turn transition, then the destination pose detects the missing floor.
        if (!transitionApplied &&
            frame.GroundedMovement is
                { Vertical.IsUnobstructedDownwardMovement: true } &&
            SamusPoseMetadata.WalksOffFloor(frame.PoseAtMovement))
        {
            SamusPoseTransitions.ApplyWalkedOffFloorTransition(samus,
                terrain.Level,
                _clock.Counter, terrain.RoomObjects);
            transitionApplied = true;
        }

        // SamusMovementType.MorphBallGround movement publishes the ordinary airborne morph ball poses when its
        // no-speed grounding probe finds no floor. The shared rolling animation is
        // preserved, while Y speed starts at zero/down exactly like the falling check.
        if (!transitionApplied &&
            frame.MorphBallMovement is
                { Vertical.IsUnobstructedDownwardMovement: true } &&
            (SamusPoseMetadata.IsGroundedMorphBallPose(frame.PoseAtFrameStart) ||
             SamusPoseMetadata.IsGroundedSpringBallPose(frame.PoseAtFrameStart)))
        {
            SamusPosturePoseTransitions.ApplyMorphBallWalkOff(samus);
            transitionApplied = true;
        }

        return transitionApplied;
    }

    /// <summary>
    /// Applies the input transition the input phase selected from the pose's input transitions, through the setup the
    /// transition names. A transition that keeps her pose means "remain in this animation", so nothing is set up.
    /// </summary>
    private void ApplyInputTransition(SamusControlFrame frame, SamusState samus, SamusPoseTransition inputTransition, SamusTerrain terrain)
    {
        SamusPoseId targetPose = inputTransition.ProspectivePose;
        inputTransition.Setup.Apply(_setups, frame, samus, targetPose, terrain);
    }

    /// <summary>
    /// Applies the setup Samus's frame-start pose, or else the movement her last pose setup left her in, names for the input
    /// phase when her input chose no new pose and no transition won (<see cref="SamusNoTransitionSetups"/>), from the
    /// fallback pose the input phase chose.
    /// </summary>
    private void ApplyNoTransitionResponse(SamusControlFrame frame, SamusState samus)
    {
        SamusPoseId source = frame.PoseAtFrameStart;
        SamusPoseId? fallback = _player.ProspectiveSamusFallbackPose;
        SamusNoTransitionSetups.For(source, frame.MovementTypeAtFrameStart).Apply(_setups, frame, samus, source, fallback);
    }
}
