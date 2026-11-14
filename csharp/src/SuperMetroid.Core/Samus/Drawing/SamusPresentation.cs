using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>Samus's presentation: her whole body picture, its placement, her particles, and the animation-frame sequencing that drives them.</summary>
internal sealed class SamusPresentation
{
    /// <summary>
    /// The whole body picture the body last drew, which the speed-booster echoes repeat; none before the body is drawn.
    /// While invincibility flicker hides the body the echoes keep repeating the picture last drawn (#1392).
    /// </summary>
    public SamusBodyFrame? DrawnFrame { get; internal set; }

    /// <summary>The screen point Samus's body picture was last drawn at, from her world position and the foreground scroll.</summary>
    public Position16<ScreenPixels> DrawOrigin { get; internal set; }

    /// <summary>Currently bound body placements and presentation catalogs; never part of save state.</summary>
    public InstalledSamusBodyArtwork? Artwork { get; private set; }

    /// <summary>Rebinds the installed body artwork.</summary>
    public void BindArtwork(InstalledSamusBodyArtwork? value) => Artwork = value;

    /// <summary>The installed pre-coloured sheets Samus draws from; never part of save state.</summary>
    public InstalledSpriteSheets? Sheets { get; private set; }

    /// <summary>Rebinds the installed sheets.</summary>
    public void BindSheets(InstalledSpriteSheets? value) => Sheets = value;

    /// <summary>Her splashes, bubbles, lava spray and dust.</summary>
    public SamusParticles Particles { get; } = new();

    /// <summary>
    /// Seeds the animation frame timer as a pose change does, including its
    /// bottom-minus-one liquid boundary test.
    /// </summary>
    public static void InitializeAnimation(SamusState samus, int initialFrame = 0)
    {

        samus.AnimationFrame = initialFrame;
        // Pose initialization does not simply reuse the previous animation pass's delay buffer. It
        // recomputes water/lava delay from the NEW pose radius at Y + radius - 1, while
        // Gravity Suit takes the ordinary speed-divisor path.
        samus.AnimationFrameBuffer = samus.LiquidPhysics.DeterminePoseChangeAnimationBuffer(samus);
        samus.AnimatedPose = samus.Pose;
        if (samus.AnimationStep is not SamusAnimationDelay initialDelay)
        {
            throw new InvalidOperationException(
                $"Samus pose {samus.Pose} cannot start its animation on frame {samus.AnimationFrame}, which is not a delay.");
        }

        samus.AnimationFrameTimer = WrappingCountdown.Start(samus.AnimationFrameBuffer).AddFrames(initialDelay.Ticks);
        samus.PendingTransitionalPose = null;
    }

    /// <summary>
    /// The pose-change animation reset as a scene setup
    /// calls it before committing pose history: the animation restarts only when Pose
    /// differs from PreviousPose, and otherwise keeps its frame and timer running.
    /// </summary>
    /// <remarks>
    /// The behaviour also returns early while the animation-frame delay adjustment is negative. This model owns it as
    /// <see cref="SamusState.AnimationFrameBuffer"/>, so callers must be paths where it is
    /// nonnegative, as it is at the intro's discovery setup and its demo's end.
    /// </remarks>
    internal static void SetAnimationFrameIfPoseChanged(SamusState samus)
    {
        if (samus.Pose == samus.PoseHistory.PreviousPose)
            return;
        InitializeAnimation(samus);
    }

    /// <summary>
    /// Restarts the pose's animation after a scripted controller writes Pose and
    /// literal animation values without calling ordinary pose initialization.
    /// </summary>
    /// <remarks>
    /// Standing drained Samus up and putting her back into the drained crouch do exactly that.
    /// The game's animation pass resolves the animation from Pose every call, whereas
    /// this model caches it as a consistency check. This narrow method updates only that
    /// cache plus the controller's explicit writes; it intentionally does not recompute the
    /// liquid animation buffer or silently refresh collision radius.
    /// </remarks>
    internal static void SetPoseAndAnimationFromScriptedController(SamusState samus, 
        SamusPoseId pose,
        int frame,
        int timer)
    {
        // Both drained controllers store only the pose; the prior movement and collision radii stay.
        samus.WritePoseWithoutInitialization(pose);
        samus.AnimatedPose = pose;
        samus.AnimationFrame = frame;
        samus.AnimationFrameTimer = WrappingCountdown.Start(timer);
        samus.PendingTransitionalPose = null;
    }

    /// <summary>
    /// Publishes the angle-selected grapple art exactly as the grapple swing does before the normal
    /// animation pass. The latter will decrement this fifteen to fourteen later in
    /// the same gameplay frame; exposing this narrow writer prevents grapple code from
    /// acquiring a general-purpose escape hatch around animation invariants.
    /// </summary>
    public static void SetGrappleSwingAnimationFrame(SamusState samus, int frame)
    {
        if (samus.Pose is not (SamusPoseId.GrappleSwingRightPose or SamusPoseId.GrappleSwingLeftPose))
            throw new InvalidOperationException($"Grapple swing art cannot be assigned to pose {samus.Pose}.");
        samus.AnimationFrame = frame;
        samus.AnimationFrameTimer = WrappingCountdown.Start(15);
    }

    /// <summary>
    /// Animates Samus, including movement-visible FX delay state.
    /// </summary>
    /// <remarks>
    /// The liquid effect step publishes water/lava delay buffering, remembered medium,
    /// atmospheric slots, sound requests, and periodic damage before the timer
    /// decrement.
    /// </remarks>
    public static void AnimateNoFx(SamusState samus, SamusAnimationStep step)
    {
        EnsureAnimationInitialized(samus);

        // The animation pass dispatches the active room-FX animation handler before touching the
        // frame timer. This also updates the remembered medium, which the next Space Jump gate
        // consumes independently of its current top-boundary submersion check.
        samus.LiquidPhysics.PrepareAnimationFrame(
            samus,
            step.Random,
            step.FrameCounter,
            step.BeginLiquidSoundRequestFrame,
            step.BossPresent);

        // The animation keeps neutral-jump frame one alive in four-tick chunks while Samus is
        // still rising. This is intentionally tested before the decrement and applies only when the
        // timer is exactly one; release/apex changes YDirection to two and lets it expire.
        if (samus.Pose is SamusPoseId.NeutralJumpRightPose or SamusPoseId.NeutralJumpLeftPose &&
            samus.Kinematics.YDirection != SamusVerticalDirection.Down &&
            samus.AnimationFrame == 1 &&
            samus.AnimationFrameTimer.IsAt(1))
        {
            samus.AnimationFrameTimer = WrappingCountdown.Start(4);
        }

        // The decrement wraps at 65536. A timer accidentally initialized to zero becomes 65535; the negative test then
        // advances just like an ordinary 1 -> 0 expiration.
        samus.AnimationFrameTimer = samus.AnimationFrameTimer.Decrement().Remaining;
        if (!samus.AnimationFrameTimer.IsZero && !samus.AnimationFrameTimer.IsNegative)
            return;

        samus.AnimationFrame++;
        HandleAnimationDelay(samus, step.ControllerInput, step.ProspectiveInputPose, step.DemoPoseInput, step.QueueEchoSound);
    }

    /// <summary>
    /// Advances the animation-only core of the death animation's start. Unlike ordinary gameplay animation, this call deliberately skips liquid-
    /// FX delay buffering, neutral-jump hacks, and controller-dependent Dash interception.
    /// </summary>
    internal static void AnimateDeathFrame(SamusState samus)
    {
        EnsureAnimationInitialized(samus);
        samus.AnimationFrameTimer = samus.AnimationFrameTimer.Decrement().Remaining;
        if (!samus.AnimationFrameTimer.IsZero && !samus.AnimationFrameTimer.IsNegative)
            return;
        samus.AnimationFrame++;
        HandleAnimationDelay(samus, controllerInput: 0);
    }

    /// <summary>
    /// Publishes an animation frame/timer pair written directly by whatever moves Samus (her
    /// <see cref="SamusMovementOwner"/>: her pose's own movement, Crystal Flash, the drain or the X-ray scope) instead of
    /// by the generic delay animation.
    /// </summary>
    /// <remarks>
    /// Crystal Flash writes the frame and timer directly. This narrow internal
    /// seam preserves the ordering: movement writes the pair, then animation later
    /// in the movement phase immediately decrements the newly written timer.
    /// </remarks>
    internal static void SetAnimationFrameFromMovementOwner(SamusState samus, int frame, int timer)
    {
        samus.AnimationFrame = frame;
        samus.AnimationFrameTimer = WrappingCountdown.Start(timer);
    }

    /// <summary>Runs the step the animation reaches when its frame timer expires: a frame's delay, or a cue.</summary>
    private static void HandleAnimationDelay(SamusState samus, ControllerButton controllerInput,
        SamusPoseId? prospectiveInputPose = null, bool demoPoseInput = false,
        Func<SoundQueueAdmission>? queueEchoSound = null)
    {
        SamusPoseAnimationStep step = samus.AnimationStep;
        if (step is SamusAnimationDelay delay)
        {
            // While running with momentum the run's cadence, not the pose's own delays, times each frame. Releasing
            // Dash keeps the cadence: only a transition or collision that cancels momentum ends it.
            int runningOrPoseDelay = samus.HorizontalSpeed.HasRunningMomentum &&
                SamusPoseMetadata.ReadMovementKind(samus) == SamusMovementType.Running
                ? samus.EquippedItems.HasAny(SamusEquipmentFlags.SpeedBooster)
                    ? samus.HorizontalSpeed.SpeedBoosterDelay(samus.UnauthoredBoostCadence, samus.AnimationFrame)
                    : SamusRunningCadences.Delay(samus.UnauthoredBoostCadence, SamusRunningCadence.Ordinary, samus.AnimationFrame)
                : delay.Ticks;
            samus.AnimationFrameTimer = WrappingCountdown.Start(samus.AnimationFrameBuffer).AddFrames(runningOrPoseDelay);
            return;
        }

        bool activeDashAnimation = samus.HorizontalSpeed.HasRunningMomentum &&
            SamusPoseMetadata.ReadMovementKind(samus) == SamusMovementType.Running &&
            (controllerInput & ControllerButton.B) != ControllerButton.None;
        if (activeDashAnimation)
        {
            // An active Dash takes the cue that closes the run before the pose's animation does. Without the
            // Speed Booster the run restarts at frame zero on the ordinary cadence, and nothing else runs.
            if (samus.EquippedItems.HasAny(SamusEquipmentFlags.SpeedBooster))
            {
                int stagedFrame = samus.AnimationFrame;
                if (samus.HorizontalSpeed.TryAdvanceSpeedBoosterAnimationStage(
                    samus.UnauthoredBoostCadence,
                    movementType: SamusMovementType.Running,
                    controllerInput,
                    samus.AnimationFrameBuffer,
                    ref stagedFrame,
                    out int stagedTimer,
                    // The frame's own echo call answers with its queue; otherwise the echo joins Samus's audio requests.
                    queueEchoSound is { } queueFrameEcho
                        ? () => queueFrameEcho()
                        : () => samus.AudioRequests.QueueSound(RoomSound.SpeedBooster, SoundRequestPriority.Normal)))
                {
                    samus.AnimationFrame = stagedFrame;
                    samus.AnimationFrameTimer = WrappingCountdown.Start(stagedTimer);
                    return;
                }

                // A boost cadence countdown still running leaves the cue to the pose's animation below.
            }
            else
            {
                samus.AnimationFrame = 0;
                samus.AnimationFrameTimer = WrappingCountdown.Start(samus.AnimationFrameBuffer).AddFrames(
                    SamusRunningCadences.Delay(samus.UnauthoredBoostCadence, SamusRunningCadence.Ordinary, frame: 0));
                return;
            }
        }

        switch (step)
        {
            case SamusAnimationHold:
                // The frame and timer stay as they are: the expired timer wraps on the next tick, which then moves on to
                // the frame after this one. The aimed falling poses spend that extra tick before their held frame.
                return;

            case SamusAnimationLoopUnlessLowEnergy:
                // Healthy Samus restarts her breathing; below 30 energy she continues into the drained breathing.
                samus.AnimationFrame = samus.Health < 30 ? samus.AnimationFrame + 1 : 0;
                break;

            case SamusAnimationInstallDrainedFall:
                samus.Drained.StartFalling(samus);
                samus.AnimationFrame++;
                break;

            case SamusAnimationTurnTransition turn:
                // A jump the pose input already selected survives the turn's final frame; checking held Jump instead
                // would accept inputs the pose input never matched.
                if (!samus.Control.InputLocked && !demoPoseInput && prospectiveInputPose is
                    SamusPoseId.NeutralJumpTransitionRightPose or
                    SamusPoseId.NeutralJumpTransitionLeftPose or
                    SamusPoseId.SpinJumpRightPose or SamusPoseId.SpinJumpLeftPose)
                    return;
                if (!samus.Control.InputLocked && !demoPoseInput) samus.InputSnapshot.AutoJumpInputPending = true;
                // The transition neither selects a new delay nor advances the shown frame; the pose transitions that
                // follow the animation consume it.
                samus.PendingTransitionalPose = turn.Pose;
                return;

            case SamusAnimationItemAirborneTransition itemAirborne:
                // Moving vertically means either part of the vertical speed is nonzero. Morph entry uses this to finish
                // without guessing whether the transition walked off a ledge.
                bool movingVertically = samus.Kinematics.YSpeed.Whole != 0 || samus.Kinematics.YSpeed.Fraction != 0;
                samus.PendingTransitionalPose = samus.EquippedItems.HasAny(itemAirborne.Item)
                    ? movingVertically ? itemAirborne.EquippedAirborne : itemAirborne.EquippedGrounded
                    : movingVertically ? itemAirborne.UnequippedAirborne : itemAirborne.UnequippedGrounded;
                return;

            case SamusAnimationWallJumpSelect wallJump:
                // The submersion test is on the body's top edge, unlike jump launch's bottom-edge test.
                samus.AnimationFrame =
                    !samus.EquippedItems.HasAny(SamusEquipmentFlags.GravitySuit) &&
                    samus.LiquidPhysics.IsTopBoundarySubmerged(samus) ? wallJump.OrdinaryFrame :
                    samus.EquippedItems.HasAny(SamusEquipmentFlags.ScrewAttack) ? wallJump.ScrewAttackFrame :
                    samus.EquippedItems.HasAny(SamusEquipmentFlags.SpaceJump) ? wallJump.SpaceJumpFrame : wallJump.OrdinaryFrame;
                break;

            case SamusAnimationVerticalSpeedTransition verticalSpeed:
                // Read only on a cue value's place (#1535): the pose depends on either part of the vertical speed alone.
                samus.PendingTransitionalPose = samus.Kinematics.YSpeed.Whole != 0 || samus.Kinematics.YSpeed.Fraction != 0
                    ? verticalSpeed.Airborne
                    : verticalSpeed.Grounded;
                return;

            case SamusAnimationItemTransition item:
                samus.PendingTransitionalPose = samus.EquippedItems.HasAny(item.Item) ? item.Equipped : item.Unequipped;
                return;

            case SamusAnimationTransition transition:
                // Unlike a turn's transition, no selected jump supersedes it.
                samus.PendingTransitionalPose = transition.Pose;
                return;

            case SamusAnimationRepeat repeat:
                samus.AnimationFrame = repeat.Frame;
                break;

            case SamusAnimationRestart:
                samus.AnimationFrame = 0;
                break;

            default:
                throw new InvalidOperationException($"Samus animation step {step} has no handler.");
        }

        if (samus.AnimationStep is not SamusAnimationDelay selected)
        {
            throw new InvalidDataException(
                $"Samus animation step {step} selected frame {samus.AnimationFrame}, which is not a delay.");
        }
        samus.AnimationFrameTimer = WrappingCountdown.Start(samus.AnimationFrameBuffer).AddFrames(selected.Ticks);
    }

    /// <summary>Rejects animation updates after a pose change that did not restart the animation for the new pose's animation.</summary>
    private static void EnsureAnimationInitialized(SamusState samus)
    {
        if (samus.AnimatedPose is not { } animated ||
            !ReferenceEquals(SamusAnimationDelays.AnimationForPose(animated), SamusAnimationDelays.AnimationForPose(samus.Pose)))
        {
            throw new InvalidOperationException(
                "Samus animation was not initialized for the current pose. Call InitializeAnimation after changing Pose.");
        }
    }

    /// <summary>
    /// Draws Samus in her standing and ordinary-running poses.
    /// </summary>
    /// <remarks>
    /// The body is one whole picture (#1513, #1585): her pose at the picture frame <see cref="SamusShownFrames"/> gives the
    /// animation frame, placed at the origin the import baked into that installed frame (#1517). The body is always a whole
    /// Samus of her pose, while her animation frame and carried movement stay as gameplay left them.
    /// </remarks>
    public bool Draw(SamusState samus,
        SpriteFrameBuffer sprites,
        Position16<RoomPixels> cameraPosition,
        int frameCounter = 0,
        TurningFloorTransform? turningFloor = null)
    {
        Ensure.NotNull(sprites);

        // The body sheet holds pictures for exactly the bodies a path of the game shows (#1513).
        int pictureFrame = SamusShownFrames.Of(samus.FramesBeyondAnimation).PictureFrame(samus.Pose, samus.AnimationFrame);

        // The draw applies invincibility flicker to the body frames, not to
        // animation or graphics streaming as a whole. An odd frame hides ordinary invincible
        // Samus. Active knockback and any nonzero stored/active shine timer independently
        // force the body visible, even though the separately drawn arm cannon follows its
        // stricter rule and may still disappear on that same odd frame.
        bool bodyVisible = samus.KnockbackTimer != 0 ||
            samus.InvincibilityTimer.IsZero ||
            SamusInputSnapshot.HoldsShine(samus) ||
            !SamusCadenceDefinitions.InvincibilityFlickerHidden.IsSetIn(frameCounter);
        // A hidden body keeps the picture and origin it last drew, which the echoes repeat.
        if (!bodyVisible)
            return false;

        // While Ceres's floor turns, Samus is drawn at her whole-pixel world point as the floor
        // rotates it. Every drawn position below therefore uses the
        // rotated point, but public physics coordinates remain completely untouched.
        Position16<RoomPixels> position = samus.Position.Whole;
        Position16<RoomPixels> renderPoint = turningFloor is { } transform
            ? transform.Transform(position)
            : position;
        int renderX = renderPoint.X;
        int renderY = renderPoint.Y;

        InstalledSamusBodyArtwork artwork = Artwork ?? throw new InvalidOperationException(
            "Samus pose presentation requires installed body artwork.");
        int originX = CoordinateAxis.Wrap(renderX - cameraPosition.X);
        int originY = CoordinateAxis.Wrap(
            renderY - artwork.OriginRise(samus.Pose, pictureFrame) - cameraPosition.Y);
        DrawOrigin = Position16<ScreenPixels>.Of(originX, originY);

        // The body is one whole picture of her pose at its picture frame, drawn at the origin (#1513, #1585).
        var body = new SamusBodyFrame(samus.Pose, pictureFrame);
        DrawnFrame = body;
        sprites.AddSamusBody(body, DrawOrigin, BodySheets, new SpriteSource(SpriteSourceKind.Samus));
        return true;
    }

    /// <summary>
    /// Draws the two ordinary Speed-Booster echoes from the positions captured by
    /// the speed-echo position update.
    /// </summary>
    /// <remarks>
    /// This preserves both branches of the echo draw. A
    /// capturing cursor draws stationary trailing snapshots only at boost stage four.
    /// Cancellation sends the echoes departing; drawing then advances each body's
    /// ±8 X / ±2 Y convergence before deciding whether it crossed the live Samus.
    /// </remarks>
    public void DrawSpeedBoosterEchoes(SamusState samus, 
        SpriteFrameBuffer sprites,
        Position16<RoomPixels> cameraPosition)
    {
        Ensure.NotNull(sprites);

        if (samus.HorizontalSpeed.SpeedEchoesDeparting)
        {
            // The echo update walks slot one before slot zero. Movement is a draw-handler side
            // effect in the retail game, so do not advance it earlier in StepFrame: frames
            // whose Samus draw handler is suppressed must also freeze these copies.
            if (samus.HorizontalSpeed.AdvanceDepartingSpeedEcho(
                    slot: 1,
                    samus.Position.Whole))
            {
                DrawActiveSpeedBoosterEcho(samus, 
                    sprites,
                    samus.HorizontalSpeed.SecondSpeedEchoPosition,
                    cameraPosition, slot: 1);
            }

            if (samus.HorizontalSpeed.AdvanceDepartingSpeedEcho(
                    slot: 0,
                    samus.Position.Whole))
            {
                DrawActiveSpeedBoosterEcho(samus, 
                    sprites,
                    samus.HorizontalSpeed.FirstSpeedEchoPosition,
                    cameraPosition, slot: 0);
            }

            samus.HorizontalSpeed.FinishDepartingSpeedEchoFrame();
            return;
        }

        if (!samus.HorizontalSpeed.IsActivelySpeedBoosting)
            return;

        // The echo draw draws slot one before slot zero. Drawing order is observable when their
        // opaque pixels overlap, so retain that otherwise-surprising reverse order.
        DrawActiveSpeedBoosterEcho(samus, 
            sprites,
            samus.HorizontalSpeed.SecondSpeedEchoPosition,
            cameraPosition, slot: 1);
        DrawActiveSpeedBoosterEcho(samus, 
            sprites,
            samus.HorizontalSpeed.FirstSpeedEchoPosition,
            cameraPosition, slot: 0);
    }

    /// <summary>
    /// On the crash echoes' updates, draws both crash-orbit copies after
    /// the real Samus body using the current pose/frame pictures.
    /// </summary>
    public void DrawShinesparkCrashEchoes(SamusState samus, 
        SpriteFrameBuffer sprites,
        Position16<RoomPixels> cameraPosition,
        int frameCounter)
    {
        Ensure.NotNull(sprites);
        if (!SamusCadenceDefinitions.CrashEchoesDrawnEverySecondUpdate.IsDue(frameCounter) ||
            samus.Shinespark.Phase is not (ShinesparkPhase.Crash or ShinesparkPhase.CrashEchoCircle))
        {
            return;
        }

        // The second copy is drawn before the first, as for the boost's echoes.
        DrawActiveSpeedBoosterEcho(samus, 
            sprites,
            samus.Shinespark.SecondOrbitingCopy,
            cameraPosition, slot: 1);
        DrawActiveSpeedBoosterEcho(samus, 
            sprites,
            samus.Shinespark.FirstOrbitingCopy,
            cameraPosition, slot: 0);
    }

    /// <summary>
    /// Draws the released shinespark crash echoes where their shots last placed them, while their shots carry them.
    /// These copies are ordinary projectile-phase visuals after the centered crash circle ends.
    /// </summary>
    public void DrawReleasedShinesparkCrashEchoes(SamusState samus, 
        SpriteFrameBuffer sprites,
        Position16<RoomPixels> cameraPosition,
        int frameCounter)
    {
        Ensure.NotNull(sprites);
        if (!SamusCadenceDefinitions.CrashEchoesDrawnEverySecondUpdate.IsDue(frameCounter))
            return;

        // The second echo is drawn before the first. That ordering controls drawing priority when copies overlap.
        DrawActiveSpeedBoosterEcho(samus,
            sprites, samus.Shinespark.SecondReleasedEcho.ShownPosition, cameraPosition, slot: 3);
        DrawActiveSpeedBoosterEcho(samus,
            sprites, samus.Shinespark.FirstReleasedEcho.ShownPosition, cameraPosition, slot: 2);
    }

    /// <summary>The installed sheets the body draws from.</summary>
    private InstalledSpriteSheets BodySheets => Sheets ?? throw new InvalidOperationException(
        "Samus rendering requires the installed sprite sheets.");

    /// <summary>
    /// Draws an echo, when its slot holds one, with the picture the body last drew, published wherever it lies; the
    /// display clips it to the view, as it does the body (#1625).
    /// </summary>
    private void DrawActiveSpeedBoosterEcho(SamusState samus,
        SpriteFrameBuffer sprites,
        Position16<RoomPixels>? stored,
        Position16<RoomPixels> cameraPosition,
        int slot)
    {
        if (stored is not { } echo)
            return;

        int graphicsYOffset = SamusPoseMetadata.ReadGraphicsYOffset(samus);
        int screenX = CoordinateAxis.Wrap(echo.X - cameraPosition.X);
        int screenY = CoordinateAxis.Wrap(echo.Y - graphicsYOffset - cameraPosition.Y);
        var origin = Position16<ScreenPixels>.Of(screenX, screenY);
        // An echo repeats the whole picture the body last drew; before the body has been drawn there is nothing to repeat.
        if (DrawnFrame is { } body)
            sprites.AddSamusBody(body, origin, BodySheets, new SpriteSource(SpriteSourceKind.SamusEcho, slot));
    }
}
