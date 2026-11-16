using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The Baby Metroid cutscene entrance, Mother Brain drain,
/// release, ceiling retreat, route to Samus, latch, and healing, and its shared
/// movement helpers.
/// </summary>
/// <remarks>
/// The entrance uses no floating point, spline or physics engine. It holds a 1/256-pixel velocity, adds
/// its fraction to the position's 256ths of a pixel only, leaving the finer part, and lets that addition's
/// overflow enter the whole-pixel position. The curved entrance
/// also scales by the game's signed sine (<see cref="Trigonometry.MultiplySignedSine"/>). Keeping those details
/// here is important: replacing them with visually similar interpolation changes the exact
/// frame on which the Baby's collision rectangle reaches Mother Brain's brain.
/// </remarks>
public sealed class BabyMetroidCutsceneState
{
    /// <summary>Horizontal collision half-extent, 36 room pixels from the Baby's center, copied without halving from the Baby's enemy definition.</summary>
    /// <remarks>The Baby's enemy definition declares a width and height of 36, and enemy loading uses those
    /// values directly as the Baby's X and Y radii; nothing halves them.</remarks>
    public const int XHitboxRadius = 36;
    /// <summary>Vertical collision half-extent, 36 room pixels from the Baby's center, copied without halving from the Baby's enemy definition.</summary>
    public const int YHitboxRadius = 36;

    /// <summary>The entrance's current phase.</summary>
    public BabyMetroidCutscenePhase Phase { get; internal set; } =
        BabyMetroidCutscenePhase.Inactive;

    /// <summary>How the actor meets Samus and her shots, after initialization makes it block the Plasma Beam.</summary>
    public EnemyContact Contact { get; private set; }

    /// <summary>What the room's enemy passes run for the actor, after initialization starts its animation.</summary>
    public EnemyProcessing Processing { get; private set; }

    /// <summary>Whether the actor is drawn and still holds its place.</summary>
    public EnemyPresence Presence { get; private set; }

    /// <summary>
    /// Whether the onion-ring hit flash shows the shared white colours this update instead of
    /// the Baby's own sprite colours.
    /// </summary>
    public bool HurtFlashShown { get; private set; }

    /// <summary>The Baby's current animation.</summary>
    public MotherBrainBabySegment Animation { get; private set; }

    /// <summary>The current frame's remaining hold, restarted at one whenever the animation changes.</summary>
    public int FrameHold { get; private set; }

    /// <summary>
    /// Room position. This actor's 1/256-pixel mover moves it in 256ths of a pixel; its precision finer
    /// than a 256th of a pixel is deliberately kept unchanged.
    /// </summary>
    public FixedPosition<RoomPixels> Position { get; internal set; }

    /// <summary>Signed 1/256-pixel velocity.</summary>
    public EightEightVector<RoomPixels> Velocity { get; internal set; }

    /// <summary>The curved entrance's speed, zero through fourteen pixels an update.</summary>
    public SignedEightEight Speed { get; private set; }

    /// <summary>Heading of the curved entrance; only its whole step is passed to shared component math.</summary>
    public RotationAngle Angle { get; private set; }

    /// <summary>Phase timer: each timed phase counts it down and moves on as it runs out (<see cref="BossCountdown"/>).</summary>
    public BossCountdown PhaseTimer { get; } = new();

    /// <summary>
    /// The leg of the ceiling-to-Samus route the Baby is steering along, the route's first once it reaches the ceiling;
    /// null before then.
    /// </summary>
    internal BabyMetroidRouteLeg? RouteLeg { get; private set; }

    /// <summary>
    /// Enemy health. The Baby's enemy definition supplies 3,200; Mother Brain later drains
    /// this value after the healing hold, so it belongs to actor state rather than rendering.
    /// </summary>
    public int Health { get; internal set; }

    /// <summary>
    /// The Baby's onion-ring flash timer. Mother Brain's onion-ring collision response
    /// writes sixteen here. Main AI observes the nonzero value as a doubled shake, then
    /// the later flashing step decrements it and alternates the enemy colours.
    /// </summary>
    public int OnionRingHitFlashTimer { get; private set; }

    /// <summary>Saved whole-pixel origin used by the fatal-blow shake.</summary>
    public Position16<RoomPixels> FatalBlowOrigin { get; private set; }

    /// <summary>Reload-eight timer producing the nine-call cadence of the Baby's fade to black.</summary>
    public BossCountdown FadeToBlackTimer { get; } = new();

    /// <summary>The shade the Baby's fade to black has reached; valid stored values are 0..6.</summary>
    public int FadeToBlackShade { get; private set; }

    /// <summary>Five-call cadence timer for the dust explosions surrounding the dying Baby.</summary>
    public BossCountdown DeathExplosionTimer { get; } = new();

    /// <summary>
    /// Index into the shared death-explosion scatter pattern, cycling through its ten offsets.
    /// This actor owns its increment and wrap.
    /// </summary>
    public int DeathExplosionPatternIndex { get; private set; }

    /// <summary>
    /// The updates the cutscene has spent readying Mother Brain's attacks again so far, zero through
    /// <see cref="MotherBrainReadyingDefinitions.AttacksAgainDuration"/>.
    /// </summary>
    public int AttacksAgainReadyingElapsed { get; private set; }

    /// <summary>The step the phase-three room lights' restoration has reached, 0 through 8.</summary>
    public int RoomLightsTransitionCounter { get; private set; }

    /// <summary>
    /// The part of a rainbow colour frame accumulated toward slowing Samus's rainbow: each call adds to it, and every
    /// completed whole frame holds her rainbow's glow longer.
    /// </summary>
    public RainbowFramePart SamusRainbowFraction { get; private set; }

    /// <summary>Which of the two rainbow colour cycles Samus currently shows.</summary>
    public BabyMetroidSamusRainbowPhase SamusRainbowPhase { get; private set; }

    /// <summary>True after the deleted enemy property marks the cutscene actor deleted.</summary>
    public bool IsDeleted => Presence.HasAny(EnemyPresence.Deleted);

    /// <summary>
    /// Initializes the Baby. The population entry supplies its properties; initialization
    /// adds two more, overwrites the population coordinates, and waits at its arrival point
    /// before beginning the dash, from the fractions of a pixel its place keeps
    /// (<see cref="BabyMetroidArrivalFractions"/>).
    /// </summary>
    /// <param name="leftInPlace">The position the Baby's place still holds from its previous occupant.</param>
    /// <param name="populationContact">The population entry's contact.</param>
    /// <param name="populationProcessing">The population entry's processing.</param>
    /// <param name="populationPresence">The population entry's presence.</param>
    public void Initialize(
        FixedPosition<RoomPixels> leftInPlace,
        EnemyContact populationContact,
        EnemyProcessing populationProcessing,
        EnemyPresence populationPresence)
    {
        Contact = populationContact.With(EnemyContact.BlocksPlasmaBeam);
        Processing = populationProcessing.With(EnemyProcessing.Animates);
        Presence = populationPresence;
        HurtFlashShown = false;
        SetAnimation(MotherBrainBabySegment.Initial);
        RouteLeg = null;
        Health = 3200;
        OnionRingHitFlashTimer = 0;
        FatalBlowOrigin = Position16<RoomPixels>.Zero;
        FadeToBlackTimer.Count = 0;
        FadeToBlackShade = 0;
        DeathExplosionTimer.Count = 0;
        DeathExplosionPatternIndex = 0;
        AttacksAgainReadyingElapsed = 0;
        RoomLightsTransitionCounter = 0;
        SamusRainbowFraction = RainbowFramePart.None;
        SamusRainbowPhase = BabyMetroidSamusRainbowPhase.Inactive;
        Position = BabyMetroidArrivalFractions.StartingPosition(BabyMetroidCutsceneFlightDefinitions.ArrivalPoint, leftInPlace);
        Velocity = default;
        Speed = SignedEightEight.Zero;
        Angle = RotationAngle.Zero;
        Phase = BabyMetroidCutscenePhase.DashOntoScreen;
        PhaseTimer.Count = 248;
    }

    /// <summary>
    /// Runs one update of the Baby's main AI: the phase first, then the unconditional enemy
    /// velocity mover. Flash/colour presentation is intentionally kept
    /// as inspectable state because it does not alter this entrance's coordinates.
    /// Every Baby step reads the coordinates of Mother Brain's head, which the head's AI
    /// has already moved this frame; <paramref name="head"/> carries them.
    /// </summary>
    public BabyMetroidCutsceneStepResult Step(
        SamusState samus,
        MotherBrainRainbowBeamAttackSequence motherBrain,
        Position16<RoomPixels> head,
        Position16<RoomPixels> cameraPosition,
        int enemyFrameCounter,
        int randomNumber)
    {
        Ensure.NotNull(samus);
        Ensure.NotNull(motherBrain);
        if (Phase == BabyMetroidCutscenePhase.Inactive)
            throw new InvalidOperationException("The cutscene Baby Metroid has not been initialized.");

        BabyMetroidCutscenePhase phaseBefore = Phase;
        bool brainCollision;

        bool latchSoundQueued = false;
        // The release emits three independent dust allocations when the Baby
        // finally lets go. Keep the individual room coordinates and animations instead of
        // collapsing that side effect into a boolean: the spawn order matters when
        // the room's enemy projectiles have nearly no room left.
        var releaseDustClouds = new List<BabyMetroidReleaseDustRequest>(capacity: 3);
        bool ambientCrySoundQueued = false;
        bool samusRainbowActivated = false;
        DustExplosionRequest? deathExplosion = null;
        int? babyFadeShade = null;
        int? recoveryLightsFrame = null;

        // Enemy collision handling runs touch AI before main AI, against the Baby's
        // position from the previous frame. The Baby's touch is active only while it latches onto Samus; broad
        // Samus/enemy hitboxes gate it, then its own divisor-16 acceleration helper decides when
        // both axes have reached the latch point. Main AI and its mover follow this frame.
        if (Phase == BabyMetroidCutscenePhase.LatchOntoSamus && TouchesSamus(samus))
        {
            bool reachedLatchPoint = AccelerateTowardsPoint(
                samus.Position.Whole + BabyMetroidCutsceneFlightDefinitions.SamusLatchOffset,
                acceleration: SignedEightEight.FromTwoHundredFiftySixthsOfPixel(16));
            if (reachedLatchPoint)
            {
                Velocity = default;
                Phase = BabyMetroidCutscenePhase.HealSamusToFullHealth;
            }
        }

        switch (Phase)
        {
            case BabyMetroidCutscenePhase.DashOntoScreen:
                // The Baby holds still for the 249 updates its count of 248 lasts. The update it runs out on immediately
                // executes the first curve update before the common mover consumes its new velocity.
                if (PhaseTimer.StepRunsOut())
                {
                    Angle = BabyMetroidCutsceneFlightDefinitions.CurveStartHeading;
                    Speed = BabyMetroidCutsceneFlightDefinitions.CurveSpeed;
                    Phase = BabyMetroidCutscenePhase.CurveTowardMotherBrainHead;
                    PhaseTimer.Count = 10;
                    goto case BabyMetroidCutscenePhase.CurveTowardMotherBrainHead;
                }
                break;

            case BabyMetroidCutscenePhase.CurveTowardMotherBrainHead:
                UpdateSpeedAndHeading(BabyMetroidCutsceneFlightDefinitions.CurveTurn, BabyMetroidCutsceneFlightDefinitions.CurveTargetHeading, BabyMetroidCutsceneFlightDefinitions.CurveSpeed);
                if (PhaseTimer.StepRunsOut())
                {
                    Phase = BabyMetroidCutscenePhase.GetRightUpInMotherBrainsFace;
                    PhaseTimer.Count = 9;
                }
                break;

            case BabyMetroidCutscenePhase.GetRightUpInMotherBrainsFace:
                UpdateSpeedAndHeading(BabyMetroidCutsceneFlightDefinitions.FaceTurn, BabyMetroidCutsceneFlightDefinitions.FaceTargetHeading, BabyMetroidCutsceneFlightDefinitions.FaceSpeed);
                brainCollision = CollidesWithRectangle(
                    head,
                    TargetRadius);

                // Collision skips the countdown's step. Otherwise its count of nine lasts ten approach calls.
                if (!brainCollision)
                {
                    brainCollision = PhaseTimer.StepRunsOut();
                }

                if (brainCollision)
                {
                    Phase = BabyMetroidCutscenePhase.LatchOntoMotherBrain;
                    samus.Drained.PutStanding(samus); // Stands the drained Samus up.
                }
                break;

            case BabyMetroidCutscenePhase.LatchOntoMotherBrain:
            {
                Position16<RoomPixels> target = head + BabyMetroidCutsceneFlightDefinitions.BrainLatchOffset;
                GraduallyAccelerateTowardsPoint(
                    target,
                    accelerationDivisor: 16,
                    BabyMetroidCutsceneFlightDefinitions.ApproachWrongWaySpeed,
                    cameraPosition);
                brainCollision = CollidesWithRectangle(target, new Extent16<RoomPixels>(8, 8));
                if (brainCollision)
                    Phase = BabyMetroidCutscenePhase.SetMotherBrainToStumbleBack;
                break;
            }

            case BabyMetroidCutscenePhase.SetMotherBrainToStumbleBack:
                // The drain step passes animation-delay index two and target Body.X-1. With a
                // standing body this always starts the really-fast backward animation.
                motherBrain.RequestBabyStumbleBackward();
                Phase = BabyMetroidCutscenePhase.ActivateRainbowBeamAndMotherBrainBody;
                goto case BabyMetroidCutscenePhase.ActivateRainbowBeamAndMotherBrainBody;

            case BabyMetroidCutscenePhase.ActivateRainbowBeamAndMotherBrainBody:
            {
                Position16<RoomPixels> target = head + BabyMetroidCutsceneFlightDefinitions.BrainLatchOffset;
                bool reachedTarget = AccelerateTowardsPoint(target, acceleration: SignedEightEight.FromPixels(2));
                if (reachedTarget)
                {
                    // The helper predicts whole-pixel overshoot but does not itself store
                    // its returned target coordinate. The drain step performs the exact pin.
                    Velocity = default;
                    Position = Position.WithWhole(target);
                    SetAnimation(MotherBrainBabySegment.DrainingMotherBrain);
                    Phase = BabyMetroidCutscenePhase.WaitForMotherBrainToTurnToCorpse;
                    motherBrain.InterruptFinalBeamForBabyDrain();
                    latchSoundQueued = true; // The rainbow-drain sound starts as the Baby latches on.
                }
                break;
            }

            case BabyMetroidCutscenePhase.WaitForMotherBrainToTurnToCorpse:
            {
                int shakingIndex = MotherBrainCadenceDefinitions.BabyShakeStep.Of(enemyFrameCounter);
                Position = Position.WithWhole(head + MotherBrainShakeDefinitions.Offsets[shakingIndex] + BabyMetroidCutsceneFlightDefinitions.BrainLatchOffset);
                if (motherBrain.State.Phase2CorpseState != MotherBrainPhase2CorpseState.Living)
                {
                    Phase = BabyMetroidCutscenePhase.StopDraining;
                    PhaseTimer.Count = 64;
                }
                break;
            }

            case BabyMetroidCutscenePhase.StopDraining:
                // Shaking ceases immediately: every update of the wait pins the Baby to the brain
                // before decrementing the timer of 64, which expires only after 65 updates.
                Position = Position.WithWhole(head + BabyMetroidCutsceneFlightDefinitions.BrainLatchOffset);
                if (PhaseTimer.StepRunsOut())
                {
                    SetAnimation(MotherBrainBabySegment.Initial);
                    Phase = BabyMetroidCutscenePhase.LetGoAndSpawnDustClouds;
                    PhaseTimer.Count = 32;
                    Velocity = default;
                }
                break;

            case BabyMetroidCutscenePhase.LetGoAndSpawnDustClouds:
                // The release branches *into* its acceleration while the timer is nonnegative. Thus the
                // Baby accelerates ceilingward for all 32 visible release calls; the dust
                // burst happens on call 33 and that call also performs another acceleration.
                if (PhaseTimer.StepRunsOut())
                {
                    // The three dust clouds on Mother Brain's head are spawned in this
                    // exact order. Each helper adds its signed offsets to the brain
                    // enemy's current room coordinate and spawns a small dust cloud
                    // in the highest free place for an enemy projectile.
                    releaseDustClouds.Add(new BabyMetroidReleaseDustRequest(
                        head + new Offset16<RoomPixels>(-16, -8),
                        Animation: MiscDustAnimation.SmallDustCloud));
                    releaseDustClouds.Add(new BabyMetroidReleaseDustRequest(
                        head + new Offset16<RoomPixels>(0, -16),
                        Animation: MiscDustAnimation.SmallDustCloud));
                    releaseDustClouds.Add(new BabyMetroidReleaseDustRequest(
                        head + new Offset16<RoomPixels>(16, -8),
                        Animation: MiscDustAnimation.SmallDustCloud));
                    Phase = BabyMetroidCutscenePhase.MoveToTheCeiling;
                }
                goto case BabyMetroidCutscenePhase.MoveToTheCeiling;

            case BabyMetroidCutscenePhase.MoveToTheCeiling:
            {
                Position16<RoomPixels> target = head.WithY(0);
                GraduallyAccelerateTowardsPoint(
                    target,
                    accelerationDivisor: 16,
                    BabyMetroidCutsceneFlightDefinitions.ApproachWrongWaySpeed,
                    cameraPosition);
                bool ceilingCollision = CollidesWithRectangle(target, TargetRadius);
                if (ceilingCollision)
                {
                    samus.Drained.PutCrouchingOrFalling(samus);
                    Phase = BabyMetroidCutscenePhase.MoveToSamus;
                    RouteLeg = BabyMetroidRoute.FirstLeg;
                }
                break;
            }

            case BabyMetroidCutscenePhase.MoveToSamus:
            {
                // The route step enables health-based colour selection on every route call. The ambient cry takes its
                // own chance.
                ambientCrySoundQueued = MotherBrainRandomDefinitions.BabyMetroidCries.IsTakenBy(randomNumber);

                // The route is its authored legs, each steered to its waypoint with its own wrong-way speed.
                BabyMetroidRouteLeg leg = RouteLeg ?? throw new InvalidOperationException(
                    "The Baby is moving to Samus without a route leg.");
                Position16<RoomPixels> target = leg.Target;

                GraduallyAccelerateTowardsPoint(
                    target,
                    BabyMetroidRouteDefinitions.AccelerationDivisor,
                    leg.WrongWayOffScreenXSpeed,
                    cameraPosition);

                // The route's 4x4 rectangle test occurs before the common velocity mover.
                // Reaching a leg's target moves on to the next leg; reaching the
                // last leg's target starts the latch-onto-Samus phase instead.
                if (CollidesWithRectangle(target, TargetRadius))
                {
                    if (BabyMetroidRoute.LegAfter(leg) is { } next)
                        RouteLeg = next;
                    else
                        Phase = BabyMetroidCutscenePhase.LatchOntoSamus;
                }
                break;
            }

            case BabyMetroidCutscenePhase.LatchOntoSamus:
                // The latch approach itself only steers toward Samus's centre minus twenty pixels. It
                // never performs the state change. That transition belongs to the enemy's touch
                // response, executed after main AI and the common mover below.
                GraduallyAccelerateTowardsPoint(
                    samus.Position.Whole + BabyMetroidCutsceneFlightDefinitions.SamusLatchOffset,
                    accelerationDivisor: 16,
                    BabyMetroidCutsceneFlightDefinitions.ApproachWrongWaySpeed,
                    cameraPosition);
                break;

            case BabyMetroidCutscenePhase.HealSamusToFullHealth:
            {
                // The healing step pins the actor to its four-step shake before healing. The
                // subsequent common mover is harmless because the touch response zeroed
                // both velocities when it started this phase.
                int shakingIndex = MotherBrainCadenceDefinitions.BabyShakeStep.Of(enemyFrameCounter);
                Position = Position.WithWhole(samus.Position.Whole + MotherBrainShakeDefinitions.Offsets[shakingIndex] + BabyMetroidCutsceneFlightDefinitions.SamusLatchOffset);

                // Healing adds one energy each enemy update until Samus's health reaches her maximum.
                int candidateHealth = samus.Health + 1;
                if (candidateHealth < samus.MaxHealth)
                {
                    samus.Health = candidateHealth;
                }
                else
                {
                    samus.Health = samus.MaxHealth;
                    samus.ReserveEnergy = samus.MaxReserveEnergy;
                    Phase = BabyMetroidCutscenePhase.IdleUntilNoHealth;
                }
                break;
            }

            case BabyMetroidCutscenePhase.IdleUntilNoHealth:
                // The Baby's damaged step consumes Mother Brain's one-shot cry flag before inspecting
                // the hit-flash timer. Sound queuing is returned by the ring system; this
                // actor owns only the resulting shake and health-to-phase transition.
                if (OnionRingHitFlashTimer != 0)
                {
                    int shakingIndex = MotherBrainCadenceDefinitions.BabyHitShakeStep.Of(OnionRingHitFlashTimer);
                    Offset16<RoomPixels> shake = MotherBrainShakeDefinitions.Offsets[shakingIndex];
                    Position = Position.WithWhole(samus.Position.Whole + shake + shake + BabyMetroidCutsceneFlightDefinitions.SamusLatchOffset);
                }

                if (Health == 0)
                {
                    // The zero reached by the ordinary volleys is a state trigger, not the
                    // health carried into the final charge. The game deliberately gives
                    // the Baby 320 health, changes to the low-health colour phase,
                    // disables health-band colours, and lets the next step run next frame.
                    Health = 320;
                    Phase = BabyMetroidCutscenePhase.ReleaseSamus;
                }
                break;

            case BabyMetroidCutscenePhase.ReleaseSamus:
                // The next step acts on both actors in one call. The speed it gives
                // Mother Brain's brain is presentation state; the meaningful body change
                // starts Mother Brain's fast retreat while this same call falls through to
                // the first flight update.
                motherBrain.PrepareForFinalBabyMetroidAttack();
                Phase = BabyMetroidCutscenePhase.StareDownMotherBrain;
                goto case BabyMetroidCutscenePhase.StareDownMotherBrain;

            case BabyMetroidCutscenePhase.StareDownMotherBrain:
                // Literal target: four pixels left of Samus and Y 96. This leg and the next two
                // reverse off screen with the staging wrong-way speed, not the approaches' one.
            {
                var target = new Position16<RoomPixels>(CoordinateAxis.Wrap(samus.Position.X.Whole - 4), 96);
                GraduallyAccelerateTowardsPoint(
                    target,
                    accelerationDivisor: 16,
                    BabyMetroidCutsceneFlightDefinitions.StagingWrongWaySpeed,
                    cameraPosition);
                if (CollidesWithRectangle(target, TargetRadius))
                {
                    Phase = BabyMetroidCutscenePhase.FlyToRightEdge;
                }
                break;
            }

            case BabyMetroidCutscenePhase.FlyToRightEdge:
                // The leg targets the fixed point (272, 64), just past the right edge of the screen.
                GraduallyAccelerateTowardsPoint(
                    new Position16<RoomPixels>(272, 64),
                    accelerationDivisor: 16,
                    BabyMetroidCutsceneFlightDefinitions.StagingWrongWaySpeed,
                    cameraPosition);
                if (CollidesWithRectangle(new Position16<RoomPixels>(272, 64), TargetRadius))
                    Phase = BabyMetroidCutscenePhase.MoveToFinalChargeStart;
                break;

            case BabyMetroidCutscenePhase.MoveToFinalChargeStart:
                // The final approach stages the Baby at `(305,160)`. Reaching that rectangle assigns
                // exactly 79 HP so the final four-ring volley kills on its first collision,
                // clears the brain's speed flag, and replaces Mother Brain's retreat AI.
                GraduallyAccelerateTowardsPoint(
                    new Position16<RoomPixels>(305, 160),
                    accelerationDivisor: 16,
                    BabyMetroidCutsceneFlightDefinitions.StagingWrongWaySpeed,
                    cameraPosition);
                if (CollidesWithRectangle(new Position16<RoomPixels>(305, 160), TargetRadius))
                {
                    Health = 79;
                    motherBrain.ExecuteFinalBabyMetroidAttack();
                    Phase = BabyMetroidCutscenePhase.InitiateFinalCharge;
                }
                break;

            case BabyMetroidCutscenePhase.InitiateFinalCharge:
                // The charge's start divides the distance by six, a much firmer pull than the staging legs' sixteen,
                // and reverses off screen with the approaches' wrong-way speed.
                GraduallyAccelerateTowardsPoint(
                    new Position16<RoomPixels>(290, 128),
                    accelerationDivisor: 6,
                    BabyMetroidCutsceneFlightDefinitions.ApproachWrongWaySpeed,
                    cameraPosition);
                if (CollidesWithRectangle(new Position16<RoomPixels>(290, 128), TargetRadius))
                    Phase = BabyMetroidCutscenePhase.FinalCharge;
                break;

            case BabyMetroidCutscenePhase.FinalCharge:
                // The final target follows Mother Brain's brain, 32 pixels above its
                // centre. Its divisor of four accelerates much more aggressively than the staging leg.
                GraduallyAccelerateTowardsPoint(
                    head + new Offset16<RoomPixels>(0, -32),
                    accelerationDivisor: 4,
                    BabyMetroidCutsceneFlightDefinitions.ApproachWrongWaySpeed,
                    cameraPosition);
                if (Health == 0)
                {
                    // The fatal-blow step changes graphics/animation state, freezes the body AI,
                    // clears velocity, remembers the shake origin, and falls through into
                    // the first of 17 fatal-blow shake calls.
                    SetAnimation(MotherBrainBabySegment.TakingFatalBlow);
                    Velocity = default;
                    Phase = BabyMetroidCutscenePhase.TakeFinalBlow;
                    PhaseTimer.Count = 16;
                    FatalBlowOrigin = Position.Whole;
                    goto case BabyMetroidCutscenePhase.TakeFinalBlow;
                }
                break;

            case BabyMetroidCutscenePhase.TakeFinalBlow:
            {
                // The cutscene's shake first adds -1 to the Y velocity,
                // which the common mover then applies, and places the Baby at the saved origin
                // plus the undoubled shaking offsets.
                Velocity = Velocity.WithY(Velocity.Y - SignedEightEight.FromTwoHundredFiftySixthsOfPixel(1));
                int shakingIndex = MotherBrainCadenceDefinitions.BabyShakeStep.Of(enemyFrameCounter);
                Position = Position.WithWhole(FatalBlowOrigin + MotherBrainShakeDefinitions.Offsets[shakingIndex]);
                if (PhaseTimer.StepRunsOut())
                {
                    Position = Position.WithWhole(FatalBlowOrigin);
                    Phase = BabyMetroidCutscenePhase.PlaySamusTheme;
                    PhaseTimer.Count = 56;
                    goto case BabyMetroidCutscenePhase.PlaySamusTheme;
                }
                break;
            }

            case BabyMetroidCutscenePhase.PlaySamusTheme:
                // The expiry call queues the two delayed music requests and immediately
                // performs the first twelve-count prepare-Hyper-Beam decrement.
                if (PhaseTimer.StepRunsOut())
                {
                    Phase = BabyMetroidCutscenePhase.PrepareSamusForHyperBeam;
                    PhaseTimer.Count = 12;
                    goto case BabyMetroidCutscenePhase.PrepareSamusForHyperBeam;
                }
                break;

            case BabyMetroidCutscenePhase.PrepareSamusForHyperBeam:
                if (PhaseTimer.StepRunsOut())
                {
                    // The Hyper Beam acquisition freezes Samus's drained animation on its Hyper Beam acquisition frame.
                    // The Hyper Beam step also installs the first rainbow step; neither step is
                    // executed until the Baby's following enemy-AI call.
                    SamusDrainedState.FreezeForHyperBeamAcquisition(samus);
                    SamusRainbowPhase = BabyMetroidSamusRainbowPhase.ActivateWhenEnemyIsLow;
                    Phase = BabyMetroidCutscenePhase.DeathSequence;
                }
                break;

            case BabyMetroidCutscenePhase.DeathSequence:
                StepSamusRainbowColours(samus, ref samusRainbowActivated);
                AccelerateDownwardsForDeath();
                if (StepFadeToBlack(ref babyFadeShade))
                {
                    // Completion does not write the Baby's eighth colour. It hides the actor,
                    // starts readying Mother Brain's attacks again, and waits through 128..0 before its first update.
                    Presence = Presence.With(EnemyPresence.Hidden);
                    Phase = BabyMetroidCutscenePhase.ReadyMotherBrainAttacksAgain;
                    PhaseTimer.Count = 128;
                }
                else
                {
                    deathExplosion = StepDeathExplosion();
                    // The flicker step clears invisibility on odd enemy frames and sets it on even.
                    Presence = MotherBrainCadenceDefinitions.BabyDeathFlickerShown.IsSetIn(enemyFrameCounter)
                        ? Presence.Without(EnemyPresence.Hidden)
                        : Presence.With(EnemyPresence.Hidden);
                }
                break;

            case BabyMetroidCutscenePhase.ReadyMotherBrainAttacksAgain:
                StepSamusRainbowColours(samus, ref samusRainbowActivated);
                if (PhaseTimer.StepRunsOut())
                {
                    if (AttacksAgainReadyingElapsed >= MotherBrainReadyingDefinitions.AttacksAgainDuration)
                        throw new InvalidOperationException("Mother Brain's attacks are already readied again.");

                    AttacksAgainReadyingElapsed++;

                    if (AttacksAgainReadyingElapsed == MotherBrainReadyingDefinitions.AttacksAgainDuration)
                    {
                        // The last readying update falls straight into the next step, so the newly written 176
                        // becomes 175 on this same call.
                        Phase = BabyMetroidCutscenePhase.LetSamusRainbowSomeMore;
                        PhaseTimer.Count = 176;
                        goto case BabyMetroidCutscenePhase.LetSamusRainbowSomeMore;
                    }
                }
                break;

            case BabyMetroidCutscenePhase.LetSamusRainbowSomeMore:
                if (PhaseTimer.StepRunsOut())
                {
                    Phase = BabyMetroidCutscenePhase.FinalCutscene;
                    RoomLightsTransitionCounter = 0;
                    goto case BabyMetroidCutscenePhase.FinalCutscene;
                }
                break;

            case BabyMetroidCutscenePhase.FinalCutscene:
            {
                // Each call shows the next of the room lights' seven recovery steps; the call after the last ends
                // the cutscene.
                int lightsStep = RoomLightsTransitionCounter;
                RoomLightsTransitionCounter++;
                if (lightsStep < 7)
                {
                    recoveryLightsFrame = lightsStep;
                    break;
                }

                motherBrain.BeginPhase3RecoveryFromBabyCutscene();
                samus.Drained.DisableRainbowAndStartStandingAnimation(samus);
                samus.Drained.EnableHyperBeam(samus);
                Presence = Presence.With(EnemyPresence.Deleted);
                break;
            }

            case BabyMetroidCutscenePhase.Inactive:
            default:
                throw new InvalidOperationException($"Unsupported Baby Metroid phase {Phase}.");
        }

        // Main AI calls this even on phase transitions and target snaps. In the snap case
        // both velocities were explicitly zeroed, so the pin remains exact.
        MoveAccordingToVelocity();

        // The flash step is called after the common velocity mover. It decrements the same
        // extra timer that onion-ring hits write and shows the flash colours while the
        // decremented value, modulo four, is two or three. Starting from sixteen, the first post-hit enemy call sees
        // sixteen for its shake above, then displays the normal colours with timer fifteen.
        if (OnionRingHitFlashTimer != 0)
        {
            OnionRingHitFlashTimer--;
            HurtFlashShown = MotherBrainCadenceDefinitions.BabyHurtFlashShown.IsSetIn(OnionRingHitFlashTimer);
        }
        else
        {
            HurtFlashShown = false;
        }

        return new BabyMetroidCutsceneStepResult(
            phaseBefore,
            Phase,
            latchSoundQueued,
            releaseDustClouds.ToArray(),
            ambientCrySoundQueued,
            deathExplosion,
            babyFadeShade,
            recoveryLightsFrame);
    }

    /// <summary>Applies the Baby's health/flash mutation from the blue-ring collision response: a living Baby gets a sixteen-frame flash timer and health subtraction saturated at zero.</summary>
    /// <param name="damage">The ring's damage (<see cref="MotherBrainAttackDamage.OnionRing"/>), zero or more; even zero damage refreshes the flash timer when health is nonzero.</param>
    /// <remarks>Zero health leaves all state unchanged. The ring's touch establishes the overlap, and the cutscene requests the Baby's cry, copies the Baby's health to its enemy and explodes and deletes the ring.</remarks>
    public void ApplyMotherBrainOnionRingHit(int damage)
    {
        int healthBefore = Health;
        if (healthBefore == 0)
            return;

        OnionRingHitFlashTimer = 16;
        Health = Math.Max(0, healthBefore - damage);
    }

    /// <summary>Activates Samus's healed rainbow colours when the Baby reaches her, then slows its frame cadence by a fractional counter.</summary>
    private void StepSamusRainbowColours(
        SamusState samus,
        ref bool samusRainbowActivated)
    {
        switch (SamusRainbowPhase)
        {
            case BabyMetroidSamusRainbowPhase.Inactive:
                return;

            case BabyMetroidSamusRainbowPhase.ActivateWhenEnemyIsLow:
                // The rainbow step uses a signed compare after adding sixteen to the Baby's Y.
                if (CoordinateAxis.AsSigned(Position.Y.Whole + 16 - samus.Position.Y.Whole) < 0)
                    return;
                samus.Drained.EnableRainbow(samus);
                samusRainbowActivated = true;
                SamusRainbowPhase = BabyMetroidSamusRainbowPhase.GraduallySlowAnimationDown;
                return;

            case BabyMetroidSamusRainbowPhase.GraduallySlowAnimationDown:
            {
                (RainbowFramePart fraction, bool frameCompleted) = SamusRainbowFraction.AdvancedByOneCall();
                SamusRainbowFraction = fraction;
                if (frameCompleted)
                    samus.Drained.HoldRainbowGlowLonger(longestHold: 10);
                return;
            }

            default:
                throw new InvalidOperationException($"Unsupported Samus rainbow phase {SamusRainbowPhase}.");
        }
    }

    /// <summary>Slows the horizontal speed by the death fall's drag and adds its gravity to the vertical speed.</summary>
    private void AccelerateDownwardsForDeath()
    {
        // The slowing step takes the drag from the speed, stopping at rest and keeping its direction, and
        // independently adds the gravity to the vertical velocity. The fall ends 63 updates after the Baby reaches
        // line 128, so even a fall across the whole coordinate space leaves the speed below 33 pixels an update.
        Velocity = new(Velocity.X.SlowedBy(BabyMetroidCutsceneFlightDefinitions.DeathFallDrag), Velocity.Y + BabyMetroidCutsceneFlightDefinitions.DeathFallGravity);
    }

    /// <summary>Advances the Baby's black-fade shade every nine calls after its center reaches Y=128, selecting the shade it shows.</summary>
    private bool StepFadeToBlack(ref int? fadeShade)
    {
        // No timer changes occur until the actor's centre reaches Y 128.
        if (CoordinateAxis.Difference(Position.Y.Whole, 128) < 0)
            return false;

        if (FadeToBlackTimer.StepKeepsRunning())
            return false;

        FadeToBlackTimer.Count = 8;
        int nextIndex = FadeToBlackShade + 1;
        if (nextIndex >= 7)
            return true;

        FadeToBlackShade = nextIndex;
        fadeShade = nextIndex;
        return false;
    }

    /// <summary>Emits a dust explosion every five calls using the shared Ridley scatter pattern.</summary>
    private DustExplosionRequest? StepDeathExplosion()
    {
        if (DeathExplosionTimer.StepKeepsRunning())
            return null;

        DeathExplosionTimer.Count = 4;
        DeathExplosionPatternIndex++;
        if (DeathExplosionPatternIndex >= DeathExplosionScatterDefinitions.Count)
            DeathExplosionPatternIndex = 0;
        // The death explosions hold the scatter shared with Ridley's death. The explosion step advances its
        // index before looking up a pair, so a freshly cleared counter begins at the scatter's second place.
        return new DustExplosionRequest(
            Position: Position.Whole + DeathExplosionScatter.Offset(DeathExplosionPatternIndex),
            Animation: MiscDustAnimation.SmallExplosion,
            SoundEffect: MotherBrainSoundDefinitions.Explosion);
    }

    /// <summary>Switches the Baby's animation and restarts its frame timer.</summary>
    private void SetAnimation(MotherBrainBabySegment segment)
    {
        Animation = segment;
        FrameHold = 1;
    }

    /// <summary>
    /// Moves the speed toward <paramref name="targetSpeed"/> and turns the heading clockwise by
    /// <paramref name="clockwiseTurn"/> toward <paramref name="targetHeading"/>, each stopping at its target, before
    /// deriving the X/Y velocity from the heading's sine values. The heading grows counterclockwise on screen, from
    /// straight down toward the right, so both curves turn it back.
    /// </summary>
    private void UpdateSpeedAndHeading(
        RotationAngle clockwiseTurn,
        RotationAngle targetHeading,
        SignedEightEight targetSpeed)
    {
        // The speed changes by its step and clamps rather than crossing the target.
        if (Speed != targetSpeed)
        {
            Speed = targetSpeed < Speed
                ? SignedEightEight.Max(Speed - BabyMetroidCutsceneFlightDefinitions.CurveSpeedStep, targetSpeed)
                : SignedEightEight.Min(Speed + BabyMetroidCutsceneFlightDefinitions.CurveSpeedStep, targetSpeed);
        }

        // The turn stops at the target heading: the signed difference from the target says
        // whether this step would carry the heading past it.
        RotationAngle candidate = Angle - clockwiseTurn;
        Angle = candidate.TurnFrom(targetHeading) >= 0
            ? candidate
            : targetHeading;

        TrigonometryAngle angle = Angle.ToTrigonometryAngle();
        Velocity = new(
            CalculateVelocityComponent(Speed, angle),
            CalculateVelocityComponent(Speed, angle.TurnedBy(TrigonometryAngle.QuarterTurn)));
    }

    /// <summary>Multiplies speed by the angle's sine, rounding the product down to a 256th of a pixel, as the game does.</summary>
    /// <param name="speed">The speed, taken without its sign.</param>
    /// <param name="angle">The angle in steps, 256ths of a turn.</param>
    internal static SignedEightEight CalculateVelocityComponent(
        SignedEightEight speed,
        TrigonometryAngle angle)
    {
        // The speed is multiplied by the sine's size and the product's sign is the sine's.
        return Trigonometry.MultiplySignedSine(speed, angle);
    }

    /// <summary>Moves the Baby by its velocity, in 256ths of a pixel, leaving the position's finer-than-256ths part untouched.</summary>
    private void MoveAccordingToVelocity()
    {
        // The velocity moves the position in 256ths of a pixel: a fraction past a whole pixel
        // carries into the whole pixels, and the position's finer part is unchanged.
        Position = Position.Advance(Velocity.ToSixteenSixteen());
    }

    /// <summary>
    /// The ordinary enemy-versus-Samus contact check, for the Baby's radius hitbox.
    /// A Samus whose body is an attack has her invincibility cleared and proceeds; otherwise an
    /// invincible Samus is not touched. Each axis then requires
    /// <c>|delta| - Samus's radius &lt; enemy radius</c> (a delta inside Samus's own radius
    /// always passes), which is strict, unlike the rectangle check other actors use.
    /// </summary>
    private bool TouchesSamus(SamusState samus)
    {
        if (samus.HorizontalSpeed.BodyAttack != SamusBodyAttack.None)
            samus.InvincibilityTimer = WrappingCountdown.Start(0);
        else if (!samus.InvincibilityTimer.IsZero)
            return false;
        return AxisTouches(samus.Position.X.Whole, Position.X.Whole, samus.Kinematics.Radius.X, XHitboxRadius) &&
            AxisTouches(samus.Position.Y.Whole, Position.Y.Whole, samus.Kinematics.Radius.Y, YHitboxRadius);

        static bool AxisTouches(int samusPosition, int enemyPosition, int samusRadius, int enemyRadius)
        {
            int distance = CoordinateAxis.Distance(samusPosition, enemyPosition);
            return distance < samusRadius || distance - samusRadius < enemyRadius;
        }
    }

    /// <summary>Uses the Baby's 36-pixel radii and inclusive rectangle overlap for route targets.</summary>
    private bool CollidesWithRectangle(
        Position16<RoomPixels> center,
        Extent16<RoomPixels> rectangleRadius)
    {
        // The rectangles touch when, on each axis, the distance between their centres is at most their radii added.
        int xDistance = CoordinateAxis.Distance(center.X, Position.X.Whole);
        if (xDistance > rectangleRadius.X + XHitboxRadius)
            return false;
        int yDistance = CoordinateAxis.Distance(center.Y, Position.Y.Whole);
        return yDistance <= rectangleRadius.Y + YHitboxRadius;
    }

    /// <summary>Accelerates toward a target using axis helpers and the offscreen wrong-way X correction.</summary>
    private void GraduallyAccelerateTowardsPoint(
        Position16<RoomPixels> target,
        int accelerationDivisor,
        SignedEightEight wrongWayOffScreenXSpeed,
        Position16<RoomPixels> cameraPosition)
    {
        Velocity = Velocity.WithX(BabyMetroidGradualAcceleration.AccelerateHorizontally(
            Position.X.Whole, target.X, Velocity.X, accelerationDivisor, wrongWayOffScreenXSpeed,
            () => IsVaguelyOffScreen(cameraPosition)));
        Velocity = Velocity.WithY(BabyMetroidGradualAcceleration.AccelerateVertically(
            Position.Y.Whole, target.Y, Velocity.Y, accelerationDivisor));
    }

    /// <summary>Checks the generous signed camera rectangle used by the Baby's wrong-way movement recovery.</summary>
    private bool IsVaguelyOffScreen(Position16<RoomPixels> cameraPosition)
    {
        // This is the signed-branch sequence; its generous rectangle extends
        // 16 pixels left/right and 96 pixels vertically beyond the ordinary 256x224 view.
        if (CoordinateAxis.AsSigned(Position.Y.Whole) < 0)
            return true;
        int relativeY = CoordinateAxis.AsSigned(Position.Y.Whole + 96 - cameraPosition.Y);
        if (relativeY is < 0 or >= 416)
            return true;
        if (CoordinateAxis.AsSigned(Position.X.Whole) < 0)
            return true;
        int relativeX = CoordinateAxis.AsSigned(Position.X.Whole + 16 - cameraPosition.X);
        return relativeX is < 0 or >= 288;
    }

    /// <summary>Predicts both axis crossings and reports arrival only when X and Y reach their targets together.</summary>
    private bool AccelerateTowardsPoint(Position16<RoomPixels> target, SignedEightEight acceleration)
    {
        // The prediction counts axes whose next whole-pixel prediction reaches/crosses target,
        // and reports arrival only when both axes count.
        bool reachedX = AccelerateTowardsXPosition(target.X, acceleration);
        bool reachedY = AccelerateTowardsYPosition(target.Y, acceleration);
        return reachedX && reachedY;
    }

    /// <summary>Accelerates Y toward its target at at most five pixels per frame and zeroes velocity on crossing.</summary>
    private bool AccelerateTowardsYPosition(int targetY, SignedEightEight acceleration)
    {
        int difference = CoordinateAxis.Difference(Position.Y.Whole, targetY);
        if (difference == 0)
            return true;

        // Each step clamps the speed to five pixels an update, so adding the acceleration stays far inside its range.
        if (difference < 0)
        {
            Velocity = Velocity.WithY(SignedEightEight.Min(Velocity.Y + acceleration, BabyMetroidCutsceneFlightDefinitions.ApproachMaximumSpeed));
            int prediction = CoordinateAxis.Wrap(Position.Y.Whole + Velocity.Y.WholePixels);
            if (CoordinateAxis.Difference(prediction, targetY) < 0)
                return false;
        }
        else
        {
            Velocity = Velocity.WithY(SignedEightEight.Max(Velocity.Y - acceleration, -BabyMetroidCutsceneFlightDefinitions.ApproachMaximumSpeed));
            int prediction = CoordinateAxis.Wrap(Position.Y.Whole + Velocity.Y.WholePixels);
            int predictedDifference = CoordinateAxis.Difference(prediction, targetY);
            if (predictedDifference > 0)
                return false;
        }

        Velocity = Velocity.WithY(default);
        return true;
    }

    /// <summary>Accelerates X toward its target with equality choosing the left branch and zeroes velocity on crossing.</summary>
    private bool AccelerateTowardsXPosition(int targetX, SignedEightEight acceleration)
    {
        int difference = CoordinateAxis.Difference(Position.X.Whole, targetX);
        if (difference < 0)
        {
            Velocity = Velocity.WithX(SignedEightEight.Min(Velocity.X + acceleration, BabyMetroidCutsceneFlightDefinitions.ApproachMaximumSpeed));
            int prediction = CoordinateAxis.Wrap(Position.X.Whole + Velocity.X.WholePixels);
            if (CoordinateAxis.Difference(prediction, targetX) < 0)
                return false;
        }
        else
        {
            // Deliberately sends exact equality through the left branch; unlike Y,
            // there is no equality test before the check for a negative difference.
            Velocity = Velocity.WithX(SignedEightEight.Max(Velocity.X - acceleration, -BabyMetroidCutsceneFlightDefinitions.ApproachMaximumSpeed));
            int prediction = CoordinateAxis.Wrap(Position.X.Whole + Velocity.X.WholePixels);
            int predictedDifference = CoordinateAxis.Difference(prediction, targetX);
            if (predictedDifference > 0)
                return false;
        }

        Velocity = Velocity.WithX(default);
        return true;
    }

    /// <summary>The 4x4 half-extent rectangle every route and staging target is tested with.</summary>
    private static Extent16<RoomPixels> TargetRadius => new(4, 4);
}

