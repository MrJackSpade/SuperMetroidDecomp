using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>Samus's presentation: spritemap selection, tile transfers, suit, cycle and charge palettes, and the animation-frame sequencing that drives them.</summary>
internal sealed class SamusPresentation
{
    /// <summary>The top-half picture the body last drew, which the speed-booster echoes repeat; none before the body is drawn.</summary>
    public SamusBodyHalfFrame? TopSpritemap { get; internal set; }

    /// <summary>
    /// The bottom-half picture the body last drew, which the speed-booster echoes repeat; none when the last drawn frame
    /// was a complete top-half picture, so an echo never repeats a bottom half left from an earlier frame.
    /// </summary>
    public SamusBodyHalfFrame? BottomSpritemap { get; internal set; }

    /// <summary>
    /// The half pictures the body showed when its pictures were last chosen, which the speed-booster echoes show
    /// with those pictures. The cartridge's echoes read whatever characters are in VRAM, so while invincibility
    /// flicker hides the body they can show an earlier spritemap over later characters; a pre-coloured frame keeps the
    /// characters it was shown with (#1392).
    /// </summary>
    public SamusBodyPictures ShownPictures { get; internal set; }

    /// <summary>Last screen-space origin calculated from world position and layer-1 scroll.</summary>
    public Position16<ScreenPixels> SpritemapPosition { get; internal set; }

    /// <summary>Which body definitions the body shows, and the installed artwork they are drawn from.</summary>
    public SamusBodyFrameState BodyFrames { get; } = new();

    /// <summary>Host-bound normal suit colors; excluded from debugger-state graphs.</summary>
    [field: NonSerialized]
    public InstalledSamusSuitColors? SuitColors { get; set; }

    /// <summary>Host-bound animation colors; excluded from debugger-state graphs.</summary>
    [field: NonSerialized]
    public SamusFullBodyCycleColours? FullBodyCycleColors { get; set; }

    /// <summary>Host-bound charge and Hyper-shot body colors; not serialized.</summary>
    [field: NonSerialized]
    public SamusChargeColours? ChargeColors { get; set; }

    /// <summary>
    /// Copies <c>SamusPalettes_PowerSuit</c> at <c>$9B:9400</c> to palette-buffer/CGRAM
    /// entries 192–207, porting <c>Samus_LoadSuitPalette</c>'s no-suit branch.
    /// </summary>
    public static void LoadPowerSuitPalette(ColourRun suit,
        InstalledSamusSuitColors? suitColors = null)
    {
        // OBJ palettes begin at CGRAM 128. Spritemap palette 4 therefore resolves to 192,
        // exactly matching CopyToSamusSuitPalette's &palette_buffer[192] destination.
        SamusNormalSuitPalette.LoadPower(suit, suitColors);
    }

    /// <summary>
    /// Ports <c>Samus_LoadSuitPalette</c> at <c>$91:DEBA</c>, giving Gravity priority over
    /// Varia and falling back to Power Suit when neither equipment bit is enabled.
    /// </summary>
    public void LoadSuitPalette(SamusState samus, SamusColours colours)
    {
        Ensure.NotNull(colours);

        SamusNormalSuitPalette.Load(colours.Suit, samus.EquippedItems, SuitColors);
    }

    /// <summary>
    /// Seeds pose/frame-zero graphics before the first displayed frame, as room/game setup must do
    /// before a visible normal-gameplay Samus can be constructed.
    /// </summary>
    public void PrimeGraphics(SamusState samus) =>
        BodyFrames.Select(samus.Pose, samus.AnimationFrame);

    /// <summary>
    /// Seeds the animation frame timer as <c>Set_Samus_AnimationFrame_if_PoseChanged</c>
    /// at <c>$91:FB08</c> does, including its bottom-minus-one liquid boundary test.
    /// </summary>
    public static void InitializeAnimation(SamusState samus, int initialFrame = 0)
    {

        samus.AnimationFrame = initialFrame;
        // `$91:FB08` does not simply reuse the previous animation pass's `$0A9A`. It
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
    /// <c>Set_Samus_AnimationFrame_if_PoseChanged</c> at <c>$91:FB08</c> as a scene setup
    /// calls it before committing pose history: the animation restarts only when Pose
    /// differs from PreviousPose ($0A20), and otherwise keeps its frame and timer running.
    /// </summary>
    /// <remarks>
    /// The routine also returns early while $0A9A is negative. This port owns $0A9A as
    /// <see cref="SamusState.AnimationFrameBuffer"/>, so callers must be paths where native $0A9A is
    /// nonnegative, as it is at the intro's $8B:AF6C setup and $91:8739 demo end.
    /// </remarks>
    internal static void SetAnimationFrameIfPoseChanged(SamusState samus)
    {
        if (samus.Pose == samus.PoseHistory.PreviousPose)
            return;
        InitializeAnimation(samus);
    }

    /// <summary>
    /// Restarts the animation's program after a bank-$91 scripted controller writes Pose and
    /// literal animation words without calling ordinary pose initialization.
    /// </summary>
    /// <remarks>
    /// Drained-controller functions one and four do exactly that at `$91:E571/$91:E60C`.
    /// The cartridge's animation pass resolves the pointer from Pose every call, whereas
    /// this C# port caches it as a consistency check. This narrow method updates only that
    /// cache plus the controller's explicit writes; it intentionally does not recompute the
    /// liquid animation buffer or silently refresh collision radius.
    /// </remarks>
    internal static void SetPoseAndAnimationFromScriptedController(SamusState samus, 
        SamusPoseId pose,
        int frame,
        int timer)
    {
        // Both drained controllers store only $0A1C; $0A1E/$0A1F keep the prior movement and collision radii.
        samus.WritePoseWithoutInitialization(pose);
        samus.AnimatedPose = pose;
        samus.AnimationFrame = frame;
        samus.AnimationFrameTimer = WrappingCountdown.FromRaw(timer);
        samus.PendingTransitionalPose = null;
    }

    /// <summary>
    /// Publishes the angle-selected grapple art exactly as $9B:BD95 does before the normal
    /// bank-$91 animation pass. The latter will decrement this fifteen to fourteen later in
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
    /// Ports <c>AnimateSamus</c> at <c>$90:8000</c>, including movement-visible FX delay state.
    /// </summary>
    /// <remarks>
    /// The FX dispatcher publishes water/lava delay buffering, remembered medium, native
    /// atmospheric slots, sound requests, and periodic-damage words before the timer
    /// decrement. The historical name remains as a source-compatible debugger API.
    /// </remarks>
    public static void AnimateNoFx(SamusState samus, SamusAnimationStep step)
    {
        EnsureAnimationInitialized(samus);

        // `$90:8000` dispatches the active room-FX animation handler before touching the
        // frame timer. This also updates remembered `$0AD2`, which the next Space Jump gate
        // consumes independently of its current top-boundary submersion check.
        samus.LiquidPhysics.PrepareAnimationFrame(
            samus,
            step.Random,
            checked((ushort)step.FrameCounter),
            step.BeginLiquidSoundRequestFrame,
            step.BossPresent);

        // $90:8032 keeps neutral-jump frame one alive in four-tick chunks while Samus is
        // still rising. This is intentionally tested before DEC and applies only when the
        // timer is exactly one; release/apex changes YDirection to two and lets it expire.
        if (samus.Pose is SamusPoseId.NeutralJumpRightPose or SamusPoseId.NeutralJumpLeftPose &&
            samus.Kinematics.YDirection != SamusVerticalDirection.Down &&
            samus.AnimationFrame == 1 &&
            samus.AnimationFrameTimer.RawValue == 1)
        {
            samus.AnimationFrameTimer = WrappingCountdown.Start(4);
        }

        // DEC is 16-bit. A timer accidentally initialized to zero becomes $FFFF; BMI then
        // advances just like BEQ does for an ordinary 1 -> 0 expiration.
        samus.AnimationFrameTimer = samus.AnimationFrameTimer.Decrement().Remaining;
        if (!samus.AnimationFrameTimer.IsZero && !samus.AnimationFrameTimer.IsNegative)
            return;

        samus.AnimationFrame++;
        HandleAnimationDelay(samus, step.ControllerInput, step.ProspectiveInputPose, step.DemoPoseInput, step.QueueEchoSound);
    }

    /// <summary>
    /// Ports the animation-only core of <c>Draw_Samus_Starting_Death_Animation</c> at
    /// `$90:8976`. Unlike ordinary gameplay animation, this call deliberately skips liquid-
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
    /// Crystal Flash writes `$0A94/$0A96` at `$90:D689` and `$90:D74E`. This narrow internal
    /// seam preserves the native ordering: movement writes the pair, then animation later
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
    /// Ports the standing and ordinary-running portions of <c>Samus_Draw</c> at
    /// <c>$90:85E2</c>.
    /// </summary>
    /// <remarks>
    /// Movement type zero uses the standing position selector, `$0F/$1B` use their explicit
    /// table-backed selectors, and every other native slot through `$1B` uses the usual
    /// pose offset. The complete `$90:864E` lower-half dispatcher is retained, including
    /// all unused-but-valid movement types. Morph types `$04/$08` draw only their complete
    /// top spritemap; spin-jump `$03` retains its own conditional bottom rule. Draygon's
    /// ten type-`$1A` bodies have ordinary split top/bottom spritemaps and no draw-time offset.
    /// </remarks>
    public bool Draw(SamusState samus, 
        SpriteFrameBuffer sprites,
        Position16<RoomPixels> layer1,
        int frameCounter = 0,
        TurningFloorTransform? turningFloor = null)
    {
        Ensure.NotNull(sprites);

        SamusMovementType movementType = SamusPoseMetadata.ReadMovementType(samus);

        // `$90:85E2-$90:85FC` applies invincibility flicker to the body spritemaps, not to
        // animation or graphics streaming as a whole. An odd frame hides ordinary invincible
        // Samus. Active knockback and any nonzero stored/active shine timer independently
        // force the body visible, even though the separately drawn arm cannon follows its
        // stricter `$90:C663` rule and may still disappear on that same odd frame.
        bool bodyVisible = !samus.KnockbackTimer.IsZero ||
            samus.InvincibilityTimer.IsZero ||
            SamusInputSnapshot.SharedShineTimer(samus) != 0 ||
            (frameCounter & 1) == 0;
        if (!bodyVisible)
        {
            // Native branches around every position/index/OAM write, then still falls
            // through to `$92:8000`. Preserve the previous visible spritemap indices for
            // echo drawing while selecting this frame's tile definitions for the next displayed frame.
            BodyFrames.Select(samus.Pose, samus.AnimationFrame);
            return false;
        }

        // `$90:8C1F` temporarily overwrites Samus's integer world point only while Ceres's
        // high status bit is set. Every position selector below therefore consumes the
        // rotated point, but public physics coordinates remain completely untouched.
        Position16<RoomPixels> position = samus.Position.Whole;
        Position16<RoomPixels> renderPoint = turningFloor is { } transform
            ? transform.Transform(position)
            : position;
        int renderX = renderPoint.X;
        int renderY = renderPoint.Y;

        // $90:8C94 sign-extends the byte at pose-definition offset four. Pose $01 stores
        // +6, moving the art origin six pixels above Samus's world-space center.
        int graphicsYOffset = SamusPoseMetadata.ReadGraphicsYOffset(samus);
        int spritemapX = unchecked((ushort)(renderX - layer1.X));
        int spritemapY;
        // The part of the stored spritemap Y that OAM never shows: a landing frame's leaked neighbouring byte.
        int landingOffsetHighPart = 0;
        if (movementType == SamusMovementType.Standing &&
            samus.Pose is SamusPoseId.ForwardFacingPowerSuitPose or SamusPoseId.ForwardFacingSuitedPose &&
            samus.AnimationFrame >= 2)
        {
            // `$90:8D07-$90:8D27` uses the ordinary pose graphics offset for front-facing
            // frames zero/one, then pins every later frame exactly one pixel above Samus's
            // center. This is independent of `$00`'s extra power-suit chest-cover OBJ.
            spritemapY = unchecked((ushort)(renderY - 1 - layer1.Y));
        }
        else if (movementType == SamusMovementType.Standing &&
            SamusPoseNumbering.InRange(samus.Pose, SamusPoseId.NormalLandingRightPose, SamusPoseId.SpinLandingLeftPose))
        {
            // `$90:8CDC-$90:8CF6` indexes sixteen packed bytes, but performs a 16-bit
            // unaligned LDA. The following landing frame's byte therefore becomes the high
            // byte of the subtraction. The complete wrapped `$0AFA` spritemap-position word
            // is observable state and is preserved, but OAM displays only low Y, so the
            // drawn origin takes back the leaked high byte (#1397).
            int landingOffsetIndex = SamusPoseNumbering.Offset(samus.Pose, SamusPoseId.NormalLandingRightPose) * 4 +
                samus.AnimationFrame;
            if (BodyFrames.Artwork is not { } landingArt ||
                !landingArt.TryLandingYOffset(landingOffsetIndex, out int landingOffset))
                throw new InvalidDataException($"Samus landing offset {landingOffsetIndex} lacks installed art.");
            spritemapY = unchecked((ushort)(
                renderY - landingOffset - layer1.Y));
            landingOffsetHighPart = (ushort)(landingOffset - (byte)landingOffset);
        }
        else if (movementType == SamusMovementType.PostureTransition &&
            !SamusPoseNumbering.Precedes(samus.Pose, SamusPoseId.CrouchingTransitionRightPose) &&
            SamusPoseNumbering.Precedes(samus.Pose, SamusPoseId.MorphBallGroundLeftPose))
        {
            // `$90:8D3C` indexes a signed byte by `2*(pose-$35)+animation frame` instead
            // of using the pose-definition graphics offset. Installed visual bytes
            // retain ordinary crouch/morph/stand/unmorph values and the four
            // valid-but-unused zero records `$39/$3A/$3F/$40`. An out-of-range
            // selection rejects; neighboring cartridge bytes are not a runtime fallback.
            int postureIndex = (samus.Pose - SamusPoseId.CrouchingTransitionRightPose) * 2 + samus.AnimationFrame;
            if (BodyFrames.Artwork is not { } postureArt ||
                !postureArt.TryPostureYOffset(postureIndex, out int transitionOffset))
                throw new InvalidDataException($"Samus posture offset {postureIndex} lacks installed art.");
            spritemapY = unchecked((ushort)(renderY + transitionOffset - layer1.Y));
        }
        else if (samus.Pose is SamusPoseId.DrainedCrouchingRightPose or SamusPoseId.DrainedCrouchingLeftPose)
        {
            // `$90:8DC1` indexes the shared 32-byte table at `$90:8DEF` directly with the
            // animation byte index. Several indices intentionally name command operands,
            // because the external controller can publish those literal indices for a
            // visible frame. The installed 32-byte table preserves that odd layout.
            if (BodyFrames.Artwork is not { } drainedArt ||
                !drainedArt.TryDrainedYOffset(samus.AnimationFrame, out int drainedOffset))
                throw new InvalidDataException($"Samus drained offset {samus.AnimationFrame} lacks installed art.");
            spritemapY = unchecked((ushort)(renderY + drainedOffset - layer1.Y));
        }
        else if ((samus.Pose is SamusPoseId.DrainedStandingRightPose or SamusPoseId.DrainedStandingLeftPose) &&
                 samus.AnimationFrame >= 5)
        {
            // `$90:8DB1-$8DBC` replaces the usual pose graphics offset with -3 after
            // standing drained art reaches byte index five.
            spritemapY = unchecked((ushort)(renderY - 3 - layer1.Y));
        }
        else
        {
            spritemapY = unchecked((ushort)(renderY - graphicsYOffset - layer1.Y));
        }
        SpritemapPosition = Position16<ScreenPixels>.Of(spritemapX, spritemapY);
        Position16<ScreenPixels> drawOrigin = Position16<ScreenPixels>.Of(spritemapX, unchecked((ushort)(spritemapY + landingOffsetHighPart)));

        var spritemaps = BodyFrames.Artwork?.Spritemaps ?? throw new InvalidOperationException(
            "Samus rendering requires installed spritemap artwork.");
        var top = new SamusBodyHalfFrame(samus.Pose, SamusBodyHalf.Top, samus.AnimationFrame);
        TopSpritemap = top;
        // `$92:8000` follows these spritemaps natively, but it only chooses the transfers the next displayed frame shows with
        // them. Selecting first lets the draws bind the characters they are displayed with; nothing between reads it.
        BodyFrames.Select(samus.Pose, samus.AnimationFrame);
        ShownPictures = BodyFrames.Shown;
        sprites.AddSamusBody(top, drawOrigin, spritemaps, BodySheets, ShownPictures, new SpriteSource(SpriteSourceKind.Samus, 0));

        // `$90:868D-$90:86C4` writes one small OBJ directly between the top and bottom
        // spritemap calls when unsuited pose `$00` faces the screen. This is not a visor:
        // the disassembly identifies tile `$021` as a cover for the left side of the power-
        // suit chest. The suited `$9B` body has that shape in its ordinary spritemap and
        // deliberately skips this write. Coordinates use Samus's world center directly,
        // not SpritemapPosition.Y (which has already applied pose graphics offset `$08`).
        if (samus.Pose == SamusPoseId.ForwardFacingPowerSuitPose)
        {
            int chestX = unchecked((ushort)(samus.Position.X.Whole - 7 - layer1.X));
            int chestY = unchecked((ushort)(samus.Position.Y.Whole - 17 - layer1.Y));
            sprites.Add(new ImageSpriteDraw(new SpriteSource(SpriteSourceKind.SamusChest),
                BodySheets.Get(SpriteSheetId.SamusOverlays).Frame(SamusOverlaySheetDefinitions.ChestCoverFrame),
                Position16<ScreenPixels>.Of(chestX, chestY), SpriteMirror.None));
        }

        // $90:8686 suppresses the ordinary spin-jump bottom half for art frames 1..A;
        // those frames' top spritemaps contain the complete curled body. Frame zero and
        // frames B+ draw the split bottom. The admitted Screw/Space Jump records use their
        // separate native rule and always draw the bottom half at every animation frame.
        bool ordinarySpinBottom = movementType != SamusMovementType.SpinJumping ||
            samus.Pose is SamusPoseId.SpaceJumpRightPose or SamusPoseId.SpaceJumpLeftPose or
                SamusPoseId.ScrewAttackRightPose or SamusPoseId.ScrewAttackLeftPose ||
            samus.AnimationFrame == 0 || samus.AnimationFrame >= 11;

        // `$90:86EE` is movement type `$0A`'s only exception. The first three frames of
        // the `$D7/$D8` Crystal-Flash-end/fatal-damage body are complete top spritemaps;
        // all other knockback-family poses and later frames retain an ordinary lower half.
        bool knockbackBottom = movementType != SamusMovementType.Knockback ||
            samus.Pose is not (SamusPoseId.DeathSequenceRightPose or SamusPoseId.DeathSequenceLeftPose) ||
            samus.AnimationFrame >= 3;

        // `$90:870C-$90:874B` is a pose-and-frame dispatcher, not a blanket type-$0F
        // policy. Basic crouch/stand transitions always use a lower half. Morph/unmorph
        // bodies below `$DB` never do. `$DB/$DC` draw it only on frame zero, `$DD-$F0`
        // only on frame two, and the aimed transition family `$F1+` always draws it.
        bool transitionBottom = movementType != SamusMovementType.PostureTransition ||
            !SamusPoseNumbering.Precedes(samus.Pose, SamusPoseId.CrouchingTransitionAimUpRightPose) ||
            samus.Pose is SamusPoseId.CrouchingTransitionRightPose or SamusPoseId.CrouchingTransitionLeftPose or
                SamusPoseId.StandingTransitionRightPose or SamusPoseId.StandingTransitionLeftPose ||
            (samus.Pose is SamusPoseId.UnusedPoseDb or SamusPoseId.UnusedPoseDc &&
                samus.AnimationFrame == 0) ||
            (!SamusPoseNumbering.Precedes(samus.Pose, SamusPoseId.UnusedPoseDd) &&
                SamusPoseNumbering.Precedes(samus.Pose, SamusPoseId.CrouchingTransitionAimUpRightPose) && samus.AnimationFrame == 2);

        // The otherwise-unused movement type `$0D` still has executable cartridge logic
        // at `$90:874C`: poses `$65/$66` draw a lower half only on frame zero, while every
        // other type-`$0D` pose follows the ordinary always-split return.
        bool unusedTypeDBottom = movementType != SamusMovementType.Unused0D ||
            samus.Pose is not (SamusPoseId.UnusedPose65 or SamusPoseId.UnusedPose66) ||
            samus.AnimationFrame < 1;

        bool wallJumpBottom = movementType != SamusMovementType.WallJumping ||
            samus.AnimationFrame < 3 || samus.AnimationFrame >= 13;
        bool damageBoostBottom = movementType != SamusMovementType.DamageBoost ||
            samus.AnimationFrame < 2 || samus.AnimationFrame >= 9;
        // `$90:8790` suppresses the lower half for vertical shinesparks and for drained
        // crouch/fall byte indices zero and one. Every other type-$1B record draws it.
        bool specialType1BBottom = movementType != SamusMovementType.Special ||
            (samus.Pose is not (SamusPoseId.ShinesparkVerticalRightPose or SamusPoseId.ShinesparkVerticalLeftPose) &&
             (samus.Pose is not (SamusPoseId.DrainedCrouchingRightPose or SamusPoseId.DrainedCrouchingLeftPose) ||
              samus.AnimationFrame >= 2));
        bool drawBottom = movementType is not (
            SamusMovementType.MorphBallGround or
            SamusMovementType.UnusedGlitchBall or
            SamusMovementType.MorphBallFalling or
            SamusMovementType.UnusedGlitchBallAlternate or
            SamusMovementType.SpringBallGround or
            SamusMovementType.SpringBallInAir or
            SamusMovementType.SpringBallFalling) &&
            ordinarySpinBottom && knockbackBottom && transitionBottom && unusedTypeDBottom &&
            wallJumpBottom && damageBoostBottom && specialType1BBottom;
        // A complete top-half frame clears the bottom picture, so the following echo renderer
        // never reuses a bottom half left by an earlier animation frame.
        BottomSpritemap = null;
        if (drawBottom)
        {
            var bottom = new SamusBodyHalfFrame(samus.Pose, SamusBodyHalf.Bottom, samus.AnimationFrame);
            BottomSpritemap = bottom;
            sprites.AddSamusBody(bottom, drawOrigin, spritemaps, BodySheets, ShownPictures, new SpriteSource(SpriteSourceKind.Samus, 1));
        }

        return true;
    }

    /// <summary>
    /// Draws the two ordinary Speed-Booster echoes from the positions captured by
    /// <c>Samus_UpdateSpeedEchoPos</c> at <c>$90:EEE7</c>.
    /// </summary>
    /// <remarks>
    /// This preserves both branches of <c>Samus_DrawEchoes</c> at <c>$90:87BD</c>. A
    /// nonnegative index draws stationary trailing snapshots only at boost stage four.
    /// Cancellation changes the index to <c>$FFFF</c>; drawing then advances each body's
    /// native ±8 X / ±2 Y convergence before deciding whether it crossed the live Samus.
    /// </remarks>
    public void DrawSpeedBoosterEchoes(SamusState samus, 
        SpriteFrameBuffer sprites,
        Position16<RoomPixels> layer1)
    {
        Ensure.NotNull(sprites);

        if (samus.HorizontalSpeed.SpeedEchoesDeparting)
        {
            // `$90:87D3` walks slot one before slot zero. Movement is a draw-handler side
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

        // `$90:87C7` draws slot one before slot zero. OAM order is observable when their
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

        // Slot one precedes slot zero just like native's X=2,0 loop.
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
    /// Ports <c>Samus_DrawShinesparkCrashEchoProjectiles</c> at <c>$90:8953</c>. These
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

        // `$90:8953` tests/draws fixed slot four (speed echo index three) before fixed
        // slot three (index two). That ordering controls OAM priority when copies overlap.
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

    /// <summary>Draws a populated echo slot with current Samus spritemaps after native Y clipping.</summary>
    private void DrawActiveSpeedBoosterEcho(SamusState samus, 
        SpriteFrameBuffer sprites,
        Position16<RoomPixels> echo,
        Position16<RoomPixels> layer1,
        int slot)
    {
        // Zero X is the cartridge's empty-slot sentinel, not merely an off-screen point.
        if (echo.X == 0)
            return;

        int graphicsYOffset = SamusPoseMetadata.ReadGraphicsYOffset(samus);
        int screenY = unchecked((short)(echo.Y - graphicsYOffset - layer1.Y));

        // The original accepts screen Y 0..247. Horizontal clipping remains OAM/PPU work,
        // exactly as it is for the current Samus body.
        if (screenY is < 0 or >= 248)
            return;

        var spritemaps = BodyFrames.Artwork?.Spritemaps ?? throw new InvalidOperationException(
            "Samus speed-booster echo requires installed spritemap artwork.");
        int screenX = unchecked((ushort)(echo.X - layer1.X));
        var origin = Position16<ScreenPixels>.Of(screenX, unchecked((ushort)screenY));
        // An echo repeats the pictures the body last drew; before the body has been drawn there is nothing to repeat.
        if (TopSpritemap is { } top)
            sprites.AddSamusBody(top, origin, spritemaps, BodySheets, ShownPictures, new SpriteSource(SpriteSourceKind.SamusEcho, slot * 2));
        if (BottomSpritemap is { } bottom)
            sprites.AddSamusBody(bottom, origin, spritemaps, BodySheets, ShownPictures, new SpriteSource(SpriteSourceKind.SamusEcho, slot * 2 + 1));
    }
}
