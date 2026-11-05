using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>Samus's presentation: spritemap selection, tile transfers, suit, cycle and charge palettes, and the animation-frame sequencing that drives them.</summary>
internal sealed class SamusPresentation
{
    /// <summary>
    /// The full body the body last drew, which the speed-booster echoes repeat; none before the body is drawn. A frame
    /// whose top half is the complete body has no bottom half, so an echo never repeats a bottom half left from an
    /// earlier frame.
    /// </summary>
    public SamusBodyFrame? DrawnFrame { get; internal set; }

    /// <summary>
    /// The half pictures the body showed when its pictures were last chosen, which the speed-booster echoes show
    /// with those pictures. The game's echoes read whatever characters are loaded, so while invincibility
    /// flicker hides the body they can show an earlier spritemap over later characters; a pre-coloured frame keeps the
    /// characters it was shown with (#1392).
    /// </summary>
    public SamusBodyPictures ShownPictures { get; internal set; }

    /// <summary>Last screen-space origin calculated from world position and layer-1 scroll.</summary>
    public Position16<ScreenPixels> SpritemapPosition { get; internal set; }

    /// <summary>Which body definitions the body shows, and the installed artwork they are drawn from.</summary>
    public SamusBodyFrameState BodyFrames { get; } = new();

    /// <summary>
    /// Seeds pose/frame-zero graphics before the first displayed frame, as room/game setup must do
    /// before a visible normal-gameplay Samus can be constructed.
    /// </summary>
    public void PrimeGraphics(SamusState samus) => SelectPictures(samus);

    /// <summary>
    /// Selects the pictures of the picture frame Samus's pose and animation frame draw (#1513): a frame outside the pose's
    /// own pictures shows the pose's nearest picture before it.
    /// </summary>
    /// <exception cref="InvalidOperationException">No path of the game shows the pose at that frame.</exception>
    private void SelectPictures(SamusState samus) =>
        BodyFrames.Select(samus.Pose, SamusShownFrames.Of(samus.AnimationContinuation).PictureFrame(samus.Pose, samus.AnimationFrame));

    /// <summary>
    /// Seeds the animation frame timer as <c>Set_Samus_AnimationFrame_if_PoseChanged</c>
    /// does, including its bottom-minus-one liquid boundary test.
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

        samus.AnimationFrameTimer = WrappingCountdown.FromRaw(samus.AnimationFrameBuffer).AddFrames(initialDelay.Ticks);
        samus.PendingTransitionalPose = null;
    }

    /// <summary>
    /// <c>Set_Samus_AnimationFrame_if_PoseChanged</c> as a scene setup
    /// calls it before committing pose history: the animation restarts only when Pose
    /// differs from PreviousPose, and otherwise keeps its frame and timer running.
    /// </summary>
    /// <remarks>
    /// The routine also returns early while the animation-frame buffer is negative. This port owns it as
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
    /// Restarts the animation's program after a scripted controller writes Pose and
    /// literal animation words without calling ordinary pose initialization.
    /// </summary>
    /// <remarks>
    /// Drained-controller functions one and four do exactly that.
    /// The game's animation pass resolves the pointer from Pose every call, whereas
    /// this C# port caches it as a consistency check. This narrow method updates only that
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
        samus.AnimationFrameTimer = WrappingCountdown.FromRaw(timer);
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
    /// Ports <c>AnimateSamus</c>, including movement-visible FX delay state.
    /// </summary>
    /// <remarks>
    /// The FX dispatcher publishes water/lava delay buffering, remembered medium,
    /// atmospheric slots, sound requests, and periodic-damage words before the timer
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
            Ensure.BetweenInclusive(0, ushort.MaxValue, step.FrameCounter),
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

        // The decrement is 16-bit. A timer accidentally initialized to zero becomes 65535; the negative test then
        // advances just like an ordinary 1 -> 0 expiration.
        samus.AnimationFrameTimer = samus.AnimationFrameTimer.Decrement().Remaining;
        if (!samus.AnimationFrameTimer.IsZero && !samus.AnimationFrameTimer.IsNegative)
            return;

        samus.AnimationFrame++;
        HandleAnimationDelay(samus, step.ControllerInput, step.ProspectiveInputPose, step.DemoPoseInput, step.QueueEchoSound);
    }

    /// <summary>
    /// Ports the animation-only core of <c>Draw_Samus_Starting_Death_Animation</c>,
    /// the death animation. Unlike ordinary gameplay animation, this call deliberately skips liquid-
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
    /// Publishes an animation frame/timer pair written directly by an installed special
    /// movement handler instead of by the generic delay-program interpreter.
    /// </summary>
    /// <remarks>
    /// Crystal Flash writes the frame and timer directly. This narrow internal
    /// seam preserves the ordering: movement writes the pair, then animation later
    /// in beta immediately decrements the newly written timer.
    /// </remarks>
    internal static void SetAnimationFrameFromSpecialHandler(SamusState samus, int frame, int timer)
    {
        samus.AnimationFrame = frame;
        samus.AnimationFrameTimer = WrappingCountdown.FromRaw(timer);
    }

    /// <summary>Runs the step the animation reaches when its frame timer expires: a frame's delay, or an instruction.</summary>
    private static void HandleAnimationDelay(SamusState samus, ControllerButton controllerInput,
        SamusPoseId? prospectiveInputPose = null, bool demoPoseInput = false,
        Func<SoundQueueAdmission>? queueEchoSound = null)
    {
        SamusAnimationProgramStep step = samus.AnimationStep;
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
            samus.AnimationFrameTimer = WrappingCountdown.FromRaw(samus.AnimationFrameBuffer).AddFrames(runningOrPoseDelay);
            return;
        }

        bool activeDashAnimation = samus.HorizontalSpeed.HasRunningMomentum &&
            SamusPoseMetadata.ReadMovementKind(samus) == SamusMovementType.Running &&
            (controllerInput & ControllerButton.B) != ControllerButton.None;
        if (activeDashAnimation)
        {
            // An active Dash takes the instruction that closes the run before the pose's program does. Without the
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
                    queueEchoSound))
                {
                    samus.AnimationFrame = stagedFrame;
                    samus.AnimationFrameTimer = WrappingCountdown.FromRaw(stagedTimer);
                    return;
                }

                // A boost cadence countdown still running leaves the instruction to the pose's program below.
            }
            else
            {
                samus.AnimationFrame = 0;
                samus.AnimationFrameTimer = WrappingCountdown.FromRaw(samus.AnimationFrameBuffer).AddFrames(
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
                samus.Drained.InstallFallingMovementHandler(samus);
                samus.AnimationFrame++;
                break;

            case SamusAnimationTurnTransition turn:
                // A jump the pose input already selected survives the turn's final frame; checking held Jump instead
                // would accept inputs the pose input never matched.
                if (!samus.Control.AlphaLocked && !demoPoseInput && prospectiveInputPose is
                    SamusPoseId.NeutralJumpTransitionRightPose or
                    SamusPoseId.NeutralJumpTransitionLeftPose or
                    SamusPoseId.SpinJumpRightPose or SamusPoseId.SpinJumpLeftPose)
                    return;
                if (!samus.Control.AlphaLocked && !demoPoseInput) samus.InputSnapshot.AutoJumpInputPending = true;
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
                $"Samus animation instruction {step} selected frame {samus.AnimationFrame}, which is not a delay.");
        }
        samus.AnimationFrameTimer = WrappingCountdown.FromRaw(samus.AnimationFrameBuffer).AddFrames(selected.Ticks);
    }

    /// <summary>Rejects animation updates after a pose change that did not restart the animation for the new pose's program.</summary>
    private static void EnsureAnimationInitialized(SamusState samus)
    {
        if (samus.AnimatedPose is not { } animated ||
            !ReferenceEquals(SamusAnimationDelays.ProgramForPose(animated), SamusAnimationDelays.ProgramForPose(samus.Pose)))
        {
            throw new InvalidOperationException(
                "Samus animation was not initialized for the current pose. Call InitializeAnimation after changing Pose.");
        }
    }

    /// <summary>
    /// Ports the standing and ordinary-running portions of <c>Samus_Draw</c>.
    /// </summary>
    /// <remarks>
    /// The body's origin and the halves it shows come from <see cref="SamusBodyDrawing"/>, which keeps the complete
    /// position selectors and lower-half dispatcher, including all unused-but-valid movement types; the body sheet is
    /// composed from the same decisions. The halves an update shows are drawn as one full-body picture (#1513), by the
    /// pose's own movement type at the picture frame <see cref="SamusShownFrames"/> gives the animation frame: the body is
    /// always a whole Samus of her pose, while her animation frame and carried movement stay as gameplay left them.
    /// </remarks>
    public bool Draw(SamusState samus,
        SpriteFrameBuffer sprites,
        Position16<RoomPixels> layer1,
        int frameCounter = 0,
        TurningFloorTransform? turningFloor = null)
    {
        Ensure.NotNull(sprites);

        // The body sheet holds pictures for exactly the bodies a path of the game shows (#1513).
        int pictureFrame = SamusShownFrames.Of(samus.AnimationContinuation).PictureFrame(samus.Pose, samus.AnimationFrame);

        // The draw applies invincibility flicker to the body spritemaps, not to
        // animation or graphics streaming as a whole. An odd frame hides ordinary invincible
        // Samus. Active knockback and any nonzero stored/active shine timer independently
        // force the body visible, even though the separately drawn arm cannon follows its
        // stricter rule and may still disappear on that same odd frame.
        bool bodyVisible = !samus.KnockbackTimer.IsZero ||
            samus.InvincibilityTimer.IsZero ||
            SamusInputSnapshot.SharedShineTimer(samus) != 0 ||
            (frameCounter & 1) == 0;
        if (!bodyVisible)
        {
            // The game branches around every position/index/sprite write, then still falls
            // through to the graphics selection. Preserve the previous visible spritemap indices for
            // echo drawing while selecting this frame's tile definitions for the next displayed frame.
            BodyFrames.Select(samus.Pose, pictureFrame);
            return false;
        }

        // Ceres's rotation temporarily overwrites Samus's integer world point only while Ceres's
        // high status bit is set. Every position selector below therefore consumes the
        // rotated point, but public physics coordinates remain completely untouched.
        Position16<RoomPixels> position = samus.Position.Whole;
        Position16<RoomPixels> renderPoint = turningFloor is { } transform
            ? transform.Transform(position)
            : position;
        int renderX = renderPoint.X;
        int renderY = renderPoint.Y;

        InstalledSamusBodyArtwork artwork = BodyFrames.Artwork ?? throw new InvalidOperationException(
            "Samus pose presentation requires installed body artwork.");
        int spritemapX = CoordinateAxis.Wrap(renderX - layer1.X);
        int spritemapY = CoordinateAxis.Wrap(
            renderY - SamusBodyDrawing.OriginRise(samus.Pose, pictureFrame, artwork) - layer1.Y);
        SpritemapPosition = Position16<ScreenPixels>.Of(spritemapX, spritemapY);

        // The body's top half, the chest cover the unsuited forward-facing body shows, and the bottom half unless the top
        // half is the complete body, are one full-body picture drawn at the spritemap origin (#1513).
        SamusBodyFrame body = SamusBodyDrawing.Frame(samus.Pose, pictureFrame, artwork);
        DrawnFrame = body;
        // The graphics selection follows these spritemaps, but it only chooses the artwork the next displayed frame shows with
        // them. Selecting first lets the draw bind the characters it is displayed with; nothing between reads it.
        BodyFrames.Select(samus.Pose, pictureFrame);
        ShownPictures = BodyFrames.Shown;
        sprites.AddSamusBody(body, SpritemapPosition, artwork.Spritemaps, BodySheets, ShownPictures, new SpriteSource(SpriteSourceKind.Samus));
        return true;
    }

    /// <summary>
    /// Draws the two ordinary Speed-Booster echoes from the positions captured by
    /// <c>Samus_UpdateSpeedEchoPos</c>.
    /// </summary>
    /// <remarks>
    /// This preserves both branches of <c>Samus_DrawEchoes</c>. A
    /// nonnegative index draws stationary trailing snapshots only at boost stage four.
    /// Cancellation changes the index to 65535; drawing then advances each body's
    /// ±8 X / ±2 Y convergence before deciding whether it crossed the live Samus.
    /// </remarks>
    public void DrawSpeedBoosterEchoes(SamusState samus, 
        SpriteFrameBuffer sprites,
        Position16<RoomPixels> layer1)
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
                    layer1, slot: 1);
            }

            if (samus.HorizontalSpeed.AdvanceDepartingSpeedEcho(
                    slot: 0,
                    samus.Position.Whole))
            {
                DrawActiveSpeedBoosterEcho(samus, 
                    sprites,
                    samus.HorizontalSpeed.FirstSpeedEchoPosition,
                    layer1, slot: 0);
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
            layer1, slot: 1);
        DrawActiveSpeedBoosterEcho(samus, 
            sprites,
            samus.HorizontalSpeed.FirstSpeedEchoPosition,
            layer1, slot: 0);
    }

    /// <summary>
    /// On odd frames of the frame counter, draws both crash-orbit copies after
    /// the real Samus body using the current pose/frame spritemaps.
    /// </summary>
    public void DrawShinesparkCrashEchoes(SamusState samus, 
        SpriteFrameBuffer sprites,
        Position16<RoomPixels> layer1,
        int frameCounter)
    {
        Ensure.NotNull(sprites);
        if ((frameCounter & 1) == 0 ||
            samus.Shinespark.Phase is not (ShinesparkPhase.Crash or ShinesparkPhase.CrashEchoCircle))
        {
            return;
        }

        // Slot one precedes slot zero just like the game's X=2,0 loop.
        DrawActiveSpeedBoosterEcho(samus, 
            sprites,
            samus.HorizontalSpeed.SecondSpeedEchoPosition,
            layer1, slot: 1);
        DrawActiveSpeedBoosterEcho(samus, 
            sprites,
            samus.HorizontalSpeed.FirstSpeedEchoPosition,
            layer1, slot: 0);
    }

    /// <summary>
    /// Ports <c>Samus_DrawShinesparkCrashEchoProjectiles</c>. These
    /// copies are ordinary projectile-phase visuals after the centered crash circle ends.
    /// </summary>
    public void DrawReleasedShinesparkCrashEchoes(SamusState samus, 
        SpriteFrameBuffer sprites,
        Position16<RoomPixels> layer1,
        int frameCounter)
    {
        Ensure.NotNull(sprites);
        if ((frameCounter & 1) == 0)
            return;

        // The shinespark echo draw tests/draws fixed slot four (speed echo index three) before fixed
        // slot three (index two). That ordering controls drawing priority when copies overlap.
        ShinesparkReleasedEcho second = samus.Shinespark.SecondReleasedCrashEcho;
        if (second.Active)
        {
            DrawActiveSpeedBoosterEcho(samus, 
                sprites, second.Position, layer1, slot: 3);
        }

        ShinesparkReleasedEcho first = samus.Shinespark.FirstReleasedCrashEcho;
        if (first.Active)
        {
            DrawActiveSpeedBoosterEcho(samus, 
                sprites, first.Position, layer1, slot: 2);
        }
    }

    /// <summary>The installed sheets the body draws from.</summary>
    private InstalledSpriteSheets BodySheets => BodyFrames.Sheets ?? throw new InvalidOperationException(
        "Samus rendering requires the installed sprite sheets.");

    /// <summary>Draws a populated echo slot with current Samus spritemaps after Y clipping.</summary>
    private void DrawActiveSpeedBoosterEcho(SamusState samus, 
        SpriteFrameBuffer sprites,
        Position16<RoomPixels> echo,
        Position16<RoomPixels> layer1,
        int slot)
    {
        // Zero X is the game's empty-slot sentinel, not merely an off-screen point.
        if (echo.X == 0)
            return;

        int graphicsYOffset = SamusPoseMetadata.ReadGraphicsYOffset(samus);
        int screenY = CoordinateAxis.Difference(echo.Y - graphicsYOffset, layer1.Y);

        // The original accepts screen Y 0..247. Horizontal clipping remains display work,
        // exactly as it is for the current Samus body.
        if (screenY is < 0 or >= 248)
            return;

        var spritemaps = BodyFrames.Artwork?.Spritemaps ?? throw new InvalidOperationException(
            "Samus speed-booster echo requires installed spritemap artwork.");
        int screenX = CoordinateAxis.Wrap(echo.X - layer1.X);
        var origin = Position16<ScreenPixels>.Of(screenX, CoordinateAxis.Wrap(screenY));
        // An echo repeats the full-body picture the body last drew; before the body has been drawn there is nothing to repeat.
        if (DrawnFrame is { } body)
            sprites.AddSamusBody(body, origin, spritemaps, BodySheets, ShownPictures, new SpriteSource(SpriteSourceKind.SamusEcho, slot));
    }
}
