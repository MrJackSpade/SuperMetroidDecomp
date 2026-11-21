using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Colours;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// Crystal Flash's three phases of moving Samus (<see cref="SamusMovementOwner.CrystalFlash"/>): raising, the main
/// bubble phase, and the ending.
/// </summary>
/// <remarks>
/// Crystal Flash starts only when the power-bomb explosion centred on Samus finishes; once it starts, it takes both her
/// pose input and her movement from her pose, and <see cref="Phase"/> holds them, so
/// <see cref="SamusPoseId.CrystalFlashRightPose"/> and <see cref="SamusPoseId.CrystalFlashLeftPose"/> move only as
/// Crystal Flash moves them.
/// </remarks>
public sealed partial class SamusCrystalFlashState
{
    /// <summary>The exact held actions that start Crystal Flash: Down, both aims and Shoot, and nothing else.</summary>
    internal const GameplayAction RequiredInput =
        GameplayAction.Down | GameplayAction.AimDown | GameplayAction.AimUp | GameplayAction.Shoot;

    /// <summary>The phase in which Crystal Flash moves Samus, or none once another owner has taken her movement.</summary>
    public CrystalFlashPhase Phase { get; private set; }

    /// <summary>
    /// The rise's updates left, the one it finishes on included: <see cref="CrystalFlashDefinitions.RaiseUpdates"/> when
    /// it starts, one for each step (<see cref="CrystalFlashDefinitions.RaiseStep"/>).
    /// </summary>
    public UpdateCountdown RaiseUpdatesLeft { get; private set; } = UpdateCountdown.Ended();

    /// <summary>
    /// The ammunition the drain is consuming: missiles at activation, then super missiles and power bombs as each
    /// kind's run expires. No other owner shares this selector.
    /// </summary>
    public CrystalFlashAmmoKind DrainingAmmo { get; private set; }

    /// <summary>
    /// The updates until the next unit of ammunition drains, reloaded between the ammunition families, on the
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
    internal void RelinquishSuitColouring() => Colouring = SpecialColouringStage.Showing;

    /// <summary>
    /// The room line Samus's whole Y reached at the end of the complete rise: the finishing phase walks her down a pixel
    /// at a time while her whole Y differs from it.
    /// </summary>
    internal int RaisedHeight { get; set; }

    /// <summary>
    /// The body's colour pulse (#2387), its shades played over and over: activation
    /// starts it, and its first shade shows on the flash's first colouring update.
    /// </summary>
    internal AnimationPlayer<SamusGlowSegment, PictureShade, NoAnimationCue> BodyPulse { get; } =
        SamusGlowPlay.Starting(SamusGlowSegment.Of(SamusGlow.CrystalFlashBody));

    /// <summary>
    /// The bubble's colours turning (#2387), its shades played over and over on their
    /// own count beside the body's: activation starts it, and its first shade shows on the flash's first colouring update.
    /// </summary>
    internal AnimationPlayer<SamusGlowSegment, PictureShade, NoAnimationCue> BubbleTurn { get; } =
        SamusGlowPlay.Starting(SamusGlowSegment.Of(SamusGlow.CrystalFlashBubble));

    /// <summary>
    /// Where the flash's colouring stands (#2562): showing its pulse and bubble, or ended by the flash's finish, its next
    /// colouring update showing the equipped beams' colours again and releasing the shade.
    /// </summary>
    internal SpecialColouringStage Colouring { get; set; }

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

        // Samus must have no reserve energy, whatever the reserve mode and whether she has a reserve tank, and at least
        // the flash's ammunition of each kind in hand.
        if (samus.Kinematics.VerticalVelocity.IsMoving ||
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

        // Crystal Flash takes Samus's movement from the shinespark.
        samus.Shinespark.RelinquishToCrystalFlash();
        // Reserve Mode leaves X-Ray's cone effect alive but Crystal Flash still takes over
        // Samus's special suit colouring. Keep the greyout; release only X-ray's suit
        // colouring so Crystal Flash's colouring alone colours Samus.
        samus.Xray.RelinquishSuitColouring();
        // Crystal Flash can start while the Baby Metroid drains Samus or while Draygon holds her; it takes her movement
        // from both, and when it ends the drain's stun is over.
        SamusMovementHandOver.To(samus, SamusMovementOwner.CrystalFlash);
        samus.InputSnapshot.CrystalFlashPoseInputLocked = true;
        RaiseUpdatesLeft = UpdateCountdown.For(CrystalFlashDefinitions.RaiseUpdates);
        DrainingAmmo = CrystalFlashAmmoKind.Missiles;
        _ammoCountdown.StartAmmoCountdown(CrystalFlashDefinitions.AmmunitionPerKind);
        RaisedHeight = 0;
        _colouring.Take(SamusSpecialColouringKind.CrystalFlash);
        SamusGlowPlay.Restart(BodyPulse, SamusGlowSegment.Of(SamusGlow.CrystalFlashBody));
        SamusGlowPlay.Restart(BubbleTurn, SamusGlowSegment.Of(SamusGlow.CrystalFlashBubble));
        Colouring = SpecialColouringStage.Showing;
        // Activation discards existing hit immunity; Crystal Flash is not a damage shield.
        EndHitImmunityAndKnockback(samus);
        samus.Knockback = Movement.SamusKnockback.None;
        Phase = CrystalFlashPhase.Raising;
        return true;
    }

    /// <summary>Runs one update of Crystal Flash's current phase in Samus's movement phase.</summary>
    /// <param name="samus">Live actor receiving movement, ammunition/energy, animation, and phase-completion writes.</param>
    /// <param name="frameCounter">The frame counter; ammunition drains only at its cadence (<see cref="SamusCadenceDefinitions.CrystalFlashDrain"/>).</param>
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
                EndHitImmunity(samus);
                break;

            case CrystalFlashPhase.Finishing:
                StepFinish(samus);
                break;

            case CrystalFlashPhase.Inactive:
            default:
                throw new InvalidOperationException($"Unknown Crystal Flash phase {Phase}.");
        }

        return new CrystalFlashMovementResult(
            BubbleStarted: phaseAtStart == CrystalFlashPhase.Raising && Phase == CrystalFlashPhase.DrainingAmmo);
    }

    /// <summary>Raises Samus by <see cref="CrystalFlashDefinitions.RaiseStep"/> for <see cref="CrystalFlashDefinitions.RaiseUpdates"/>, then starts the drain phase and requests the bubble effect.</summary>
    private void StepRaising(SamusState samus)
    {
        // The raise changes only the whole pixels; where Samus lies within a pixel stays.
        samus.Position -= new Offset16<RoomPixels>(0, CrystalFlashDefinitions.RaiseStep);

        // The rise's last update starts the main bubble phase after completing its rise.
        if (!RaiseUpdatesLeft.StepEnds())
            return;

        // The delay animation is already on its raise loop, but the raise forces
        // the glow's first frame and its hold so the glow begins on this precise transition frame.
        SamusPresentation.SetAnimationFrameFromMovementOwner(samus, SamusAnimationFrames.CrystalFlashGlow, timer: CrystalFlashDefinitions.PhasePictureHold);
        RaisedHeight = samus.Position.Y.Whole;
        Phase = CrystalFlashPhase.DrainingAmmo;
        // The raise becomes the main phase with one essential-priority request that silences the room library.
        samus.AudioRequests.QueueSound(RoomSoundStop.Any, SoundRequestPriority.Essential);
        EndHitImmunityAndKnockback(samus);
    }

    /// <summary>
    /// Consumes missiles, supers, then power bombs at the drain's cadence and restores <see cref="CrystalFlashDefinitions.EnergyPerAmmunition"/> for each one spent.
    /// </summary>
    private void StepAmmoDrain(SamusState samus, int frameCounter)
    {
        // All three drains do nothing off their cadence's place in the frame counter;
        // Crystal Flash keeps no cadence of its own.
        if (!SamusCadenceDefinitions.CrystalFlashDrain.IsDue(frameCounter))
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
            samus.Position += CrystalFlashDefinitions.SettleStep;

        // The pose's animation ends in a transition to the facing standing pose, which occurs after movement.
        // Therefore cleanup naturally happens on the following movement phase, once standing movement is
        // observable here, and that frame performs no ordinary standing movement.
        if (SamusPoseMetadata.ReadMovementType(samus) != SamusMovementType.Standing)
            return;

        Colouring = SpecialColouringStage.Ending;
        samus.InputSnapshot.ShinesparkPoseInputLocked = false;
        samus.InputSnapshot.CrystalFlashPoseInputLocked = false;
        // Normal-input completion clears the same hit timers as the return path.
        EndHitImmunityAndKnockback(samus);
        Phase = CrystalFlashPhase.Inactive;
    }

    /// <summary>
    /// Runs Crystal Flash's suit colouring for one update.
    /// </summary>
    /// <remarks>
    /// Her beams' colours show two shades: the body's shade pulses slowly through its steps, while the bubble's shade,
    /// over the upper part of her beam colours, rotates more quickly through its own. The import pre-colours Samus in
    /// every step of both (#1512); completion shows the beam shade of the currently equipped beam combination again.
    /// </remarks>
    public bool UpdateColouring(SamusColours colours, SamusState samus)
    {
        Ensure.NotNull(colours);
        Ensure.NotNull(samus);
        if (!_colouring.Holds(SamusSpecialColouringKind.CrystalFlash))
            return false;

        if (Colouring == SpecialColouringStage.Ending)
        {
            // Completion reloads the equipped combination's beam shade with the loader equipping a beam uses, so a Spazer
            // and Plasma combination shows the same colours as everywhere else (#1506, #1736).
            SamusProjectileSystem.ShowBeamShade(colours, samus.EquippedBeams);

            _colouring.Release(SamusSpecialColouringKind.CrystalFlash);
            Colouring = SpecialColouringStage.Showing;
            return true;
        }

        // Each runs on its own count; on an update that reaches the next shade of both, the bubble's is laid first.
        if (BubbleTurn.StepToNewFrame() is { } bubble)
            colours.LayOverBeams(bubble);
        if (BodyPulse.StepToNewFrame() is { } body)
            colours.LayOverBeams(body);

        // Crystal Flash's shade holds Samus's colours on every update it runs, whether or not either timer runs out.
        return true;
    }

    /// <summary>Ends Samus's hit immunity: her invincibility and her knockback's hold run out at once.</summary>
    private static void EndHitImmunity(SamusState samus)
    {
        samus.InvincibilityTimer = WrappingCountdown.Start(0);
        samus.KnockbackHold = UpdateCountdown.Ended();
    }

    /// <summary>Ends Samus's hit immunity (<see cref="EndHitImmunity"/>) and her knockback with it.</summary>
    private static void EndHitImmunityAndKnockback(SamusState samus)
    {
        EndHitImmunity(samus);
        samus.Knockback = samus.Knockback.Released();
    }
}
