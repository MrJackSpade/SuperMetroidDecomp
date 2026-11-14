using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// Crystal Flash's three phases of moving Samus (<see cref="SamusMovementOwner.CrystalFlash"/>): raising, the main
/// bubble phase, and the ending.
/// </summary>
/// <remarks>
/// Crystal Flash is unusual even among Samus's scripted movement states. The game
/// attempts it only when the power-bomb explosion centred on Samus finishes; successful
/// initiation then takes both her pose input and her movement from her pose. Keeping that
/// hold as <see cref="Phase"/> prevents <see cref="SamusPoseId.CrystalFlashRightPose"/> and <see cref="SamusPoseId.CrystalFlashLeftPose"/>
/// from accidentally inheriting invented special-movement physics.
/// </remarks>
public sealed class SamusCrystalFlashState
{
    /// <summary>The exact normal-game controller chord: Down + L + R + the Shot binding.</summary>
    public const ControllerButton RequiredInputWithoutShot = ControllerButtons.CrystalFlashWithoutShot;

    /// <summary>The phase in which Crystal Flash moves Samus, or none once another owner has taken her movement.</summary>
    public CrystalFlashPhase Phase { get; private set; }

    /// <summary>Initialized to nine for the ten two-pixel raise frames.</summary>
    public int RaiseTimer { get; private set; }

    /// <summary>
    /// The ammunition the drain is consuming: missiles at activation, then super missiles and power bombs as each
    /// ten-decrement run expires. No other owner shares this selector.
    /// </summary>
    public CrystalFlashAmmoKind DrainingAmmo { get; private set; }

    /// <summary>
    /// The updates until the next unit of ammunition drains, reloaded to ten between the three ammunition families, on the
    /// count Draygon's escape and the suit pickup's stage share, so it counts on from whatever they last left.
    /// </summary>
    private readonly SharedEscapeAndDrainCount _ammoCountdown;

    /// <summary>Creates a Crystal Flash owner whose ammunition countdown runs on Samus's shared escape and drain count.</summary>
    /// <param name="ammoCountdown">Samus's shared escape and drain count.</param>
    internal SamusCrystalFlashState(SharedEscapeAndDrainCount ammoCountdown) => _ammoCountdown = Ensure.NotNull(ammoCountdown);

    /// <summary>
    /// Ends Crystal Flash's hold on Samus's movement when another owner takes it (<see cref="SamusMovementHandOver"/>),
    /// without clearing the live Flash palette/timer.
    /// </summary>
    internal void RelinquishMovement() => Phase = CrystalFlashPhase.Inactive;

    /// <summary>Another special suit colouring takes over from Crystal Flash's, along with its running timer.</summary>
    internal void RelinquishSuitColouring()
    {
        SpecialPaletteKind = SamusSpecialPaletteType.None;
        BubbleTimer = 0;
        BeamPaletteRestoreRequested = false;
    }

    /// <summary>
    /// Samus's room position captured after the complete 20-pixel rise. Only the Y axis is meaningful: the finishing
    /// phase compares Samus's whole Y with it through the wrapped spatial difference and walks her down one
    /// pixel per call while they differ.
    /// </summary>
    public Position16<RoomPixels> RaisedPosition { get; private set; }

    /// <summary>Seeded to one for Samus's palette animation.</summary>
    public int CrystalPaletteTimer { get; private set; }

    /// <summary>
    /// The special suit colouring Crystal Flash runs: Crystal Flash's from activation until the finish restores the
    /// beam colours, or none once another colouring takes over.
    /// </summary>
    public SamusSpecialPaletteType SpecialPaletteKind { get; internal set; }

    /// <summary>
    /// The bubble colour step shown next, 0 through the bubble-set count minus one. Each bubble-timer expiry advances
    /// it by one and wraps it to zero after the last set.
    /// </summary>
    public int BubbleStep { get; private set; }

    /// <summary>
    /// The palette updates until the bubble shows its next colour step, one through five while Crystal Flash's palette
    /// runs (it is reloaded on reaching zero), and zero once it stops; other values are rejected.
    /// </summary>
    public int BubbleTimer
    {
        get;
        private set => field = Ensure.BetweenInclusive(0, BubbleStepUpdates, value);
    }

    /// <summary>The palette updates each bubble colour step shows for.</summary>
    private const int BubbleStepUpdates = 5;

    /// <summary>
    /// Whether the flash has finished and asks its next palette update to show the equipped beams' colours again and
    /// release the palette.
    /// </summary>
    public bool BeamPaletteRestoreRequested { get; internal set; }

    /// <summary>
    /// The step of the body's colour pulse shown next, zero through nine (#1512): activation starts it at the first, and
    /// each body-timer expiry shows it and advances it, wrapping to the first after the last.
    /// </summary>
    public int BodyStep { get; private set; }

    /// <summary>Set when the raise finishes and the bubble's two per-line window effects start.</summary>
    public bool BubbleEffectRequested { get; private set; }


    /// <summary>
    /// Starts Crystal Flash when Samus may use it: checks every admission condition and sets up the raise.
    /// </summary>
    /// <param name="samus">Live actor whose admission state is checked and whose pose, movement owner, shared timers, and hit state are updated on success.</param>
    /// <param name="controllerInput">The held buttons; ordinary admission requires exactly Down, L, R and the Shot button held.</param>
    /// <param name="shotBinding">The button bound to Shot, defaulting to X; a binding that names no button is rejected.</param>
    /// <param name="inputCheck">
    /// True only while a title demo is ending and the game is returning from it. Ordinary
    /// gameplay must supply the exact chord; extra held buttons make initiation fail.
    /// </param>
    /// <returns>True after starting the raising phase; false when the chord or the speed, health, reserve, or ammunition preconditions reject activation.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="samus"/> is null.</exception>
    /// <exception cref="InvalidDataException">The held buttons or the Shot binding name something that is not a button, or the Crystal Flash pose no longer has special movement.</exception>
    public bool TryBegin(
        SamusState samus,
        ControllerButton controllerInput,
        ControllerButton shotBinding = ControllerButton.X,
        CrystalFlashInputCheck inputCheck = CrystalFlashInputCheck.Required)
    {
        Ensure.NotNull(samus);

        // The chord test uses an equality comparison, not a bit test. This deliberately rejects any extra
        // held button, including Jump, Dash, Select, or the opposite D-pad directions.
        ControllerButton heldButtons = controllerInput;
        ControllerButton shotButton = shotBinding;
        ControllerButton requiredInput = RequiredInputWithoutShot | shotButton;
        if (inputCheck != CrystalFlashInputCheck.Skipped && heldButtons != requiredInput)
            return false;

        // The remaining preconditions are literal comparisons. Reserve energy must
        // be empty even when the reserve mode is manual; possession of a reserve tank alone
        // does not matter. Each current ammo count—not its maximum—must supply ten shots.
        if (samus.Kinematics.YSpeed.Whole != 0 || samus.Kinematics.YSpeed.Fraction != 0 ||
            samus.Health >= 51 || samus.ReserveEnergy != 0 ||
            samus.Missiles < 10 ||
            samus.SuperMissiles < 10 ||
            samus.PowerBombs < 10)
        {
            return false;
        }

        // Crystal Flash takes a left-facing pose as left and every other facing as right.
        // Read the facing from the source pose before replacing it with Crystal Flash art.
        bool facingLeft = SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left);
        samus.Pose = facingLeft
            ? SamusPoseId.CrystalFlashLeftPose
            : SamusPoseId.CrystalFlashRightPose;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        if (SamusPoseMetadata.ReadMovementKind(samus) != SamusMovementType.Special)
            throw new InvalidDataException("The Crystal Flash poses no longer have the special movement type.");
        SamusPresentation.InitializeAnimation(samus, initialFrame: 0);
        // Successful activation publishes history directly, before
        // taking Samus's movement. Rejected activation must not shift it.
        samus.CommitPoseHistory();

        // Crystal Flash reuses shinespark state but moves Samus by its own phases. The
        // states stay separate so Crystal Flash is never mistaken for a spark.
        samus.Shinespark.RelinquishToCrystalFlash();
        // Reserve Mode leaves X-Ray's cone effect alive but Crystal Flash still takes over
        // Samus's special suit colouring. Keep the greyout; release only X-ray's suit
        // colouring so Crystal Flash's colouring alone colours Samus.
        samus.Xray.RelinquishSuitColouring();
        // Flash admission can occur after the Super Metroid has already installed drained
        // crouching art, or while Draygon holds Samus. Crystal Flash takes her movement from both; if the
        // drained state kept its Drained phase alive, it would re-lock Samus after Flash completed
        // instead of reproducing the documented adjacent timing that frees the stun.
        SamusMovementHandOver.To(samus, SamusMovementOwner.CrystalFlash);
        samus.InputSnapshot.CrystalFlashPoseInputLocked = true;
        RaiseTimer = 9;
        DrainingAmmo = CrystalFlashAmmoKind.Missiles;
        _ammoCountdown.StartAmmoCountdown(10);
        RaisedPosition = Position16<RoomPixels>.Zero;
        CrystalPaletteTimer = 1;
        SpecialPaletteKind = SamusSpecialPaletteType.CrystalFlash;
        BubbleStep = 0;
        BubbleTimer = 1;
        BeamPaletteRestoreRequested = false;
        BodyStep = 0;
        BubbleEffectRequested = false;
        // Activation discards existing hit immunity; Crystal Flash is not a damage shield.
        samus.InvincibilityTimer = WrappingCountdown.Start(0);
        samus.KnockbackTimer = 0;
        samus.KnockbackDirection = Movement.SamusKnockbackDirection.None;
        samus.KnockbackActive = false;
        Phase = CrystalFlashPhase.Raising;
        return true;
    }

    /// <summary>Runs one update of Crystal Flash's current phase in Samus's movement phase.</summary>
    /// <param name="samus">Live actor receiving movement, ammunition/energy, animation, and phase-completion writes.</param>
    /// <param name="frameCounter">The frame counter; ammunition drains only on every eighth frame.</param>
    /// <returns>A value snapshot of the entry phase and retained bubble-effect request, even when this call changes phase.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="samus"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Crystal Flash does not hold Samus's movement, or the phase/ammunition-family state is unsupported.</exception>
    public CrystalFlashMovementResult Step(
        SamusState samus,
        int frameCounter)
    {
        Ensure.NotNull(samus);
        if (Phase == CrystalFlashPhase.Inactive)
            throw new InvalidOperationException("Crystal Flash does not hold Samus's movement.");

        CrystalFlashPhase phaseAtStart = Phase;

        switch (Phase)
        {
            case CrystalFlashPhase.Raising:
                StepRaising(samus);
                break;

            case CrystalFlashPhase.DrainingAmmo:
                StepAmmoDrain(samus, frameCounter);
                // The main phase clears both values after each ammunition drain,
                // including its seven no-drain frames and the final phase-changing call.
                samus.InvincibilityTimer = WrappingCountdown.Start(0);
                samus.KnockbackTimer = 0;
                break;

            case CrystalFlashPhase.Finishing:
                StepFinish(samus);
                break;

            case CrystalFlashPhase.Inactive:
            default:
                throw new InvalidOperationException($"Unknown Crystal Flash phase {Phase}.");
        }

        return new CrystalFlashMovementResult(
            phaseAtStart,
            BubbleEffectRequested);
    }

    /// <summary>Raises Samus two whole pixels for ten calls, then starts the drain phase and requests the bubble effect.</summary>
    private void StepRaising(SamusState samus)
    {
        // The raise changes only the whole pixels. The subposition survives.
        samus.Position -= new Offset16<RoomPixels>(0, 2);

        // The game initializes nine, then decrements and checks for a count below zero. Values 8..0 return; the tenth call runs
        // past zero and starts the main bubble phase after completing its Y move.
        if (--RaiseTimer >= 0)
            return;

        // The delay animation is already on its raise loop, but the raise forces
        // frame six and timer three so main art begins on this precise transition frame.
        SamusPresentation.SetAnimationFrameFromSpecialHandler(samus, frame: 6, timer: 3);
        RaisedPosition = Position16<RoomPixels>.Zero.WithY(samus.Position.Y.Whole);
        Phase = CrystalFlashPhase.DrainingAmmo;
        BubbleEffectRequested = true;
        // The raise becomes the main phase with one essential-priority request that silences the room library.
        samus.AudioRequests.QueueSound(RoomSound.Silence, SoundRequestPriority.Essential);
        samus.InvincibilityTimer = WrappingCountdown.Start(0);
        samus.KnockbackTimer = 0;
        samus.KnockbackActive = false;
    }

    /// <summary>
    /// Consumes missiles, supers, then power bombs on each eighth frame and restores 50 energy per decrement. Each
    /// decrement is unchecked, so a kind the activation check let through with fewer than ten drains below zero.
    /// </summary>
    private void StepAmmoDrain(SamusState samus, int frameCounter)
    {
        // All three drains do nothing on seven of every eight frames of the frame counter;
        // Crystal Flash keeps no cadence of its own.
        if (!SamusCadenceDefinitions.CrystalFlashDrainEveryEighthUpdate.IsDue(frameCounter))
            return;

        switch (DrainingAmmo)
        {
            case CrystalFlashAmmoKind.Missiles:
                samus.Missiles = CountsRunBelowZero.DecrementWrapping(samus.Missiles);
                break;
            case CrystalFlashAmmoKind.SuperMissiles:
                samus.SuperMissiles = CountsRunBelowZero.DecrementWrapping(samus.SuperMissiles);
                break;
            case CrystalFlashAmmoKind.PowerBombs:
                samus.PowerBombs = CountsRunBelowZero.DecrementWrapping(samus.PowerBombs);
                break;
            default:
                throw new InvalidOperationException(
                    $"Crystal Flash ammunition kind {DrainingAmmo} is undefined.");
        }

        SamusEnergyRestoration.Restore(samus, 50);
        CountdownStep ammoTimer = _ammoCountdown.CountDownAmmo();
        bool timerExpired = ammoTimer.IsZeroOrNegative;
        if (!timerExpired)
            return;

        if (DrainingAmmo != CrystalFlashAmmoKind.PowerBombs)
        {
            _ammoCountdown.StartAmmoCountdown(10);
            DrainingAmmo = DrainingAmmo == CrystalFlashAmmoKind.Missiles
                ? CrystalFlashAmmoKind.SuperMissiles
                : CrystalFlashAmmoKind.PowerBombs;
            return;
        }

        // The ammunition drain switches to finish art directly; it does not wait for the main
        // four-frame animation loop to reach a cue.
        Phase = CrystalFlashPhase.Finishing;
        SamusPresentation.SetAnimationFrameFromSpecialHandler(samus, frame: 12, timer: 3);
    }

    /// <summary>Waits for standing pose after the finish art, then releases input locks and requests palette restoration.</summary>
    private void StepFinish(SamusState samus)
    {
        // Retail usually enters with equality because RaisedPosition was captured at the
        // end of the rise. Preserve the real defensive branch: an external producer that
        // displaced Samus upward/downward makes the whole Y walk downward one per call.
        if ((samus.Position.Whole - RaisedPosition).Y != 0)
            samus.Position += new Offset16<RoomPixels>(0, 1);

        // The pose's animation ends in a transition to the facing standing pose, which occurs after movement.
        // Therefore cleanup naturally happens on the following movement phase, once type zero is
        // observable here, and that frame performs no ordinary standing movement.
        if (SamusPoseMetadata.ReadMovementKind(samus) != SamusMovementType.Standing)
            return;

        BeamPaletteRestoreRequested = true;
        samus.InputSnapshot.ShinesparkPoseInputLocked = false;
        samus.InputSnapshot.CrystalFlashPoseInputLocked = false;
        // Normal-input completion clears the same hit timers as the return path.
        samus.InvincibilityTimer = WrappingCountdown.Start(0);
        samus.KnockbackTimer = 0;
        samus.KnockbackActive = false;
        Phase = CrystalFlashPhase.Inactive;
    }

    /// <summary>
    /// Runs Crystal Flash's suit colouring for one update.
    /// </summary>
    /// <remarks>
    /// The beams' colours are split deliberately: the body's shade pulses through ten steps of ten calls each, while the
    /// bubble's shade, laid over the beams' upper colours, rotates through six steps every five updates. The import
    /// pre-colours Samus in every step of both (#1512); completion shows the beam shade of the currently equipped beam
    /// combination again.
    /// </remarks>
    public bool UpdatePalette(SamusColours colours, SamusState samus)
    {
        Ensure.NotNull(colours);
        Ensure.NotNull(samus);
        if (SpecialPaletteKind != SamusSpecialPaletteType.CrystalFlash)
            return false;

        if (BeamPaletteRestoreRequested)
        {
            // Completion reloads the equipped combination's beam palette with the loader equipping a beam uses, so a Spazer
            // and Plasma combination shows the same colours as everywhere else (#1506, #1736).
            SuperMetroid.Core.Samus.Projectiles.SamusProjectileSystem.LoadBeamPalette(colours, samus.EquippedBeams);

            SpecialPaletteKind = SamusSpecialPaletteType.None;
            BubbleStep = 0;
            BodyStep = 0;
            BubbleTimer = 0;
            BeamPaletteRestoreRequested = false;
            CrystalPaletteTimer = 0;
            return true;
        }

        // The bubble countdown runs one through five while the palette runs, so its decrement reaches zero, never past it.
        if (--BubbleTimer == 0)
        {
            BubbleTimer = BubbleStepUpdates;
            colours.LayOverBeams(SamusShades.CrystalFlashBubble(BubbleStep));

            // The step was validated below the set count above, so this advance stays far below any wrap.
            int nextBubbleStep = BubbleStep + 1;
            BubbleStep = nextBubbleStep < SamusPaletteDefinitions.CrystalFlash.BubblePaletteCount
                ? nextBubbleStep
                : 0;
        }

        if (--CrystalPaletteTimer <= 0)
        {
            CrystalPaletteTimer = CrystalFlashPaletteTiming.DurationForStep(BodyStep);
            colours.LayOverBeams(SamusShades.CrystalFlashBody(BodyStep));

            // The step was validated as one of the pulse's above, so this advance cannot wrap.
            int nextStep = BodyStep + 1;
            BodyStep = nextStep < SamusPaletteDefinitions.CrystalFlash.BodyStepCount ? nextStep : 0;
        }

        // Every active call reports the palette handled, including frames on which neither timer
        // expires. The outer palette step must therefore suppress its normal copy.
        return true;
    }
}
