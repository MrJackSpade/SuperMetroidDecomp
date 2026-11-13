using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Literal stateful translation of the Baby Metroid cutscene entrance, Mother Brain drain,
/// release, ceiling retreat, route to Samus, latch, and healing, and its shared
/// movement helpers.
/// </summary>
/// <remarks>
/// The retail routine does not use floating point, a spline, or a host physics engine. It
/// carries an 8.8 velocity, updates only byte <c>+1</c> of each enemy subposition, and lets
/// the carry from that byte addition enter the whole-pixel position. The curved entrance
/// also reads the game's signed sine table. Keeping those details
/// here is important: replacing them with visually similar interpolation changes the exact
/// frame on which the Baby's collision rectangle reaches Mother Brain's brain slot.
/// </remarks>
public sealed class BabyMetroidCutsceneState
{
    /// <summary>Authored X/Y shake offsets shared by Mother Brain and the latched Baby; the frame counter selects one pair.</summary>
    private static readonly Offset16<RoomPixels>[] ShakingOffsets =
        [new(0, 0), new(-1, 1), new(0, -1), new(1, 1)];

    /// <summary>Places the latched Baby 24 pixels above Mother Brain's brain anchor.</summary>
    private static readonly Offset16<RoomPixels> BrainLatchOffset = new(0, -24);
    /// <summary>Places the latched Baby 20 pixels above Samus's center.</summary>
    private static readonly Offset16<RoomPixels> SamusLatchOffset = new(0, -20);

    /// <summary>Horizontal collision half-extent, 36 room pixels from the Baby's center, copied without halving from the Baby's enemy definition.</summary>
    /// <remarks>The enemy header declares width/height 36. Despite the header macro's friendly names,
    /// enemy loading copies those values directly into the enemy slot's X/Y *radius* fields; there is no
    /// diameter-to-radius division anywhere in between.</remarks>
    public const int XHitboxRadius = 36;
    /// <summary>Vertical collision half-extent, 36 room pixels from the Baby's center, copied without halving from the Baby's enemy definition.</summary>
    public const int YHitboxRadius = 36;

    /// <summary>The entrance's current phase.</summary>
    public BabyMetroidCutscenePhase Phase { get; internal set; } =
        BabyMetroidCutscenePhase.Inactive;

    /// <summary>How the actor meets Samus and her shots, after initialization makes it block the Plasma Beam.</summary>
    public EnemyContact Contact { get; private set; }

    /// <summary>What the room's enemy passes run for the actor, after initialization starts its animation script.</summary>
    public EnemyProcessing Processing { get; private set; }

    /// <summary>Whether the actor is drawn and still occupies its slot.</summary>
    public EnemyPresence Presence { get; private set; }

    /// <summary>
    /// Whether the onion-ring hit flash shows the shared white palette this update instead of
    /// the Baby's own sprite palette.
    /// </summary>
    public bool HurtFlashShown { get; private set; }

    /// <summary>The Baby's current program.</summary>
    public MotherBrainBabyProgram Program { get; private set; }

    /// <summary>Enemy instruction timer, restarted at one whenever the program changes.</summary>
    public int FrameHold { get; private set; }

    /// <summary>
    /// Room position. This actor's 8.8 mover moves it in 256ths of a pixel; its precision finer
    /// than a 256th of a pixel is deliberately kept unchanged.
    /// </summary>
    public FixedPosition<RoomPixels> Position { get; internal set; }

    /// <summary>Signed 8.8 velocity.</summary>
    public EightEightVector<RoomPixels> Velocity { get; internal set; }

    /// <summary>Speed magnitude in 8.8 pixels per update used by the curved entrance, 0 through 3584.</summary>
    public int Speed { get; private set; }

    /// <summary>Heading of the curved entrance; only its table index is passed to shared component math.</summary>
    public RotationAngle Angle { get; private set; }

    /// <summary>Function timer: each phase counts it down and moves on once it runs past zero.</summary>
    public int FunctionTimer { get; private set; }

    /// <summary>Initial request to permit the Baby cry effect.</summary>
    public bool CrySoundEnabled { get; private set; }

    /// <summary>
    /// Zero-based leg of the ceiling-to-Samus route the Baby is steering along, started at the
    /// first leg after the ceiling collision.
    /// </summary>
    public int RouteLeg { get; private set; }

    /// <summary>
    /// Enemy health. The Baby's enemy header supplies 3,200; Mother Brain later drains
    /// this word after the healing hold, so it belongs to actor state rather than rendering.
    /// </summary>
    public int Health { get; internal set; }

    /// <summary>
    /// The Baby's onion-ring flash timer. Mother Brain's onion-ring collision handler
    /// writes sixteen here. Main AI observes the nonzero value as a doubled shake, then
    /// the later flashing handler decrements it and alternates the enemy palette.
    /// </summary>
    public int OnionRingHitFlashTimer { get; private set; }

    /// <summary>Saved whole-pixel origin used by the fatal-blow shake.</summary>
    public Position16<RoomPixels> FatalBlowOrigin { get; private set; }

    /// <summary>Reload-eight timer producing the nine-call cadence of the Baby's fade to black.</summary>
    public int FadeToBlackTimer { get; private set; }

    /// <summary>The shade the Baby's fade to black has reached; valid stored values are 0..6.</summary>
    public int FadeToBlackShade { get; private set; }

    /// <summary>Five-call cadence word for the dust explosions surrounding the dying Baby.</summary>
    public int DeathExplosionTimer { get; private set; }

    /// <summary>
    /// Index into the shared death-explosion scatter pattern, cycling through its ten offsets.
    /// The original game keeps it in the word it calls Mother Brain body's walk counter, indexed
    /// through the Baby's slot, so this actor owns its increment and wrap.
    /// </summary>
    public int DeathExplosionPatternIndex { get; private set; }

    /// <summary>
    /// The updates the cutscene has spent readying Mother Brain's attacks again so far, zero through
    /// <see cref="MotherBrainReadyingDefinitions.AttacksAgainDuration"/>.
    /// </summary>
    public int AttacksAgainReadyingElapsed { get; private set; }

    /// <summary>Palette-table argument used while restoring the phase-three room lights, 0 through 8.</summary>
    public int RoomLightsTransitionCounter { get; private set; }

    /// <summary>
    /// Fraction of a rainbow frame accumulated toward slowing Samus's rainbow, in 256ths of a frame: each call adds
    /// three, and every completed whole frame advances her rainbow.
    /// </summary>
    public int SamusRainbowFraction { get; private set; }

    /// <summary>The accumulated 256ths that complete one rainbow frame.</summary>
    private const int SamusRainbowFractionsPerFrame = 256;

    /// <summary>The 256ths of a rainbow frame one call adds.</summary>
    private const int SamusRainbowFractionPerCall = 3;

    /// <summary>
    /// Adds one call's three 256ths of a rainbow palette frame to <paramref name="fraction"/>, returning the fraction
    /// left over and whether the addition completed a whole frame: the slowing rainbow advances one palette frame each
    /// time the fraction completes a frame, and keeps the remainder.
    /// </summary>
    internal static SamusRainbowFractionStep AdvanceSamusRainbowFraction(int fraction)
    {
        int sum = Ensure.BetweenInclusive(0, SamusRainbowFractionsPerFrame - 1, fraction) + SamusRainbowFractionPerCall;
        return sum >= SamusRainbowFractionsPerFrame
            ? new SamusRainbowFractionStep(sum - SamusRainbowFractionsPerFrame, true)
            : new SamusRainbowFractionStep(sum, false);
    }

    /// <summary>Which of the two rainbow colour cycles Samus currently shows.</summary>
    public BabyMetroidSamusRainbowPhase SamusRainbowPhase { get; private set; }

    /// <summary>True after the deleted enemy property marks the cutscene actor deleted.</summary>
    public bool IsDeleted => Presence.HasAny(EnemyPresence.Deleted);

    /// <summary>
    /// Initializes the Baby. The population record supplies its properties; initialization
    /// adds two more, overwrites the population coordinates, and waits at X/Y
    /// 320/96 before beginning the dash. Neither enemy loading nor this
    /// initializer writes the fractions, so the Baby keeps those of its slot's previous
    /// occupant and moves from them.
    /// </summary>
    public void Initialize(
        FixedPosition<RoomPixels> previousOccupant,
        EnemyContact populationContact = EnemyContact.None,
        EnemyProcessing populationProcessing = EnemyProcessing.AnimationScript | EnemyProcessing.OffScreen,
        EnemyPresence populationPresence = EnemyPresence.None)
    {
        Contact = populationContact.With(EnemyContact.BlocksPlasmaBeam);
        Processing = populationProcessing.With(EnemyProcessing.AnimationScript);
        Presence = populationPresence;
        HurtFlashShown = false;
        SetProgram(MotherBrainBabyProgram.Initial);
        CrySoundEnabled = true;
        RouteLeg = 0;
        Health = 3200;
        OnionRingHitFlashTimer = 0;
        FatalBlowOrigin = Position16<RoomPixels>.Zero;
        FadeToBlackTimer = 0;
        FadeToBlackShade = 0;
        DeathExplosionTimer = 0;
        DeathExplosionPatternIndex = 0;
        AttacksAgainReadyingElapsed = 0;
        RoomLightsTransitionCounter = 0;
        SamusRainbowFraction = 0;
        SamusRainbowPhase = BabyMetroidSamusRainbowPhase.Inactive;
        Position = FixedPosition<RoomPixels>.FromWhole(new(320, 96)).WithFractionsOf(previousOccupant);
        Velocity = default;
        Speed = 0;
        Angle = RotationAngle.Zero;
        Phase = BabyMetroidCutscenePhase.DashOntoScreen;
        FunctionTimer = 248;
    }

    /// <summary>
    /// Executes one complete main-AI call: function first, then the unconditional enemy
    /// velocity mover. Flash/palette presentation is intentionally kept
    /// as inspectable state because it does not alter this entrance's coordinates.
    /// Every Baby routine reads the physical head enemy <c>Enemy[1]</c>'s coordinates, which
    /// the head AI has already moved this frame; <paramref name="head"/> carries them.
    /// </summary>
    public BabyMetroidCutsceneStepResult Step(
        SamusState samus,
        MotherBrainRainbowBeamAttackSequence motherBrain,
        Position16<RoomPixels> head,
        Position16<RoomPixels> cameraPosition = default,
        int enemyFrameCounter = 0,
        int randomNumber = 0)
    {
        Ensure.NotNull(samus);
        Ensure.NotNull(motherBrain);
        if (Phase == BabyMetroidCutscenePhase.Inactive)
            throw new InvalidOperationException("The cutscene Baby Metroid has not been initialized.");

        BabyMetroidCutscenePhase phaseBefore = Phase;
        bool brainCollision;
        bool bodyStumbleRequested = false;
        bool latchSoundQueued = false;
        // The release emits three independent dust allocations when the Baby
        // finally lets go. Keep the individual room coordinates and parameter instead of
        // collapsing that side effect into a boolean: allocation order matters when
        // the shared eighteen-slot enemy-projectile pool is nearly full.
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
                samus.Position.Whole + SamusLatchOffset,
                acceleration: SignedEightEight.FromSignedRaw(16));
            if (reachedLatchPoint)
            {
                Velocity = default;
                Phase = BabyMetroidCutscenePhase.HealSamusToFullHealth;
            }
        }

        switch (Phase)
        {
            case BabyMetroidCutscenePhase.DashOntoScreen:
                // The timer of 248 is decremented before the sign test, so calls 1..248 leave the Baby still and
                // call 249 takes zero to -1. The expiry call immediately executes the
                // first curve update before the common mover consumes its new velocity.
                if (--FunctionTimer < 0)
                {
                    Angle = RotationAngle.FromSteps(216);
                    Speed = 2560;
                    Phase = BabyMetroidCutscenePhase.CurveTowardMotherBrainHead;
                    FunctionTimer = 10;
                    goto case BabyMetroidCutscenePhase.CurveTowardMotherBrainHead;
                }
                break;

            case BabyMetroidCutscenePhase.CurveTowardMotherBrainHead:
                UpdateSpeedAndAngle(angleDelta: -384, targetAngle: RotationAngle.FromSteps(176), targetSpeed: 2560);
                if (--FunctionTimer < 0)
                {
                    Phase = BabyMetroidCutscenePhase.GetRightUpInMotherBrainsFace;
                    FunctionTimer = 9;
                }
                break;

            case BabyMetroidCutscenePhase.GetRightUpInMotherBrainsFace:
                UpdateSpeedAndAngle(angleDelta: -1536, targetAngle: RotationAngle.FromSteps(130), targetSpeed: 3584);
                brainCollision = CollidesWithRectangle(
                    head,
                    TargetRadius);

                // Collision skips the timer decrement. Otherwise nine permits ten
                // approach calls and expires when the tenth decrement runs to -1.
                if (!brainCollision)
                {
                    brainCollision = --FunctionTimer < 0;
                }

                if (brainCollision)
                {
                    Phase = BabyMetroidCutscenePhase.LatchOntoMotherBrain;
                    samus.Drained.PutStanding(samus); // Drained Samus command one.
                }
                break;

            case BabyMetroidCutscenePhase.LatchOntoMotherBrain:
            {
                Position16<RoomPixels> target = head + BrainLatchOffset;
                GraduallyAccelerateTowardsPoint(
                    target,
                    accelerationDivisor: 16,
                    wrongWayOffScreenXSpeed: 1024,
                    cameraPosition);
                brainCollision = CollidesWithRectangle(target, new Extent16<RoomPixels>(8, 8));
                if (brainCollision)
                    Phase = BabyMetroidCutscenePhase.SetMotherBrainToStumbleBack;
                break;
            }

            case BabyMetroidCutscenePhase.SetMotherBrainToStumbleBack:
                // The drain step passes animation-delay index two and target Body.X-1. With a
                // standing body this always installs the really-fast backward list.
                bodyStumbleRequested = motherBrain.RequestBabyStumbleBackward();
                Phase = BabyMetroidCutscenePhase.ActivateRainbowBeamAndMotherBrainBody;
                goto case BabyMetroidCutscenePhase.ActivateRainbowBeamAndMotherBrainBody;

            case BabyMetroidCutscenePhase.ActivateRainbowBeamAndMotherBrainBody:
            {
                Position16<RoomPixels> target = head + BrainLatchOffset;
                bool reachedTarget = AccelerateTowardsPoint(target, acceleration: SignedEightEight.FromSignedRaw(512));
                if (reachedTarget)
                {
                    // The helper predicts whole-pixel overshoot but does not itself store
                    // its returned target coordinate. The drain step performs the exact pin.
                    Velocity = default;
                    Position = Position.WithWhole(target);
                    SetProgram(MotherBrainBabyProgram.DrainingMotherBrain);
                    Phase = BabyMetroidCutscenePhase.WaitForMotherBrainToTurnToCorpse;
                    motherBrain.InterruptFinalBeamForBabyDrain();
                    latchSoundQueued = true; // The rainbow-drain sound starts as the Baby latches on.
                }
                break;
            }

            case BabyMetroidCutscenePhase.WaitForMotherBrainToTurnToCorpse:
            {
                int shakingIndex = MotherBrainCadenceDefinitions.BabyShakeStep.Of(enemyFrameCounter);
                Position = Position.WithWhole(head + ShakingOffsets[shakingIndex] + BrainLatchOffset);
                if (motherBrain.State.Phase2CorpseState != MotherBrainPhase2CorpseState.Living)
                {
                    Phase = BabyMetroidCutscenePhase.StopDraining;
                    FunctionTimer = 64;
                }
                break;
            }

            case BabyMetroidCutscenePhase.StopDraining:
                // Shaking ceases immediately: every wait call pins to the unoffset brain
                // coordinate before decrementing the timer of 64, which expires only after 65 calls.
                Position = Position.WithWhole(head + BrainLatchOffset);
                if (--FunctionTimer < 0)
                {
                    SetProgram(MotherBrainBabyProgram.Initial);
                    Phase = BabyMetroidCutscenePhase.LetGoAndSpawnDustClouds;
                    FunctionTimer = 32;
                    Velocity = default;
                }
                break;

            case BabyMetroidCutscenePhase.LetGoAndSpawnDustClouds:
                // The release branches *into* its acceleration while the timer is nonnegative. Thus the
                // Baby accelerates ceilingward for all 32 visible release calls; the dust
                // burst happens on call 33 and that call also performs another acceleration.
                if (--FunctionTimer < 0)
                {
                    // The three dust clouds on Mother Brain's head are spawned in this
                    // exact order. Each helper adds its signed offsets to the brain
                    // enemy's current room coordinate and passes animation parameter nine
                    // to the ordinary highest-free-slot allocator.
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
                    wrongWayOffScreenXSpeed: 1024,
                    cameraPosition);
                bool ceilingCollision = CollidesWithRectangle(target, TargetRadius);
                if (ceilingCollision)
                {
                    samus.Drained.PutCrouchingOrFalling(samus);
                    Phase = BabyMetroidCutscenePhase.MoveToSamus;
                    RouteLeg = 0;
                }
                break;
            }

            case BabyMetroidCutscenePhase.MoveToSamus:
            {
                // The route step clears the ordinary cry request and enables health-based
                // palette selection on *every* route call. The ambient cry takes its own chance.
                CrySoundEnabled = false;
                ambientCrySoundQueued = MotherBrainRandomDefinitions.BabyMetroidCries.IsTakenBy(randomNumber);

                // The route is eight overlapping records. The fixed targets, divisors, and callback identities are
                // compiled mechanics definitions; sprite/instruction presentation remains separately backed.
                BabyMetroidRouteRecord record = BabyMetroidRoute.GetRecord(RouteLeg);
                Position16<RoomPixels> target = record.Target;
                int accelerationDivisor = record.AccelerationDivisor;
                int wrongWayExtra = BabyMetroidRoute.GetWrongWayOffScreenXSpeed(
                    record.MovementFunction);

                GraduallyAccelerateTowardsPoint(
                    target,
                    accelerationDivisor,
                    wrongWayExtra,
                    cameraPosition);

                // The route's 4x4 rectangle test occurs before the common velocity mover.
                // Reaching an ordinary leg's target moves on to the next leg; reaching the
                // final leg's target installs the latch-onto-Samus function instead.
                if (CollidesWithRectangle(target, TargetRadius))
                {
                    if (record.EndsWithLatchOntoSamus)
                        Phase = BabyMetroidCutscenePhase.LatchOntoSamus;
                    else
                        RouteLeg++;
                }
                break;
            }

            case BabyMetroidCutscenePhase.LatchOntoSamus:
                // The latch approach itself only steers toward Samus's centre minus twenty pixels. It
                // never performs the state change. That transition belongs to enemy-touch
                // handler, executed after main AI and the common mover below.
                GraduallyAccelerateTowardsPoint(
                    samus.Position.Whole + SamusLatchOffset,
                    accelerationDivisor: 16,
                    wrongWayOffScreenXSpeed: 1024,
                    cameraPosition);
                break;

            case BabyMetroidCutscenePhase.HealSamusToFullHealth:
            {
                // The healing step pins the actor to the four-entry shake table before healing. The
                // subsequent common mover is harmless because the touch handler zeroed
                // both velocities when it installed this function.
                CrySoundEnabled = false;
                int shakingIndex = MotherBrainCadenceDefinitions.BabyShakeStep.Of(enemyFrameCounter);
                Position = Position.WithWhole(samus.Position.Whole + ShakingOffsets[shakingIndex] + SamusLatchOffset);

                // Healing adds exactly one energy per enemy-processing call. The compare is a
                // signed comparison in -32768..32767; normal game maxima stay in its positive range.
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
                // the hit-flash word. Sound queuing is returned by the ring system; this
                // actor owns only the resulting shake and health-to-function transition.
                if (OnionRingHitFlashTimer != 0)
                {
                    int shakingIndex = MotherBrainCadenceDefinitions.BabyHitShakeStep.Of(OnionRingHitFlashTimer);
                    Offset16<RoomPixels> shake = ShakingOffsets[shakingIndex];
                    Position = Position.WithWhole(samus.Position.Whole + shake + shake + SamusLatchOffset);
                }

                if (Health == 0)
                {
                    // The zero reached by the ordinary volleys is a state trigger, not the
                    // health carried into the final charge. Code deliberately gives
                    // the Baby 320 health, changes to the low-health palette function,
                    // disables health-band palettes, and lets the next step run next frame.
                    Health = 320;
                    Phase = BabyMetroidCutscenePhase.ReleaseSamus;
                }
                break;

            case BabyMetroidCutscenePhase.ReleaseSamus:
                // The next step is a one-call cross-actor dispatcher. The speed it gives
                // Mother Brain's brain is presentation state; the meaningful body change
                // starts Mother Brain's fast retreat while this same call falls through to
                // the first flight update.
                motherBrain.PrepareForFinalBabyMetroidAttack();
                Phase = BabyMetroidCutscenePhase.StareDownMotherBrain;
                goto case BabyMetroidCutscenePhase.StareDownMotherBrain;

            case BabyMetroidCutscenePhase.StareDownMotherBrain:
                // Literal target: four pixels left of Samus and Y 96.
                // The helper's Y index zero means divisor 16. This leg and the next two
                // enter, which stores a 16 wrong-way off-screen X extra,
                // not the 1024 of the entrance helpers.
            {
                var target = new Position16<RoomPixels>(CoordinateAxis.Wrap(samus.Position.X.Whole - 4), 96);
                GraduallyAccelerateTowardsPoint(
                    target,
                    accelerationDivisor: 16,
                    wrongWayOffScreenXSpeed: 16,
                    cameraPosition);
                if (CollidesWithRectangle(target, TargetRadius))
                {
                    Phase = BabyMetroidCutscenePhase.FlyOffScreen;
                }
                break;
            }

            case BabyMetroidCutscenePhase.FlyOffScreen:
                // Despite the function's historical name, this leg targets the fixed point
                // `(272,64)` just beyond the right edge of the original 256-pixel camera.
                GraduallyAccelerateTowardsPoint(
                    new Position16<RoomPixels>(272, 64),
                    accelerationDivisor: 16,
                    wrongWayOffScreenXSpeed: 16,
                    cameraPosition);
                if (CollidesWithRectangle(new Position16<RoomPixels>(272, 64), TargetRadius))
                    Phase = BabyMetroidCutscenePhase.MoveToFinalChargeStart;
                break;

            case BabyMetroidCutscenePhase.MoveToFinalChargeStart:
                // The final approach stages the Baby at `(305,160)`. Reaching that rectangle assigns
                // exactly 79 HP so the final four-ring volley kills on its first collision,
                // clears the brain-slot speed flag, and replaces Mother Brain's retreat AI.
                GraduallyAccelerateTowardsPoint(
                    new Position16<RoomPixels>(305, 160),
                    accelerationDivisor: 16,
                    wrongWayOffScreenXSpeed: 16,
                    cameraPosition);
                if (CollidesWithRectangle(new Position16<RoomPixels>(305, 160), TargetRadius))
                {
                    Health = 79;
                    motherBrain.ExecuteFinalBabyMetroidAttack();
                    Phase = BabyMetroidCutscenePhase.InitiateFinalCharge;
                }
                break;

            case BabyMetroidCutscenePhase.InitiateFinalCharge:
                // Divisor-table index 10 maps to 16 - 10 == 6. This helper family adds
                // 1024 only when an off-screen actor is moving the wrong horizontal way.
                GraduallyAccelerateTowardsPoint(
                    new Position16<RoomPixels>(290, 128),
                    accelerationDivisor: 6,
                    wrongWayOffScreenXSpeed: 1024,
                    cameraPosition);
                if (CollidesWithRectangle(new Position16<RoomPixels>(290, 128), TargetRadius))
                    Phase = BabyMetroidCutscenePhase.FinalCharge;
                break;

            case BabyMetroidCutscenePhase.FinalCharge:
                // The final target follows the brain enemy slot, 32 pixels above its
                // centre. Index `$C` maps to divisor four and therefore accelerates much
                // more aggressively than the staging leg.
                GraduallyAccelerateTowardsPoint(
                    head + new Offset16<RoomPixels>(0, -32),
                    accelerationDivisor: 4,
                    wrongWayOffScreenXSpeed: 1024,
                    cameraPosition);
                if (Health == 0)
                {
                    // The fatal-blow step changes graphics/list state, freezes the body AI,
                    // clears velocity, remembers the shake origin, and falls through into
                    // the first of 17 fatal-blow shake calls.
                    SetProgram(MotherBrainBabyProgram.TakingFatalBlow);
                    Velocity = default;
                    Phase = BabyMetroidCutscenePhase.TakeFinalBlow;
                    FunctionTimer = 16;
                    FatalBlowOrigin = Position.Whole;
                    goto case BabyMetroidCutscenePhase.TakeFinalBlow;
                }
                break;

            case BabyMetroidCutscenePhase.TakeFinalBlow:
            {
                // The cutscene's shake first adds -1 to the Y velocity,
                // which the common mover then applies, and places the Baby at the saved origin
                // plus the undoubled shaking offsets.
                Velocity = Velocity.WithY(Velocity.Y - SignedEightEight.FromSignedRaw(1));
                int shakingIndex = MotherBrainCadenceDefinitions.BabyShakeStep.Of(enemyFrameCounter);
                Position = Position.WithWhole(FatalBlowOrigin + ShakingOffsets[shakingIndex]);
                if (--FunctionTimer < 0)
                {
                    Position = Position.WithWhole(FatalBlowOrigin);
                    Phase = BabyMetroidCutscenePhase.PlaySamusTheme;
                    FunctionTimer = 56;
                    goto case BabyMetroidCutscenePhase.PlaySamusTheme;
                }
                break;
            }

            case BabyMetroidCutscenePhase.PlaySamusTheme:
                // The expiry call queues the two delayed music words and immediately
                // performs the first twelve-count prepare-Hyper-Beam decrement.
                if (--FunctionTimer < 0)
                {
                    Phase = BabyMetroidCutscenePhase.PrepareSamusForHyperBeam;
                    FunctionTimer = 12;
                    goto case BabyMetroidCutscenePhase.PrepareSamusForHyperBeam;
                }
                break;

            case BabyMetroidCutscenePhase.PrepareSamusForHyperBeam:
                if (--FunctionTimer < 0)
                {
                    // The Hyper Beam acquisition freezes Samus's drained animation at byte-index 28.
                    // The Hyper Beam step also installs the first rainbow handler; neither handler is
                    // executed until the Baby's following enemy-AI call.
                    SamusDrainedState.FreezeForHyperBeamAcquisition(samus);
                    SamusRainbowPhase = BabyMetroidSamusRainbowPhase.ActivateWhenEnemyIsLow;
                    Phase = BabyMetroidCutscenePhase.DeathSequence;
                }
                break;

            case BabyMetroidCutscenePhase.DeathSequence:
                StepSamusRainbowPaletteAnimation(samus, ref samusRainbowActivated);
                AccelerateDownwardsForDeath();
                if (StepFadeToBlack(ref babyFadeShade))
                {
                    // Completion does not write palette index seven. It hides the actor,
                    // starts readying Mother Brain's attacks again, and waits through 128..0 before its first update.
                    Presence = Presence.With(EnemyPresence.Hidden);
                    Phase = BabyMetroidCutscenePhase.ReadyMotherBrainAttacksAgain;
                    FunctionTimer = 128;
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
                StepSamusRainbowPaletteAnimation(samus, ref samusRainbowActivated);
                if (--FunctionTimer < 0)
                {
                    if (AttacksAgainReadyingElapsed >= MotherBrainReadyingDefinitions.AttacksAgainDuration)
                        throw new InvalidOperationException("Mother Brain's attacks are already readied again.");

                    AttacksAgainReadyingElapsed++;

                    if (AttacksAgainReadyingElapsed == MotherBrainReadyingDefinitions.AttacksAgainDuration)
                    {
                        // The last readying update falls straight into the next step, so the newly written 176
                        // becomes 175 on this same call.
                        Phase = BabyMetroidCutscenePhase.LetSamusRainbowSomeMore;
                        FunctionTimer = 176;
                        goto case BabyMetroidCutscenePhase.LetSamusRainbowSomeMore;
                    }
                }
                break;

            case BabyMetroidCutscenePhase.LetSamusRainbowSomeMore:
                if (--FunctionTimer < 0)
                {
                    Phase = BabyMetroidCutscenePhase.FinalCutscene;
                    RoomLightsTransitionCounter = 0;
                    goto case BabyMetroidCutscenePhase.FinalCutscene;
                }
                break;

            case BabyMetroidCutscenePhase.FinalCutscene:
            {
                // The room-lights step increments the shared counter, passes its previous value to the
                // palette step, and treats its seventh step as the end.
                int paletteIndex = RoomLightsTransitionCounter;
                RoomLightsTransitionCounter++;
                if (paletteIndex < 7)
                {
                    recoveryLightsFrame = paletteIndex;
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
        // extra timer that onion-ring hits write and shows the flash colours while bit one of the
        // decremented value is set. Starting from sixteen, the first post-hit enemy call sees
        // sixteen for its shake above, then displays the normal palette with timer fifteen.
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
            bodyStumbleRequested,
            latchSoundQueued,
            releaseDustClouds.ToArray(),
            ambientCrySoundQueued,
            deathExplosion,
            babyFadeShade,
            recoveryLightsFrame);
    }

    /// <summary>Applies the Baby's health/flash mutation from the blue-ring collision handler: a living Baby gets a sixteen-frame flash timer and health subtraction saturated at zero.</summary>
    /// <param name="damage">Unsigned health units to subtract, defaulting to 80; even zero damage refreshes the flash timer when health is nonzero.</param>
    /// <returns>An empty compatibility result; the effects are written directly to <see cref="Health"/> and <see cref="OnionRingHitFlashTimer"/>.</returns>
    /// <remarks>Zero health leaves all state unchanged. The caller must establish overlap and owns the Baby cry request, physical enemy-slot synchronization, and ring explosion/deletion.</remarks>
    public BabyMetroidOnionRingHitResult ApplyMotherBrainOnionRingHit(int damage = 80)
    {
        int healthBefore = Health;
        if (healthBefore == 0)
            return new BabyMetroidOnionRingHitResult();

        OnionRingHitFlashTimer = 16;
        Health = Math.Max(0, healthBefore - damage);
        return new BabyMetroidOnionRingHitResult();
    }

    /// <summary>Activates Samus's healed rainbow palette when the Baby reaches her, then slows its frame cadence by a fractional accumulator.</summary>
    private void StepSamusRainbowPaletteAnimation(
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
                (int fraction, bool frameCompleted) = AdvanceSamusRainbowFraction(SamusRainbowFraction);
                SamusRainbowFraction = fraction;
                if (frameCompleted)
                    samus.Drained.IncrementRainbowPaletteFrame(maximumFrame: 10);
                return;
            }

            default:
                throw new InvalidOperationException($"Unsupported Samus rainbow phase {SamusRainbowPhase}.");
        }
    }

    /// <summary>The drag the Baby's death fall takes from its sideways speed each update.</summary>
    private static readonly SignedEightEight DeathFallDrag = SignedEightEight.FromSignedRaw(32);

    /// <summary>The gravity the Baby's death fall adds to its vertical speed each update.</summary>
    private static readonly SignedEightEight DeathFallGravity = SignedEightEight.FromSignedRaw(2);

    /// <summary>The fastest the Baby approaches a point, on each axis, in either direction: five pixels an update.</summary>
    private static readonly SignedEightEight ApproachMaximumSpeed = SignedEightEight.FromSignedRaw(1280);

    /// <summary>Slows the horizontal speed by the death fall's drag and adds its gravity to the vertical speed.</summary>
    private void AccelerateDownwardsForDeath()
    {
        // The slowing step takes the drag from the speed, stopping at rest and keeping its direction, and
        // independently adds the gravity to the vertical velocity. The fall ends 63 updates after the Baby reaches
        // line 128, so even a fall across the whole coordinate space leaves the speed below 33 pixels an update.
        Velocity = new(Velocity.X.SlowedBy(DeathFallDrag), Velocity.Y + DeathFallGravity);
    }

    /// <summary>Advances the Baby's black-fade shade every nine calls after its center reaches Y=128, selecting the shade it shows.</summary>
    private bool StepFadeToBlack(ref int? fadeShade)
    {
        // No timer changes occur until the actor's centre reaches Y 128.
        if (CoordinateAxis.Difference(Position.Y.Whole, 128) < 0)
            return false;

        if (--FadeToBlackTimer >= 0)
            return false;

        FadeToBlackTimer = 8;
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
        if (--DeathExplosionTimer >= 0)
            return null;

        DeathExplosionTimer = 4;
        DeathExplosionPatternIndex++;
        if (DeathExplosionPatternIndex >= DeathExplosionScatterDefinitions.Count)
            DeathExplosionPatternIndex = 0;
        // The death explosions hold the scatter shared with Ridley's death. The handler increments its
        // index before looking up a pair, so a freshly cleared counter begins at entry one.
        return new DustExplosionRequest(
            Position: Position.Whole + DeathExplosionScatter.Offset(DeathExplosionPatternIndex),
            Animation: MiscDustAnimation.SmallExplosion,
            SoundEffect: MotherBrainSoundDefinitions.Explosion);
    }

    /// <summary>Switches the Baby's program and restarts its instruction timer.</summary>
    private void SetProgram(MotherBrainBabyProgram program)
    {
        Program = program;
        FrameHold = 1;
    }

    /// <summary>Clamps speed and angle toward their targets before deriving 8.8 X/Y velocity from sine values.</summary>
    private void UpdateSpeedAndAngle(
        int angleDelta,
        RotationAngle targetAngle,
        int targetSpeed)
    {
        // The approach step changes speed by exactly 32 and clamps rather than crossing the target.
        if (Speed != targetSpeed)
        {
            if (targetSpeed < Speed)
                Speed = Math.Max(Speed - 32, targetSpeed);
            else
                Speed = Math.Min(Speed + 32, targetSpeed);
        }

        // The turn stops at the target angle: the signed difference from the target says
        // whether this step would carry the heading past it.
        RotationAngle candidateAngle = Angle.AddRaw(angleDelta);
        if (angleDelta < 0)
        {
            Angle = candidateAngle.TurnFrom(targetAngle) >= 0
                ? candidateAngle
                : targetAngle;
        }
        else
        {
            Angle = candidateAngle.TurnFrom(targetAngle) < 0
                ? candidateAngle
                : targetAngle;
        }

        TrigonometryAngle angle = Angle.ToTrigonometryAngle();
        Velocity = new(
            CalculateVelocityComponent(Speed, angle),
            CalculateVelocityComponent(Speed, angle.TurnedBy(TrigonometryAngle.QuarterTurn)));
    }

    /// <summary>Multiplies speed by a signed sine-table entry with the game's bit-8-to-23 result.</summary>
    /// <param name="speed">The speed in 256ths of a pixel an update, never negative.</param>
    /// <param name="angle">The sine-table angle.</param>
    internal static SignedEightEight CalculateVelocityComponent(
        int speed,
        TrigonometryAngle angle)
    {
        // The sine multiply takes unsigned speed by the absolute signed-table value,
        // returns product bits 8..23, then reapplies the original sign.
        return Trigonometry.MultiplySignedSine(speed, angle);
    }

    /// <summary>Advances 16.16 position from 8.8 velocity while leaving the low subposition byte untouched.</summary>
    private void MoveAccordingToVelocity()
    {
        // The velocity's fraction is added to the upper half of the subpixel position and its
        // whole part to the whole pixels, with the fraction's carry. That is exactly a 16.16
        // add of the 8.8 velocity: the lower half of the subpixel position is unchanged.
        Position = Position.Advance(Velocity.ToSixteenSixteen());
    }

    /// <summary>
    /// The ordinary enemy-versus-Samus contact check, for the Baby's radius hitbox.
    /// A non-zero contact-damage index clears Samus's invincibility and proceeds; otherwise an
    /// invincible Samus is not touched. Each axis then requires
    /// <c>|delta| - Samus's radius &lt; enemy radius</c> (a delta inside Samus's own radius
    /// always passes), which is strict, unlike the rectangle check other actors use.
    /// </summary>
    private bool TouchesSamus(SamusState samus)
    {
        if (samus.HorizontalSpeed.ContactDamageIndex != SamusContactDamageIndex.None)
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
        // The rectangle check adds both radii and one, then rejects `abs(delta) >= sum+1`.
        // In the normal room-coordinate domain that is equivalently `abs(delta) <= sum`.
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
        int wrongWayOffScreenXSpeed,
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
        // then shifts the count twice. Carry is set only for count two.
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
            Velocity = Velocity.WithY(SignedEightEight.Min(Velocity.Y + acceleration, ApproachMaximumSpeed));
            int prediction = CoordinateAxis.Wrap(Position.Y.Whole + Velocity.Y.WholePixels);
            if (CoordinateAxis.Difference(prediction, targetY) < 0)
                return false;
        }
        else
        {
            Velocity = Velocity.WithY(SignedEightEight.Max(Velocity.Y - acceleration, -ApproachMaximumSpeed));
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
            Velocity = Velocity.WithX(SignedEightEight.Min(Velocity.X + acceleration, ApproachMaximumSpeed));
            int prediction = CoordinateAxis.Wrap(Position.X.Whole + Velocity.X.WholePixels);
            if (CoordinateAxis.Difference(prediction, targetX) < 0)
                return false;
        }
        else
        {
            // Deliberately sends exact equality through the left branch; unlike Y,
            // there is no equality test before the sign test.
            Velocity = Velocity.WithX(SignedEightEight.Max(Velocity.X - acceleration, -ApproachMaximumSpeed));
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

