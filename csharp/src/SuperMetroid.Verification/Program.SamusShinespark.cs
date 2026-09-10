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

/// <summary>Stored-shine and shinespark verification.</summary>
static void VerifySamusStoredShineAndShinespark()
{
    var bus = new TestAddressSpace();

    // These eight pose records contain only fields consumed by this isolated state test:
    // direction, movement type `$1B`, and collision radius. Animation still follows the
    // normal live pointer table, proving special movement does not bypass cartridge art.
    foreach (byte pose in new byte[]
    {
        SamusPoseIds.ShinesparkWindupRightPose,
        SamusPoseIds.ShinesparkWindupLeftPose,
        SamusPoseIds.ShinesparkHorizontalRightPose,
        SamusPoseIds.ShinesparkHorizontalLeftPose,
        SamusPoseIds.ShinesparkVerticalRightPose,
        SamusPoseIds.ShinesparkVerticalLeftPose,
        SamusPoseIds.ShinesparkDiagonalRightPose,
        SamusPoseIds.ShinesparkDiagonalLeftPose,
    })
    {
        WritePoseDefinition(bus, pose,
            [(byte)((pose & 1) != 0 ? 0x08 : 0x04), 0x1b, 0xff, 0x02, 0x00, 0x00, 0x13, 0x00]);
        ushort stream = unchecked((ushort)(0xc600 + pose));
        WriteTestWord(bus, 0x91b010 + pose * 2, stream);
        bus.WriteByte(0x910000 | stream, 4);
    }
    WritePoseDefinition(bus, SamusPoseIds.FacingRightNormalPose,
        [0x08, 0x00, 0xff, 0x02, 0x00, 0x00, 0x15, 0x00]);
    WriteTestWord(bus, 0x91b010 + SamusPoseIds.FacingRightNormalPose * 2, 0xc500);
    bus.WriteByte(0x91c500, 4);

    // Give every positive-half sine/cosine entry magnitude 1.000. This deliberately is
    // not a trigonometric approximation: it makes `$90:CC8A`'s byte-angle sign folding and
    // radius multiplication observable with simple ±radius expectations below.
    for (int angle = 0; angle < 128; angle++)
        WriteTestWord(bus, 0xa0b443 + angle * 2, 0x0100);

    // Stored-shine table `$91:DB10` -> bank-$91 list -> bank-$9B palette. Distinct
    // sentinel colors make either missing indirection immediately observable.
    WriteTestWord(bus, 0x91db10, 0xd200);
    for (int frame = 0; frame < 6; frame++)
    {
        WriteTestWord(bus, 0x91d200 + frame * 2, unchecked((ushort)(0xe100 + frame * 0x20)));
        WriteTestWord(bus, 0x9be100 + frame * 0x20, unchecked((ushort)(0x1100 + frame)));
    }
    WriteTestWord(bus, 0x91d727, 0x9400);
    WriteTestWord(bus, 0x9b9400, 0x0321);

    var shine = new SamusState
    {
        Pose = SamusPoseIds.ShinesparkWindupRightPose,
        XPosition = 160,
        YPosition = 160,
        Health = 99,
    };
    AssertTrue(!shine.Shinespark.TryStoreFromSpeedBooster(0x03ff),
        "stage three cannot store shine");
    AssertTrue(shine.Shinespark.TryStoreFromSpeedBooster(0x0400),
        "stage four stores shine");
    AssertEqual(180, shine.Shinespark.ShineTimer, "stored shine begins at 180");
    AssertEqual(1, shine.Shinespark.PaletteType, "stored shine installs palette handler one");

    var cgram = new SnesCgram();
    AssertTrue(shine.Shinespark.UpdatePalette(bus, cgram, equippedItems: 0),
        "stored shine copies first ROM palette");
    AssertEqual(0x1100, cgram.Colors[192], "stored shine follows double pointer");
    AssertEqual(179, shine.Shinespark.ShineTimer, "stored palette decrements every frame");
    AssertEqual(2, shine.Shinespark.PaletteFrameOffset,
        "stored palette advances an even pointer offset");

    // Ten further calls arrive with timer 170 on the final call and publish the warning.
    for (int tick = 0; tick < 10; tick++)
        shine.Shinespark.UpdatePalette(bus, cgram, equippedItems: 0);
    AssertTrue(shine.Shinespark.StoredShineWarningSoundRequested,
        "stored timer 170 requests warning sound");
    AssertTrue(shine.Shinespark.ConsumeStoredShineWarningSoundRequest(),
        "frontend can consume the stored-shine warning exactly once");
    AssertTrue(!shine.Shinespark.ConsumeStoredShineWarningSoundRequest(),
        "consumed stored-shine warning cannot replay");
    AssertEqual(169, shine.Shinespark.ShineTimer,
        "warning frame still decrements timer");

    shine.RefreshCollisionRadii(bus);
    shine.InitializeAnimation(bus);
    shine.Shinespark.BeginWindup(shine);
    AssertEqual(ShinesparkPhase.Windup, shine.Shinespark.Phase,
        "stored shine installs windup handler");
    AssertEqual(30, shine.Shinespark.StartStopTimer, "windup timer is 30");
    AssertEqual(8, shine.HorizontalSpeed.ExtraRunSpeed,
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
    RoomLevelData empty = CreateRoom(width, height, sparkForeground, sparkBehavior);

    // Exercise all six stable direction poses through the public install and movement
    // handlers. The pose definition's literal X-direction byte—not target-pose parity in
    // production—selects horizontal sign, while the phase selects which block axes run.
    RoomLevelData directionLevel = new(
        width,
        height,
        new ushort[width * height],
        new byte[width * height],
        new ushort[width * height],
        new byte[8]);
    foreach ((byte windupPose, byte targetPose, ShinesparkPhase expectedPhase,
        bool expectsHorizontal, bool expectsVertical) in new[]
    {
        (SamusPoseIds.ShinesparkWindupRightPose, SamusPoseIds.ShinesparkHorizontalRightPose,
            ShinesparkPhase.Horizontal, true, false),
        (SamusPoseIds.ShinesparkWindupLeftPose, SamusPoseIds.ShinesparkHorizontalLeftPose,
            ShinesparkPhase.Horizontal, true, false),
        (SamusPoseIds.ShinesparkWindupRightPose, SamusPoseIds.ShinesparkVerticalRightPose,
            ShinesparkPhase.Vertical, false, true),
        (SamusPoseIds.ShinesparkWindupLeftPose, SamusPoseIds.ShinesparkVerticalLeftPose,
            ShinesparkPhase.Vertical, false, true),
        (SamusPoseIds.ShinesparkWindupRightPose, SamusPoseIds.ShinesparkDiagonalRightPose,
            ShinesparkPhase.Diagonal, true, true),
        (SamusPoseIds.ShinesparkWindupLeftPose, SamusPoseIds.ShinesparkDiagonalLeftPose,
            ShinesparkPhase.Diagonal, true, true),
    })
    {
        var directional = new SamusState
        {
            Pose = windupPose,
            XPosition = 160,
            YPosition = 160,
            Health = 99,
        };
        directional.RefreshCollisionRadii(bus);
        directional.InitializeAnimation(bus);
        directional.Shinespark.TryStoreFromSpeedBooster(0x0400);
        directional.Shinespark.BeginWindup(directional);
        directional.Shinespark.BeginDirectionalLaunch(bus, directional, targetPose);
        AssertTrue(directional.Shinespark.ConsumeLaunchSoundRequest(),
            $"pose ${targetPose:X2} publishes one launch sound");
        AssertTrue(!directional.Shinespark.ConsumeLaunchSoundRequest(),
            $"pose ${targetPose:X2} launch sound is one-shot");
        directional.Kinematics.YAcceleration = 0;
        directional.Kinematics.YSubacceleration = 0x2800;
        ShinesparkMovementResult directionalStep = directional.Shinespark.Step(
            bus, directionLevel, directional, nmiFrameCounter: 0);
        AssertEqual(expectedPhase, directional.Shinespark.Phase,
            $"pose ${targetPose:X2} installs expected shinespark handler");
        AssertEqual(expectsHorizontal, directionalStep.Horizontal is not null,
            $"pose ${targetPose:X2} horizontal-axis dispatch");
        AssertEqual(expectsVertical, directionalStep.Vertical is not null,
            $"pose ${targetPose:X2} vertical-axis dispatch");
        if (expectsHorizontal)
        {
            AssertEqual(targetPose is SamusPoseIds.ShinesparkHorizontalLeftPose or
                SamusPoseIds.ShinesparkDiagonalLeftPose,
                directional.XPosition < 160,
                $"pose ${targetPose:X2} follows ROM X-direction metadata");
        }
        if (expectsVertical)
        {
            AssertTrue(directional.YPosition < 160,
                $"pose ${targetPose:X2} moves upward through block collision");
        }
    }

    // `$90:D261-$D29F` adds the producer after negating the spark speed. A sufficiently
    // positive external Y value can therefore reverse an upward shinespark. Native then
    // skips its 15-pixel upward-only clamp because the temporary magnitude is negative.
    // Starting at 7.0000 with acceleration 0.2800 and adding +8.0000 yields +0.D800.
    var externallyReversedSpark = new SamusState
    {
        Pose = SamusPoseIds.ShinesparkWindupRightPose,
        XPosition = 160,
        YPosition = 160,
        Health = 99,
    };
    externallyReversedSpark.RefreshCollisionRadii(bus);
    externallyReversedSpark.InitializeAnimation(bus);
    externallyReversedSpark.Shinespark.TryStoreFromSpeedBooster(0x0400);
    externallyReversedSpark.Shinespark.BeginWindup(externallyReversedSpark);
    externallyReversedSpark.Shinespark.BeginDirectionalLaunch(
        bus,
        externallyReversedSpark,
        SamusPoseIds.ShinesparkVerticalRightPose);
    externallyReversedSpark.Kinematics.YAcceleration = 0;
    externallyReversedSpark.Kinematics.YSubacceleration = 0x2800;
    externallyReversedSpark.Kinematics.ExtraYDisplacement = 8;
    externallyReversedSpark.Kinematics.ExtraYSubdisplacement = 0;
    ShinesparkMovementResult reversedSpark = externallyReversedSpark.Shinespark.Step(
        bus,
        directionLevel,
        externallyReversedSpark,
        nmiFrameCounter: 0);
    AssertEqual(0x0000d800, reversedSpark.Vertical!.Value.AcceptedDisplacement,
        "positive external Y reverses vertical shinespark after native speed negation");
    AssertEqual(160, externallyReversedSpark.YPosition,
        "subpixel shinespark reversal retains the whole Y coordinate");
    AssertEqual(0xd800, externallyReversedSpark.Kinematics.YSubposition,
        "subpixel shinespark reversal publishes exact D800 fraction");

    for (ushort frame = 0; frame < 29; frame++)
    {
        ShinesparkMovementResult waiting = shine.Shinespark.Step(bus, empty, shine, frame);
        AssertTrue(!waiting.WindupTimedOut, $"windup frame {frame} remains stationary");
    }
    ShinesparkMovementResult timeout = shine.Shinespark.Step(bus, empty, shine, 29);
    AssertTrue(timeout.WindupTimedOut, "thirtieth windup frame launches vertically");
    AssertEqual(SamusPoseIds.ShinesparkVerticalRightPose, shine.Pose,
        "windup timeout selects right-metadata vertical pose");
    AssertEqual(ShinesparkPhase.Vertical, shine.Shinespark.Phase,
        "windup timeout installs vertical handler");

    // The testing cheat affects every direction, including energy values on both sides
    // of the native cutoff. Verify actual movement and drain, not only lack of a crash.
    foreach (byte targetPose in new[]
    {
        SamusPoseIds.ShinesparkHorizontalRightPose,
        SamusPoseIds.ShinesparkHorizontalLeftPose,
        SamusPoseIds.ShinesparkVerticalRightPose,
        SamusPoseIds.ShinesparkVerticalLeftPose,
        SamusPoseIds.ShinesparkDiagonalRightPose,
        SamusPoseIds.ShinesparkDiagonalLeftPose,
    })
    foreach (ushort energy in new ushort[] { 1, 2, 29, 30 })
    {
        var invincible = new SamusState
        {
            Pose = SamusPoseIds.ShinesparkWindupRightPose,
            XPosition = 160,
            YPosition = 160,
            Health = energy,
        };
        invincible.RefreshCollisionRadii(bus);
        invincible.InitializeAnimation(bus);
        invincible.Shinespark.TryStoreFromSpeedBooster(SamusSpecialSequenceRomData.Shinespark.ActiveSpeedBoostCounter);
        invincible.Shinespark.BeginWindup(invincible);
        invincible.Shinespark.BeginDirectionalLaunch(bus, invincible, targetPose);
        var result = invincible.Shinespark.Step(bus, directionLevel, invincible, 0,
            playerInvincibilityEnabled: true);
        AssertTrue(!result.EndedByLowEnergy && !result.EndedByCollision,
            $"invincible pose {targetPose:X2} continues at {energy} energy");
        AssertTrue(invincible.XPosition != 160 || invincible.YPosition != 160,
            "invincible spark actually advances");
        AssertEqual(Math.Max(1, energy - 1), invincible.Health,
            "invincible spark drains without underflow or health restoration");
        AssertEqual(energy > 1, result.EnergyDrained,
            "drain event reports only a real energy decrement");
    }

    // Use a fresh state so horizontal arithmetic begins from exactly CFFA's writes.
    var horizontal = new SamusState
    {
        Pose = SamusPoseIds.ShinesparkWindupRightPose,
        XPosition = 160,
        YPosition = 160,
        Health = 30,
    };
    horizontal.RefreshCollisionRadii(bus);
    horizontal.InitializeAnimation(bus);
    horizontal.Shinespark.TryStoreFromSpeedBooster(0x0400);
    horizontal.Shinespark.BeginWindup(horizontal);
    horizontal.Shinespark.BeginDirectionalLaunch(
        bus, horizontal, SamusPoseIds.ShinesparkHorizontalRightPose);
    horizontal.Kinematics.YAcceleration = 0;
    horizontal.Kinematics.YSubacceleration = 0x2800;
    var shineBombPlms = new RoomPlmSystem();

    ShinesparkMovementResult first = horizontal.Shinespark.Step(
        bus, empty, horizontal, nmiFrameCounter: 4, plms: shineBombPlms);
    AssertTrue(first.Horizontal is { Collided: false } && first.Vertical is null,
        "horizontal spark uses only block X movement");
    AssertEqual(sparkBombBlockIndex, first.Horizontal!.Value.BrokenBombBlock!.Value.Index,
        "horizontal spark publishes the broken BTS-7 block");
    AssertEqual(0x0058, empty.ForegroundEntries.Span[sparkBombBlockIndex],
        "bomb-block setup replaces collision and visual words like CE83");
    AssertEqual(1, shineBombPlms.ActiveCount,
        "shinespark collision installs the bank-$84 BTS-7 PLM in the active room owner");
    AssertEqual(0xd456, empty.ForegroundEntries.Span[sparkExtensionBlockIndex],
        "extension redispatch mutates its target rather than the extension word");
    AssertEqual(168, horizontal.XPosition, "first horizontal spark moves 8 whole pixels");
    AssertEqual(0x2800, horizontal.Kinematics.XSubposition,
        "first horizontal spark preserves 16.16 fraction");
    AssertEqual(29, horizontal.Health, "active spark drains one energy at threshold 30");
    AssertEqual(2, horizontal.HorizontalSpeed.ContactDamageIndex,
        "active spark publishes contact damage two");

    ShinesparkMovementResult lowEnergy = horizontal.Shinespark.Step(
        bus, empty, horizontal, nmiFrameCounter: 5);
    AssertTrue(lowEnergy.EndedByLowEnergy && !lowEnergy.EnergyDrained,
        "29-energy frame moves then enters crash without another drain");
    AssertEqual(ShinesparkPhase.Crash, horizontal.Shinespark.Phase,
        "low energy installs crash handler");
    AssertTrue(horizontal.Shinespark.ConsumeCrashSoundRequest(),
        "crash setup publishes its paired library-one/library-three event once");
    AssertTrue(!horizontal.Shinespark.ConsumeCrashSoundRequest(),
        "consumed crash sound event cannot replay during the crash orbit");
    AssertEqual(0, horizontal.HorizontalSpeed.ExtraRunSpeed,
        "crash clears horizontal spark velocity");

    ushort crashCenterX = horizontal.XPosition;
    ushort crashCenterY = horizontal.YPosition;
    ShinesparkMovementResult firstCrash = horizontal.Shinespark.Step(
        bus, empty, horizontal, nmiFrameCounter: 6);
    AssertEqual(4, horizontal.Shinespark.CrashRadius,
        "crash orbit expands by four");
    AssertEqual(unchecked((ushort)(crashCenterX - 4)),
        horizontal.HorizontalSpeed.FirstSpeedEchoXPosition,
        "right-facing first crash echo folds into negative X half");
    AssertEqual(unchecked((ushort)(crashCenterY - 4)),
        horizontal.HorizontalSpeed.FirstSpeedEchoYPosition,
        "right-facing first crash echo folds into negative Y half");
    AssertEqual(unchecked((ushort)(crashCenterX + 4)),
        horizontal.HorizontalSpeed.SecondSpeedEchoXPosition,
        "opposite crash echo uses positive X half");
    AssertTrue(!firstCrash.CrashSequenceFinished,
        "first crash orbit frame cannot finish sequence");

    // Four expansion calls, 32 separation calls, and four contraction calls comprise the
    // exact `$90:D383-$D3CC` orbit. One was consumed above, so 39 remain.
    StepFrames(39, frame =>
        horizontal.Shinespark.Step(bus, empty, horizontal, unchecked((ushort)(7 + frame))));
    AssertEqual(ShinesparkPhase.CrashEchoCircle, horizontal.Shinespark.Phase,
        "crash orbit contracts into echo-circle hold");
    AssertEqual(30, horizontal.Shinespark.StartStopTimer,
        "echo circle begins at 30 frames");

    StepFrames(30, frame =>
        horizontal.Shinespark.Step(bus, empty, horizontal, unchecked((ushort)(46 + frame))));
    AssertEqual(ShinesparkPhase.CrashFinish, horizontal.Shinespark.Phase,
        "thirtieth echo-circle frame installs finish handler");
    ShinesparkMovementResult finished = horizontal.Shinespark.Step(
        bus, empty, horizontal, nmiFrameCounter: 76);
    AssertTrue(finished.CrashSequenceFinished, "finish handler publishes completion");
    AssertEqual(ShinesparkPhase.Inactive, horizontal.Shinespark.Phase,
        "finish restores ordinary movement handler");
    AssertEqual(SamusPoseIds.FacingRightNormalPose, horizontal.Pose,
        "right-facing crash finish returns through standing pose one");

    // `$90:D40D` sampled horizontal-right crash pose `$C9`, so its literal pair is
    // angle $00/$80. Both fixed projectile slots begin centered at radius 64 and do not
    // execute `$90:D4D2` until the following alpha projectile pass.
    AssertEqual(2, horizontal.Shinespark.ReleasedCrashEchoCount,
        "empty projectile capacity admits both departing crash echoes");
    AssertEqual(0x00, horizontal.Shinespark.FirstReleasedCrashEcho.Angle.TableIndex,
        "horizontal-right first departing echo uses ROM table angle zero");
    AssertEqual(0x80, horizontal.Shinespark.SecondReleasedCrashEcho.Angle.TableIndex,
        "horizontal-right second departing echo uses opposite ROM table angle $80");
    AssertEqual(64, horizontal.Shinespark.FirstReleasedCrashEcho.Radius,
        "departing echo initializes to native radius 64");
    AssertEqual(crashCenterX, horizontal.Shinespark.FirstReleasedCrashEcho.XPosition,
        "departing echo does not move in its spawn frame");

    horizontal.Shinespark.StepReleasedCrashEchoProjectiles(
        bus, horizontal,
        layer1X: unchecked((ushort)(crashCenterX - 128)),
        layer1Y: 0);
    AssertEqual(72, horizontal.Shinespark.FirstReleasedCrashEcho.Radius,
        "speed-echo pre-instruction expands radius by eight");
    AssertEqual(unchecked((ushort)(crashCenterX + 72)),
        horizontal.Shinespark.FirstReleasedCrashEcho.XPosition,
        "angle-zero departing echo uses positive sine-table X component");
    AssertEqual(unchecked((ushort)(crashCenterY - 72)),
        horizontal.Shinespark.FirstReleasedCrashEcho.YPosition,
        "angle-zero departing echo uses negative cosine-table Y component");
    AssertEqual(unchecked((ushort)(crashCenterX - 72)),
        horizontal.Shinespark.SecondReleasedCrashEcho.XPosition,
        "angle-$80 departing echo uses negative sine-table X component");
    AssertEqual(unchecked((ushort)(crashCenterY + 72)),
        horizontal.Shinespark.SecondReleasedCrashEcho.YPosition,
        "angle-$80 departing echo uses positive cosine-table Y component");

    // Both rays leave the 256-pixel-tall viewport at radius 136. The pre-instruction
    // clears every published word at that instant; it does not keep an off-screen ghost.
    for (int frame = 0; frame < 8; frame++)
    {
        horizontal.Shinespark.StepReleasedCrashEchoProjectiles(
            bus, horizontal,
            layer1X: unchecked((ushort)(crashCenterX - 128)),
            layer1Y: 0);
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
            Pose = SamusPoseIds.ShinesparkWindupRightPose,
            XPosition = 160,
            YPosition = 160,
            Health = 29,
        };
        fixture.RefreshCollisionRadii(fixtureBus);
        fixture.InitializeAnimation(fixtureBus);
        fixture.Shinespark.TryStoreFromSpeedBooster(0x0400);
        fixture.Shinespark.BeginWindup(fixture);
        fixture.Shinespark.BeginDirectionalLaunch(
            fixtureBus, fixture, SamusPoseIds.ShinesparkHorizontalRightPose);
        fixture.Kinematics.YAcceleration = 0;
        fixture.Kinematics.YSubacceleration = 0;
        fixture.Shinespark.Step(
            fixtureBus, fixtureLevel, fixture, 0, projectileCounter);
        for (ushort frame = 1; frame <= 71; frame++)
        {
            fixture.Shinespark.Step(
                fixtureBus, fixtureLevel, fixture, frame, projectileCounter);
        }
        return fixture;
    }

    SamusState countFour = FinishAtProjectileCount(bus, directionLevel, 4);
    AssertEqual(1, countFour.Shinespark.ReleasedCrashEchoCount,
        "projectile count four admits only fixed crash-echo slot four");
    AssertEqual(128, countFour.Shinespark.CrashAngularTravel,
        "capacity-four finish retains the angular-travel word because slot three was not allocated");
    AssertEqual(countFour.Shinespark.CrashAngularTravel,
        countFour.Shinespark.FirstReleasedCrashEcho.YPosition,
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

/// <summary>
/// Exercises the complete no-equipment neutral-jump route: ROM pose transition bytecode,
/// jump initialization constants, old-speed displacement ordering, variable-height cut,
/// falling acceleration, solid-floor landing, radius alignment, and landing animation.
/// </summary>
/// <summary>
/// Walks the exact `$90:D5A2-$D792` Crystal Flash handler chain, including its strict
/// controller equality test, ten raise calls, NMI-mod-eight ammo cadence, ROM delay-list
/// finish command, energy overflow, and one-frame-late movement-handler cleanup.
/// </summary>
}
