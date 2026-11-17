using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The Baby Metroid cutscene entrance, Mother Brain drain,
/// release, ceiling retreat, route to Samus, latch, and healing, and the movement they share.
/// </summary>
/// <remarks>
/// The entrance moves by a velocity in 256ths of a pixel, adding it to the position's 256ths of a pixel and carrying
/// into its whole pixels, and the curved entrance scales by the game's sine
/// (<see cref="Trigonometry.MultiplySignedSine"/>). Those steps decide the update on which the Baby's hit area reaches
/// Mother Brain's brain, so a smooth curve that looks the same would change it.
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

    /// <summary>
    /// Whether the onion-ring hit flash shows the shared white colours this update instead of
    /// the Baby's own sprite colours.
    /// </summary>
    public bool HurtFlashShown { get; private set; }

    /// <summary>
    /// The Baby's animation and where it stands (#2319): it arrives playing its opening pulse, which its setup begins, and
    /// each animation a step starts (<see cref="BabyMetroidCutsceneStepResult.StartedAnimation"/>) replaces it.
    /// </summary>
    internal AnimationPlayback<MotherBrainBabySegment> Playback { get; set; } = MotherBrainBabyAnimations.Starting(MotherBrainBabySegment.Initial);

    /// <summary>
    /// Room position. This actor's 1/256-pixel mover moves it in 256ths of a pixel; its precision finer
    /// than a 256th of a pixel is deliberately kept unchanged.
    /// </summary>
    public FixedPosition<RoomPixels> Position { get; internal set; }

    /// <summary>
    /// Where the Baby lies from the room's origin: flying the wrong way off screen it may pass above or left of the room,
    /// where it lies at a negative height or column. Every comparison of its place reads this.
    /// </summary>
    internal Offset16<RoomPixels> SignedPlace => new(CoordinateAxis.AsSigned(Position.X.Whole), CoordinateAxis.AsSigned(Position.Y.Whole));

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

    /// <summary>The wait between the images of the Baby's fade to black.</summary>
    public BossCountdown FadeToBlackTimer { get; } = new();

    /// <summary>The images of its fade to black the Baby has shown, zero through all six (#2270).</summary>
    public int FadeToBlackImagesShown { get; private set; }

    /// <summary>The wait between the dust explosions surrounding the dying Baby.</summary>
    public BossCountdown DeathExplosionTimer { get; } = new();

    /// <summary>
    /// Where the Baby's next death explosion bursts (#2315): each takes the next point of the scatter shared with Ridley's
    /// death, the first after the last, starting from the scatter's second point.
    /// </summary>
    public ListStep<Offset16<RoomPixels>> NextDeathExplosion { get; private set; } = FirstDeathExplosion;

    /// <summary>Where the Baby's first death explosion bursts: the shared scatter's second point.</summary>
    private static ListStep<Offset16<RoomPixels>> FirstDeathExplosion => ListStep<Offset16<RoomPixels>>.At(DeathExplosionScatterDefinitions.Offsets, 1);

    /// <summary>
    /// The updates the cutscene has spent readying Mother Brain's attacks again so far, zero through
    /// <see cref="MotherBrainReadyingDefinitions.AttacksAgainDuration"/>.
    /// </summary>
    public int AttacksAgainReadyingElapsed { get; private set; }

    /// <summary>How far the room lights' recovery has come (#2316): each update of the final cutscene shows its next image.</summary>
    public MotherBrainFadeProgress RecoveryLights { get; } = new();

    /// <summary>
    /// The part of a rainbow colour frame accumulated toward slowing Samus's rainbow: each update adds to it, and every
    /// completed whole frame holds her rainbow's glow longer.
    /// </summary>
    public RainbowFramePart SamusRainbowFraction { get; private set; }

    /// <summary>Which of the two rainbow colour cycles Samus currently shows.</summary>
    public BabyMetroidSamusRainbowPhase SamusRainbowPhase { get; private set; }

    /// <summary>True once the Baby has deleted itself at the end of the cutscene (<see cref="BabyMetroidCutscenePhase.Deleted"/>).</summary>
    public bool IsDeleted => Phase == BabyMetroidCutscenePhase.Deleted;

    /// <summary>
    /// Initializes the Baby. Its place keeps how it meets Samus, what the room runs for it and whether it is drawn
    /// (#2310); initialization overwrites the population coordinates and waits at its arrival point before beginning
    /// the dash, from the fractions of a pixel its place keeps (<see cref="BabyMetroidArrivalFractions"/>).
    /// </summary>
    /// <param name="leftInPlace">The position the Baby's place still holds from its previous occupant.</param>
    public void Initialize(FixedPosition<RoomPixels> leftInPlace)
    {
        HurtFlashShown = false;
        RouteLeg = null;
        Health = BabyMetroidCutsceneDefinitions.ArrivalHealth;
        OnionRingHitFlashTimer = 0;
        FatalBlowOrigin = Position16<RoomPixels>.Zero;
        FadeToBlackTimer.LastFor(1);
        FadeToBlackImagesShown = 0;
        DeathExplosionTimer.LastFor(1);
        NextDeathExplosion = FirstDeathExplosion;
        AttacksAgainReadyingElapsed = 0;
        RecoveryLights.Restart();
        SamusRainbowFraction = RainbowFramePart.None;
        SamusRainbowPhase = BabyMetroidSamusRainbowPhase.Inactive;
        Position = BabyMetroidArrivalFractions.StartingPosition(BabyMetroidCutsceneFlightDefinitions.ArrivalPoint, leftInPlace);
        Velocity = default;
        Speed = SignedEightEight.Zero;
        Angle = RotationAngle.Zero;
        Phase = BabyMetroidCutscenePhase.DashOntoScreen;
        PhaseTimer.LastFor(BabyMetroidCutsceneDefinitions.ArrivalWaitUpdates);
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
        var releaseDustClouds = new List<BabyMetroidReleaseDustRequest>(capacity: BabyMetroidCutsceneDefinitions.ReleaseDustOffsets.Count);
        bool ambientCrySoundQueued = false;
        bool samusRainbowActivated = false;
        DustExplosionRequest? deathExplosion = null;
        MotherBrainFadeImage? babyFadeImage = null;
        MotherBrainFadeImage? recoveryLightsImage = null;
        MotherBrainBabySegment? startedAnimation = null;
        BabyMetroidVisibilityRequest visibility = BabyMetroidVisibilityRequest.Unchanged;

        // Enemy collision handling runs touch AI before main AI, against the Baby's
        // position from the previous frame. The Baby's touch is active only while it latches onto Samus; broad
        // Samus/enemy hitboxes gate it, then its own divisor-16 acceleration decides when
        // both axes have reached the latch point. Main AI and its mover follow this frame.
        if (Phase == BabyMetroidCutscenePhase.LatchOntoSamus && TouchesSamus(samus))
        {
            bool reachedLatchPoint = AccelerateTowardsPoint(
                samus.Position.Whole + BabyMetroidCutsceneFlightDefinitions.SamusLatchOffset,
                acceleration: BabyMetroidCutsceneFlightDefinitions.SamusLatchAcceleration);
            if (reachedLatchPoint)
            {
                Velocity = default;
                Phase = BabyMetroidCutscenePhase.HealSamusToFullHealth;
            }
        }

        switch (Phase)
        {
            case BabyMetroidCutscenePhase.DashOntoScreen:
                // The Baby holds still through its arrival wait. The update the wait runs out on immediately executes the
                // first curve update before the common mover consumes its new velocity.
                if (PhaseTimer.StepRunsOut())
                {
                    Angle = BabyMetroidCutsceneFlightDefinitions.CurveStartHeading;
                    Speed = BabyMetroidCutsceneFlightDefinitions.CurveSpeed;
                    Phase = BabyMetroidCutscenePhase.CurveTowardMotherBrainHead;
                    PhaseTimer.LastFor(BabyMetroidCutsceneDefinitions.CurveUpdates);
                    goto case BabyMetroidCutscenePhase.CurveTowardMotherBrainHead;
                }
                break;

            case BabyMetroidCutscenePhase.CurveTowardMotherBrainHead:
                UpdateSpeedAndHeading(BabyMetroidCutsceneFlightDefinitions.CurveTurn, BabyMetroidCutsceneFlightDefinitions.CurveTargetHeading, BabyMetroidCutsceneFlightDefinitions.CurveSpeed);
                if (PhaseTimer.StepRunsOut())
                {
                    Phase = BabyMetroidCutscenePhase.GetRightUpInMotherBrainsFace;
                    PhaseTimer.LastFor(BabyMetroidCutsceneDefinitions.FaceApproachUpdates);
                }
                break;

            case BabyMetroidCutscenePhase.GetRightUpInMotherBrainsFace:
                UpdateSpeedAndHeading(BabyMetroidCutsceneFlightDefinitions.FaceTurn, BabyMetroidCutsceneFlightDefinitions.FaceTargetHeading, BabyMetroidCutsceneFlightDefinitions.FaceSpeed);
                brainCollision = CollidesWithRectangle(
                    head,
                    BabyMetroidCutsceneFlightDefinitions.TargetReach);

                // Collision skips the approach's step. Otherwise the approach ends on its last update.
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
                    accelerationDivisor: BabyMetroidCutsceneFlightDefinitions.GentleAccelerationDivisor,
                    BabyMetroidCutsceneFlightDefinitions.ApproachWrongWaySpeed,
                    cameraPosition);
                brainCollision = CollidesWithRectangle(target, BabyMetroidCutsceneFlightDefinitions.BrainLatchReach);
                if (brainCollision)
                    Phase = BabyMetroidCutscenePhase.SetMotherBrainToStumbleBack;
                break;
            }

            case BabyMetroidCutscenePhase.SetMotherBrainToStumbleBack:
                // The drain asks her to stumble back a pixel; standing, she always starts her really-fast backward
                // walk.
                motherBrain.RequestBabyStumbleBackward();
                Phase = BabyMetroidCutscenePhase.ActivateRainbowBeamAndMotherBrainBody;
                goto case BabyMetroidCutscenePhase.ActivateRainbowBeamAndMotherBrainBody;

            case BabyMetroidCutscenePhase.ActivateRainbowBeamAndMotherBrainBody:
            {
                Position16<RoomPixels> target = head + BabyMetroidCutsceneFlightDefinitions.BrainLatchOffset;
                bool reachedTarget = AccelerateTowardsPoint(target, acceleration: BabyMetroidCutsceneFlightDefinitions.BrainDrainAcceleration);
                if (reachedTarget)
                {
                    // The approach only tells whether the Baby would pass its target's whole pixel; the drain then pins
                    // the Baby exactly to the target.
                    Velocity = default;
                    Position = Position.WithWhole(target);
                    startedAnimation = MotherBrainBabySegment.DrainingMotherBrain;
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
                    PhaseTimer.LastFor(BabyMetroidCutsceneDefinitions.StopDrainingUpdates);
                }
                break;
            }

            case BabyMetroidCutscenePhase.StopDraining:
                // Shaking ceases immediately: every update of the wait pins the Baby to the brain
                // before stepping the wait.
                Position = Position.WithWhole(head + BabyMetroidCutsceneFlightDefinitions.BrainLatchOffset);
                if (PhaseTimer.StepRunsOut())
                {
                    startedAnimation = MotherBrainBabySegment.Initial;
                    Phase = BabyMetroidCutscenePhase.LetGoAndSpawnDustClouds;
                    PhaseTimer.LastFor(BabyMetroidCutsceneDefinitions.LetGoUpdates);
                    Velocity = default;
                }
                break;

            case BabyMetroidCutscenePhase.LetGoAndSpawnDustClouds:
                // The release runs on into its acceleration on every update. Thus the Baby accelerates
                // ceilingward through the whole release; the dust burst happens on its last update, which
                // also accelerates.
                if (PhaseTimer.StepRunsOut())
                {
                    // The three dust clouds on Mother Brain's head are spawned in this
                    // exact order. Each cloud adds its signed offsets to the brain
                    // enemy's current room coordinate and spawns a small dust cloud
                    // in the highest free place for an enemy projectile.
                    foreach (Offset16<RoomPixels> dust in BabyMetroidCutsceneDefinitions.ReleaseDustOffsets)
                        releaseDustClouds.Add(new BabyMetroidReleaseDustRequest(head + dust, Animation: MiscDustAnimation.SmallDustCloud));
                    Phase = BabyMetroidCutscenePhase.MoveToTheCeiling;
                }
                goto case BabyMetroidCutscenePhase.MoveToTheCeiling;

            case BabyMetroidCutscenePhase.MoveToTheCeiling:
            {
                Position16<RoomPixels> target = head.WithY(0);
                GraduallyAccelerateTowardsPoint(
                    target,
                    accelerationDivisor: BabyMetroidCutsceneFlightDefinitions.GentleAccelerationDivisor,
                    BabyMetroidCutsceneFlightDefinitions.ApproachWrongWaySpeed,
                    cameraPosition);
                bool ceilingCollision = CollidesWithRectangle(target, BabyMetroidCutsceneFlightDefinitions.TargetReach);
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
                // The route step enables health-based colour selection on every route update. The ambient cry takes its
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
                if (CollidesWithRectangle(target, BabyMetroidCutsceneFlightDefinitions.TargetReach))
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
                    accelerationDivisor: BabyMetroidCutsceneFlightDefinitions.GentleAccelerationDivisor,
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
                    Health = BabyMetroidCutsceneDefinitions.FinalChargeHealth;
                    Phase = BabyMetroidCutscenePhase.ReleaseSamus;
                }
                break;

            case BabyMetroidCutscenePhase.ReleaseSamus:
                // The next step acts on both actors in one update. The speed it gives
                // Mother Brain's brain is presentation state; the meaningful body change
                // starts Mother Brain's fast retreat, and the Baby's flight takes its first update in this same
                // update.
                motherBrain.PrepareForFinalBabyMetroidAttack();
                Phase = BabyMetroidCutscenePhase.StareDownMotherBrain;
                goto case BabyMetroidCutscenePhase.StareDownMotherBrain;

            case BabyMetroidCutscenePhase.StareDownMotherBrain:
                // The target is four pixels left of Samus, at a fixed height. This leg and the next two
                // reverse off screen with the staging wrong-way speed, not the approaches' one.
            {
                var target = new Position16<RoomPixels>(samus.Position.X.Whole + BabyMetroidCutsceneFlightDefinitions.StareDownOffsetX, BabyMetroidCutsceneFlightDefinitions.StareDownY);
                GraduallyAccelerateTowardsPoint(
                    target,
                    accelerationDivisor: BabyMetroidCutsceneFlightDefinitions.GentleAccelerationDivisor,
                    BabyMetroidCutsceneFlightDefinitions.StagingWrongWaySpeed,
                    cameraPosition);
                if (CollidesWithRectangle(target, BabyMetroidCutsceneFlightDefinitions.TargetReach))
                {
                    Phase = BabyMetroidCutscenePhase.FlyToRightEdge;
                }
                break;
            }

            case BabyMetroidCutscenePhase.FlyToRightEdge:
                // The leg targets a fixed point just past the right edge of the screen.
                GraduallyAccelerateTowardsPoint(
                    BabyMetroidCutsceneFlightDefinitions.RightEdgePoint,
                    accelerationDivisor: BabyMetroidCutsceneFlightDefinitions.GentleAccelerationDivisor,
                    BabyMetroidCutsceneFlightDefinitions.StagingWrongWaySpeed,
                    cameraPosition);
                if (CollidesWithRectangle(BabyMetroidCutsceneFlightDefinitions.RightEdgePoint, BabyMetroidCutsceneFlightDefinitions.TargetReach))
                    Phase = BabyMetroidCutscenePhase.MoveToFinalChargeStart;
                break;

            case BabyMetroidCutscenePhase.MoveToFinalChargeStart:
                // The final approach stages the Baby at the charge's start. Reaching that rectangle assigns
                // the final volley's health so the final four-ring volley kills on its first collision,
                // clears the brain's speed flag, and replaces Mother Brain's retreat AI.
                GraduallyAccelerateTowardsPoint(
                    BabyMetroidCutsceneFlightDefinitions.FinalChargeStart,
                    accelerationDivisor: BabyMetroidCutsceneFlightDefinitions.GentleAccelerationDivisor,
                    BabyMetroidCutsceneFlightDefinitions.StagingWrongWaySpeed,
                    cameraPosition);
                if (CollidesWithRectangle(BabyMetroidCutsceneFlightDefinitions.FinalChargeStart, BabyMetroidCutsceneFlightDefinitions.TargetReach))
                {
                    Health = BabyMetroidCutsceneDefinitions.FinalVolleyHealth;
                    motherBrain.ExecuteFinalBabyMetroidAttack();
                    Phase = BabyMetroidCutscenePhase.InitiateFinalCharge;
                }
                break;

            case BabyMetroidCutscenePhase.InitiateFinalCharge:
                // The charge's start divides the distance by six, a much firmer pull than the staging legs' sixteen,
                // and reverses off screen with the approaches' wrong-way speed.
                GraduallyAccelerateTowardsPoint(
                    BabyMetroidCutsceneFlightDefinitions.FinalChargeAim,
                    accelerationDivisor: BabyMetroidCutsceneFlightDefinitions.ChargeStartDivisor,
                    BabyMetroidCutsceneFlightDefinitions.ApproachWrongWaySpeed,
                    cameraPosition);
                if (CollidesWithRectangle(BabyMetroidCutsceneFlightDefinitions.FinalChargeAim, BabyMetroidCutsceneFlightDefinitions.TargetReach))
                    Phase = BabyMetroidCutscenePhase.FinalCharge;
                break;

            case BabyMetroidCutscenePhase.FinalCharge:
                // The final target follows Mother Brain's brain, 32 pixels above its
                // centre. Its divisor of four accelerates much more aggressively than the staging leg.
                GraduallyAccelerateTowardsPoint(
                    head + BabyMetroidCutsceneFlightDefinitions.FinalChargeOffset,
                    accelerationDivisor: BabyMetroidCutsceneFlightDefinitions.FinalChargeDivisor,
                    BabyMetroidCutsceneFlightDefinitions.ApproachWrongWaySpeed,
                    cameraPosition);
                if (Health == 0)
                {
                    // The fatal-blow step changes graphics/animation state, freezes the body AI,
                    // clears velocity, remembers the shake origin, and takes the first of its 17 updates of shaking in
                    // this same update.
                    startedAnimation = MotherBrainBabySegment.TakingFatalBlow;
                    Velocity = default;
                    Phase = BabyMetroidCutscenePhase.TakeFinalBlow;
                    PhaseTimer.LastFor(BabyMetroidCutsceneDefinitions.FatalBlowShakeUpdates);
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
                    PhaseTimer.LastFor(BabyMetroidCutsceneDefinitions.SamusThemeUpdates);
                    goto case BabyMetroidCutscenePhase.PlaySamusTheme;
                }
                break;
            }

            case BabyMetroidCutscenePhase.PlaySamusTheme:
                // The theme's last update queues the two delayed music requests and is immediately
                // the first update of the Hyper Beam's preparation.
                if (PhaseTimer.StepRunsOut())
                {
                    Phase = BabyMetroidCutscenePhase.PrepareSamusForHyperBeam;
                    PhaseTimer.LastFor(BabyMetroidCutsceneDefinitions.HyperBeamPreparationUpdates);
                    goto case BabyMetroidCutscenePhase.PrepareSamusForHyperBeam;
                }
                break;

            case BabyMetroidCutscenePhase.PrepareSamusForHyperBeam:
                if (PhaseTimer.StepRunsOut())
                {
                    // The Hyper Beam acquisition freezes Samus's drained animation on its Hyper Beam acquisition frame.
                    // The Hyper Beam step also starts the first rainbow step; neither takes effect until the Baby's next
                    // update.
                    SamusDrainedState.FreezeForHyperBeamAcquisition(samus);
                    SamusRainbowPhase = BabyMetroidSamusRainbowPhase.ActivateWhenEnemyIsLow;
                    Phase = BabyMetroidCutscenePhase.DeathSequence;
                }
                break;

            case BabyMetroidCutscenePhase.DeathSequence:
                StepSamusRainbowColours(samus, ref samusRainbowActivated);
                AccelerateDownwardsForDeath();
                if (StepFadeToBlack(ref babyFadeImage))
                {
                    // Completion does not write the Baby's eighth colour. It hides the actor,
                    // and waits out the pause before it readies Mother Brain's attacks again.
                    visibility = BabyMetroidVisibilityRequest.Hide;
                    Phase = BabyMetroidCutscenePhase.ReadyMotherBrainAttacksAgain;
                    PhaseTimer.LastFor(BabyMetroidCutsceneDefinitions.BeforeReadyingAttacksUpdates);
                }
                else
                {
                    deathExplosion = StepDeathExplosion();
                    // The flicker step shows the Baby on odd enemy frames and hides it on even.
                    visibility = MotherBrainCadenceDefinitions.BabyDeathFlickerShown.IsSetIn(enemyFrameCounter)
                        ? BabyMetroidVisibilityRequest.Show
                        : BabyMetroidVisibilityRequest.Hide;
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
                        // Letting Samus rainbow some more starts in the last readying update, which is its first.
                        Phase = BabyMetroidCutscenePhase.LetSamusRainbowSomeMore;
                        PhaseTimer.LastFor(BabyMetroidCutsceneDefinitions.RainbowSomeMoreUpdates);
                        goto case BabyMetroidCutscenePhase.LetSamusRainbowSomeMore;
                    }
                }
                break;

            case BabyMetroidCutscenePhase.LetSamusRainbowSomeMore:
                if (PhaseTimer.StepRunsOut())
                {
                    Phase = BabyMetroidCutscenePhase.FinalCutscene;
                    RecoveryLights.Restart();
                    goto case BabyMetroidCutscenePhase.FinalCutscene;
                }
                break;

            case BabyMetroidCutscenePhase.FinalCutscene:
            {
                // Each update shows the next image of the room lights' recovery; the update after the last ends the
                // cutscene.
                if (MotherBrainFadeDefinitions.RecoveryLights.ImageAt(RecoveryLights.TakeStep()) is { } lights)
                {
                    recoveryLightsImage = lights;
                    break;
                }

                motherBrain.BeginPhase3RecoveryFromBabyCutscene();
                samus.Drained.DisableRainbowAndStartStandingAnimation(samus);
                samus.Drained.EnableHyperBeam(samus);
                Phase = BabyMetroidCutscenePhase.Deleted;
                break;
            }

            case BabyMetroidCutscenePhase.Inactive:
            case BabyMetroidCutscenePhase.Deleted:
            default:
                throw new InvalidOperationException($"Unsupported Baby Metroid phase {Phase}.");
        }

        // The Baby moves by its velocity on every update, its phase changes and its snaps to a target included; a snap
        // zeroes both velocities, so the Baby stays exactly on its target.
        MoveAccordingToVelocity();

        // After the Baby moves, its hit flash counts down the timer an onion ring's hit sets, and shows the flash colours
        // on the third and fourth update of each four. A hit sets sixteen: the next update shakes the Baby at sixteen,
        // then shows its normal colours at fifteen.
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
            babyFadeImage,
            recoveryLightsImage,
            startedAnimation,
            visibility);
    }

    /// <summary>Applies the Baby's health/flash mutation from the blue-ring collision response: a living Baby gets a sixteen-frame flash timer and health subtraction saturated at zero.</summary>
    /// <param name="damage">The ring's damage (<see cref="MotherBrainAttackDamage.OnionRing"/>), zero or more; even zero damage refreshes the flash timer when health is nonzero.</param>
    /// <remarks>Zero health leaves all state unchanged. The ring's touch establishes the overlap, and the cutscene requests the Baby's cry, copies the Baby's health to its enemy and explodes and deletes the ring.</remarks>
    public void ApplyMotherBrainOnionRingHit(int damage)
    {
        int healthBefore = Health;
        if (healthBefore == 0)
            return;

        OnionRingHitFlashTimer = BabyMetroidCutsceneDefinitions.OnionRingHitFlash;
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
                // Samus's rainbow starts once the Baby has come down to no more than its margin above her.
                if (SignedPlace.Y + BabyMetroidCutsceneDefinitions.SamusRainbowAboveBaby < samus.Position.Y.Whole)
                    return;
                samus.Drained.EnableRainbow(samus);
                samusRainbowActivated = true;
                SamusRainbowPhase = BabyMetroidSamusRainbowPhase.GraduallySlowAnimationDown;
                return;

            case BabyMetroidSamusRainbowPhase.GraduallySlowAnimationDown:
            {
                (RainbowFramePart fraction, bool frameCompleted) = SamusRainbowFraction.AdvancedByOneUpdate();
                SamusRainbowFraction = fraction;
                if (frameCompleted)
                    samus.Drained.HoldRainbowGlowLonger(longestHold: BabyMetroidCutsceneDefinitions.SamusRainbowLongestHold);
                return;
            }

            default:
                throw new InvalidOperationException($"Unsupported Samus rainbow phase {SamusRainbowPhase}.");
        }
    }

    /// <summary>Slows the horizontal speed by the death fall's drag and adds its gravity to the vertical speed.</summary>
    private void AccelerateDownwardsForDeath()
    {
        // The slowing takes the drag from the speed, stopping at rest and keeping its direction, and adds the gravity
        // to the vertical velocity. The fall ends 63 updates after the Baby's center reaches Y 128, so its speed stays
        // below 33 pixels an update.
        Velocity = new(Velocity.X.SlowedBy(BabyMetroidCutsceneFlightDefinitions.DeathFallDrag), Velocity.Y + BabyMetroidCutsceneFlightDefinitions.DeathFallGravity);
    }

    /// <summary>
    /// Shows the next image of the Baby's fade to black every nine updates after its center reaches Y=128; true on the update
    /// after its last image, which completes the fade.
    /// </summary>
    private bool StepFadeToBlack(ref MotherBrainFadeImage? fadeImage)
    {
        // No timer changes occur until the actor's centre reaches the fade's line.
        if (SignedPlace.Y < BabyMetroidCutsceneDefinitions.FadeToBlackLineY)
            return false;

        if (FadeToBlackTimer.StepKeepsRunning())
            return false;

        FadeToBlackTimer.LastFor(BabyMetroidCutsceneDefinitions.FadeToBlackStepUpdates);
        if (MotherBrainFadeDefinitions.BabyFade.ImageAt(FadeToBlackImagesShown) is not { } image)
            return true;

        FadeToBlackImagesShown++;
        fadeImage = image;
        return false;
    }

    /// <summary>Emits a dust explosion every five updates using the shared Ridley scatter pattern.</summary>
    private DustExplosionRequest? StepDeathExplosion()
    {
        if (DeathExplosionTimer.StepKeepsRunning())
            return null;

        DeathExplosionTimer.LastFor(BabyMetroidCutsceneDefinitions.DeathExplosionIntervalUpdates);
        Offset16<RoomPixels> scatter = NextDeathExplosion.Entry;
        NextDeathExplosion = NextDeathExplosion.NextInCycle;
        return new DustExplosionRequest(
            Position: Position.Whole + scatter,
            Animation: MiscDustAnimation.SmallExplosion,
            SoundEffect: MotherBrainSoundDefinitions.Explosion);
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
        return AxisTouches(samus.Position.X.Whole, SignedPlace.X, samus.Kinematics.Radius.X, XHitboxRadius) &&
            AxisTouches(samus.Position.Y.Whole, SignedPlace.Y, samus.Kinematics.Radius.Y, YHitboxRadius);

        static bool AxisTouches(int samusPosition, int enemyPosition, int samusRadius, int enemyRadius)
        {
            int distance = Math.Abs(samusPosition - enemyPosition);
            return distance < samusRadius || distance - samusRadius < enemyRadius;
        }
    }

    /// <summary>Uses the Baby's 36-pixel radii and inclusive rectangle overlap for route targets.</summary>
    private bool CollidesWithRectangle(
        Position16<RoomPixels> center,
        Extent16<RoomPixels> rectangleRadius)
    {
        // The rectangles touch when, on each axis, the distance between their centres is at most their radii added.
        int xDistance = Math.Abs(center.X - SignedPlace.X);
        if (xDistance > rectangleRadius.X + XHitboxRadius)
            return false;
        int yDistance = Math.Abs(center.Y - SignedPlace.Y);
        return yDistance <= rectangleRadius.Y + YHitboxRadius;
    }

    /// <summary>Accelerates toward a target axis by axis, with the offscreen wrong-way X correction.</summary>
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
        // The Baby is off screen only outside a generous rectangle: 16 pixels past the view's sides and 96 above and
        // below it.
        if (SignedPlace.Y < 0)
            return true;
        int relativeY = SignedPlace.Y + BabyMetroidFlightWindowDefinitions.VerticalMargin - cameraPosition.Y;
        if (relativeY is < 0 or >= BabyMetroidFlightWindowDefinitions.Height)
            return true;
        if (SignedPlace.X < 0)
            return true;
        int relativeX = SignedPlace.X + BabyMetroidFlightWindowDefinitions.HorizontalMargin - cameraPosition.X;
        return relativeX is < 0 or >= BabyMetroidFlightWindowDefinitions.Width;
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
        int difference = SignedPlace.Y - targetY;
        if (difference == 0)
            return true;

        // Each step clamps the speed to five pixels an update, so adding the acceleration stays far inside its range.
        if (difference < 0)
        {
            Velocity = Velocity.WithY(SignedEightEight.Min(Velocity.Y + acceleration, BabyMetroidCutsceneFlightDefinitions.ApproachMaximumSpeed));
            int prediction = SignedPlace.Y + Velocity.Y.WholePixels;
            if (prediction < targetY)
                return false;
        }
        else
        {
            Velocity = Velocity.WithY(SignedEightEight.Max(Velocity.Y - acceleration, -BabyMetroidCutsceneFlightDefinitions.ApproachMaximumSpeed));
            int prediction = SignedPlace.Y + Velocity.Y.WholePixels;
            int predictedDifference = prediction - targetY;
            if (predictedDifference > 0)
                return false;
        }

        Velocity = Velocity.WithY(default);
        return true;
    }

    /// <summary>Accelerates X toward its target with equality choosing the left branch and zeroes velocity on crossing.</summary>
    private bool AccelerateTowardsXPosition(int targetX, SignedEightEight acceleration)
    {
        int difference = SignedPlace.X - targetX;
        if (difference < 0)
        {
            Velocity = Velocity.WithX(SignedEightEight.Min(Velocity.X + acceleration, BabyMetroidCutsceneFlightDefinitions.ApproachMaximumSpeed));
            int prediction = SignedPlace.X + Velocity.X.WholePixels;
            if (prediction < targetX)
                return false;
        }
        else
        {
            // Deliberately sends exact equality through the left branch; unlike Y,
            // there is no equality test before the check for a negative difference.
            Velocity = Velocity.WithX(SignedEightEight.Max(Velocity.X - acceleration, -BabyMetroidCutsceneFlightDefinitions.ApproachMaximumSpeed));
            int prediction = SignedPlace.X + Velocity.X.WholePixels;
            int predictedDifference = prediction - targetX;
            if (predictedDifference > 0)
                return false;
        }

        Velocity = Velocity.WithX(default);
        return true;
    }

}

