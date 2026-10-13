using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

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
    /// <summary>
    /// Public debugger witness for <c>InstList_BabyMetroid_Initial</c> at $A9:CFA2.
    /// </summary>
    public const ushort InitialInstructionList =
        MotherBrainBabyInstructionProgramDefinitions.Initial;

    /// <summary>
    /// Public debugger witness for <c>InstList_BabyMetroid_DrainingMotherBrain</c> at
    /// $A9:CFB8.
    /// </summary>
    public const ushort DrainingMotherBrainInstructionList =
        MotherBrainBabyInstructionProgramDefinitions.DrainingMotherBrain;
    // `$A9:93BB-$93CA` is shared by Mother Brain's brain shake and the latched Baby.
    // `Enemy.frameCounter & 6` is a byte offset into these four X/Y entries. The jitter
    // is authored: X steps 0,-1,0,1 but Y does not mirror it (see residualScalarInputsReview).
    private static readonly Offset16<RoomPixels>[] ShakingOffsets =
        [new(0, 0), new(-1, 1), new(0, -1), new(1, 1)];

    // The Baby's latch points hang below their anchors: 24 pixels above Mother Brain's
    // brain and 20 above Samus's centre.
    private static readonly Offset16<RoomPixels> BrainLatchOffset = new(0, -0x0018);
    private static readonly Offset16<RoomPixels> SamusLatchOffset = new(0, -0x0014);

    // Enemy header `$A0:ECBF` declares width/height `$24`. Despite the header macro's
    // friendly names, `$A0:8AFF/$8B05` copy those words directly into the enemy slot's
    // X/Y *radius* fields; there is no diameter-to-radius division anywhere in between.
    /// <summary>Horizontal collision half-extent, 36 room pixels from the Baby's center, copied without halving from <c>EnemyHeaders_BabyMetroidCutscene</c> at $A0:ECBF.</summary>
    public const ushort XHitboxRadius = 0x0024;
    /// <summary>Vertical collision half-extent, 36 room pixels from the Baby's center, copied without halving from <c>EnemyHeaders_BabyMetroidCutscene</c> at $A0:ECBF.</summary>
    public const ushort YHitboxRadius = 0x0024;

    // Shared bank-$86 component math indexes this 16-bit sign-extended table with an
    // eight-bit angle. Angle zero points down; positive rotation is anti-clockwise.

    /// <summary>Current native function-pointer equivalent.</summary>
    public BabyMetroidCutscenePhase Phase { get; internal set; } =
        BabyMetroidCutscenePhase.Inactive;

    /// <summary>Enemy properties after initialization ORs in <c>$3000</c>.</summary>
    public ushort Properties { get; private set; }

    /// <summary>Enemy palette word. Flashing may later alternate this with zero.</summary>
    public ushort Palette { get; private set; }

    /// <summary>Enemy graphics offset selecting the four transferred Baby tile rows.</summary>
    public ushort GraphicsOffset { get; private set; }

    /// <summary>Current bank-$A9 enemy instruction list.</summary>
    public ushort InstructionList { get; private set; }

    /// <summary>Enemy instruction timer reset to one whenever the list changes.</summary>
    public ushort InstructionTimer { get; private set; }

    /// <summary>Enemy instruction loop counter.</summary>
    public ushort InstructionLoopCounter { get; private set; }

    /// <summary>
    /// Native 16.16 room position. Only the high byte of each subposition participates in
    /// this actor's 8.8 mover; the low bytes are deliberately retained and untouched.
    /// </summary>
    public FixedPosition<RoomPixels> Position { get; internal set; }

    /// <summary>Signed 8.8 velocity.</summary>
    public EightEightVector<RoomPixels> Velocity { get; internal set; }

    /// <summary>Unsigned 8.8 magnitude used by the curved entrance.</summary>
    public ushort Speed { get; private set; }

    /// <summary>8.8 angle; only the high byte is passed to shared component math.</summary>
    public ushort Angle { get; private set; }

    /// <summary>Native function timer, decremented with 16-bit wrap and BMI tests.</summary>
    public ushort FunctionTimer { get; private set; }

    /// <summary>Normal-palette handler delay initialized to ten and changed to one at latch.</summary>
    public ushort PaletteHandlerDelay { get; private set; }

    /// <summary>Initial request to permit the Baby cry effect.</summary>
    public bool CrySoundEnabled { get; private set; }

    /// <summary>Health-palette gate, initially disabled for the entrance.</summary>
    public bool HealthBasedPaletteEnabled { get; private set; }

    /// <summary>
    /// Bank-$A9 movement-table pointer installed after the ceiling collision. Zero means
    /// the table-driven ceiling-to-Samus route has not started yet.
    /// </summary>
    public ushort MovementTablePointer { get; private set; }

    /// <summary>
    /// Native enemy health. Header <c>$A0:ECBF</c> supplies 3,200; Mother Brain later drains
    /// this word after the healing hold, so it belongs to actor state rather than rendering.
    /// </summary>
    public ushort Health { get; internal set; }

    /// <summary>
    /// Extra enemy word <c>$7E:7806,x</c>. Mother Brain's onion-ring collision handler
    /// writes <c>$10</c> here. Main AI observes the nonzero value as a doubled shake, then
    /// the later flashing handler decrements it and alternates the enemy palette.
    /// </summary>
    public ushort OnionRingHitFlashTimer { get; private set; }

    /// <summary>
    /// The low-health palette countdown installed by <c>$A9:CADA</c> when the ordinary
    /// ring volleys have reduced health to zero. This is separate from the hit-flash word.
    /// </summary>
    public ushort LowHealthPaletteTimer { get; private set; }

    /// <summary>Saved whole-pixel origin used by the fatal-blow shake at <c>$A9:CC3E</c>.</summary>
    public Position16<RoomPixels> FatalBlowOrigin { get; private set; }

    /// <summary>Reload-eight timer producing the native nine-call black-palette cadence.</summary>
    public ushort FadeToBlackPaletteTimer { get; private set; }

    /// <summary>Current `$AD:E8E2` black-fade palette index; valid stored values are 0..6.</summary>
    public ushort FadeToBlackPaletteIndex { get; private set; }

    /// <summary>Five-call cadence word for the dust explosions surrounding the dying Baby.</summary>
    public ushort DeathExplosionTimer { get; private set; }

    /// <summary>
    /// Host name for the native layout alias called Mother Brain body's walk counter.
    /// Long-indexed access uses the Baby's slot, so this actor owns its increment/wrap.
    /// </summary>
    public ushort DeathExplosionPatternIndex { get; private set; }

    /// <summary>Zero-based index of the next `$A9:8FC7` attack-tile DMA record.</summary>
    public ushort AttackTileTransferIndex { get; private set; }

    /// <summary>Palette-table argument used while restoring the phase-three room lights.</summary>
    public ushort RoomLightsTransitionCounter { get; private set; }

    /// <summary>Fractional `$0300` accumulator used to slow the rainbow Samus palette.</summary>
    public ushort SamusRainbowPaletteAnimationCounter { get; private set; }

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
        ushort populationProperties =
            (ushort)(EnemyProperties.ProcessInstructions |
                EnemyProperties.ProcessOffScreen))
    {
        Properties = populationProperties.With(
            EnemyProperties.BlocksPlasmaBeam |
            EnemyProperties.ProcessInstructions);
        Palette = EnemyPaletteBits.Palette7;
        GraphicsOffset = 0x00a0;
        SetInstructionList(InitialInstructionList);
        CrySoundEnabled = true;
        PaletteHandlerDelay = 0x000a;
        HealthBasedPaletteEnabled = false;
        MovementTablePointer = 0;
        Health = 3200;
        OnionRingHitFlashTimer = 0;
        LowHealthPaletteTimer = 0;
        FatalBlowOrigin = Position16<RoomPixels>.Zero;
        FadeToBlackPaletteTimer = 0;
        FadeToBlackPaletteIndex = 0;
        DeathExplosionTimer = 0;
        DeathExplosionPatternIndex = 0;
        AttackTileTransferIndex = 0;
        RoomLightsTransitionCounter = 0;
        SamusRainbowPaletteAnimationCounter = 0;
        SamusRainbowPhase = BabyMetroidSamusRainbowPhase.Inactive;
        Position = FixedPosition<RoomPixels>.FromWhole(new(0x0140, 0x0060)).WithFractionsOf(previousOccupant);
        Velocity = default;
        Speed = 0;
        Angle = 0;
        Phase = BabyMetroidCutscenePhase.DashOntoScreen;
        FunctionTimer = 0x00f8;
    }

    /// <summary>
    /// Executes one complete main-AI call: function first, then the unconditional enemy
    /// velocity mover at <c>$A9:C782</c>. Flash/palette presentation is intentionally kept
    /// as inspectable state because it does not alter this entrance's coordinates.
    /// Every Baby routine reads the physical head enemy <c>Enemy[1]</c>'s coordinates, which
    /// the head AI has already moved this frame; <paramref name="head"/> carries them.
    /// </summary>
    public BabyMetroidCutsceneStepResult Step(
        ISnesAddressSpace bus,
        SamusState samus,
        MotherBrainRainbowBeamAttackSequence motherBrain,
        Position16<RoomPixels> head,
        Position16<RoomPixels> layer1 = default,
        ushort enemyFrameCounter = 0,
        ushort randomNumber = 0)
    {
        Ensure.NotNull(bus);
        Ensure.NotNull(samus);
        Ensure.NotNull(motherBrain);
        if (Phase == BabyMetroidCutscenePhase.Inactive)
            throw new InvalidOperationException("The cutscene Baby Metroid has not been initialized.");

        BabyMetroidCutscenePhase phaseBefore = Phase;
        BabyMetroidCutscenePoint before = Capture();
        bool brainCollision = false;
        bool bodyStumbleRequested = false;
        bool latchSoundQueued = false;
        // `$A9:C98C-C9C2` emits three independent `$86:E509` allocations when the Baby
        // finally lets go. Keep the individual room coordinates and parameter instead of
        // collapsing that native side effect into a boolean: allocation order matters when
        // the shared eighteen-slot enemy-projectile pool is nearly full.
        var releaseDustClouds = new List<BabyMetroidReleaseDustRequest>(capacity: 3);
        bool ambientCrySoundQueued = false;
        bool samusRainbowActivated = false;
        BabyMetroidDeathExplosionRequest? deathExplosion = null;
        BabyMetroidPaletteTransferRequest? babyPaletteTransfer = null;
        MotherBrainSpriteTileTransferRequest? attackTileTransfer = null;
        MotherBrainBackgroundPaletteTransferRequest? backgroundPaletteTransfer = null;

        // EnemyCollisionHandling ($A0:903E) runs touch AI before main AI, against the Baby's
        // position from the previous frame. `$CF03` is active only during `$CA66`; broad
        // Samus/enemy hitboxes gate it, then its own acceleration-$10 helper decides when
        // both axes have reached the latch point. Main AI and its mover follow this frame.
        if (Phase == BabyMetroidCutscenePhase.LatchOntoSamus && TouchesSamus(samus))
        {
            bool reachedLatchPoint = AccelerateTowardsPoint(
                samus.Position.Whole + SamusLatchOffset,
                acceleration: 0x0010);
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
                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) != 0)
                {
                    Angle = 0xd800;
                    Speed = 0x0a00;
                    Phase = BabyMetroidCutscenePhase.CurveTowardMotherBrainHead;
                    FunctionTimer = 0x000a;
                    goto case BabyMetroidCutscenePhase.CurveTowardMotherBrainHead;
                }
                break;

            case BabyMetroidCutscenePhase.CurveTowardMotherBrainHead:
                UpdateSpeedAndAngle(angleDelta: 0xfe80, targetAngle: 0xb000, targetSpeed: 0x0a00);
                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) != 0)
                {
                    Phase = BabyMetroidCutscenePhase.GetRightUpInMotherBrainsFace;
                    FunctionTimer = 0x0009;
                }
                break;

            case BabyMetroidCutscenePhase.GetRightUpInMotherBrainsFace:
                UpdateSpeedAndAngle(angleDelta: 0xfa00, targetAngle: 0x8200, targetSpeed: 0x0e00);
                brainCollision = CollidesWithRectangle(
                    head,
                    TargetRadius);

                // Collision skips the timer decrement. Otherwise `$0009` permits ten
                // approach calls and expires when the tenth decrement wraps to `$FFFF`.
                if (!brainCollision)
                {
                    FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                    brainCollision = (FunctionTimer & 0x8000) != 0;
                }

                if (brainCollision)
                {
                    Phase = BabyMetroidCutscenePhase.LatchOntoMotherBrain;
                    samus.Drained.PutStanding(bus, samus); // Drained Samus command one.
                }
                break;

            case BabyMetroidCutscenePhase.LatchOntoMotherBrain:
            {
                Position16<RoomPixels> target = head + BrainLatchOffset;
                GraduallyAccelerateTowardsPoint(
                    target,
                    accelerationDivisor: 0x10,
                    wrongWayOffScreenXSpeed: 0x0400,
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
                bool reachedTarget = AccelerateTowardsPoint(target, acceleration: 0x0200);
                if (reachedTarget)
                {
                    // The helper predicts whole-pixel overshoot but does not itself store
                    // its returned target coordinate. `$C8A4-$C8B7` performs the exact pin.
                    Velocity = default;
                    Position = Position.WithWhole(target);
                    SetInstructionList(DrainingMotherBrainInstructionList);
                    Phase = BabyMetroidCutscenePhase.WaitForMotherBrainToTurnToCorpse;
                    PaletteHandlerDelay = 1;
                    motherBrain.InterruptFinalBeamForBabyDrain();
                    latchSoundQueued = true; // Sound library one, effect `$40`.
                }
                break;
            }

            case BabyMetroidCutscenePhase.WaitForMotherBrainToTurnToCorpse:
            {
                int shakingIndex = (enemyFrameCounter & 6) >> 1;
                Position = Position.WithWhole(head + ShakingOffsets[shakingIndex] + BrainLatchOffset);
                if (motherBrain.Phase2CorpseState != 0)
                {
                    Phase = BabyMetroidCutscenePhase.StopDraining;
                    FunctionTimer = 0x0040;
                }
                break;
            }

            case BabyMetroidCutscenePhase.StopDraining:
                // Shaking ceases immediately: every wait call pins to the unoffset brain
                // coordinate before decrementing `$40`. BMI expires only after 65 calls.
                Position = Position.WithWhole(head + BrainLatchOffset);
                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) != 0)
                {
                    SetInstructionList(InitialInstructionList);
                    PaletteHandlerDelay = 0x000a;
                    Phase = BabyMetroidCutscenePhase.LetGoAndSpawnDustClouds;
                    FunctionTimer = 0x0020;
                    Velocity = default;
                }
                break;

            case BabyMetroidCutscenePhase.LetGoAndSpawnDustClouds:
                // `$C94B` branches *into* `$C959` while the timer is nonnegative. Thus the
                // Baby accelerates ceilingward for all 32 visible release calls; the dust
                // burst happens on call 33 and that call also performs another acceleration.
                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) != 0)
                {
                    // `SpawnThreeDustCloudsOnMotherBrainHead` performs these calls in this
                    // exact order. Each helper adds its signed word offsets to the brain
                    // enemy's current room coordinate and passes animation parameter nine
                    // to the ordinary highest-free-slot allocator.
                    releaseDustClouds.Add(new BabyMetroidReleaseDustRequest(
                        head + new Offset16<RoomPixels>(-0x0010, -0x0008),
                        ProjectileParameter: 0x0009));
                    releaseDustClouds.Add(new BabyMetroidReleaseDustRequest(
                        head + new Offset16<RoomPixels>(0, -0x0010),
                        ProjectileParameter: 0x0009));
                    releaseDustClouds.Add(new BabyMetroidReleaseDustRequest(
                        head + new Offset16<RoomPixels>(0x0010, -0x0008),
                        ProjectileParameter: 0x0009));
                    Phase = BabyMetroidCutscenePhase.MoveToTheCeiling;
                }
                goto case BabyMetroidCutscenePhase.MoveToTheCeiling;

            case BabyMetroidCutscenePhase.MoveToTheCeiling:
            {
                Position16<RoomPixels> target = head.WithY(0);
                GraduallyAccelerateTowardsPoint(
                    target,
                    accelerationDivisor: 0x10,
                    wrongWayOffScreenXSpeed: 0x0400,
                    layer1);
                bool ceilingCollision = CollidesWithRectangle(target, TargetRadius);
                brainCollision = ceilingCollision;
                if (ceilingCollision)
                {
                    samus.Drained.PutCrouchingOrFalling(bus, samus);
                    Phase = BabyMetroidCutscenePhase.MoveToSamus;
                    MovementTablePointer = BabyMetroidRouteDefinitions.FirstRecordPointer;
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
                ambientCrySoundQueued = (randomNumber & 0x0fff) >= 0x0fa0;

                // `$CA24-$CA64` is eight overlapping records. The fixed targets, divisor
                // indexes, and callback identities are compiled mechanics definitions;
                // sprite/instruction presentation remains separately cartridge-backed.
                BabyMetroidRouteRecord record =
                    BabyMetroidRouteDefinitions.GetRecord(MovementTablePointer);
                Position16<RoomPixels> target = record.Target;
                ushort divisorIndex = record.AccelerationDivisorIndex;

                // `$F56A` is `[10,0F,...,01]`; every retail `$CA24` record uses index zero,
                // but implementing all legal indices costs nothing and catches malformed
                // data before a divide-by-zero or an invented fallback can hide it.
                if (divisorIndex > 0x000f)
                {
                    throw new InvalidDataException(
                        $"Baby route ${MovementTablePointer:X4} has invalid divisor index ${divisorIndex:X4}.");
                }
                ushort accelerationDivisor = unchecked((ushort)(0x0010 - divisorIndex));
                ushort wrongWayExtra = BabyMetroidRouteDefinitions.GetWrongWayOffScreenXSpeed(
                    record.MovementFunction);

                GraduallyAccelerateTowardsPoint(
                    target,
                    accelerationDivisor,
                    wrongWayExtra,
                    layer1);

                // The route's 4x4 rectangle test occurs before the common velocity mover.
                // On ordinary records, +8 is merely the next record's target X and the
                // pointer advances eight bytes. The final record deliberately overlaps its
                // +8 word with `$CA66`, so BMI installs the next AI function instead.
                if (CollidesWithRectangle(target, TargetRadius))
                {
                    ushort nextWord = record.FollowingWord;
                    if ((nextWord & 0x8000) != 0)
                    {
                        if (nextWord != BabyMetroidRouteDefinitions.LatchOntoSamusFunction)
                        {
                            throw new InvalidDataException(
                                $"Baby route terminates at unexpected function ${nextWord:X4}.");
                        }
                        Phase = BabyMetroidCutscenePhase.LatchOntoSamus;
                    }
                    else
                    {
                        MovementTablePointer = unchecked((ushort)(MovementTablePointer + 8));
                    }
                }
                break;
            }

            case BabyMetroidCutscenePhase.LatchOntoSamus:
                // `$CA66` itself only steers toward Samus's centre minus twenty pixels. It
                // never performs the state change. That transition belongs to enemy-touch
                // handler `$CF03`, executed after main AI and the common mover below.
                GraduallyAccelerateTowardsPoint(
                    samus.Position.Whole + SamusLatchOffset,
                    accelerationDivisor: 0x10,
                    wrongWayOffScreenXSpeed: 0x0400,
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
                ushort candidateHealth = unchecked((ushort)(samus.Health + 1));
                if (unchecked((short)(candidateHealth - samus.MaxHealth)) < 0)
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
                if (OnionRingHitFlashTimer != 0)
                {
                    int shakingIndex = (OnionRingHitFlashTimer & 6) >> 1;
                    Offset16<RoomPixels> shake = ShakingOffsets[shakingIndex];
                    Position = Position.WithWhole(samus.Position.Whole + shake + shake + SamusLatchOffset);
                }

                if (Health == 0)
                {
                    // The zero reached by the ordinary volleys is a state trigger, not the
                    // health carried into the final charge. Native code deliberately gives
                    // the Baby `$0140` health, changes to the low-health palette function,
                    // disables health-band palettes, and lets `$CB13` run next frame.
                    Health = 0x0140;
                    Phase = BabyMetroidCutscenePhase.ReleaseSamus;
                    LowHealthPaletteTimer = 0x000a;
                    // `$A9:CB05` stores ten in the same enemy variable used by the normal
                    // palette handler.  This is not an immediate/zero-delay switch: the
                    // low-health palette routine counts through its own ten-frame cadence.
                    PaletteHandlerDelay = 0x000a;
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
                var target = new Position16<RoomPixels>(unchecked((ushort)(samus.Position.X.Whole - 4)), 0x0060);
                GraduallyAccelerateTowardsPoint(
                    target,
                    accelerationDivisor: 0x0010,
                    wrongWayOffScreenXSpeed: 0x0010,
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
                    new Position16<RoomPixels>(0x0110, 0x0040),
                    accelerationDivisor: 0x0010,
                    wrongWayOffScreenXSpeed: 0x0010,
                    layer1);
                if (CollidesWithRectangle(new Position16<RoomPixels>(0x0110, 0x0040), TargetRadius))
                    Phase = BabyMetroidCutscenePhase.MoveToFinalChargeStart;
                break;

            case BabyMetroidCutscenePhase.MoveToFinalChargeStart:
                // `$CB7B` stages the Baby at `(305,160)`. Reaching that rectangle assigns
                // exactly 79 HP so the final four-ring volley kills on its first collision,
                // clears the brain-slot speed flag, and replaces Mother Brain's retreat AI.
                GraduallyAccelerateTowardsPoint(
                    new Position16<RoomPixels>(0x0131, 0x00a0),
                    accelerationDivisor: 0x0010,
                    wrongWayOffScreenXSpeed: 0x0010,
                    layer1);
                if (CollidesWithRectangle(new Position16<RoomPixels>(0x0131, 0x00a0), TargetRadius))
                {
                    Health = 0x004f;
                    motherBrain.ExecuteFinalBabyMetroidAttack();
                    Phase = BabyMetroidCutscenePhase.InitiateFinalCharge;
                }
                break;

            case BabyMetroidCutscenePhase.InitiateFinalCharge:
                // Divisor-table index `$A` maps to `$10-$A == 6`. This helper family adds
                // `$400` only when an off-screen actor is moving the wrong horizontal way.
                GraduallyAccelerateTowardsPoint(
                    new Position16<RoomPixels>(0x0122, 0x0080),
                    accelerationDivisor: 0x0006,
                    wrongWayOffScreenXSpeed: 0x0400,
                    layer1);
                if (CollidesWithRectangle(new Position16<RoomPixels>(0x0122, 0x0080), TargetRadius))
                    Phase = BabyMetroidCutscenePhase.FinalCharge;
                break;

            case BabyMetroidCutscenePhase.FinalCharge:
                // The final target follows the brain enemy slot, 32 pixels above its
                // centre. Index `$C` maps to divisor four and therefore accelerates much
                // more aggressively than the staging leg.
                GraduallyAccelerateTowardsPoint(
                    head + new Offset16<RoomPixels>(0, -0x0020),
                    accelerationDivisor: 0x0004,
                    wrongWayOffScreenXSpeed: 0x0400,
                    layer1);
                if (Health == 0)
                {
                    // `$A9:CBF2-$CC3D` changes graphics/list state, freezes the body AI,
                    // clears velocity, remembers the shake origin, and falls through into
                    // the first of 17 fatal-blow shake calls.
                    GraphicsOffset = 0x10a0;
                    SetInstructionList(
                        MotherBrainBabyInstructionProgramDefinitions.TakingFatalBlow);
                    Velocity = default;
                    Phase = BabyMetroidCutscenePhase.TakeFinalBlow;
                    FunctionTimer = 0x0010;
                    FatalBlowOrigin = Position.Whole;
                    goto case BabyMetroidCutscenePhase.TakeFinalBlow;
                }
                break;

            case BabyMetroidCutscenePhase.TakeFinalBlow:
            {
                // `ShakeBabyMetroidCutscene` ($A9:CEDB) first adds `$FFFF` to the Y velocity,
                // which the common mover then applies, and places the Baby at the saved origin
                // plus the undoubled shaking offsets.
                Velocity = Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(Velocity.Y.RawValue + 0xffff))));
                int shakingIndex = (enemyFrameCounter & 6) >> 1;
                Position = Position.WithWhole(FatalBlowOrigin + ShakingOffsets[shakingIndex]);
                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) != 0)
                {
                    Position = Position.WithWhole(FatalBlowOrigin);
                    Phase = BabyMetroidCutscenePhase.PlaySamusTheme;
                    FunctionTimer = 0x0038;
                    goto case BabyMetroidCutscenePhase.PlaySamusTheme;
                }
                break;
            }

            case BabyMetroidCutscenePhase.PlaySamusTheme:
                // The expiry call queues the two delayed music words and immediately
                // performs the first `$000C` prepare-Hyper-Beam decrement.
                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) != 0)
                {
                    Phase = BabyMetroidCutscenePhase.PrepareSamusForHyperBeam;
                    FunctionTimer = 0x000c;
                    goto case BabyMetroidCutscenePhase.PrepareSamusForHyperBeam;
                }
                break;

            case BabyMetroidCutscenePhase.PrepareSamusForHyperBeam:
                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) != 0)
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
                if (StepFadeToBlack(ref babyPaletteTransfer))
                {
                    // Completion does not write palette index seven. It hides the actor,
                    // installs `$CCC0`, and waits through `$80..0` before the first DMA.
                    Properties = Properties.With(EnemyProperties.Invisible);
                    Phase = BabyMetroidCutscenePhase.UnloadTiles;
                    FunctionTimer = 0x0080;
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
                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) != 0)
                {
                    int transferIndex = AttackTileTransferIndex;
                    MotherBrainSpecialSpriteSheetDefinition attack =
                        MotherBrainSpecialSpriteArtworkDefinitions.Attack;
                    if ((uint)transferIndex >= (uint)attack.PageCount)
                        throw new InvalidOperationException("Mother Brain attack-tile transfer list is already complete.");

                    attackTileTransfer = new MotherBrainSpriteTileTransferRequest(
                        EntryIndex: AttackTileTransferIndex,
                        Size: MotherBrainSpecialSpriteSheetDefinition.PageByteCount,
                        SourceAddress: unchecked((uint)(attack.SourceAddress +
                            transferIndex * MotherBrainSpecialSpriteSheetDefinition.PageByteCount)),
                        VramDestination: unchecked((ushort)(attack.FirstDestinationWord +
                            transferIndex * MotherBrainSpecialSpriteSheetDefinition.DestinationWordStride)));
                    AttackTileTransferIndex++;

                    if (AttackTileTransferIndex == attack.PageCount)
                    {
                        // ProcessSpriteTilesTransfers observes the zero terminator after
                        // publishing entry four. Native falls straight into `$CCDE`, so the
                        // newly written `$B0` becomes `$AF` on this same call.
                        Phase = BabyMetroidCutscenePhase.LetSamusRainbowSomeMore;
                        FunctionTimer = 0x00b0;
                        goto case BabyMetroidCutscenePhase.LetSamusRainbowSomeMore;
                    }
                }
                break;

            case BabyMetroidCutscenePhase.LetSamusRainbowSomeMore:
                FunctionTimer = unchecked((ushort)(FunctionTimer - 1));
                if ((FunctionTimer & 0x8000) != 0)
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
                ushort paletteIndex = RoomLightsTransitionCounter;
                RoomLightsTransitionCounter = unchecked((ushort)(RoomLightsTransitionCounter + 1));
                if (paletteIndex < 7)
                {
                    backgroundPaletteTransfer = new MotherBrainBackgroundPaletteTransferRequest(
                        PaletteIndex: paletteIndex,
                        SourceAddress: unchecked((uint)(0xadf3d3 - paletteIndex * 0x38)),
                        FirstDestinationColorIndex: 0x0062,
                        SecondDestinationColorIndex: 0x00a2,
                        ColorsPerDestination: 0x000e);
                    break;
                }

                motherBrain.BeginPhase3RecoveryFromBabyCutscene();
                samus.Drained.DisableRainbowAndStartStandingAnimation(samus);
                samus.Drained.EnableHyperBeam(samus);
                Properties = Properties.With(EnemyProperties.Deleted);
                break;
            }

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
        if (OnionRingHitFlashTimer != 0)
        {
            OnionRingHitFlashTimer = unchecked((ushort)(OnionRingHitFlashTimer - 1));
            Palette = (OnionRingHitFlashTimer & 2) != 0 ? (ushort)0 : EnemyPaletteBits.Palette7;
        }
        else
        {
            Palette = EnemyPaletteBits.Palette7;
        }

        return new BabyMetroidCutsceneStepResult(
            phaseBefore,
            Phase,
            bodyStumbleRequested,
            latchSoundQueued,
            releaseDustClouds.ToArray(),
            ambientCrySoundQueued,
            deathExplosion,
            babyPaletteTransfer,
            attackTileTransfer,
            backgroundPaletteTransfer);
    }

    // $86:C381-$C3A8's Baby half of an onion-ring collision is implemented in the
    // other partial declaration: the projectile system owns explosion/deletion and
    // the Mother Brain cry flag; the enemy slot owns the flash timer and saturating
    // health subtraction.

    /// <summary>Applies the Baby's health/flash mutation from the blue-ring collision handler at $86:C381–C3A8: a living Baby gets a $0010 flash timer and health subtraction saturated at zero.</summary>
    /// <param name="damage">Unsigned health units to subtract, defaulting to the native $0050 (80); even zero damage refreshes the flash timer when health is nonzero.</param>
    /// <returns>An empty compatibility result; the effects are written directly to <see cref="Health"/> and <see cref="OnionRingHitFlashTimer"/>.</returns>
    /// <remarks>Zero health leaves all state unchanged. The caller must establish overlap and owns the Baby cry request, physical enemy-slot synchronization, and ring explosion/deletion.</remarks>
    public BabyMetroidOnionRingHitResult ApplyMotherBrainOnionRingHit(ushort damage = 0x0050)
    {
        ushort healthBefore = Health;
        if (healthBefore == 0)
            return new BabyMetroidOnionRingHitResult();

        OnionRingHitFlashTimer = 0x0010;
        Health = healthBefore < damage
            ? (ushort)0
            : unchecked((ushort)(healthBefore - damage));
        return new BabyMetroidOnionRingHitResult();
    }

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
                if (unchecked((short)(Position.Y.Whole + 0x0010 - samus.Position.Y.Whole)) < 0)
                    return;
                samus.Drained.EnableRainbow(samus);
                samusRainbowActivated = true;
                SamusRainbowPhase = BabyMetroidSamusRainbowPhase.GraduallySlowAnimationDown;
                return;

            case BabyMetroidSamusRainbowPhase.GraduallySlowAnimationDown:
            {
                uint sum = (uint)SamusRainbowPaletteAnimationCounter + 0x0300;
                SamusRainbowPaletteAnimationCounter = unchecked((ushort)sum);
                if (sum > ushort.MaxValue)
                    samus.Drained.IncrementRainbowPaletteFrame(maximumFrame: 10);
                return;
            }

            default:
                throw new InvalidOperationException($"Unsupported Samus rainbow phase {SamusRainbowPhase}.");
        }
    }

    private void AccelerateDownwardsForDeath()
    {
        // `$CE40` subtracts `$20` from the magnitude, clamps at zero, reapplies the old
        // sign, and independently adds two to vertical 8.8 velocity with 16-bit wrap.
        ushort xVelocity = Velocity.X.RawValue;
        bool negative = (xVelocity & 0x8000) != 0;
        ushort magnitude = negative ? unchecked((ushort)-xVelocity) : xVelocity;
        magnitude = magnitude >= 0x0020 ? unchecked((ushort)(magnitude - 0x0020)) : (ushort)0;
        Velocity = new(
            new SnesSignedEightEight(negative ? unchecked((ushort)-magnitude) : magnitude),
            new SnesSignedEightEight(unchecked((ushort)(Velocity.Y.RawValue + 2))));
    }

    private bool StepFadeToBlack(ref BabyMetroidPaletteTransferRequest? paletteTransfer)
    {
        // No timer changes occur until the actor's centre reaches Y `$80`.
        if (unchecked((short)(Position.Y.Whole - 0x0080)) < 0)
            return false;

        FadeToBlackPaletteTimer = unchecked((ushort)(FadeToBlackPaletteTimer - 1));
        if ((FadeToBlackPaletteTimer & 0x8000) == 0)
            return false;

        FadeToBlackPaletteTimer = 8;
        ushort nextIndex = unchecked((ushort)(FadeToBlackPaletteIndex + 1));
        if (nextIndex >= 7)
            return true;

        FadeToBlackPaletteIndex = nextIndex;
        paletteTransfer = new BabyMetroidPaletteTransferRequest(
            PaletteIndex: nextIndex,
            SourceAddress: unchecked((uint)BabyMetroidCutsceneColorRomData.FadeSource(nextIndex)),
            DestinationColorIndex: BabyMetroidCutsceneColorRomData.DestinationByteIndex,
            ColorCount: BabyMetroidCutsceneColorRomData.FadeColorCount);
        return false;
    }

    private BabyMetroidDeathExplosionRequest? StepDeathExplosion()
    {
        DeathExplosionTimer = unchecked((ushort)(DeathExplosionTimer - 1));
        if ((DeathExplosionTimer & 0x8000) == 0)
            return null;

        DeathExplosionTimer = 4;
        DeathExplosionPatternIndex++;
        if (DeathExplosionPatternIndex >= DeathExplosionScatterDefinitions.Count)
            DeathExplosionPatternIndex = 0;
        // `$A9:CDFC` holds the scatter shared with Ridley's death. The handler increments its
        // index before looking up a pair, so a freshly cleared counter begins at entry one.
        return new BabyMetroidDeathExplosionRequest(
            Position: Position.Whole + DeathExplosionScatterDefinitions.Offset(DeathExplosionPatternIndex),
            ProjectileParameter: 3,
            SoundEffect: 0x0013);
    }

    private void SetInstructionList(ushort pointer)
    {
        InstructionList = pointer;
        InstructionTimer = 1;
        InstructionLoopCounter = 0;
    }

    private void UpdateSpeedAndAngle(
        ushort angleDelta,
        ushort targetAngle,
        ushort targetSpeed)
    {
        // `$CF31` changes speed by exactly `$20` and clamps rather than crossing the target.
        if (Speed != targetSpeed)
        {
            if (targetSpeed < Speed)
                Speed = Speed - 0x20 < targetSpeed ? targetSpeed : unchecked((ushort)(Speed - 0x20));
            else
                Speed = Speed + 0x20 >= targetSpeed ? targetSpeed : unchecked((ushort)(Speed + 0x20));
        }

        ushort candidateAngle = unchecked((ushort)(Angle + angleDelta));
        if (unchecked((short)angleDelta) < 0)
        {
            Angle = unchecked((short)(candidateAngle - targetAngle)) >= 0
                ? candidateAngle
                : targetAngle;
        }
        else
        {
            Angle = unchecked((short)(candidateAngle - targetAngle)) < 0
                ? candidateAngle
                : targetAngle;
        }

        byte angleByte = SnesLittleEndian.HighByte(Angle);
        Velocity = new(
            new SnesSignedEightEight(CalculateVelocityComponent(Speed, angleByte)),
            new SnesSignedEightEight(CalculateVelocityComponent(Speed, unchecked((byte)(angleByte + 0x40)))));
    }

    internal static ushort CalculateVelocityComponent(
        ushort speed,
        byte sineIndex)
    {
        // Bank `$86:C27A` multiplies unsigned speed by the absolute signed-table value,
        // returns product bits 8..23, then reapplies the original sign.
        return EnemyTrigonometryTables.MultiplySignedSine(speed, sineIndex);
    }

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
            samus.InvincibilityTimer = 0;
        else if (samus.InvincibilityTimer != 0)
            return false;
        return AxisTouches(samus.Position.X.Whole, Position.X.Whole, samus.Kinematics.Radius.X, XHitboxRadius) &&
            AxisTouches(samus.Position.Y.Whole, Position.Y.Whole, samus.Kinematics.Radius.Y, YHitboxRadius);

        static bool AxisTouches(ushort samusPosition, ushort enemyPosition, ushort samusRadius, ushort enemyRadius)
        {
            int distance = Math.Abs(unchecked((short)(samusPosition - enemyPosition)));
            return distance < samusRadius || distance - samusRadius < enemyRadius;
        }
    }

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

    private void GraduallyAccelerateTowardsPoint(
        Position16<RoomPixels> target,
        ushort accelerationDivisor,
        ushort wrongWayOffScreenXSpeed,
        Position16<RoomPixels> layer1)
    {
        byte divisor = checked((byte)accelerationDivisor);
        Velocity = Velocity.WithX(new SnesSignedEightEight(BabyMetroidGradualAcceleration.AccelerateHorizontally(
            Position.X.Whole, target.X, Velocity.X.RawValue, divisor, wrongWayOffScreenXSpeed,
            () => IsVaguelyOffScreen(layer1))));
        Velocity = Velocity.WithY(new SnesSignedEightEight(BabyMetroidGradualAcceleration.AccelerateVertically(
            Position.Y.Whole, target.Y, Velocity.Y.RawValue, divisor)));
    }

    private bool IsVaguelyOffScreen(Position16<RoomPixels> layer1)
    {
        // This is the signed-branch sequence at `$A9:F57A`; its generous rectangle extends
        // 16 pixels left/right and 96 pixels vertically beyond the ordinary 256x224 view.
        if (unchecked((short)Position.Y.Whole) < 0)
            return true;
        short relativeY = unchecked((short)(Position.Y.Whole + 0x0060 - layer1.Y));
        if (relativeY is < 0 or >= 0x01a0)
            return true;
        if (unchecked((short)Position.X.Whole) < 0)
            return true;
        short relativeX = unchecked((short)(Position.X.Whole + 0x0010 - layer1.X));
        return relativeX is < 0 or >= 0x0120;
    }

    private bool AccelerateTowardsPoint(Position16<RoomPixels> target, ushort acceleration)
    {
        // `$F5A6` counts axes whose next whole-pixel prediction reaches/crosses target,
        // then shifts the count twice. Carry is set only for count two.
        bool reachedX = AccelerateTowardsXPosition(target.X, acceleration);
        bool reachedY = AccelerateTowardsYPosition(target.Y, acceleration);
        return reachedX && reachedY;
    }

    private bool AccelerateTowardsYPosition(ushort targetY, ushort acceleration)
    {
        short difference = unchecked((short)(Position.Y.Whole - targetY));
        if (difference == 0)
            return true;

        if (difference < 0)
        {
            int velocity = Math.Min(Velocity.Y.SignedRawValue + acceleration, 0x0500);
            Velocity = Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)velocity)));
            ushort prediction = unchecked((ushort)(Position.Y.Whole + unchecked((sbyte)(SnesEightEight.WholeOf(Velocity.Y.RawValue)))));
            if (unchecked((short)(prediction - targetY)) < 0)
                return false;
        }
        else
        {
            int velocity = Math.Max(Velocity.Y.SignedRawValue - acceleration, -0x0500);
            Velocity = Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)velocity)));
            ushort prediction = unchecked((ushort)(Position.Y.Whole + unchecked((sbyte)(SnesEightEight.WholeOf(Velocity.Y.RawValue)))));
            short predictedDifference = unchecked((short)(prediction - targetY));
            if (predictedDifference > 0)
                return false;
        }

        Velocity = Velocity.WithY(default);
        return true;
    }

    private bool AccelerateTowardsXPosition(ushort targetX, ushort acceleration)
    {
        short difference = unchecked((short)(Position.X.Whole - targetX));
        if (difference < 0)
        {
            int velocity = Math.Min(Velocity.X.SignedRawValue + acceleration, 0x0500);
            Velocity = Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)velocity)));
            ushort prediction = unchecked((ushort)(Position.X.Whole + unchecked((sbyte)(SnesEightEight.WholeOf(Velocity.X.RawValue)))));
            if (unchecked((short)(prediction - targetX)) < 0)
                return false;
        }
        else
        {
            // Native deliberately sends exact equality through the left branch; unlike Y,
            // there is no BEQ before BPL at `$A9:F619-$F61B`.
            int velocity = Math.Max(Velocity.X.SignedRawValue - acceleration, -0x0500);
            Velocity = Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)velocity)));
            ushort prediction = unchecked((ushort)(Position.X.Whole + unchecked((sbyte)(SnesEightEight.WholeOf(Velocity.X.RawValue)))));
            short predictedDifference = unchecked((short)(prediction - targetX));
            if (predictedDifference > 0)
                return false;
        }

        Velocity = Velocity.WithX(default);
        return true;
    }

    private static BabyMetroidCutscenePoint Capture() => new();

    /// <summary>The 4x4 half-extent rectangle every route and staging target is tested with.</summary>
    private static Extent16<RoomPixels> TargetRadius => new(4, 4);
}

