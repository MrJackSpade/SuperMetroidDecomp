using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Colours;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// Samus's horizontal speeds, each to a fraction of a pixel.
/// </summary>
/// <remarks>
/// Every property is one value of Samus's state, and every operation below gives the speeds the game gives them. Room/enemy
/// collision consumes
/// the resulting displacement later; it is not guessed here.
/// </remarks>
public sealed class SamusHorizontalSpeedState
{
    /// <summary>The water speeds Draygon's hold and special movement take in air, which have no speeds of their own there.</summary>
    private readonly WaterSpeedsInAir<SamusMovementType, SamusHorizontalSpeeds> _waterSpeedsInAir;

    /// <summary>How the boost restarts after its echo and how a run counted off an empty countdown takes it down a stage.</summary>
    private readonly BoostRestartAfterEcho _boostRestart;

    /// <summary>Creates Samus's horizontal speeds, at rest and moving by air's speeds.</summary>
    /// <param name="compatibility">
    /// Her compatibility services: the movements that take water's speeds in air, and how her boost restarts.
    /// </param>
    internal SamusHorizontalSpeedState(SamusCompatibility compatibility)
    {
        Ensure.NotNull(compatibility);
        _waterSpeedsInAir = compatibility.WaterSpeedsInAir;
        _boostRestart = compatibility.BoostRestart;
    }

    /// <summary>Samus's base horizontal speed, whole and fraction.</summary>
    public UnsignedSixteenSixteen BaseSpeed { get; set; }

    /// <summary>The run-button/speed-booster addition to horizontal speed.</summary>
    public UnsignedSixteenSixteen ExtraRunSpeed { get; set; }

    /// <summary>
    /// Whether Samus has running momentum. Once dash begins, releasing Dash or leaving
    /// <see cref="SamusMovementType.Running"/> movement retains the accumulated extra component until a specific cancel
    /// (standing, turning, collision, etc.) clears it.
    /// </summary>
    public bool HasRunningMomentum { get; set; }

    /// <summary>
    /// Speed Booster progress: the boost stage and the countdown of runs that advances it. Stages zero through four are
    /// reached by running; No-Speed-Booster Dash keeps it zero. A run counted off an empty countdown takes the boost down
    /// a stage (<see cref="TryAdvanceSpeedBoosterAnimationStage"/>, <see cref="BoostRestartAfterEcho"/>).
    /// </summary>
    public SamusSpeedBoostCounter SpeedBoostCounter { get; set; }

    /// <summary>
    /// Whether the boost is at its full stage: Speed Booster blocks, boost contact damage, the echoes, the full-boost
    /// shade and lava cancellation test it.
    /// </summary>
    public bool IsActivelySpeedBoosting => SpeedBoostCounter.IsAtFullStage;

    /// <summary>
    /// The Speed Booster's and the Screw Attack's glow (#2387, #2447). The two share where it stands: each goes on from where
    /// the other left it (<see cref="AnimationPlayback{TSegment}.GoingOnIn"/>), so a Speed Booster glow following a Screw
    /// Attack spin starts at the spin's shade, or rests on its own brightest when the spin stood past its four, and a spin
    /// following a boosted run starts where the run's glow stood. A change of suit goes on alike. Starting or cancelling
    /// the boost's momentum, and the first frames of a spin, start it again. The player counts the Speed Booster's hold of
    /// its shown shade: the first boosted running frame moves it on, then each shade holds for its own beat; a Screw
    /// Attack spin moves the shared place on and leaves the hold where it stood.
    /// </summary>
    private AnimationPlayer<SamusGlowSegment, PictureShade, NoAnimationCue> FullBodyGlow { get; } =
        SamusGlowPlay.Starting(FullBodyGlowStart);

    /// <summary>The glow the shared place starts in, before its first shade.</summary>
    private static SamusGlowSegment FullBodyGlowStart => SamusGlowSegment.In(SamusGlow.ScrewAttack, SamusSuit.Power);

    /// <summary>
    /// Whether the Speed Booster's echo sound is playing: set before the echo sound's start request, cleared with the
    /// echo's end sound after the boost ends, or cleared silently by quicksand.
    /// </summary>
    public bool EchoSoundPlaying { get; set; }

    /// <summary>
    /// Which attack Samus's body is when an enemy touches her; her run's fourth boost stage makes it the Speed Booster's. It
    /// is her state from one movement phase to the next: each movement phase begins with none and its movers leave the
    /// attack they make, which the next update's enemies and her terrain, hazards and shots read.
    /// </summary>
    public SamusBodyAttack BodyAttack { get; set; }

    /// <summary>Her movement phase begins with no body attack, after the update's enemies have read the last movement's.</summary>
    internal void BeginMovementWithoutBodyAttack() => BodyAttack = SamusBodyAttack.None;

    /// <summary>
    /// Whether cancelling a speed boost asked for the suit's shade back at once. Movement cancels the boost and the
    /// colour step, later in the same frame, shows the suit.
    /// </summary>
    private SuitShadeAtColourStep SuitShade { get; } = new();

    /// <summary>
    /// What the speed echoes are doing. Ordinary active Speed Booster captures alternate between the first and second
    /// echoes; cancellation sends them departing while the two stored bodies travel back toward Samus.
    /// </summary>
    private SpeedEchoPhase SpeedEchoPhase { get; set; }

    /// <summary>Whether the echoes are departing toward Samus, which only cancellation starts.</summary>
    public bool SpeedEchoesDeparting => SpeedEchoPhase == SpeedEchoPhase.Departing;

    /// <summary>The Speed Booster's two echoes, one per <see cref="SpeedEcho"/> (#2557).</summary>
    public IReadOnlyDictionary<SpeedEcho, SamusSpeedEcho> SpeedEchoes { get; } = new Dictionary<SpeedEcho, SamusSpeedEcho>
    {
        [SpeedEcho.First] = new(),
        [SpeedEcho.Second] = new(),
    };

    /// <summary>The total horizontal speed produced by the total-speed calculation.</summary>
    public UnsignedSixteenSixteen TotalSpeed { get; private set; }

    /// <summary>
    /// How slowed Samus is (#2286), 0 through <see cref="LargestSlowdownLevel"/>: total speed is halved this many times,
    /// capped at <see cref="SamusHorizontalSpeedDefinitions.LargestSpeedHalvings"/>, and the game's same level is her dry-ground animation delay. Each Draygon goop stuck to
    /// Samus adds one, one past the largest is refused, and the Baby Metroid's drain stores its own level, so the setter rejects any other value.
    /// </summary>
    public int SlowdownLevel
    {
        get;
        set => field = Ensure.BetweenInclusive(0, LargestSlowdownLevel, value);
    }

    /// <summary>The largest slowdown level: five Draygon goops stuck to Samus, the most the goop's attach step allows.</summary>
    public const int LargestSlowdownLevel = 5;

    /// <summary>
    /// Acceleration mode: accelerating speeds up, while turning and decelerating both slow her down.
    /// </summary>
    public SamusAccelerationMode AccelerationMode { get; set; }

    /// <summary>
    /// The medium whose speeds Samus moves by, as the update's inside-block reaction or movement last chose it:
    /// air, water or lava and acid. Each movement takes its speeds from it (<see cref="Speeds"/>).
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The value set is not a medium.</exception>
    private SamusLiquidMedium SpeedMedium
    {
        get;
        set => field = Enum.IsDefined(value)
            ? value
            : throw new ArgumentOutOfRangeException(nameof(value), value, "Samus moves by the speeds of air, water or lava and acid.");
    } = SamusLiquidMedium.Air;

    /// <summary>
    /// Has Samus move by air's speeds, as the inside-block reaction does before movement in ordinary air.
    /// </summary>
    public void SelectAirSpeeds() => SpeedMedium = SamusLiquidMedium.Air;

    /// <summary>Has Samus move by the speeds of <paramref name="liquidMedium"/>, the medium her movement found her in.</summary>
    /// <param name="liquidMedium">Air, water, or lava and acid.</param>
    public void SelectSpeedsFor(SamusLiquidMedium liquidMedium) => SpeedMedium = liquidMedium;

    /// <summary>
    /// Runs the dry-air portion of the extra run speed.
    /// Both routes accelerate by the same step each update. Ordinary Dash is held at its unboosted cap; equipped Speed
    /// Booster is held at its boosted cap and starts its boost counter at its first stage. Their shared running
    /// momentum survives release and jumps.
    /// </summary>
    [IndependentFlag("Speed Booster equipment and liquid impedance are independent facts about Samus.")]
    public void HandleExtraRunSpeed(
        SamusMovementType movementType,
        GameplayAction controllerInput,
        bool speedBoosterEquipped,
        bool liquidImpeded = false)
    {
        const GameplayAction dashButton = GameplayAction.Dash;
        // A submerged body without the Gravity Suit gains no Dash speed, just as when Dash is
        // released. Existing momentum retains its numeric extra component;
        // without momentum, both values are cleared. This is not a multiplier or hard reset.
        bool activelyDashing = !liquidImpeded &&
            movementType == SamusMovementType.Running &&
            controllerInput.HasAny(dashButton);
        if (!activelyDashing)
        {
            // The extra run speed is cleared only before momentum has been established.
            // Established momentum carries it through airborne movement and a released button.
            if (!HasRunningMomentum)
            {
                ExtraRunSpeed = default;
            }
            // Boost contact damage is published on every move, including when Dash is released,
            // movement becomes airborne, or liquid suppresses acceleration.
            PublishBoostContactDamage();
            return;
        }

        if (speedBoosterEquipped)
        {
            if (!HasRunningMomentum)
            {
                // Dashing with the Speed Booster starts the running momentum and the boost's colour timer and step.
                // Showing the colours remains drawing work, but their
                // frame/timer state is movement-visible and therefore retained here.
                HasRunningMomentum = true;
                StartBoost();
            }

            if (SamusSpeedMagnitude.ReadsAtCap(ExtraRunSpeed, SamusHorizontalSpeedDefinitions.BoostedExtraRunSpeedCap))
            {
                ExtraRunSpeed = SamusHorizontalSpeedDefinitions.BoostedExtraRunSpeedCap;
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
            SpeedBoostCounter = SamusSpeedBoostCounter.NotBuilding;
        }

        // The cap test reads the whole pixels and the fraction each on their own (SamusSpeedMagnitude.ReadsAtCap). The
        // cap has no fraction, so a normal progression reaches exactly the cap and is held there on the following
        // update before another step can be added.
        if (SamusSpeedMagnitude.ReadsAtCap(ExtraRunSpeed, SamusHorizontalSpeedDefinitions.UnboostedExtraRunSpeedCap))
        {
            ExtraRunSpeed = SamusHorizontalSpeedDefinitions.UnboostedExtraRunSpeedCap;
            PublishBoostContactDamage();
            return;
        }

        AddExtraRunAcceleration();
        PublishBoostContactDamage();
    }

    /// <summary>
    /// Takes the cue that closes the run for an equipped Speed Booster when the running
    /// animation reaches it. True means this update took the cue: the run restarts on its first frame, which the
    /// new boost stage's cadence times.
    /// </summary>
    /// <param name="unauthoredBoostCadence">The session's unauthored Speed Booster cadence, which a full sound queue selects.</param>
    /// <param name="movementType">Current movement category; only running takes this cue.</param>
    /// <param name="controllerInput">Current held actions; Dash must be held.</param>
    /// <param name="extraAnimationDelay">Samus's extra animation delay, added to the selected stage's first animation delay.</param>
    /// <param name="animationFrameTimer">Zero when the cue is not taken; otherwise the new first-frame delay plus the extra animation delay.</param>
    /// <param name="requestEchoSound">
    /// Requests the echo's sound as the stage reaches four, returning what its queue did with it, from which
    /// <see cref="BoostRestartAfterEcho"/> picks the cadence the boost restarts on and so its countdown.
    /// </param>
    public bool TryAdvanceSpeedBoosterAnimationStage(
        UnauthoredBoostCadence unauthoredBoostCadence,
        SamusMovementType movementType,
        GameplayAction controllerInput,
        int extraAnimationDelay,
        out int animationFrameTimer,
        Func<SoundQueueAdmission> requestEchoSound)
    {
        Ensure.NotNull(requestEchoSound);
        animationFrameTimer = 0;
        const GameplayAction dashButton = GameplayAction.Dash;
        GameplayAction heldButtons = controllerInput;
        if (!HasRunningMomentum || movementType != SamusMovementType.Running || !heldButtons.HasAny(dashButton))
            return false;

        // A countdown already empty takes the boost down a stage. A stage advances only
        // when the counted-down countdown is zero; that test leaves the stage unchanged.
        (SamusSpeedBoostCounter decremented, bool countdownExpired) = SpeedBoostCounter.CountDownOneRun(_boostRestart);
        SpeedBoostCounter = decremented;
        if (!countdownExpired)
            return false;

        SamusBoostCadence? cadenceAfterEcho = null;
        if (!SpeedBoostCounter.IsAtFullStage)
        {
            SpeedBoostCounter = SpeedBoostCounter.AdvanceStage();
            if (SpeedBoostCounter.IsAtFullStage)
            {
                EchoSoundPlaying = true;
                // What the echo's queue request found, not the stage just stored, picks
                // both the restart countdown and the first frame's cadence.
                cadenceAfterEcho = SamusRunningCadences.CadenceAfterEcho(_boostRestart, requestEchoSound());
            }
        }

        SamusBoostCadence cadence = cadenceAfterEcho ?? SamusRunningCadences.SpeedBoostCadence(SpeedBoostCounter.Stage);
        SpeedBoostCounter = SpeedBoostCounter.WithCountdown(cadence.CountdownAfterAdvance);

        animationFrameTimer = WrappingCountdown.Start(extraAnimationDelay)
            .AddFrames(SamusRunningCadences.Delay(unauthoredBoostCadence, cadence.Cadence, frame: 0)).RemainingFrames;
        // Animation runs after movement: the boost's stage changes its cadence and sound, and its contact damage starts
        // on the next movement update.
        return true;
    }

    /// <summary>The delay, in ticks, of running frame <paramref name="frame"/> at the boost's current stage.</summary>
    /// <param name="unauthoredBoostCadence">The session's unauthored Speed Booster cadence, which a full sound queue selects.</param>
    /// <param name="frame">The running frame, zero through nine.</param>
    internal int SpeedBoosterDelay(UnauthoredBoostCadence unauthoredBoostCadence, int frame) =>
        SamusRunningCadences.Delay(unauthoredBoostCadence, SamusRunningCadences.SpeedBoostCadence(SpeedBoostCounter.Stage).Cadence, frame);

    /// <summary>
    /// Runs the Speed Booster's share of the Screw Attack and Speed Booster glow when she is out of liquid.
    /// </summary>
    /// <param name="colours">Samus's colours, which the copied shade replaces.</param>
    /// <param name="frame">What this update's suit colouring reads of Samus.</param>
    internal void UpdateSpeedBoosterGlow(SamusColours colours, SpeedBoosterGlowFrame frame)
    {
        Ensure.NotNull(colours);
        _ = ShowDueSuitShade(colours, frame.EquippedItems);
        SamusSuit suit = frame.EquippedItems.ShownSuit();

        // The stored shine's and the shinespark's suit colourings take priority. Cancellation's
        // immediate normal colours above still show, but the ordinary colouring must not advance
        // its hidden four-frame Speed Booster cycle beneath the stored shine's or the shinespark's.
        if (frame.SuppressActiveSpeedBoosterGlow)
            return;

        // The glow tests for the Gravity Suit before it asks whether
        // Samus's *bottom* boundary lies under water or lava/acid. Power and Varia Samus
        // stop at once when submerged: the current colours, common
        // timer, and cycle shade step all remain untouched. The Gravity Suit passes this
        // test and goes on to the ordinary Screw Attack / Speed Booster glow even
        // though her bottom boundary still lies under the liquid. Her movement
        // supplies whether the bottom boundary is submerged, so the glow reads the liquid's surface exactly as
        // movement does.
        bool gravitySuitEquipped = frame.EquippedItems.HasAny(SamusEquipmentFlags.GravitySuit);
        if (!gravitySuitEquipped && frame.BottomBoundarySubmerged)
            return;

        bool screwAttackEquipped = frame.EquippedItems.HasAny(SamusEquipmentFlags.ScrewAttack);
        if (frame.MovementType == SamusMovementType.SpinJumping && screwAttackEquipped)
        {
            switch (frame.SpinPlace)
            {
                case SpinGlowPlace.Restarting:
                    // The spin's first frame restarts only the colour cycle's step. That frame keeps the normal suit colours
                    // already shown; it shows no Screw Attack shade.
                    FullBodyGlow.GoOnFrom(SamusGlowPlay.Before(FullBodyGlowStart));
                    return;
                case SpinGlowPlace.ShowsNormalColours:
                    // From the spin's first wall-jump-ready frame onward Samus shows her normal suit colours; the spin's
                    // frames between its first and that one cycle below. Her colours are what the next shown frame displays.
                    SamusSuitShade.Show(colours, frame.EquippedItems);
                    return;
                case SpinGlowPlace.Glowing:
                    ShowNextScrewAttackShade(colours, suit);
                    return;
                case SpinGlowPlace.NotSpinning:
                    throw new InvalidOperationException("A spin jump's animation stands somewhere in its spin.");
                default:
                    throw new InvalidOperationException($"Undefined {nameof(SpinGlowPlace)} {frame.SpinPlace}.");
            }
        }

        if (frame.MovementType == SamusMovementType.WallJumping)
        {
            if (!screwAttackEquipped)
                return;

            switch (frame.SpinPlace)
            {
                case SpinGlowPlace.Restarting:
                    // The wall-jump animation keeps the normal suit colours until its spin starts; its spin frames
                    // share the same six colour steps.
                    FullBodyGlow.GoOnFrom(SamusGlowPlay.Before(FullBodyGlowStart));
                    return;
                case SpinGlowPlace.Glowing:
                    ShowNextScrewAttackShade(colours, suit);
                    return;
                case SpinGlowPlace.NotSpinning:
                case SpinGlowPlace.ShowsNormalColours:
                    throw new InvalidOperationException($"A wall jump's animation stands at no {frame.SpinPlace} place.");
                default:
                    throw new InvalidOperationException($"Undefined {nameof(SpinGlowPlace)} {frame.SpinPlace}.");
            }
        }

        if (!IsActivelySpeedBoosting)
            return;

        // The Speed Booster goes on from where the shared glow stands once its hold runs out: a Screw Attack spin that stood
        // past the Speed Booster's four shades leaves it on its brightest, which it rests on as every boosted run does.
        if (SamusGlowPlay.StepToNextShadeGoingOnIn(FullBodyGlow, SamusGlowSegment.In(SamusGlow.SpeedBoosterRising, suit)) is not { } shown)
            return;
        colours.ShowSuit(shown);
    }

    /// <summary>
    /// Defers restoring Samus's normal suit colours until just before this update's suit colourings run.
    /// The game's pose initialization performs this when leaving a Screw Attack spin family.
    /// </summary>
    public void ShowSuitShadeAtColourStep() => SuitShade.Show();

    /// <summary>
    /// Shows at once the suit colours requested by momentum cancellation and
    /// pose initialization. These precede charge and special colouring handling, which may
    /// replace their colors in the same frame; they must not erase a later charge flash.
    /// </summary>
    public bool ShowDueSuitShade(
        SamusColours colours,
        SamusEquipmentFlags equippedItems)
    {
        Ensure.NotNull(colours);
        if (!SuitShade.Take())
            return false;

        SamusSuitShade.Show(colours, equippedItems);
        return true;
    }

    /// <summary>Shows the Screw Attack glow's next shade, going on from where the shared cycle stands.</summary>
    private void ShowNextScrewAttackShade(
        SamusColours colours,
        SamusSuit suit)
    {
        AnimationPlayback<SamusGlowSegment> cycle = FullBodyGlow.Playback;
        colours.ShowSuit(SamusGlowPlay.NextShade(SamusGlowSegment.In(SamusGlow.ScrewAttack, suit), ref cycle).Shown);
        FullBodyGlow.GoOnFrom(cycle);
    }

    /// <summary>
    /// Captures the speed-echo position for ordinary active boost.
    /// The gameplay clock, not the frame counter, selects the four-frame sampling phase.
    /// </summary>
    /// <param name="gameTimeFrames">The gameplay clock's frames within its second, zero or more; only its four-frame phase is read and nothing stores it.</param>
    /// <param name="position">Samus's whole-pixel room position sampled into the echo the phase names.</param>
    public bool CaptureSpeedEchoPosition(
        int gameTimeFrames,
        Position16<RoomPixels> position)
    {
        Ensure.AtLeastZero(gameTimeFrames);
        if (!IsActivelySpeedBoosting ||
            SpeedEchoesDeparting ||
            !SamusCadenceDefinitions.SpeedEchoCapture.IsDue(gameTimeFrames))
        {
            return false;
        }

        switch (SpeedEchoPhase)
        {
            case SpeedEchoPhase.CapturingFirst:
                SpeedEchoes[SpeedEcho.First].Position = position;
                SpeedEchoPhase = SpeedEchoPhase.CapturingSecond;
                break;
            case SpeedEchoPhase.CapturingSecond:
                SpeedEchoes[SpeedEcho.Second].Position = position;
                SpeedEchoPhase = SpeedEchoPhase.CapturingFirst;
                break;
            case SpeedEchoPhase.Departing:
                throw new InvalidOperationException("Departing echoes are not captured.");
            default:
                throw new InvalidOperationException($"Undefined {nameof(SpeedEchoPhase)} {SpeedEchoPhase}.");
        }

        return true;
    }

    /// <summary>
    /// Removes both echoes, stops their departure and points the next capture at the first echo: a shinespark's
    /// directional launch, its crash's start and its crash's end do this, and so does a room load.
    /// </summary>
    public void ClearSpeedEchoes()
    {
        SpeedEchoPhase = SpeedEchoPhase.CapturingFirst;
        foreach (SamusSpeedEcho echo in SpeedEchoes.Values)
            echo.Clear();
    }

    /// <summary>
    /// Starts the speed boost (#2557): the shared full-body glow over from its start, its first shade after
    /// <see cref="SamusShadeDefinitions.FullBodyCycles.SpeedBoosterFirstStepWait"/> updates, and the boost's counter at its
    /// first stage with that stage's countdown.
    /// </summary>
    private void StartBoost()
    {
        SamusGlowPlay.RestartAfter(FullBodyGlow, FullBodyGlowStart, SamusShadeDefinitions.FullBodyCycles.SpeedBoosterFirstStepWait);
        SpeedBoostCounter = new SamusSpeedBoostCounter(stage: 0, countdown: SamusRunningCadences.SpeedBoostCadence(0).CountdownAfterAdvance);
    }

    /// <summary>Adds the extra-run increment, a fraction past a whole pixel carrying into the whole pixels.</summary>
    private void AddExtraRunAcceleration() => ExtraRunSpeed += SamusMovementDefinitions.HorizontalMotion.ExtraRunAcceleration;

    /// <summary>Enables Speed Booster contact damage only at the counter's fully charged stage.</summary>
    private void PublishBoostContactDamage()
    {
        if (IsActivelySpeedBoosting)
            BodyAttack = SamusBodyAttack.SpeedBooster;
    }

    /// <summary>
    /// Cancels the speed boost, including the transition
    /// from alternating captured positions into the echoes' departure.
    /// </summary>
    /// <param name="facing">
    /// Current pose facing. The game treats exactly <see cref="SamusFacingDirection.Left"/> as facing left;
    /// every other facing uses the right-facing echo velocity.
    /// </param>
    public void CancelRunningMomentum(SamusFacingDirection facing)
    {
        if (HasRunningMomentum)
        {
            HasRunningMomentum = false;
            SpeedBoostCounter = SamusSpeedBoostCounter.NotBuilding;
            SamusGlowPlay.Restart(FullBodyGlow, FullBodyGlowStart);
            SuitShade.Show();
        }

        // Every movement can send the echoes departing, whether or not she has running momentum; later updates while
        // the echoes depart do not reset their velocities or restart the two bodies.
        if (!SpeedEchoesDeparting)
        {
            SpeedEchoPhase = SpeedEchoPhase.Departing;
            int speed = SamusMovementDefinitions.SpeedEchoDepartureSpeed.Toward(
                facing == SamusFacingDirection.Left ? HorizontalDirection.Left : HorizontalDirection.Right);
            foreach (SamusSpeedEcho echo in SpeedEchoes.Values)
                echo.DepartureSpeed = speed;
        }
    }

    /// <summary>
    /// Reconciles the speed and echoes after the equipment menu: unequipping the Speed Booster erases the echoes at
    /// once, and the extra run speed stays until the next movement update clears it.
    /// </summary>
    public void ReconcilePauseSpeedBoosterState(bool speedBoosterEquipped)
    {
        if (speedBoosterEquipped)
        {
            if (HasRunningMomentum && !SpeedBoostCounter.IsBuilding)
                StartBoost();
            return;
        }

        HasRunningMomentum = false;
        SamusGlowPlay.Restart(FullBodyGlow, FullBodyGlowStart);
        SpeedBoostCounter = SamusSpeedBoostCounter.NotBuilding;
        ClearSpeedEchoes();
    }

    /// <summary>
    /// Performs the complete horizontal-momentum teardown shared by collision, standing,
    /// posture, aerial, and Morph-Ball movements. The seam first cancels the Speed Booster's
    /// charge and echoes, then clears the extra run speed, the base speed and the
    /// acceleration mode, in that order.
    /// </summary>
    public void ClearHorizontalMomentum(SamusFacingDirection facingDirection)
    {
        CancelRunningMomentum(facingDirection);
        ClearHorizontalVelocity();
    }

    /// <summary>
    /// Clears the extra run speed, the base speed and the acceleration mode without cancelling the
    /// boost's charge. Crouching's cleanup clears only these;
    /// its angle-held input route can therefore preserve a temporary Blue Suit.
    /// </summary>
    public void ClearHorizontalVelocity()
    {
        ExtraRunSpeed = default;
        BaseSpeed = default;
        AccelerationMode = SamusAccelerationMode.Accelerating;
    }

    /// <summary>Stops Samus on an input fallback: stops acceleration and cancels boost, without clearing velocity.</summary>
    public void StopForNoTransition(SamusFacingDirection facingDirection)
    {
        AccelerationMode = SamusAccelerationMode.Accelerating;
        CancelRunningMomentum(facingDirection);
    }

    /// <summary>
    /// Executes running / normal jumping / airborne ball fallback's deceleration or stop, as the input phase selected it.
    /// Deceleration rechecks speed after movement before folding extra momentum.
    /// </summary>
    /// <param name="response">What the input phase selected (#2560): deceleration, or a stop.</param>
    /// <param name="facingDirection">The direction Samus faces, whose running momentum is cancelled.</param>
    /// <exception cref="ArgumentOutOfRangeException">The response is neither deceleration nor a stop.</exception>
    public void DecelerateForNoTransition(SamusMomentumResponse response, SamusFacingDirection facingDirection)
    {
        switch (response)
        {
            case SamusMomentumResponse.Decelerate when HasBaseSpeed:
                BaseSpeed += ExtraRunSpeed;
                ExtraRunSpeed = default;
                AccelerationMode = SamusAccelerationMode.Decelerating;
                break;
            case SamusMomentumResponse.Decelerate:
            case SamusMomentumResponse.Stop:
                AccelerationMode = SamusAccelerationMode.Accelerating;
                break;
            case SamusMomentumResponse.None:
            case SamusMomentumResponse.KillXSpeed:
            case SamusMomentumResponse.KillRunSpeed:
            default:
                throw new ArgumentOutOfRangeException(nameof(response), response, "A no-transition fallback decelerates or stops.");
        }
        CancelRunningMomentum(facingDirection);
    }

    /// <summary>
    /// Advances both departing echoes one update toward Samus at <paramref name="samusPosition"/>, each as
    /// <see cref="AdvanceDepartingSpeedEcho"/> moves it, and ends the departure once neither remains
    /// (<see cref="FinishDepartingSpeedEchoFrame"/>). Echoes that are not departing stay where they stand (#2573).
    /// </summary>
    /// <param name="samusPosition">Samus's whole-pixel room position as she is shown this update.</param>
    public void AdvanceDepartingSpeedEchoes(Position16<RoomPixels> samusPosition)
    {
        if (!SpeedEchoesDeparting)
            return;
        AdvanceDepartingSpeedEcho(SpeedEcho.First, samusPosition);
        AdvanceDepartingSpeedEcho(SpeedEcho.Second, samusPosition);
        FinishDepartingSpeedEchoFrame();
    }

    /// <summary>
    /// Advances one departing echo one update: Y approaches the live Samus center by its vertical approach, X advances by
    /// the signed departure speed, and the echo is gone on the exact update it crosses Samus. An echo already gone stays gone.
    /// </summary>
    private void AdvanceDepartingSpeedEcho(
        SpeedEcho echo,
        Position16<RoomPixels> samusPosition)
    {
        if (!SpeedEchoesDeparting)
            return;

        SamusSpeedEcho departing = Ensure.Listed(SpeedEchoes, echo);
        if (departing.Position is not { } standing)
            return;

        int xPosition = standing.X;
        int yPosition = standing.Y;
        // The echo stands where Samus stood and closes on where she stands, both in the room. An echo moving away from
        // her has already crossed her and is gone, and a crossing echo is gone before it is placed.
        if (yPosition != samusPosition.Y)
        {
            yPosition = yPosition < samusPosition.Y
                ? yPosition + SamusMovementDefinitions.SpeedEchoVerticalApproach
                : yPosition - SamusMovementDefinitions.SpeedEchoVerticalApproach;
        }

        int speed = departing.DepartureSpeed;
        xPosition += speed;
        bool crossedSamus = speed < 0
            ? xPosition < samusPosition.X
            : xPosition >= samusPosition.X;
        departing.Position = crossedSamus ? null : Position16<RoomPixels>.Of(xPosition, yPosition);
    }

    /// <summary>
    /// Clears the departure state after both departing echoes have been considered.
    /// Once neither echo remains, the next capture fills the first echo again.
    /// </summary>
    private void FinishDepartingSpeedEchoFrame()
    {
        if (SpeedEchoesDeparting && SpeedEchoes.Values.All(echo => echo.Position is null))
        {
            SpeedEchoPhase = SpeedEchoPhase.CapturingFirst;
        }
    }

    /// <summary>
    /// The speeds <paramref name="movementType"/> moves Samus by in her <see cref="SpeedMedium"/>: the medium's own, except
    /// the movements that take water's speeds in air (<see cref="WaterSpeedsInAir{TMovement, TSpeeds}"/>). Each speed keeps
    /// its whole pixels and its fraction exactly as authored instead of becoming a floating-point value.
    /// </summary>
    /// <param name="movementType">Samus's movement.</param>
    private SamusHorizontalSpeeds Speeds(SamusMovementType movementType) =>
        SpeedMedium == SamusLiquidMedium.Air && _waterSpeedsInAir.TryRead(movementType, out SamusHorizontalSpeeds waterSpeeds)
            ? waterSpeeds
            : Ensure.Listed(Ensure.Listed(SamusHorizontalSpeedDefinitions.ByMedium, SpeedMedium), movementType);

    /// <summary>
    /// Calculates the horizontal base speed and returns its
    /// magnitude. It updates Samus's base speed exactly once.
    /// </summary>
    public UnsignedSixteenSixteen CalculateBaseSpeed(SamusMovementType movementType)
    {
        SamusHorizontalSpeeds entry = Speeds(movementType);
        return CalculateBaseSpeed(entry);
    }

    /// <summary>
    /// Applies <paramref name="entry"/>'s acceleration or deceleration to the whole and fraction separately and updates
    /// base speed. A diagonal bomb jump and a grapple release pass their own speeds
    /// (<see cref="SamusHorizontalSpeedDefinitions.DiagonalBombJump"/>, <see cref="SamusHorizontalSpeedDefinitions.GrappleRelease"/>)
    /// instead of their movement's.
    /// </summary>
    internal UnsignedSixteenSixteen CalculateBaseSpeed(SamusHorizontalSpeeds entry)
    {
        if (AccelerationMode != SamusAccelerationMode.Accelerating)
        {
            BaseSpeed -= entry.Deceleration;

            // The test reads only the whole pixels. Once deceleration takes the speed
            // below zero, the speed stops and the mode returns to SamusAccelerationMode.Accelerating.
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
            UnsignedSixteenSixteen maximum = entry.MaximumSpeed;
            if (SamusSpeedMagnitude.ReadsPastLimit(BaseSpeed, maximum))
            {
                BaseSpeed = maximum;
            }
        }

        return BaseSpeed;
    }

    /// <summary>
    /// Calculates the horizontal base speed with deceleration disallowed.
    /// Despite the name, the turning acceleration mode still slows her as a turn
    /// does; accelerating and decelerating both accelerate toward the movement's
    /// maximum. The returned reached-maximum result is important to spin
    /// jump's decision to retain horizontal motion.
    /// </summary>
    public AerialBaseSpeedResult CalculateAerialBaseSpeed(
        SamusMovementType movementType)
    {
        SamusHorizontalSpeeds entry = Speeds(movementType);

        if (AccelerationMode == SamusAccelerationMode.Turning)
        {
            // This is the same deceleration the deceleration-allowed calculation subtracts. The distinction between the
            // two calculations is the turning test above and their reached-maximum result, not the subtract.
            BaseSpeed -= entry.Deceleration;
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
        UnsignedSixteenSixteen maximum = entry.MaximumSpeed;
        if (SamusSpeedMagnitude.ReadsPastLimit(BaseSpeed, maximum))
        {
            BaseSpeed = maximum;
            return new AerialBaseSpeedResult(BaseSpeed, ReachedMaximum: true);
        }

        return new AerialBaseSpeedResult(BaseSpeed, ReachedMaximum: false);
    }

    /// <summary>
    /// Calculates the total horizontal speed: adds the extra run speed, halves the sum once for each slowdown level up to
    /// <see cref="SamusHorizontalSpeedDefinitions.LargestSpeedHalvings"/>, and publishes the total speed animation and
    /// collision use.
    /// </summary>
    private UnsignedSixteenSixteen CalculateTotalSpeed(UnsignedSixteenSixteen baseSpeed)
    {
        TotalSpeed = (baseSpeed + ExtraRunSpeed).Halved(Math.Min(SlowdownLevel, SamusHorizontalSpeedDefinitions.LargestSpeedHalvings));
        return TotalSpeed;
    }

    /// <summary>
    /// Calculates the non-collision portion of a displacement toward <paramref name="side"/> (#2569): the total speed's
    /// velocity toward the side plus the extra displacement, within the whole-pixel clamp (<see cref="SamusHorizontalSpeedDefinitions.LargestWholeDisplacement"/>), so a leftward move is
    /// the extra displacement less the total speed rather than the rightward result reversed.
    /// </summary>
    public SignedSixteenSixteen CalculateDisplacement(HorizontalDirection side, UnsignedSixteenSixteen baseSpeed, SignedSixteenSixteen extraDisplacement) =>
        ClampDisplacement(CalculateTotalSpeed(baseSpeed).AsDisplacement().Toward(side) + extraDisplacement);

    /// <summary>Whether Samus has any base speed: either its whole pixels or its fraction is nonzero.</summary>
    public bool HasBaseSpeed => BaseSpeed != default;

    /// <summary>Clamps the whole-pixel part to <see cref="SamusHorizontalSpeedDefinitions.LargestWholeDisplacement"/> either way while preserving fractional displacement.</summary>
    private static SignedSixteenSixteen ClampDisplacement(SignedSixteenSixteen displacement) =>
        displacement.WithWholePixelsClamped(-SamusHorizontalSpeedDefinitions.LargestWholeDisplacement, SamusHorizontalSpeedDefinitions.LargestWholeDisplacement);
}
