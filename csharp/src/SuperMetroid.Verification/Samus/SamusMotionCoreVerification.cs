using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Samus.Drawing;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.SyntheticSamusCartridgeVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Colours;
using SuperMetroid.Core.Samus.Poses;
namespace SuperMetroid.Verification.Samus;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Progression;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.AssetExtraction.Samus.Artwork;

/// <summary>Checks Samus pose-input matching, horizontal speed, and external displacement arithmetic.</summary>
internal static class SamusMotionCoreVerification
{

/// <summary>Checks ordered pose-input matches, extra held bits, self-pose suppression, and fallback paths.</summary>
internal static void VerifySamusPoseTransitionMatching()
{
    SamusPoseId fixturePose = SamusPoseId.FacingRightNormalPose;

    // The authored standing-right table at $91:A0EC demonstrates priority and subset
    // matching: Jump+Up outranks plain Up, which outranks Right because the matcher stops
    // at the first record whose complete required masks are present.

    SamusPoseTransition jumpUp = (SamusPoseInputTransitions.Find(
        currentPose: fixturePose,
        heldActions: GameplayAction.Up | GameplayAction.Right | GameplayAction.Jump,
        newlyPressedActions: GameplayAction.Jump) ?? throw new InvalidOperationException("Verification requires SamusPoseInputTransitions.Find( bus, currentPose: fixturePose, heldActions: 0x098...."));
    AssertEqual(0x55, SamusPoseNumbers.Number(jumpUp.ProspectivePose), "Samus transition required-new plus held chord");

    SamusPoseTransition up = (SamusPoseInputTransitions.Find(
        currentPose: fixturePose,
        heldActions: (GameplayAction.Up | GameplayAction.ItemCancel),
        newlyPressedActions: 0) ?? throw new InvalidOperationException("Verification requires SamusPoseInputTransitions.Find( bus, currentPose: fixturePose, heldActions: (usho...."));
    AssertEqual(0x03, SamusPoseNumbers.Number(up.ProspectivePose), "Samus transition permits extra held action");

    SamusPoseTransition right = (SamusPoseInputTransitions.Find(
        currentPose: fixturePose,
        heldActions: GameplayAction.Right,
        newlyPressedActions: 0) ?? throw new InvalidOperationException("Verification requires SamusPoseInputTransitions.Find( bus, currentPose: fixturePose, heldActions: 0x010...."));
    AssertEqual(0x09, SamusPoseNumbers.Number(right.ProspectivePose), "Samus standing Right proposes running pose");

    AssertEqual<SamusPoseTransition?>(null,
        SamusPoseInputTransitions.Find(fixturePose, heldActions: 0, newlyPressedActions: 0),
        "Samus zero input bypasses transition table");
    AssertEqual<SamusPoseTransition?>(null,
        SamusPoseInputTransitions.Find(
            SamusPoseId.MovingRightGunExtendedPose,
            heldActions: (GameplayAction.Right | GameplayAction.Shoot),
            newlyPressedActions: 0),
        "Samus same-pose transition is suppressed");
    AssertEqual<SamusPoseTransition?>(null,
        SamusPoseInputTransitions.Find(fixturePose, heldActions: GameplayAction.Down, newlyPressedActions: 0),
        "Samus transition terminator returns no match");

    SamusPoseTransitionLookup zeroInput = SamusPoseInputTransitions.Lookup(
        fixturePose, heldActions: 0, newlyPressedActions: 0);
    SamusPoseTransitionLookup samePose = SamusPoseInputTransitions.Lookup(
        SamusPoseId.MovingRightGunExtendedPose,
        heldActions: (GameplayAction.Right | GameplayAction.Shoot),
        newlyPressedActions: 0);
    SamusPoseTransitionLookup terminator = SamusPoseInputTransitions.Lookup(
        fixturePose, heldActions: GameplayAction.Down, newlyPressedActions: 0);
    AssertTrue(zeroInput.UsesPoseDefinitionFallback,
        "Samus zero input enters pose-definition fallback");
    AssertTrue(!samePose.UsesPoseDefinitionFallback,
        "Samus same-pose record returns without fallback");
    AssertTrue(terminator.UsesPoseDefinitionFallback,
        "Samus unmatched nonzero input enters pose-definition fallback");

    // Pose $2F uses the authored empty list at $91:A0DC. Its immediate terminator is a
    // direct return for held input, unlike exhaustion after at least one unmatched record.
    const SamusPoseId emptyListPose = SamusPoseId.TurningRightToLeftJumpPose;
    AssertTrue(!SamusPoseInputTransitions.Lookup(emptyListPose, GameplayAction.Jump, 0).UsesPoseDefinitionFallback,
        "empty pose table preserves turn momentum while Jump remains held");
    AssertTrue(SamusPoseInputTransitions.Lookup(emptyListPose, 0, 0).UsesPoseDefinitionFallback,
        "releasing all input still invokes fallback before reading an empty table");

    Console.WriteLine(
        "  Samus input: ROM transition masks, priority, same-pose return, and fallback agree.");
}

/// <summary>
/// Exercises the exact normal-air pointer chain and word-oriented arithmetic from
/// <c>$94:97D0</c>, <c>$90:9BD1</c>, <c>$90:9A7E</c>, and <c>$90:E4E6-$90:E4E5</c>.
/// Synthetic bus bytes are the retail running entry at $90:9F61, so these assertions stay
/// deterministic while the DebugRunner independently reads the user's cartridge.
/// </summary>
internal static void VerifySamusHorizontalSpeed()
{
    var bus = new TestAddressSpace();
    SamusFullBodyCycleColours CycleColors() =>
        SamusFullBodyCycleColorFile.Read(new MemoryStream(
            SuperMetroid.AssetExtraction.Samus.SamusFullBodyCycleColorExtractor.Extract(bus))).ToColours();
    InstalledSamusSuitColors SuitColors() =>
        SamusSuitColorFile.Read(new MemoryStream(
            SuperMetroid.AssetExtraction.Samus.SamusSuitColorExtractor.Extract(bus)));

    // Normal air stores $9F55. Running movement type one advances by one more 12-byte
    // record to $9F61: acceleration 0.3000, maximum 2.C000, deceleration 0.8000.
    SyntheticSamusCartridge.WriteNormalAirSpeedEntry(bus, SamusMovementType.Running, [
        0x00, 0x00, 0x00, 0x30,
        0x02, 0x00, 0x00, 0xc0,
        0x00, 0x00, 0x00, 0x80,
    ]);

    var speed = new SamusHorizontalSpeedState(SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus());
    speed.SelectAirSpeeds();
    AssertEqual(0x909f61, speed.ResolveEntryAddress(movementType: SamusMovementType.Running), "running speed entry pointer chain");

    SamusHorizontalSpeeds entry = speed.Speeds(movementType: SamusMovementType.Running);
    AssertEqual(new UnsignedSixteenSixteen(0, 0x3000), entry.Acceleration,
        "running accelerates by three sixteenths of a pixel per frame");
    AssertEqual(new UnsignedSixteenSixteen(2, 0xc000), entry.MaximumSpeed,
        "running caps at two and three quarter pixels per frame");
    AssertEqual(new UnsignedSixteenSixteen(0, 0x8000), entry.Deceleration,
        "running decelerates by half a pixel per frame");

    // Four acceleration calls produce 0.C000 exactly. Fifteen calls would reach 2.D000,
    // so the native quirked comparison clamps that result down to the table's 2.C000 cap.
    StepFrames(4, _ => speed.CalculateBaseSpeed(movementType: SamusMovementType.Running));
    AssertEqual(0x0000c000u, speed.BaseSpeed.RawValue, "running acceleration after four calls");
    StepFrames(11, _ => speed.CalculateBaseSpeed(movementType: SamusMovementType.Running));
    AssertEqual(0x0002c000u, speed.BaseSpeed.RawValue, "running acceleration clamps at 2.C000");

    // Deceleration subtracts 0.8000 per call. Crossing below zero makes the signed high
    // word negative, clearing both halves and restoring acceleration mode zero.
    speed.AccelerationMode = SamusAccelerationMode.Decelerating;
    StepFrames(6, _ => speed.CalculateBaseSpeed(movementType: SamusMovementType.Running));
    AssertEqual(0u, speed.BaseSpeed.RawValue, "running deceleration underflow clears speed");
    AssertEqual(SamusAccelerationMode.Accelerating, speed.AccelerationMode, "running deceleration restores acceleration mode");

    // `$90:973E` adds the literal no-booster 0.1000 pair once per running+B frame. The
    // native cap test runs before addition, so call 32 reaches exactly 2.0000 and call 33
    // performs the visible clamp write without changing the pair.
    StepFrames(32, _ =>
        speed.HandleExtraRunSpeed(
            movementType: SamusMovementType.Running,
            controllerInput: GameplayAction.Dash,
            speedBoosterEquipped: false));
    AssertTrue(speed.HasRunningMomentum, "ordinary Dash establishes native momentum flag");
    AssertEqual(new SamusSpeedBoostCounter(0, 0), speed.SpeedBoostCounter, "ordinary Dash leaves booster stage zero");
    AssertEqual(2, speed.ExtraRunSpeed.Whole, "ordinary Dash whole-speed cap");
    AssertEqual(0, speed.ExtraRunSpeed.Fraction, "ordinary Dash fractional-speed cap");
    speed.HandleExtraRunSpeed(SamusMovementType.Running, GameplayAction.Dash, speedBoosterEquipped: false);
    AssertEqual(2, speed.ExtraRunSpeed.Whole, "ordinary Dash remains clamped on next call");

    // B release and an airborne movement type both take `$90:9808`; a set momentum flag
    // bypasses the numeric clear. Only the separately invoked cancel routine clears the
    // flag/counter, after which another non-running call clears the retained pair.
    speed.HandleExtraRunSpeed(SamusMovementType.Running, controllerInput: 0, speedBoosterEquipped: false);
    speed.HandleExtraRunSpeed(SamusMovementType.SpinJumping, controllerInput: 0, speedBoosterEquipped: false);
    AssertEqual(2, speed.ExtraRunSpeed.Whole, "Dash release and spin jump retain extra speed");
    speed.CancelRunningMomentum(SamusFacingDirection.Right);
    AssertTrue(!speed.HasRunningMomentum, "CancelSpeedBoost clears ordinary momentum flag");
    speed.HandleExtraRunSpeed(SamusMovementType.SpinJumping, controllerInput: 0, speedBoosterEquipped: false);
    AssertEqual(0, speed.ExtraRunSpeed.Whole, "post-cancel airborne handler clears extra speed");

    // Ordinary Dash uses the compiled native ten-frame, two-tick cadence. Do not
    // rewrite the cartridge pointer/delay ROM in the fake bus: gameplay now reads
    // those immutable definitions from SamusRunningCadenceDefinitions.
    var dashAnimation = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
    {
        Pose = SamusPoseId.MovingRightNormalPose,
        // She runs in a room, whose footsteps her stride's foot contacts raise: a dry one, the Landing Site without its rain.
        Ground = SamusGround.Of(RoomDefinitions.LandingSite),
    };
    SamusPresentation.InitializeAnimation(dashAnimation);
    dashAnimation.HorizontalSpeed.HandleExtraRunSpeed(
        movementType: SamusMovementType.Running,
        controllerInput: GameplayAction.Dash,
        speedBoosterEquipped: false);
    for (int tick = 0; tick < 2; tick++)
        SamusPresentation.Animate(dashAnimation, new SamusAnimationStep { Random = new SharedRandom(), ControllerInput = GameplayAction.Dash });
    AssertEqual(1, dashAnimation.AnimationNumber, "Dash advances into running frame one");
    AssertEqual(2, dashAnimation.AnimationFrameTimer.RawValue, "Dash selects native frame-one delay");
    for (int tick = 0; tick < 18; tick++)
        SamusPresentation.Animate(dashAnimation, new SamusAnimationStep { Random = new SharedRandom(), ControllerInput = GameplayAction.Dash });
    AssertEqual(0, dashAnimation.AnimationNumber, "Dash command interception restarts frame zero");
    AssertEqual(2, dashAnimation.AnimationFrameTimer.RawValue, "Dash restart uses shared frame-zero delay");

    // Native stages zero through three last one command loop; stage four lasts two.
    // Their compiled cadence still publishes the native echo/contact-damage events.

    var booster = new SamusHorizontalSpeedState(SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus());
    booster.HandleExtraRunSpeed(
        movementType: SamusMovementType.Running,
        controllerInput: GameplayAction.Dash,
        speedBoosterEquipped: true);
    AssertTrue(booster.HasRunningMomentum, "Speed Booster establishes momentum");
    AssertEqual(new SamusSpeedBoostCounter(0, 1), booster.SpeedBoostCounter, "Speed Booster seeds native stage-zero countdown");
    AssertEqual(1, booster.CycleShadeTimer, "Speed Booster seeds special-palette timer");

    // Hexadecimal `.1000` is one sixteenth, so 112 movement calls reach 7.0000 exactly.
    for (int frame = 1; frame < 112; frame++)
    {
        booster.HandleExtraRunSpeed(
            movementType: SamusMovementType.Running,
            controllerInput: GameplayAction.Dash,
            speedBoosterEquipped: true);
    }
    AssertEqual(7, booster.ExtraRunSpeed.Whole, "Speed Booster reaches exact 7.0000 cap");
    AssertEqual(0, booster.ExtraRunSpeed.Fraction, "Speed Booster cap has zero fraction");

    int echoRequests = 0;
    for (int command = 0; command < 4; command++)
    {
        bool intercepted = booster.TryAdvanceSpeedBoosterAnimationStage(
            new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(),
            movementType: SamusMovementType.Running,
            controllerInput: GameplayAction.Dash,
            extraAnimationDelay: 0,
            out int boostTimer,
            requestEchoSound: () =>
            {
                echoRequests++;
                // The echo is admitted behind as many requests as the boost's full stage, which restarts on that stage's cadence.
                return SuperMetroid.Core.Compatibility.SoundQueueAdmission.Admitted(SamusSpeedBoostCounter.FullStage);
            });
        if (command == 0)
        {
            AssertTrue(intercepted, "first stage-zero command advances Speed Booster and restarts the run");
            AssertEqual(2, boostTimer, "stage-one delay matches native cadence");
        }
    }
    AssertEqual(new SamusSpeedBoostCounter(4, 2), booster.SpeedBoostCounter, "Speed Booster reaches stage four countdown");
    AssertEqual(1, echoRequests, "reaching stage four requests the speed-echo sound once, as the stage advances");
    AssertEqual(SamusBodyAttack.None, booster.BodyAttack, "stage four animation waits for movement to publish contact damage");

    // $90:9813 is a common epilogue, including the released/airborne/liquid
    // branches. Beta clears contact damage each frame; retained blue speed
    // must republish protection even though no extra acceleration occurs.
    foreach (var movement in new[] { SamusMovementType.Running, SamusMovementType.SpinJumping })
    foreach (bool liquid in new[] { false, true })
    foreach (bool activeStage in new[] { false, true })
    {
        var retainedBoost = new SamusHorizontalSpeedState(SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus())
        {
            HasRunningMomentum = true,
            ExtraRunSpeed = new UnsignedSixteenSixteen(7, 0),
            SpeedBoostCounter = activeStage ? new SamusSpeedBoostCounter(4, 1) : new SamusSpeedBoostCounter(3, 1),
            BodyAttack = SamusBodyAttack.None,
        };
        retainedBoost.HandleExtraRunSpeed(movement, 0, true, liquidImpeded: liquid);
        AssertEqual(activeStage ? SamusBodyAttack.SpeedBooster : SamusBodyAttack.None, retainedBoost.BodyAttack,
            "retained boost publishes contact protection only in active stage");
        AssertEqual(7, retainedBoost.ExtraRunSpeed.Whole, "released boost retains numeric speed");
    }

    // The active suit/phase pointer lists are compiled. Distinct first/second
    // colors at their actual targets prove the one-then-four timer progression.
    AssertTrue(SamusShadeDefinitions.FullBodyCycles.TryActiveSpeedBoosterPalettePointer(
        0, 0, out ushort powerBoostFirst), "compiled first Power boost phase exists");
    AssertTrue(SamusShadeDefinitions.FullBodyCycles.TryActiveSpeedBoosterPalettePointer(
        0, 2, out ushort powerBoostSecond), "compiled second Power boost phase exists");
    SyntheticSamusCartridge.WriteSamusPaletteColors(bus, powerBoostFirst, 0x1234);
    SyntheticSamusCartridge.WriteSamusPaletteColors(bus, powerBoostSecond, 0x4567);
    SyntheticSamusCartridge.WriteSamusPaletteColors(
        bus, SamusPaletteNative.NormalSuitPalettePointer(0), 0x0321);

    var boostColours = new SamusColours();
    booster.UpdateSpeedBoosterGlow(boostColours, new SpeedBoosterGlowFrame { MovementType = SamusMovementType.Running, AnimationOnSpinStart = (0) == 0, AnimationPastScrewAttackSpin = (0) >= 27, AnimationBeforeWallJumpSpin = (0) < 3, EquippedItems = SamusEquipmentFlags.SpeedBooster, SuppressActiveSpeedBoosterGlow = false, BottomBoundarySubmerged = false });
    AssertEqual(0x1234, boostColours.ShownSuit((InstalledFixtures.SamusShadeSources with { Suits = SuitColors(), FullBodyCycles = CycleColors() }))[0], "first Speed Booster palette comes from bank $9B");
    AssertEqual(1, booster.CycleShadeStep, "Speed Booster palette advances to its second step");
    AssertEqual(4, booster.CycleShadeTimer, "Speed Booster palette reloads four-frame timer");
    for (int paletteTick = 0; paletteTick < 3; paletteTick++)
    {
        booster.UpdateSpeedBoosterGlow(boostColours, new SpeedBoosterGlowFrame { MovementType = SamusMovementType.Running, AnimationOnSpinStart = (0) == 0, AnimationPastScrewAttackSpin = (0) >= 27, AnimationBeforeWallJumpSpin = (0) < 3, EquippedItems = SamusEquipmentFlags.SpeedBooster, SuppressActiveSpeedBoosterGlow = false, BottomBoundarySubmerged = false });
    }
    booster.UpdateSpeedBoosterGlow(boostColours, new SpeedBoosterGlowFrame { MovementType = SamusMovementType.Running, AnimationOnSpinStart = (0) == 0, AnimationPastScrewAttackSpin = (0) >= 27, AnimationBeforeWallJumpSpin = (0) < 3, EquippedItems = SamusEquipmentFlags.SpeedBooster, SuppressActiveSpeedBoosterGlow = false, BottomBoundarySubmerged = false });
    AssertEqual(0x4567, boostColours.ShownSuit((InstalledFixtures.SamusShadeSources with { Suits = SuitColors(), FullBodyCycles = CycleColors() }))[0], "second Speed Booster palette pointer");

    // `$91:D9B2-$D9D8` performs its bottom-boundary liquid gate before either the Screw
    // Attack or active Speed Booster branches. A submerged Power/Varia body returns with
    // carry set: neither CGRAM nor the shared timer/index words may move. Gravity Suit's
    // palette-index bit two bypasses that exact gate and must still follow the ROM pointer.
    AssertTrue(SamusShadeDefinitions.FullBodyCycles.TryActiveSpeedBoosterPalettePointer(
        4, 0, out ushort gravityBoostFirst), "compiled first Gravity boost phase exists");
    SyntheticSamusCartridge.WriteSamusPaletteColors(bus, gravityBoostFirst, 0x6a5a);
    var submergedBoost = new SamusHorizontalSpeedState(SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus()) { SpeedBoostCounter = new SamusSpeedBoostCounter(4, 1) };
    var submergedBoostColours = new SamusColours();
    // A sentinel shade shows whether the handler changed the suit's (#1512).
    submergedBoostColours.ShowSuit(SamusShades.ChargedShotGlow);
    submergedBoost.UpdateSpeedBoosterGlow(submergedBoostColours, new SpeedBoosterGlowFrame { MovementType = SamusMovementType.Running, AnimationOnSpinStart = (0) == 0, AnimationPastScrewAttackSpin = (0) >= 27, AnimationBeforeWallJumpSpin = (0) < 3, EquippedItems = SamusEquipmentFlags.SpeedBooster, BottomBoundarySubmerged = true, SuppressActiveSpeedBoosterGlow = false });
    submergedBoost.UpdateSpeedBoosterGlow(submergedBoostColours, new SpeedBoosterGlowFrame { MovementType = SamusMovementType.Running, AnimationOnSpinStart = (0) == 0, AnimationPastScrewAttackSpin = (0) >= 27, AnimationBeforeWallJumpSpin = (0) < 3, EquippedItems = (SamusEquipmentFlags.VariaSuit | SamusEquipmentFlags.SpeedBooster), BottomBoundarySubmerged = true, SuppressActiveSpeedBoosterGlow = false });
    AssertEqual(SamusShades.ChargedShotGlow, submergedBoostColours.Suit,
        "submerged Speed Booster leaves live Samus palette untouched");
    // A glow not yet started holds one update: its next boosted update moves it on (#2447).
    AssertEqual(1, submergedBoost.CycleShadeTimer,
        "submerged Speed Booster freezes common palette timer");
    AssertEqual(0, submergedBoost.CycleShadeStep,
        "submerged Speed Booster freezes palette-list offset");
    submergedBoost.UpdateSpeedBoosterGlow(submergedBoostColours, new SpeedBoosterGlowFrame { MovementType = SamusMovementType.Running, AnimationOnSpinStart = (0) == 0, AnimationPastScrewAttackSpin = (0) >= 27, AnimationBeforeWallJumpSpin = (0) < 3, EquippedItems = (SamusEquipmentFlags.GravitySuit | SamusEquipmentFlags.SpeedBooster), BottomBoundarySubmerged = true, SuppressActiveSpeedBoosterGlow = false });
    AssertEqual(0x6a5a, submergedBoostColours.ShownSuit((InstalledFixtures.SamusShadeSources with { Suits = SuitColors(), FullBodyCycles = CycleColors() }))[0],
        "submerged Gravity Suit Speed Booster palette remains ROM-authored");

    // The same early return surrounds Screw Attack. Exercise it independently so a future
    // refactor cannot fix running boost while accidentally leaving the spin palette active.
    AssertTrue(SamusShadeDefinitions.FullBodyCycles.TryScrewAttackPalettePointer(
        4, 0, out ushort gravityScrewFirst), "compiled first Gravity Screw phase exists");
    SyntheticSamusCartridge.WriteSamusPaletteColors(bus, gravityScrewFirst, 0x5b4b);
    var submergedScrew = new SamusHorizontalSpeedState(SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus());
    var submergedScrewColours = new SamusColours();
    submergedScrewColours.ShowSuit(SamusShades.ChargedShotGlow);
    submergedScrew.UpdateSpeedBoosterGlow(submergedScrewColours, new SpeedBoosterGlowFrame { MovementType = SamusMovementType.SpinJumping, AnimationOnSpinStart = (26) == 0, AnimationPastScrewAttackSpin = (26) >= 27, AnimationBeforeWallJumpSpin = (26) < 3, EquippedItems = SamusEquipmentFlags.ScrewAttack, BottomBoundarySubmerged = true, SuppressActiveSpeedBoosterGlow = false });
    AssertEqual(SamusShades.ChargedShotGlow, submergedScrewColours.Suit,
        "submerged Screw Attack retains the current suit colors");
    AssertEqual(0, submergedScrew.CycleShadeStep,
        "submerged Screw Attack freezes palette-list offset");
    submergedScrew.UpdateSpeedBoosterGlow(submergedScrewColours, new SpeedBoosterGlowFrame { MovementType = SamusMovementType.SpinJumping, AnimationOnSpinStart = (26) == 0, AnimationPastScrewAttackSpin = (26) >= 27, AnimationBeforeWallJumpSpin = (26) < 3, EquippedItems = (SamusEquipmentFlags.GravitySuit | SamusEquipmentFlags.ScrewAttack), BottomBoundarySubmerged = true, SuppressActiveSpeedBoosterGlow = false });
    AssertEqual(0x5b4b, submergedScrewColours.ShownSuit((InstalledFixtures.SamusShadeSources with { Suits = SuitColors(), FullBodyCycles = CycleColors() }))[0],
        "submerged Gravity Screw palette remains ROM-authored");

    SyntheticSamusCartridge.WriteSamusPaletteColors(
        bus, SamusPaletteNative.NormalSuitPalettePointer(4), 0x3a2a);
    submergedScrew.UpdateSpeedBoosterGlow(submergedScrewColours, new SpeedBoosterGlowFrame { MovementType = SamusMovementType.SpinJumping, AnimationOnSpinStart = (27) == 0, AnimationPastScrewAttackSpin = (27) >= 27, AnimationBeforeWallJumpSpin = (27) < 3, EquippedItems = (SamusEquipmentFlags.GravitySuit | SamusEquipmentFlags.ScrewAttack), BottomBoundarySubmerged = true, SuppressActiveSpeedBoosterGlow = false });
    AssertEqual(0x3a2a, submergedScrewColours.ShownSuit((InstalledFixtures.SamusShadeSources with { Suits = SuitColors(), FullBodyCycles = CycleColors() }))[0],
        "late Gravity Screw uses the fixture's normal suit palette");
    // `$90:EEE7` samples post-movement positions only on game-time multiples of four and
    // alternates native word offsets zero/two. These become the two trailing bodies drawn
    // by `$90:87BD`; capture itself deliberately contains no interpolation.
    AssertTrue(!booster.CaptureSpeedEchoPosition(3, new(100, 80)), "speed echo skips non-fourth frame");
    AssertTrue(booster.CaptureSpeedEchoPosition(4, new(101, 81)), "speed echo captures the first echo");
    AssertEqual(SpeedEchoPhase.CapturingSecond, booster.SpeedEchoPhase, "speed echo advances to the second echo");
    AssertTrue(booster.CaptureSpeedEchoPosition(8, new(105, 82)), "speed echo captures the second echo");
    AssertEqual(SpeedEchoPhase.CapturingFirst, booster.SpeedEchoPhase, "speed echo alternation wraps after the second echo");
    AssertEqual(101, booster.FirstSpeedEchoPosition!.Value.X, "first speed echo X snapshot");
    AssertEqual(105, booster.SecondSpeedEchoPosition!.Value.X, "second speed echo X snapshot");

    booster.CancelRunningMomentum(SamusFacingDirection.Right);
    AssertTrue(booster.SuitShade.IsDue,
        "CancelSpeedBoost publishes normal-suit palette restoration");
    AssertEqual(SpeedEchoPhase.Departing, booster.SpeedEchoPhase,
        "right-facing cancellation enters high-bit echo departure");
    AssertEqual(8, booster.FirstSpeedEchoHorizontalSpeed,
        "right-facing first departure speed is positive eight");
    AssertEqual(8, booster.SecondSpeedEchoHorizontalSpeed,
        "right-facing second departure speed is positive eight");

    // `$90:87D3-$90:884B` advances the second echo, then the first echo as an actual draw side effect.
    // Both stored bodies trail a rightward-running Samus, so +8 approaches X=120 while Y
    // independently approaches 90 by two. Crossing clears an echo before it can emit OAM.
    booster.AdvanceDepartingSpeedEcho(SpeedEcho.Second, new(120, 90));
    AssertTrue(booster.SecondSpeedEchoPosition is not null,
        "right departure the second echo remains before crossing");
    AssertEqual(113, booster.SecondSpeedEchoPosition!.Value.X,
        "right departure the second echo advances positive eight");
    AssertEqual(84, booster.SecondSpeedEchoPosition!.Value.Y,
        "right departure the second echo approaches Samus Y by two");
    booster.AdvanceDepartingSpeedEcho(SpeedEcho.First, new(120, 90));
    AssertTrue(booster.FirstSpeedEchoPosition is not null,
        "right departure the first echo remains before crossing");
    AssertEqual(109, booster.FirstSpeedEchoPosition!.Value.X,
        "right departure the first echo advances positive eight");
    AssertEqual(83, booster.FirstSpeedEchoPosition!.Value.Y,
        "right departure the first echo approaches Samus Y by two");
    booster.FinishDepartingSpeedEchoFrame();
    AssertEqual(SpeedEchoPhase.Departing, booster.SpeedEchoPhase,
        "departure index survives while either echo remains");

    // Samus_CancelSpeedBoost is called every applicable standing/turn frame. Its BMI guard
    // must preserve an already-running departure even if the new pose faces the other way.
    booster.CancelRunningMomentum(SamusFacingDirection.Left);
    AssertEqual(8, booster.FirstSpeedEchoHorizontalSpeed,
        "repeated cancellation cannot reverse an active departure");
    booster.AdvanceDepartingSpeedEcho(SpeedEcho.Second, new(120, 90));
    AssertTrue(booster.SecondSpeedEchoPosition is null,
        "right departure the second echo clears on crossing");
    AssertTrue(booster.SecondSpeedEchoPosition is null,
        "crossed the second echo holds no echo");
    booster.AdvanceDepartingSpeedEcho(SpeedEcho.First, new(120, 90));
    AssertTrue(booster.FirstSpeedEchoPosition is not null,
        "right departure the first echo remains one more frame");
    booster.FinishDepartingSpeedEchoFrame();
    AssertEqual(SpeedEchoPhase.Departing, booster.SpeedEchoPhase,
        "one surviving departure keeps high-bit index");
    booster.AdvanceDepartingSpeedEcho(SpeedEcho.First, new(120, 90));
    AssertTrue(booster.FirstSpeedEchoPosition is null,
        "right departure the first echo eventually crosses");
    booster.FinishDepartingSpeedEchoFrame();
    AssertEqual(SpeedEchoPhase.CapturingFirst, booster.SpeedEchoPhase,
        "last crossed departure restores ordinary echo index");

    // Mirror the same signed-word comparison for a left-facing cancellation, with both echoes
    // captured by an ordinary full boost.
    var leftDeparture = new SamusHorizontalSpeedState(SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus()) { SpeedBoostCounter = SamusSpeedBoostCounter.AtFullStage };
    AssertTrue(leftDeparture.CaptureSpeedEchoPosition(0, new(150, 90)), "left departure captures its first echo");
    AssertTrue(leftDeparture.CaptureSpeedEchoPosition(4, new(154, 94)), "left departure captures its second echo");
    leftDeparture.CancelRunningMomentum(SamusFacingDirection.Left);
    AssertEqual(-8, leftDeparture.FirstSpeedEchoHorizontalSpeed,
        "left-facing first departure speed is negative eight");
    AssertEqual(-8, leftDeparture.SecondSpeedEchoHorizontalSpeed,
        "left-facing second departure speed is negative eight");
    leftDeparture.AdvanceDepartingSpeedEcho(SpeedEcho.Second, new(140, 100));
    AssertTrue(leftDeparture.SecondSpeedEchoPosition is not null,
        "left departure the second echo remains before crossing");
    AssertEqual(146, leftDeparture.SecondSpeedEchoPosition!.Value.X,
        "left departure the second echo advances negative eight");
    AssertEqual(96, leftDeparture.SecondSpeedEchoPosition!.Value.Y,
        "left departure the second echo approaches Samus Y by two");
    leftDeparture.AdvanceDepartingSpeedEcho(SpeedEcho.First, new(140, 100));
    AssertTrue(leftDeparture.FirstSpeedEchoPosition is not null,
        "left departure the first echo remains before crossing");
    AssertEqual(142, leftDeparture.FirstSpeedEchoPosition!.Value.X,
        "left departure the first echo advances negative eight");
    leftDeparture.AdvanceDepartingSpeedEcho(SpeedEcho.Second, new(140, 100));
    AssertTrue(leftDeparture.SecondSpeedEchoPosition is null,
        "left departure the second echo clears after passing Samus");
    leftDeparture.AdvanceDepartingSpeedEcho(SpeedEcho.First, new(140, 100));
    AssertTrue(leftDeparture.FirstSpeedEchoPosition is null,
        "left departure the first echo clears after passing Samus");
    leftDeparture.FinishDepartingSpeedEchoFrame();
    AssertEqual(SpeedEchoPhase.CapturingFirst, leftDeparture.SpeedEchoPhase,
        "left departure clears shared index after both slots cross");

    booster.UpdateSpeedBoosterGlow(boostColours, new SpeedBoosterGlowFrame { MovementType = SamusMovementType.Standing, AnimationOnSpinStart = (0) == 0, AnimationPastScrewAttackSpin = (0) >= 27, AnimationBeforeWallJumpSpin = (0) < 3, EquippedItems = SamusEquipmentFlags.SpeedBooster, SuppressActiveSpeedBoosterGlow = false, BottomBoundarySubmerged = false });
    AssertEqual(0x0321, boostColours.ShownSuit((InstalledFixtures.SamusShadeSources with { Suits = SuitColors(), FullBodyCycles = CycleColors() }))[0], "cancel restores ROM-authored Power Suit palette");
    AssertTrue(!booster.SuitShade.IsDue, "normal-suit palette request is one-shot");

    var boostedJump = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()) { EquippedItems = SamusEquipmentFlags.SpeedBooster };
    boostedJump.Kinematics.VerticalVelocity = boostedJump.Kinematics.VerticalVelocity.WithSpeed(new UnsignedSixteenSixteen(4, 0xe000));
    boostedJump.HorizontalSpeed.ExtraRunSpeed = new UnsignedSixteenSixteen(3, 0x4000);
    SamusAerialMovement.ApplyEquippedSpeedBoosterJumpBonus(boostedJump);
    AssertEqual(5, boostedJump.Kinematics.VerticalVelocity.Speed.Whole, "boosted jump adds half whole extra speed");
    AssertEqual(0x2000, boostedJump.Kinematics.VerticalVelocity.Speed.Fraction,
        "boosted jump wraps fractional addition without carrying");

    // $90:E4E6 caps the divisor at four halvings. Extra run speed is added before
    // that shift; 2.0 + 2.0 therefore becomes 0.4000 when the largest divisor, five, is stored.
    speed.ExtraRunSpeed = new UnsignedSixteenSixteen(2, 0);
    speed.SlowdownLevel = SamusHorizontalSpeedState.LargestSlowdownLevel;
    AssertEqual(0x00004000u, speed.CalculateTotalSpeed(UnsignedSixteenSixteen.FromRaw(0x00020000)).RawValue, "total speed divisor caps at four");
    AssertEqual(0x4000, speed.TotalSpeed.Fraction, "total subspeed publication");

    // The displacement clamp replaces only the signed whole word and preserves the low
    // fraction. These deliberately oversized values catch a tempting floating-point clamp.
    speed.ExtraRunSpeed = speed.ExtraRunSpeed.WithWhole(0);
    speed.SlowdownLevel = 0;
    AssertEqual(0x000f1234, speed.CalculateDisplacement(HorizontalDirection.Right, UnsignedSixteenSixteen.FromRaw(0x00101234), extraDisplacement: default).RawValue, "right displacement +15 clamp");
    // Negation happens before clamping, so 0 - $0010:1234 is $FFEF:EDCC. The clamp
    // replaces only its high word, producing $FFF1:EDCC rather than mirroring $1234.
    AssertEqual(unchecked((int)0xfff1edcc), speed.CalculateDisplacement(HorizontalDirection.Left, UnsignedSixteenSixteen.FromRaw(0x00101234), extraDisplacement: default).RawValue, "left displacement -15 clamp");

    Console.WriteLine("  Samus speed: acceleration, deceleration, submerged palette gate, boost departure echoes, divisor, and clamp agree.");
}

/// <summary>
/// Exercises the four persistent external-displacement words through real movement-family
/// consumers. The fixtures are intentionally all-air so accepted 16.16 movement exposes
/// arithmetic directly instead of conflating it with the separately verified block clipper.
/// </summary>
internal static void VerifySamusExtraDisplacement()
{
    var bus = new TestAddressSpace();

    // Literal right-standing and normal-jump records. Only direction/movement/radius are
    // needed here, but using the retail bytes ensures production follows its normal metadata
    // path instead of a test-only pose shortcut.
    WritePoseDefinition(bus, 0x01, [0x08, 0x00, 0xff, 0x02, 0x06, 0x00, 0x15, 0x00]); // $01
    WritePoseDefinition(
        bus,
        SamusPoseNumbers.Number(SamusPoseId.MorphBallFallingRightPose),
        [0x08, 0x08, 0xff, 0xff, 0x00, 0x00, 0x07, 0x00]); // $31
    WritePoseDefinition(bus, 0x4B, [0x08, 0x02, 0xff, 0x02, 0x03, 0x00, 0x13, 0x00]); // $4B
    WritePoseDefinition(bus, 0x4D, [0x08, 0x02, 0xff, 0x02, 0x08, 0x00, 0x13, 0x00]); // $4D

    // Type-two normal-air X physics is deliberately zero. That isolates extra displacement
    // while still exercising the real pointer selection and no-input base-speed clear.
    SyntheticSamusCartridge.WriteNormalAirSpeedEntry(bus, SamusMovementType.NormalJumping, new byte[12]);

    // Falling Morph Ball uses the type-eight record. Keeping it at exactly zero prevents
    // ordinary rolling physics from obscuring the external-producer bounce override below.
    SyntheticSamusCartridge.WriteNormalAirSpeedEntry(bus, SamusMovementType.MorphBallFalling, new byte[12]);

    const int width = 16;
    const int height = 16;
    RoomLevelData level = CreateEmptyRoom(new(width, height));

    var standing = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
    {
        Pose = SamusPoseId.FacingRightNormalPose,
        Position = FixedPosition<RoomPixels>.FromWhole(new(64, 64)),
    };
    standing.Kinematics.Radius = new(5, 21);
    standing.Kinematics.ExtraDisplacement = FixedVector<RoomPixels>.FromStoredParts(1, 0x8000, 2, 0x4000);
    GroundedMovementResult positive = SamusGroundedMovement.StepStanding(

        level,
        standing,
        frameCounter: 0,
        controllerInput: 0, roomObjects: SamusRoomObjectFixtures.Uninstalled);
    AssertEqual(0x00018000, positive.Horizontal.AcceptedDisplacement.RawValue,
        "standing external +X bypasses zero base speed");
    AssertEqual(0x00034000, positive.Vertical.AcceptedDisplacement.RawValue,
        "positive no-speed external Y gains one whole pixel");
    AssertEqual(1, standing.Kinematics.ExtraDisplacement.X.WholePixels,
        "movement does not consume persistent external X producer word");
    AssertEqual(2, standing.Kinematics.ExtraDisplacement.Y.WholePixels,
        "movement does not consume persistent external Y producer word");

    var negative = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
    {
        Pose = SamusPoseId.FacingRightNormalPose,
        Position = FixedPosition<RoomPixels>.FromWhole(new(64, 64)),
    };
    negative.Kinematics.Radius = new(5, 21);
    negative.Kinematics.ExtraDisplacement = negative.Kinematics.ExtraDisplacement.WithWholeY(-2).WithFractionY(0x8000);
    GroundedMovementResult upward = SamusGroundedMovement.StepStanding(

        level,
        negative,
        frameCounter: 1,
        controllerInput: 0, roomObjects: SamusRoomObjectFixtures.Uninstalled);
    AssertEqual(unchecked((int)0xfffe8000), upward.Vertical.AcceptedDisplacement.RawValue,
        "negative no-speed external Y has no downward bias");

    var transition = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
    {
        Pose = SamusPoseId.NeutralJumpTransitionRightPose,
        Position = FixedPosition<RoomPixels>.FromWhole(new(64, 64)),
    };
    transition.Kinematics.Radius = new(5, 19);
    transition.Kinematics.ExtraDisplacement = FixedVector<RoomPixels>.FromStoredParts(0xffff, 0x8000, 0, 0x4000); // -0.8000 // +0.4000
    FixedCoordinate transitionXBefore = transition.Kinematics.Position.X;
    FixedCoordinate transitionYBefore = transition.Kinematics.Position.Y;
    SamusAerialMovement.StepNormalJump(

        level,
        transition,
        GameplayAction.Jump,
        frameCounter: 0, roomObjects: SamusRoomObjectFixtures.Uninstalled);
    AssertEqual(SixteenSixteenWords.ToDisplacement(unchecked((int)0xffff8000)), transition.Kinematics.Position.X - transitionXBefore,
        "jump transition applies signed external X with zero base speed");
    AssertEqual(SixteenSixteenWords.ToDisplacement(0x00014000), transition.Kinematics.Position.Y - transitionYBefore,
        "jump transition applies extra-only Y with positive bias");

    var rising = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
    {
        Pose = SamusPoseId.NeutralJumpRightPose,
        Position = FixedPosition<RoomPixels>.FromWhole(new(64, 64)),
    };
    rising.Kinematics.Radius = new(5, 19);
    rising.Kinematics.VerticalVelocity = SamusVerticalVelocityConstants.Of(new UnsignedSixteenSixteen(1, 0), SamusVerticalDirection.Up);
    rising.Kinematics.ExtraDisplacement = rising.Kinematics.ExtraDisplacement.WithWholeY(1).WithFractionY(0x8000);
    FixedCoordinate risingYBefore = rising.Kinematics.Position.Y;
    SamusAerialMovement.StepNormalJump(

        level,
        rising,
        GameplayAction.Jump,
        frameCounter: 0, roomObjects: SamusRoomObjectFixtures.Uninstalled);
    AssertEqual(SixteenSixteenWords.ToDisplacement(0x00008000), rising.Kinematics.Position.Y - risingYBefore,
        "gravity path adds external Y directly and may reverse actual direction");
    AssertEqual(SamusVerticalDirection.Up, rising.Kinematics.VerticalVelocity.Direction,
        "external reversal does not rewrite native velocity-direction word");

    var bouncingBall = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
    {
        Pose = SamusPoseId.MorphBallFallingRightPose,
        Position = FixedPosition<RoomPixels>.FromWhole(new(64, 64)),
        MorphBallBounceState = SamusBallBounceState.FirstRebound,
        KnockbackDirection = SamusKnockbackDirection.None,
    };
    bouncingBall.Kinematics.Radius = new(5, 7);
    bouncingBall.Kinematics.VerticalVelocity = SamusVerticalVelocityConstants.Of(new UnsignedSixteenSixteen(3, 0x4000), SamusVerticalDirection.Up);
    bouncingBall.Kinematics.ExtraDisplacement = bouncingBall.Kinematics.ExtraDisplacement.WithWholeY(-1).WithFractionY(0x8000); // -0.8000
    MorphBallMovementResult displacedBounce = SamusMorphBallMovement.StepFalling(

        level,
        bouncingBall,
        controllerInput: 0,
        frameCounter: 0, roomObjects: SamusRoomObjectFixtures.Uninstalled);
    AssertEqual(unchecked((int)0xffff8000), displacedBounce.Vertical.AcceptedDisplacement.RawValue,
        "external Y replaces an active Morph Ball rebound rather than joining gravity");
    AssertEqual(0, bouncingBall.Kinematics.VerticalVelocity.Speed.Whole,
        "external Morph Ball bounce override clears whole rebound speed");
    AssertEqual(0, bouncingBall.Kinematics.VerticalVelocity.Speed.Fraction,
        "external Morph Ball bounce override clears fractional rebound speed");
    AssertEqual(SamusVerticalDirection.Down, bouncingBall.Kinematics.VerticalVelocity.Direction,
        "external Morph Ball bounce override forces native direction word two");
    AssertTrue(!displacedBounce.HitCeiling && !displacedBounce.Landed,
        "unobstructed signed-negative Morph Ball producer moves without a false collision");

    Console.WriteLine(
        "  Samus displacement: signed X/Y, grounded bias, transition motion, persistence, gravity reversal, and Morph Ball override agree.");
}

    /// <summary>Runs the isolated horizontal speed fixture.</summary>
    [VerificationCommand("--horizontal-speed-fixture")]
    internal static int HorizontalSpeedFixtureCommand()
    {
     VerifySamusHorizontalSpeed(); return 0; 
    }

    /// <summary>Runs the full Samus horizontal speed comparison.</summary>
    [VerificationCommand("--samus-horizontal-speed")]
    internal static int SamusHorizontalSpeedCommand()
    {
        Suite(nameof(VerifySamusHorizontalSpeed), () => VerifySamusHorizontalSpeed());
        return 0;
    }
}
