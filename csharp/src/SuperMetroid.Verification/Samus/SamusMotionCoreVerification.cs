using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.SyntheticSamusCartridgeVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Artwork;
namespace SuperMetroid.Verification.Samus;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Samus pose-input matching, horizontal speed, and external displacement arithmetic.</summary>
internal static class SamusMotionCoreVerification
{

/// <summary>Checks ordered pose-input matches, extra held bits, self-pose suppression, and fallback paths.</summary>
internal static void VerifySamusPoseTransitionMatching()
{
    var bus = new TestAddressSpace();
    SamusPoseId fixturePose = SamusPoseId.FacingRightNormalPose;

    // The authored standing-right table at $91:A0EC demonstrates priority and subset
    // matching: Jump+Up outranks plain Up, which outranks Right because the matcher stops
    // at the first record whose complete required masks are present.

    SamusPoseTransition jumpUp = (SamusPoseTransitionTable.Find(
        bus,
        currentPose: fixturePose,
        canonicalHeldInput: 0x0980,
        canonicalNewInput: 0x0080) ?? throw new InvalidOperationException("Verification requires SamusPoseTransitionTable.Find( bus, currentPose: fixturePose, canonicalHeldInput: 0x098...."));
    AssertEqual(0x55, (byte)jumpUp.ProspectivePose, "Samus transition required-new plus held chord");
    AssertTrue(SamusPoseInputDefinitions.TryGetPointer(fixturePose, out _),
        "Samus transition fixture pose has an authored table");

    SamusPoseTransition up = (SamusPoseTransitionTable.Find(
        bus,
        currentPose: fixturePose,
        canonicalHeldInput: (ushort)(SnesButton.Up | SnesButton.Y),
        canonicalNewInput: 0) ?? throw new InvalidOperationException("Verification requires SamusPoseTransitionTable.Find( bus, currentPose: fixturePose, canonicalHeldInput: (usho...."));
    AssertEqual(0x03, (byte)up.ProspectivePose, "Samus transition permits extra held action");

    SamusPoseTransition right = (SamusPoseTransitionTable.Find(
        bus,
        currentPose: fixturePose,
        canonicalHeldInput: 0x0100,
        canonicalNewInput: 0) ?? throw new InvalidOperationException("Verification requires SamusPoseTransitionTable.Find( bus, currentPose: fixturePose, canonicalHeldInput: 0x010...."));
    AssertEqual(0x09, (byte)right.ProspectivePose, "Samus standing Right proposes running pose");

    AssertEqual<SamusPoseTransition?>(null,
        SamusPoseTransitionTable.Find(bus, fixturePose, canonicalHeldInput: 0, canonicalNewInput: 0),
        "Samus zero input bypasses transition table");
    AssertEqual<SamusPoseTransition?>(null,
        SamusPoseTransitionTable.Find(
            bus,
            SamusPoseId.MovingRightGunExtendedPose,
            canonicalHeldInput: (ushort)(SnesButton.Right | SnesButton.X),
            canonicalNewInput: 0),
        "Samus same-pose transition is suppressed");
    AssertEqual<SamusPoseTransition?>(null,
        SamusPoseTransitionTable.Find(bus, fixturePose, canonicalHeldInput: 0x0400, canonicalNewInput: 0),
        "Samus transition terminator returns no match");

    SamusPoseTransitionLookup zeroInput = SamusPoseTransitionTable.Lookup(
        bus, fixturePose, canonicalHeldInput: 0, canonicalNewInput: 0);
    SamusPoseTransitionLookup samePose = SamusPoseTransitionTable.Lookup(
        bus,
        SamusPoseId.MovingRightGunExtendedPose,
        canonicalHeldInput: (ushort)(SnesButton.Right | SnesButton.X),
        canonicalNewInput: 0);
    SamusPoseTransitionLookup terminator = SamusPoseTransitionTable.Lookup(
        bus, fixturePose, canonicalHeldInput: 0x0400, canonicalNewInput: 0);
    AssertTrue(zeroInput.UsesPoseDefinitionFallback,
        "Samus zero input enters pose-definition fallback");
    AssertTrue(!samePose.UsesPoseDefinitionFallback,
        "Samus same-pose record returns without fallback");
    AssertTrue(terminator.UsesPoseDefinitionFallback,
        "Samus unmatched nonzero input enters pose-definition fallback");

    // Pose $2F uses the authored empty list at $91:A0DC. Its immediate terminator is a
    // direct return for held input, unlike exhaustion after at least one unmatched record.
    const SamusPoseId emptyListPose = SamusPoseId.TurningRightToLeftJumpPose;
    AssertTrue(!SamusPoseTransitionTable.Lookup(bus, emptyListPose, (ushort)SnesButton.A, 0).UsesPoseDefinitionFallback,
        "empty pose table preserves turn momentum while Jump remains held");
    AssertTrue(SamusPoseTransitionTable.Lookup(bus, emptyListPose, 0, 0).UsesPoseDefinitionFallback,
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
[PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
internal static void VerifySamusHorizontalSpeed()
{
    var bus = new TestAddressSpace();
    SamusFullBodyCycleColorCatalog CycleColors() =>
        SamusFullBodyCycleColorCatalog.Load(new MemoryStream(
            SuperMetroid.AssetExtraction.Samus.SamusFullBodyCycleColorExtractor.Extract(bus)));
    SamusSuitColorCatalog SuitColors() =>
        SamusSuitColorCatalog.Load(new MemoryStream(
            SuperMetroid.AssetExtraction.Samus.SamusSuitColorExtractor.Extract(bus)));

    // Normal air stores $9F55. Running movement type one advances by one more 12-byte
    // record to $9F61: acceleration 0.3000, maximum 2.C000, deceleration 0.8000.
    SyntheticSamusCartridge.WriteNormalAirSpeedEntry(bus, SamusMovementType.Running, [
        0x00, 0x00, 0x00, 0x30,
        0x02, 0x00, 0x00, 0xc0,
        0x00, 0x00, 0x00, 0x80,
    ]);

    var speed = new SamusHorizontalSpeedState();
    speed.SelectNormalAirSpeedTable();
    AssertEqual(0x909f61, speed.ResolveEntryAddress(movementType: SamusMovementType.Running), "running speed entry pointer chain");

    SpeedTableEntry entry = speed.ReadEntry(bus, movementType: SamusMovementType.Running);
    AssertEqual(0x0000, entry.Acceleration, "running acceleration whole word");
    AssertEqual(0x3000, entry.AccelerationSubspeed, "running acceleration fraction");
    AssertEqual(0x0002, entry.MaximumSpeed, "running maximum whole word");
    AssertEqual(0xc000, entry.MaximumSubspeed, "running maximum fraction");
    AssertEqual(0x8000, entry.DecelerationSubspeed, "running deceleration fraction");

    // Four acceleration calls produce 0.C000 exactly. Fifteen calls would reach 2.D000,
    // so the native quirked comparison clamps that result down to the table's 2.C000 cap.
    StepFrames(4, _ => speed.CalculateBaseSpeed(bus, movementType: SamusMovementType.Running));
    AssertEqual(0x0000c000u, speed.BaseFixed, "running acceleration after four calls");
    StepFrames(11, _ => speed.CalculateBaseSpeed(bus, movementType: SamusMovementType.Running));
    AssertEqual(0x0002c000u, speed.BaseFixed, "running acceleration clamps at 2.C000");

    // Deceleration subtracts 0.8000 per call. Crossing below zero makes the signed high
    // word negative, clearing both halves and restoring acceleration mode zero.
    speed.AccelerationMode = 2;
    StepFrames(6, _ => speed.CalculateBaseSpeed(bus, movementType: SamusMovementType.Running));
    AssertEqual(0u, speed.BaseFixed, "running deceleration underflow clears speed");
    AssertEqual(0, speed.AccelerationMode, "running deceleration restores acceleration mode");

    // `$90:973E` adds the literal no-booster 0.1000 pair once per running+B frame. The
    // native cap test runs before addition, so call 32 reaches exactly 2.0000 and call 33
    // performs the visible clamp write without changing the pair.
    StepFrames(32, _ =>
        speed.HandleExtraRunSpeed(
            movementType: SamusMovementType.Running,
            controllerInput: (ushort)SnesButton.B,
            speedBoosterEquipped: false));
    AssertTrue(speed.HasRunningMomentum, "ordinary Dash establishes native momentum flag");
    AssertEqual(0, speed.SpeedBoostCounter, "ordinary Dash leaves booster stage zero");
    AssertEqual(2, speed.ExtraRunSpeed, "ordinary Dash whole-speed cap");
    AssertEqual(0, speed.ExtraRunSubspeed, "ordinary Dash fractional-speed cap");
    speed.HandleExtraRunSpeed(SamusMovementType.Running, (ushort)SnesButton.B, speedBoosterEquipped: false);
    AssertEqual(2, speed.ExtraRunSpeed, "ordinary Dash remains clamped on next call");

    // B release and an airborne movement type both take `$90:9808`; a set momentum flag
    // bypasses the numeric clear. Only the separately invoked cancel routine clears the
    // flag/counter, after which another non-running call clears the retained pair.
    speed.HandleExtraRunSpeed(SamusMovementType.Running, controllerInput: 0, speedBoosterEquipped: false);
    speed.HandleExtraRunSpeed(SamusMovementType.SpinJumping, controllerInput: 0, speedBoosterEquipped: false);
    AssertEqual(2, speed.ExtraRunSpeed, "Dash release and spin jump retain extra speed");
    speed.CancelRunningMomentum(SamusFacingDirection.Right);
    AssertTrue(!speed.HasRunningMomentum, "CancelSpeedBoost clears ordinary momentum flag");
    speed.HandleExtraRunSpeed(SamusMovementType.SpinJumping, controllerInput: 0, speedBoosterEquipped: false);
    AssertEqual(0, speed.ExtraRunSpeed, "post-cancel airborne handler clears extra speed");

    // Ordinary Dash uses the compiled native ten-frame, two-tick cadence. Do not
    // rewrite the cartridge pointer/delay ROM in the fake bus: gameplay now reads
    // those immutable definitions from SamusRunningCadenceDefinitions.
    var dashAnimation = new SamusState { Pose = SamusPoseId.MovingRightNormalPose };
    dashAnimation.Presentation.InitializeAnimation(bus);
    dashAnimation.HorizontalSpeed.HandleExtraRunSpeed(
        movementType: SamusMovementType.Running,
        controllerInput: (ushort)SnesButton.B,
        speedBoosterEquipped: false);
    for (int tick = 0; tick < 2; tick++)
        dashAnimation.Presentation.AnimateNoFx(bus, new SamusAnimationStep { ControllerInput = (ushort)SnesButton.B });
    AssertEqual(1, dashAnimation.AnimationFrame, "Dash advances into running frame one");
    AssertEqual(2, dashAnimation.AnimationFrameTimer.RawValue, "Dash selects native frame-one delay");
    for (int tick = 0; tick < 18; tick++)
        dashAnimation.Presentation.AnimateNoFx(bus, new SamusAnimationStep { ControllerInput = (ushort)SnesButton.B });
    AssertEqual(0, dashAnimation.AnimationFrame, "Dash command interception restarts frame zero");
    AssertEqual(2, dashAnimation.AnimationFrameTimer.RawValue, "Dash restart uses shared frame-zero delay");

    // Native stages zero through three last one command loop; stage four lasts two.
    // Their compiled cadence still publishes the native echo/contact-damage events.

    var booster = new SamusHorizontalSpeedState();
    booster.HandleExtraRunSpeed(
        movementType: SamusMovementType.Running,
        controllerInput: (ushort)SnesButton.B,
        speedBoosterEquipped: true);
    AssertTrue(booster.HasRunningMomentum, "Speed Booster establishes momentum");
    AssertEqual(1, booster.SpeedBoostCounter, "Speed Booster seeds native stage-zero countdown");
    AssertEqual(1, booster.SpecialPaletteTimer.RawValue, "Speed Booster seeds special-palette timer");

    // Hexadecimal `.1000` is one sixteenth, so 112 movement calls reach 7.0000 exactly.
    for (int frame = 1; frame < 112; frame++)
    {
        booster.HandleExtraRunSpeed(
            movementType: SamusMovementType.Running,
            controllerInput: (ushort)SnesButton.B,
            speedBoosterEquipped: true);
    }
    AssertEqual(7, booster.ExtraRunSpeed, "Speed Booster reaches exact 7.0000 cap");
    AssertEqual(0, booster.ExtraRunSubspeed, "Speed Booster cap has zero fraction");

    ushort boostFrame = 1;
    for (int command = 0; command < 4; command++)
    {
        bool intercepted = booster.TryAdvanceSpeedBoosterAnimationStage(
            bus,
            movementType: SamusMovementType.Running,
            controllerInput: (ushort)SnesButton.B,
            animationFrameBuffer: 0,
            ref boostFrame,
            out ushort boostTimer);
        if (command == 0)
        {
            AssertTrue(intercepted, "first stage-zero command advances Speed Booster");
            AssertEqual(0, boostFrame, "Speed Booster stage change restarts animation");
            AssertEqual(2, boostTimer, "stage-one delay matches native cadence");
        }
    }
    AssertEqual(0x0402, booster.SpeedBoostCounter, "Speed Booster reaches stage four countdown");
    AssertTrue(booster.EchoSoundRequested, "stage four publishes speed-echo sound event");
    AssertTrue(booster.ConsumeEchoSoundRequest(),
        "frontend can consume the speed-echo publication exactly once");
    AssertTrue(!booster.ConsumeEchoSoundRequest(),
        "consumed speed-echo publication cannot replay on a later frame");
    AssertEqual(SamusContactDamageIndex.None, booster.ContactDamageIndex, "stage four animation waits for movement to publish contact damage");

    // $90:9813 is a common epilogue, including the released/airborne/liquid
    // branches. Beta clears contact damage each frame; retained blue speed
    // must republish protection even though no extra acceleration occurs.
    foreach (var movement in new[] { SamusMovementType.Running, SamusMovementType.SpinJumping })
    foreach (bool liquid in new[] { false, true })
    foreach (bool activeStage in new[] { false, true })
    {
        var retainedBoost = new SamusHorizontalSpeedState
        {
            HasRunningMomentum = true,
            ExtraRunSpeed = 7,
            SpeedBoostCounter = activeStage ? (ushort)0x0401 : (ushort)0x0301,
            ContactDamageIndex = SamusContactDamageIndex.None,
        };
        retainedBoost.HandleExtraRunSpeed(movement, 0, true, liquidImpeded: liquid);
        AssertEqual(activeStage ? SamusContactDamageIndex.SpeedBooster : SamusContactDamageIndex.None, retainedBoost.ContactDamageIndex,
            "retained boost publishes contact protection only in active stage");
        AssertEqual(7, retainedBoost.ExtraRunSpeed, "released boost retains numeric speed");
    }

    // The active suit/phase pointer lists are compiled. Distinct first/second
    // colors at their actual targets prove the one-then-four timer progression.
    AssertTrue(SamusPaletteRomData.FullBodyCycles.TryActiveSpeedBoosterPalettePointer(
        0, 0, out ushort powerBoostFirst), "compiled first Power boost phase exists");
    AssertTrue(SamusPaletteRomData.FullBodyCycles.TryActiveSpeedBoosterPalettePointer(
        0, 2, out ushort powerBoostSecond), "compiled second Power boost phase exists");
    SyntheticSamusCartridge.WriteSamusPaletteColors(bus, powerBoostFirst, 0x1234);
    SyntheticSamusCartridge.WriteSamusPaletteColors(bus, powerBoostSecond, 0x4567);
    SyntheticSamusCartridge.WriteSamusPaletteColors(
        bus, SamusPaletteRomData.Common.NormalSuitPalettePointer(0), 0x0321);

    var boostCgram = new SnesCgram();
    AssertTrue(booster.UpdateSpeedBoosterPalette(bus, boostCgram, new SpeedBoosterPaletteFrame { MovementType = SamusMovementType.Running, AnimationFrame = 0, EquippedItems = 0x2000 }, suitColors: SuitColors(), cycleColors: CycleColors()),
        "stage-four palette timer one copies immediately");
    AssertEqual(0x1234, boostCgram.Colors[192], "first Speed Booster palette comes from bank $9B");
    AssertEqual(2, booster.SpecialPaletteFrame, "Speed Booster palette advances to pointer offset two");
    AssertEqual(4, booster.SpecialPaletteTimer.RawValue, "Speed Booster palette reloads four-frame timer");
    for (int paletteTick = 0; paletteTick < 3; paletteTick++)
    {
        AssertTrue(!booster.UpdateSpeedBoosterPalette(bus, boostCgram, new SpeedBoosterPaletteFrame { MovementType = SamusMovementType.Running, AnimationFrame = 0, EquippedItems = 0x2000 }),
            "Speed Booster palette waits during positive timer");
    }
    AssertTrue(booster.UpdateSpeedBoosterPalette(bus, boostCgram, new SpeedBoosterPaletteFrame { MovementType = SamusMovementType.Running, AnimationFrame = 0, EquippedItems = 0x2000 }, suitColors: SuitColors(), cycleColors: CycleColors()),
        "fourth Speed Booster palette tick copies next frame");
    AssertEqual(0x4567, boostCgram.Colors[192], "second Speed Booster palette pointer");

    // `$91:D9B2-$D9D8` performs its bottom-boundary liquid gate before either the Screw
    // Attack or active Speed Booster branches. A submerged Power/Varia body returns with
    // carry set: neither CGRAM nor the shared timer/index words may move. Gravity Suit's
    // palette-index bit two bypasses that exact gate and must still follow the ROM pointer.
    AssertTrue(SamusPaletteRomData.FullBodyCycles.TryActiveSpeedBoosterPalettePointer(
        4, 0, out ushort gravityBoostFirst), "compiled first Gravity boost phase exists");
    SyntheticSamusCartridge.WriteSamusPaletteColors(bus, gravityBoostFirst, 0x6a5a);
    var submergedBoost = new SamusHorizontalSpeedState { SpeedBoostCounter = 0x0401 };
    var submergedBoostCgram = new SnesCgram();
    submergedBoostCgram.SetColor(192, Bgr555.FromWord(0x7777));
    AssertTrue(!submergedBoost.UpdateSpeedBoosterPalette(bus, submergedBoostCgram, new SpeedBoosterPaletteFrame { MovementType = SamusMovementType.Running, AnimationFrame = 0, EquippedItems = 0x2000, BottomBoundarySubmerged = true }),
        "submerged Power Suit suppresses active Speed Booster palette copy");
    AssertTrue(!submergedBoost.UpdateSpeedBoosterPalette(bus, submergedBoostCgram, new SpeedBoosterPaletteFrame { MovementType = SamusMovementType.Running, AnimationFrame = 0, EquippedItems = 0x2001, BottomBoundarySubmerged = true }),
        "submerged Varia Suit suppresses active Speed Booster palette copy");
    AssertEqual(0x7777, submergedBoostCgram.Colors[192],
        "submerged Speed Booster leaves live Samus palette untouched");
    AssertEqual(0, submergedBoost.SpecialPaletteTimer.RawValue,
        "submerged Speed Booster freezes common palette timer");
    AssertEqual(0, submergedBoost.SpecialPaletteFrame,
        "submerged Speed Booster freezes palette-list offset");
    AssertTrue(submergedBoost.UpdateSpeedBoosterPalette(bus, submergedBoostCgram, new SpeedBoosterPaletteFrame { MovementType = SamusMovementType.Running, AnimationFrame = 0, EquippedItems = (SamusEquipmentFlags.GravitySuit | SamusEquipmentFlags.SpeedBooster).ToNativeWord(), BottomBoundarySubmerged = true }, suitColors: SuitColors(), cycleColors: CycleColors()),
        "submerged Gravity Suit bypasses Speed Booster palette suppression");
    AssertEqual(0x6a5a, submergedBoostCgram.Colors[192],
        "submerged Gravity Suit Speed Booster palette remains ROM-authored");

    // The same early return surrounds Screw Attack. Exercise it independently so a future
    // refactor cannot fix running boost while accidentally leaving the spin palette active.
    AssertTrue(SamusPaletteRomData.FullBodyCycles.TryScrewAttackPalettePointer(
        4, 0, out ushort gravityScrewFirst), "compiled first Gravity Screw phase exists");
    SyntheticSamusCartridge.WriteSamusPaletteColors(bus, gravityScrewFirst, 0x5b4b);
    var submergedScrew = new SamusHorizontalSpeedState();
    var submergedScrewCgram = new SnesCgram();
    submergedScrewCgram.SetColor(192, Bgr555.FromWord(0x2222));
    AssertTrue(!submergedScrew.UpdateSpeedBoosterPalette(bus, submergedScrewCgram, new SpeedBoosterPaletteFrame { MovementType = SamusMovementType.SpinJumping, AnimationFrame = 26, EquippedItems = 0x0008, BottomBoundarySubmerged = true }),
        "submerged Power Suit suppresses active Screw Attack palette copy");
    AssertEqual(0x2222, submergedScrewCgram.Colors[192],
        "submerged Screw Attack retains the current suit colors");
    AssertEqual(0, submergedScrew.SpecialPaletteFrame,
        "submerged Screw Attack freezes palette-list offset");
    AssertTrue(submergedScrew.UpdateSpeedBoosterPalette(bus, submergedScrewCgram, new SpeedBoosterPaletteFrame { MovementType = SamusMovementType.SpinJumping, AnimationFrame = 26, EquippedItems = (SamusEquipmentFlags.GravitySuit | SamusEquipmentFlags.ScrewAttack).ToNativeWord(), BottomBoundarySubmerged = true }, suitColors: SuitColors(), cycleColors: CycleColors()),
        "submerged Gravity Suit bypasses Screw Attack palette suppression");
    AssertEqual(0x5b4b, submergedScrewCgram.Colors[192],
        "submerged Gravity Screw palette remains ROM-authored");

    SyntheticSamusCartridge.WriteSamusPaletteColors(
        bus, SamusPaletteRomData.Common.NormalSuitPalettePointer(4), 0x3a2a);
    AssertTrue(submergedScrew.UpdateSpeedBoosterPalette(bus, submergedScrewCgram, new SpeedBoosterPaletteFrame { MovementType = SamusMovementType.SpinJumping, AnimationFrame = 27, EquippedItems = (SamusEquipmentFlags.GravitySuit | SamusEquipmentFlags.ScrewAttack).ToNativeWord(), BottomBoundarySubmerged = true }, suitColors: SuitColors(), cycleColors: CycleColors()),
        "Gravity Screw frame 27 restores normal suit colors underwater");
    AssertEqual(0x3a2a, submergedScrewCgram.Colors[192],
        "late Gravity Screw uses the fixture's normal suit palette");
    // `$90:EEE7` samples post-movement positions only on game-time multiples of four and
    // alternates native word offsets zero/two. These become the two trailing bodies drawn
    // by `$90:87BD`; capture itself deliberately contains no interpolation.
    AssertTrue(!booster.CaptureSpeedEchoPosition(3, new(100, 80)), "speed echo skips non-fourth frame");
    AssertTrue(booster.CaptureSpeedEchoPosition(4, new(101, 81)), "speed echo captures slot zero");
    AssertEqual(2, booster.SpeedEchoIndex, "speed echo advances to slot one");
    AssertTrue(booster.CaptureSpeedEchoPosition(8, new(105, 82)), "speed echo captures slot one");
    AssertEqual(0, booster.SpeedEchoIndex, "speed echo alternation wraps after slot one");
    AssertEqual(101, booster.FirstSpeedEchoPosition.X, "first speed echo X snapshot");
    AssertEqual(105, booster.SecondSpeedEchoPosition.X, "second speed echo X snapshot");

    booster.CancelRunningMomentum(SamusFacingDirection.Right);
    AssertTrue(booster.NormalSuitPaletteRestoreRequested,
        "CancelSpeedBoost publishes normal-suit palette restoration");
    AssertEqual(0xffff, booster.SpeedEchoIndex,
        "right-facing cancellation enters high-bit echo departure");
    AssertEqual(8, booster.FirstSpeedEchoXSpeed,
        "right-facing first departure speed is positive eight");
    AssertEqual(8, booster.SecondSpeedEchoXSpeed,
        "right-facing second departure speed is positive eight");

    // `$90:87D3-$90:884B` advances slot one, then slot zero as an actual draw side effect.
    // Both stored bodies trail a rightward-running Samus, so +8 approaches X=120 while Y
    // independently approaches 90 by two. Crossing clears a slot before it can emit OAM.
    AssertTrue(booster.AdvanceDepartingSpeedEcho(1, new(120, 90)),
        "right departure slot one remains before crossing");
    AssertEqual(113, booster.SecondSpeedEchoPosition.X,
        "right departure slot one advances positive eight");
    AssertEqual(84, booster.SecondSpeedEchoPosition.Y,
        "right departure slot one approaches Samus Y by two");
    AssertTrue(booster.AdvanceDepartingSpeedEcho(0, new(120, 90)),
        "right departure slot zero remains before crossing");
    AssertEqual(109, booster.FirstSpeedEchoPosition.X,
        "right departure slot zero advances positive eight");
    AssertEqual(83, booster.FirstSpeedEchoPosition.Y,
        "right departure slot zero approaches Samus Y by two");
    booster.FinishDepartingSpeedEchoFrame();
    AssertEqual(0xffff, booster.SpeedEchoIndex,
        "departure index survives while either echo remains");

    // Samus_CancelSpeedBoost is called every applicable standing/turn frame. Its BMI guard
    // must preserve an already-running departure even if the new pose faces the other way.
    booster.CancelRunningMomentum(SamusFacingDirection.Left);
    AssertEqual(8, booster.FirstSpeedEchoXSpeed,
        "repeated cancellation cannot reverse an active departure");
    AssertTrue(!booster.AdvanceDepartingSpeedEcho(1, new(120, 90)),
        "right departure slot one clears on crossing");
    AssertEqual(0, booster.SecondSpeedEchoPosition.X,
        "crossed slot one publishes empty sentinel");
    AssertTrue(booster.AdvanceDepartingSpeedEcho(0, new(120, 90)),
        "right departure slot zero remains one more frame");
    booster.FinishDepartingSpeedEchoFrame();
    AssertEqual(0xffff, booster.SpeedEchoIndex,
        "one surviving departure keeps high-bit index");
    AssertTrue(!booster.AdvanceDepartingSpeedEcho(0, new(120, 90)),
        "right departure slot zero eventually crosses");
    booster.FinishDepartingSpeedEchoFrame();
    AssertEqual(0, booster.SpeedEchoIndex,
        "last crossed departure restores ordinary echo index");

    // Mirror the same signed-word comparison for a left-facing cancellation. This uses the
    // shared position-word publisher because those words really are overloaded by active
    // boost, departure, and shinespark-crash state in WRAM.
    var leftDeparture = new SamusHorizontalSpeedState();
    leftDeparture.SetShinesparkCrashEchoState(
        encodedIndex: 0,
        first: new(150, 90),
        second: new(154, 94));
    leftDeparture.CancelRunningMomentum(SamusFacingDirection.Left);
    AssertEqual(unchecked((ushort)-8), leftDeparture.FirstSpeedEchoXSpeed,
        "left-facing first departure speed is negative eight");
    AssertEqual(unchecked((ushort)-8), leftDeparture.SecondSpeedEchoXSpeed,
        "left-facing second departure speed is negative eight");
    AssertTrue(leftDeparture.AdvanceDepartingSpeedEcho(1, new(140, 100)),
        "left departure slot one remains before crossing");
    AssertEqual(146, leftDeparture.SecondSpeedEchoPosition.X,
        "left departure slot one advances negative eight");
    AssertEqual(96, leftDeparture.SecondSpeedEchoPosition.Y,
        "left departure slot one approaches Samus Y by two");
    AssertTrue(leftDeparture.AdvanceDepartingSpeedEcho(0, new(140, 100)),
        "left departure slot zero remains before crossing");
    AssertEqual(142, leftDeparture.FirstSpeedEchoPosition.X,
        "left departure slot zero advances negative eight");
    AssertTrue(!leftDeparture.AdvanceDepartingSpeedEcho(1, new(140, 100)),
        "left departure slot one clears after passing Samus");
    AssertTrue(!leftDeparture.AdvanceDepartingSpeedEcho(0, new(140, 100)),
        "left departure slot zero clears after passing Samus");
    leftDeparture.FinishDepartingSpeedEchoFrame();
    AssertEqual(0, leftDeparture.SpeedEchoIndex,
        "left departure clears shared index after both slots cross");

    AssertTrue(booster.UpdateSpeedBoosterPalette(bus, boostCgram, new SpeedBoosterPaletteFrame { MovementType = SamusMovementType.Standing, AnimationFrame = 0, EquippedItems = 0x2000 }, suitColors: SuitColors()),
        "cancel copies normal suit palette through runtime seam");
    AssertEqual(0x0321, boostCgram.Colors[192], "cancel restores ROM-authored Power Suit palette");
    AssertTrue(!booster.NormalSuitPaletteRestoreRequested, "normal-suit palette request is one-shot");

    var boostedJump = new SamusState { EquippedItems = 0x2000 };
    boostedJump.Kinematics.YSpeed = 4;
    boostedJump.Kinematics.YSubspeed = 0xe000;
    boostedJump.HorizontalSpeed.ExtraRunSpeed = 3;
    boostedJump.HorizontalSpeed.ExtraRunSubspeed = 0x4000;
    SamusAerialMovement.ApplyEquippedSpeedBoosterJumpBonus(boostedJump);
    AssertEqual(5, boostedJump.Kinematics.YSpeed, "boosted jump adds half whole extra speed");
    AssertEqual(0x2000, boostedJump.Kinematics.YSubspeed,
        "boosted jump wraps fractional addition without carrying");

    // $90:E4E6 caps a nonsensically large divisor at four. Extra run speed is added before
    // that shift; 2.0 + 2.0 therefore becomes 0.4000 when divisor $1234 is stored.
    speed.ExtraRunSpeed = 2;
    speed.ExtraRunSubspeed = 0;
    speed.SpeedDivisor = 0x1234;
    AssertEqual(0x00004000u, speed.CalculateTotalSpeed(0x00020000), "total speed divisor caps at four");
    AssertEqual(0x4000, speed.TotalSubspeed, "total subspeed publication");

    // The displacement clamp replaces only the signed whole word and preserves the low
    // fraction. These deliberately oversized values catch a tempting floating-point clamp.
    speed.ExtraRunSpeed = 0;
    speed.SpeedDivisor = 0;
    AssertEqual(0x000f1234, speed.CalculateRightDisplacement(0x00101234), "right displacement +15 clamp");
    // Negation happens before clamping, so 0 - $0010:1234 is $FFEF:EDCC. The clamp
    // replaces only its high word, producing $FFF1:EDCC rather than mirroring $1234.
    AssertEqual(unchecked((int)0xfff1edcc), speed.CalculateLeftDisplacement(0x00101234), "left displacement -15 clamp");

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
        (int)SamusPoseId.MorphBallFallingRightPose,
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

    var standing = new SamusState
    {
        Pose = SamusPoseId.FacingRightNormalPose,
        Position = FixedPosition<RoomPixels>.FromWhole(new(64, 64)),
    };
    standing.Kinematics.Radius = new(5, 21);
    standing.Kinematics.ExtraDisplacement = FixedVector<RoomPixels>.FromWords(1, 0x8000, 2, 0x4000);
    GroundedMovementResult positive = SamusGroundedMovement.StepStandingRight(
        bus,
        level,
        standing,
        nmiFrameCounter: 0,
        controllerInput: 0);
    AssertEqual(0x00018000, positive.Horizontal.AcceptedDisplacement,
        "standing external +X bypasses zero base speed");
    AssertEqual(0x00034000, positive.Vertical.AcceptedDisplacement,
        "positive no-speed external Y gains one whole pixel");
    AssertEqual(1, standing.Kinematics.ExtraDisplacement.X.WholePixels,
        "movement does not consume persistent external X producer word");
    AssertEqual(2, standing.Kinematics.ExtraDisplacement.Y.WholePixels,
        "movement does not consume persistent external Y producer word");

    var negative = new SamusState
    {
        Pose = SamusPoseId.FacingRightNormalPose,
        Position = FixedPosition<RoomPixels>.FromWhole(new(64, 64)),
    };
    negative.Kinematics.Radius = new(5, 21);
    negative.Kinematics.ExtraDisplacement = negative.Kinematics.ExtraDisplacement.WithWholeY(-2).WithFractionY(0x8000);
    GroundedMovementResult upward = SamusGroundedMovement.StepStandingRight(
        bus,
        level,
        negative,
        nmiFrameCounter: 1,
        controllerInput: 0);
    AssertEqual(unchecked((int)0xfffe8000), upward.Vertical.AcceptedDisplacement,
        "negative no-speed external Y has no downward bias");

    var transition = new SamusState
    {
        Pose = SamusPoseId.NeutralJumpTransitionRightPose,
        Position = FixedPosition<RoomPixels>.FromWhole(new(64, 64)),
    };
    transition.Kinematics.Radius = new(5, 19);
    transition.Kinematics.ExtraDisplacement = FixedVector<RoomPixels>.FromWords(0xffff, 0x8000, 0, 0x4000); // -0.8000 // +0.4000
    uint transitionXBefore = transition.Kinematics.Position.X.RawValue;
    uint transitionYBefore = transition.Kinematics.Position.Y.RawValue;
    AerialMovementResult transitionResult = SamusAerialMovement.StepNormalJump(
        bus,
        level,
        transition,
        (ushort)SnesButton.A,
        nmiFrameCounter: 0);
    AssertEqual(unchecked((int)0xffff8000), unchecked((int)(transition.Kinematics.Position.X.RawValue - transitionXBefore)),
        "jump transition applies signed external X with zero base speed");
    AssertEqual(0x00014000, unchecked((int)(transition.Kinematics.Position.Y.RawValue - transitionYBefore)),
        "jump transition applies extra-only Y with positive bias");

    var rising = new SamusState
    {
        Pose = SamusPoseId.NeutralJumpRightPose,
        Position = FixedPosition<RoomPixels>.FromWhole(new(64, 64)),
    };
    rising.Kinematics.Radius = new(5, 19);
    rising.Kinematics.YDirection = SamusVerticalDirection.Up;
    rising.Kinematics.YSpeed = 1;
    rising.Kinematics.YSubspeed = 0;
    rising.Kinematics.ExtraDisplacement = rising.Kinematics.ExtraDisplacement.WithWholeY(1).WithFractionY(0x8000);
    uint risingYBefore = rising.Kinematics.Position.Y.RawValue;
    AerialMovementResult reversed = SamusAerialMovement.StepNormalJump(
        bus,
        level,
        rising,
        (ushort)SnesButton.A,
        nmiFrameCounter: 0);
    AssertEqual(0x00008000, unchecked((int)(rising.Kinematics.Position.Y.RawValue - risingYBefore)),
        "gravity path adds external Y directly and may reverse actual direction");
    AssertEqual(SamusVerticalDirection.Up, rising.Kinematics.YDirection,
        "external reversal does not rewrite native velocity-direction word");

    var bouncingBall = new SamusState
    {
        Pose = SamusPoseId.MorphBallFallingRightPose,
        Position = FixedPosition<RoomPixels>.FromWhole(new(64, 64)),
        MorphBallBounceState = SamusBallBounceState.FirstRebound,
        KnockbackDirection = 0,
    };
    bouncingBall.Kinematics.Radius = new(5, 7);
    bouncingBall.Kinematics.YDirection = SamusVerticalDirection.Up;
    bouncingBall.Kinematics.YSpeed = 3;
    bouncingBall.Kinematics.YSubspeed = 0x4000;
    bouncingBall.Kinematics.ExtraDisplacement = bouncingBall.Kinematics.ExtraDisplacement.WithWholeY(-1).WithFractionY(0x8000); // -0.8000
    MorphBallMovementResult displacedBounce = SamusMorphBallMovement.StepFalling(
        bus,
        level,
        bouncingBall,
        controllerInput: 0,
        nmiFrameCounter: 0);
    AssertEqual(unchecked((int)0xffff8000), displacedBounce.Vertical.AcceptedDisplacement,
        "external Y replaces an active Morph Ball rebound rather than joining gravity");
    AssertEqual(0, bouncingBall.Kinematics.YSpeed,
        "external Morph Ball bounce override clears whole rebound speed");
    AssertEqual(0, bouncingBall.Kinematics.YSubspeed,
        "external Morph Ball bounce override clears fractional rebound speed");
    AssertEqual(SamusVerticalDirection.Down, bouncingBall.Kinematics.YDirection,
        "external Morph Ball bounce override forces native direction word two");
    AssertTrue(!displacedBounce.HitCeiling && !displacedBounce.Landed,
        "unobstructed signed-negative Morph Ball producer moves without a false collision");

    Console.WriteLine(
        "  Samus displacement: signed X/Y, grounded bias, transition motion, persistence, gravity reversal, and Morph Ball override agree.");
}

// Exercises `$91:F7B0/$91:DAC7/$90:CFFA-$D2B9` without relying on host elapsed time:
// stage-gated storage, ROM palette indirection, windup timeout, directional installation,
// 16.16 acceleration, energy drain, and the low-energy crash handoff.

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
