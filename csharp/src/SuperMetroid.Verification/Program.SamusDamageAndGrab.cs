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

/// <summary>Drained Samus, solid-enemy collision, and Draygon-grab verification.</summary>
static void VerifySamusDrainedController()
{
    var bus = new TestAddressSpace();

    // These are the literal direction/type/graphics-offset/radius fields for the source,
    // four drained records, and their eventual ordinary-standing destinations.
    WritePoseDefinition(bus, SamusState.CrouchingRightPose,
        [0x08, 0x05, 0xff, 0x02, 0x03, 0x00, 0x10, 0x00]);
    WritePoseDefinition(bus, SamusState.CrouchingLeftPose,
        [0x04, 0x05, 0xff, 0x07, 0x03, 0x00, 0x10, 0x00]);
    WritePoseDefinition(bus, SamusState.KnockbackLeftPose,
        [0x04, 0x0a, 0xff, 0xff, 0x06, 0x00, 0x15, 0x00]);
    WritePoseDefinition(bus, SamusState.DrainedCrouchingRightPose,
        [0x08, 0x1b, 0xff, 0xff, 0xfc, 0x00, 0x15, 0x00]);
    WritePoseDefinition(bus, SamusState.DrainedCrouchingLeftPose,
        [0x04, 0x1b, 0xff, 0xff, 0xfc, 0x00, 0x15, 0x00]);
    WritePoseDefinition(bus, SamusState.DrainedStandingRightPose,
        [0x08, 0x1b, 0xff, 0xff, 0xfc, 0x00, 0x15, 0x00]);
    WritePoseDefinition(bus, SamusState.DrainedStandingLeftPose,
        [0x04, 0x1b, 0xff, 0xff, 0xfc, 0x00, 0x15, 0x00]);
    WritePoseDefinition(bus, SamusState.FacingRightNormalPose,
        [0x08, 0x00, 0xff, 0x02, 0x06, 0x00, 0x15, 0x00]);
    WritePoseDefinition(bus, SamusState.FacingLeftNormalPose,
        [0x04, 0x00, 0xff, 0x07, 0x06, 0x00, 0x15, 0x00]);

    WriteTestWord(bus, 0x91b010 + SamusState.CrouchingRightPose * 2, 0xc000);
    WriteTestWord(bus, 0x91b010 + SamusState.CrouchingLeftPose * 2, 0xc001);
    WriteTestWord(bus, 0x91b010 + SamusState.KnockbackLeftPose * 2, 0xc020);
    WriteTestWord(bus, 0x91b010 + SamusState.DrainedCrouchingRightPose * 2, 0xb257);
    WriteTestWord(bus, 0x91b010 + SamusState.DrainedCrouchingLeftPose * 2, 0xb268);
    WriteTestWord(bus, 0x91b010 + SamusState.DrainedStandingRightPose * 2, 0xb288);
    WriteTestWord(bus, 0x91b010 + SamusState.DrainedStandingLeftPose * 2, 0xb290);
    WriteTestWord(bus, 0x91b010 + SamusState.FacingRightNormalPose * 2, 0xc010);
    WriteTestWord(bus, 0x91b010 + SamusState.FacingLeftNormalPose * 2, 0xc011);
    bus.WriteBytes(0x91c000, [0x05, 0x05]);
    bus.WriteBytes(0x91c010, [0x05, 0x05]);
    bus.WriteBytes(0x91c020, [0x01, 0xfe, 0x01]);

    // Copy the byte-oriented streams verbatim. Controller-written frame values are byte
    // indices, so command operands remain part of the index space by design.
    bus.WriteBytes(0x91b257, [
        0x02, 0x02, 0x02, 0x10, 0xf7,
        0x01, 0xfe, 0x01,
        0x10, 0x10, 0x10, 0x10, 0xfe, 0x04,
        0x03, 0xfd, 0x01,
    ]);
    bus.WriteBytes(0x91b268, [
        0x02, 0x02, 0x10, 0xf7,
        0x01, 0xfe, 0x01,
        0x08, 0x10, 0x10, 0x10, 0x10, 0xfe, 0x04,
        0x03, 0x03, 0x03, 0xfd, 0x02,
        0x10, 0x10, 0x10, 0x10, 0xfe, 0x0e,
        0x10, 0xfe, 0x11,
        0x10, 0xfe, 0x01,
    ]);
    bus.WriteBytes(0x91b288, [0x10, 0x10, 0x10, 0x10, 0xff, 0x03, 0xfd, 0x01]);
    bus.WriteBytes(0x91b290, [0x10, 0x10, 0x10, 0x10, 0xff, 0x03, 0xfd, 0x02]);

    // `$91:D99E` points at ten complete Hyper Beam palettes in reverse-numbered order.
    // Distinct synthetic words expose both the pointer index and all-$20-byte copy. The
    // Power Suit entry at `$91:D727` independently verifies controller-zero/command-$17
    // restoration instead of allowing the last rainbow palette to remain accidentally.
    for (ushort palette = 0; palette < 10; palette++)
    {
        ushort pointer = unchecked((ushort)(0xa000 + palette * 0x20));
        WriteTestWord(bus, 0x91d99e + palette * 2, pointer);
        for (ushort color = 0; color < 16; color++)
        {
            WriteTestWord(
                bus,
                0x9b0000 | (pointer + color * 2),
                unchecked((ushort)(0x1000 + palette * 0x20 + color)));
        }
    }
    WriteTestWord(bus, 0x91d727, 0xb800);
    for (ushort color = 0; color < 16; color++)
        WriteTestWord(bus, 0x9bb800 + color * 2, unchecked((ushort)(0x3000 + color)));

    // `$8D:E1F0` points at the compact Hyper Beam projectile-palette program. Each of its
    // ten records lasts two handler calls and writes CGRAM `$E1-$E8`; synthetic colors make
    // the record number, color number, and exact loop boundary independently observable.
    WriteTestWord(bus, 0x8de1f0, 0xc685);
    WriteTestWord(bus, 0x8de1f2, 0xd900);
    WriteTestWord(bus, 0x8dd900, 0xc655);
    WriteTestWord(bus, 0x8dd902, 0x01c2);
    for (ushort frame = 0; frame < HyperBeamPaletteFxState.FrameCount; frame++)
    {
        int record = 0x8dd904 + frame * 20;
        WriteTestWord(bus, record, 2);
        for (ushort color = 0; color < HyperBeamPaletteFxState.ColorsPerFrame; color++)
        {
            WriteTestWord(
                bus,
                record + 2 + color * 2,
                unchecked((ushort)(0x0100 + frame * 0x20 + color)));
        }
        WriteTestWord(bus, record + 18, 0xc595);
    }
    WriteTestWord(bus, 0x8dd9cc, 0xc61e);
    WriteTestWord(bus, 0x8dd9ce, 0xd904);
    var drainedCgram = new SnesCgram();

    const int width = 8;
    const int height = 8;
    var foreground = new ushort[width * height];
    for (int x = 0; x < width; x++)
        foreground[6 * width + x] = 0x8000; // Solid floor begins at whole Y 96.
    RoomLevelData level = CreateRoom(
        width, height, foreground, new byte[foreground.Length]);

    var right = new SamusState
    {
        Pose = SamusState.CrouchingRightPose,
        XPosition = 48,
        YPosition = 75,
    };
    right.RefreshCollisionRadii(bus);
    right.InitializeAnimation(bus);
    right.HorizontalSpeed.BaseSpeed = 3;
    right.HorizontalSpeed.BaseSubspeed = 0x4444;
    right.Kinematics.YSpeed = 2;
    right.Kinematics.YSubspeed = 0x2222;

    right.Drained.LetFall(bus, right);
    AssertEqual(SamusState.DrainedCrouchingRightPose, right.Pose,
        "drained controller zero selects right pose");
    AssertEqual((ushort)70, right.YPosition,
        "drained controller preserves source bottom while radius grows 16 to 21");
    AssertEqual((ushort)2, right.AnimationFrame, "drained fall begins at byte index two");
    AssertEqual((ushort)0, right.Kinematics.YSpeed, "drained fall clears whole Y speed");
    AssertEqual((ushort)2, right.Kinematics.YDirection, "drained fall selects down direction");
    AssertTrue(right.Drained.UpdatePalette(bus, drainedCgram, right.EquippedItems),
        "drained controller zero restores selected suit palette");
    AssertEqual((ushort)0x300f, drainedCgram.Colors[207],
        "drained controller zero copies all sixteen suit colors");

    // Index two lasts two ticks; index three lasts sixteen. Expiration reaches `$F7` at
    // index four, which installs the handler and advances again to index five's delay one.
    for (int tick = 0; tick < 18; tick++)
        right.AnimateNoFx(bus);
    AssertEqual((byte)0xf7, right.LastAnimationDelayCommand!.Value,
        "drained animation reaches F7");
    AssertEqual(DrainedSamusPhase.Falling, right.Drained.Phase,
        "F7 installs drained movement handler");
    AssertEqual((ushort)5, right.AnimationFrame, "F7 performs its second frame increment");
    AssertEqual((ushort)1, right.AnimationFrameTimer, "F7 selects following literal delay");

    right.Kinematics.YAcceleration = 0;
    right.Kinematics.YSubacceleration = 0x4000;
    ushort firstHandlerY = right.YPosition;
    DrainedSamusMovementResult first = right.Drained.StepFalling(bus, level, right, 0);
    AssertEqual(0, first.Vertical.AcceptedDisplacement,
        "first drained handler call uses old zero speed");
    AssertEqual(firstHandlerY, right.YPosition, "first drained handler frame is stationary");
    AssertEqual((ushort)0x4000, right.Kinematics.YSubspeed,
        "first drained handler frame stores gravity for next call");

    DrainedSamusMovementResult landing = default;
    StepUntil(
        () => landing.Landed,
        frame => landing = right.Drained.StepFalling(
            bus, level, right, unchecked((ushort)(frame + 1))),
        maximumFrames: 99,
        context: "drained handler block-floor landing");
    AssertEqual((ushort)75, right.YPosition, "drained body rests at floor minus radius");
    AssertEqual(DrainedSamusPhase.OnFloor, right.Drained.Phase,
        "collision restores normal movement pointer");
    AssertEqual((ushort)7, right.AnimationFrame, "collision jumps to crouched floor art");
    AssertEqual((ushort)8, right.AnimationFrameTimer, "collision loads literal floor-art timer");
    AssertTrue(landing.ImpactYSpeed != 0 || landing.ImpactYSubspeed != 0,
        "drained landing preserves pre-clear impact magnitude");

    right.Drained.PutStanding(bus, right);
    AssertEqual(SamusState.DrainedStandingRightPose, right.Pose,
        "controller one selects standing right");
    AssertEqual((ushort)0, right.AnimationFrame, "standing drained starts index zero");
    AssertEqual((ushort)16, right.AnimationFrameTimer, "standing drained timer is literal sixteen");

    right.Drained.Release(bus, right);
    AssertEqual((ushort)4, right.AnimationFrame, "standing release writes byte index four");
    AssertEqual((ushort)1, right.AnimationFrameTimer, "standing release writes timer one");
    for (int tick = 0; tick < 8 && right.PendingTransitionalPose is null; tick++)
        right.AnimateNoFx(bus);
    AssertEqual<byte?>(SamusState.FacingRightNormalPose, right.PendingTransitionalPose,
        "standing drained release reaches ROM FD operand");
    AssertTrue(right.ApplyPendingVerifiedAnimationTransition(bus),
        "right drained release applies standing transition");
    AssertEqual(DrainedSamusPhase.Inactive, right.Drained.Phase,
        "right drained release clears host handler marker");

    var left = new SamusState
    {
        Pose = SamusState.CrouchingLeftPose,
        XPosition = 48,
        YPosition = 75,
    };
    left.RefreshCollisionRadii(bus);
    left.InitializeAnimation(bus);
    left.Drained.PutCrouchingOrFalling(bus, left);
    AssertEqual(SamusState.DrainedCrouchingLeftPose, left.Pose,
        "controller four selects left crouching/falling pose");
    AssertEqual((ushort)8, left.AnimationFrame, "controller four writes byte index eight");
    AssertEqual((ushort)16, left.AnimationFrameTimer, "controller four writes timer sixteen");
    left.Drained.Release(bus, left);
    AssertEqual((ushort)13, left.AnimationFrame,
        "crouched release preserves literal operand-adjacent byte index thirteen");
    for (int tick = 0; tick < 64 && left.PendingTransitionalPose is null; tick++)
        left.AnimateNoFx(bus);
    AssertEqual<byte?>(SamusState.FacingLeftNormalPose, left.PendingTransitionalPose,
        "left crouched release reaches asymmetric FD operand");
    AssertTrue(left.ApplyPendingVerifiedAnimationTransition(bus),
        "left drained release applies standing transition");

    left.Drained.EnableHyperBeam(left);
    AssertEqual((ushort)0x1009, left.EquippedBeams,
        "controller three installs exact hyper beam equipment word");
    AssertEqual((ushort)0x8000, left.HyperBeam,
        "controller three sets hyper beam flag");
    AssertTrue(left.Drained.HyperBeamPaletteFxRequested,
        "controller three spawns Hyper Beam palette-FX object");
    AssertEqual((ushort)1, left.Drained.HyperBeamPaletteFx.InstructionTimer,
        "palette-FX spawn installs native timer one");
    AssertEqual((ushort)0xd900, left.Drained.HyperBeamPaletteFx.InstructionPointer,
        "palette-FX spawn installs object instruction pointer");

    // Seed both neighboring colors so the test can distinguish the exact eight-color
    // write from a convenient whole-palette copy. Calls 1/2 show frame zero, calls 3/4
    // show frame one, and call 21 executes `$C61E,$D904` and reloads frame zero.
    drainedCgram.SetColor(0xe0, 0x4567);
    drainedCgram.SetColor(0xe9, 0x2345);
    for (int call = 0; call < 21; call++)
    {
        HyperBeamPaletteFxStepResult paletteFx =
            left.Drained.HyperBeamPaletteFx.Step(bus, drainedCgram);
        int expectedFrame = (call / 2) % HyperBeamPaletteFxState.FrameCount;
        AssertEqual(expectedFrame, paletteFx.FrameIndex,
            $"Hyper Beam palette-FX call {call + 1} frame index");
        AssertEqual((call & 1) == 0, paletteFx.PaletteWritten,
            $"Hyper Beam palette-FX call {call + 1} write cadence");
        for (int color = 0; color < HyperBeamPaletteFxState.ColorsPerFrame; color++)
        {
            AssertEqual(
                unchecked((ushort)(0x0100 + expectedFrame * 0x20 + color)),
                drainedCgram.Colors[0xe1 + color],
                $"Hyper Beam palette-FX call {call + 1} color {color}");
        }
    }
    AssertEqual((ushort)1, left.Drained.HyperBeamPaletteFx.CompletedCycles,
        "Hyper Beam palette-FX completes one cycle on call twenty-one");
    AssertEqual((ushort)0x4567, drainedCgram.Colors[0xe0],
        "Hyper Beam palette-FX preserves color before its range");
    AssertEqual((ushort)0x2345, drainedCgram.Colors[0xe9],
        "Hyper Beam palette-FX preserves color after its range");

    // Mother Brain's first rainbow-beam hit calls command five or `$18`. Both routes force
    // pose `$54` and lock normal input; only their installed Up-edge handler differs.
    var able = new SamusState
    {
        Pose = SamusState.FacingRightNormalPose,
        XPosition = 48,
        YPosition = 75,
    };
    able.RefreshCollisionRadii(bus);
    able.InitializeAnimation(bus);
    able.Drained.SetupForRainbowBeamAbleToStand(bus, able);
    AssertEqual(SamusState.KnockbackLeftPose, able.Pose,
        "rainbow command five unconditionally selects left knockback pose $54");
    AssertEqual(DrainedSamusPhase.RainbowBeamLocked, able.Drained.Phase,
        "rainbow command five locks Samus-side movement");
    AssertEqual(DrainedGetUpHandler.AbleToStand, able.Drained.GetUpHandler,
        "rainbow command five installs able timer handler");

    // The handler survives the later controller-four pose change. It accepts Up only from
    // left drained `$E9` at frame >=8, writes 13/1, and replaces itself with RTS.
    able.Drained.PutCrouchingOrFalling(bus, able);
    AssertTrue(!able.Drained.StepGetUpHandler(able, newlyPressedInput: 0),
        "able timer handler ignores absent Up edge");
    AssertTrue(able.Drained.StepGetUpHandler(able, newlyPressedInput: 0x0800),
        "able timer handler accepts Up from E9 frame eight");
    AssertEqual((ushort)13, able.AnimationFrame, "able timer handler selects stand-up frame thirteen");
    AssertEqual((ushort)1, able.AnimationFrameTimer, "able timer handler selects one-tick timer");
    AssertEqual(DrainedGetUpHandler.Inactive, able.Drained.GetUpHandler,
        "able timer handler replaces itself with RTS");

    var unable = new SamusState
    {
        Pose = SamusState.FacingRightNormalPose,
        XPosition = 48,
        YPosition = 75,
    };
    unable.RefreshCollisionRadii(bus);
    unable.InitializeAnimation(bus);
    unable.Drained.SetupForRainbowBeamUnableToStand(bus, unable);
    unable.Drained.PutCrouchingOrFalling(bus, unable);
    AssertEqual(DrainedGetUpHandler.UnableToStand, unable.Drained.GetUpHandler,
        "rainbow command $18 installs failed-stand timer handler");
    AssertTrue(unable.Drained.StepGetUpHandler(unable, newlyPressedInput: 0x0800),
        "failed-stand handler accepts Up only inside frames eight through eleven");
    AssertEqual((ushort)18, unable.AnimationFrame, "failed-stand handler selects frame eighteen");
    AssertEqual(DrainedGetUpHandler.UnableToStand, unable.Drained.GetUpHandler,
        "failed-stand handler remains installed");

    // The two later cutscene commands are direct animation writes. They do not select a
    // new pose or reinitialize a delay stream.
    unable.Drained.FreezeForHyperBeamAcquisition(unable);
    AssertEqual((ushort)28, unable.AnimationFrame, "command $19 freezes drained art at frame $1C");
    AssertEqual((ushort)1, unable.AnimationFrameTimer, "command $19 freeze timer");
    // Command `$16` starts negative-super-special palette zero with one-call cadence. After
    // the Baby raises its delay to two, index two must remain visible for two calls before
    // advancing. Each call loads before decrementing, matching `$91:D96F-$D997` ordering.
    unable.Drained.EnableRainbow(unable);
    AssertTrue(unable.Drained.UpdatePalette(bus, drainedCgram, unable.EquippedItems),
        "rainbow handler owns palette dispatcher");
    AssertEqual((ushort)0x1000, drainedCgram.Colors[192],
        "rainbow first call loads Hyper Beam palette zero");
    AssertEqual((ushort)1, unable.Drained.ChargePaletteIndex,
        "one-call rainbow cadence advances immediately");
    unable.Drained.IncrementRainbowPaletteFrame(maximumFrame: 10);
    unable.Drained.UpdatePalette(bus, drainedCgram, unable.EquippedItems);
    AssertEqual((ushort)2, unable.Drained.CommonPaletteTimer,
        "Baby-raised rainbow delay reloads two");
    AssertEqual((ushort)2, unable.Drained.ChargePaletteIndex,
        "rainbow second palette advances into delayed index");
    unable.Drained.UpdatePalette(bus, drainedCgram, unable.EquippedItems);
    AssertEqual((ushort)0x1040, drainedCgram.Colors[192],
        "rainbow delayed index loads before timer decrement");
    AssertEqual((ushort)2, unable.Drained.ChargePaletteIndex,
        "rainbow delay holds palette index for first call");
    unable.Drained.UpdatePalette(bus, drainedCgram, unable.EquippedItems);
    AssertEqual((ushort)3, unable.Drained.ChargePaletteIndex,
        "rainbow delay advances on second call");

    unable.Drained.DisableRainbowAndStartStandingAnimation(unable);
    AssertEqual((ushort)13, unable.AnimationFrame, "command $17 resumes standing animation at frame thirteen");
    AssertEqual((ushort)1, unable.AnimationFrameTimer, "command $17 resume timer");
    AssertTrue(unable.Drained.UpdatePalette(bus, drainedCgram, unable.EquippedItems),
        "command $17 restores selected suit palette");
    AssertEqual((ushort)0x3000, drainedCgram.Colors[192],
        "command $17 replaces rainbow palette immediately");

    Console.WriteLine("  Drained Samus: rainbow/body and Hyper Beam projectile palettes, controllers, fall, and releases agree.");
}

/// <summary>
/// Exercises the shared bank-$A0 enemy probe independently of room blocks. The fixtures are
/// intentionally synthetic: no translated room currently owns a live enemy actor list, and
/// silently substituting terrain or decorative sprites would not test the native routine.
/// </summary>
static void VerifySamusSolidEnemyCollision()
{
    var samus = new SamusKinematicsState
    {
        XPosition = 100,
        XSubposition = 0,
        YPosition = 100,
        YSubposition = 0x7777,
        XRadius = 5,
        YRadius = 10,
    };

    // The no-enemy path returns A=0 and leaves Samus completely untouched.
    SolidEnemyCollisionResult empty = SamusSolidEnemyCollision.Probe(
        samus, [], SamusCollisionDirection.Right, distance: 3, distanceSubposition: 0x4000);
    AssertTrue(!empty.Collided, "empty interactive-enemy list does not collide");
    AssertEqual((ushort)104, empty.TargetXPosition, "right fractional target rounds outward");
    AssertEqual((ushort)0x7777, samus.YSubposition, "no collision preserves Y subposition");

    // `$A0:A90A-$A0:A9B7` has asymmetric-looking but literal carry/borrow rounding. Test all
    // four jump-table entries, including fractional underflow and overflow, so a later
    // refactor cannot replace this with ordinary truncation or Math.Round.
    SolidEnemyCollisionResult left = SamusSolidEnemyCollision.Probe(
        samus, [], SamusCollisionDirection.Left, distance: 0, distanceSubposition: 0x8000);
    AssertEqual((ushort)98, left.TargetXPosition, "left fractional borrow plus outward decrement");
    AssertEqual((ushort)100, left.TargetYPosition, "left probe preserves target Y");

    samus.XSubposition = 0xf000;
    SolidEnemyCollisionResult rightCarry = SamusSolidEnemyCollision.Probe(
        samus, [], SamusCollisionDirection.Right, distance: 0, distanceSubposition: 0x2000);
    AssertEqual((ushort)102, rightCarry.TargetXPosition, "right fractional carry plus outward increment");

    samus.YSubposition = 0;
    SolidEnemyCollisionResult up = SamusSolidEnemyCollision.Probe(
        samus, [], SamusCollisionDirection.Up, distance: 1, distanceSubposition: 0x8000);
    AssertEqual((ushort)97, up.TargetYPosition, "up target shares negative-direction rounding");

    samus.YSubposition = 0xf000;
    SolidEnemyCollisionResult down = SamusSolidEnemyCollision.Probe(
        samus, [], SamusCollisionDirection.Down, distance: 1, distanceSubposition: 0x2000);
    AssertEqual((ushort)103, down.TargetYPosition, "down target shares positive-direction rounding");

    samus.XSubposition = 0;
    samus.YSubposition = 0x7777;
    var decorative = new SolidEnemyCollisionBody(
        Index: 0x0040, XPosition: 110, YPosition: 100, XRadius: 5, YRadius: 5,
        FreezeTimer: 0, Properties: 0);
    SolidEnemyCollisionResult ignored = SamusSolidEnemyCollision.Probe(
        samus, [decorative], SamusCollisionDirection.Right, distance: 2, distanceSubposition: 0);
    AssertTrue(!ignored.Collided, "non-solid unfrozen enemy is ignored");

    // A frozen enemy is eligible even without property $8000. With a one-pixel current gap,
    // the future box overlaps and the routine clips the requested distance to exactly one.
    SolidEnemyCollisionBody frozen = decorative with
    {
        Index = 0x0080,
        XPosition = 111,
        FreezeTimer = 1,
    };
    SolidEnemyCollisionResult frozenHit = SamusSolidEnemyCollision.Probe(
        samus, [frozen], SamusCollisionDirection.Right, distance: 2, distanceSubposition: 0);
    AssertTrue(frozenHit.Collided, "frozen enemy is solid to Samus");
    AssertEqual((ushort)1, frozenHit.Distance, "right collision publishes current edge gap");
    AssertEqual((ushort)0, frozenHit.DistanceSubposition, "collision clears fractional distance output");
    AssertEqual((ushort?)0x0080, frozenHit.EnemyIndex, "collision publishes native enemy index");
    AssertTrue(!frozenHit.WasTouching, "positive gap is not reported as touching");
    AssertEqual((ushort)0x7777, samus.YSubposition, "positive-gap collision preserves Samus subposition");

    // Property bit 15 takes the other eligibility route. Exact contact reaches `$A0:AAC8`,
    // whose STZ $0AFC bug clears Y subposition even though this is a horizontal probe.
    SolidEnemyCollisionBody solidTouch = decorative with
    {
        Index = 0x00c0,
        Properties = 0x8000,
    };
    SolidEnemyCollisionResult touching = SamusSolidEnemyCollision.Probe(
        samus, [solidTouch], SamusCollisionDirection.Right, distance: 1, distanceSubposition: 0);
    AssertTrue(touching.Collided && touching.WasTouching, "zero-gap solid enemy takes touching path");
    AssertEqual((ushort)0, touching.Distance, "touching collision publishes zero distance");
    AssertEqual((ushort)0, samus.YSubposition, "horizontal enemy touch preserves native Y-subposition bug");

    // The future broad-phase test is strict. Boxes that merely touch at their radii sum do
    // not advance to the directional gap test.
    SolidEnemyCollisionBody tangent = solidTouch with { XPosition = 110 };
    SolidEnemyCollisionResult tangentMiss = SamusSolidEnemyCollision.Probe(
        samus, [tangent], SamusCollisionDirection.Right, distance: 0, distanceSubposition: 0);
    AssertTrue(!tangentMiss.Collided, "future box tangency is not broad-phase overlap");

    // A negative directional gap is a signed/BPL rejection: Samus already overlaps this
    // enemy, so the routine must not trap her inside it or manufacture distance zero.
    SolidEnemyCollisionBody embedded = solidTouch with { XPosition = 109 };
    SolidEnemyCollisionResult embeddedMiss = SamusSolidEnemyCollision.Probe(
        samus, [embedded], SamusCollisionDirection.Right, distance: 2, distanceSubposition: 0);
    AssertTrue(!embeddedMiss.Collided, "enemy already intersecting movement axis is skipped");

    // Broad overlap must succeed on the perpendicular axis as well.
    SolidEnemyCollisionBody verticalMiss = frozen with { YPosition = 116 };
    SolidEnemyCollisionResult perpendicularMiss = SamusSolidEnemyCollision.Probe(
        samus, [verticalMiss], SamusCollisionDirection.Right, distance: 2, distanceSubposition: 0);
    AssertTrue(!perpendicularMiss.Collided, "perpendicular separation rejects directional candidate");

    // Native list order wins. The first entry is farther away than the second but is still
    // returned as soon as both its broad and directional tests pass.
    SolidEnemyCollisionBody fartherFirst = solidTouch with { Index = 0x0100, XPosition = 120 };
    SolidEnemyCollisionBody nearerSecond = solidTouch with { Index = 0x0140, XPosition = 112 };
    SolidEnemyCollisionResult ordered = SamusSolidEnemyCollision.Probe(
        samus, [fartherFirst, nearerSecond], SamusCollisionDirection.Right,
        distance: 20, distanceSubposition: 0);
    AssertEqual((ushort?)0x0100, ordered.EnemyIndex, "first interactive collision wins over nearest geometry");
    AssertEqual((ushort)10, ordered.Distance, "first list entry publishes its own gap");

    // Exercise both vertical directional formulas rather than relying only on target tests.
    SolidEnemyCollisionBody above = solidTouch with
    {
        Index = 0x0180,
        XPosition = 100,
        YPosition = 84,
        XRadius = 5,
        YRadius = 5,
    };
    SolidEnemyCollisionResult ceiling = SamusSolidEnemyCollision.Probe(
        samus, [above], SamusCollisionDirection.Up, distance: 2, distanceSubposition: 0);
    AssertTrue(ceiling.Collided, "upward solid-enemy collision detected");
    AssertEqual((ushort)1, ceiling.Distance, "upward collision publishes top-to-bottom gap");

    SolidEnemyCollisionBody below = above with { Index = 0x01c0, YPosition = 116 };
    SolidEnemyCollisionResult floor = SamusSolidEnemyCollision.Probe(
        samus, [below], SamusCollisionDirection.Down, distance: 2, distanceSubposition: 0);
    AssertTrue(floor.Collided, "downward solid-enemy collision detected");
    AssertEqual((ushort)1, floor.Distance, "downward collision publishes bottom-to-top gap");

    // Ordinary managed movement now preserves the native wrapper order: enemy probe first,
    // room blocks only on a miss, then position addition. An all-air room isolates that seam.
    const int roomWidth = 8;
    const int roomHeight = 8;
    RoomLevelData airRoom = CreateEmptyRoom(roomWidth, roomHeight);
    var integrated = new SamusKinematicsState
    {
        XPosition = 100,
        YPosition = 100,
        XRadius = 5,
        YRadius = 10,
        InteractiveEnemies = [frozen],
    };
    BlockMoveResult integratedHorizontal = SamusBlockCollision.MoveHorizontal(
        new TestAddressSpace(), airRoom, integrated, displacement: 2 << 16);
    AssertTrue(integratedHorizontal.Collided, "ordinary horizontal mover reports enemy collision");
    AssertEqual((ushort)101, integrated.XPosition, "ordinary horizontal mover clips to enemy gap");
    AssertTrue(integratedHorizontal.CollisionBlock is null, "enemy collision does not invent terrain block");
    AssertEqual(
        (ushort?)0x0080,
        integratedHorizontal.EnemyCollision?.EnemyIndex,
        "ordinary mover retains colliding enemy identity");

    integrated.YPosition = 100;
    integrated.InteractiveEnemies = [below];
    BlockMoveResult integratedVertical = SamusBlockCollision.MoveVertical(
        new TestAddressSpace(),
        airRoom,
        integrated,
        displacement: 2 << 16,
        scanLeftToRight: true);
    AssertTrue(integratedVertical.Collided, "ordinary vertical mover reports enemy collision");
    AssertEqual((ushort)101, integrated.YPosition, "ordinary vertical mover clips to enemy gap");
    AssertEqual((ushort?)0x01c0, integratedVertical.EnemyCollision?.EnemyIndex,
        "vertical mover retains colliding enemy identity");

    // Wall-jump probing copies Samus so it cannot commit movement. The one exception is the
    // original `$A0:AAC8` touching bug, which still clears the real Y subposition.
    integrated.XPosition = 100;
    integrated.YPosition = 100;
    integrated.YSubposition = 0x4321;
    integrated.InteractiveEnemies = [solidTouch];
    BlockMoveResult wallProbe = SamusBlockCollision.ProbeWallHorizontal(
        new TestAddressSpace(), airRoom, integrated, signedDistance: 1 << 16);
    AssertTrue(wallProbe.EnemyCollision is { WasTouching: true },
        "wall probe detects touching enemy before blocks");
    AssertEqual((ushort)100, integrated.XPosition, "wall probe does not commit X motion");
    AssertEqual((ushort)0, integrated.YSubposition, "wall probe commits only native touching side effect");

    AssertThrows<ArgumentOutOfRangeException>(
        () => SamusSolidEnemyCollision.Probe(
            samus, [], (SamusCollisionDirection)4, distance: 0, distanceSubposition: 0),
        "invalid solid-enemy direction rejected");

    Console.WriteLine("  Solid enemies: eligibility, native rounding, overlap, gaps, order, and touching quirk agree.");
}

/// <summary>
/// Exercises Mother Brain's separate bank-$A9 position owner. These checks deliberately
/// target byte carry, signed 8.8 easing, hardcoded arena clamps, trig-table scaling, and
/// previous-position publication—the details most likely to be lost in a float rewrite.
/// </summary>
/// <summary>
/// Exercises the complete ten-pose movement-type-`$1A` family, both owner offsets, ROM
/// transition-table priority, no-input fallback, the alternating-D-pad escape counter, and
/// every velocity word cleared by `$90:E2DE` release.
/// </summary>
static void VerifySamusGrabbedByDraygon()
{
    var bus = new TestAddressSpace();

    byte[] leftPoses = [
        SamusState.DraygonGrabbedNeutralLeftPose,
        SamusState.DraygonGrabbedAimUpLeftPose,
        SamusState.DraygonGrabbedFiringLeftPose,
        SamusState.DraygonGrabbedAimDownLeftPose,
        SamusState.DraygonGrabbedMovingLeftPose,
    ];
    byte[] rightPoses = [
        SamusState.DraygonGrabbedNeutralRightPose,
        SamusState.DraygonGrabbedAimUpRightPose,
        SamusState.DraygonGrabbedFiringRightPose,
        SamusState.DraygonGrabbedAimDownRightPose,
        SamusState.DraygonGrabbedMovingRightPose,
    ];

    // These are the literal retail pose-definition records at `$91:BBF9-$BC20` and
    // `$91:BD89-$BDB0`. In particular, the four non-neutral records on each side fall back
    // to `$BA/$EC`, all ten use radius 21, and moving art disables projectile direction.
    byte[][] leftDefinitions = [
        [0x04, 0x1a, 0xff, 0x07, 0x06, 0x00, 0x15, 0x00],
        [0x04, 0x1a, 0xba, 0x08, 0x06, 0x00, 0x15, 0x00],
        [0x04, 0x1a, 0xba, 0x07, 0x06, 0x00, 0x15, 0x00],
        [0x04, 0x1a, 0xba, 0x06, 0x06, 0x00, 0x15, 0x00],
        [0x04, 0x1a, 0xba, 0xff, 0x06, 0x00, 0x15, 0x00],
    ];
    byte[][] rightDefinitions = [
        [0x08, 0x1a, 0xff, 0x02, 0x06, 0x00, 0x15, 0x00],
        [0x08, 0x1a, 0xec, 0x01, 0x06, 0x00, 0x15, 0x00],
        [0x08, 0x1a, 0xec, 0x02, 0x06, 0x00, 0x15, 0x00],
        [0x08, 0x1a, 0xec, 0x03, 0x06, 0x00, 0x15, 0x00],
        [0x08, 0x1a, 0xec, 0xff, 0x06, 0x00, 0x15, 0x00],
    ];
    for (int index = 0; index < leftPoses.Length; index++)
    {
        WritePoseDefinition(bus, leftPoses[index], leftDefinitions[index]);
        WritePoseDefinition(bus, rightPoses[index], rightDefinitions[index]);

        // `$BA-$BD/$EC-$EF` use stationary `$B2B4`; `$BE/$F0` use six-frame `$B53C`.
        ushort leftDelay = index == 4 ? (ushort)0xb53c : (ushort)0xb2b4;
        ushort rightDelay = index == 4 ? (ushort)0xb53c : (ushort)0xb2b4;
        WriteTestWord(bus, 0x91b010 + leftPoses[index] * 2, leftDelay);
        WriteTestWord(bus, 0x91b010 + rightPoses[index] * 2, rightDelay);

        // All five poses on a side point to the same held-input transition program.
        WriteTestWord(bus, 0x919ee2 + leftPoses[index] * 2, 0xae18);
        WriteTestWord(bus, 0x919ee2 + rightPoses[index] * 2, 0xae56);
    }

    // Ordinary release destinations need real metadata and a harmless initial delay.
    WritePoseDefinition(bus, SamusState.FacingRightNormalPose,
        [0x08, 0x00, 0xff, 0x02, 0x06, 0x00, 0x15, 0x00]);
    WritePoseDefinition(bus, SamusState.FacingLeftNormalPose,
        [0x04, 0x00, 0xff, 0x07, 0x06, 0x00, 0x15, 0x00]);
    WriteTestWord(bus, 0x91b010 + SamusState.FacingRightNormalPose * 2, 0xc000);
    WriteTestWord(bus, 0x91b010 + SamusState.FacingLeftNormalPose * 2, 0xc001);
    bus.WriteBytes(0x91b2b4, [0x10, 0xff]);
    bus.WriteBytes(0x91b53c, [0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0xff]);
    bus.WriteBytes(0x91c000, [0x0a]);
    bus.WriteBytes(0x91c001, [0x0a]);

    static void WriteTransitionProgram(
        TestAddressSpace target,
        int address,
        (ushort Held, ushort Pose)[] records)
    {
        foreach ((ushort held, ushort pose) in records)
        {
            WriteTestWord(target, address, 0x0000);
            WriteTestWord(target, address + 2, held);
            WriteTestWord(target, address + 4, pose);
            address += 6;
        }
        WriteTestWord(target, address, 0xffff);
    }

    WriteTransitionProgram(bus, 0x91ae18, [
        (0x0a40, 0x00bb), (0x0640, 0x00bd), (0x0240, 0x00bc),
        (0x0010, 0x00bb), (0x0020, 0x00bd), (0x0040, 0x00bc),
        (0x0200, 0x00be), (0x0100, 0x00be), (0x0800, 0x00be),
        (0x0400, 0x00be),
    ]);
    WriteTransitionProgram(bus, 0x91ae56, [
        (0x0940, 0x00ed), (0x0540, 0x00ef), (0x0140, 0x00ee),
        (0x0010, 0x00ed), (0x0020, 0x00ef), (0x0040, 0x00ee),
        (0x0200, 0x00f0), (0x0100, 0x00f0), (0x0800, 0x00f0),
        (0x0400, 0x00f0),
    ]);

    var samus = new SamusState { XPosition = 0x0080, YPosition = 0x0100 };
    samus.DraygonGrabbed.Begin(bus, samus, draygonFacingRight: true);
    AssertEqual(SamusState.DraygonGrabbedNeutralRightPose, samus.Pose,
        "right-facing Draygon entry selects $EC");
    AssertEqual((ushort)21, samus.Kinematics.YRadius, "Draygon entry loads radius 21");

    DraygonOwnerPlacement rightPlacement = samus.DraygonGrabbed.ApplyOwnerPosition(
        samus, ownerXPosition: 0x0100, ownerYPosition: 0x0180, draygonFacingRight: true);
    AssertEqual((short)8, rightPlacement.XOffset, "right-facing claw offset is +8");
    AssertEqual((ushort)0x0108, samus.XPosition, "right-facing owner placement X");
    AssertEqual((ushort)0x01a8, samus.YPosition, "owner placement Y is body plus $28");

    samus.SolidVerticalCollisionResult = 5;
    DraygonGrabbedMovementResult movement = samus.DraygonGrabbed.StepMovement(samus);
    AssertEqual((ushort)5, movement.PreviousSolidVerticalCollisionResult,
        "type-$1A observes stale vertical collision word");
    AssertEqual((ushort)0, movement.SolidVerticalCollisionResult,
        "type-$1A performs its sole STZ side effect");
    AssertEqual((ushort)0x0108, movement.XPosition, "type-$1A does not move X");
    AssertEqual((ushort)0x01a8, movement.YPosition, "type-$1A does not move Y");

    // The larger right+up+shoot chord must select the first `$AE56` record (`$ED`), not
    // the later generic shoot record (`$EE`), proving ROM priority rather than host rules.
    SamusPoseTransition rightUpShoot = SamusPoseTransitionTable.Find(
        bus, samus.Pose, canonicalHeldInput: 0x0940, canonicalNewInput: 0)!.Value;
    AssertEqual((ushort)SamusState.DraygonGrabbedAimUpRightPose,
        rightUpShoot.ProspectivePose, "right grabbed transition priority");
    samus.ApplyDraygonGrabbedPoseChange(bus, (byte)rightUpShoot.ProspectivePose);
    AssertEqual(SamusState.DraygonGrabbedAimUpRightPose, samus.Pose,
        "right grabbed aim transition applies");
    AssertEqual((ushort)SamusState.DraygonGrabbedNeutralRightPose,
        samus.ReadNoInputFallbackPose(bus), "right grabbed aim fallback is $EC");
    samus.ApplyDraygonGrabbedPoseChange(bus, samus.ReadNoInputFallbackPose(bus));

    DraygonEscapeResult locked = samus.DraygonGrabbed.StepEscapeHandler(
        bus, samus, newlyPressedInput: 0, grappleLockedInPlace: true);
    AssertTrue(locked.SuppressProspectivePose, "locked grapple suppresses grabbed pose transition");
    AssertEqual((ushort)0, locked.EscapeButtonCounter, "no D-pad edge does not count");

    DraygonEscapeResult firstUp = samus.DraygonGrabbed.StepEscapeHandler(
        bus, samus, newlyPressedInput: 0x0800, grappleLockedInPlace: false);
    DraygonEscapeResult repeatedUp = samus.DraygonGrabbed.StepEscapeHandler(
        bus, samus, newlyPressedInput: 0x0800, grappleLockedInPlace: false);
    AssertTrue(firstUp.CountedInput, "first Up edge increments escape counter");
    AssertTrue(!repeatedUp.CountedInput, "repeated D-pad pattern is rejected");
    AssertEqual((ushort)1, repeatedUp.EscapeButtonCounter,
        "repeated direction leaves escape counter unchanged");

    samus.HorizontalSpeed.BaseSpeed = 3;
    samus.HorizontalSpeed.BaseSubspeed = 0x4000;
    samus.HorizontalSpeed.ExtraRunSpeed = 2;
    samus.HorizontalSpeed.ExtraRunSubspeed = 0x8000;
    samus.HorizontalSpeed.AccelerationMode = 2;
    samus.Kinematics.YSpeed = 4;
    samus.Kinematics.YSubspeed = 0xc000;
    samus.Kinematics.YDirection = 2;
    samus.MorphBallBounceState = 2;

    DraygonEscapeResult release = repeatedUp;
    for (int inputNumber = 1; inputNumber < SamusDraygonGrabbedState.EscapeButtonCounterTarget; inputNumber++)
    {
        ushort direction = (inputNumber & 1) != 0 ? (ushort)0x0400 : (ushort)0x0800;
        release = samus.DraygonGrabbed.StepEscapeHandler(
            bus, samus, direction, grappleLockedInPlace: false);
    }
    AssertTrue(release.Released, "sixtieth alternating D-pad input releases Samus");
    AssertEqual(SamusState.FacingRightNormalPose, samus.Pose,
        "right grabbed family releases to pose $01");
    AssertEqual((ushort)0, samus.HorizontalSpeed.BaseSpeed, "release clears base X speed");
    AssertEqual((ushort)0, samus.HorizontalSpeed.BaseSubspeed, "release clears base X subspeed");
    AssertEqual((ushort)2, samus.HorizontalSpeed.ExtraRunSpeed,
        "release intentionally preserves extra run speed");
    AssertEqual((ushort)0x8000, samus.HorizontalSpeed.ExtraRunSubspeed,
        "release intentionally preserves extra run subspeed");
    AssertEqual((ushort)0, samus.Kinematics.YSpeed, "release clears Y speed");
    AssertEqual((ushort)0, samus.Kinematics.YSubspeed, "release clears Y subspeed");
    AssertEqual((ushort)0, samus.Kinematics.YDirection, "release clears Y direction");
    AssertEqual((ushort)0, samus.MorphBallBounceState, "release clears bounce state");
    AssertEqual((ushort)0, samus.HorizontalSpeed.AccelerationMode,
        "release clears X acceleration mode");
    AssertTrue(samus.DraygonGrabbed.ConsumeOwnerReleaseSignal(),
        "release publishes one owner-consumed bit");
    AssertTrue(!samus.DraygonGrabbed.ConsumeOwnerReleaseSignal(),
        "owner release bit is one-shot");

    // Mirror the entry/owner/release direction without repeating the 60-input route.
    samus.DraygonGrabbed.Begin(bus, samus, draygonFacingRight: false);
    DraygonOwnerPlacement leftPlacement = samus.DraygonGrabbed.ApplyOwnerPosition(
        samus, ownerXPosition: 0x0100, ownerYPosition: 0x0180, draygonFacingRight: false);
    AssertEqual((short)-8, leftPlacement.XOffset, "left-facing claw offset is -8");
    AssertEqual((ushort)0x00f8, samus.XPosition, "left-facing owner placement X");
    samus.DraygonGrabbed.Release(bus, samus);
    AssertEqual(SamusState.FacingLeftNormalPose, samus.Pose,
        "left grabbed family releases to pose $02");

    Console.WriteLine("  Samus/Draygon: ten poses, owner offsets, transitions, escape, and release agree.");
}

}
