using SuperMetroid.Core.Input;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.World;

/// <summary>
/// UpdateSamusPose after normal beta's animation: animation command three, the interruptions, the collision
/// transitions, the input transition alpha selected or its lookup-failure command, and the pose-history commit
/// ($91:EB88).
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
    internal void Apply(SamusHandlerFrame frame, SamusState samus, ISamusRoomPlms plms)
    {
        if (!_player.GroundedSamusMovementEnabled ||
            frame.Mode.Beta is SamusBetaHandler.Fatal or SamusBetaHandler.SuitPickup or SamusBetaHandler.ScriptLocked)
            return;

        // Hit interruption observes the old movement type before UpdateSamusPose.
        // Its carry-clear bomb rejection still occurs when an animation transition
        // wins pose selection; otherwise a stale request survives unmorph/morph.
        if (samus.KnockbackTimer.IsZero && samus.KnockbackDirection == SuperMetroid.Core.Samus.Movement.SamusKnockbackDirection.None)
            SamusPoseMetadata.RejectUnsupportedPublishedBombJump(samus, _freeze.TimeIsFrozen);

        // Command $F8's command-three “super-special” transition wins at this seam.
        // F8 publishes a turn-completion pose only if alpha did not select one
        // of its four jump exceptions. When published, it still wins here.
        // `$90:DDE9` samples hurt expiry before bank $91 applies a queued
        // animation command. Preserve that decision even if Shinespark setup
        // clears the live knockback direction while installing `$C7/$C8`.
        bool hitInterruptionExpiryPending =
            SamusKnockbackMovement.IsExpiredHitInterruptionPending(samus);
        bool animationTransitionApplied =
            SamusPoseTransitions.ApplyPendingVerifiedAnimationTransition(samus);

        animationTransitionApplied = ApplyInterruptionTransitions(frame, samus, animationTransitionApplied,
            hitInterruptionExpiryPending, plms);
        animationTransitionApplied = ApplyCollisionTransitions(frame, samus, animationTransitionApplied, plms);

        if (!animationTransitionApplied && _player.ProspectiveSamusPose is { } inputTransition)
            ApplyInputTransition(frame, samus, inputTransition, plms);
        else if (!animationTransitionApplied)
            ApplyLookupFailureCommand(frame, samus);

        // Crouching selects command two only on lookup failure. A matched
        // angle-held pose must retain boost even though movement zeroed X speed.
        if (!animationTransitionApplied && frame.UsePoseDefinitionFallback &&
            _player.ProspectiveSamusPose is null &&
            frame.MovementTypeAtFrameStart == SamusMovementType.Crouching &&
            SamusPoseMetadata.ReadMovementType(samus) == SamusMovementType.Crouching)
            samus.HorizontalSpeed.ApplyStoppedInputFallback(SamusPoseMetadata.ReadFacingDirection(samus));

        // Windup still runs the ordinary type-$1B input lookup. Its unmatched
        // chord selects command two at $91:8304, just like crouching. Skipping
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

        // F433's normal-jump initializer publishes the fresh Shoot direction
        // for every accepted route, including aim changes within a jump. The
        // earlier projectile epilogue cleared the previous update's handoff.
        if (samus.Pose != samus.PoseHistory.PreviousPose)
            SamusAerialPoseTransitions.PublishNormalJumpPoseShotDirection(samus, _controller1.NewlyPressed);

        // $91:EB88 shifts history after consuming any transition slot,
        // including a self-transition. A frame with no selected transition
        // does not shift it. Keep this after final collision/pose selection.
        // The translated grapple launch applies its $9B:C9CE transitional
        // slot early and clears ordinary alpha input. It still reaches this
        // same native history epilogue, once, after the forced launch pose.
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
        bool hitInterruptionExpiryPending, ISamusRoomPlms plms)
    {
        bool animationTransitionApplied = alreadyApplied;
        // C8C5 queues a transitional pose rather than replacing beta movement.
        // Commit after animation, suppressing the lower-priority input/grounding
        // pose. Its speed clears already happened before movement in bank $9B.
        if (!animationTransitionApplied && _player.LastGrappleMovement is { PendingDropPose: SamusPoseId dropPose })
        {
            SamusAerialPoseTransitions.ApplyGrappleDropTransition(samus, 
                _room.LevelData ?? throw new InvalidOperationException("Grapple drop requires room geometry."),
                dropPose, _clock.Counter, plms, speed: GrappleDropSpeed.Kept);
            _player.ProspectiveSamusPose = null;
            _player.ProspectiveSamusFallbackPose = null;
            _player.ProspectiveSamusWallCollisionPose = null;
            animationTransitionApplied = true;
        }

        // $9B:CB8B queues its release pose the same way, so the hit interruption
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
        // body seen by AnimateSamus earlier in this frame. Command-three
        // animation transitions retain their higher cartridge priority.
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
        // With an animation command three already pending, native changes it
        // to command eight: keep the animation pose and ALSO clear hurt state.
        if (hitInterruptionExpiryPending)
        {
            SamusKnockbackMovement.FinishExpiredHitInterruption(samus);
            _player.ProspectiveSamusPose = null;
            _player.ProspectiveSamusFallbackPose = null;
            _player.ProspectiveSamusWallCollisionPose = null;
            animationTransitionApplied = true;
        }

        // `$90:DDE9` runs after AnimateSamus and before UpdateSamusPose. Terrain
        // spike reactions only publish `$18A8/$18AA/$0A54`; this common interruption
        // converts that request into the same hurt pose and special movement handler
        // used by every cartridge damage producer. Animation command three retains
        // its higher UpdateSamusPose priority and therefore suppresses this branch.
        if (!animationTransitionApplied &&
            SamusKnockbackMovement.TryStartPendingHitInterruption(

                samus,
                _controller1.Current,
                _freeze.TimeIsFrozen,
                _room.LevelData,
                _clock.Counter,
                plms))
        {
            _player.ProspectiveSamusPose = null;
            _player.ProspectiveSamusFallbackPose = null;
            _player.ProspectiveSamusWallCollisionPose = null;
            animationTransitionApplied = true;
        }

        // Bomb overlap is published between alpha and beta, but the native
        // interruption dispatcher considers it only AFTER movement/animation.
        // Active hurt words retain the request without arming or rejecting it;
        // expiry owns this frame even after clearing those words. This permits
        // a later bomb launch to retain the damaged pose, with input locked by
        // special command three rather than an early speculative pose change.
        if (!animationTransitionApplied && samus.KnockbackTimer.IsZero &&
            samus.KnockbackDirection == SuperMetroid.Core.Samus.Movement.SamusKnockbackDirection.None &&
            SamusPoseMetadata.TrySetupPublishedBombJump(samus, 

                _room.LevelData ?? throw new InvalidOperationException(
                    "A published bomb-jump direction requires active room level data."),
                _freeze.TimeIsFrozen,
                _clock.Counter,
                _controller1.NewlyPressed,
                plms))
        {
            _player.ProspectiveSamusPose = null;
            _player.ProspectiveSamusFallbackPose = null;
            _player.ProspectiveSamusWallCollisionPose = null;
            animationTransitionApplied = true;
        }

        // Windup expiry installs vertical movement immediately, but publishes
        // an interrupted pose for this later phase. AnimateSamus must still see
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

        // Grapple connection publishes a bank-$91 special prospective command.
        // Super-special animation/hurt owners above can suppress it without
        // undoing the already-installed bank-$9B beam function. This is the
        // cartridge ordering used by bounded Grapple momentum-retention setups.
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
    private bool ApplyCollisionTransitions(SamusHandlerFrame frame, SamusState samus, bool alreadyApplied, ISamusRoomPlms plms)
    {
        bool animationTransitionApplied = alreadyApplied;
        // $91:E8E5 replaces the ordinary input target with the current pose
        // on ceiling collision. Apply the ball's deferred EFDF vertical
        // stop and consume this same-pose transition so history still shifts
        // and an alpha aim/turn selection cannot override it. Higher-priority
        // animation and hurt interruptions above retain their native priority.
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

        // $91:E8F2 overrides the input target when the bomb arc moved down through
        // its apex with no floor below; the airborne pose is installed this frame.
        if (!animationTransitionApplied &&
            _player.LastBombJumpMovement is { FellWithoutFloor: true })
        {
            SamusPosturePoseTransitions.ApplyBombJumpFallingPose(samus);
            _player.ProspectiveSamusPose = null;
            _player.ProspectiveSamusFallbackPose = null;
            animationTransitionApplied = true;
        }

        // `$91:EADE` runs inside UpdateSamusPose after beta movement/animation and
        // only when no super-special animation command has already won. Its first
        // branch consumes the X-speed-killed flag produced by CURRENT type-one
        // running movement. This creates a wall-stop pose even with no input match.
        // An unobstructed upward platform carry leaves the prospective input
        // pose intact too. Requiring a vertical collision here wrongly skipped
        // the native one-pixel run check while a Kamer was carrying Samus upward.
        // A special movement pointer can also retain a standing/running pose
        // (suit-interrupted windup). EADE does not require normal grounded beta.
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
                    // Native alpha writes the same prospective-pose slot for a
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
                    plms);
        }

        if (!animationTransitionApplied &&
            _player.ProspectiveSamusWallCollisionPose is { } wallCollisionPose)
        {
            // The selector result replaces the ordinary input target. Applying it
            // here also covers the no-input “running speed was killed” branch.
            SamusGroundedPoseTransitions.ApplyRanIntoWallPoseChange(samus, wallCollisionPose);
            animationTransitionApplied = true;
            // `$91:EADE` replaces only ProspectivePose. When alpha's table lookup
            // failed, UpdateSamusPose ($91:EBF0) still runs the command `$91:8304`
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
        // impact retains `$31/$32` and launches bounce one; the next collision
        // launches bounce two; only a gentle/second-bounce collision installs
        // grounded `$1D/$41`. Treat every branch as a consumed transition so input
        // selected before collision cannot override the cartridge's bounce result.
        // The ROM collision table ignores ground-family type four, even if hurt
        // movement left that pose with nonzero Y speed. Do not infer an airborne
        // landing dispatcher from velocity alone.
        if (!animationTransitionApplied &&
            _player.LastMorphBallMovement is { Landed: true } &&
            SamusPoseMetadata.IsAirborneMorphBallPose(frame.PoseAtFrameStart))
        {
            SamusPosturePoseTransitions.ApplyMorphBallLanding(samus);
            animationTransitionApplied = true;
        }

        // `$90:9D35` publishes solid-vertical collision result five and returns
        // carry set, preventing vertical motion. The matching bank-$91 command
        // installs `$83/$84`, clears old momentum, and launches from ROM constants.
        if (!animationTransitionApplied &&
            _player.LastAerialSamusMovement is { WallJumpTriggered: true })
        {
            SamusAerialPoseTransitions.ApplyWallJumpTrigger(samus, _room.LevelData, _clock.Counter, plms);
            animationTransitionApplied = true;
        }

        // Spring Ball's three movement families share collision result three, but
        // `$91:F25E` makes held Jump an immediate relaunch and stores `$0601/$0602`
        // during automatic rebounds. Keep it distinct from ordinary-ball state.
        // Ground-family type $11 likewise has the table's no-transition entry.
        if (!animationTransitionApplied &&
            _player.LastMorphBallMovement is { Landed: true } &&
            SamusPoseMetadata.IsAirborneSpringBallPose(frame.PoseAtFrameStart))
        {
            SamusPosturePoseTransitions.ApplySpringBallLanding(samus, _controller1.Current);
            animationTransitionApplied = true;
        }

        // A downward collision publishes result one. $91:E95D chooses normal or
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
                    plms);
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
                    plms);
            }
            animationTransitionApplied = true;
        }

        // An unobstructed DOWNWARD grounding probe publishes result two. An
        // unobstructed upward platform carry publishes result zero at `$90:E606`
        // and must leave the pose alone; treating every non-collision as result two
        // made rising Kamers repeatedly alternate falling and landing. For admitted
        // standing (including landing art), running, and stable crouching types,
        // data zero at `$90:E65A`
        // selects airborne family zero and `$91:E8F2` chooses falling art.
        // Turn types `$0E/$17` contain `$04` (no pose change) in that literal table;
        // they finish `$F8`, then the destination pose detects the missing floor.
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
                fallingPose, _clock.Counter, plms);
            animationTransitionApplied = true;
        }

        // Movement type four publishes ordinary airborne `$31/$32` when its
        // no-speed grounding probe finds no floor. The shared rolling animation is
        // preserved, while Y speed starts at zero/down exactly like `$91:E8F2`.
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
    private void ApplyInputTransition(SamusHandlerFrame frame, SamusState samus, SamusPoseTransition inputTransition, ISamusRoomPlms plms)
    {
        SamusPoseId targetPose = unchecked(inputTransition.ProspectivePose);

        // Reinitializing frame zero would prevent $F8 from ever being reached.
        if (targetPose == frame.PoseAtFrameStart)
            return;
        if (!TryApplySpecialBodyTransition(frame, samus, targetPose, plms) && !TryApplyAimRunOrTurnTransition(frame, samus, targetPose))
            ApplyJumpCrouchOrPostureTransition(frame, samus, targetPose, plms);
    }

    /// <summary>
    /// Applies a Draygon-grab, moonwalk, damage-boost, aerial-turn, spin, Morph Ball or compact aerial transition.
    /// Returns false when the pair belongs to a later family.
    /// </summary>
    private bool TryApplySpecialBodyTransition(SamusHandlerFrame frame, SamusState samus, SamusPoseId targetPose, ISamusRoomPlms plms)
    {
        switch (new SamusPoseChange(frame.PoseAtFrameStart, targetPose))
        {
            case var (source, target)
                when SamusPoseMetadata.IsDraygonGrabbedPose(source) &&
                     SamusPoseMetadata.IsDraygonGrabbedPose(target):
                // Both ROM tables stay within one facing. The helper makes
                // that invariant explicit and initializes the target's real
                // `$91:B2B4/$B53C` animation list.
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
                // The stable `$49/$4A/$75-$78` table publishes a generic
                // reversal target. `$91:F8D3` validates it against the old
                // shot direction, folds momentum, and leaves `$BF-$C4`
                // grounded until either its input table or `$F8` starts jump.
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
                // `$2F/$30/$87/$88` are generic table outputs. The helper
                // reads the source shot-direction record, chooses the exact
                // `$8F-$A1` art when needed, folds momentum, and runs compact
                // pose-expansion collision before committing the turn.
                SamusAerialPoseTransitions.TryApplyAerialTurn(samus, 

                    _room.LevelData ?? throw new InvalidOperationException(
                        "Aerial turn requires active room level data."),
                    target,
                    _clock.Counter,
                    plms);
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
                // `$1D/$1E/$1F/$31/$32/$41` all share delay list `$B378`.
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
                when ((SamusPoseMetadata.ReadPoseXDirection(source) == 8 &&
                       target == SamusPoseId.MorphingTransitionRightPose) ||
                      (SamusPoseMetadata.ReadPoseXDirection(source) == 4 &&
                       target == SamusPoseId.MorphingTransitionLeftPose) ||
                      (SamusPoseMetadata.IsStableBallPose(source) &&
                       target is SamusPoseId.UnmorphingTransitionRightPose or
                           SamusPoseId.UnmorphingTransitionLeftPose)):
                SamusPosturePoseTransitions.TryApplyMorphTransition(samus, 

                    _room.LevelData ?? throw new InvalidOperationException(
                        "Morph transition requires active room level data."),
                    targetPose,
                    _clock.Counter,
                    plms);
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
                    plms);
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
                // Ceres releases controls while `$00` is still active.
                // Its own ROM transition table maps Left/Right to generic
                // `$25/$26`; `$91:F8D3` recognizes old pose `$00/$9B` and
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
    private void ApplyJumpCrouchOrPostureTransition(SamusHandlerFrame frame, SamusState samus, SamusPoseId targetPose, ISamusRoomPlms plms)
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
                // All `$A4-$A7/$E0-$E7` landing records expose the same
                // standing input table while their brief streams are active.
                // A fresh Jump edge can therefore preserve any authored
                // normal-jump aim target, even from horizontal-fire `$E6/$E7`
                // before `$F8` fallback.
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
                // Every standing `$0E` turn—including the aimed `$8B-$8E`
                // and `$9C/$9D` records—continues consulting `$91:8142`.
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
                // `$91:A66C/$91:A6BC` use the ordinary `$4B/$4C` transition
                // art for every crouched aim direction. The helper preserves
                // `$91:FC7D`'s literal `$27/$28`-only ten-pixel adjustment.
                SamusPosturePoseTransitions.TryApplyCrouchJumpTransition(samus, 

                    _room.LevelData ?? throw new InvalidOperationException(
                        "Crouch jump requires active room level data."),
                    targetPose,
                    _clock.Counter,
                    plms);
                break;
            case var (source, target)
                when (SamusPoseMetadata.IsSpinJumpPose(source) ||
                      SamusPoseMetadata.IsWallJumpPose(source)) &&
                     (SamusPoseMetadata.IsRightFacingNormalJumpPose(target) ||
                      SamusPoseMetadata.IsLeftFacingNormalJumpPose(target)):
                // Aim and Fire cancel both compact spin and `$83/$84` wall-
                // jump bodies through the same normal-jump initializer without
                // restarting the jump arc. `$91:A9EC`, for example, maps Shot
                // directly from `$83` to `$13`. The shared helper performs
                // changed-pose collision for every radius expansion; a rejected
                // body leaves the compact source pose installed.
                SamusAerialPoseTransitions.TryApplySpinOrWallJumpToNormalJumpTransition(samus, 

                    _room.LevelData ?? throw new InvalidOperationException(
                        "Spin-to-normal-jump transition requires active room level data."),
                    targetPose,
                    _clock.Counter,
                    _controller1.NewlyPressed,
                    plms);
                break;
            case var (source, target)
                when (source == SamusPoseId.UnmorphingTransitionRightPose &&
                      SamusPoseMetadata.IsRightFacingFallingPose(target)) ||
                     (source == SamusPoseId.UnmorphingTransitionLeftPose &&
                      SamusPoseMetadata.IsLeftFacingFallingPose(target)):
                // Route an animation/collision-selected falling body through
                // ordinary expansion collision while preserving the fall.
                // Posture transitions do not poll controller pose tables:
                // the type-$0F input handler returns before lookup.
                SamusPosturePoseTransitions.TryApplyUnmorphToFallingTransition(samus, 

                    _room.LevelData ?? throw new InvalidOperationException(
                        "Unmorph-to-falling transition requires active room level data."),
                    targetPose,
                    _clock.Counter,
                    plms);
                break;
            case var (source, target)
                when ((SamusPoseMetadata.IsRightFacingCrouchingPose(source) &&
                       target == SamusPoseId.FacingRightNormalPose) ||
                      (SamusPoseMetadata.IsLeftFacingCrouchingPose(source) &&
                       target == SamusPoseId.FacingLeftNormalPose)):
                // These direct `$01/$02` records bypass the animated
                // `$F7-$FC` stand-up family, but still run pose-expansion
                // collision before installing the final standing body.
                SamusPosturePoseTransitions.TryApplyDirectCrouchToStandingTransition(samus, 

                    _room.LevelData ?? throw new InvalidOperationException(
                        "Direct crouch exit requires active room level data."),
                    targetPose,
                    _clock.Counter,
                    plms);
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
                    plms);
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
                // `$91:AD6C/$AD80` are ordinary held/new input records, but
                // `$91:F80F` installs a special movement pointer instead of
                // invoking a normal movement-type initializer.
                SamusAerialPoseTransitions.ApplyShinesparkDirectionTransition(samus, 
                    targetPose);
                break;
            default:
                // MOVEMENT_COVERAGE exhaustively audits every active retail
                // transition pair entering this dispatcher. A ROM match not
                // owned by one of the explicit families above means the pose
                // table or state was mixed from an incompatible revision.
                throw new InvalidDataException(
                    $"Grounded input transition {frame.PoseAtFrameStart} -> " +
                    $"{targetPose} escaped the exhaustive retail dispatcher.");
        }
    }

    /// <summary>
    /// Applies the command alpha's lookup failure selected ($91:82D9 and $91:8304) when no transition won: hurt and
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
            // Releasing every boost chord reaches definition byte two, `$4D/$4E`:
            // the same initializer as the table's held-Jump exit.
            SamusKnockbackMovement.ApplyDamageBoostPoseTransition(
                samus,
                unchecked(boostFallback),
                _controller1.NewlyPressed);
        }
        else if (SamusPoseMetadata.IsDraygonGrabbedPose(frame.PoseAtFrameStart) &&
                 _player.ProspectiveSamusFallbackPose is { } draygonFallback)
        {
            // Zero input uses pose-definition byte two, returning aimed/firing/
            // struggling art to neutral `$BA/$EC` without touching owner placement.
            SamusPoseMetadata.ApplyDraygonGrabbedPoseChange(samus, 
                unchecked(draygonFallback));
        }
        else if (SamusPoseMetadata.IsWallJumpPose(frame.PoseAtFrameStart) &&
                 _player.ProspectiveSamusFallbackPose is
                     SamusPoseId.SpinJumpRightPose or SamusPoseId.SpinJumpLeftPose)
        {
            // Definition byte two leaves the launch animation for ordinary spin
            // art. Type $14 selects command six in $91:8304. HandleSamusPoseChange
            // ($91:F426) sets carry only when InitializeSamusPose itself replaces the
            // installed pose; then UpdateSamusPose ($91:EBEE) skips the command. So
            // a spin jump keeps its launch speed only when initialization promotes it
            // to Space Jump or Screw Attack; otherwise EC85 clears base/extra speed.
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
            // Spring Ball in air/falling (types $12/$13) select command six in
            // $91:8304. After this frame's movement, $91:EC85 clears mode and base
            // speed and falls through to $91:EC8E's boost/extra-speed cancel.
            samus.HorizontalSpeed.ClearHorizontalMomentum(SamusPoseMetadata.ReadFacingDirection(samus));
        }
        else if (frame.PoseAtFrameStart is SamusPoseId.MorphBallGroundRightPose or SamusPoseId.MorphBallGroundLeftPose or
                     SamusPoseId.SpringBallGroundRightPose or SamusPoseId.SpringBallGroundLeftPose &&
                 _player.ProspectiveSamusFallbackPose == frame.PoseAtFrameStart)
        {
            // $91:EC85 clears base/mode, then cancels extra running momentum.
            // Consume the same-pose command after this frame's movement.
            samus.HorizontalSpeed.ClearHorizontalMomentum(SamusPoseMetadata.ReadFacingDirection(samus));
        }
        else if ((frame.PoseAtFrameStart is SamusPoseId.MorphBallMovingRightPose or
                     SamusPoseId.MorphBallMovingLeftPose or
                     SamusPoseId.SpringBallMovingRightPose or
                     SamusPoseId.SpringBallMovingLeftPose) &&
                 _player.ProspectiveSamusFallbackPose == frame.PoseAtFrameStart)
        {
            // Prospective command one rechecks the post-movement base words. A
            // residue selects mode two deceleration; exact zero falls through to
            // command two's stopped mode and definition fallback on the next frame.
            samus.HorizontalSpeed.AccelerationMode =
                samus.HorizontalSpeed.BaseFixed != 0 ? SamusAccelerationMode.Decelerating : SamusAccelerationMode.Accelerating;
        }
        else if ((frame.PoseAtFrameStart is SamusPoseId.MorphBallMovingRightPose or
                     SamusPoseId.MorphBallMovingLeftPose or
                     SamusPoseId.SpringBallMovingRightPose or
                     SamusPoseId.SpringBallMovingLeftPose) &&
                 _player.ProspectiveSamusFallbackPose is
                     SamusPoseId.MorphBallGroundRightPose or SamusPoseId.MorphBallGroundLeftPose or
                     SamusPoseId.SpringBallGroundRightPose or SamusPoseId.SpringBallGroundLeftPose)
        {
            // $91:EC85 command six clears base and extra momentum after the
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
            // Applying it after movement preserves the native one-last-step timing.
            SamusGroundedPoseTransitions.ApplyMoonwalkPoseChange(samus, 
                unchecked(moonwalkFallback),
                _options.MoonwalkEnabled);
        }
        else if (SamusPoseMetadata.IsAimedRanIntoWallPose(frame.PoseAtFrameStart) &&
                 _player.ProspectiveSamusFallbackPose is { } wallFallback)
        {
            // `$CF-$D2` store `$89/$8A` in pose-definition byte two. The stable
            // target shares radius, animation list, and type-$15 cleanup physics.
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
            // Only `$91:82D9`'s explicit fallback path above populates this value.
            // Validate and apply it through the same radius/animation seam as
            // held-input aim changes; never assign the ROM byte directly to Pose.
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
            // A $FF definition fallback retains the animation, not the momentum
            // command. Native command two still cancels reverse acceleration,
            // for ground turns ($0E) exactly as for aerial ones ($17/$18).
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
            // Install the fallback's pose metadata and native initializer first.
            // Falling command eight below then clears only the extra component;
            // it must not be folded into base speed like running deceleration.
            // A retained compact pose selects history/momentum work only;
            // native pose initialization and animation restart require a change.
            if (aerialFallback != samus.Pose)
                SamusAerialPoseTransitions.ApplyAerialAimTransition(samus, 
                    unchecked(aerialFallback));
        }
    }
}
