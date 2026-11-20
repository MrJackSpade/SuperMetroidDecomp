using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The Baby Metroid cutscene entrance, Mother Brain drain,
/// release, ceiling retreat, route to Samus, latch, and healing, and the movement they share.
/// </summary>
/// <remarks>
/// The entrance moves by a velocity, its speed and position carrying their fractions of a pixel in their types, and the
/// curved entrance follows the shared trigonometry (<see cref="SpeedAlongAngle.Along"/>). Those steps decide the update
/// on which the Baby's hit area reaches Mother Brain's brain, so a smooth curve that looks the same would change it.
/// </remarks>
[ArchitectureReview("back-reference", "The Baby of Mother Brain's late fight (#2471): the late fight makes and owns its Baby for the rest of the room, and the Baby's steps move the late fight that made it on, so the two share one lifetime.")]
public sealed class BabyMetroidCutsceneState
{
    /// <summary>
    /// Makes the Baby of <paramref name="lateFight"/> in <paramref name="place"/>, the place the final beam's spawn gave it,
    /// both of which it holds from here.
    /// </summary>
    internal BabyMetroidCutsceneState(MotherBrainRainbowBeamAttackSequence lateFight, RoomEnemyPlace place)
    {
        LateFight = Ensure.NotNull(lateFight);
        Place = Ensure.NotNull(place);
    }

    /// <summary>
    /// Mother Brain's late fight, which made the Baby (#2471): the Baby's steps move her on, and it reads whether she lies
    /// as a corpse.
    /// </summary>
    internal MotherBrainRainbowBeamAttackSequence LateFight { get; }

    /// <summary>
    /// The Baby's place in the room's enemy list, which the final beam's spawn gave it and which holds its position and
    /// health: the Baby is an enemy of its own, which Mother Brain's onion rings find there, not an effect her body draws.
    /// </summary>
    private RoomEnemyPlace Place { get; }

    /// <summary>How far the Baby's hit area reaches to either side of its center: 36 room pixels.</summary>
    public const int XHitboxRadius = 36;
    /// <summary>How far the Baby's hit area reaches above and below its center: 36 room pixels.</summary>
    public const int YHitboxRadius = 36;

    /// <summary>The entrance's current phase.</summary>
    public BabyMetroidCutscenePhase Phase { get; private set; } =
        BabyMetroidCutscenePhase.Inactive;

    /// <summary>
    /// Whether the onion-ring hit flash shows the shared white colours this update instead of
    /// the Baby's own sprite colours.
    /// </summary>
    public bool HurtFlashShown => HurtFlash is { IsResting: false, Shown: BabyMetroidHurtLook.Flashing };

    /// <summary>
    /// The last onion-ring hit's flash playing (#2404), each update after the Baby moves, or null before its first hit
    /// (<see cref="BabyMetroidHurtFlashAnimations"/>).
    /// </summary>
    internal AnimationPlayer<BabyMetroidHurtFlashSegment, BabyMetroidHurtLook, NoAnimationCue>? HurtFlash { get; private set; }

    /// <summary>
    /// The Baby's animation and where it stands (#2319): it arrives playing its opening pulse, which its setup begins, and
    /// each animation a step starts (<see cref="BabyMetroidCutsceneStepResult.StartedAnimation"/>) replaces it.
    /// </summary>
    internal AnimationPlayback<MotherBrainBabySegment> Playback { get; set; } = MotherBrainBabyAnimations.Starting(MotherBrainBabySegment.IdlePulse);

    /// <summary>
    /// Room position, which the Baby's place holds (#2572): the Baby's steps move its place, which every other part of
    /// the game reads. The position keeps the fraction of a pixel its place held, which the Baby's moves never change.
    /// </summary>
    public FixedPosition<RoomPixels> Position
    {
        get => Place.Position;
        private set => Place.Position = value;
    }

    /// <summary>
    /// Where the Baby lies from the room's origin: flying the wrong way off screen it may pass above or left of the room,
    /// where it lies at a negative height or column. Every comparison of its place reads this.
    /// </summary>
    internal Offset16<RoomPixels> SignedPlace => OffRoomPlaces.Signed(Position.Whole);

    /// <summary>Velocity in pixels per update.</summary>
    private PixelVelocity<RoomPixels> Velocity { get; set; }

    /// <summary>The curved entrance's speed, zero through fourteen pixels an update.</summary>
    private SignedEightEight Speed { get; set; }

    /// <summary>Heading of the curved entrance; only its whole step is passed to shared component math.</summary>
    private RotationAngle Angle { get; set; }

    /// <summary>Phase timer: each timed phase counts it down and moves on as it runs out (<see cref="BossCountdown"/>).</summary>
    private BossCountdown PhaseTimer { get; } = new();

    /// <summary>
    /// The leg of the ceiling-to-Samus route the Baby steers along once it reaches the ceiling, at its place on the route
    /// (#2470, #2471): the route's first until then, which reaching the ceiling starts it on.
    /// </summary>
    internal ListStep<BabyMetroidRouteLeg> RouteLeg { get; private set; } = BabyMetroidRoute.FirstLeg;

    /// <summary>
    /// Enemy health, which the Baby's place holds (#2572): Mother Brain's onion rings and her drain lower it there, and the
    /// Baby's steps read it there. The Baby arrives with <see cref="BabyMetroidCutsceneDefinitions.ArrivalHealth"/>.
    /// </summary>
    public int Health
    {
        get => Place.Health;
        private set => Place.Health = value;
    }

    /// <summary>
    /// The Baby's shake when Mother Brain's onion ring strikes it (#2469): a hit starts it for its length; while it
    /// shakes the Baby, held on Samus, shows its offset doubled, and each update after the Baby moves counts it down. The
    /// hit's flash plays on its own (<see cref="HurtFlash"/>).
    /// </summary>
    internal MotherBrainShake OnionRingHitShake { get; } = new(MotherBrainCadenceDefinitions.BabyHitShakeStep);

    /// <summary>Saved whole-pixel origin used by the fatal-blow shake.</summary>
    public Position16<RoomPixels> FatalBlowOrigin { get; private set; }

    /// <summary>
    /// The Baby's fade to black (#2270, #2404): from the update its centre reaches the fade's line, an image each fade step
    /// (<see cref="MotherBrainFadeDefinitions.BabyFade"/>).
    /// </summary>
    public MotherBrainFadeProgress FadeToBlack { get; } = new(MotherBrainFadeDefinitions.BabyFade);

    /// <summary>The wait between the dust explosions surrounding the dying Baby.</summary>
    public BossCountdown DeathExplosionTimer { get; } = new();

    /// <summary>
    /// Where the Baby's next death explosion bursts (#2315): each takes the next point of the scatter shared with Ridley's
    /// death, the first after the last, starting from the scatter's first point.
    /// </summary>
    public ListStep<Offset16<RoomPixels>> NextDeathExplosion { get; private set; } = FirstDeathExplosion;

    /// <summary>Where the Baby's first death explosion bursts: the shared scatter's first point.</summary>
    private static ListStep<Offset16<RoomPixels>> FirstDeathExplosion => ListStep<Offset16<RoomPixels>>.FirstOf(DeathExplosionScatterDefinitions.Offsets);

    /// <summary>How far the room lights' recovery has come (#2316): each update of the final cutscene shows its next image.</summary>
    public MotherBrainFadeProgress RecoveryLights { get; } = new(MotherBrainFadeDefinitions.RecoveryLights);

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
    private void Initialize(FixedPosition<RoomPixels> leftInPlace)
    {
        HurtFlash = null;
        RouteLeg = BabyMetroidRoute.FirstLeg;
        Health = BabyMetroidCutsceneDefinitions.ArrivalHealth;
        OnionRingHitShake.Stop();
        FatalBlowOrigin = Position16<RoomPixels>.Zero;
        FadeToBlack.StartAfter(1);
        DeathExplosionTimer.LastFor(1);
        NextDeathExplosion = FirstDeathExplosion;
        RecoveryLights.StartAfter(1);
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
    /// The Baby's setup in its place: it initializes from the position the place still holds from its previous occupant
    /// (<see cref="Initialize"/>).
    /// </summary>
    internal void Arrive() => Initialize(Place.Position);

    /// <summary>
    /// Runs one update of the Baby's main AI: its phase runs, then it moves by its velocity.
    /// Every Baby step reads the coordinates of Mother Brain's head, which the head's AI
    /// has already moved this frame; <paramref name="head"/> carries them.
    /// </summary>
    public BabyMetroidCutsceneStepResult Step(
        SamusState samus,
        Position16<RoomPixels> head,
        Position16<RoomPixels> cameraPosition,
        int enemyFrameCounter,
        int randomNumber)
    {
        Ensure.NotNull(samus);
        if (Phase == BabyMetroidCutscenePhase.Inactive)
            throw new InvalidOperationException("The cutscene Baby Metroid has not been initialized.");

        BabyMetroidCutscenePhase phaseBefore = Phase;
        bool brainCollision;

        bool latchSoundQueued = false;
        // When the Baby finally lets go, three dust clouds rise, each at its own place with its own animation and
        // spawned in turn: when the room's enemy projectiles have nearly no room left, the earlier clouds take it.
        var releaseDustClouds = new List<BabyMetroidReleaseDustRequest>(capacity: BabyMetroidCutsceneDefinitions.ReleaseDustOffsets.Count);
        bool ambientCrySoundQueued = false;
        DustExplosionRequest? deathExplosion = null;
        MotherBrainFadeImage? babyFadeImage = null;
        MotherBrainFadeImage? recoveryLightsImage = null;
        MotherBrainBabySegment? startedAnimation = null;
        BabyMetroidVisibilityRequest visibility = BabyMetroidVisibilityRequest.Unchanged;

        // The Baby's touch is tested before its movement, against the Baby's position from the previous frame. The
        // Baby's touch is active only while it latches onto Samus; broad Samus/enemy hitboxes gate it, then the latch
        // acceleration's arrival on both axes at the latch point starts the healing. The Baby's phase and its move by
        // its velocity follow this update.
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
                // The Baby holds still through its arrival wait. The update the wait runs out on runs the first curve
                // update, and the Baby moves by the new velocity on the same update.
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
                LateFight.RequestBabyStumbleBackward();
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
                    LateFight.InterruptFinalBeamForBabyDrain();
                    latchSoundQueued = true; // The rainbow-drain sound starts as the Baby latches on.
                }
                break;
            }

            case BabyMetroidCutscenePhase.WaitForMotherBrainToTurnToCorpse:
            {
                Position = Position.WithWhole(head + MotherBrainShake.OffsetOn(MotherBrainCadenceDefinitions.BabyShakeStep, enemyFrameCounter)
                    + BabyMetroidCutsceneFlightDefinitions.BrainLatchOffset);
                if (LateFight.State.Phase2CorpseState != MotherBrainPhase2CorpseState.Living)
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
                    startedAnimation = MotherBrainBabySegment.IdlePulse;
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
                        releaseDustClouds.Add(new BabyMetroidReleaseDustRequest(head + dust, Animation: DustExplosionAnimation.SmallDustCloud));
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
                    samus.Drained.CrouchDrained(samus);
                    Phase = BabyMetroidCutscenePhase.MoveToSamus;
                    RouteLeg = BabyMetroidRoute.FirstLeg;
                }
                break;
            }

            case BabyMetroidCutscenePhase.MoveToSamus:
            {
                // The ambient cry takes its own chance each route update.
                ambientCrySoundQueued = MotherBrainRandomDefinitions.BabyMetroidCries.IsTakenBy(randomNumber);

                // The route is its authored legs, each steered to its waypoint with its own wrong-way speed.
                ListStep<BabyMetroidRouteLeg> leg = RouteLeg;
                Position16<RoomPixels> target = leg.Entry.Target;

                GraduallyAccelerateTowardsPoint(
                    target,
                    BabyMetroidRouteDefinitions.AccelerationDivisor,
                    leg.Entry.WrongWayOffScreenXSpeed,
                    cameraPosition);

                // The route's target rectangle is tested before the Baby moves.
                // Reaching a leg's target moves on to the next leg; reaching the
                // last leg's target starts the latch-onto-Samus phase instead.
                if (CollidesWithRectangle(target, BabyMetroidCutsceneFlightDefinitions.TargetReach))
                {
                    if (leg.Following is { } next)
                        RouteLeg = next;
                    else
                        Phase = BabyMetroidCutscenePhase.LatchOntoSamus;
                }
                break;
            }

            case BabyMetroidCutscenePhase.LatchOntoSamus:
                // The latch approach itself only steers toward Samus's centre less its latch offset; the touch at the
                // next update's start begins the healing.
                GraduallyAccelerateTowardsPoint(
                    samus.Position.Whole + BabyMetroidCutsceneFlightDefinitions.SamusLatchOffset,
                    accelerationDivisor: BabyMetroidCutsceneFlightDefinitions.GentleAccelerationDivisor,
                    BabyMetroidCutsceneFlightDefinitions.ApproachWrongWaySpeed,
                    cameraPosition);
                break;

            case BabyMetroidCutscenePhase.HealSamusToFullHealth:
            {
                // The healing step pins the actor to its four-step shake before healing; the touch that started this
                // phase stopped the Baby, so its zero velocity keeps it pinned.
                Position = Position.WithWhole(samus.Position.Whole + MotherBrainShake.OffsetOn(MotherBrainCadenceDefinitions.BabyShakeStep, enemyFrameCounter)
                    + BabyMetroidCutsceneFlightDefinitions.SamusLatchOffset);

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
                // While the onion rings hit it, the Baby shakes on Samus for as long as each hit shakes it, and once the rings
                // have worn its health away it lets go of her. Its cry at the rings that struck it plays before this step.
                if (OnionRingHitShake.IsShaking)
                {
                    Offset16<RoomPixels> shake = OnionRingHitShake.Offset;
                    Position = Position.WithWhole(samus.Position.Whole + shake + shake + BabyMetroidCutsceneFlightDefinitions.SamusLatchOffset);
                }

                if (Health == 0)
                {
                    // Worn down by the volleys, the Baby takes the health its final charge starts with and lets go of Samus
                    // on its next update.
                    Health = BabyMetroidCutsceneDefinitions.FinalChargeHealth;
                    Phase = BabyMetroidCutscenePhase.ReleaseSamus;
                }
                break;

            case BabyMetroidCutscenePhase.ReleaseSamus:
                // The next step acts on both actors in one update. The speed it gives
                // Mother Brain's brain is presentation state; the meaningful body change
                // starts Mother Brain's fast retreat, and the Baby's flight takes its first update in this same
                // update.
                LateFight.PrepareForFinalBabyMetroidAttack();
                Phase = BabyMetroidCutscenePhase.StareDownMotherBrain;
                goto case BabyMetroidCutscenePhase.StareDownMotherBrain;

            case BabyMetroidCutscenePhase.StareDownMotherBrain:
                // The target is just left of Samus, at a fixed height. This leg and the next two
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
                // The final approach brings the Baby to the charge's start. There it takes the health the final four-ring
                // volley kills it with on its first hit, and Mother Brain turns from her retreat to that volley.
                GraduallyAccelerateTowardsPoint(
                    BabyMetroidCutsceneFlightDefinitions.FinalChargeStart,
                    accelerationDivisor: BabyMetroidCutsceneFlightDefinitions.GentleAccelerationDivisor,
                    BabyMetroidCutsceneFlightDefinitions.StagingWrongWaySpeed,
                    cameraPosition);
                if (CollidesWithRectangle(BabyMetroidCutsceneFlightDefinitions.FinalChargeStart, BabyMetroidCutsceneFlightDefinitions.TargetReach))
                {
                    Health = BabyMetroidCutsceneDefinitions.FinalVolleyHealth;
                    LateFight.ExecuteFinalBabyMetroidAttack();
                    Phase = BabyMetroidCutscenePhase.InitiateFinalCharge;
                }
                break;

            case BabyMetroidCutscenePhase.InitiateFinalCharge:
                // The charge's start divides the distance by its own divisor, a much firmer pull than the staging legs',
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
                // The final target follows Mother Brain's brain, above its centre. Its
                // divisor accelerates much more aggressively than the staging leg's.
                GraduallyAccelerateTowardsPoint(
                    head + BabyMetroidCutsceneFlightDefinitions.FinalChargeOffset,
                    accelerationDivisor: BabyMetroidCutsceneFlightDefinitions.FinalChargeDivisor,
                    BabyMetroidCutsceneFlightDefinitions.ApproachWrongWaySpeed,
                    cameraPosition);
                if (Health == 0)
                {
                    // The fatal blow stops the Baby where it is struck and shakes it about that place, its first update of
                    // shaking this same update.
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
                // The cutscene's shake first slows the Baby's rise by the finest step each update, before it moves, and
                // places the Baby at the saved origin plus the undoubled shaking offsets.
                Velocity = Velocity.WithY(Velocity.Y - SignedEightEight.FinestStep);
                Position = Position.WithWhole(FatalBlowOrigin + MotherBrainShake.OffsetOn(MotherBrainCadenceDefinitions.BabyShakeStep, enemyFrameCounter));
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
                StepSamusRainbowColours(samus);
                AccelerateDownwardsForDeath();
                if (StepFadeToBlack(ref babyFadeImage))
                {
                    // Faded to black, the Baby is hidden, and the pause starts before Samus's rainbow runs on.
                    visibility = BabyMetroidVisibilityRequest.Hide;
                    Phase = BabyMetroidCutscenePhase.PauseAfterFade;
                    PhaseTimer.LastFor(BabyMetroidCutsceneDefinitions.AfterFadeUpdates);
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

            case BabyMetroidCutscenePhase.PauseAfterFade:
                StepSamusRainbowColours(samus);
                if (PhaseTimer.StepRunsOut())
                {
                    // Letting Samus rainbow some more starts on the pause's last update, which is its first.
                    Phase = BabyMetroidCutscenePhase.LetSamusRainbowSomeMore;
                    PhaseTimer.LastFor(BabyMetroidCutsceneDefinitions.RainbowSomeMoreUpdates);
                    goto case BabyMetroidCutscenePhase.LetSamusRainbowSomeMore;
                }
                break;

            case BabyMetroidCutscenePhase.LetSamusRainbowSomeMore:
                if (PhaseTimer.StepRunsOut())
                {
                    Phase = BabyMetroidCutscenePhase.FinalCutscene;
                    RecoveryLights.StartAfter(1);
                    goto case BabyMetroidCutscenePhase.FinalCutscene;
                }
                break;

            case BabyMetroidCutscenePhase.FinalCutscene:
            {
                // Each update shows the next image of the room lights' recovery; the update the recovery is over ends the
                // cutscene.
                if (RecoveryLights.Step() is { } lights)
                {
                    recoveryLightsImage = lights;
                    break;
                }
                if (!RecoveryLights.IsOver)
                    break;

                LateFight.BeginPhase3RecoveryFromBabyCutscene();
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

        // After the Baby moves, its hit shake counts down and its hit flash plays on: a hit starts the shake with its whole
        // length left, so the next update shakes the Baby at its full length and shows the flash's first hurt colours.
        _ = OnionRingHitShake.Step();
        HurtFlash?.Step();

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

    /// <summary>Applies the Baby's health/flash mutation from the blue-ring collision response: a living Baby shakes for <see cref="BabyMetroidCutsceneDefinitions.OnionRingHitShake"/> updates, starts its hurt flash and loses health saturated at zero.</summary>
    /// <param name="damage">The ring's damage (<see cref="MotherBrainAttackDamage.OnionRing"/>), zero or more; even zero damage refreshes the flash timer when health is nonzero.</param>
    /// <remarks>Zero health leaves all state unchanged. The ring's touch establishes the overlap, and the cutscene requests the Baby's cry, copies the Baby's health to its enemy and explodes and deletes the ring.</remarks>
    public void ApplyMotherBrainOnionRingHit(int damage)
    {
        int healthBefore = Health;
        if (healthBefore == 0)
            return;

        OnionRingHitShake.ShakeFor(BabyMetroidCutsceneDefinitions.OnionRingHitShake);
        HurtFlash = BabyMetroidHurtFlashAnimations.Starting();
        Health = Math.Max(0, healthBefore - damage);
    }

    /// <summary>Activates Samus's healed rainbow colours when the Baby reaches her, then slows its frame cadence by a fractional counter.</summary>
    private void StepSamusRainbowColours(SamusState samus)
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
        // to the vertical velocity.
        Velocity = new(Velocity.X.SlowedBy(BabyMetroidCutsceneFlightDefinitions.DeathFallDrag), Velocity.Y + BabyMetroidCutsceneFlightDefinitions.DeathFallGravity);
    }

    /// <summary>
    /// Plays the Baby's fade to black once its center reaches the fade's line, an image each fade step; true on the update the fade
    /// is over, after its last image's hold.
    /// </summary>
    private bool StepFadeToBlack(ref MotherBrainFadeImage? fadeImage)
    {
        // The fade waits, unplayed, until the actor's centre reaches the fade's line.
        if (SignedPlace.Y < BabyMetroidCutsceneDefinitions.FadeToBlackLineY)
            return false;

        if (FadeToBlack.Step() is { } image)
            fadeImage = image;
        return FadeToBlack.IsOver;
    }

    /// <summary>Emits a dust explosion at the death's explosion interval using the shared Ridley scatter pattern.</summary>
    private DustExplosionRequest? StepDeathExplosion()
    {
        if (DeathExplosionTimer.StepKeepsRunning())
            return null;

        DeathExplosionTimer.LastFor(BabyMetroidCutsceneDefinitions.DeathExplosionIntervalUpdates);
        Offset16<RoomPixels> scatter = NextDeathExplosion.Entry;
        NextDeathExplosion = NextDeathExplosion.NextInCycle;
        return new DustExplosionRequest(
            Position: Position.Whole + scatter,
            Animation: DustExplosionAnimation.SmallExplosion,
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
        // Each component is the speed multiplied by the sine's size, signed as the sine is.
        Velocity = new(
            SpeedAlongAngle.Along(Speed, angle),
            SpeedAlongAngle.Along(Speed, angle.TurnedBy(TrigonometryAngle.QuarterTurn)));
    }

    /// <summary>Moves the Baby by its velocity; its position keeps the fraction of a pixel its place held.</summary>
    private void MoveAccordingToVelocity()
    {
        // A fraction past a whole pixel carries into the whole pixels.
        Position = Position.Advance(Velocity.ToDisplacement());
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

    /// <summary>Uses the Baby's hitbox radii and inclusive rectangle overlap for route targets.</summary>
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
        // The Baby is off screen only outside a generous rectangle past the view's sides, above and below it
        // (BabyMetroidFlightWindowDefinitions).
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

    /// <summary>Accelerates Y toward its target at no more than the approach's fastest speed and zeroes velocity on crossing.</summary>
    private bool AccelerateTowardsYPosition(int targetY, SignedEightEight acceleration)
    {
        int difference = SignedPlace.Y - targetY;
        if (difference == 0)
            return true;

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

    /// <summary>
    /// Accelerates X toward its target, leftward when it stands exactly on the target, and zeroes the velocity on crossing.
    /// </summary>
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
            // A Baby exactly on its target column still accelerates leftward.
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

