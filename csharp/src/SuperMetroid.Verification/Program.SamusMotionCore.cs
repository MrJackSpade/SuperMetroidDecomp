using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{

/// <summary>Pose-transition, horizontal-speed, and extra-displacement verification.</summary>
static void VerifySamusPoseTransitionMatching()
{
    var bus = new TestAddressSpace();

    // Pose $01 points at a compact synthetic table shaped like the real $91:A0EC data.
    // Jump+Up outranks plain Up, which outranks Right because the matcher stops at the
    // first record whose complete required masks are present.
    WriteTestWord(bus, 0x919ee4, 0xa0ec);
    bus.WriteBytes(0x91a0ec, [
        0x80, 0x00, 0x00, 0x08, 0x55, 0x00,
        0x00, 0x00, 0x00, 0x08, 0x03, 0x00,
        0x00, 0x00, 0x00, 0x01, 0x09, 0x00,
        0x00, 0x00, 0x00, 0x02, 0x01, 0x00,
        0xff, 0xff,
    ]);

    SamusPoseTransition jumpUp = SamusPoseTransitionTable.Find(
        bus,
        currentPose: 1,
        canonicalHeldInput: 0x0980,
        canonicalNewInput: 0x0080)!.Value;
    AssertEqual(0x55, jumpUp.ProspectivePose, "Samus transition required-new plus held chord");
    AssertEqual(0x91a0ec, jumpUp.EntryAddress, "Samus transition winning ROM record address");

    SamusPoseTransition up = SamusPoseTransitionTable.Find(
        bus,
        currentPose: 1,
        canonicalHeldInput: 0x0900,
        canonicalNewInput: 0)!.Value;
    AssertEqual(0x03, up.ProspectivePose, "Samus transition permits extra held direction");

    SamusPoseTransition right = SamusPoseTransitionTable.Find(
        bus,
        currentPose: 1,
        canonicalHeldInput: 0x0100,
        canonicalNewInput: 0)!.Value;
    AssertEqual(0x09, right.ProspectivePose, "Samus standing Right proposes running pose");

    AssertEqual<SamusPoseTransition?>(null,
        SamusPoseTransitionTable.Find(bus, 1, canonicalHeldInput: 0, canonicalNewInput: 0),
        "Samus zero input bypasses transition table");
    AssertEqual<SamusPoseTransition?>(null,
        SamusPoseTransitionTable.Find(bus, 1, canonicalHeldInput: 0x0200, canonicalNewInput: 0),
        "Samus same-pose transition is suppressed");
    AssertEqual<SamusPoseTransition?>(null,
        SamusPoseTransitionTable.Find(bus, 1, canonicalHeldInput: 0x0400, canonicalNewInput: 0),
        "Samus transition terminator returns no match");

    SamusPoseTransitionLookup zeroInput = SamusPoseTransitionTable.Lookup(
        bus, 1, canonicalHeldInput: 0, canonicalNewInput: 0);
    SamusPoseTransitionLookup samePose = SamusPoseTransitionTable.Lookup(
        bus, 1, canonicalHeldInput: 0x0200, canonicalNewInput: 0);
    SamusPoseTransitionLookup terminator = SamusPoseTransitionTable.Lookup(
        bus, 1, canonicalHeldInput: 0x0400, canonicalNewInput: 0);
    AssertTrue(zeroInput.UsesPoseDefinitionFallback,
        "Samus zero input enters pose-definition fallback");
    AssertTrue(!samePose.UsesPoseDefinitionFallback,
        "Samus same-pose record returns without fallback");
    AssertTrue(terminator.UsesPoseDefinitionFallback,
        "Samus unmatched nonzero input enters pose-definition fallback");

    Console.WriteLine(
        "  Samus input: ROM transition masks, priority, same-pose return, and fallback agree.");
}

/// <summary>
/// Exercises the exact normal-air pointer chain and word-oriented arithmetic from
/// <c>$94:97D0</c>, <c>$90:9BD1</c>, <c>$90:9A7E</c>, and <c>$90:E4E6-$90:E4E5</c>.
/// Synthetic bus bytes are the retail running entry at $90:9F61, so these assertions stay
/// deterministic while the DebugRunner independently reads the user's cartridge.
/// </summary>
static void VerifySamusHorizontalSpeed()
{
    var bus = new TestAddressSpace();

    // Normal air stores $9F55. Running movement type one advances by one more 12-byte
    // record to $9F61: acceleration 0.3000, maximum 2.C000, deceleration 0.8000.
    bus.WriteBytes(0x909f61, [
        0x00, 0x00, 0x00, 0x30,
        0x02, 0x00, 0x00, 0xc0,
        0x00, 0x00, 0x00, 0x80,
    ]);

    var speed = new SamusHorizontalSpeedState();
    speed.SelectNormalAirSpeedTable();
    AssertEqual(0x909f61, speed.ResolveEntryAddress(movementType: 1), "running speed entry pointer chain");

    SpeedTableEntry entry = speed.ReadEntry(bus, movementType: 1);
    AssertEqual(0x0000, entry.Acceleration, "running acceleration whole word");
    AssertEqual(0x3000, entry.AccelerationSubspeed, "running acceleration fraction");
    AssertEqual(0x0002, entry.MaximumSpeed, "running maximum whole word");
    AssertEqual(0xc000, entry.MaximumSubspeed, "running maximum fraction");
    AssertEqual(0x8000, entry.DecelerationSubspeed, "running deceleration fraction");

    // Four acceleration calls produce 0.C000 exactly. Fifteen calls would reach 2.D000,
    // so the native quirked comparison clamps that result down to the table's 2.C000 cap.
    StepFrames(4, _ => speed.CalculateBaseSpeed(bus, movementType: 1));
    AssertEqual(0x0000c000u, speed.BaseFixed, "running acceleration after four calls");
    StepFrames(11, _ => speed.CalculateBaseSpeed(bus, movementType: 1));
    AssertEqual(0x0002c000u, speed.BaseFixed, "running acceleration clamps at 2.C000");

    // Deceleration subtracts 0.8000 per call. Crossing below zero makes the signed high
    // word negative, clearing both halves and restoring acceleration mode zero.
    speed.AccelerationMode = 2;
    StepFrames(6, _ => speed.CalculateBaseSpeed(bus, movementType: 1));
    AssertEqual(0u, speed.BaseFixed, "running deceleration underflow clears speed");
    AssertEqual(0, speed.AccelerationMode, "running deceleration restores acceleration mode");

    // `$90:973E` adds the literal no-booster 0.1000 pair once per running+B frame. The
    // native cap test runs before addition, so call 32 reaches exactly 2.0000 and call 33
    // performs the visible clamp write without changing the pair.
    StepFrames(32, _ =>
        speed.HandleExtraRunSpeed(
            movementType: 1,
            controllerInput: (ushort)SnesButton.B,
            speedBoosterEquipped: false));
    AssertTrue(speed.HasRunningMomentum, "ordinary Dash establishes native momentum flag");
    AssertEqual(0, speed.SpeedBoostCounter, "ordinary Dash leaves booster stage zero");
    AssertEqual(2, speed.ExtraRunSpeed, "ordinary Dash whole-speed cap");
    AssertEqual(0, speed.ExtraRunSubspeed, "ordinary Dash fractional-speed cap");
    speed.HandleExtraRunSpeed(1, (ushort)SnesButton.B, speedBoosterEquipped: false);
    AssertEqual(2, speed.ExtraRunSpeed, "ordinary Dash remains clamped on next call");

    // B release and an airborne movement type both take `$90:9808`; a set momentum flag
    // bypasses the numeric clear. Only the separately invoked cancel routine clears the
    // flag/counter, after which another non-running call clears the retained pair.
    speed.HandleExtraRunSpeed(1, controllerInput: 0, speedBoosterEquipped: false);
    speed.HandleExtraRunSpeed(3, controllerInput: 0, speedBoosterEquipped: false);
    AssertEqual(2, speed.ExtraRunSpeed, "Dash release and spin jump retain extra speed");
    speed.CancelRunningMomentum(poseXDirection: 8);
    AssertTrue(!speed.HasRunningMomentum, "CancelSpeedBoost clears ordinary momentum flag");
    speed.HandleExtraRunSpeed(3, controllerInput: 0, speedBoosterEquipped: false);
    AssertEqual(0, speed.ExtraRunSpeed, "post-cancel airborne handler clears extra speed");

    // The animation side reads its ordinary-Dash cadence through the live pointer at
    // `$91:B5D1`. The pose-specific stream deliberately uses different delays so these
    // checks would fail if the implementation merely sped up a host timer by coincidence.
    bus.WriteBytes(0x91b671, [0x08, 0x01, 0xff, 0x02, 0x00, 0x00, 0x15, 0x00]); // pose $09
    WriteTestWord(bus, 0x91b022, 0xc000); // pose $09's normal delay stream
    WriteTestWord(bus, 0x91b5d1, 0xc100); // shared ordinary-Dash delay stream pointer
    bus.WriteBytes(0x91c000, [0x09, 0x09, 0xff]);
    bus.WriteBytes(0x91c100, [0x02, 0x03, 0xff]);
    var dashAnimation = new SamusState { Pose = SamusPoseIds.MovingRightNormalPose };
    dashAnimation.InitializeAnimation(bus);
    dashAnimation.HorizontalSpeed.HandleExtraRunSpeed(
        movementType: 1,
        controllerInput: (ushort)SnesButton.B,
        speedBoosterEquipped: false);
    for (int tick = 0; tick < 9; tick++)
        dashAnimation.AnimateNoFx(bus, (ushort)SnesButton.B);
    AssertEqual(1, dashAnimation.AnimationFrame, "Dash advances into running frame one");
    AssertEqual(3, dashAnimation.AnimationFrameTimer, "Dash selects shared frame-one delay");
    for (int tick = 0; tick < 3; tick++)
        dashAnimation.AnimateNoFx(bus, (ushort)SnesButton.B);
    AssertEqual(0, dashAnimation.AnimationFrame, "Dash command interception restarts frame zero");
    AssertEqual(2, dashAnimation.AnimationFrameTimer, "Dash restart uses shared frame-zero delay");

    // `$91:B61F` supplies each stage's command-loop countdown, while `$91:B5DE` supplies
    // the corresponding animation stream. Distinct synthetic values prove both lookups
    // remain ROM-backed and that stage four publishes the echo/contact-damage events.
    for (int stage = 0; stage <= 4; stage++)
    {
        WriteTestWord(bus, 0x91b61f + stage * 2, (ushort)(stage == 0 ? 3 : 2));
        WriteTestWord(bus, 0x91b5de + stage * 2, (ushort)(0xc200 + stage * 0x10));
        bus.WriteBytes(0x91c200 + stage * 0x10, [(byte)(3 + stage), 0xff]);
    }

    var booster = new SamusHorizontalSpeedState();
    booster.HandleExtraRunSpeed(
        movementType: 1,
        controllerInput: (ushort)SnesButton.B,
        speedBoosterEquipped: true,
        bus);
    AssertTrue(booster.HasRunningMomentum, "Speed Booster establishes momentum");
    AssertEqual(3, booster.SpeedBoostCounter, "Speed Booster seeds stage-zero countdown from ROM");
    AssertEqual(1, booster.SpecialPaletteTimer, "Speed Booster seeds special-palette timer");

    // Hexadecimal `.1000` is one sixteenth, so 112 movement calls reach 7.0000 exactly.
    for (int frame = 1; frame < 112; frame++)
    {
        booster.HandleExtraRunSpeed(
            movementType: 1,
            controllerInput: (ushort)SnesButton.B,
            speedBoosterEquipped: true,
            bus);
    }
    AssertEqual(7, booster.ExtraRunSpeed, "Speed Booster reaches exact 7.0000 cap");
    AssertEqual(0, booster.ExtraRunSubspeed, "Speed Booster cap has zero fraction");

    ushort boostFrame = 1;
    for (int command = 0; command < 9; command++)
    {
        bool intercepted = booster.TryAdvanceSpeedBoosterAnimationStage(
            bus,
            movementType: 1,
            controllerInput: (ushort)SnesButton.B,
            animationFrameBuffer: 0,
            ref boostFrame,
            out ushort boostTimer);
        if (command == 2)
        {
            AssertTrue(intercepted, "third stage-zero command advances Speed Booster");
            AssertEqual(0, boostFrame, "Speed Booster stage change restarts animation");
            AssertEqual(4, boostTimer, "stage-one delay comes from ROM-selected stream");
        }
    }
    AssertEqual(0x0402, booster.SpeedBoostCounter, "Speed Booster reaches stage four countdown");
    AssertTrue(booster.EchoSoundRequested, "stage four publishes speed-echo sound event");
    AssertTrue(booster.ConsumeEchoSoundRequest(),
        "frontend can consume the speed-echo publication exactly once");
    AssertTrue(!booster.ConsumeEchoSoundRequest(),
        "consumed speed-echo publication cannot replay on a later frame");
    AssertEqual(1, booster.ContactDamageIndex, "stage four enables contact damage");

    // `$91:DAA9` is a pointer to the active suit's four-entry palette-pointer list, not a
    // direct bank-$9B palette address. Distinct first/second colors prove both levels of
    // indirection, the one-then-four frame timer, and the pinned frame-six progression.
    WriteTestWord(bus, 0x91daa9, 0xd100); // Power Suit speed-palette list in bank $91.
    WriteTestWord(bus, 0x91d100, 0xe000);
    WriteTestWord(bus, 0x91d102, 0xe020);
    WriteTestWord(bus, 0x91d104, 0xe040);
    WriteTestWord(bus, 0x91d106, 0xe060);
    WriteTestWord(bus, 0x9be000, 0x1234);
    WriteTestWord(bus, 0x9be020, 0x4567);
    WriteTestWord(bus, 0x91d727, 0x9400); // Normal Power Suit palette for cancellation.
    WriteTestWord(bus, 0x9b9400, 0x0321);

    var boostCgram = new SnesCgram();
    AssertTrue(booster.UpdateSpeedBoosterPalette(
        bus, boostCgram, movementType: 1, animationFrame: 0, equippedItems: 0x2000),
        "stage-four palette timer one copies immediately");
    AssertEqual(0x1234, boostCgram.Colors[192], "first Speed Booster palette comes from bank $9B");
    AssertEqual(2, booster.SpecialPaletteFrame, "Speed Booster palette advances to pointer offset two");
    AssertEqual(4, booster.SpecialPaletteTimer, "Speed Booster palette reloads four-frame timer");
    for (int paletteTick = 0; paletteTick < 3; paletteTick++)
    {
        AssertTrue(!booster.UpdateSpeedBoosterPalette(
            bus, boostCgram, movementType: 1, animationFrame: 0, equippedItems: 0x2000),
            "Speed Booster palette waits during positive timer");
    }
    AssertTrue(booster.UpdateSpeedBoosterPalette(
        bus, boostCgram, movementType: 1, animationFrame: 0, equippedItems: 0x2000),
        "fourth Speed Booster palette tick copies next frame");
    AssertEqual(0x4567, boostCgram.Colors[192], "second Speed Booster palette pointer");

    // `$91:D9B2-$D9D8` performs its bottom-boundary liquid gate before either the Screw
    // Attack or active Speed Booster branches. A submerged Power/Varia body returns with
    // carry set: neither CGRAM nor the shared timer/index words may move. Gravity Suit's
    // palette-index bit two bypasses that exact gate and must still follow the ROM pointer.
    WriteTestWord(bus, 0x91daad, 0xd120); // Gravity Suit speed-palette pointer list.
    WriteTestWord(bus, 0x91d120, 0xe080);
    WriteTestWord(bus, 0x9be080, 0x6a5a);
    var submergedBoost = new SamusHorizontalSpeedState { SpeedBoostCounter = 0x0401 };
    var submergedBoostCgram = new SnesCgram();
    WriteTestWord(bus, 0x9bf000, 0x7777);
    submergedBoostCgram.LoadFromBus(bus, 0x9bf000, colorCount: 1, destinationIndex: 192);
    AssertTrue(!submergedBoost.UpdateSpeedBoosterPalette(
        bus,
        submergedBoostCgram,
        movementType: 1,
        animationFrame: 0,
        equippedItems: 0x2000,
        bottomBoundarySubmerged: true),
        "submerged Power Suit suppresses active Speed Booster palette copy");
    AssertTrue(!submergedBoost.UpdateSpeedBoosterPalette(
        bus,
        submergedBoostCgram,
        movementType: 1,
        animationFrame: 0,
        equippedItems: 0x2001,
        bottomBoundarySubmerged: true),
        "submerged Varia Suit suppresses active Speed Booster palette copy");
    AssertEqual(0x7777, submergedBoostCgram.Colors[192],
        "submerged Speed Booster leaves live Samus palette untouched");
    AssertEqual(0, submergedBoost.SpecialPaletteTimer,
        "submerged Speed Booster freezes common palette timer");
    AssertEqual(0, submergedBoost.SpecialPaletteFrame,
        "submerged Speed Booster freezes palette-list offset");
    AssertTrue(submergedBoost.UpdateSpeedBoosterPalette(
        bus,
        submergedBoostCgram,
        movementType: 1,
        animationFrame: 0,
        equippedItems: (SamusEquipmentFlags.GravitySuit | SamusEquipmentFlags.SpeedBooster).ToNativeWord(),
        bottomBoundarySubmerged: true),
        "submerged Gravity Suit bypasses Speed Booster palette suppression");
    AssertEqual(0x6a5a, submergedBoostCgram.Colors[192],
        "submerged Gravity Suit Speed Booster palette remains ROM-authored");

    // The same early return surrounds Screw Attack. Exercise it independently so a future
    // refactor cannot fix running boost while accidentally leaving the spin palette active.
    WriteTestWord(bus, 0x91da4e, 0xd140); // Gravity Suit Screw-palette pointer list.
    WriteTestWord(bus, 0x91d140, 0xe0a0);
    WriteTestWord(bus, 0x9be0a0, 0x5b4b);
    var submergedScrew = new SamusHorizontalSpeedState();
    var submergedScrewCgram = new SnesCgram();
    WriteTestWord(bus, 0x9bf020, 0x2222);
    submergedScrewCgram.LoadFromBus(bus, 0x9bf020, colorCount: 1, destinationIndex: 192);
    AssertTrue(!submergedScrew.UpdateSpeedBoosterPalette(
        bus,
        submergedScrewCgram,
        movementType: 3,
        animationFrame: 27,
        equippedItems: 0x0008,
        bottomBoundarySubmerged: true),
        "submerged Power Suit suppresses late Screw Attack palette copy");
    AssertEqual(0x2222, submergedScrewCgram.Colors[192],
        "submerged Screw Attack retains the current suit colors");
    AssertEqual(0, submergedScrew.SpecialPaletteFrame,
        "submerged Screw Attack freezes palette-list offset");
    AssertTrue(submergedScrew.UpdateSpeedBoosterPalette(
        bus,
        submergedScrewCgram,
        movementType: 3,
        animationFrame: 27,
        equippedItems: (SamusEquipmentFlags.GravitySuit | SamusEquipmentFlags.ScrewAttack).ToNativeWord(),
        bottomBoundarySubmerged: true),
        "submerged Gravity Suit bypasses Screw Attack palette suppression");
    AssertEqual(0x5b4b, submergedScrewCgram.Colors[192],
        "submerged Gravity Screw palette remains ROM-authored");

    // `$90:EEE7` samples post-movement positions only on game-time multiples of four and
    // alternates native word offsets zero/two. These become the two trailing bodies drawn
    // by `$90:87BD`; capture itself deliberately contains no interpolation.
    AssertTrue(!booster.CaptureSpeedEchoPosition(3, 100, 80), "speed echo skips non-fourth frame");
    AssertTrue(booster.CaptureSpeedEchoPosition(4, 101, 81), "speed echo captures slot zero");
    AssertEqual(2, booster.SpeedEchoIndex, "speed echo advances to slot one");
    AssertTrue(booster.CaptureSpeedEchoPosition(8, 105, 82), "speed echo captures slot one");
    AssertEqual(0, booster.SpeedEchoIndex, "speed echo alternation wraps after slot one");
    AssertEqual(101, booster.FirstSpeedEchoXPosition, "first speed echo X snapshot");
    AssertEqual(105, booster.SecondSpeedEchoXPosition, "second speed echo X snapshot");

    booster.CancelRunningMomentum(poseXDirection: 8);
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
    AssertTrue(booster.AdvanceDepartingSpeedEcho(1, 120, 90),
        "right departure slot one remains before crossing");
    AssertEqual(113, booster.SecondSpeedEchoXPosition,
        "right departure slot one advances positive eight");
    AssertEqual(84, booster.SecondSpeedEchoYPosition,
        "right departure slot one approaches Samus Y by two");
    AssertTrue(booster.AdvanceDepartingSpeedEcho(0, 120, 90),
        "right departure slot zero remains before crossing");
    AssertEqual(109, booster.FirstSpeedEchoXPosition,
        "right departure slot zero advances positive eight");
    AssertEqual(83, booster.FirstSpeedEchoYPosition,
        "right departure slot zero approaches Samus Y by two");
    booster.FinishDepartingSpeedEchoFrame();
    AssertEqual(0xffff, booster.SpeedEchoIndex,
        "departure index survives while either echo remains");

    // Samus_CancelSpeedBoost is called every applicable standing/turn frame. Its BMI guard
    // must preserve an already-running departure even if the new pose faces the other way.
    booster.CancelRunningMomentum(poseXDirection: 4);
    AssertEqual(8, booster.FirstSpeedEchoXSpeed,
        "repeated cancellation cannot reverse an active departure");
    AssertTrue(!booster.AdvanceDepartingSpeedEcho(1, 120, 90),
        "right departure slot one clears on crossing");
    AssertEqual(0, booster.SecondSpeedEchoXPosition,
        "crossed slot one publishes empty sentinel");
    AssertTrue(booster.AdvanceDepartingSpeedEcho(0, 120, 90),
        "right departure slot zero remains one more frame");
    booster.FinishDepartingSpeedEchoFrame();
    AssertEqual(0xffff, booster.SpeedEchoIndex,
        "one surviving departure keeps high-bit index");
    AssertTrue(!booster.AdvanceDepartingSpeedEcho(0, 120, 90),
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
        firstX: 150,
        secondX: 154,
        firstY: 90,
        secondY: 94);
    leftDeparture.CancelRunningMomentum(poseXDirection: 4);
    AssertEqual(unchecked((ushort)-8), leftDeparture.FirstSpeedEchoXSpeed,
        "left-facing first departure speed is negative eight");
    AssertEqual(unchecked((ushort)-8), leftDeparture.SecondSpeedEchoXSpeed,
        "left-facing second departure speed is negative eight");
    AssertTrue(leftDeparture.AdvanceDepartingSpeedEcho(1, 140, 100),
        "left departure slot one remains before crossing");
    AssertEqual(146, leftDeparture.SecondSpeedEchoXPosition,
        "left departure slot one advances negative eight");
    AssertEqual(96, leftDeparture.SecondSpeedEchoYPosition,
        "left departure slot one approaches Samus Y by two");
    AssertTrue(leftDeparture.AdvanceDepartingSpeedEcho(0, 140, 100),
        "left departure slot zero remains before crossing");
    AssertEqual(142, leftDeparture.FirstSpeedEchoXPosition,
        "left departure slot zero advances negative eight");
    AssertTrue(!leftDeparture.AdvanceDepartingSpeedEcho(1, 140, 100),
        "left departure slot one clears after passing Samus");
    AssertTrue(!leftDeparture.AdvanceDepartingSpeedEcho(0, 140, 100),
        "left departure slot zero clears after passing Samus");
    leftDeparture.FinishDepartingSpeedEchoFrame();
    AssertEqual(0, leftDeparture.SpeedEchoIndex,
        "left departure clears shared index after both slots cross");

    AssertTrue(booster.UpdateSpeedBoosterPalette(
        bus, boostCgram, movementType: 0, animationFrame: 0, equippedItems: 0x2000),
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
static void VerifySamusExtraDisplacement()
{
    var bus = new TestAddressSpace();

    // Literal right-standing and normal-jump records. Only direction/movement/radius are
    // needed here, but using the retail bytes ensures production follows its normal metadata
    // path instead of a test-only pose shortcut.
    bus.WriteBytes(0x91b631, [0x08, 0x00, 0xff, 0x02, 0x06, 0x00, 0x15, 0x00]); // $01
    WritePoseDefinition(
        bus,
        SamusPoseIds.MorphBallFallingRightPose,
        [0x08, 0x08, 0xff, 0xff, 0x00, 0x00, 0x07, 0x00]); // $31
    bus.WriteBytes(0x91b881, [0x08, 0x02, 0xff, 0x02, 0x03, 0x00, 0x13, 0x00]); // $4B
    bus.WriteBytes(0x91b891, [0x08, 0x02, 0xff, 0x02, 0x08, 0x00, 0x13, 0x00]); // $4D

    // Type-two normal-air X physics is deliberately zero. That isolates extra displacement
    // while still exercising the real pointer selection and no-input base-speed clear.
    for (int offset = 0; offset < 12; offset += 2)
        WriteTestWord(bus, 0x909f6d + offset, 0);

    // Falling Morph Ball uses the type-eight record. Keeping it at exactly zero prevents
    // ordinary rolling physics from obscuring the external-producer bounce override below.
    for (int offset = 0; offset < 12; offset += 2)
        WriteTestWord(bus, 0x909f55 + 8 * 12 + offset, 0);

    const int width = 16;
    const int height = 16;
    RoomLevelData level = CreateEmptyRoom(width, height);

    var standing = new SamusState
    {
        Pose = SamusPoseIds.FacingRightNormalPose,
        XPosition = 64,
        YPosition = 64,
    };
    standing.Kinematics.XRadius = 5;
    standing.Kinematics.YRadius = 21;
    standing.Kinematics.ExtraXDisplacement = 1;
    standing.Kinematics.ExtraXSubdisplacement = 0x8000;
    standing.Kinematics.ExtraYDisplacement = 2;
    standing.Kinematics.ExtraYSubdisplacement = 0x4000;
    GroundedMovementResult positive = SamusGroundedMovement.StepStandingRight(
        bus,
        level,
        standing,
        nmiFrameCounter: 0);
    AssertEqual(0x00018000, positive.Horizontal.AcceptedDisplacement,
        "standing external +X bypasses zero base speed");
    AssertEqual(0x00034000, positive.Vertical.AcceptedDisplacement,
        "positive no-speed external Y gains one whole pixel");
    AssertEqual(1, standing.Kinematics.ExtraXDisplacement,
        "movement does not consume persistent external X producer word");
    AssertEqual(2, standing.Kinematics.ExtraYDisplacement,
        "movement does not consume persistent external Y producer word");

    var negative = new SamusState
    {
        Pose = SamusPoseIds.FacingRightNormalPose,
        XPosition = 64,
        YPosition = 64,
    };
    negative.Kinematics.XRadius = 5;
    negative.Kinematics.YRadius = 21;
    negative.Kinematics.ExtraYDisplacement = 0xfffe;
    negative.Kinematics.ExtraYSubdisplacement = 0x8000;
    GroundedMovementResult upward = SamusGroundedMovement.StepStandingRight(
        bus,
        level,
        negative,
        nmiFrameCounter: 1);
    AssertEqual(unchecked((int)0xfffe8000), upward.Vertical.AcceptedDisplacement,
        "negative no-speed external Y has no downward bias");

    var transition = new SamusState
    {
        Pose = SamusPoseIds.NeutralJumpTransitionRightPose,
        XPosition = 64,
        YPosition = 64,
    };
    transition.Kinematics.XRadius = 5;
    transition.Kinematics.YRadius = 19;
    transition.Kinematics.ExtraXDisplacement = 0xffff;
    transition.Kinematics.ExtraXSubdisplacement = 0x8000; // -0.8000
    transition.Kinematics.ExtraYDisplacement = 0;
    transition.Kinematics.ExtraYSubdisplacement = 0x4000; // +0.4000
    AerialMovementResult transitionResult = SamusAerialMovement.StepNormalJump(
        bus,
        level,
        transition,
        (ushort)SnesButton.A,
        nmiFrameCounter: 0);
    AssertEqual(unchecked((int)0xffff8000), transitionResult.Horizontal.AcceptedDisplacement,
        "jump transition applies signed external X with zero base speed");
    AssertEqual(0x00014000, transitionResult.Vertical!.Value.AcceptedDisplacement,
        "jump transition applies extra-only Y with positive bias");

    var rising = new SamusState
    {
        Pose = SamusPoseIds.NeutralJumpRightPose,
        XPosition = 64,
        YPosition = 64,
    };
    rising.Kinematics.XRadius = 5;
    rising.Kinematics.YRadius = 19;
    rising.Kinematics.YDirection = 1;
    rising.Kinematics.YSpeed = 1;
    rising.Kinematics.YSubspeed = 0;
    rising.Kinematics.ExtraYDisplacement = 1;
    rising.Kinematics.ExtraYSubdisplacement = 0x8000;
    AerialMovementResult reversed = SamusAerialMovement.StepNormalJump(
        bus,
        level,
        rising,
        (ushort)SnesButton.A,
        nmiFrameCounter: 0);
    AssertEqual(0x00008000, reversed.Vertical!.Value.AcceptedDisplacement,
        "gravity path adds external Y directly and may reverse actual direction");
    AssertEqual(1, rising.Kinematics.YDirection,
        "external reversal does not rewrite native velocity-direction word");

    var bouncingBall = new SamusState
    {
        Pose = SamusPoseIds.MorphBallFallingRightPose,
        XPosition = 64,
        YPosition = 64,
        MorphBallBounceState = 1,
        KnockbackDirection = 0,
    };
    bouncingBall.Kinematics.XRadius = 5;
    bouncingBall.Kinematics.YRadius = 7;
    bouncingBall.Kinematics.YDirection = 1;
    bouncingBall.Kinematics.YSpeed = 3;
    bouncingBall.Kinematics.YSubspeed = 0x4000;
    bouncingBall.Kinematics.ExtraYDisplacement = 0xffff;
    bouncingBall.Kinematics.ExtraYSubdisplacement = 0x8000; // -0.8000
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
    AssertEqual(2, bouncingBall.Kinematics.YDirection,
        "external Morph Ball bounce override forces native direction word two");
    AssertTrue(!displacedBounce.HitCeiling && !displacedBounce.Landed,
        "unobstructed signed-negative Morph Ball producer moves without a false collision");

    Console.WriteLine(
        "  Samus displacement: signed X/Y, grounded bias, transition motion, persistence, gravity reversal, and Morph Ball override agree.");
}

/// <summary>
/// Exercises `$91:F7B0/$91:DAC7/$90:CFFA-$D2B9` without relying on host elapsed time:
/// stage-gated storage, ROM palette indirection, windup timeout, directional installation,
/// 16.16 acceleration, energy drain, and the low-energy crash handoff.
/// </summary>
}
