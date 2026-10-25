using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Literal stateful translation of the Baby Metroid cutscene entrance, Mother Brain drain,
/// release, ceiling retreat, route to Samus, latch, and healing at
/// <c>$A9:C710-$A9:CABC</c> and its shared movement helpers.
/// </summary>
/// <remarks>
/// The retail routine does not use floating point, a spline, or a host physics engine. It
/// carries an 8.8 velocity, updates only byte <c>+1</c> of each enemy subposition, and lets
/// the carry from that byte addition enter the whole-pixel position. The curved entrance
/// also reads the cartridge's signed sine table at <c>$A0:B443</c>. Keeping those details
/// here is important: replacing them with visually similar interpolation changes the exact
/// frame on which the Baby's native collision rectangle reaches Mother Brain's brain slot.
/// </remarks>
public sealed class BabyMetroidCutsceneState
{
    /// <summary>Authored $A9:93BB X/Y shake offsets shared by Mother Brain and the latched Baby; the frame counter selects one pair.</summary>
    private static readonly Offset16<RoomPixels>[] ShakingOffsets =
        [new(0, 0), new(-1, 1), new(0, -1), new(1, 1)];

    /// <summary>Places the latched Baby 24 pixels above Mother Brain's brain anchor.</summary>
    private static readonly Offset16<RoomPixels> BrainLatchOffset = new(0, -24);
    /// <summary>Places the latched Baby 20 pixels above Samus's center.</summary>
    private static readonly Offset16<RoomPixels> SamusLatchOffset = new(0, -20);

    /// <summary>Horizontal collision half-extent, 36 room pixels from the Baby's center, copied without halving from <c>EnemyHeaders_BabyMetroidCutscene</c> at $A0:ECBF.</summary>
    /// <remarks>Enemy header `$A0:ECBF` declares width/height `$24`. Despite the header macro's friendly names,
    /// `$A0:8AFF/$8B05` copy those words directly into the enemy slot's X/Y *radius* fields; there is no
    /// diameter-to-radius division anywhere in between.</remarks>
    public const ushort XHitboxRadius = 36;
    /// <summary>Vertical collision half-extent, 36 room pixels from the Baby's center, copied without halving from <c>EnemyHeaders_BabyMetroidCutscene</c> at $A0:ECBF.</summary>
    public const ushort YHitboxRadius = 36;

    /// <summary>Current native function-pointer equivalent.</summary>
    public BabyMetroidCutscenePhase Phase { get; internal set; } =
        BabyMetroidCutscenePhase.Inactive;

    /// <summary>Enemy properties after initialization ORs in <c>$3000</c>.</summary>
    public EnemyPropertyWord Properties { get; private set; }

    /// <summary>
    /// Whether the onion-ring hit flash shows the shared white palette this update instead of
    /// the Baby's own sprite palette.
    /// </summary>
    public bool HitFlashPaletteShown { get; private set; }

    /// <summary>
    /// The sheet character every Baby frame's parts count from: character 160, where the four transferred Baby tile
    /// rows live. The cartridge also raised the parts' priority one step for the fatal blow by adding to this offset;
    /// per-part priority is a presentation quirk the port does not keep (#1422).
    /// </summary>
    public int TileBase { get; private set; }

    /// <summary>The sheet character the Baby's four transferred tile rows start at.</summary>
    private const int BabyTileBase = 160;

    /// <summary>The Baby's current program.</summary>
    public MotherBrainBabyProgram Program { get; private set; }

    /// <summary>Enemy instruction timer, restarted at one whenever the program changes.</summary>
    public WrappingCountdown InstructionTimer { get; private set; }

    /// <summary>
    /// Native 16.16 room position. Only the high byte of each subposition participates in
    /// this actor's 8.8 mover; the low bytes are deliberately retained and untouched.
    /// </summary>
    public FixedPosition<RoomPixels> Position { get; internal set; }

    /// <summary>Signed 8.8 velocity.</summary>
    public EightEightVector<RoomPixels> Velocity { get; internal set; }

    /// <summary>Speed magnitude in 8.8 pixels per update used by the curved entrance, 0 through $0E00.</summary>
    public int Speed { get; private set; }

    /// <summary>Heading of the curved entrance; only its table index is passed to shared component math.</summary>
    public RotationAngle Angle { get; private set; }

    /// <summary>Native function timer, decremented with 16-bit wrap and BMI tests.</summary>
    public WrappingCountdown FunctionTimer { get; private set; }

    /// <summary>Initial request to permit the Baby cry effect.</summary>
    public bool CrySoundEnabled { get; private set; }

    /// <summary>Health-palette gate, initially disabled for the entrance.</summary>
    public bool HealthBasedPaletteEnabled { get; private set; }

    /// <summary>
    /// Zero-based leg of the ceiling-to-Samus route the Baby is steering along, started at the
    /// first leg after the ceiling collision.
    /// </summary>
    public int RouteLeg { get; private set; }

    /// <summary>
    /// Native enemy health. Header <c>$A0:ECBF</c> supplies 3,200; Mother Brain later drains
    /// this word after the healing hold, so it belongs to actor state rather than rendering.
    /// </summary>
    public int Health { get; internal set; }

    /// <summary>
    /// Extra enemy word <c>$7E:7806,x</c>. Mother Brain's onion-ring collision handler
    /// writes <c>$10</c> here. Main AI observes the nonzero value as a doubled shake, then
    /// the later flashing handler decrements it and alternates the enemy palette.
    /// </summary>
    public WrappingCountdown OnionRingHitFlashTimer { get; private set; }

    /// <summary>Saved whole-pixel origin used by the fatal-blow shake at <c>$A9:CC3E</c>.</summary>
    public Position16<RoomPixels> FatalBlowOrigin { get; private set; }

    /// <summary>Reload-eight timer producing the native nine-call black-palette cadence.</summary>
    public WrappingCountdown FadeToBlackPaletteTimer { get; private set; }

    /// <summary>Current `$AD:E8E2` black-fade palette index; valid stored values are 0..6.</summary>
    public int FadeToBlackPaletteIndex { get; private set; }

    /// <summary>Five-call cadence word for the dust explosions surrounding the dying Baby.</summary>
    public WrappingCountdown DeathExplosionTimer { get; private set; }

    /// <summary>
    /// Index into the shared death-explosion scatter pattern, cycling through its ten offsets.
    /// The cartridge keeps it in the word it calls Mother Brain body's walk counter, indexed
    /// through the Baby's slot, so this actor owns its increment and wrap.
    /// </summary>
    public int DeathExplosionPatternIndex { get; private set; }

    /// <summary>How many attack pages the cutscene has shown again (<c>$A9:8FC7</c>), which is also the next to show.</summary>
    public int AttackPagesShown { get; private set; }

    /// <summary>Palette-table argument used while restoring the phase-three room lights, 0 through 8.</summary>
    public int RoomLightsTransitionCounter { get; private set; }

    /// <summary>
    /// Fraction of a rainbow palette frame accumulated toward slowing Samus's rainbow, in
    /// 1/65536 frames: each call adds 768, and every completed whole frame advances the palette.
    /// </summary>
    public int SamusRainbowPaletteAnimationCounter { get; private set; }

    /// <summary>The accumulated fraction, in 1/65536 frames, that completes one rainbow palette frame.</summary>
    private const int SamusRainbowPaletteFractionsPerFrame = 65536;

    /// <summary>
    /// Adds one call's 768/65536 of a rainbow palette frame to <paramref name="fraction"/>, returning
    /// the fraction left over and whether the addition completed a whole frame. This is the
    /// cartridge's carry out of its 16-bit accumulator: the slowing rainbow advances one palette
    /// frame on every carry and keeps the remainder.
    /// </summary>
    internal static SamusRainbowFractionStep AdvanceSamusRainbowFraction(int fraction)
    {
        int sum = Ensure.BetweenInclusive(0, SamusRainbowPaletteFractionsPerFrame - 1, fraction) + 768;
        return sum >= SamusRainbowPaletteFractionsPerFrame
            ? new SamusRainbowFractionStep(sum - SamusRainbowPaletteFractionsPerFrame, true)
            : new SamusRainbowFractionStep(sum, false);
    }

    /// <summary>Which of the two native rainbow-palette handlers is currently installed.</summary>
    public BabyMetroidSamusRainbowPhase SamusRainbowPhase { get; private set; }

    /// <summary>True after enemy property `$0200` marks the cutscene actor deleted.</summary>
    public bool IsDeleted => Properties.HasAny(EnemyProperties.Deleted);

    /// <summary>
    /// Ports <c>$A9:C710</c>. The population record supplies <c>$2800</c>; initialization
    /// ORs <c>$3000</c>, overwrites the population coordinates, and waits at X/Y
    /// <c>$140/$60</c> before beginning the dash. Neither <c>$A0:9275</c> nor this
    /// initializer writes the fractions, so the Baby keeps those of its slot's previous
    /// occupant and moves from them.
    /// </summary>
    public void Initialize(
        FixedPosition<RoomPixels> previousOccupant,
        EnemyPropertyWord? populationProperties = null)
    {
        Properties = (populationProperties ?? new EnemyPropertyWord(
            EnemyProperties.ProcessInstructions | EnemyProperties.ProcessOffScreen)).With(
            EnemyProperties.BlocksPlasmaBeam |
            EnemyProperties.ProcessInstructions);
        HitFlashPaletteShown = false;
        TileBase = BabyTileBase;
        SetProgram(MotherBrainBabyProgram.Initial);
        CrySoundEnabled = true;
        HealthBasedPaletteEnabled = false;
        RouteLeg = 0;
        Health = 3200;
        OnionRingHitFlashTimer = WrappingCountdown.Start(0);
        FatalBlowOrigin = Position16<RoomPixels>.Zero;
        FadeToBlackPaletteTimer = WrappingCountdown.Start(0);
        FadeToBlackPaletteIndex = 0;
        DeathExplosionTimer = WrappingCountdown.Start(0);
        DeathExplosionPatternIndex = 0;
        AttackPagesShown = 0;
        RoomLightsTransitionCounter = 0;
        SamusRainbowPaletteAnimationCounter = 0;
        SamusRainbowPhase = BabyMetroidSamusRainbowPhase.Inactive;
        Position = FixedPosition<RoomPixels>.FromWhole(new(320, 96)).WithFractionsOf(previousOccupant);
        Velocity = default;
        Speed = 0;
        Angle = RotationAngle.Zero;
        Phase = BabyMetroidCutscenePhase.DashOntoScreen;
        FunctionTimer = WrappingCountdown.Start(248);
    }

    /// <summary>
    /// Executes one complete main-AI call: function first, then the unconditional enemy
    /// velocity mover at <c>$A9:C782</c>. Flash/palette presentation is intentionally kept
    /// as inspectable state because it does not alter this entrance's coordinates.
    /// Every Baby routine reads the physical head enemy <c>Enemy[1]</c>'s coordinates, which
    /// the head AI has already moved this frame; <paramref name="head"/> carries them.
    /// </summary>
    public BabyMetroidCutsceneStepResult Step(
        SamusState samus,
        MotherBrainRainbowBeamAttackSequence motherBrain,
        Position16<RoomPixels> head,
        Position16<RoomPixels> layer1 = default,
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
        // `$A9:C98C-C9C2` emits three independent `$86:E509` allocations when the Baby
        // finally lets go. Keep the individual room coordinates and parameter instead of
        // collapsing that native side effect into a boolean: allocation order matters when
        // the shared eighteen-slot enemy-projectile pool is nearly full.
        var releaseDustClouds = new List<BabyMetroidReleaseDustRequest>(capacity: 3);
        bool ambientCrySoundQueued = false;
        bool samusRainbowActivated = false;
        DustExplosionRequest? deathExplosion = null;
        int? babyFadeShade = null;
        MotherBrainSpritePage? attackPage = null;
        int? recoveryLightsFrame = null;

        // EnemyCollisionHandling ($A0:903E) runs touch AI before main AI, against the Baby's
        // position from the previous frame. `$CF03` is active only during `$CA66`; broad
        // Samus/enemy hitboxes gate it, then its own acceleration-$10 helper decides when
        // both axes have reached the latch point. Main AI and its mover follow this frame.
        if (Phase == BabyMetroidCutscenePhase.LatchOntoSamus && TouchesSamus(samus))
        {
            bool reachedLatchPoint = AccelerateTowardsPoint(
                samus.Position.Whole + SamusLatchOffset,
                acceleration: 16);
            if (reachedLatchPoint)
            {
                Velocity = default;
                Phase = BabyMetroidCutscenePhase.HealSamusToFullHealth;
            }
        }

        switch (Phase)
        {
            case BabyMetroidCutscenePhase.DashOntoScreen:
                // `$F8` is decremented before BMI, so calls 1..248 leave the Baby still and
                // call 249 wraps zero to `$FFFF`. The expiry call immediately executes the
                // first curve update before the common mover consumes its new velocity.
                FunctionTimer = FunctionTimer.Decrement().Remaining;
                if (FunctionTimer.IsNegative)
                {
                    Angle = RotationAngle.FromTableIndex(216);
                    Speed = 2560;
                    Phase = BabyMetroidCutscenePhase.CurveTowardMotherBrainHead;
                    FunctionTimer = WrappingCountdown.Start(10);
                    goto case BabyMetroidCutscenePhase.CurveTowardMotherBrainHead;
                }
                break;

            case BabyMetroidCutscenePhase.CurveTowardMotherBrainHead:
                UpdateSpeedAndAngle(angleDelta: -384, targetAngle: RotationAngle.FromRaw(45056), targetSpeed: 2560);
                FunctionTimer = FunctionTimer.Decrement().Remaining;
                if (FunctionTimer.IsNegative)
                {
                    Phase = BabyMetroidCutscenePhase.GetRightUpInMotherBrainsFace;
                    FunctionTimer = WrappingCountdown.Start(9);
                }
                break;

            case BabyMetroidCutscenePhase.GetRightUpInMotherBrainsFace:
                UpdateSpeedAndAngle(angleDelta: -1536, targetAngle: RotationAngle.FromRaw(33280), targetSpeed: 3584);
                brainCollision = CollidesWithRectangle(
                    head,
                    TargetRadius);

                // Collision skips the timer decrement. Otherwise `$0009` permits ten
                // approach calls and expires when the tenth decrement wraps to `$FFFF`.
                if (!brainCollision)
                {
                    FunctionTimer = FunctionTimer.Decrement().Remaining;
                    brainCollision = FunctionTimer.IsNegative;
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
                    layer1);
                brainCollision = CollidesWithRectangle(target, new Extent16<RoomPixels>(8, 8));
                if (brainCollision)
                    Phase = BabyMetroidCutscenePhase.SetMotherBrainToStumbleBack;
                break;
            }

            case BabyMetroidCutscenePhase.SetMotherBrainToStumbleBack:
                // `$C879` passes animation-delay index two and target Body.X-1. With a
                // standing body this always installs the native really-fast backward list.
                bodyStumbleRequested = motherBrain.RequestBabyStumbleBackward();
                Phase = BabyMetroidCutscenePhase.ActivateRainbowBeamAndMotherBrainBody;
                goto case BabyMetroidCutscenePhase.ActivateRainbowBeamAndMotherBrainBody;

            case BabyMetroidCutscenePhase.ActivateRainbowBeamAndMotherBrainBody:
            {
                Position16<RoomPixels> target = head + BrainLatchOffset;
                bool reachedTarget = AccelerateTowardsPoint(target, acceleration: 512);
                if (reachedTarget)
                {
                    // The helper predicts whole-pixel overshoot but does not itself store
                    // its returned target coordinate. `$C8A4-$C8B7` performs the exact pin.
                    Velocity = default;
                    Position = Position.WithWhole(target);
                    SetProgram(MotherBrainBabyProgram.DrainingMotherBrain);
                    Phase = BabyMetroidCutscenePhase.WaitForMotherBrainToTurnToCorpse;
                    motherBrain.InterruptFinalBeamForBabyDrain();
                    latchSoundQueued = true; // Sound library one, effect `$40`.
                }
                break;
            }

            case BabyMetroidCutscenePhase.WaitForMotherBrainToTurnToCorpse:
            {
                int shakingIndex = (enemyFrameCounter & 6) >> 1;
                Position = Position.WithWhole(head + ShakingOffsets[shakingIndex] + BrainLatchOffset);
                if (motherBrain.State.Phase2CorpseState != MotherBrainPhase2CorpseState.Living)
                {
                    Phase = BabyMetroidCutscenePhase.StopDraining;
                    FunctionTimer = WrappingCountdown.Start(64);
                }
                break;
            }

            case BabyMetroidCutscenePhase.StopDraining:
                // Shaking ceases immediately: every wait call pins to the unoffset brain
                // coordinate before decrementing `$40`. BMI expires only after 65 calls.
                Position = Position.WithWhole(head + BrainLatchOffset);
                FunctionTimer = FunctionTimer.Decrement().Remaining;
                if (FunctionTimer.IsNegative)
                {
                    SetProgram(MotherBrainBabyProgram.Initial);
                    Phase = BabyMetroidCutscenePhase.LetGoAndSpawnDustClouds;
                    FunctionTimer = WrappingCountdown.Start(32);
                    Velocity = default;
                }
                break;

            case BabyMetroidCutscenePhase.LetGoAndSpawnDustClouds:
                // `$C94B` branches *into* `$C959` while the timer is nonnegative. Thus the
                // Baby accelerates ceilingward for all 32 visible release calls; the dust
                // burst happens on call 33 and that call also performs another acceleration.
                FunctionTimer = FunctionTimer.Decrement().Remaining;
                if (FunctionTimer.IsNegative)
                {
                    // `SpawnThreeDustCloudsOnMotherBrainHead` performs these calls in this
                    // exact order. Each helper adds its signed word offsets to the brain
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
                    layer1);
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
                // `$C9C3-$CA23` clears the ordinary cry request and enables health-based
                // palette selection on *every* route call. The random cry comparison uses
                // only the low twelve bits and succeeds for `$FA0-$FFF` (96 of 4096 seeds).
                CrySoundEnabled = false;
                HealthBasedPaletteEnabled = true;
                ambientCrySoundQueued = (randomNumber & 4095) >= 4000;

                // `$CA24-$CA64` is eight overlapping records. The fixed targets, divisor
                // indexes, and callback identities are compiled mechanics definitions;
                // sprite/instruction presentation remains separately cartridge-backed.
                BabyMetroidRouteRecord record = BabyMetroidRouteDefinitions.GetRecord(RouteLeg);
                Position16<RoomPixels> target = record.Target;
                int divisorIndex = record.AccelerationDivisorIndex;

                // `$F56A` is `[10,0F,...,01]`; every retail `$CA24` record uses index zero,
                // but implementing all legal indices costs nothing and catches malformed
                // data before a divide-by-zero or an invented fallback can hide it.
                if (divisorIndex > 15)
                {
                    throw new InvalidDataException(
                        $"Baby route leg {RouteLeg} has invalid divisor index {divisorIndex}.");
                }
                int accelerationDivisor = 16 - divisorIndex;
                int wrongWayExtra = BabyMetroidRouteDefinitions.GetWrongWayOffScreenXSpeed(
                    record.MovementFunction);

                GraduallyAccelerateTowardsPoint(
                    target,
                    accelerationDivisor,
                    wrongWayExtra,
                    layer1);

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
                // `$CA66` itself only steers toward Samus's centre minus twenty pixels. It
                // never performs the state change. That transition belongs to enemy-touch
                // handler `$CF03`, executed after main AI and the common mover below.
                GraduallyAccelerateTowardsPoint(
                    samus.Position.Whole + SamusLatchOffset,
                    accelerationDivisor: 16,
                    wrongWayOffScreenXSpeed: 1024,
                    layer1);
                break;

            case BabyMetroidCutscenePhase.HealSamusToFullHealth:
            {
                // `$CA7A` pins the actor to the four-entry shake table before healing. The
                // subsequent common mover is harmless because touch `$CF24/$CF27` zeroed
                // both velocities when it installed this function.
                CrySoundEnabled = false;
                int shakingIndex = (enemyFrameCounter & 6) >> 1;
                Position = Position.WithWhole(samus.Position.Whole + ShakingOffsets[shakingIndex] + SamusLatchOffset);

                // `$C59F` adds exactly one energy per enemy-processing call. CMP/BMI is a
                // signed 16-bit comparison; normal game maxima stay in its positive range.
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
                // `$A9:CABD` consumes Mother Brain's one-shot cry flag before inspecting
                // the hit-flash word. Sound queuing is returned by the ring system; this
                // actor owns only the resulting shake and health-to-function transition.
                if (!OnionRingHitFlashTimer.IsZero)
                {
                    int shakingIndex = (OnionRingHitFlashTimer.RawValue & 6) >> 1;
                    Offset16<RoomPixels> shake = ShakingOffsets[shakingIndex];
                    Position = Position.WithWhole(samus.Position.Whole + shake + shake + SamusLatchOffset);
                }

                if (Health == 0)
                {
                    // The zero reached by the ordinary volleys is a state trigger, not the
                    // health carried into the final charge. Native code deliberately gives
                    // the Baby `$0140` health, changes to the low-health palette function,
                    // disables health-band palettes, and lets `$CB13` run next frame.
                    Health = 320;
                    Phase = BabyMetroidCutscenePhase.ReleaseSamus;
                    HealthBasedPaletteEnabled = false;
                }
                break;

            case BabyMetroidCutscenePhase.ReleaseSamus:
                // `$A9:CB13` is a one-call cross-actor dispatcher. The native write to the
                // brain-slot speed word is presentation state; the meaningful body write
                // starts Mother Brain's fast retreat while this same call falls through to
                // the first flight update.
                motherBrain.PrepareForFinalBabyMetroidAttack();
                Phase = BabyMetroidCutscenePhase.StareDownMotherBrain;
                goto case BabyMetroidCutscenePhase.StareDownMotherBrain;

            case BabyMetroidCutscenePhase.StareDownMotherBrain:
                // Literal target from `$CB2D`: four pixels left of Samus and Y `$60`.
                // The helper's Y index zero means divisor `$10`. This leg and the next two
                // enter at `$A9:F466`, which stores a `$10` wrong-way off-screen X extra,
                // not the `$400` of the entrance helpers.
            {
                var target = new Position16<RoomPixels>(unchecked((ushort)(samus.Position.X.Whole - 4)), 96);
                GraduallyAccelerateTowardsPoint(
                    target,
                    accelerationDivisor: 16,
                    wrongWayOffScreenXSpeed: 16,
                    layer1);
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
                    layer1);
                if (CollidesWithRectangle(new Position16<RoomPixels>(272, 64), TargetRadius))
                    Phase = BabyMetroidCutscenePhase.MoveToFinalChargeStart;
                break;

            case BabyMetroidCutscenePhase.MoveToFinalChargeStart:
                // `$CB7B` stages the Baby at `(305,160)`. Reaching that rectangle assigns
                // exactly 79 HP so the final four-ring volley kills on its first collision,
                // clears the brain-slot speed flag, and replaces Mother Brain's retreat AI.
                GraduallyAccelerateTowardsPoint(
                    new Position16<RoomPixels>(305, 160),
                    accelerationDivisor: 16,
                    wrongWayOffScreenXSpeed: 16,
                    layer1);
                if (CollidesWithRectangle(new Position16<RoomPixels>(305, 160), TargetRadius))
                {
                    Health = 79;
                    motherBrain.ExecuteFinalBabyMetroidAttack();
                    Phase = BabyMetroidCutscenePhase.InitiateFinalCharge;
                }
                break;

            case BabyMetroidCutscenePhase.InitiateFinalCharge:
                // Divisor-table index `$A` maps to `$10-$A == 6`. This helper family adds
                // `$400` only when an off-screen actor is moving the wrong horizontal way.
                GraduallyAccelerateTowardsPoint(
                    new Position16<RoomPixels>(290, 128),
                    accelerationDivisor: 6,
                    wrongWayOffScreenXSpeed: 1024,
                    layer1);
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
                    layer1);
                if (Health == 0)
                {
                    // `$A9:CBF2-$CC3D` changes graphics/list state, freezes the body AI,
                    // clears velocity, remembers the shake origin, and falls through into
                    // the first of 17 fatal-blow shake calls.
                    TileBase = BabyTileBase;
                    SetProgram(MotherBrainBabyProgram.TakingFatalBlow);
                    Velocity = default;
                    Phase = BabyMetroidCutscenePhase.TakeFinalBlow;
                    FunctionTimer = WrappingCountdown.Start(16);
                    FatalBlowOrigin = Position.Whole;
                    goto case BabyMetroidCutscenePhase.TakeFinalBlow;
                }
                break;

            case BabyMetroidCutscenePhase.TakeFinalBlow:
            {
                // `ShakeBabyMetroidCutscene` ($A9:CEDB) first adds `$FFFF` to the Y velocity,
                // which the common mover then applies, and places the Baby at the saved origin
                // plus the undoubled shaking offsets.
                Velocity = Velocity.WithY(new SignedEightEight(unchecked((ushort)(Velocity.Y.RawValue - 1))));
                int shakingIndex = (enemyFrameCounter & 6) >> 1;
                Position = Position.WithWhole(FatalBlowOrigin + ShakingOffsets[shakingIndex]);
                FunctionTimer = FunctionTimer.Decrement().Remaining;
                if (FunctionTimer.IsNegative)
                {
                    Position = Position.WithWhole(FatalBlowOrigin);
                    Phase = BabyMetroidCutscenePhase.PlaySamusTheme;
                    FunctionTimer = WrappingCountdown.Start(56);
                    goto case BabyMetroidCutscenePhase.PlaySamusTheme;
                }
                break;
            }

            case BabyMetroidCutscenePhase.PlaySamusTheme:
                // The expiry call queues the two delayed music words and immediately
                // performs the first `$000C` prepare-Hyper-Beam decrement.
                FunctionTimer = FunctionTimer.Decrement().Remaining;
                if (FunctionTimer.IsNegative)
                {
                    Phase = BabyMetroidCutscenePhase.PrepareSamusForHyperBeam;
                    FunctionTimer = WrappingCountdown.Start(12);
                    goto case BabyMetroidCutscenePhase.PrepareSamusForHyperBeam;
                }
                break;

            case BabyMetroidCutscenePhase.PrepareSamusForHyperBeam:
                FunctionTimer = FunctionTimer.Decrement().Remaining;
                if (FunctionTimer.IsNegative)
                {
                    // Samus command `$19` freezes the drained animation at byte-index 28.
                    // `$CC8B` also installs the first rainbow handler; neither handler is
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
                    // installs `$CCC0`, and waits through `$80..0` before the first DMA.
                    Properties = Properties.With(EnemyProperties.Invisible);
                    Phase = BabyMetroidCutscenePhase.UnloadTiles;
                    FunctionTimer = WrappingCountdown.Start(128);
                }
                else
                {
                    deathExplosion = StepDeathExplosion();
                    // `$CE24` clears invisibility on odd enemy frames and sets it on even.
                    Properties = (enemyFrameCounter & 1) != 0
                        ? Properties.Without(EnemyProperties.Invisible)
                        : Properties.With(EnemyProperties.Invisible);
                }
                break;

            case BabyMetroidCutscenePhase.UnloadTiles:
                StepSamusRainbowPaletteAnimation(samus, ref samusRainbowActivated);
                FunctionTimer = FunctionTimer.Decrement().Remaining;
                if (FunctionTimer.IsNegative)
                {
                    int attackPages = MotherBrainSpritePage.PageCount(MotherBrainSpritePageSheet.Attack);
                    if (AttackPagesShown >= attackPages)
                        throw new InvalidOperationException("Every Mother Brain attack page has already shown again.");

                    attackPage = MotherBrainSpritePage.Of(MotherBrainSpritePageSheet.Attack, AttackPagesShown);
                    AttackPagesShown++;

                    if (AttackPagesShown == attackPages)
                    {
                        // ProcessSpriteTilesTransfers observes the zero terminator after
                        // publishing entry four. Native falls straight into `$CCDE`, so the
                        // newly written `$B0` becomes `$AF` on this same call.
                        Phase = BabyMetroidCutscenePhase.LetSamusRainbowSomeMore;
                        FunctionTimer = WrappingCountdown.Start(176);
                        goto case BabyMetroidCutscenePhase.LetSamusRainbowSomeMore;
                    }
                }
                break;

            case BabyMetroidCutscenePhase.LetSamusRainbowSomeMore:
                FunctionTimer = FunctionTimer.Decrement().Remaining;
                if (FunctionTimer.IsNegative)
                {
                    Phase = BabyMetroidCutscenePhase.FinalCutscene;
                    RoomLightsTransitionCounter = 0;
                    goto case BabyMetroidCutscenePhase.FinalCutscene;
                }
                break;

            case BabyMetroidCutscenePhase.FinalCutscene:
            {
                // `$CCF0` increments the shared counter, passes its previous value to
                // `$AD:F24B`, and treats pointer-table entry seven as the carry-set end.
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
                Properties = Properties.With(EnemyProperties.Deleted);
                break;
            }

            case BabyMetroidCutscenePhase.Inactive:
            default:
                throw new InvalidOperationException($"Unsupported Baby Metroid phase {Phase}.");
        }

        // Main AI calls this even on phase transitions and target snaps. In the snap case
        // both velocities were explicitly zeroed, so the pin remains exact.
        MoveAccordingToVelocity();

        // `$A9:C79C` is called after the common velocity mover. It decrements the same
        // extra word that onion-ring hits write and shows palette zero while bit one of the
        // decremented value is set. Starting from `$10`, the first post-hit enemy call sees
        // `$10` for its shake above, then displays the normal palette with timer `$0F`.
        if (!OnionRingHitFlashTimer.IsZero)
        {
            OnionRingHitFlashTimer = OnionRingHitFlashTimer.Decrement().Remaining;
            HitFlashPaletteShown = (OnionRingHitFlashTimer.RawValue & 2) != 0;
        }
        else
        {
            HitFlashPaletteShown = false;
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
            attackPage,
            recoveryLightsFrame);
    }

    /// <summary>Applies the Baby's health/flash mutation from the blue-ring collision handler at $86:C381–C3A8: a living Baby gets a $0010 flash timer and health subtraction saturated at zero.</summary>
    /// <param name="damage">Unsigned health units to subtract, defaulting to the native $0050 (80); even zero damage refreshes the flash timer when health is nonzero.</param>
    /// <returns>An empty compatibility result; the effects are written directly to <see cref="Health"/> and <see cref="OnionRingHitFlashTimer"/>.</returns>
    /// <remarks>Zero health leaves all state unchanged. The caller must establish overlap and owns the Baby cry request, physical enemy-slot synchronization, and ring explosion/deletion.</remarks>
    public BabyMetroidOnionRingHitResult ApplyMotherBrainOnionRingHit(int damage = 80)
    {
        int healthBefore = Health;
        if (healthBefore == 0)
            return new BabyMetroidOnionRingHitResult();

        OnionRingHitFlashTimer = WrappingCountdown.Start(16);
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
                // `$CD30` uses a signed CMP/BMI after adding sixteen to the Baby's Y.
                if (unchecked((short)(Position.Y.Whole + 16 - samus.Position.Y.Whole)) < 0)
                    return;
                samus.Drained.EnableRainbow(samus);
                samusRainbowActivated = true;
                SamusRainbowPhase = BabyMetroidSamusRainbowPhase.GraduallySlowAnimationDown;
                return;

            case BabyMetroidSamusRainbowPhase.GraduallySlowAnimationDown:
            {
                (int fraction, bool frameCompleted) = AdvanceSamusRainbowFraction(SamusRainbowPaletteAnimationCounter);
                SamusRainbowPaletteAnimationCounter = fraction;
                if (frameCompleted)
                    samus.Drained.IncrementRainbowPaletteFrame(maximumFrame: 10);
                return;
            }

            default:
                throw new InvalidOperationException($"Unsupported Samus rainbow phase {SamusRainbowPhase}.");
        }
    }

    /// <summary>Reduces horizontal 8.8 speed magnitude by $20 and adds two raw units of downward velocity.</summary>
    private void AccelerateDownwardsForDeath()
    {
        // `$CE40` subtracts `$20` from the magnitude, clamps at zero, reapplies the old
        // sign, and independently adds two to vertical 8.8 velocity with 16-bit wrap.
        // The slowed magnitude is at most 32736, so it always fits a signed 8.8 velocity.
        int xVelocity = Velocity.X.SignedRawValue;
        int magnitude = Math.Max(0, Math.Abs(xVelocity) - 32);
        Velocity = new(
            SignedEightEight.FromSignedRaw((short)(xVelocity < 0 ? -magnitude : magnitude)),
            new SignedEightEight(unchecked((ushort)(Velocity.Y.RawValue + 2))));
    }

    /// <summary>Advances the Baby's black-fade shade every nine calls after its center reaches Y=$80, selecting the shade it shows.</summary>
    private bool StepFadeToBlack(ref int? fadeShade)
    {
        // No timer changes occur until the actor's centre reaches Y `$80`.
        if (unchecked((short)(Position.Y.Whole - 128)) < 0)
            return false;

        FadeToBlackPaletteTimer = FadeToBlackPaletteTimer.Decrement().Remaining;
        if (!FadeToBlackPaletteTimer.IsNegative)
            return false;

        FadeToBlackPaletteTimer = WrappingCountdown.Start(8);
        int nextIndex = FadeToBlackPaletteIndex + 1;
        if (nextIndex >= 7)
            return true;

        FadeToBlackPaletteIndex = nextIndex;
        fadeShade = nextIndex;
        return false;
    }

    /// <summary>Emits a dust explosion every five calls using the shared Ridley scatter pattern.</summary>
    private DustExplosionRequest? StepDeathExplosion()
    {
        DeathExplosionTimer = DeathExplosionTimer.Decrement().Remaining;
        if (!DeathExplosionTimer.IsNegative)
            return null;

        DeathExplosionTimer = WrappingCountdown.Start(4);
        DeathExplosionPatternIndex++;
        if (DeathExplosionPatternIndex >= DeathExplosionScatterDefinitions.Count)
            DeathExplosionPatternIndex = 0;
        // `$A9:CDFC` holds the scatter shared with Ridley's death. The handler increments its
        // index before looking up a pair, so a freshly cleared counter begins at entry one.
        return new DustExplosionRequest(
            Position: Position.Whole + DeathExplosionScatterDefinitions.Offset(DeathExplosionPatternIndex),
            Animation: MiscDustAnimation.SmallExplosion,
            SoundEffect: MotherBrainSoundDefinitions.Explosion);
    }

    /// <summary>Switches the Baby's program and restarts its instruction timer.</summary>
    private void SetProgram(MotherBrainBabyProgram program)
    {
        Program = program;
        InstructionTimer = WrappingCountdown.Start(1);
    }

    /// <summary>Clamps speed and angle toward their targets before deriving 8.8 X/Y velocity from native sine values.</summary>
    private void UpdateSpeedAndAngle(
        int angleDelta,
        RotationAngle targetAngle,
        int targetSpeed)
    {
        // `$CF31` changes speed by exactly `$20` and clamps rather than crossing the target.
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
            Angle = unchecked((short)(candidateAngle.RawValue - targetAngle.RawValue)) >= 0
                ? candidateAngle
                : targetAngle;
        }
        else
        {
            Angle = unchecked((short)(candidateAngle.RawValue - targetAngle.RawValue)) < 0
                ? candidateAngle
                : targetAngle;
        }

        ByteAngle angle = new(Angle.TableIndex);
        Velocity = new(
            new SignedEightEight(CalculateVelocityComponent(Speed, angle)),
            new SignedEightEight(CalculateVelocityComponent(Speed, angle.AddTableUnits(ByteAngle.QuarterTurn.RawValue))));
    }

    /// <summary>Multiplies speed by a signed sine-table entry with the cartridge's bit-8-to-23 result.</summary>
    /// <param name="speed">The unsigned 8.8 speed, 0 through 65535.</param>
    /// <param name="angle">The sine-table angle.</param>
    internal static ushort CalculateVelocityComponent(
        int speed,
        ByteAngle angle)
    {
        // Bank `$86:C27A` multiplies unsigned speed by the absolute signed-table value,
        // returns product bits 8..23, then reapplies the original sign.
        return EnemyTrigonometryTables.MultiplySignedSine(checked((ushort)speed), angle);
    }

    /// <summary>Advances native 16.16 position from 8.8 velocity while leaving the low subposition byte untouched.</summary>
    private void MoveAccordingToVelocity()
    {
        // SEP #$20 adds velocity's low byte to subposition byte +1. REP #$20 then sign-
        // extends velocity's high byte, retaining the 8-bit ADC carry for whole pixels. That
        // is exactly a 16.16 add of the 8.8 velocity shifted left eight bits: the low
        // subposition byte receives zero and the carry out of the fraction enters the whole.
        Position = Position.Advance(Velocity.ToSixteenSixteen());
    }

    /// <summary>
    /// <c>Enemy_vs_Samus_CollisionHandling</c> ($A0:A07A-$A0FE) for the Baby's radius hitbox.
    /// A non-zero contact-damage index clears Samus's invincibility and proceeds; otherwise an
    /// invincible Samus is not touched. Each axis then requires
    /// <c>|delta| - SamusRadius &lt; enemy radius</c> (a delta inside Samus's own radius
    /// always passes), which is strict, unlike <c>$A9:EF06</c>.
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
            int distance = Math.Abs(unchecked((short)(samusPosition - enemyPosition)));
            return distance < samusRadius || distance - samusRadius < enemyRadius;
        }
    }

    /// <summary>Uses the Baby's 36-pixel radii and inclusive native rectangle overlap for route targets.</summary>
    private bool CollidesWithRectangle(
        Position16<RoomPixels> center,
        Extent16<RoomPixels> rectangleRadius)
    {
        // `$A9:EF06` adds both radii and one, then rejects `abs(delta) >= sum+1`.
        // In the normal room-coordinate domain that is equivalently `abs(delta) <= sum`.
        int xDistance = Math.Abs(unchecked((short)(center.X - Position.X.Whole)));
        if (xDistance > rectangleRadius.X + XHitboxRadius)
            return false;
        int yDistance = Math.Abs(unchecked((short)(center.Y - Position.Y.Whole)));
        return yDistance <= rectangleRadius.Y + YHitboxRadius;
    }

    /// <summary>Accelerates toward a target using native axis helpers and the offscreen wrong-way X correction.</summary>
    private void GraduallyAccelerateTowardsPoint(
        Position16<RoomPixels> target,
        int accelerationDivisor,
        int wrongWayOffScreenXSpeed,
        Position16<RoomPixels> layer1)
    {
        Velocity = Velocity.WithX(BabyMetroidGradualAcceleration.AccelerateHorizontally(
            Position.X.Whole, target.X, Velocity.X, accelerationDivisor, wrongWayOffScreenXSpeed,
            () => IsVaguelyOffScreen(layer1)));
        Velocity = Velocity.WithY(BabyMetroidGradualAcceleration.AccelerateVertically(
            Position.Y.Whole, target.Y, Velocity.Y, accelerationDivisor));
    }

    /// <summary>Checks the generous signed camera rectangle used by the Baby's wrong-way movement recovery.</summary>
    private bool IsVaguelyOffScreen(Position16<RoomPixels> layer1)
    {
        // This is the signed-branch sequence at `$A9:F57A`; its generous rectangle extends
        // 16 pixels left/right and 96 pixels vertically beyond the ordinary 256x224 view.
        if (unchecked((short)Position.Y.Whole) < 0)
            return true;
        int relativeY = unchecked((short)(Position.Y.Whole + 96 - layer1.Y));
        if (relativeY is < 0 or >= 416)
            return true;
        if (unchecked((short)Position.X.Whole) < 0)
            return true;
        int relativeX = unchecked((short)(Position.X.Whole + 16 - layer1.X));
        return relativeX is < 0 or >= 288;
    }

    /// <summary>Predicts both axis crossings and reports arrival only when X and Y reach their targets together.</summary>
    private bool AccelerateTowardsPoint(Position16<RoomPixels> target, int acceleration)
    {
        // `$F5A6` counts axes whose next whole-pixel prediction reaches/crosses target,
        // then shifts the count twice. Carry is set only for count two.
        bool reachedX = AccelerateTowardsXPosition(target.X, acceleration);
        bool reachedY = AccelerateTowardsYPosition(target.Y, acceleration);
        return reachedX && reachedY;
    }

    /// <summary>Accelerates Y toward its target at at most five pixels per frame and zeroes velocity on crossing.</summary>
    private bool AccelerateTowardsYPosition(int targetY, int acceleration)
    {
        int difference = unchecked((short)(Position.Y.Whole - targetY));
        if (difference == 0)
            return true;

        if (difference < 0)
        {
            int velocity = Math.Min(Velocity.Y.SignedRawValue + acceleration, 1280);
            Velocity = Velocity.WithY(new SignedEightEight(unchecked((ushort)velocity)));
            int prediction = unchecked((ushort)(Position.Y.Whole + unchecked((sbyte)(EightEight.WholeOf(Velocity.Y.RawValue)))));
            if (unchecked((short)(prediction - targetY)) < 0)
                return false;
        }
        else
        {
            int velocity = Math.Max(Velocity.Y.SignedRawValue - acceleration, -1280);
            Velocity = Velocity.WithY(new SignedEightEight(unchecked((ushort)velocity)));
            int prediction = unchecked((ushort)(Position.Y.Whole + unchecked((sbyte)(EightEight.WholeOf(Velocity.Y.RawValue)))));
            int predictedDifference = unchecked((short)(prediction - targetY));
            if (predictedDifference > 0)
                return false;
        }

        Velocity = Velocity.WithY(default);
        return true;
    }

    /// <summary>Accelerates X toward its target with native equality choosing the left branch and zeroes velocity on crossing.</summary>
    private bool AccelerateTowardsXPosition(int targetX, int acceleration)
    {
        int difference = unchecked((short)(Position.X.Whole - targetX));
        if (difference < 0)
        {
            int velocity = Math.Min(Velocity.X.SignedRawValue + acceleration, 1280);
            Velocity = Velocity.WithX(new SignedEightEight(unchecked((ushort)velocity)));
            int prediction = unchecked((ushort)(Position.X.Whole + unchecked((sbyte)(EightEight.WholeOf(Velocity.X.RawValue)))));
            if (unchecked((short)(prediction - targetX)) < 0)
                return false;
        }
        else
        {
            // Native deliberately sends exact equality through the left branch; unlike Y,
            // there is no BEQ before BPL at `$A9:F619-$F61B`.
            int velocity = Math.Max(Velocity.X.SignedRawValue - acceleration, -1280);
            Velocity = Velocity.WithX(new SignedEightEight(unchecked((ushort)velocity)));
            int prediction = unchecked((ushort)(Position.X.Whole + unchecked((sbyte)(EightEight.WholeOf(Velocity.X.RawValue)))));
            int predictedDifference = unchecked((short)(prediction - targetX));
            if (predictedDifference > 0)
                return false;
        }

        Velocity = Velocity.WithX(default);
        return true;
    }

    /// <summary>The 4x4 half-extent rectangle every route and staging target is tested with.</summary>
    private static Extent16<RoomPixels> TargetRadius => new(4, 4);
}

