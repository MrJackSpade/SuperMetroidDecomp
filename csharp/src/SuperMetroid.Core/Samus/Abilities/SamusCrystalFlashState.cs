using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Colours;
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
    /// <summary>The exact held actions that start Crystal Flash: Down, both aims and Shoot, and nothing else.</summary>
    public const GameplayAction RequiredInput =
        GameplayAction.Down | GameplayAction.AimDown | GameplayAction.AimUp | GameplayAction.Shoot;

    /// <summary>The phase in which Crystal Flash moves Samus, or none once another owner has taken her movement.</summary>
    public CrystalFlashPhase Phase { get; private set; }

    /// <summary>The rise's countdown: nine when it starts, for its ten two-pixel steps.</summary>
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
    private readonly SharedEscapeAndDrainCount<SamusSuitPickupStage> _ammoCountdown;

    /// <summary>The special suit colouring holding Samus's suit, which Crystal Flash takes.</summary>
    private readonly SamusSpecialColouring _colouring;

    /// <summary>Creates a Crystal Flash owner whose ammunition countdown runs on Samus's shared escape and drain count.</summary>
    /// <param name="ammoCountdown">Samus's shared escape and drain count.</param>
    /// <param name="colouring">Samus's special suit colouring, which Crystal Flash takes.</param>
    internal SamusCrystalFlashState(SharedEscapeAndDrainCount<SamusSuitPickupStage> ammoCountdown, SamusSpecialColouring colouring)
    {
        _ammoCountdown = Ensure.NotNull(ammoCountdown);
        _colouring = Ensure.NotNull(colouring);
    }

    /// <summary>
    /// Ends Crystal Flash's hold on Samus's movement when another owner takes it (<see cref="SamusMovementHandOver"/>),
    /// without clearing the live Flash colouring and timer.
    /// </summary>
    internal void RelinquishMovement() => Phase = CrystalFlashPhase.Inactive;

    /// <summary>
    /// Another special suit colouring takes over from Crystal Flash's, along with its running timer. Its pulse and bubble
    /// play no more until Crystal Flash starts them again.
    /// </summary>
    internal void RelinquishSuitColouring() => BeamShadeRestoreRequested = false;

    /// <summary>
    /// The room line Samus's whole Y reached at the end of the complete 20-pixel rise: the finishing phase walks her down
    /// one pixel an update while her whole Y differs from it.
    /// </summary>
    public int RaisedHeight { get; private set; }

    /// <summary>
    /// The body's colour pulse (#2387), its ten shades of ten colouring updates each played over and over: activation
    /// starts it, and its first shade shows on the flash's first colouring update.
    /// </summary>
    internal AnimationPlayer<SamusGlowSegment, PictureShade, NoAnimationCue> BodyPulse { get; } =
        SamusGlowPlay.Starting(SamusGlowSegment.CrystalFlashBody);

    /// <summary>
    /// The bubble's colours turning (#2387), its six shades of five colouring updates each played over and over on their
    /// own count beside the body's: activation starts it, and its first shade shows on the flash's first colouring update.
    /// </summary>
    internal AnimationPlayer<SamusGlowSegment, PictureShade, NoAnimationCue> BubbleTurn { get; } =
        SamusGlowPlay.Starting(SamusGlowSegment.CrystalFlashBubble);

    /// <summary>
    /// Whether the flash has finished and asks its next colouring update to show the equipped beams' colours again and
    /// release the shade.
    /// </summary>
    public bool BeamShadeRestoreRequested { get; private set; }

    /// <summary>Set when the raise finishes and the bubble's explosion and its glow start.</summary>
    public bool BubbleEffectRequested { get; private set; }


    /// <summary>
    /// Starts Crystal Flash when Samus may use it: checks every admission condition and sets up the raise.
    /// </summary>
    /// <param name="samus">Live actor whose admission state is checked and whose pose, movement owner, shared timers, and hit state are updated on success.</param>
    /// <param name="controllerInput">The held actions: exactly <see cref="RequiredInput"/>; extra held buttons make initiation fail.</param>
    /// <returns>True after starting the raising phase; false when the chord or the speed, health, reserve, or ammunition preconditions reject activation.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="samus"/> is null.</exception>
    /// <exception cref="InvalidDataException">The Crystal Flash pose no longer has special movement.</exception>
    public bool TryBegin(
        SamusState samus,
        GameplayAction controllerInput)
    {
        Ensure.NotNull(samus);

        // The held actions must be exactly the Crystal Flash's: any other held action, Jump, Dash, item selection or
        // another direction included, keeps it from starting.
        if (controllerInput != RequiredInput)
            return false;

        // The remaining preconditions are literal comparisons. Reserve energy must
        // be empty even when the reserve mode is manual; possession of a reserve tank alone
        // does not matter. Each current ammo count, not its maximum, must supply the flash's ammunition.
        if (samus.Kinematics.YSpeed.Whole != 0 || samus.Kinematics.YSpeed.Fraction != 0 ||
            samus.Health >= CrystalFlashDefinitions.AdmittingEnergyBelow || samus.ReserveEnergy != 0 ||
            samus.Missiles < CrystalFlashDefinitions.AmmunitionPerKind ||
            samus.SuperMissiles < CrystalFlashDefinitions.AmmunitionPerKind ||
            samus.PowerBombs < CrystalFlashDefinitions.AmmunitionPerKind)
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
        if (SamusPoseMetadata.ReadMovementType(samus) != SamusMovementType.Special)
            throw new InvalidDataException("The Crystal Flash poses no longer have the special movement type.");
        SamusPresentation.InitializeAnimation(samus);
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
        RaiseTimer = CrystalFlashDefinitions.RaiseUpdates - 1;
        DrainingAmmo = CrystalFlashAmmoKind.Missiles;
        _ammoCountdown.StartAmmoCountdown(CrystalFlashDefinitions.AmmunitionPerKind);
        RaisedHeight = 0;
        _colouring.Take(SamusSpecialColouringKind.CrystalFlash);
        SamusGlowPlay.Restart(BodyPulse, SamusGlowSegment.CrystalFlashBody);
        SamusGlowPlay.Restart(BubbleTurn, SamusGlowSegment.CrystalFlashBubble);
        BeamShadeRestoreRequested = false;
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
    /// <returns>A value snapshot of the entry phase and retained bubble-effect request, even when this update changes phase.</returns>
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
                // including its seven no-drain frames and the final phase-changing update.
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

    /// <summary>Raises Samus two whole pixels for ten updates, then starts the drain phase and requests the bubble effect.</summary>
    private void StepRaising(SamusState samus)
    {
        // The raise changes only the whole pixels. The subposition survives.
        samus.Position -= new Offset16<RoomPixels>(0, CrystalFlashDefinitions.RaiseStep);

        // The count starts one below the raise's updates and falls by one each update; the raise's last update takes
        // it below zero and starts the main bubble phase after completing its rise.
        if (--RaiseTimer >= 0)
            return;

        // The delay animation is already on its raise loop, but the raise forces
        // the glow's first frame and its hold so the glow begins on this precise transition frame.
        SamusPresentation.SetAnimationFrameFromMovementOwner(samus, SamusAnimationFrames.CrystalFlashGlow, timer: CrystalFlashDefinitions.PhasePictureHold);
        RaisedHeight = samus.Position.Y.Whole;
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

        SamusEnergyRestoration.Restore(samus, CrystalFlashDefinitions.EnergyPerAmmunition);
        CountdownStep ammoTimer = _ammoCountdown.CountDownAmmo();
        bool timerExpired = ammoTimer.IsZeroOrNegative;
        if (!timerExpired)
            return;

        if (DrainingAmmo != CrystalFlashAmmoKind.PowerBombs)
        {
            _ammoCountdown.StartAmmoCountdown(CrystalFlashDefinitions.AmmunitionPerKind);
            DrainingAmmo = DrainingAmmo == CrystalFlashAmmoKind.Missiles
                ? CrystalFlashAmmoKind.SuperMissiles
                : CrystalFlashAmmoKind.PowerBombs;
            return;
        }

        // The ammunition drain switches to finish art directly; it does not wait for the main
        // four-frame animation loop to reach a cue.
        Phase = CrystalFlashPhase.Finishing;
        SamusPresentation.SetAnimationFrameFromMovementOwner(samus, SamusAnimationFrames.CrystalFlashFinish, timer: CrystalFlashDefinitions.PhasePictureHold);
    }

    /// <summary>Waits for standing pose after the finish art, then releases input locks and requests suit shade restoration.</summary>
    private void StepFinish(SamusState samus)
    {
        // The step is usually entered with equality because the raised height was captured at the
        // end of the rise. Anything else that moved Samus up or down makes her whole vertical position step down one
        // pixel each update it differs from the raised height.
        if (samus.Position.Y.Whole != RaisedHeight)
            samus.Position += new Offset16<RoomPixels>(0, 1);

        // The pose's animation ends in a transition to the facing standing pose, which occurs after movement.
        // Therefore cleanup naturally happens on the following movement phase, once standing movement is
        // observable here, and that frame performs no ordinary standing movement.
        if (SamusPoseMetadata.ReadMovementType(samus) != SamusMovementType.Standing)
            return;

        BeamShadeRestoreRequested = true;
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
    /// The beams' colours are split deliberately: the body's shade pulses through ten steps of ten updates each, while the
    /// bubble's shade, laid over the beams' upper colours, rotates through six steps every five updates. The import
    /// pre-colours Samus in every step of both (#1512); completion shows the beam shade of the currently equipped beam
    /// combination again.
    /// </remarks>
    public bool UpdateColouring(SamusColours colours, SamusState samus)
    {
        Ensure.NotNull(colours);
        Ensure.NotNull(samus);
        if (!_colouring.Holds(SamusSpecialColouringKind.CrystalFlash))
            return false;

        if (BeamShadeRestoreRequested)
        {
            // Completion reloads the equipped combination's beam shade with the loader equipping a beam uses, so a Spazer
            // and Plasma combination shows the same colours as everywhere else (#1506, #1736).
            SuperMetroid.Core.Samus.Projectiles.SamusProjectileSystem.ShowBeamShade(colours, samus.EquippedBeams);

            _colouring.Release(SamusSpecialColouringKind.CrystalFlash);
            BeamShadeRestoreRequested = false;
            return true;
        }

        // Each runs on its own count; on an update that reaches the next shade of both, the bubble's is laid first.
        if (BubbleTurn.StepToNewFrame() is { } bubble)
            colours.LayOverBeams(bubble);
        if (BodyPulse.StepToNewFrame() is { } body)
            colours.LayOverBeams(body);

        // Every active update reports the shade handled, including frames on which neither timer
        // expires. The outer colouring step must therefore suppress its normal copy.
        return true;
    }
}
