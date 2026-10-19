using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core;
namespace SuperMetroid.Verification.Samus.Abilities;
using SuperMetroid.Verification.Harness;

/// <summary>Checks stored shine, launch, flight, crash, and echo phases against native state and palette behavior.</summary>
internal static class SamusShinesparkVerification
{

/// <summary>
/// Seeds the six compiled stored-shine phase targets with distinct sentinel colors, making an
/// incorrect suit or phase selection immediately observable, plus a distinct first Power Suit color.
/// </summary>
[CpuBusBoundary("exact bus test: synthetic cartridge fixture for the stored-shine and Power Suit palettes")]
static void SeedStoredShinePalettes(TestAddressSpace bus)
{
    for (int frame = 0; frame < 6; frame++)
    {
        AssertTrue(SamusPaletteRomData.FullBodyCycles.TryStoredShinePalettePointer(
            0, unchecked((ushort)(frame * 2)), out ushort pointer),
            $"compiled Power stored-shine phase {frame} exists");
        WriteTestWord(bus, SamusPaletteRomData.Banks.Palette | pointer,
            unchecked((ushort)(0x1100 + frame)));
    }
    WriteTestWord(bus, SamusRenderingRomData.Body.PowerSuitPalette, 0x0321);
}

/// <summary>Checks shine storage and palette selection through launch, movement, collision, crash, echoes, and return to standing.</summary>
[PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
internal static void VerifySamusStoredShineAndShinespark()
{
    var bus = new TestAddressSpace();

    // These eight pose records contain only fields consumed by this isolated state test:
    // direction, movement type `$1B`, and collision radius. Animation still follows the
    // normal live pointer table, proving special movement does not bypass cartridge art.
    foreach (SamusPoseId pose in new SamusPoseId[]
    {
        SamusPoseId.ShinesparkWindupRightPose,
        SamusPoseId.ShinesparkWindupLeftPose,
        SamusPoseId.ShinesparkHorizontalRightPose,
        SamusPoseId.ShinesparkHorizontalLeftPose,
        SamusPoseId.ShinesparkVerticalRightPose,
        SamusPoseId.ShinesparkVerticalLeftPose,
        SamusPoseId.ShinesparkDiagonalRightPose,
        SamusPoseId.ShinesparkDiagonalLeftPose,
    })
    {
        WritePoseDefinition(bus, (int)pose,
            [(byte)(((int)pose & 1) != 0 ? 0x08 : 0x04), 0x1b, 0xff, 0x02, 0x00, 0x00, 0x13, 0x00]);
        WritePoseAnimationDelays(bus, pose, unchecked((ushort)(0xc600 + (int)pose)), [4]);
    }
    WritePoseDefinition(bus, (int)SamusPoseId.FacingRightNormalPose,
        [0x08, 0x00, 0xff, 0x02, 0x00, 0x00, 0x15, 0x00]);
    WritePoseAnimationDelays(bus, SamusPoseId.FacingRightNormalPose, 0xc500, [4]);

    // No sine-table region is installed: echo mechanics use compiled stock samples.

    SeedStoredShinePalettes(bus);
    var suitColors = SamusSuitColorCatalog.Load(new MemoryStream(
        SuperMetroid.AssetExtraction.Samus.SamusSuitColorExtractor.Extract(bus)));
    var cycleColors = SamusFullBodyCycleColorCatalog.Load(new MemoryStream(
        SuperMetroid.AssetExtraction.Samus.SamusFullBodyCycleColorExtractor.Extract(bus)));

    var shine = new SamusState
    {
        Pose = SamusPoseId.ShinesparkWindupRightPose,
        Position = FixedPosition<RoomPixels>.FromWhole(new(160, 160)),
        Health = 99,
    };
    AssertTrue(!shine.Shinespark.TryStoreFromSpeedBooster(0x03ff),
        "stage three cannot store shine");
    AssertTrue(shine.Shinespark.TryStoreFromSpeedBooster(0x0400),
        "stage four stores shine");
    AssertEqual(180, shine.Shinespark.ShineTimer.RawValue, "stored shine begins at 180");
    AssertEqual(SamusSpecialPaletteType.SpeedBoosterShine, shine.Shinespark.PaletteType, "stored shine installs palette handler one");

    var cgram = new SnesCgram();
    AssertTrue(shine.Shinespark.UpdatePalette(cgram, equippedItems: 0, suitColors, cycleColors),
        "stored shine copies first ROM palette");
    AssertEqual(0x1100, cgram.Colors[192], "stored shine follows double pointer");
    AssertEqual(179, shine.Shinespark.ShineTimer.RawValue, "stored palette decrements every frame");
    AssertEqual(2, shine.Shinespark.PaletteFrameOffset,
        "stored palette advances an even pointer offset");

    // Ten further calls arrive with timer 170 on the final call and publish the warning.
    for (int tick = 0; tick < 10; tick++)
        shine.Shinespark.UpdatePalette(cgram, equippedItems: 0, suitColors, cycleColors);
    AssertTrue(shine.Shinespark.StoredShineWarningSoundRequested,
        "stored timer 170 requests warning sound");
    AssertTrue(shine.Shinespark.ConsumeStoredShineWarningSoundRequest(),
        "frontend can consume the stored-shine warning exactly once");
    AssertTrue(!shine.Shinespark.ConsumeStoredShineWarningSoundRequest(),
        "consumed stored-shine warning cannot replay");
    AssertEqual(169, shine.Shinespark.ShineTimer.RawValue,
        "warning frame still decrements timer");

    shine.PoseMetadata.RefreshCollisionRadii();
    shine.Presentation.InitializeAnimation();
    shine.Shinespark.BeginWindup(shine);
    AssertEqual(ShinesparkPhase.Windup, shine.Shinespark.Phase,
        "stored shine installs windup handler");
    AssertEqual(30, shine.Shinespark.StartStopTimer.RawValue, "windup timer is 30");
    AssertEqual(8, shine.HorizontalSpeed.ExtraRunSpeed.Whole,
        "windup seeds exact 8.0000 extra speed");
    AssertEqual(0x0400, shine.HorizontalSpeed.SpeedBoostCounter,
        "windup republishes stage four");

    const int width = 24;
    const int height = 24;
    var sparkForeground = new ushort[width * height];
    var sparkBehavior = new byte[width * height];
    // X=160, radius=19, and the first 8.2800 rightward move put the leading edge in
    // block column 10. BTS 7 is the cartridge's 2x2 non-respawning collision bomb block.
    // Pose `$C9` must make `$84:CE83` clear its dispatcher nibble without reporting a hit.
    int sparkBombBlockIndex = 9 * width + 10;
    int sparkExtensionBlockIndex = 8 * width + 10;
    sparkForeground[sparkBombBlockIndex] = 0xf123;
    sparkBehavior[sparkBombBlockIndex] = 7;
    // Type `$D` adds signed BTS rows and asks the dispatcher to run again. Point the first
    // scanned row down one row to the bomb block; the native real-room case uses FF/up.
    sparkForeground[sparkExtensionBlockIndex] = 0xd456;
    sparkBehavior[sparkExtensionBlockIndex] = 1;
    RoomLevelData empty = CreateRoom(new(width, height), sparkForeground, sparkBehavior);

    // Exercise all six stable direction poses through the public install and movement
    // handlers. The pose definition's literal X-direction byte—not target-pose parity in
    // production—selects horizontal sign, while the phase selects which block axes run.
    RoomLevelData directionLevel = new RoomLevelData(new Extent16<RoomBlocks>(width, height), new RoomLevelPlanes(new ushort[width * height], new byte[width * height], new ushort[width * height]), new byte[8]);
    foreach ((SamusPoseId windupPose, SamusPoseId targetPose, ShinesparkPhase expectedPhase,
        bool expectsHorizontal, bool expectsVertical) in new[]
    {
        (SamusPoseId.ShinesparkWindupRightPose, SamusPoseId.ShinesparkHorizontalRightPose,
            ShinesparkPhase.Horizontal, true, false),
        (SamusPoseId.ShinesparkWindupLeftPose, SamusPoseId.ShinesparkHorizontalLeftPose,
            ShinesparkPhase.Horizontal, true, false),
        (SamusPoseId.ShinesparkWindupRightPose, SamusPoseId.ShinesparkVerticalRightPose,
            ShinesparkPhase.Vertical, false, true),
        (SamusPoseId.ShinesparkWindupLeftPose, SamusPoseId.ShinesparkVerticalLeftPose,
            ShinesparkPhase.Vertical, false, true),
        (SamusPoseId.ShinesparkWindupRightPose, SamusPoseId.ShinesparkDiagonalRightPose,
            ShinesparkPhase.Diagonal, true, true),
        (SamusPoseId.ShinesparkWindupLeftPose, SamusPoseId.ShinesparkDiagonalLeftPose,
            ShinesparkPhase.Diagonal, true, true),
    })
    {
        var directional = new SamusState
        {
            Pose = windupPose,
            Position = FixedPosition<RoomPixels>.FromWhole(new(160, 160)),
            Health = 99,
        };
        directional.PoseMetadata.RefreshCollisionRadii();
        directional.Presentation.InitializeAnimation();
        directional.Shinespark.TryStoreFromSpeedBooster(0x0400);
        directional.Shinespark.BeginWindup(directional);
        directional.Shinespark.BeginDirectionalLaunch(directional, targetPose);
        AssertTrue(directional.Shinespark.ConsumeLaunchSoundRequest(),
            $"pose ${(int)targetPose:X2} publishes one launch sound");
        AssertTrue(!directional.Shinespark.ConsumeLaunchSoundRequest(),
            $"pose ${(int)targetPose:X2} launch sound is one-shot");
        directional.Kinematics.YAcceleration = new SnesUnsignedSixteenSixteen(0, 0x2800);
        uint directionalXBefore = directional.Kinematics.Position.X.RawValue;
        uint directionalYBefore = directional.Kinematics.Position.Y.RawValue;
        directional.Shinespark.Step(new SamusTerrain { Level = directionLevel }, directional, new ShinesparkFrame { NmiFrameCounter = 0 });
        AssertEqual(expectedPhase, directional.Shinespark.Phase,
            $"pose ${(int)targetPose:X2} installs expected shinespark handler");
        AssertEqual(expectsHorizontal, directional.Kinematics.Position.X.RawValue != directionalXBefore,
            $"pose ${(int)targetPose:X2} horizontal-axis dispatch moves X");
        AssertEqual(expectsVertical, directional.Kinematics.Position.Y.RawValue != directionalYBefore,
            $"pose ${(int)targetPose:X2} vertical-axis dispatch moves Y");
        if (expectsHorizontal)
        {
            AssertEqual(targetPose is SamusPoseId.ShinesparkHorizontalLeftPose or
                SamusPoseId.ShinesparkDiagonalLeftPose,
                directional.Position.X.Whole < 160,
                $"pose ${(int)targetPose:X2} follows ROM X-direction metadata");
        }
        if (expectsVertical)
        {
            AssertTrue(directional.Position.Y.Whole < 160,
                $"pose ${(int)targetPose:X2} moves upward through block collision");
        }
    }

    // `$90:D261-$D29F` adds the producer after negating the spark speed. A sufficiently
    // positive external Y value can therefore reverse an upward shinespark. Native then
    // skips its 15-pixel upward-only clamp because the temporary magnitude is negative.
    // Starting at 7.0000 with acceleration 0.2800 and adding +8.0000 yields +0.D800.
    var externallyReversedSpark = new SamusState
    {
        Pose = SamusPoseId.ShinesparkWindupRightPose,
        Position = FixedPosition<RoomPixels>.FromWhole(new(160, 160)),
        Health = 99,
    };
    externallyReversedSpark.PoseMetadata.RefreshCollisionRadii();
    externallyReversedSpark.Presentation.InitializeAnimation();
    externallyReversedSpark.Shinespark.TryStoreFromSpeedBooster(0x0400);
    externallyReversedSpark.Shinespark.BeginWindup(externallyReversedSpark);
    externallyReversedSpark.Shinespark.BeginDirectionalLaunch(
        externallyReversedSpark,
        SamusPoseId.ShinesparkVerticalRightPose);
    externallyReversedSpark.Kinematics.YAcceleration = new SnesUnsignedSixteenSixteen(0, 0x2800);
    externallyReversedSpark.Kinematics.ExtraDisplacement = externallyReversedSpark.Kinematics.ExtraDisplacement.WithWholeY(8).WithFractionY(0);
    uint reversedSparkYBefore = externallyReversedSpark.Kinematics.Position.Y.RawValue;
    externallyReversedSpark.Shinespark.Step(new SamusTerrain { Level = directionLevel }, externallyReversedSpark, new ShinesparkFrame { NmiFrameCounter = 0 });
    AssertEqual(0x0000d800, unchecked((int)(externallyReversedSpark.Kinematics.Position.Y.RawValue - reversedSparkYBefore)),
        "positive external Y reverses vertical shinespark after native speed negation");
    AssertEqual(160, externallyReversedSpark.Position.Y.Whole,
        "subpixel shinespark reversal retains the whole Y coordinate");
    AssertEqual(0xd800, externallyReversedSpark.Kinematics.Position.Y.Fraction,
        "subpixel shinespark reversal publishes exact D800 fraction");

    for (ushort frame = 0; frame < 29; frame++)
    {
        ShinesparkMovementResult waiting = shine.Shinespark.Step(new SamusTerrain { Level = empty }, shine, new ShinesparkFrame { NmiFrameCounter = frame });
        AssertTrue(!waiting.WindupTimedOut, $"windup frame {frame} remains stationary");
    }
    ShinesparkMovementResult timeout = shine.Shinespark.Step(new SamusTerrain { Level = empty }, shine, new ShinesparkFrame { NmiFrameCounter = 29 });
    AssertTrue(timeout.WindupTimedOut, "thirtieth windup frame launches vertically");
    AssertEqual(SamusPoseId.ShinesparkVerticalRightPose, shine.Pose,
        "windup timeout selects right-metadata vertical pose");
    AssertEqual(ShinesparkPhase.Vertical, shine.Shinespark.Phase,
        "windup timeout installs vertical handler");

    // The testing cheat affects every direction, including energy values on both sides
    // of the native cutoff. Verify actual movement and drain, not only lack of a crash.
    foreach (SamusPoseId targetPose in new[]
    {
        SamusPoseId.ShinesparkHorizontalRightPose,
        SamusPoseId.ShinesparkHorizontalLeftPose,
        SamusPoseId.ShinesparkVerticalRightPose,
        SamusPoseId.ShinesparkVerticalLeftPose,
        SamusPoseId.ShinesparkDiagonalRightPose,
        SamusPoseId.ShinesparkDiagonalLeftPose,
    })
    foreach (ushort energy in new ushort[] { 1, 2, 29, 30 })
    {
        var invincible = new SamusState
        {
            Pose = SamusPoseId.ShinesparkWindupRightPose,
            Position = FixedPosition<RoomPixels>.FromWhole(new(160, 160)),
            Health = energy,
        };
        invincible.PoseMetadata.RefreshCollisionRadii();
        invincible.Presentation.InitializeAnimation();
        invincible.Shinespark.TryStoreFromSpeedBooster(SamusSpecialSequenceRomData.Shinespark.ActiveSpeedBoostCounter);
        invincible.Shinespark.BeginWindup(invincible);
        invincible.Shinespark.BeginDirectionalLaunch(invincible, targetPose);
        invincible.Shinespark.Step(new SamusTerrain { Level = directionLevel }, invincible, new ShinesparkFrame { NmiFrameCounter = 0, PlayerInvincibilityEnabled = true });
        AssertTrue(invincible.Shinespark.Phase is not ShinesparkPhase.Crash,
            $"invincible pose {(int)targetPose:X2} continues at {energy} energy");
        AssertTrue(invincible.Position.X.Whole != 160 || invincible.Position.Y.Whole != 160,
            "invincible spark actually advances");
        AssertEqual(Math.Max(1, energy - 1), invincible.Health,
            "invincible spark drains without underflow or health restoration");
        AssertEqual(energy > 1, invincible.Health < energy,
            "drain occurs only as a real energy decrement");
    }

    // Use a fresh state so horizontal arithmetic begins from exactly CFFA's writes.
    var horizontal = new SamusState
    {
        Pose = SamusPoseId.ShinesparkWindupRightPose,
        Position = FixedPosition<RoomPixels>.FromWhole(new(160, 160)),
        Health = 30,
    };
    horizontal.PoseMetadata.RefreshCollisionRadii();
    horizontal.Presentation.InitializeAnimation();
    horizontal.Shinespark.TryStoreFromSpeedBooster(0x0400);
    horizontal.Shinespark.BeginWindup(horizontal);
    horizontal.Shinespark.BeginDirectionalLaunch(
        horizontal, SamusPoseId.ShinesparkHorizontalRightPose);
    horizontal.Kinematics.YAcceleration = new SnesUnsignedSixteenSixteen(0, 0x2800);
    var shineBombPlms = new RoomPlmSystem();

    uint firstYBefore = horizontal.Kinematics.Position.Y.RawValue;
    horizontal.Shinespark.Step(new SamusTerrain { Level = empty, Plms = shineBombPlms }, horizontal, new ShinesparkFrame { NmiFrameCounter = 4 });
    AssertTrue(horizontal.Shinespark.Phase == ShinesparkPhase.Horizontal &&
        horizontal.Kinematics.Position.Y.RawValue == firstYBefore,
        "horizontal spark uses only block X movement without colliding");
    AssertSequenceEqual(new[] { sparkBombBlockIndex }, shineBombPlms.ActivePlmBlockIndices,
        "horizontal spark breaks the BTS-7 block");
    AssertEqual(0x0058, empty.ForegroundEntries.Span[sparkBombBlockIndex],
        "bomb-block setup replaces collision and visual words like CE83");
    AssertEqual(1, shineBombPlms.SlotTable.ActiveCount,
        "shinespark collision installs the bank-$84 BTS-7 PLM in the active room owner");
    AssertEqual(0xd456, empty.ForegroundEntries.Span[sparkExtensionBlockIndex],
        "extension redispatch mutates its target rather than the extension word");
    AssertEqual(168, horizontal.Position.X.Whole, "first horizontal spark moves 8 whole pixels");
    AssertEqual(0x2800, horizontal.Kinematics.Position.X.Fraction,
        "first horizontal spark preserves 16.16 fraction");
    AssertEqual(29, horizontal.Health, "active spark drains one energy at threshold 30");
    AssertEqual(SamusContactDamageIndex.Shinespark, horizontal.HorizontalSpeed.ContactDamageIndex,
        "active spark publishes contact damage two");

    uint lowEnergyXBefore = horizontal.Kinematics.Position.X.RawValue;
    horizontal.Shinespark.Step(new SamusTerrain { Level = empty }, horizontal, new ShinesparkFrame { NmiFrameCounter = 5 });
    AssertTrue(horizontal.Kinematics.Position.X.RawValue != lowEnergyXBefore && horizontal.Health == 29,
        "29-energy frame moves then enters crash without another drain");
    AssertEqual(ShinesparkPhase.Crash, horizontal.Shinespark.Phase,
        "low energy installs crash handler");
    AssertTrue(horizontal.Shinespark.ConsumeCrashSoundRequest(),
        "crash setup publishes its paired library-one/library-three event once");
    AssertTrue(!horizontal.Shinespark.ConsumeCrashSoundRequest(),
        "consumed crash sound event cannot replay during the crash orbit");
    AssertEqual(0, horizontal.HorizontalSpeed.ExtraRunSpeed.Whole,
        "crash clears horizontal spark velocity");

    ushort crashCenterX = horizontal.Position.X.Whole;
    ushort crashCenterY = horizontal.Position.Y.Whole;
    ShinesparkMovementResult firstCrash = horizontal.Shinespark.Step(new SamusTerrain { Level = empty }, horizontal, new ShinesparkFrame { NmiFrameCounter = 6 });
    AssertEqual(4, horizontal.Shinespark.CrashRadius,
        "crash orbit expands by four");
    AssertEqual(unchecked((ushort)(crashCenterX - 2)),
        horizontal.HorizontalSpeed.FirstSpeedEchoPosition.X,
        "right-facing first crash echo folds into negative X half");
    AssertEqual(unchecked((ushort)(crashCenterY - 2)),
        horizontal.HorizontalSpeed.FirstSpeedEchoPosition.Y,
        "right-facing first crash echo folds into negative Y half");
    AssertEqual(unchecked((ushort)(crashCenterX + 2)),
        horizontal.HorizontalSpeed.SecondSpeedEchoPosition.X,
        "opposite crash echo uses positive X half");
    AssertTrue(!firstCrash.CrashSequenceFinished,
        "first crash orbit frame cannot finish sequence");

    // Four expansion calls, 32 separation calls, and four contraction calls comprise the
    // exact `$90:D383-$D3CC` orbit. One was consumed above, so 39 remain.
    StepFrames(39, frame =>
        horizontal.Shinespark.Step(new SamusTerrain { Level = empty }, horizontal, new ShinesparkFrame { NmiFrameCounter = unchecked((ushort)(7 + frame)) }));
    AssertEqual(ShinesparkPhase.CrashEchoCircle, horizontal.Shinespark.Phase,
        "crash orbit contracts into echo-circle hold");
    AssertEqual(30, horizontal.Shinespark.StartStopTimer.RawValue,
        "echo circle begins at 30 frames");

    StepFrames(30, frame =>
        horizontal.Shinespark.Step(new SamusTerrain { Level = empty }, horizontal, new ShinesparkFrame { NmiFrameCounter = unchecked((ushort)(46 + frame)) }));
    AssertEqual(ShinesparkPhase.CrashFinish, horizontal.Shinespark.Phase,
        "thirtieth echo-circle frame installs finish handler");
    ShinesparkMovementResult finished = horizontal.Shinespark.Step(new SamusTerrain { Level = empty }, horizontal, new ShinesparkFrame { NmiFrameCounter = 76 });
    AssertTrue(finished.CrashSequenceFinished, "finish handler publishes completion");
    AssertEqual(SamusPoseId.ShinesparkHorizontalRightPose, horizontal.Pose,
        "crash finish retains old pose until the post-animation transition");
    SamusShinesparkState.ApplyCrashFinishPose(horizontal);
    AssertEqual(19, horizontal.Kinematics.Radius.Y, "crash standing commit retains old live radius until alpha");
    AssertEqual(ShinesparkPhase.Inactive, horizontal.Shinespark.Phase,
        "finish restores ordinary movement handler");
    AssertEqual(SamusPoseId.FacingRightNormalPose, horizontal.Pose,
        "right-facing crash finish returns through standing pose one");

    // `$90:D40D` sampled horizontal-right crash pose `$C9`, so its literal pair is
    // angle $00/$80. Both fixed projectile slots begin centered at radius 64 and do not
    // execute `$90:D4D2` until the following alpha projectile pass.
    AssertEqual(2, horizontal.Shinespark.ReleasedCrashEchoCount,
        "empty projectile capacity admits both departing crash echoes");
    AssertEqual(0x00, horizontal.Shinespark.FirstReleasedCrashEchoAngle.TableIndex,
        "horizontal-right first departing echo uses ROM table angle zero");
    AssertEqual(0x80, horizontal.Shinespark.SecondReleasedCrashEchoAngle.TableIndex,
        "horizontal-right second departing echo uses opposite ROM table angle $80");
    AssertEqual(0, horizontal.Shinespark.FirstReleasedCrashEchoRadius,
        "departing projectile radius starts at zero, separate from drawing speed");
    AssertEqual(crashCenterX, horizontal.Shinespark.FirstReleasedCrashEcho.Position.X,
        "departing echo does not move in its spawn frame");

    horizontal.Shinespark.StepReleasedCrashEchoProjectiles(
        bus, horizontal,
        layer1: new(unchecked((ushort)(crashCenterX - 128)), 0));
    AssertEqual(8, horizontal.Shinespark.FirstReleasedCrashEchoRadius,
        "speed-echo pre-instruction expands radius by eight");
    AssertEqual(crashCenterX,
        horizontal.Shinespark.FirstReleasedCrashEcho.Position.X,
        "angle-zero departing echo has zero X component");
    AssertEqual(unchecked((ushort)(horizontal.Position.Y.Whole - 8)),
        horizontal.Shinespark.FirstReleasedCrashEcho.Position.Y,
        "angle-zero departing echo uses negative cosine-table Y component");
    AssertEqual(crashCenterX,
        horizontal.Shinespark.SecondReleasedCrashEcho.Position.X,
        "angle-$80 departing echo has zero X component");
    AssertEqual(unchecked((ushort)(horizontal.Position.Y.Whole + 8)),
        horizontal.Shinespark.SecondReleasedCrashEcho.Position.Y,
        "angle-$80 departing echo uses positive cosine-table Y component");

    // These are vertical rays, recentered on the aligned standing body rather
    // than the old crash body. Apply viewport bounds to that current center.
    // Verify each exact admission boundary rather than only eventual disappearance.
    for (int radius = 16; radius <= 168; radius += 8)
    {
        horizontal.Shinespark.StepReleasedCrashEchoProjectiles(
            bus, horizontal,
            layer1: new(unchecked((ushort)(crashCenterX - 128)), 0));
        AssertEqual(horizontal.Position.Y.Whole - radius >= 0,
            horizontal.Shinespark.FirstReleasedCrashEcho.Active,
            "upward echo remains active through row zero");
        AssertEqual(horizontal.Position.Y.Whole + radius < 256,
            horizontal.Shinespark.SecondReleasedCrashEcho.Active,
            "downward echo clears on reaching row 256");
    }
    AssertEqual(0, horizontal.Shinespark.ReleasedCrashEchoCount,
        "departing crash echoes delete themselves outside the 256-pixel viewport");

    // Repeat only the state-machine spine for native projectile-capacity edges. A count of
    // four suppresses fixed slot three but still allocates slot four; five suppresses both.
    static SamusState FinishAtProjectileCount(
        TestAddressSpace fixtureBus,
        RoomLevelData fixtureLevel,
        ushort projectileCounter)
    {
        var fixture = new SamusState
        {
            Pose = SamusPoseId.ShinesparkWindupRightPose,
            Position = FixedPosition<RoomPixels>.FromWhole(new(160, 160)),
            Health = 29,
        };
        fixture.PoseMetadata.RefreshCollisionRadii();
        fixture.Presentation.InitializeAnimation();
        fixture.Shinespark.TryStoreFromSpeedBooster(0x0400);
        fixture.Shinespark.BeginWindup(fixture);
        fixture.Shinespark.BeginDirectionalLaunch(
            fixture, SamusPoseId.ShinesparkHorizontalRightPose);
        fixture.Kinematics.YAcceleration = default;
        fixture.Shinespark.Step(new SamusTerrain { Level = fixtureLevel }, fixture, new ShinesparkFrame { NmiFrameCounter = 0, ProjectileCounter = projectileCounter });
        for (ushort frame = 1; frame <= 71; frame++)
        {
            fixture.Shinespark.Step(new SamusTerrain { Level = fixtureLevel }, fixture, new ShinesparkFrame { NmiFrameCounter = frame, ProjectileCounter = projectileCounter });
        }
        return fixture;
    }

    SamusState countFour = FinishAtProjectileCount(bus, directionLevel, 4);
    AssertEqual(1, countFour.Shinespark.ReleasedCrashEchoCount,
        "projectile count four admits only fixed crash-echo slot four");
    AssertEqual(128, countFour.Shinespark.CrashAngularTravel,
        "capacity-four finish retains the angular-travel word because slot three was not allocated");
    AssertEqual(countFour.Shinespark.CrashAngularTravel,
        countFour.Shinespark.FirstReleasedCrashEcho.Position.Y,
        "inactive first echo Y and crash travel expose the same native word");
    AssertTrue(!countFour.Shinespark.FirstReleasedCrashEcho.Active &&
        countFour.Shinespark.SecondReleasedCrashEcho.Active,
        "capacity-four branch preserves native fixed-slot selection");
    SamusState countFive = FinishAtProjectileCount(bus, directionLevel, 5);
    AssertEqual(0, countFive.Shinespark.ReleasedCrashEchoCount,
        "projectile count five suppresses both departing crash echoes");
    AssertEqual(128, countFive.Shinespark.CrashAngularTravel,
        "capacity-five finish does not clear retained crash travel");

    Console.WriteLine("  Shinespark: storage, palette, launch, motion, bomb blocks, crash orbit/circle, departing echoes, and standing return agree.");
}

// Exercises the complete no-equipment neutral-jump route: ROM pose transition bytecode,
// jump initialization constants, old-speed displacement ordering, variable-height cut,
// falling acceleration, solid-floor landing, radius alignment, and landing animation.
// Walks the exact `$90:D5A2-$D792` Crystal Flash handler chain, including its strict
// controller equality test, ten raise calls, NMI-mod-eight ammo cadence, ROM delay-list
// finish command, energy overflow, and one-frame-late movement-handler cleanup.

    /// <summary>
    /// Checks that the crash orbit's angular travel and a departing echo's radius wrap past 65535 to zero. The travel
    /// is seeded through the slot-three echo coordinate it shares, as a preceding echo leaves it, and both values then
    /// advance only through the production crash and echo handlers.
    /// </summary>
    internal static void VerifyCrashTravelAndEchoRadiusWrap()
    {
        const int width = 24;
        const int height = 24;
        var level = new RoomLevelData(new Extent16<RoomBlocks>(width, height),
            new RoomLevelPlanes(new ushort[width * height], new byte[width * height], new ushort[width * height]), new byte[8]);
        var samus = new SamusState
        {
            Pose = SamusPoseId.ShinesparkWindupRightPose,
            Position = FixedPosition<RoomPixels>.FromWhole(new(160, 160)),
            Health = 29,
        };
        samus.PoseMetadata.RefreshCollisionRadii();
        samus.Presentation.InitializeAnimation();
        samus.Shinespark.TryStoreFromSpeedBooster(SamusSpecialSequenceRomData.Shinespark.ActiveSpeedBoostCounter);
        samus.Shinespark.BeginWindup(samus);
        samus.Shinespark.BeginDirectionalLaunch(samus, SamusPoseId.ShinesparkHorizontalRightPose);
        samus.Kinematics.YAcceleration = default;

        // Four short of the largest travel, so the first orbiting call wraps it to zero.
        int seededTravel = SamusShinesparkState.LargestCrashAngularTravel - 3;
        samus.Shinespark._firstReleasedCrashEcho.Position = new Position16<RoomPixels>(0, checked((ushort)seededTravel));
        int nmiFrame = 0;
        void StepSpark() => samus.Shinespark.Step(new SamusTerrain { Level = level }, samus, new ShinesparkFrame { NmiFrameCounter = nmiFrame++ });

        StepSpark();
        AssertEqual(ShinesparkPhase.Crash, samus.Shinespark.Phase, "below the sustaining energy the spark installs the crash handler");
        AssertEqual(seededTravel, samus.Shinespark.CrashAngularTravel, "crash entry keeps the travel a preceding echo left");
        for (int call = 1; call <= 4; call++)
            StepSpark();
        AssertEqual(16, samus.Shinespark.CrashRadius, "four expanding calls reach radius sixteen");
        AssertEqual(ShinesparkCrashOrbitStage.Orbiting, samus.Shinespark.CrashOrbitStage, "radius sixteen starts the orbit");

        StepSpark();
        AssertEqual(0, samus.Shinespark.CrashAngularTravel, "the first orbiting call wraps the travel past 65535 to zero");
        AssertEqual((ushort)0, samus.Shinespark.FirstReleasedCrashEcho.Position.Y, "the shared echo coordinate holds the wrapped travel");
        // From zero the travel climbs four per call; 124 still orbits and 128 starts contracting.
        for (int call = 2; call <= 32; call++)
        {
            StepSpark();
            AssertEqual(ShinesparkCrashOrbitStage.Orbiting, samus.Shinespark.CrashOrbitStage, $"orbiting call {call} keeps orbiting below 128");
        }
        StepSpark();
        AssertEqual(128, samus.Shinespark.CrashAngularTravel, "the wrapped travel climbs back to 128");
        AssertEqual(ShinesparkCrashOrbitStage.Contracting, samus.Shinespark.CrashOrbitStage, "travel 128 starts contracting");

        // A departing echo seven short of the largest radius wraps to zero on its next expansion and is drawn on Samus.
        var echo = new SamusShinesparkState.ReleasedEchoSlot();
        echo.Initialize(SnesAngle.Zero, samus.Position.Whole);
        echo.Radius = SamusShinesparkState.ReleasedEchoSlot.LargestRadius - 7;
        AssertTrue(SamusShinesparkState.StepReleasedCrashEcho(samus, new Position16<RoomPixels>(0, 0), echo),
            "a wrapped echo stays inside the viewport");
        AssertEqual(0, echo.Radius, "a departing echo's radius wraps past 65535 to zero");
        AssertEqual(samus.Position.Whole, echo.Position, "a zero radius places the echo on Samus");
        Console.WriteLine("Shinespark crash wraps: angular travel and departing echo radius wrap past 65535 to zero.");
    }

    /// <summary>Runs the crash angular-travel and echo-radius wrap check, which needs no cartridge.</summary>
    [VerificationCommand("--shinespark-crash-wraps")]
    internal static int ShinesparkCrashWrapsCommand()
    {
        Suite(nameof(VerifyCrashTravelAndEchoRadiusWrap), () => VerifyCrashTravelAndEchoRadiusWrap());
        return 0;
    }

    /// <summary>Runs the focused stored-shine and shinespark lifecycle check.</summary>
    [VerificationCommand("--shinespark")]
    internal static int ShinesparkCommand()
    {
        Suite(nameof(VerifySamusStoredShineAndShinespark), () => VerifySamusStoredShineAndShinespark());
        Console.WriteLine("PASS shinespark: native movement, energy cutoff, invincibility, and crash lifecycle.");
        return 0;
    }
}
