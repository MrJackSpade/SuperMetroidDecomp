using SuperMetroid.Core.Input;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// The fixed-point horizontal speeds used by Samus's movement code.
/// </summary>
/// <remarks>
/// This class is intentionally narrower than a general-purpose desktop physics body. Every
/// property corresponds to one value of Samus's state, and every operation below preserves
/// the game's wrapping at 65536 and its comparisons. Room/enemy collision consumes
/// the resulting displacement later; it is not guessed here.
/// </remarks>
public sealed class SamusHorizontalSpeedState
{
    /// <summary>Samus's base horizontal speed, whole and fraction.</summary>
    public UnsignedSixteenSixteen BaseSpeed { get; set; }

    /// <summary>The run-button/speed-booster addition to horizontal speed.</summary>
    public UnsignedSixteenSixteen ExtraRunSpeed { get; set; }

    /// <summary>
    /// Whether Samus has running momentum. Once dash begins, releasing Dash or leaving
    /// movement type one retains the accumulated extra component until a specific cancel
    /// (standing, turning, collision, etc.) clears it.
    /// </summary>
    public bool HasRunningMomentum { get; set; }

    /// <summary>
    /// Speed Booster progress: the boost stage and the running cadence that advances it. Stages zero through four are
    /// reached by running; No-Speed-Booster Dash keeps it zero. The cadence borrows from the stage when it is counted
    /// down at zero (<see cref="TryAdvanceSpeedBoosterAnimationStage"/>).
    /// </summary>
    public SamusSpeedBoostCounter SpeedBoostCounter { get; set; }

    /// <summary>
    /// Whether the boost is at its full stage: Speed Booster blocks, boost contact damage, the echoes, the full-boost
    /// palette and lava cancellation test it.
    /// </summary>
    public bool IsActivelySpeedBoosting => SpeedBoostCounter.IsAtFullStage;

    /// <summary>
    /// The step of the Speed Booster or Screw Attack colour cycle shown next, counted from the shown suit's first
    /// entry: 0 through 5. The two cycles share it, so Speed Booster can inherit a Screw Attack step past its own
    /// list's end; the Screw Attack cycle wraps to zero after its sixth step and the Speed Booster cycle holds its last,
    /// so no step passes five, and the setter rejects any other. Reset when Speed Booster momentum begins or cancels.
    /// </summary>
    public int CyclePaletteStep
    {
        get;
        set => field = Ensure.BetweenInclusive(0, SamusPaletteDefinitions.FullBodyCycles.ScrewAttackSteps - 1, value);
    }

    /// <summary>Seeded to one on the first boosted running frame.</summary>
    public int SpecialPaletteTimer { get; set; }

    /// <summary>
    /// The echo sound's flag: set before the echo sound's start request,
    /// consumed after boost ends, or cleared silently by quicksand: one while set, zero while clear, and the setter rejects
    /// any other value.
    /// </summary>
    public int EchoSoundFlag
    {
        get;
        set => field = Ensure.BetweenInclusive(0, 1, value);
    }

    /// <summary>Contact-damage selector published when the counter reaches stage four.</summary>
    public SamusContactDamageIndex ContactDamageIndex { get; set; }

    /// <summary>
    /// Publication of the immediate suit-palette copy performed by
    /// cancelling a speed boost. Movement owns the cancellation,
    /// while the runtime owns the colours, so this flag preserves the same-frame handoff
    /// without passing a renderer through every collision and pose step.
    /// </summary>
    public bool NormalSuitPaletteRestoreRequested { get; private set; }

    /// <summary>
    /// The speed echo cursor. Ordinary active Speed Booster echoes alternate between the first and second slots;
    /// cancellation sends them departing while the two stored bodies travel back toward Samus.
    /// </summary>
    public SpeedEchoCursor SpeedEchoIndex { get; private set; }

    /// <summary>Whether the echoes are departing toward Samus, which only cancellation starts.</summary>
    public bool SpeedEchoesDeparting => SpeedEchoIndex.IsDeparting;

    /// <summary>Where the first Speed Booster echo stands, or none while its slot holds no echo.</summary>
    public Position16<RoomPixels>? FirstSpeedEchoPosition { get; private set; }

    /// <summary>Where the second Speed Booster echo stands, or none while its slot holds no echo.</summary>
    public Position16<RoomPixels>? SecondSpeedEchoPosition { get; private set; }

    /// <summary>
    /// Whole-pixel per-frame velocity of departing echo zero; only X moves. Cancellation
    /// writes -8 while facing left or +8 otherwise.
    /// </summary>
    public Offset16<RoomPixels> FirstSpeedEchoVelocity { get; private set; }

    /// <summary>Whole-pixel per-frame velocity of departing echo one; only X moves.</summary>
    public Offset16<RoomPixels> SecondSpeedEchoVelocity { get; private set; }

    /// <summary>The total horizontal speed produced by the total-speed calculation.</summary>
    public UnsignedSixteenSixteen TotalSpeed { get; internal set; }

    /// <summary>
    /// Speed divisor, 0 through <see cref="LargestSpeedDivisor"/>: total speed is halved this many times, capped at four
    /// halvings. It doubles as the dry-room animation delay. Each Draygon goop stuck to Samus adds one, a sixth is
    /// refused, and the Baby Metroid's drain stores two, so the setter rejects any other value.
    /// </summary>
    public int SpeedDivisor
    {
        get;
        set => field = Ensure.BetweenInclusive(0, LargestSpeedDivisor, value);
    }

    /// <summary>The largest speed divisor: five Draygon goops stuck to Samus, the most the goop's attach step allows.</summary>
    public const int LargestSpeedDivisor = 5;

    /// <summary>
    /// Acceleration mode: accelerating speeds up, while turning and decelerating both take the
    /// deceleration branch of the speed calculation.
    /// </summary>
    public SamusAccelerationMode AccelerationMode { get; set; }

    /// <summary>
    /// Optional deceleration multiplier in 256ths, 0 through <see cref="MultipliedDeceleration.LargestMultiplier"/>, the
    /// multipliers the multiplied-deceleration rule scales by (#1627). Zero selects the table's plain deceleration; nonzero
    /// scales it by <see cref="MultipliedDeceleration"/>.
    /// </summary>
    public int DecelerationMultiplier
    {
        get;
        set => field = Ensure.BetweenInclusive(0, MultipliedDeceleration.LargestMultiplier, value);
    }

    /// <summary>
    /// The table the current inside-block/environment reaction selected. Normal air, water, and
    /// lava/acid select three complete tables before movement type is added.
    /// </summary>
    public SamusLiquidMedium ActiveSpeedTable { get; internal set; } = SamusLiquidMedium.Air;

    /// <summary>
    /// Reproduces the ordinary-air assignment made by the block reaction before movement.
    /// </summary>
    public void SelectNormalAirSpeedTable() => ActiveSpeedTable = SamusLiquidMedium.Air;

    /// <summary>
    /// Selects the horizontal speed table for the environment.
    /// The selected table's records are indexed later by movement type.
    /// </summary>
    public void SelectEnvironmentSpeedTable(SamusLiquidMedium liquidMedium)
    {
        _ = SamusHorizontalMotion.FirstRow(liquidMedium);
        ActiveSpeedTable = liquidMedium;
    }

    /// <summary>
    /// Runs the dry-air portion of the extra run speed.
    /// Both routes accelerate by hexadecimal <c>0.1000</c>. Ordinary Dash caps at
    /// <c>2.0000</c>; equipped Speed Booster caps at <c>7.0000</c> and initializes its
    /// counter through the boost stage table. Their shared momentum flag survives release and jumps.
    /// </summary>
    [IndependentFlag("Speed Booster equipment and liquid impedance are independent facts about Samus.")]
    public void HandleExtraRunSpeed(
        SamusMovementType movementType,
        ControllerButton controllerInput,
        bool speedBoosterEquipped,
        bool liquidImpeded = false)
    {
        const ControllerButton dashButton = ControllerButton.B;
        // Diverts a non-Gravity submerged body to the same no-acceleration
        // branch as releasing Dash. Existing momentum retains its numeric extra component;
        // without momentum, both values are cleared. This is not a multiplier or hard reset.
        bool activelyDashing = !liquidImpeded &&
            movementType == SamusMovementType.Running &&
            controllerInput.HasAny(dashButton);
        if (!activelyDashing)
        {
            // Clears the extra pair only before momentum has been established.
            // A true flag carries the pair through airborne movement and a released button.
            if (!HasRunningMomentum)
            {
                ExtraRunSpeed = default;
            }
            // The common epilogue also runs when Dash is released,
            // movement becomes airborne, or liquid suppresses acceleration.
            PublishBoostContactDamage();
            return;
        }

        if (speedBoosterEquipped)
        {
            if (!HasRunningMomentum)
            {
                // Starts the running momentum and the boost's colour timer and step.
                // Palette writes themselves remain renderer work, but their
                // frame/timer state is movement-visible and therefore retained here.
                HasRunningMomentum = true;
                SpecialPaletteTimer = 1;
                CyclePaletteStep = 0;
                SpeedBoostCounter = new SamusSpeedBoostCounter(stage: 0, cadence: SamusRunningCadences.CountdownAfterAdvance(0));
            }

            if (SamusSpeedMagnitude.ReadsAtCap(ExtraRunSpeed, BoostedExtraRunSpeedCap))
            {
                ExtraRunSpeed = BoostedExtraRunSpeedCap;
                PublishBoostContactDamage();
                return;
            }

            AddExtraRunAcceleration();
            PublishBoostContactDamage();
            return;
        }

        if (!HasRunningMomentum)
        {
            HasRunningMomentum = true;
            SpeedBoostCounter = default;
        }

        // The cap test reads the whole pixels and the fraction each on their own (SamusSpeedMagnitude.ReadsAtCap). The
        // cap has no fraction, so a normal progression reaches exactly two pixels and is held there on the following
        // call before another sixteenth of a pixel can be added.
        if (SamusSpeedMagnitude.ReadsAtCap(ExtraRunSpeed, UnboostedExtraRunSpeedCap))
        {
            ExtraRunSpeed = UnboostedExtraRunSpeedCap;
            PublishBoostContactDamage();
            return;
        }

        AddExtraRunAcceleration();
        PublishBoostContactDamage();
    }

    /// <summary>The extra run speed the Speed Booster's dash is held at: seven pixels an update.</summary>
    private static readonly UnsignedSixteenSixteen BoostedExtraRunSpeedCap = new(7, 0);

    /// <summary>The extra run speed a dash without the Speed Booster is held at: two pixels an update.</summary>
    private static readonly UnsignedSixteenSixteen UnboostedExtraRunSpeedCap = new(2, 0);

    /// <summary>
    /// Takes the cue that closes the run for an equipped Speed Booster when the running
    /// animation reaches it. True means this call took the cue and restarted the run at
    /// frame zero on the new boost stage's cadence.
    /// </summary>
    /// <param name="unauthoredBoostCadence">The session's unauthored Speed Booster cadence, which a full sound queue selects.</param>
    /// <param name="movementType">Current movement category; only running takes this cue.</param>
    /// <param name="controllerInput">Current held buttons; the dash button must be held.</param>
    /// <param name="animationFrameBuffer">Frame-delay adjustment added to the selected stage's first animation delay.</param>
    /// <param name="animationFrame">Current animation frame, reset to zero only when this call takes the cue.</param>
    /// <param name="animationFrameTimer">Zero when the cue is not taken; otherwise the new first-frame delay plus the buffer, wrapped to sixteen bits.</param>
    /// <param name="requestEchoSound">
    /// Requests the echo's sound as the stage reaches four, returning what its queue did with it, which selects the
    /// restart countdown and the first frame's cadence; or none when no queue answered (a Samus outside a session), when
    /// the stage just stored selects them.
    /// </param>
    public bool TryAdvanceSpeedBoosterAnimationStage(
        UnauthoredBoostCadence unauthoredBoostCadence,
        SamusMovementType movementType,
        ControllerButton controllerInput,
        int animationFrameBuffer,
        ref int animationFrame,
        out int animationFrameTimer,
        Func<SoundQueueAdmission?> requestEchoSound)
    {
        Ensure.NotNull(requestEchoSound);
        animationFrameTimer = 0;
        const ControllerButton dashButton = ControllerButton.B;
        ControllerButton heldButtons = controllerInput;
        if (!HasRunningMomentum || movementType != SamusMovementType.Running || !heldButtons.HasAny(dashButton))
            return false;

        // A cadence countdown already at zero borrows from the stage. A stage advances only
        // when the decremented cadence countdown is zero; that test leaves the stage unchanged.
        (SamusSpeedBoostCounter decremented, bool cadenceExpired) = SpeedBoostCounter.CountDownOneRun();
        SpeedBoostCounter = decremented;
        if (!cadenceExpired)
            return false;

        int? echoSelection = null;
        if (!SpeedBoostCounter.IsAtFullStage)
        {
            SpeedBoostCounter = SpeedBoostCounter.AdvanceStage();
            if (SpeedBoostCounter.IsAtFullStage)
            {
                EchoSoundFlag = 1;
                // What the echo's queue request reports, not the stage just stored, picks
                // both the restart countdown and the first frame's cadence. Do not clamp
                // it: a full queue picks the unauthored cadence.
                if (requestEchoSound() is { } admission)
                    echoSelection = SamusRunningCadences.EchoQueueSelection(admission);
            }
        }

        int stage = echoSelection ?? SpeedBoostCounter.Stage;
        SpeedBoostCounter = SpeedBoostCounter.WithCadence(SamusRunningCadences.CountdownAfterAdvance(stage));

        SamusRunningCadence cadence = SamusRunningCadences.SpeedBoostCadence(stage);
        animationFrame = 0;
        animationFrameTimer = WrappingCountdown.Start(animationFrameBuffer)
            .AddFrames(SamusRunningCadences.Delay(unauthoredBoostCadence, cadence, frame: 0)).RemainingFrames;
        // Animation runs after movement. Stage advancement changes cadence
        // and sound, not contact damage; the next movement epilogue publishes that.
        // Publishing here enables boost damage one frame before the game.
        return true;
    }

    /// <summary>The delay, in ticks, of running frame <paramref name="frame"/> at the boost's current stage.</summary>
    /// <param name="unauthoredBoostCadence">The session's unauthored Speed Booster cadence, which a full sound queue selects.</param>
    /// <param name="frame">The running frame, zero through nine.</param>
    internal int SpeedBoosterDelay(UnauthoredBoostCadence unauthoredBoostCadence, int frame) =>
        SamusRunningCadences.Delay(unauthoredBoostCadence, SamusRunningCadences.SpeedBoostCadence(SpeedBoostCounter.Stage), frame);

    /// <summary>
    /// Runs the dry-room Speed Booster branch of the Screw Attack and
    /// speed-boosting palette.
    /// </summary>
    /// <returns>True when this call copied an authored palette into Samus's colours.</returns>
    /// <param name="colours">Samus's colours, which the copied palette replaces.</param>
    /// <param name="frame">What this update's suit colouring reads of Samus.</param>
    internal bool UpdateSpeedBoosterPalette(SamusColours colours, SpeedBoosterPaletteFrame frame)
    {
        Ensure.NotNull(colours);

        bool paletteCopied = ApplyPendingNormalSuitPaletteRestore(colours, frame.EquippedItems);
        SamusSuit suit = frame.EquippedItems.ShownSuit();

        // The stored shine's and the shinespark's suit colourings take priority. Cancellation's
        // immediate normal-colour copy above still occurs, but the ordinary colouring must not advance
        // its hidden four-frame Speed Booster cycle beneath the stored shine's or the shinespark's.
        if (frame.SuppressActiveSpeedBoosterPalette)
            return paletteCopied;

        // Tests the suit-palette Gravity bit before it asks whether
        // Samus's *bottom* boundary lies under water or lava/acid. Power and Varia Samus
        // return at once when submerged: the current colours, common
        // timer, and palette-list offset all remain untouched. Gravity Suit bypasses this
        // gate and continues into the ordinary Screw Attack / Speed Booster logic even
        // though the raw room-FX comparison still says the body is underwater. The caller
        // supplies the raw boundary result so this state owner does not invent a second,
        // subtly different reading of the room's FX values.
        bool gravitySuitEquipped = frame.EquippedItems.HasAny(SamusEquipmentFlags.GravitySuit);
        if (!gravitySuitEquipped && frame.BottomBoundarySubmerged)
            return paletteCopied;

        bool screwAttackEquipped = frame.EquippedItems.HasAny(SamusEquipmentFlags.ScrewAttack);
        if (frame.MovementType == SamusMovementType.SpinJumping && screwAttackEquipped)
        {
            if (frame.AnimationFrame == 0)
            {
                // Restarts only the colour cycle's step. Frame zero retains
                // the already loaded normal suit colors; it does not emit a Screw palette.
                CyclePaletteStep = 0;
                return paletteCopied;
            }

            if (frame.AnimationFrame >= 27)
            {
                // From frame 27 onward Samus shows her normal suit colours.
                // Frames 1..26 cycle below. Samus's colours are what the next shown frame displays.
                SamusNormalSuitPalette.Load(colours, frame.EquippedItems);
                return true;
            }

            CopyAndAdvanceScrewAttackPalette(colours, suit);
            return true;
        }

        if (frame.MovementType == SamusMovementType.WallJumping)
        {
            if (!screwAttackEquipped)
                return paletteCopied;

            if (frame.AnimationFrame < 3)
            {
                // The wall-jump animation keeps the normal suit colours on frames 0..2;
                // its Screw frames begin on frame three and share the same six colour steps.
                CyclePaletteStep = 0;
                return paletteCopied;
            }

            CopyAndAdvanceScrewAttackPalette(colours, suit);
            return true;
        }

        if (!IsActivelySpeedBoosting)
            return paletteCopied;

        // Zero and a count run past zero both reload four, though ordinary execution arrives with timer one.
        if (--SpecialPaletteTimer > 0)
            return paletteCopied;

        SpecialPaletteTimer = 4;

        SpeedBoosterPaletteSource source = SamusFullBodyCyclePalettes.ReadActiveSpeedBoosterPalette(
            suit, CyclePaletteStep);
        SamusSpeedBoosterPalette.Apply(colours, source);

        // The clamp is AFTER the copy. Screw Attack can leave step four or five in
        // this shared step; retain that one bounded overrun before pinning the last step.
        CyclePaletteStep = CyclePaletteStep >= SamusPaletteDefinitions.FullBodyCycles.SpeedBoosterLastStep
            ? SamusPaletteDefinitions.FullBodyCycles.SpeedBoosterLastStep
            : CyclePaletteStep + 1;
        return true;
    }

    /// <summary>
    /// Defers restoring Samus's normal suit colours until just before this update's suit colourings run.
    /// The game's pose initialization performs this when leaving a Screw Attack spin family.
    /// </summary>
    public void RequestNormalSuitPaletteRestore() => NormalSuitPaletteRestoreRequested = true;

    /// <summary>
    /// Publishes the immediate suit copies requested by momentum cancellation and
    /// pose initialization. These precede charge and special-palette handling, which may
    /// replace their colors in the same frame; they must not erase a later charge flash.
    /// </summary>
    public bool ApplyPendingNormalSuitPaletteRestore(
        SamusColours colours, SuperMetroid.Core.Progression.SamusEquipmentFlags equippedItems)
    {
        Ensure.NotNull(colours);
        if (!NormalSuitPaletteRestoreRequested)
            return false;

        SamusNormalSuitPalette.Load(colours, equippedItems);
        NormalSuitPaletteRestoreRequested = false;
        return true;
    }

    /// <summary>Copies one of the six authored Screw Attack palettes and wraps its offset.</summary>
    private void CopyAndAdvanceScrewAttackPalette(
        SamusColours colours,
        SamusSuit suit)
    {
        colours.ShowSuit(SamusShades.FullBodyCycle(SamusFullBodyCyclePalettes.ReadScrewAttackPalette(
            suit, CyclePaletteStep)));

        // Steps 0 through 5 form the six-frame cycle. The comparison uses the current
        // step, so the last wraps to zero only after its palette has been copied.
        CyclePaletteStep = CyclePaletteStep >= SamusPaletteDefinitions.FullBodyCycles.ScrewAttackSteps - 1
            ? 0
            : CyclePaletteStep + 1;
    }

    /// <summary>
    /// Captures the speed-echo position for ordinary active boost.
    /// The gameplay clock, not the frame counter, selects the four-frame sampling phase.
    /// </summary>
    /// <param name="gameTimeFrames">The gameplay clock's frames within its second, zero or more; only its four-frame phase is read and nothing stores it.</param>
    /// <param name="position">Samus's whole-pixel room position sampled into the next echo slot.</param>
    public bool CaptureSpeedEchoPosition(
        int gameTimeFrames,
        Position16<RoomPixels> position)
    {
        Ensure.AtLeastZero(gameTimeFrames);
        if (!IsActivelySpeedBoosting ||
            SpeedEchoesDeparting ||
            !SamusCadenceDefinitions.SpeedEchoCaptureEveryFourthUpdate.IsDue(gameTimeFrames))
        {
            return false;
        }

        if (SpeedEchoIndex.CapturesFirstSlot)
            FirstSpeedEchoPosition = position;
        else
            SecondSpeedEchoPosition = position;

        SpeedEchoIndex = SpeedEchoIndex.AfterCapture();
        return true;
    }

    /// <summary>
    /// Removes both echoes, stops their departure and points the next capture at the first slot: a shinespark's
    /// directional launch, its crash's start and its crash's end do this, and so does a room load.
    /// </summary>
    public void ClearSpeedEchoes()
    {
        SpeedEchoIndex = SpeedEchoCursor.FirstSlot;
        FirstSpeedEchoVelocity = Offset16<RoomPixels>.Zero;
        SecondSpeedEchoVelocity = Offset16<RoomPixels>.Zero;
        FirstSpeedEchoPosition = null;
        SecondSpeedEchoPosition = null;
    }

    /// <summary>Adds the 16.16 extra-run increment while retaining separate whole and fractional parts.</summary>
    private void AddExtraRunAcceleration() => ExtraRunSpeed += SamusMovementDefinitions.HorizontalMotion.ExtraRunAcceleration;

    /// <summary>Enables Speed Booster contact damage only at the counter's fully charged stage.</summary>
    private void PublishBoostContactDamage()
    {
        if (IsActivelySpeedBoosting)
            ContactDamageIndex = SamusContactDamageIndex.SpeedBooster;
    }

    /// <summary>
    /// Cancels the speed boost, including the transition
    /// from alternating captured positions into the echoes' departure.
    /// </summary>
    /// <param name="facing">
    /// Current pose-definition direction. The game treats exactly left (four) as facing left;
    /// every other direction uses the right-facing +8 echo velocity branch.
    /// </param>
    public void CancelRunningMomentum(SamusFacingDirection facing)
    {
        if (HasRunningMomentum)
        {
            HasRunningMomentum = false;
            SpeedBoostCounter = default;
            CyclePaletteStep = 0;
            SpecialPaletteTimer = 0;
            NormalSuitPaletteRestoreRequested = true;
        }

        // This block is intentionally outside the momentum test. Every caller can
        // enter echo-departure mode even when the momentum flag was already clear. Repeated calls while
        // the echoes are departing must *not* reset the velocities or restart the two bodies.
        if (!SpeedEchoesDeparting)
        {
            SpeedEchoIndex = SpeedEchoCursor.Departing;
            Offset16<RoomPixels> velocity = new(facing == SamusFacingDirection.Left ? -8 : 8, 0);
            FirstSpeedEchoVelocity = velocity;
            SecondSpeedEchoVelocity = velocity;
        }
    }

    /// <summary>
    /// Reconciles the speed/echo portion of the unpause equipment check after the equipment menu.
    /// Unlike ordinary momentum cancellation, unequipping Speed Booster erases
    /// the echoes outright and leaves the numeric run-speed pair until the next
    /// movement update observes the cleared momentum flag.
    /// </summary>
    public void ReconcilePauseSpeedBoosterState(bool speedBoosterEquipped)
    {
        if (speedBoosterEquipped)
        {
            if (HasRunningMomentum && SpeedBoostCounter == default)
            {
                SpecialPaletteTimer = 0;
                CyclePaletteStep = 0;
                SpeedBoostCounter = new SamusSpeedBoostCounter(stage: 0, cadence: SamusRunningCadences.CountdownAfterAdvance(0));
            }
            return;
        }

        HasRunningMomentum = false;
        SpecialPaletteTimer = 0;
        CyclePaletteStep = 0;
        SpeedBoostCounter = default;
        SpeedEchoIndex = SpeedEchoCursor.FirstSlot;
        FirstSpeedEchoVelocity = SecondSpeedEchoVelocity = Offset16<RoomPixels>.Zero;
        FirstSpeedEchoPosition = SecondSpeedEchoPosition = null;
    }

    /// <summary>
    /// Performs the complete horizontal-momentum teardown shared by collision, standing,
    /// posture, aerial, and Morph-Ball handlers. The seam first cancels Speed Booster
    /// bookkeeping/echoes, then clears the extra run speed, the base speed and the
    /// acceleration mode; keeping that ordering here prevents five copies from diverging.
    /// </summary>
    public void ClearHorizontalMomentum(SamusFacingDirection facingDirection)
    {
        CancelRunningMomentum(facingDirection);
        ClearHorizontalVelocity();
    }

    /// <summary>
    /// Clears the five horizontal velocity/acceleration values without canceling
    /// boost bookkeeping. Crouching's cleanup performs only these stores;
    /// its angle-held input route can therefore preserve a temporary Blue Suit.
    /// </summary>
    public void ClearHorizontalVelocity()
    {
        ExtraRunSpeed = default;
        BaseSpeed = default;
        AccelerationMode = SamusAccelerationMode.Accelerating;
    }

    /// <summary>Stops Samus on an input fallback: stops acceleration and cancels boost, without clearing velocity.</summary>
    public void ApplyStoppedInputFallback(SamusFacingDirection facingDirection)
    {
        AccelerationMode = SamusAccelerationMode.Accelerating;
        CancelRunningMomentum(facingDirection);
    }

    /// <summary>
    /// Executes running / normal jumping / airborne ball fallback's deceleration or stop, as the input phase selected it.
    /// Deceleration rechecks speed after movement before folding extra momentum.
    /// </summary>
    /// <param name="selectedDeceleration">Whether the input phase selected deceleration rather than a stop.</param>
    /// <param name="facingDirection">The direction Samus faces, whose running momentum is cancelled.</param>
    public void ApplyDeceleratingInputFallback(bool selectedDeceleration, SamusFacingDirection facingDirection)
    {
        if (selectedDeceleration && HasBaseSpeed)
        {
            BaseSpeed += ExtraRunSpeed;
            ExtraRunSpeed = default;
            AccelerationMode = SamusAccelerationMode.Decelerating;
        }
        else
            AccelerationMode = SamusAccelerationMode.Accelerating;
        CancelRunningMomentum(facingDirection);
    }

    /// <summary>
    /// Advances one cancellation echo while the echoes are drawn.
    /// </summary>
    /// <remarks>
    /// This side effect really belongs to drawing in the game: Y approaches the live
    /// Samus center by two pixels, X advances by the signed eight-pixel velocity, and the
    /// echo is gone on the exact frame it crosses Samus. Returning false means the
    /// slot is empty or crossed this frame and therefore must not be drawn.
    /// </remarks>
    public bool AdvanceDepartingSpeedEcho(
        int slot,
        Position16<RoomPixels> samusPosition)
    {
        if (!SpeedEchoesDeparting)
            return false;
        if (slot is < 0 or > 1)
            throw new ArgumentOutOfRangeException(nameof(slot));

        Position16<RoomPixels>? stored = slot == 0
            ? FirstSpeedEchoPosition
            : SecondSpeedEchoPosition;
        if (stored is not { } echo)
            return false;

        int xPosition = echo.X;
        int yPosition = echo.Y;
        if (yPosition != samusPosition.Y)
        {
            yPosition = CoordinateAxis.Difference(yPosition, samusPosition.Y) < 0
                ? CoordinateAxis.Wrap(yPosition + 2)
                : CoordinateAxis.Wrap(yPosition - 2);
        }

        Offset16<RoomPixels> velocity = slot == 0
            ? FirstSpeedEchoVelocity
            : SecondSpeedEchoVelocity;
        xPosition = CoordinateAxis.Wrap(xPosition + velocity.X);
        bool crossedSamus = velocity.X < 0
            ? CoordinateAxis.Difference(xPosition, samusPosition.X) < 0
            : CoordinateAxis.Difference(xPosition, samusPosition.X) >= 0;
        Position16<RoomPixels>? advanced = crossedSamus ? null : Position16<RoomPixels>.Of(xPosition, yPosition);

        if (slot == 0)
            FirstSpeedEchoPosition = advanced;
        else
            SecondSpeedEchoPosition = advanced;

        return advanced is not null;
    }

    /// <summary>
    /// Clears the departure state after both departure slots have been considered.
    /// Once neither echo remains, the cursor returns to the first slot.
    /// </summary>
    public void FinishDepartingSpeedEchoFrame()
    {
        if (SpeedEchoesDeparting &&
            FirstSpeedEchoPosition is null &&
            SecondSpeedEchoPosition is null)
        {
            SpeedEchoIndex = SpeedEchoCursor.FirstSlot;
        }
    }

    /// <summary>
    /// Resolves the row of the entry the horizontal speed-table selection picks
    /// among the three adjacent tables, assuming the already-modeled inside-block
    /// reaction chose the table.
    /// </summary>
    public int ResolveEntryRow(SamusMovementType movementType) =>
        SamusHorizontalMotion.Row(ActiveSpeedTable, movementType);

    /// <summary>
    /// Resolves authored speed entries. Each speed keeps its whole pixels and its fraction exactly as
    /// authored instead of becoming a floating-point value.
    /// </summary>
    public SpeedTableEntry ReadEntry(SamusMovementType movementType) => ReadEntryAtRow(ResolveEntryRow(movementType));

    /// <summary>Resolves an authored speed record by row; a row past the tables reads beyond the authored records.</summary>
    internal static SpeedTableEntry ReadEntryAtRow(int row) =>
        SamusHorizontalMotion.TryResolveRow(row, out SpeedTableEntry compiled)
            ? compiled
            : throw new InvalidDataException(
                $"Samus horizontal speed row {row} is past the {SamusHorizontalMotionDefinitions.RowCount} authored mechanics records.");

    /// <summary>
    /// Calculates the horizontal base speed and returns its unsigned
    /// 16.16 result. It updates Samus's base speed exactly once.
    /// </summary>
    public UnsignedSixteenSixteen CalculateBaseSpeed(SamusMovementType movementType)
    {
        SpeedTableEntry entry = ReadEntry(movementType);
        return CalculateBaseSpeed(entry);
    }

    /// <summary>
    /// Executes the speed calculation against a standalone record. Bomb jumps use their own standalone record
    /// instead of the movement-type-indexed normal table.
    /// </summary>
    public UnsignedSixteenSixteen CalculateBaseSpeed(SamusStandaloneSpeedRecord record) =>
        CalculateBaseSpeed(SamusHorizontalMotion.Standalone(record));

    /// <summary>Applies the table's acceleration or deceleration to the whole and fraction separately and updates base speed.</summary>
    internal UnsignedSixteenSixteen CalculateBaseSpeed(SpeedTableEntry entry)
    {

        if (AccelerationMode != SamusAccelerationMode.Accelerating)
        {
            BaseSpeed -= MultipliedDeceleration.Of(entry.Deceleration, DecelerationMultiplier);

            // The test reads only the whole pixels. Once deceleration crosses
            // below zero it clears both halves and returns to acceleration mode zero.
            if (SamusSpeedMagnitude.HasPassedZero(BaseSpeed.Whole))
            {
                BaseSpeed = default;
                AccelerationMode = SamusAccelerationMode.Accelerating;
            }
        }
        else
        {
            BaseSpeed += entry.Acceleration;

            // The maximum replaces a speed that reads as past it (SamusSpeedMagnitude.ReadsPastLimit); a speed exactly
            // at its maximum is left untouched.
            UnsignedSixteenSixteen maximum = UnsignedSixteenSixteen.FromDisplacement(entry.MaximumSpeed);
            if (SamusSpeedMagnitude.ReadsPastLimit(BaseSpeed, maximum))
            {
                BaseSpeed = maximum;
            }
        }

        return BaseSpeed;
    }

    /// <summary>
    /// Calculates the horizontal base speed with deceleration disallowed.
    /// Despite the name, the turning acceleration mode still selects the
    /// turning/deceleration branch; modes zero and two both accelerate toward the table
    /// maximum. The returned reached-maximum result is important to spin
    /// jump's decision to retain horizontal motion.
    /// </summary>
    public AerialBaseSpeedResult CalculateBaseSpeedDecelerationDisallowed(
        SamusMovementType movementType)
    {
        SpeedTableEntry entry = ReadEntry(movementType);

        if (AccelerationMode == SamusAccelerationMode.Turning)
        {
            // This is the same multiplied deceleration the deceleration-allowed calculation uses. The distinction
            // between the two calculations is the turning test above and their reached-maximum result, not the subtract.
            BaseSpeed -= MultipliedDeceleration.Of(entry.Deceleration, DecelerationMultiplier);
            if (SamusSpeedMagnitude.HasPassedZero(BaseSpeed.Whole))
            {
                BaseSpeed = default;
                AccelerationMode = SamusAccelerationMode.Accelerating;
            }

            // No path through the deceleration-disallowed calculation reports reaching the maximum.
            return new AerialBaseSpeedResult(BaseSpeed, ReachedMaximum: false);
        }

        BaseSpeed += entry.Acceleration;

        // The same maximum test as the deceleration-allowed calculation: a speed exactly at its maximum is
        // deliberately *not* capped, and the calculation reports no cap in that case.
        UnsignedSixteenSixteen maximum = UnsignedSixteenSixteen.FromDisplacement(entry.MaximumSpeed);
        if (SamusSpeedMagnitude.ReadsPastLimit(BaseSpeed, maximum))
        {
            BaseSpeed = maximum;
            return new AerialBaseSpeedResult(BaseSpeed, ReachedMaximum: true);
        }

        return new AerialBaseSpeedResult(BaseSpeed, ReachedMaximum: false);
    }

    /// <summary>
    /// Calculates the total horizontal speed: add extra run speed, shift by
    /// min(divisor, 4), and publish the total-speed pair used by animation/collision.
    /// </summary>
    public UnsignedSixteenSixteen CalculateTotalSpeed(UnsignedSixteenSixteen baseSpeed)
    {
        TotalSpeed = (baseSpeed + ExtraRunSpeed).Halved(Math.Min(SpeedDivisor, LargestSpeedHalvings));
        return TotalSpeed;
    }

    /// <summary>The most times a speed divisor halves the total speed, however large the divisor.</summary>
    private const int LargestSpeedHalvings = 4;

    /// <summary>
    /// Calculates the non-collision portion of a rightward displacement,
    /// including the ±15-pixel whole-pixel clamp.
    /// </summary>
    public SignedSixteenSixteen CalculateRightDisplacement(UnsignedSixteenSixteen baseSpeed, SignedSixteenSixteen extraDisplacement = default) =>
        ClampDisplacement(CalculateTotalSpeed(baseSpeed).AsDisplacement() + extraDisplacement);

    /// <summary>
    /// Calculates the non-collision portion of a leftward displacement.
    /// Left is <c>extra displacement - total speed</c>, not merely
    /// the rightward result reversed.
    /// </summary>
    public SignedSixteenSixteen CalculateLeftDisplacement(UnsignedSixteenSixteen baseSpeed, SignedSixteenSixteen extraDisplacement = default) =>
        ClampDisplacement(extraDisplacement - CalculateTotalSpeed(baseSpeed).AsDisplacement());

    /// <summary>Whether Samus has any base speed: either its whole pixels or its fraction is nonzero.</summary>
    public bool HasBaseSpeed => BaseSpeed != default;

    /// <summary>Clamps the whole-pixel part to plus or minus fifteen while preserving fractional displacement.</summary>
    private static SignedSixteenSixteen ClampDisplacement(SignedSixteenSixteen displacement) =>
        displacement.WithWholePixelsClamped(-LargestWholeDisplacement, LargestWholeDisplacement);

    /// <summary>The most whole pixels Samus's speed moves her in one update, either way.</summary>
    private const int LargestWholeDisplacement = 15;

}
