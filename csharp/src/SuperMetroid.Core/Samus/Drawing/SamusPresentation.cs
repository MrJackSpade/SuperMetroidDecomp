using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>Samus's presentation: her whole body picture, its placement, her particles, and the animation-frame sequencing that drives them.</summary>
internal sealed partial class SamusPresentation
{
    /// <summary>
    /// The whole body picture the body last drew, which the speed-booster echoes repeat; none before the body is drawn.
    /// While invincibility flicker hides the body the echoes keep repeating the picture last drawn (#1392).
    /// </summary>
    internal SamusBodyFrame? DrawnFrame { get; set; }

    /// <summary>
    /// The signed screen place Samus's body picture was last drawn at, from her world position and the foreground scroll:
    /// left of or above the screen is negative.
    /// </summary>
    internal Offset16<ScreenPixels> DrawOrigin { get; set; }

    /// <summary>The installed artwork she draws from, bound by whoever built her; never part of save state.</summary>
    private readonly SamusArtworkBinding _artwork;

    /// <summary>Starts her presentation drawing from <paramref name="artwork"/>.</summary>
    internal SamusPresentation(SamusArtworkBinding artwork)
    {
        _artwork = Ensure.NotNull(artwork);
    }

    /// <summary>Whether she is presented in play or as a cinematic's figure; the intro's flashback is the one cinematic.</summary>
    public SamusPresentationMode Mode { get; internal set; }

    /// <summary>Presents her as a cinematic's figure from now on (<see cref="SamusPresentationMode.Cinematic"/>).</summary>
    internal void PresentInCinematic() => Mode = SamusPresentationMode.Cinematic;

    /// <summary>The installed body placements and presentation catalogs.</summary>
    public InstalledSamusBodyArtwork Artwork => _artwork.Body;

    /// <summary>The installed pre-coloured sheets Samus draws from.</summary>
    public InstalledSpriteSheets Sheets => _artwork.Sheets;

    /// <summary>Her splashes, bubbles, lava spray and dust.</summary>
    public SamusParticles Particles { get; } = new();

    /// <summary>
    /// Starts the pose's animation on its first frame and seeds the animation frame timer as a pose change does, including
    /// its bottom-minus-one liquid boundary test.
    /// </summary>
    public static void InitializeAnimation(SamusState samus) => InitializeAnimation(samus, SamusAnimationDelays.AnimationForPose(samus.Pose).Start);

    /// <summary>
    /// Starts the pose's animation on <paramref name="initialFrame"/>, a place named for another animation landing on the
    /// frame the import decoded for it in the pose's (<see cref="SamusAnimationDelays.Land"/>), and seeds the timer as
    /// <see cref="InitializeAnimation(SamusState)"/> does.
    /// </summary>
    internal static void InitializeAnimation(SamusState samus, SamusAnimationPlace initialFrame) =>
        InitializeAnimation(samus, SamusAnimationDelays.Land(samus.Pose, initialFrame, samus.FramesBeyondAnimation));

    /// <summary>
    /// Starts the pose's animation on <paramref name="initialFrame"/>, a frame of the pose's own animation, and seeds the
    /// timer as <see cref="InitializeAnimation(SamusState)"/> does.
    /// </summary>
    internal static void InitializeAnimation(SamusState samus, SamusAnimationFrame initialFrame)
    {
        samus.AnimationFrame = initialFrame;
        // Pose initialization does not simply reuse the previous animation pass's extra animation delay. It
        // recomputes the water or lava delay from the NEW pose radius at Y + radius - 1, while
        // the Gravity Suit takes the slowdown level.
        samus.ExtraAnimationDelay = samus.LiquidPhysics.DeterminePoseChangeExtraAnimationDelay(samus);
        samus.AnimatedPose = samus.Pose;
        if (samus.AnimationStep is not SamusAnimationDelay initialDelay)
        {
            throw new InvalidOperationException(
                $"Samus pose {samus.Pose} cannot start its animation on frame {samus.AnimationFrame}, which is not a delay.");
        }

        samus.AnimationFrameTimer = WrappingCountdown.Start(samus.ExtraAnimationDelay).AddFrames(initialDelay.Ticks);
        samus.CuePose.Forget();
    }

    /// <summary>
    /// The pose-change animation reset as a scene setup
    /// calls it before committing pose history: the animation restarts only when Pose
    /// differs from PreviousPose, and otherwise keeps its frame and timer running.
    /// </summary>
    /// <remarks>
    /// The animation does not restart while <see cref="SamusState.ExtraAnimationDelay"/> is negative; it is nonnegative
    /// at the intro's discovery setup and its demo's end, the only places this runs.
    /// </remarks>
    internal static void SetAnimationFrameIfPoseChanged(SamusState samus)
    {
        if (samus.Pose == samus.PoseHistory.PreviousPose)
            return;
        InitializeAnimation(samus);
    }

    /// <summary>
    /// Restarts the pose's animation after a scripted sequence sets Samus's pose and animation directly.
    /// </summary>
    /// <remarks>
    /// Standing drained Samus up and putting her back into the drained crouch do exactly that; her liquid's
    /// extra animation delay and her collision radius stay as they were.
    /// </remarks>
    internal static void SetPoseAndAnimationForScriptedSequence(SamusState samus,
        SamusPoseId pose,
        SamusAnimationPlace frame,
        int timer) =>
        SetPoseAndAnimationForScriptedSequence(samus, pose, SamusAnimationDelays.Land(pose, frame, samus.FramesBeyondAnimation), timer);

    /// <summary>
    /// Writes Pose and a frame of its own animation as
    /// <see cref="SetPoseAndAnimationForScriptedSequence(SamusState, SamusPoseId, SamusAnimationPlace, int)"/> does.
    /// </summary>
    internal static void SetPoseAndAnimationForScriptedSequence(SamusState samus,
        SamusPoseId pose,
        SamusAnimationFrame frame,
        int timer)
    {
        // Both drained sequences store only the pose; the prior movement and collision radii stay.
        samus.WritePoseWithoutInitialization(pose);
        samus.AnimatedPose = pose;
        samus.AnimationFrame = SamusAnimationDelays.AnimationForPose(pose).Require(frame);
        samus.AnimationFrameTimer = WrappingCountdown.Start(timer);
        samus.CuePose.Forget();
    }

    /// <summary>
    /// Shows the grapple art the swing's angle selects, as the grapple swing does before her animation runs; her
    /// animation counts its hold (<see cref="SamusAnimationDelayDefinitions.GrappleSwingPictureHold"/>) down later in the
    /// same update.
    /// </summary>
    public static void ShowGrappleSwingPicture(SamusState samus, GrappleSwingPicture picture)
    {
        if (!SamusPoseTraits.IsIn(samus.Pose, SamusPoseFamily.GrappleSwing))
            throw new InvalidOperationException($"Grapple swing art cannot be assigned to pose {samus.Pose}.");
        SamusPoseAnimation swing = SamusAnimationDelayDefinitions.GrappleSwingAnimation;
        if (!ReferenceEquals(SamusAnimationDelays.AnimationForPose(samus.AnimationPose), swing))
            throw new InvalidOperationException($"Grapple swing art cannot be assigned while Samus animates by pose {samus.AnimationPose}'s animation.");
        samus.AnimationFrame = swing.Require(picture.AnimationFrame);
        samus.AnimationFrameTimer = WrappingCountdown.Start(SamusAnimationDelayDefinitions.GrappleSwingPictureHold);
    }

    /// <summary>
    /// Runs one update of Samus's animation: her liquid step first, then her animation frame's countdown.
    /// </summary>
    /// <remarks>
    /// The liquid step publishes the water and lava animation delay, the remembered medium, the splashes and bubbles,
    /// sound requests and periodic damage before the frame's countdown runs.
    /// </remarks>
    public static void Animate(SamusState samus, SamusAnimationStep step)
    {
        EnsureAnimationInitialized(samus);

        // The animation pass runs the active room effect's liquid step before touching the
        // frame timer. This also updates the remembered medium, which the next Space Jump gate
        // consumes independently of its current top-boundary submersion check.
        samus.LiquidPhysics.PrepareAnimationFrame(
            samus,
            step.Random,
            step.FrameCounter,
            step.BossPresent);

        // The animation keeps the neutral jump's second frame alive in chunks of its rise extension while Samus is
        // still rising. This is intentionally tested before the decrement and applies only when the
        // timer's last update remains; release or the apex turns her downward and lets it expire.
        if (SamusPoseTraits.IsIn(samus.Pose, SamusPoseFamily.NeutralJump) &&
            !samus.Kinematics.VerticalVelocity.IsFalling &&
            samus.AnimationIsOn(SamusAnimationFrames.NeutralJumpRise) &&
            samus.AnimationFrameTimer.IsAt(1))
        {
            samus.AnimationFrameTimer = WrappingCountdown.Start(SamusAnimationDelayDefinitions.NeutralJumpRiseExtension);
        }

        // A held frame (SamusAnimationHold) leaves its timer expired, and the next update's count expires it again,
        // so the held frame lasts exactly one more update before the following frame.
        samus.AnimationFrameTimer = samus.AnimationFrameTimer.Decrement().Remaining;
        if (!samus.AnimationFrameTimer.IsZero && !samus.AnimationFrameTimer.IsNegative)
            return;

        samus.AdvanceAnimationFrame();
        HandleAnimationDelay(samus, step.ControllerInput, step.ProspectiveInputPose, step.DemoPoseInput);
    }

    /// <summary>
    /// Advances the animation-only core of the death animation's start: unlike ordinary gameplay animation, it skips
    /// the liquid's extra animation delay, the neutral jump's held rise, and controller-dependent Dash interception.
    /// </summary>
    internal static void AnimateDeathFrame(SamusState samus)
    {
        EnsureAnimationInitialized(samus);
        samus.AnimationFrameTimer = samus.AnimationFrameTimer.Decrement().Remaining;
        if (!samus.AnimationFrameTimer.IsZero && !samus.AnimationFrameTimer.IsNegative)
            return;
        samus.AdvanceAnimationFrame();
        HandleAnimationDelay(samus, controllerInput: GameplayAction.None);
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
    internal static void SetAnimationFrameFromMovementOwner(SamusState samus, SamusAnimationPlace frame, int timer)
    {
        samus.SetAnimationFrame(frame);
        samus.AnimationFrameTimer = WrappingCountdown.Start(timer);
    }

    /// <summary>
    /// Publishes a frame of the animation Samus animates by, named by its segment, with its timer, as
    /// <see cref="SetAnimationFrameFromMovementOwner(SamusState, SamusAnimationPlace, int)"/> does.
    /// </summary>
    internal static void SetAnimationFrameFromMovementOwner(SamusState samus, SamusAnimationFrame frame, int timer)
    {
        samus.AnimationFrame = SamusAnimationDelays.AnimationForPose(samus.AnimationPose).Require(frame);
        samus.AnimationFrameTimer = WrappingCountdown.Start(timer);
    }

    /// <summary>Runs the step the animation reaches when its frame timer expires: a frame's delay, or a cue.</summary>
    private static void HandleAnimationDelay(SamusState samus, GameplayAction controllerInput,
        SamusPoseId? prospectiveInputPose = null, bool demoPoseInput = false)
    {
        SamusPoseAnimationStep step = samus.AnimationStep;
        if (step is SamusAnimationDelay delay)
        {
            // While running with momentum the run's cadence, not the pose's own delays, times each frame. Releasing
            // Dash keeps the cadence: only a transition or collision that cancels momentum ends it.
            int runningOrPoseDelay = samus.HorizontalSpeed.HasRunningMomentum &&
                SamusPoseMetadata.ReadMovementType(samus) == SamusMovementType.Running
                ? samus.EquippedItems.HasAny(SamusEquipmentFlags.SpeedBooster)
                    ? samus.HorizontalSpeed.SpeedBoosterDelay(samus.UnauthoredBoostCadence, samus.RunStrideFrame.Frame)
                    : SamusRunningCadences.Delay(samus.UnauthoredBoostCadence, SamusRunningCadence.Ordinary, samus.RunStrideFrame.Frame)
                : delay.Ticks;
            samus.AnimationFrameTimer = WrappingCountdown.Start(samus.ExtraAnimationDelay).AddFrames(runningOrPoseDelay);
            return;
        }

        bool activeDashAnimation = samus.HorizontalSpeed.HasRunningMomentum &&
            SamusPoseMetadata.ReadMovementType(samus) == SamusMovementType.Running &&
            (controllerInput & GameplayAction.Dash) != GameplayAction.None;
        if (activeDashAnimation)
        {
            // An active Dash takes the cue that closes the run before the pose's animation does. Without the
            // Speed Booster the run restarts at frame zero on the ordinary cadence, and nothing else runs.
            if (samus.EquippedItems.HasAny(SamusEquipmentFlags.SpeedBooster))
            {
                // Taking the cue restarts the run on its first frame; otherwise the frame stays where it is.
                if (samus.HorizontalSpeed.TryAdvanceSpeedBoosterAnimationStage(
                    samus.UnauthoredBoostCadence,
                    movementType: SamusMovementType.Running,
                    controllerInput,
                    samus.ExtraAnimationDelay,
                    out int stagedTimer,
                    // The echo joins Samus's audio requests, whose queue's answer selects the boost's cadence.
                    () => samus.AudioRequests.QueueSound(RoomSound.SpeedBooster, SoundRequestPriority.Normal)))
                {
                    samus.RestartAnimation();
                    samus.AnimationFrameTimer = WrappingCountdown.Start(stagedTimer);
                    return;
                }

                // A boost cadence countdown still running leaves the cue to the pose's animation below.
            }
            else
            {
                samus.RestartAnimation();
                samus.AnimationFrameTimer = WrappingCountdown.Start(samus.ExtraAnimationDelay).AddFrames(
                    SamusRunningCadences.Delay(samus.UnauthoredBoostCadence, SamusRunningCadence.Ordinary, frame: 0));
                return;
            }
        }

        switch (step)
        {
            case SamusAnimationHold:
                // The frame and timer stay as they are, so the frame lasts one more tick and the next tick moves on to
                // the frame after this one. The aimed falling poses spend that extra tick before their held frame.
                return;

            case SamusAnimationLoopUnlessLowEnergy:
                // Healthy Samus restarts her breathing; on low energy she continues into the drained breathing.
                if (samus.Health < SamusAnimationDelayDefinitions.LowEnergyBreathingBelow)
                    samus.AdvanceAnimationFrame();
                else
                    samus.RestartAnimation();
                break;

            case SamusAnimationInstallDrainedFall:
                samus.Drained.StartFalling(samus);
                samus.AdvanceAnimationFrame();
                break;

            case SamusAnimationTurnTransition turn:
                // A jump the pose input already selected survives the turn's final frame; checking held Jump instead
                // would accept inputs the pose input never matched.
                if (!samus.Control.InputLocked && !demoPoseInput && prospectiveInputPose is { } selectedJump &&
                    SamusPoseTraits.IsIn(selectedJump, SamusPoseFamily.JumpLaunch))
                    return;
                if (!samus.Control.InputLocked && !demoPoseInput) samus.InputSnapshot.AutoJump.Install();
                // The transition neither selects a new delay nor advances the shown frame; the pose transitions that
                // follow the animation consume it.
                samus.CuePose.Name(turn.Pose);
                return;

            case SamusAnimationItemAirborneTransition itemAirborne:
                // Moving vertically means any vertical speed at all. Morph entry uses this to finish
                // without guessing whether the transition walked off a ledge.
                bool movingVertically = samus.Kinematics.VerticalVelocity.IsMoving;
                samus.CuePose.Name(samus.EquippedItems.HasAny(itemAirborne.Item)
                    ? movingVertically ? itemAirborne.EquippedAirborne : itemAirborne.EquippedGrounded
                    : movingVertically ? itemAirborne.UnequippedAirborne : itemAirborne.UnequippedGrounded);
                return;

            case SamusAnimationWallJumpSelect wallJump:
                // The submersion test is on the body's top edge, unlike jump launch's bottom-edge test.
                samus.MoveAnimationTo(
                    !samus.EquippedItems.HasAny(SamusEquipmentFlags.GravitySuit) &&
                    samus.LiquidPhysics.IsTopBoundarySubmerged(samus) ? wallJump.OrdinaryFrame :
                    samus.EquippedItems.HasAny(SamusEquipmentFlags.ScrewAttack) ? wallJump.ScrewAttackFrame :
                    samus.EquippedItems.HasAny(SamusEquipmentFlags.SpaceJump) ? wallJump.SpaceJumpFrame : wallJump.OrdinaryFrame);
                break;

            case SamusAnimationVerticalSpeedTransition verticalSpeed:
                // Read only when the cue's place is reached (#1535): the pose it names depends only on whether Samus has
                // any vertical speed.
                samus.CuePose.Name(samus.Kinematics.VerticalVelocity.IsMoving
                    ? verticalSpeed.Airborne
                    : verticalSpeed.Grounded);
                return;

            case SamusAnimationItemTransition item:
                samus.CuePose.Name(samus.EquippedItems.HasAny(item.Item) ? item.Equipped : item.Unequipped);
                return;

            case SamusAnimationTransition transition:
                // Unlike a turn's transition, no selected jump supersedes it.
                samus.CuePose.Name(transition.Pose);
                return;

            case SamusAnimationRepeat repeat:
                samus.MoveAnimationTo(repeat.Target);
                break;

            case SamusAnimationRestart:
                samus.RestartAnimation();
                break;

            default:
                throw new InvalidOperationException($"Samus animation step {step} has no playback.");
        }

        if (samus.AnimationStep is not SamusAnimationDelay selected)
        {
            throw new InvalidDataException(
                $"Samus animation step {step} selected frame {samus.AnimationFrame}, which is not a delay.");
        }
        samus.AnimationFrameTimer = WrappingCountdown.Start(samus.ExtraAnimationDelay).AddFrames(selected.Ticks);
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
    /// Draws Samus's body in her current pose, unless invincibility's flicker hides it on this frame.
    /// </summary>
    /// <param name="samus">The Samus to draw.</param>
    /// <param name="sprites">The frame's sprites.</param>
    /// <param name="cameraPosition">The camera the body is placed against.</param>
    /// <param name="frameCounter">The update counter invincibility's flicker reads.</param>
    /// <param name="turningFloor">The turning floor that rotates her drawn point, or null on a still floor.</param>
    /// <remarks>
    /// The body is one whole picture (#1513, #1585): her pose at the picture frame the installed <see cref="SamusShownFrames"/> give the
    /// animation frame, placed at the origin the import baked into that installed frame (#1517). The body is always a whole
    /// Samus of her pose, while her animation frame and carried movement stay as gameplay left them.
    /// </remarks>
    public void Draw(SamusState samus,
        SpriteFrameBuffer sprites,
        Position16<RoomPixels> cameraPosition,
        int frameCounter,
        TurningFloorTransform? turningFloor = null)
    {
        Ensure.NotNull(sprites);

        // The body sheet holds pictures for exactly the bodies a path of the game shows (#1513).
        SamusAnimationFrame pictureFrame = _artwork.ShownFrames.PictureFrame(samus);

        // Invincibility flicker hides only the body's picture; her animation runs on. An odd frame hides ordinary invincible
        // Samus. Active knockback and any nonzero stored/active shine timer independently
        // force the body visible, even though the separately drawn arm cannon follows its
        // stricter rule and may still disappear on that same odd frame.
        bool bodyVisible = samus.KnockbackHold.IsRunning ||
            samus.InvincibilityTimer.IsZero ||
            SamusInputSnapshot.HoldsShine(samus) ||
            !SamusCadenceDefinitions.InvincibilityFlickerHidden.IsInSecondHalfAt(frameCounter);
        // A hidden body keeps the picture and origin it last drew, which the echoes repeat.
        if (!bodyVisible)
            return;

        DrawBody(samus, sprites, cameraPosition, pictureFrame, turningFloor);
    }

    /// <summary>
    /// Draws Samus's body in her current pose on every frame, whatever her invincibility: the door transition, the
    /// elevator ride and the death sequence show her without the flicker.
    /// </summary>
    /// <param name="samus">The Samus to draw.</param>
    /// <param name="sprites">The frame's sprites.</param>
    /// <param name="cameraPosition">The camera the body is placed against.</param>
    /// <param name="turningFloor">The turning floor that rotates her drawn point, or null on a still floor.</param>
    public void DrawWithoutFlicker(SamusState samus,
        SpriteFrameBuffer sprites,
        Position16<RoomPixels> cameraPosition,
        TurningFloorTransform? turningFloor = null)
    {
        Ensure.NotNull(sprites);
        DrawBody(samus, sprites, cameraPosition, _artwork.ShownFrames.PictureFrame(samus), turningFloor);
    }

    /// <summary>Places the body's whole picture of her pose at the picture frame, at the origin that frame gives.</summary>
    private void DrawBody(SamusState samus,
        SpriteFrameBuffer sprites,
        Position16<RoomPixels> cameraPosition,
        SamusAnimationFrame pictureFrame,
        TurningFloorTransform? turningFloor)
    {
        // While Ceres's floor turns, Samus is drawn at her whole-pixel world point as the floor
        // rotates it. Every drawn position below therefore uses the
        // rotated point, but public physics coordinates remain completely untouched.
        Position16<RoomPixels> position = samus.Position.Whole;
        Position16<RoomPixels> renderPoint = turningFloor is { } transform
            ? transform.Transform(position)
            : position;

        InstalledSamusBodyArtwork artwork = _artwork.Body;
        DrawOrigin = (renderPoint - Offset16<RoomPixels>.Of(0, artwork.OriginRise(samus.Pose, pictureFrame)))
            .RelativeToCamera(cameraPosition);

        // The body is one whole picture of her pose at its picture frame, drawn at the origin (#1513, #1585).
        var body = new SamusBodyFrame(samus.Pose, pictureFrame);
        DrawnFrame = body;
        sprites.AddSamusBody(body, DrawOrigin, BodySheets, new SpriteSource(SpriteSourceKind.Samus));
    }

    /// <summary>
    /// Draws the two ordinary Speed-Booster echoes where they stand: the positions the speed-echo position update captured,
    /// or, once cancellation sends them departing, where their departure has carried them
    /// (<see cref="SamusHorizontalSpeedState.AdvanceDepartingSpeedEchoes"/>). Trailing echoes show only at boost stage
    /// four. The draw changes nothing (#2573).
    /// </summary>
    public void DrawSpeedBoosterEchoes(SamusState samus,
        SpriteFrameBuffer sprites,
        Position16<RoomPixels> cameraPosition)
    {
        Ensure.NotNull(sprites);
        if (!samus.HorizontalSpeed.SpeedEchoesDeparting && !samus.HorizontalSpeed.IsActivelySpeedBoosting)
            return;
        DrawActiveSpeedBoosterEcho(samus,
            sprites,
            samus.HorizontalSpeed.SpeedEchoes[SpeedEcho.First].Position,
            cameraPosition);
        DrawActiveSpeedBoosterEcho(samus,
            sprites,
            samus.HorizontalSpeed.SpeedEchoes[SpeedEcho.Second].Position,
            cameraPosition);
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
        if (!SamusCadenceDefinitions.CrashEchoesDrawn.IsDue(frameCounter) ||
            samus.Shinespark.Phase is not (ShinesparkPhase.Crash or ShinesparkPhase.CrashHold))
        {
            return;
        }

        DrawActiveSpeedBoosterEcho(samus,
            sprites,
            samus.Shinespark.FirstOrbitingCopy,
            cameraPosition);
        DrawActiveSpeedBoosterEcho(samus,
            sprites,
            samus.Shinespark.SecondOrbitingCopy,
            cameraPosition);
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
        if (!SamusCadenceDefinitions.CrashEchoesDrawn.IsDue(frameCounter))
            return;

        DrawActiveSpeedBoosterEcho(samus,
            sprites, samus.Shinespark.FirstReleasedEcho.ShownPosition, cameraPosition);
        DrawActiveSpeedBoosterEcho(samus,
            sprites, samus.Shinespark.SecondReleasedEcho.ShownPosition, cameraPosition);
    }

    /// <summary>The installed sheets the body draws from.</summary>
    private InstalledSpriteSheets BodySheets => _artwork.Sheets;

    /// <summary>
    /// Draws an echo, when it stands somewhere, with the picture the body last drew, published wherever it lies; the
    /// display clips it to the view, as it does the body (#1625).
    /// </summary>
    private void DrawActiveSpeedBoosterEcho(SamusState samus,
        SpriteFrameBuffer sprites,
        Position16<RoomPixels>? stored,
        Position16<RoomPixels> cameraPosition)
    {
        if (stored is not { } echo)
            return;

        int artOrigin = SamusPoseMetadata.ReadArtOrigin(samus);
        Offset16<ScreenPixels> origin = (echo - Offset16<RoomPixels>.Of(0, artOrigin)).RelativeToCamera(cameraPosition);
        // An echo repeats the whole picture the body last drew; before the body has been drawn there is nothing to repeat.
        if (DrawnFrame is { } body)
            sprites.AddSamusBody(body, origin, BodySheets, new SpriteSource(SpriteSourceKind.SamusEcho));
    }
}
