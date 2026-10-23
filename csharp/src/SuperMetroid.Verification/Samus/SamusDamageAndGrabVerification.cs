using SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Samus.Abilities.DrainedPoseHistoryVerification;
using static SuperMetroid.Verification.Fx.PaletteFx.HyperBeamPaletteFxColorDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SyntheticSamusCartridgeVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Samus.Artwork;
namespace SuperMetroid.Verification.Samus;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Progression;

/// <summary>Constructed cartridge checks for Samus draining, solid-enemy probes, and Draygon grab control.</summary>
internal static class SamusDamageAndGrabVerification
{

/// <summary>Checks drained-pose animation, control recovery, and palette progression against native pose streams.</summary>
[PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
internal static void VerifySamusDrainedController()
{
    var bus = new TestAddressSpace();

    // These are the literal direction/type/graphics-offset/radius fields for the source,
    // four drained records, and their eventual ordinary-standing destinations.
    WritePoseDefinition(bus, (int)SamusPoseId.CrouchingRightPose,
        [0x08, 0x05, 0xff, 0x02, 0x03, 0x00, 0x10, 0x00]);
    WritePoseDefinition(bus, (int)SamusPoseId.CrouchingLeftPose,
        [0x04, 0x05, 0xff, 0x07, 0x03, 0x00, 0x10, 0x00]);
    WritePoseDefinition(bus, (int)SamusPoseId.KnockbackLeftPose,
        [0x04, 0x0a, 0xff, 0xff, 0x06, 0x00, 0x15, 0x00]);
    WritePoseDefinition(bus, (int)SamusPoseId.DrainedCrouchingRightPose,
        [0x08, 0x1b, 0xff, 0xff, 0xfc, 0x00, 0x15, 0x00]);
    WritePoseDefinition(bus, (int)SamusPoseId.DrainedCrouchingLeftPose,
        [0x04, 0x1b, 0xff, 0xff, 0xfc, 0x00, 0x15, 0x00]);
    WritePoseDefinition(bus, (int)SamusPoseId.DrainedStandingRightPose,
        [0x08, 0x1b, 0xff, 0xff, 0xfc, 0x00, 0x15, 0x00]);
    WritePoseDefinition(bus, (int)SamusPoseId.DrainedStandingLeftPose,
        [0x04, 0x1b, 0xff, 0xff, 0xfc, 0x00, 0x15, 0x00]);
    WritePoseDefinition(bus, (int)SamusPoseId.FacingRightNormalPose,
        [0x08, 0x00, 0xff, 0x02, 0x06, 0x00, 0x15, 0x00]);
    WritePoseDefinition(bus, (int)SamusPoseId.FacingLeftNormalPose,
        [0x04, 0x00, 0xff, 0x07, 0x06, 0x00, 0x15, 0x00]);

    WritePoseAnimationDelayPointer(bus, SamusPoseId.CrouchingRightPose, 0xc000);
    WritePoseAnimationDelayPointer(bus, SamusPoseId.CrouchingLeftPose, 0xc001);
    WritePoseAnimationDelayPointer(bus, SamusPoseId.KnockbackLeftPose, 0xc020);
    WritePoseAnimationDelayPointer(bus, SamusPoseId.DrainedCrouchingRightPose, 0xb257);
    WritePoseAnimationDelayPointer(bus, SamusPoseId.DrainedCrouchingLeftPose, 0xb268);
    WritePoseAnimationDelayPointer(bus, SamusPoseId.DrainedStandingRightPose, 0xb288);
    WritePoseAnimationDelayPointer(bus, SamusPoseId.DrainedStandingLeftPose, 0xb290);
    WritePoseAnimationDelayPointer(bus, SamusPoseId.FacingRightNormalPose, 0xc010);
    WritePoseAnimationDelayPointer(bus, SamusPoseId.FacingLeftNormalPose, 0xc011);
    WriteAnimationDelayStream(bus, 0xc000, [0x05, 0x05]);
    WriteAnimationDelayStream(bus, 0xc010, [0x05, 0x05]);
    WriteAnimationDelayStream(bus, 0xc020, [0x01, 0xfe, 0x01]);

    // Copy the byte-oriented streams verbatim. Controller-written frame values are byte
    // indices, so command operands remain part of the index space by design.
    WriteAnimationDelayStream(bus, 0xb257, [
        0x02, 0x02, 0x02, 0x10, 0xf7,
        0x01, 0xfe, 0x01,
        0x10, 0x10, 0x10, 0x10, 0xfe, 0x04,
        0x03, 0xfd, 0x01,
    ]);
    WriteAnimationDelayStream(bus, 0xb268, [
        0x02, 0x02, 0x10, 0xf7,
        0x01, 0xfe, 0x01,
        0x08, 0x10, 0x10, 0x10, 0x10, 0xfe, 0x04,
        0x03, 0x03, 0x03, 0xfd, 0x02,
        0x10, 0x10, 0x10, 0x10, 0xfe, 0x0e,
        0x10, 0xfe, 0x11,
        0x10, 0xfe, 0x01,
    ]);
    WriteAnimationDelayStream(bus, 0xb288, [0x10, 0x10, 0x10, 0x10, 0xff, 0x03, 0xfd, 0x01]);
    WriteAnimationDelayStream(bus, 0xb290, [0x10, 0x10, 0x10, 0x10, 0xff, 0x03, 0xfd, 0x02]);

    InstallHyperBeamSuitPaletteFixture(bus);
    InstallHyperBeamProjectilePaletteProgramFixture(bus);
    var suitColors = SamusSuitColorCatalog.Load(new MemoryStream(
        SuperMetroid.AssetExtraction.Samus.SamusSuitColorExtractor.Extract(bus)));
    var rainbowColors = SamusHyperBeamColorCatalog.Load(new MemoryStream(
        SuperMetroid.AssetExtraction.Samus.SamusHyperBeamColorExtractor.Extract(bus)));
    var projectileColors = HyperBeamFxColorCatalog.Load(new MemoryStream(
        SuperMetroid.AssetExtraction.Samus.HyperBeamFxColorExtractor.Extract(bus)));
    var drainedCgram = new SamusColourRig();

    const int width = 8;
    const int height = 8;
    var foreground = new ushort[width * height];
    for (int x = 0; x < width; x++)
        foreground[6 * width + x] = 0x8000; // Solid floor begins at whole Y 96.
    RoomLevelData level = CreateRoom(new(width, height), foreground, new byte[foreground.Length]);

    VerifyDrainedPoseHistory();
    var right = new SamusState
    {
        Pose = SamusPoseId.CrouchingRightPose,
        Position = FixedPosition<RoomPixels>.FromWhole(new(48, 75)),
    };
    SamusPoseMetadata.RefreshCollisionRadii(right);
    SamusPresentation.InitializeAnimation(right);
    right.HorizontalSpeed.BaseSpeed = new SnesUnsignedSixteenSixteen(3, 0x4444);
    right.Kinematics.YSpeed = new SnesUnsignedSixteenSixteen(2, 0x2222);

    right.Drained.LetFall(right);
    AssertEqual(SamusPoseId.DrainedCrouchingRightPose, right.Pose,
        "drained controller zero selects right pose");
    AssertEqual(70, right.Position.Y.Whole,
        "drained controller preserves source bottom while radius grows 16 to 21");
    AssertEqual(2, right.AnimationFrame, "drained fall begins at byte index two");
    AssertEqual(0, right.Kinematics.YSpeed.Whole, "drained fall clears whole Y speed");
    AssertEqual(SamusVerticalDirection.Down, right.Kinematics.YDirection, "drained fall selects down direction");
    AssertTrue(right.Drained.UpdatePalette(drainedCgram.Samus, right.EquippedItems, suitColors),
        "drained controller zero restores selected suit palette");
    AssertEqual(0x300f, drainedCgram.Colors[207],
        "drained controller zero copies all sixteen suit colors");

    // Index two lasts two ticks; index three lasts sixteen. Expiration reaches `$F7` at
    // index four, which installs the handler and advances again to index five's delay one.
    for (int tick = 0; tick < 18; tick++)
        SamusPresentation.AnimateNoFx(right, new SamusAnimationStep { Random = new CartridgeRandom() });
    AssertEqual(DrainedSamusPhase.Falling, right.Drained.Phase,
        "F7 installs drained movement handler");
    AssertEqual(5, right.AnimationFrame, "F7 performs its second frame increment");
    AssertEqual(1, right.AnimationFrameTimer.RawValue, "F7 selects following literal delay");

    right.Kinematics.YAcceleration = new SnesUnsignedSixteenSixteen(0, 0x4000);
    ushort firstHandlerY = right.Position.Y.Whole;
    ushort firstHandlerYSubposition = right.Kinematics.Position.Y.Fraction;
    right.Drained.StepFalling(level, right, 0);
    AssertEqual(firstHandlerYSubposition, right.Kinematics.Position.Y.Fraction,
        "first drained handler call uses old zero speed");
    AssertEqual(firstHandlerY, right.Position.Y.Whole, "first drained handler frame is stationary");
    AssertEqual(0x4000, right.Kinematics.YSpeed.Fraction,
        "first drained handler frame stores gravity for next call");

    DrainedSamusMovementResult landing = default;
    StepUntil(
        () => landing.Landed,
        frame => landing = right.Drained.StepFalling(
level, right, unchecked((ushort)(frame + 1))),
        maximumFrames: 99,
        context: "drained handler block-floor landing");
    AssertEqual(75, right.Position.Y.Whole, "drained body rests at floor minus radius");
    AssertEqual(DrainedSamusPhase.OnFloor, right.Drained.Phase,
        "collision restores normal movement pointer");
    AssertEqual(7, right.AnimationFrame, "collision jumps to crouched floor art");
    AssertEqual(8, right.AnimationFrameTimer.RawValue, "collision loads literal floor-art timer");
    AssertTrue(landing.ImpactYSpeed != default,
        "drained landing preserves pre-clear impact magnitude");

    right.Drained.PutStanding(right);
    AssertEqual(SamusPoseId.DrainedStandingRightPose, right.Pose,
        "controller one selects standing right");
    AssertEqual(0, right.AnimationFrame, "standing drained starts index zero");
    AssertEqual(16, right.AnimationFrameTimer.RawValue, "standing drained timer is literal sixteen");

    right.Drained.Release(right);
    AssertEqual(4, right.AnimationFrame, "standing release writes byte index four");
    AssertEqual(1, right.AnimationFrameTimer.RawValue, "standing release writes timer one");
    for (int tick = 0; tick < 8 && right.PendingTransitionalPose is null; tick++)
        SamusPresentation.AnimateNoFx(right, new SamusAnimationStep { Random = new CartridgeRandom() });
    AssertEqual<SamusPoseId?>(SamusPoseId.FacingRightNormalPose, right.PendingTransitionalPose,
        "standing drained release reaches ROM FD operand");
    AssertTrue(SamusPoseTransitions.ApplyPendingVerifiedAnimationTransition(right),
        "right drained release applies standing transition");
    AssertEqual(DrainedSamusPhase.Inactive, right.Drained.Phase,
        "right drained release clears host handler marker");

    var left = new SamusState
    {
        Pose = SamusPoseId.CrouchingLeftPose,
        Position = FixedPosition<RoomPixels>.FromWhole(new(48, 75)),
    };
    SamusPoseMetadata.RefreshCollisionRadii(left);
    SamusPresentation.InitializeAnimation(left);
    left.Drained.PutCrouchingOrFalling(left);
    AssertEqual(SamusPoseId.DrainedCrouchingLeftPose, left.Pose,
        "controller four selects left crouching/falling pose");
    AssertEqual(8, left.AnimationFrame, "controller four writes byte index eight");
    AssertEqual(16, left.AnimationFrameTimer.RawValue, "controller four writes timer sixteen");
    left.Drained.Release(left);
    AssertEqual(13, left.AnimationFrame,
        "crouched release preserves literal operand-adjacent byte index thirteen");
    for (int tick = 0; tick < 64 && left.PendingTransitionalPose is null; tick++)
        SamusPresentation.AnimateNoFx(left, new SamusAnimationStep { Random = new CartridgeRandom() });
    AssertEqual<SamusPoseId?>(SamusPoseId.FacingLeftNormalPose, left.PendingTransitionalPose,
        "left crouched release reaches asymmetric FD operand");
    AssertTrue(SamusPoseTransitions.ApplyPendingVerifiedAnimationTransition(left),
        "left drained release applies standing transition");

    left.Drained.EnableHyperBeam(left);
    AssertEqual(0x1009, left.EquippedBeams.ToNativeWord(),
        "controller three installs exact hyper beam equipment word");
    AssertTrue(left.HyperBeamEnabled,
        "controller three sets hyper beam flag");
    AssertTrue(left.Drained.HyperBeamPaletteFxRequested,
        "controller three spawns Hyper Beam palette-FX object");
    AssertEqual(1, left.Drained.HyperBeamPaletteFx.InstructionTimer.RawValue,
        "palette-FX spawn installs native timer one");
    AssertEqual(AnimationProgramCursor<PaletteFxProgram>.Start(PaletteFxProgram.HyperBeam0), left.Drained.HyperBeamPaletteFx.Cursor,
        "palette-FX spawn starts the object's program");

    // Seed both neighboring colors so the test can distinguish the exact eight-color
    // write from a convenient whole-palette copy. Calls 1/2 show frame zero, calls 3/4
    // show frame one, and call 21 executes `$C61E,$D904` and reloads frame zero.
    drainedCgram.SetColor(0xe0, Bgr555.FromWord(0x4567));
    drainedCgram.SetColor(0xe9, Bgr555.FromWord(0x2345));
    var hyperBeamGuard = NewHyperBeamPaletteFxControlGuard(bus);
    for (int call = 0; call < 21; call++)
    {
        // `$8D:C552` writes a palette record only when its pre-decrement timer is one.
        bool paletteWritten = left.Drained.HyperBeamPaletteFx.InstructionTimer.RawValue == 1;
        left.Drained.HyperBeamPaletteFx.Step(drainedCgram.Samus, projectileColors);
        int expectedFrame = (call / 2) % HyperBeamPaletteFxState.FrameCount;
        AssertEqual(expectedFrame, left.Drained.HyperBeamPaletteFx.CurrentFrameIndex,
            $"Hyper Beam palette-FX call {call + 1} frame index");
        AssertEqual((call & 1) == 0, paletteWritten,
            $"Hyper Beam palette-FX call {call + 1} write cadence");
        for (int color = 0; color < HyperBeamPaletteFxState.ColorsPerFrame; color++)
        {
            AssertEqual(
                unchecked((ushort)(0x0100 + expectedFrame * 0x20 + color)),
                drainedCgram.Colors[0xe1 + color],
                $"Hyper Beam palette-FX call {call + 1} color {color}");
        }
    }
    AssertEqual(1, left.Drained.HyperBeamPaletteFx.CompletedCycles,
        "Hyper Beam palette-FX completes one cycle on call twenty-one");
    AssertEqual(0x4567, drainedCgram.Colors[0xe0],
        "Hyper Beam palette-FX preserves color before its range");
    AssertEqual(0x2345, drainedCgram.Colors[0xe9],
        "Hyper Beam palette-FX preserves color after its range");
    AssertEqual(0, hyperBeamGuard.ForbiddenReadAttempts,
        "Hyper Beam palette-FX performs no fixed-control ROM reads");

    // Mother Brain's first rainbow-beam hit calls command five or `$18`. Both routes force
    // pose `$54` and lock normal input; only their installed Up-edge handler differs.
    var able = new SamusState
    {
        Pose = SamusPoseId.FacingRightNormalPose,
        Position = FixedPosition<RoomPixels>.FromWhole(new(48, 75)),
    };
    SamusPoseMetadata.RefreshCollisionRadii(able);
    SamusPresentation.InitializeAnimation(able);
    able.Drained.SetupForRainbowBeamAbleToStand(able);
    AssertEqual(SamusPoseId.KnockbackLeftPose, able.Pose,
        "rainbow command five unconditionally selects left knockback pose $54");
    AssertEqual(DrainedSamusPhase.RainbowBeamLocked, able.Drained.Phase,
        "rainbow command five locks Samus-side movement");
    AssertEqual(DrainedGetUpHandler.AbleToStand, able.Drained.GetUpHandler,
        "rainbow command five installs able timer handler");

    // The handler survives the later controller-four pose change. It accepts Up only from
    // left drained `$E9` at frame >=8, writes 13/1, and replaces itself with RTS.
    able.Drained.PutCrouchingOrFalling(able);
    AssertTrue(!able.Drained.StepGetUpHandler(able, newlyPressedInput: 0),
        "able timer handler ignores absent Up edge");
    AssertTrue(able.Drained.StepGetUpHandler(able, newlyPressedInput: SnesButton.Up),
        "able timer handler accepts Up from E9 frame eight");
    AssertEqual(13, able.AnimationFrame, "able timer handler selects stand-up frame thirteen");
    AssertEqual(1, able.AnimationFrameTimer.RawValue, "able timer handler selects one-tick timer");
    AssertEqual(DrainedGetUpHandler.Inactive, able.Drained.GetUpHandler,
        "able timer handler replaces itself with RTS");

    var unable = new SamusState
    {
        Pose = SamusPoseId.FacingRightNormalPose,
        Position = FixedPosition<RoomPixels>.FromWhole(new(48, 75)),
    };
    SamusPoseMetadata.RefreshCollisionRadii(unable);
    SamusPresentation.InitializeAnimation(unable);
    unable.Drained.SetupForRainbowBeamUnableToStand(unable);
    unable.Drained.PutCrouchingOrFalling(unable);
    AssertEqual(DrainedGetUpHandler.UnableToStand, unable.Drained.GetUpHandler,
        "rainbow command $18 installs failed-stand timer handler");
    AssertTrue(unable.Drained.StepGetUpHandler(unable, newlyPressedInput: SnesButton.Up),
        "failed-stand handler accepts Up only inside frames eight through eleven");
    AssertEqual(18, unable.AnimationFrame, "failed-stand handler selects frame eighteen");
    AssertEqual(DrainedGetUpHandler.UnableToStand, unable.Drained.GetUpHandler,
        "failed-stand handler remains installed");

        // The two later cutscene commands are direct animation writes. They do not select a
        // new pose or reinitialize a delay stream.
        SamusDrainedState.FreezeForHyperBeamAcquisition(unable);
    AssertEqual(28, unable.AnimationFrame, "command $19 freezes drained art at frame $1C");
    AssertEqual(1, unable.AnimationFrameTimer.RawValue, "command $19 freeze timer");
    // Command `$16` starts negative-super-special palette zero with one-call cadence. After
    // the Baby raises its delay to two, index two must remain visible for two calls before
    // advancing. Each call loads before decrementing, matching `$91:D96F-$D997` ordering.
    unable.Drained.PresentationColors = rainbowColors;
    unable.Drained.EnableRainbow(unable);
    AssertTrue(unable.Drained.UpdatePalette(drainedCgram.Samus, unable.EquippedItems, suitColors),
        "rainbow handler owns palette dispatcher");
    AssertEqual(0x1000, drainedCgram.Colors[192],
        "rainbow first call loads Hyper Beam palette zero");
    AssertEqual(1, unable.Drained.ChargePaletteIndex,
        "one-call rainbow cadence advances immediately");
    unable.Drained.IncrementRainbowPaletteFrame(maximumFrame: 10);
    unable.Drained.UpdatePalette(drainedCgram.Samus, unable.EquippedItems, suitColors);
    AssertEqual(2, unable.Drained.CommonPaletteTimer.RawValue,
        "Baby-raised rainbow delay reloads two");
    AssertEqual(2, unable.Drained.ChargePaletteIndex,
        "rainbow second palette advances into delayed index");
    unable.Drained.UpdatePalette(drainedCgram.Samus, unable.EquippedItems, suitColors);
    AssertEqual(0x1040, drainedCgram.Colors[192],
        "rainbow delayed index loads before timer decrement");
    AssertEqual(2, unable.Drained.ChargePaletteIndex,
        "rainbow delay holds palette index for first call");
    unable.Drained.UpdatePalette(drainedCgram.Samus, unable.EquippedItems, suitColors);
    AssertEqual(3, unable.Drained.ChargePaletteIndex,
        "rainbow delay advances on second call");

    unable.Drained.DisableRainbowAndStartStandingAnimation(unable);
    AssertEqual(13, unable.AnimationFrame, "command $17 resumes standing animation at frame thirteen");
    AssertEqual(1, unable.AnimationFrameTimer.RawValue, "command $17 resume timer");
    AssertTrue(unable.Drained.UpdatePalette(drainedCgram.Samus, unable.EquippedItems, suitColors),
        "command $17 restores selected suit palette");
    AssertEqual(0x3000, drainedCgram.Colors[192],
        "command $17 replaces rainbow palette immediately");

    Console.WriteLine("  Drained Samus: rainbow/body and Hyper Beam projectile palettes, controllers, fall, and releases agree.");
}

/// <summary>
/// Observes the rounded whole-pixel broad-phase target that
/// <see cref="SamusSolidEnemyCollision.Probe"/> builds at `$A0:A90A-$A0:A9B7`. The probe
/// does not publish that target, so it is recovered from collision outcomes: a solid enemy
/// with one-pixel radii is swept over a grid around Samus. Strict overlap makes the
/// colliding region span target ± Samus radius, clipped on the movement axis by the
/// directional gap test, so the far edge locates the movement-axis target and the midpoint
/// locates the perpendicular one. Each probe uses a copy of Samus because exact contact
/// clears her Y subposition.
/// </summary>
static MotherBrainRinkaDoorSpawnExpected ObserveSolidEnemyProbeTarget(
    SamusKinematicsState samus,
    SamusCollisionDirection direction,
    ushort distance,
    ushort distanceSubposition)
{
    const int sweepRadius = 48;
    int minX = int.MaxValue, maxX = int.MinValue, minY = int.MaxValue, maxY = int.MinValue;
    for (int dy = -sweepRadius; dy <= sweepRadius; dy++)
    {
        for (int dx = -sweepRadius; dx <= sweepRadius; dx++)
        {
            int enemyX = samus.Position.X.Whole + dx;
            int enemyY = samus.Position.Y.Whole + dy;
            var probeState = new SamusKinematicsState
            {
                Position = new(new(samus.Position.X.Whole, samus.Position.X.Fraction), new(samus.Position.Y.Whole, samus.Position.Y.Fraction)),
                Radius = new(samus.Radius.X, samus.Radius.Y),
            };
            var enemy = new SolidEnemyCollisionBody(
                Slot: 1, Index: 0x0040, Position: new(unchecked((ushort)enemyX), unchecked((ushort)enemyY)),
                Radius: new(1, 1), Solid: true);
            if (!SamusSolidEnemyCollision.Probe(
                    probeState, [enemy], direction, distance, distanceSubposition).Collided)
                continue;

            minX = Math.Min(minX, enemyX);
            maxX = Math.Max(maxX, enemyX);
            minY = Math.Min(minY, enemyY);
            maxY = Math.Max(maxY, enemyY);
        }
    }

    if (maxX == int.MinValue)
        throw new InvalidOperationException(
            $"Solid-enemy probe {direction} never collided within the target sweep.");

    int targetX = direction switch
    {
        SamusCollisionDirection.Right => maxX - samus.Radius.X,
        SamusCollisionDirection.Left => minX + samus.Radius.X,
        SamusCollisionDirection.Up or
            SamusCollisionDirection.Down or
            SamusCollisionDirection.NonDirectionalProbe => (minX + maxX) / 2,
        _ => (minX + maxX) / 2,
    };
    int targetY = direction switch
    {
        SamusCollisionDirection.Down => maxY - samus.Radius.Y,
        SamusCollisionDirection.Up => minY + samus.Radius.Y,
        SamusCollisionDirection.Left or
            SamusCollisionDirection.Right or
            SamusCollisionDirection.NonDirectionalProbe => (minY + maxY) / 2,
        _ => (minY + maxY) / 2,
    };
    return new MotherBrainRinkaDoorSpawnExpected(unchecked((ushort)targetX), unchecked((ushort)targetY));
}

/// <summary>
/// Exercises the shared bank-$A0 enemy probe independently of room blocks. The fixtures are
/// intentionally synthetic: no translated room currently owns a live enemy actor list, and
/// silently substituting terrain or decorative sprites would not test the native routine.
/// </summary>
internal static void VerifySamusSolidEnemyCollision()
{
    var samus = new SamusKinematicsState
    {
        Position = new(new(100, 0), new(100, 0x7777)),
        Radius = new(5, 10),
    };

    // The no-enemy path returns A=0 and leaves Samus completely untouched.
    SolidEnemyCollisionResult empty = SamusSolidEnemyCollision.Probe(
        samus, [], SamusCollisionDirection.Right, distance: 3, distanceSubposition: 0x4000);
    AssertTrue(!empty.Collided, "empty interactive-enemy list does not collide");
    AssertEqual(0x7777, samus.Position.Y.Fraction, "no collision preserves Y subposition");
    AssertEqual(104, ObserveSolidEnemyProbeTarget(
            samus, SamusCollisionDirection.Right, distance: 3, distanceSubposition: 0x4000).X,
        "right fractional target rounds outward");

    // `$A0:A90A-$A0:A9B7` has asymmetric-looking but literal carry/borrow rounding. Test all
    // four jump-table entries, including fractional underflow and overflow, so a later
    // refactor cannot replace this with ordinary truncation or Math.Round.
    MotherBrainRinkaDoorSpawnExpected left = ObserveSolidEnemyProbeTarget(
        samus, SamusCollisionDirection.Left, distance: 0, distanceSubposition: 0x8000);
    AssertEqual(98, left.X, "left fractional borrow plus outward decrement");
    AssertEqual(100, left.Y, "left probe preserves target Y");

    samus.Position = samus.Position.WithFractionX(0xf000);
    MotherBrainRinkaDoorSpawnExpected rightCarry = ObserveSolidEnemyProbeTarget(
        samus, SamusCollisionDirection.Right, distance: 0, distanceSubposition: 0x2000);
    AssertEqual(102, rightCarry.X, "right fractional carry plus outward increment");

    samus.Position = samus.Position.WithFractionY(0);
    MotherBrainRinkaDoorSpawnExpected up = ObserveSolidEnemyProbeTarget(
        samus, SamusCollisionDirection.Up, distance: 1, distanceSubposition: 0x8000);
    AssertEqual(97, up.Y, "up target shares negative-direction rounding");

    samus.Position = samus.Position.WithFractionY(0xf000);
    MotherBrainRinkaDoorSpawnExpected down = ObserveSolidEnemyProbeTarget(
        samus, SamusCollisionDirection.Down, distance: 1, distanceSubposition: 0x2000);
    AssertEqual(103, down.Y, "down target shares positive-direction rounding");

    // #1269: after a carry the INC sets Z from the target word, so `$A0:A9A7` rounds a
    // second pixel even when the carried fraction is exactly zero.
    samus.Position = samus.Position.WithFractionY(0x9800);
    MotherBrainRinkaDoorSpawnExpected exactCarry = ObserveSolidEnemyProbeTarget(
        samus, SamusCollisionDirection.Down, distance: 2, distanceSubposition: 0x6800);
    AssertEqual(104, exactCarry.Y, "an exact fractional carry still rounds two pixels outward");

    // The 100% movie's Mother Brain room: Samus falls onto a frozen Rinka whose top is three
    // pixels below her; only the doubled rounding brings the future boxes into overlap.
    var falling = new SamusKinematicsState
    {
        Position = new(new(0x00c5, 0), new(0x0085, 0x9800)), Radius = new(5, 0x13),
    };
    var frozenRinka = new SolidEnemyCollisionBody(
        Slot: 3, Index: 0x00c0, Position: new(0x00c8, 0x00a3), Radius: new(8, 8),
        Solid: true);
    SolidEnemyCollisionResult landing = SamusSolidEnemyCollision.Probe(
        falling, [frozenRinka], SamusCollisionDirection.Down, distance: 2, distanceSubposition: 0x6800);
    AssertTrue(landing.Collided, "a falling Samus lands on the frozen Rinka");
    AssertEqual(3, landing.Distance, "the landing stops at the Rinka's top edge");

    samus.Position = samus.Position.WithFractionX(0).WithFractionY(0x7777);
    var decorative = new SolidEnemyCollisionBody(
        Slot: 1, Index: 0x0040, Position: new(110, 100), Radius: new(5, 5),
        Solid: false);
    SolidEnemyCollisionResult ignored = SamusSolidEnemyCollision.Probe(
        samus, [decorative], SamusCollisionDirection.Right, distance: 2, distanceSubposition: 0);
    AssertTrue(!ignored.Collided, "non-solid unfrozen enemy is ignored");

    // A frozen enemy is eligible even without property $8000. With a one-pixel current gap,
    // the future box overlaps and the routine clips the requested distance to exactly one.
    SolidEnemyCollisionBody frozen = decorative with
    {
        Slot = 2,
        Index = 0x0080,
        Position = decorative.Position.WithX(111),
        Solid = true,
    };
    SolidEnemyCollisionResult frozenHit = SamusSolidEnemyCollision.Probe(
        samus, [frozen], SamusCollisionDirection.Right, distance: 2, distanceSubposition: 0);
    AssertTrue(frozenHit.Collided, "frozen enemy is solid to Samus");
    AssertEqual(1, frozenHit.Distance, "right collision publishes current edge gap");
    AssertEqual(0x0080, frozenHit.EnemyIndex, "collision publishes native enemy index");
    AssertTrue(!frozenHit.WasTouching, "positive gap is not reported as touching");
    AssertEqual(0x7777, samus.Position.Y.Fraction, "positive-gap collision preserves Samus subposition");

    // Property bit 15 takes the other eligibility route. Exact contact reaches `$A0:AAC8`,
    // whose STZ $0AFC bug clears Y subposition even though this is a horizontal probe.
    SolidEnemyCollisionBody solidTouch = decorative with
    {
        Slot = 3,
        Index = 0x00c0,
        Solid = true,
    };
    SolidEnemyCollisionResult touching = SamusSolidEnemyCollision.Probe(
        samus, [solidTouch], SamusCollisionDirection.Right, distance: 1, distanceSubposition: 0);
    AssertTrue(touching.Collided && touching.WasTouching, "zero-gap solid enemy takes touching path");
    AssertEqual(0, touching.Distance, "touching collision publishes zero distance");
    AssertEqual(0, samus.Position.Y.Fraction, "horizontal enemy touch preserves native Y-subposition bug");

    // The future broad-phase test is strict. Boxes that merely touch at their radii sum do
    // not advance to the directional gap test.
    SolidEnemyCollisionBody tangent = solidTouch with { Position = solidTouch.Position.WithX(110) };
    SolidEnemyCollisionResult tangentMiss = SamusSolidEnemyCollision.Probe(
        samus, [tangent], SamusCollisionDirection.Right, distance: 0, distanceSubposition: 0);
    AssertTrue(!tangentMiss.Collided, "future box tangency is not broad-phase overlap");

    // A negative directional gap is a signed/BPL rejection: Samus already overlaps this
    // enemy, so the routine must not trap her inside it or manufacture distance zero.
    SolidEnemyCollisionBody embedded = solidTouch with { Position = solidTouch.Position.WithX(109) };
    SolidEnemyCollisionResult embeddedMiss = SamusSolidEnemyCollision.Probe(
        samus, [embedded], SamusCollisionDirection.Right, distance: 2, distanceSubposition: 0);
    AssertTrue(!embeddedMiss.Collided, "enemy already intersecting movement axis is skipped");

    // Broad overlap must succeed on the perpendicular axis as well.
    SolidEnemyCollisionBody verticalMiss = frozen with { Position = frozen.Position.WithY(116) };
    SolidEnemyCollisionResult perpendicularMiss = SamusSolidEnemyCollision.Probe(
        samus, [verticalMiss], SamusCollisionDirection.Right, distance: 2, distanceSubposition: 0);
    AssertTrue(!perpendicularMiss.Collided, "perpendicular separation rejects directional candidate");

    // Native list order wins. The first entry is farther away than the second but is still
    // returned as soon as both its broad and directional tests pass.
    SolidEnemyCollisionBody fartherFirst = solidTouch with { Index = 0x0100, Position = solidTouch.Position.WithX(120) };
    SolidEnemyCollisionBody nearerSecond = solidTouch with { Index = 0x0140, Position = solidTouch.Position.WithX(112) };
    SolidEnemyCollisionResult ordered = SamusSolidEnemyCollision.Probe(
        samus, [fartherFirst, nearerSecond], SamusCollisionDirection.Right,
        distance: 20, distanceSubposition: 0);
    AssertEqual(0x0100, ordered.EnemyIndex, "first interactive collision wins over nearest geometry");
    AssertEqual(10, ordered.Distance, "first list entry publishes its own gap");

    // Exercise both vertical directional formulas rather than relying only on target tests.
    SolidEnemyCollisionBody above = solidTouch with
    {
        Index = 0x0180,
        Position = new(100, 84),
        Radius = new(5, 5),
    };
    SolidEnemyCollisionResult ceiling = SamusSolidEnemyCollision.Probe(
        samus, [above], SamusCollisionDirection.Up, distance: 2, distanceSubposition: 0);
    AssertTrue(ceiling.Collided, "upward solid-enemy collision detected");
    AssertEqual(1, ceiling.Distance, "upward collision publishes top-to-bottom gap");

    SolidEnemyCollisionBody below = above with { Index = 0x01c0, Position = above.Position.WithY(116) };
    SolidEnemyCollisionResult floor = SamusSolidEnemyCollision.Probe(
        samus, [below], SamusCollisionDirection.Down, distance: 2, distanceSubposition: 0);
    AssertTrue(floor.Collided, "downward solid-enemy collision detected");
    AssertEqual(1, floor.Distance, "downward collision publishes bottom-to-top gap");

    // Ordinary managed movement now preserves the native wrapper order: enemy probe first,
    // room blocks only on a miss, then position addition. An all-air room isolates that seam.
    const int roomWidth = 8;
    const int roomHeight = 8;
    RoomLevelData airRoom = CreateEmptyRoom(new(roomWidth, roomHeight));
    var integrated = new SamusKinematicsState
    {
        Position = FixedPosition<RoomPixels>.FromWhole(new(100, 100)),
        Radius = new(5, 10),
        InteractiveEnemies = [frozen],
    };
    BlockMoveResult integratedHorizontal = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = airRoom }, integrated, null, new HorizontalBlockMove { Displacement = 2 << 16 });
    AssertTrue(integratedHorizontal.Collided, "ordinary horizontal mover reports enemy collision");
    AssertEqual(101, integrated.Position.X.Whole, "ordinary horizontal mover clips to enemy gap");
    AssertTrue(integratedHorizontal.EnemyCollision is not null,
        "enemy collision, not an invented terrain block, stops the ordinary mover");
    AssertEqual(
        0x0080,
        integratedHorizontal.EnemyCollision?.EnemyIndex,
        "ordinary mover retains colliding enemy identity");

    integrated.Position = integrated.Position.WithWholeY(100);
    integrated.InteractiveEnemies = [below];
    BlockMoveResult integratedVertical = SamusBlockCollision.MoveVertical(new SamusTerrain { Level = airRoom }, integrated, null, new VerticalBlockMove { Displacement = 2 << 16, ScanLeftToRight = true });
    AssertTrue(integratedVertical.Collided, "ordinary vertical mover reports enemy collision");
    AssertEqual(101, integrated.Position.Y.Whole, "ordinary vertical mover clips to enemy gap");
    AssertEqual(0x01c0, integratedVertical.EnemyCollision?.EnemyIndex,
        "vertical mover retains colliding enemy identity");

    // Wall-jump probing copies Samus so it cannot commit movement. The one exception is the
    // original `$A0:AAC8` touching bug, which still clears the real Y subposition.
    integrated.Position = integrated.Position.WithWholeX(100).WithWholeY(100).WithFractionY(0x4321);
    integrated.InteractiveEnemies = [solidTouch];
    BlockMoveResult wallProbe = SamusBlockCollision.ProbeWallHorizontal(
airRoom, integrated, null, signedDistance: 1 << 16);
    AssertTrue(wallProbe.EnemyCollision is { WasTouching: true },
        "wall probe detects touching enemy before blocks");
    AssertEqual(100, integrated.Position.X.Whole, "wall probe does not commit X motion");
    AssertEqual(0, integrated.Position.Y.Fraction, "wall probe commits only native touching side effect");

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
internal static void VerifySamusGrabbedByDraygon()
{
    var bus = new TestAddressSpace();

    SamusPoseId[] leftPoses = [
        SamusPoseId.DraygonGrabbedNeutralLeftPose,
        SamusPoseId.DraygonGrabbedAimUpLeftPose,
        SamusPoseId.DraygonGrabbedFiringLeftPose,
        SamusPoseId.DraygonGrabbedAimDownLeftPose,
        SamusPoseId.DraygonGrabbedMovingLeftPose,
    ];
    SamusPoseId[] rightPoses = [
        SamusPoseId.DraygonGrabbedNeutralRightPose,
        SamusPoseId.DraygonGrabbedAimUpRightPose,
        SamusPoseId.DraygonGrabbedFiringRightPose,
        SamusPoseId.DraygonGrabbedAimDownRightPose,
        SamusPoseId.DraygonGrabbedMovingRightPose,
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
        WritePoseDefinition(bus, (int)leftPoses[index], leftDefinitions[index]);
        WritePoseDefinition(bus, (int)rightPoses[index], rightDefinitions[index]);

        // `$BA-$BD/$EC-$EF` use stationary `$B2B4`; `$BE/$F0` use six-frame `$B53C`.
        ushort leftDelay = index == 4 ? (ushort)0xb53c : (ushort)0xb2b4;
        ushort rightDelay = index == 4 ? (ushort)0xb53c : (ushort)0xb2b4;
        WritePoseAnimationDelayPointer(bus, leftPoses[index], leftDelay);
        WritePoseAnimationDelayPointer(bus, rightPoses[index], rightDelay);

        // All five poses on a side point to the same held-input transition program.
        SyntheticSamusCartridge.WritePoseTransitionPointer(bus, leftPoses[index], 0xae18);
        SyntheticSamusCartridge.WritePoseTransitionPointer(bus, rightPoses[index], 0xae56);
    }

    // Ordinary release destinations need real metadata and a harmless initial delay.
    WritePoseDefinition(bus, (int)SamusPoseId.FacingRightNormalPose,
        [0x08, 0x00, 0xff, 0x02, 0x06, 0x00, 0x15, 0x00]);
    WritePoseDefinition(bus, (int)SamusPoseId.FacingLeftNormalPose,
        [0x04, 0x00, 0xff, 0x07, 0x06, 0x00, 0x15, 0x00]);
    WritePoseAnimationDelayPointer(bus, SamusPoseId.FacingRightNormalPose, 0xc000);
    WritePoseAnimationDelayPointer(bus, SamusPoseId.FacingLeftNormalPose, 0xc001);
    WriteAnimationDelayStream(bus, 0xb2b4, [0x10, 0xff]);
    WriteAnimationDelayStream(bus, 0xb53c, [0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0xff]);
    WriteAnimationDelayStream(bus, 0xc000, [0x0a]);
    WriteAnimationDelayStream(bus, 0xc001, [0x0a]);

    SyntheticSamusCartridge.WritePoseTransitionList(bus, 0xae18, [
        new SyntheticSamusCartridgeRecord(0x0000, 0x0a40, 0x00bb), new SyntheticSamusCartridgeRecord(0x0000, 0x0640, 0x00bd), new SyntheticSamusCartridgeRecord(0x0000, 0x0240, 0x00bc),
        new SyntheticSamusCartridgeRecord(0x0000, 0x0010, 0x00bb), new SyntheticSamusCartridgeRecord(0x0000, 0x0020, 0x00bd), new SyntheticSamusCartridgeRecord(0x0000, 0x0040, 0x00bc),
        new SyntheticSamusCartridgeRecord(0x0000, 0x0200, 0x00be), new SyntheticSamusCartridgeRecord(0x0000, 0x0100, 0x00be), new SyntheticSamusCartridgeRecord(0x0000, 0x0800, 0x00be),
        new SyntheticSamusCartridgeRecord(0x0000, 0x0400, 0x00be),
    ]);
    SyntheticSamusCartridge.WritePoseTransitionList(bus, 0xae56, [
        new SyntheticSamusCartridgeRecord(0x0000, 0x0940, 0x00ed), new SyntheticSamusCartridgeRecord(0x0000, 0x0540, 0x00ef), new SyntheticSamusCartridgeRecord(0x0000, 0x0140, 0x00ee),
        new SyntheticSamusCartridgeRecord(0x0000, 0x0010, 0x00ed), new SyntheticSamusCartridgeRecord(0x0000, 0x0020, 0x00ef), new SyntheticSamusCartridgeRecord(0x0000, 0x0040, 0x00ee),
        new SyntheticSamusCartridgeRecord(0x0000, 0x0200, 0x00f0), new SyntheticSamusCartridgeRecord(0x0000, 0x0100, 0x00f0), new SyntheticSamusCartridgeRecord(0x0000, 0x0800, 0x00f0),
        new SyntheticSamusCartridgeRecord(0x0000, 0x0400, 0x00f0),
    ]);

    foreach (bool facingRight in new[] { false, true })
    {
        var historySamus = new SamusState();
        var history = historySamus.PoseHistory;
        history.PreviousPose = SamusPoseId.SpinJumpRightPose;
        history.PreviousDirectionAndMovement = 0x0308;
        history.LastDifferentPose = SamusPoseId.WallJumpLeftPose;
        history.LastDifferentDirectionAndMovement = 0x1404;
        SamusPoseId grabbedPose = facingRight ? SamusPoseId.DraygonGrabbedNeutralRightPose : SamusPoseId.DraygonGrabbedNeutralLeftPose;
        ushort grabbedMetadata = (ushort)(facingRight ? 0x1a08 : 0x1a04);
        historySamus.DraygonGrabbed.Begin(historySamus, facingRight);
        AssertEqual(SamusPoseId.SpinJumpRightPose, history.LastDifferentPose, "Draygon grab shifts previous pose");
        AssertEqual(0x0308, history.LastDifferentDirectionAndMovement, "Draygon grab shifts previous metadata");
        AssertEqual(grabbedPose, history.PreviousPose, "Draygon grab commits grabbed pose");
        AssertEqual(grabbedMetadata, history.PreviousDirectionAndMovement, "Draygon grab commits grabbed metadata");
        historySamus.DraygonGrabbed.Begin(historySamus, facingRight);
        AssertEqual(grabbedPose, history.LastDifferentPose, "repeated Draygon grab still shifts history");
        AssertEqual(grabbedMetadata, history.LastDifferentDirectionAndMovement, "repeated grab shifts metadata");
        historySamus.DraygonGrabbed.Release(historySamus);
        AssertEqual(grabbedPose, history.LastDifferentPose, "Draygon release retains prior grabbed pose");
        AssertEqual(grabbedMetadata, history.LastDifferentDirectionAndMovement, "Draygon release retains prior grabbed metadata");
        AssertEqual(historySamus.Pose, history.PreviousPose, "Draygon release commits standing pose");
        AssertEqual(facingRight ? 8 : 4, history.PreviousDirectionAndMovement, "Draygon release commits standing metadata");
        AssertTrue(!history.AllowsWallJumpProbe, "Draygon release cannot retain pre-grab spin eligibility");
    }

    var samus = new SamusState { Position = FixedPosition<RoomPixels>.FromWhole(new(0x0080, 0x0100)) };
    samus.DraygonGrabbed.Begin(samus, draygonFacingRight: true);
    AssertEqual(SamusPoseId.DraygonGrabbedNeutralRightPose, samus.Pose,
        "right-facing Draygon entry selects $EC");
    AssertEqual(21, samus.Kinematics.Radius.Y, "Draygon entry loads radius 21");

    samus.DraygonGrabbed.ApplyOwnerPosition(
        samus, ownerPosition: new(0x0100, 0x0180), draygonFacingRight: true);
    AssertEqual(8, samus.Position.X.Whole - samus.DraygonGrabbed.OwnerPosition.X,
        "right-facing claw offset is +8");
    AssertEqual(0x0108, samus.Position.X.Whole, "right-facing owner placement X");
    AssertEqual(0x01a8, samus.Position.Y.Whole, "owner placement Y is body plus $28");

    samus.SolidVerticalCollisionResult = SamusVerticalCollisionResult.WallJump;
    SamusDraygonGrabbedState.StepMovement(samus);
    AssertEqual(SamusVerticalCollisionResult.None, samus.SolidVerticalCollisionResult,
        "type-$1A performs its sole STZ side effect on the stale vertical collision word");
    AssertEqual(0x0108, samus.Position.X.Whole, "type-$1A does not move X");
    AssertEqual(0x01a8, samus.Position.Y.Whole, "type-$1A does not move Y");

    // The larger right+up+shoot chord must select the first `$AE56` record (`$ED`), not
    // the later generic shoot record (`$EE`), proving ROM priority rather than host rules.
    SamusPoseTransition rightUpShoot = (SamusPoseTransitionTable.Find(
        samus.Pose, canonicalHeldInput: SnesButton.Up | SnesButton.Right | SnesButton.X, canonicalNewInput: 0) ?? throw new InvalidOperationException("Verification requires SamusPoseTransitionTable.Find( bus, samus.Pose, canonicalHeldInput: 0x0940, canonicalNe...."));
    AssertEqual(SamusPoseId.DraygonGrabbedAimUpRightPose,
        rightUpShoot.ProspectivePose, "right grabbed transition priority");
    SamusPoseMetadata.ApplyDraygonGrabbedPoseChange(samus, rightUpShoot.ProspectivePose);
    AssertEqual(SamusPoseId.DraygonGrabbedAimUpRightPose, samus.Pose,
        "right grabbed aim transition applies");
    AssertEqual(SamusPoseId.DraygonGrabbedNeutralRightPose,
        SamusPoseMetadata.ReadNoInputFallbackPose(samus), "right grabbed aim fallback is $EC");
    SamusPoseMetadata.ApplyDraygonGrabbedPoseChange(samus, SamusPoseMetadata.ReadNoInputFallbackPose(samus));
    AssertThrows<InvalidOperationException>(
        () => SamusPoseMetadata.ApplyDraygonGrabbedPoseChange(samus, 
            SamusPoseId.DraygonGrabbedNeutralLeftPose),
        "Draygon input table cannot cross owner-controlled facing families");
    AssertEqual(SamusPoseId.DraygonGrabbedNeutralRightPose, samus.Pose,
        "rejected cross-family Draygon input retains current pose");

    DraygonEscapeResult locked = samus.DraygonGrabbed.StepEscapeHandler(
        samus, newlyPressedInput: 0, grappleLockedInPlace: true);
    AssertTrue(locked.SuppressProspectivePose, "locked grapple suppresses grabbed pose transition");
    AssertEqual(0, samus.DraygonGrabbed.EscapeButtonCounter, "no D-pad edge does not count");

    samus.DraygonGrabbed.StepEscapeHandler(
        samus, newlyPressedInput: SnesButton.Up, grappleLockedInPlace: false);
    AssertEqual(1, samus.DraygonGrabbed.EscapeButtonCounter,
        "first Up edge increments escape counter");
    DraygonEscapeResult repeatedUp = samus.DraygonGrabbed.StepEscapeHandler(
        samus, newlyPressedInput: SnesButton.Up, grappleLockedInPlace: false);
    AssertEqual(1, samus.DraygonGrabbed.EscapeButtonCounter,
        "repeated D-pad pattern is rejected and leaves escape counter unchanged");

    samus.HorizontalSpeed.BaseSpeed = new SnesUnsignedSixteenSixteen(3, 0x4000);
    samus.HorizontalSpeed.ExtraRunSpeed = new SnesUnsignedSixteenSixteen(2, 0x8000);
    samus.HorizontalSpeed.AccelerationMode = SamusAccelerationMode.Decelerating;
    samus.Kinematics.YSpeed = new SnesUnsignedSixteenSixteen(4, 0xc000);
    samus.Kinematics.YDirection = SamusVerticalDirection.Down;
    samus.MorphBallBounceState = SamusBallBounceState.SecondRebound;

    DraygonEscapeResult release = repeatedUp;
    for (int inputNumber = 1; inputNumber < SamusDraygonGrabbedState.EscapeButtonCounterTarget; inputNumber++)
    {
        SnesButton direction = (inputNumber & 1) != 0 ? SnesButton.Down : SnesButton.Up;
        release = samus.DraygonGrabbed.StepEscapeHandler(
            samus, direction, grappleLockedInPlace: false);
    }
    AssertTrue(release.Released, "sixtieth alternating D-pad input releases Samus");
    AssertEqual(SamusPoseId.FacingRightNormalPose, samus.Pose,
        "right grabbed family releases to pose $01");
    AssertEqual(0, samus.HorizontalSpeed.BaseSpeed.Whole, "release clears base X speed");
    AssertEqual(0, samus.HorizontalSpeed.BaseSpeed.Fraction, "release clears base X subspeed");
    AssertEqual(2, samus.HorizontalSpeed.ExtraRunSpeed.Whole,
        "release intentionally preserves extra run speed");
    AssertEqual(0x8000, samus.HorizontalSpeed.ExtraRunSpeed.Fraction,
        "release intentionally preserves extra run subspeed");
    AssertEqual(0, samus.Kinematics.YSpeed.Whole, "release clears Y speed");
    AssertEqual(0, samus.Kinematics.YSpeed.Fraction, "release clears Y subspeed");
    AssertEqual(SamusVerticalDirection.None, samus.Kinematics.YDirection, "release clears Y direction");
    AssertEqual(SamusBallBounceState.None, samus.MorphBallBounceState, "release clears bounce state");
    AssertEqual(SamusAccelerationMode.Accelerating, samus.HorizontalSpeed.AccelerationMode,
        "release clears X acceleration mode");
    AssertTrue(samus.DraygonGrabbed.ConsumeOwnerReleaseSignal(),
        "release publishes one owner-consumed bit");
    AssertTrue(!samus.DraygonGrabbed.ConsumeOwnerReleaseSignal(),
        "owner release bit is one-shot");

    // Mirror the entry/owner/release direction without repeating the 60-input route.
    samus.DraygonGrabbed.Begin(samus, draygonFacingRight: false);
    samus.DraygonGrabbed.ApplyOwnerPosition(
        samus, ownerPosition: new(0x0100, 0x0180), draygonFacingRight: false);
    AssertEqual(-8, samus.Position.X.Whole - samus.DraygonGrabbed.OwnerPosition.X,
        "left-facing claw offset is -8");
    AssertEqual(0x00f8, samus.Position.X.Whole, "left-facing owner placement X");
    samus.DraygonGrabbed.Release(samus);
    AssertEqual(SamusPoseId.FacingLeftNormalPose, samus.Pose,
        "left grabbed family releases to pose $02");

    Console.WriteLine("  Samus/Draygon: ten poses, owner offsets, transitions, escape, and release agree.");
}

/// <summary>
/// Seeds the <c>$91:D99E</c> Hyper Beam suit-palette pointer table, its ten bank-$9B
/// palettes, and the normal Power Suit palette with distinct synthetic colors.
/// </summary>
[CpuBusBoundary("exact bus test: synthetic cartridge fixture for Hyper Beam suit palettes")]
static void InstallHyperBeamSuitPaletteFixture(TestAddressSpace bus)
{
    // `$91:D99E` points at ten complete Hyper Beam palettes in reverse-numbered order.
    // Distinct synthetic words expose both the pointer index and all-$20-byte copy. The
    // The compiled Power Suit entry at `$91:D727` independently verifies
    // controller-zero/command-$17 restoration rather than retaining rainbow colors.
    for (ushort palette = 0; palette < 10; palette++)
    {
        ushort pointer = unchecked((ushort)SamusPaletteRomData.FullBodyCycles.HyperBeamPaletteSource(palette));
        WriteTestWord(bus, 0x91d99e + palette * 2, pointer);
        for (ushort color = 0; color < 16; color++)
        {
            WriteTestWord(
                bus,
                0x9b0000 | (pointer + color * 2),
                unchecked((ushort)(0x1000 + palette * 0x20 + color)));
        }
    }
    for (ushort color = 0; color < 16; color++)
        WriteTestWord(bus, SamusPaletteRomData.Banks.Palette |
            (SamusPaletteRomData.Common.NormalSuitPalettePointer(0) + color * 2),
            unchecked((ushort)(0x3000 + color)));
}

/// <summary>
/// Seeds the bank-$8D Hyper Beam projectile-palette program at <c>$8D:E1F0</c>: ten
/// two-call records of eight synthetic colors and the looping tail.
/// </summary>
[CpuBusBoundary("exact bus test: synthetic cartridge fixture for the Hyper Beam projectile-palette program")]
static void InstallHyperBeamProjectilePaletteProgramFixture(TestAddressSpace bus)
{
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
}

    /// <summary>Runs the focused bank-$A0 solid-enemy collision probe fixture.</summary>
    [VerificationCommand("--samus-solid-enemy-collision")]
    internal static int SamusSolidEnemyCollisionCommand()
    {
        VerifySamusSolidEnemyCollision();
        Console.WriteLine("Samus solid-enemy collision: native target rounding, eligibility and edge clipping passed.");
        return 0;
    }
}
