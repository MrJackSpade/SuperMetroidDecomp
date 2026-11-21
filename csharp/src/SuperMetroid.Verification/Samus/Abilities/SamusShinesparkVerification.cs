using SuperMetroid.AssetExtraction;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.CartridgeTables;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Colours;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core;
namespace SuperMetroid.Verification.Samus.Abilities;
using SuperMetroid.Verification.Harness;
using SuperMetroid.AssetExtraction.Samus;

/// <summary>Checks stored shine, launch, flight, crash, and echo phases against native state and palette behavior.</summary>
internal static class SamusShinesparkVerification
{

/// <summary>
/// Seeds the six compiled stored-shine phase targets with distinct sentinel colors, making an
/// incorrect suit or phase selection immediately observable, plus a distinct first Power Suit color.
/// </summary>
static void SeedStoredShinePalettes(TestAddressSpace bus)
{
    for (int frame = 0; frame < 6; frame++)
    {
        AssertTrue(SamusShadeDefinitions.FullBodyCycles.TryStoredShinePalettePointer(
            0, unchecked((ushort)(frame * 2)), out ushort pointer),
            $"compiled Power stored-shine phase {frame} exists");
        WriteTestWord(bus, SamusSourceDefinitions.PaletteBank.Value | pointer,
            unchecked((ushort)(0x1100 + frame)));
    }
    WriteTestWord(bus, SamusSourceDefinitions.PowerSuitPalette.Value, 0x0321);
}

/// <summary>A fresh shot pool for a spark fixture, whose count admits the crash's echoes and whose shots carry them.</summary>
static SuperMetroid.Core.Samus.Projectiles.SamusProjectileSystem Shots() =>
    new(GameplayAudioRequests.Standalone(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), InstalledFixtures.SamusOverreads,
        new LeftBeamStartFromLastShot(SuperMetroid.Core.Samus.Projectiles.SamusProjectileSystem.PlaceCount), new SamusShotCount(), InstalledFixtures.SamusArtwork(), new global::SuperMetroid.Core.Fx.RoomEarthquake());

/// <summary>Checks shine storage and palette selection through launch, movement, collision, crash, echoes, and return to standing.</summary>
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
        WritePoseDefinition(bus, SamusPoseNumbers.Number(pose),
            [(byte)((SamusPoseNumbers.Number(pose) & 1) != 0 ? 0x08 : 0x04), 0x1b, 0xff, 0x02, 0x00, 0x00, 0x13, 0x00]);
        WritePoseAnimationDelays(bus, pose, unchecked((ushort)(0xc600 + SamusPoseNumbers.Number(pose))), [4]);
    }
    WritePoseDefinition(bus, SamusPoseNumbers.Number(SamusPoseId.FacingRightNormalPose),
        [0x08, 0x00, 0xff, 0x02, 0x00, 0x00, 0x15, 0x00]);
    WritePoseAnimationDelays(bus, SamusPoseId.FacingRightNormalPose, 0xc500, [4]);

    // No sine-table region is installed: echo mechanics use compiled stock samples.

    SeedStoredShinePalettes(bus);
    var suitColors = SamusSuitColorFile.Read(new MemoryStream(
        SuperMetroid.AssetExtraction.Samus.SamusSuitColorExtractor.Extract(bus)));
    var cycleColors = SamusFullBodyCycleColorFile.Read(new MemoryStream(
        SuperMetroid.AssetExtraction.Samus.SamusFullBodyCycleColorExtractor.Extract(bus)));

    var shine = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
    {
        Position = FixedPosition<RoomPixels>.FromWhole(new(160, 160)),
        Health = 99,
    }.InPose(SamusPoseId.ShinesparkWindupRightPose);
    AssertTrue(!shine.Shinespark.TryStoreFromSpeedBooster(new SamusSpeedBoostCounter(3, 255)),
        "stage three cannot store shine");
    AssertTrue(shine.Shinespark.TryStoreFromSpeedBooster(new SamusSpeedBoostCounter(4, 0)),
        "stage four stores shine");
    AssertEqual(180, shine.Shinespark.ShineTimer.UpdatesLeft, "stored shine begins at 180");
    AssertEqual(SamusSpecialColouringKind.SpeedBoosterShine, shine.SpecialColouring.Holder, "the stored shine takes Samus's suit colouring");

    var samusColours = new SamusColours();
    // Samus names shades (#1512); the import resolves them from the seeded suits and cycles.
    var shineSources = InstalledFixtures.SamusShadeSources with { Suits = suitColors, FullBodyCycles = cycleColors.ToColours() };
    AssertTrue(shine.Shinespark.UpdateColouring(samusColours, equippedItems: 0),
        "stored shine copies first ROM palette");
    AssertEqual(0x1100, samusColours.ShownSuit(shineSources)[0], "stored shine follows double pointer");
    AssertEqual(179, shine.Shinespark.ShineTimer.UpdatesLeft, "stored palette decrements every frame");
    AssertEqual(1, shine.Shinespark.ShineGlow.Place.Beat,
        "stored palette stands past the cycle's first shade");

    // Ten further calls arrive with timer 170 on the final call and publish the warning.
    for (int tick = 0; tick < 10; tick++)
        shine.Shinespark.UpdateColouring(samusColours, equippedItems: 0);
    AssertSequenceEqual(new[] { new SoundRequest(RoomSound.StoredShinespark, SoundRequestPriority.High) },
        shine.AudioRequests.SoundRequests.Where(request => request.SoundEffect == RoomSound.StoredShinespark),
        "stored timer 170 queues the warning sound once, at high priority");
    AssertEqual(169, shine.Shinespark.ShineTimer.UpdatesLeft,
        "warning frame still decrements timer");

    SamusPoseMetadata.RefreshCollisionRadii(shine);
    SamusPresentation.InitializeAnimation(shine);
    shine.Shinespark.BeginWindup(shine);
    AssertEqual(ShinesparkPhase.Windup, shine.Shinespark.Phase,
        "stored shine installs windup handler");
    AssertEqual(30, shine.Shinespark.WindupCountdown.UpdatesLeft, "windup timer is 30");
    AssertEqual(8, shine.HorizontalSpeed.ExtraRunSpeed.Whole,
        "windup seeds exact 8.0000 extra speed");
    AssertEqual(new SamusSpeedBoostCounter(4, 0), shine.HorizontalSpeed.SpeedBoostCounter,
        "windup republishes stage four");

    const int width = 24;
    const int height = 24;
    var sparkForeground = new RoomTerrainBlock[width * height];
    var sparkBehavior = new RoomBlockBehavior[width * height];
    // X=160, radius=19, and the first 8.2800 rightward move put the leading edge in
    // block column 10. BTS 7 is the cartridge's 2x2 non-respawning collision bomb block.
    // Pose `$C9` must make `$84:CE83` clear its dispatcher nibble without reporting a hit.
    int sparkBombBlockIndex = 9 * width + 10;
    int sparkExtensionBlockIndex = 8 * width + 10;
    sparkForeground[sparkBombBlockIndex] = RoomTerrainBlock.Of(291, BlockMirroring.None, RoomCollisionType.BombableBlock);
    sparkBehavior[sparkBombBlockIndex] = RoomBlockBehaviorCodec.Decode(7);
    // Type `$D` adds signed BTS rows and asks the dispatcher to run again. Point the first
    // scanned row down one row to the bomb block; the native real-room case uses FF/up.
    sparkForeground[sparkExtensionBlockIndex] = RoomTerrainBlock.Of(86, BlockMirroring.Horizontal, RoomCollisionType.VerticalExtension);
    sparkBehavior[sparkExtensionBlockIndex] = RoomBlockBehaviorCodec.Decode(1);
    RoomLevelData empty = CreateRoom(new(width, height), sparkForeground, sparkBehavior);

    // Exercise all six stable direction poses through the public install and movement
    // handlers. The pose definition's literal X-direction byte—not target-pose parity in
    // production—selects horizontal sign, while the phase selects which block axes run.
    RoomLevelData directionLevel = new RoomLevelData(new Extent16<RoomBlocks>(width, height), new RoomLevelPlanes(new RoomTerrainBlock[width * height], new RoomBlockBehavior[width * height], new RoomBlockAppearance[width * height]), RoomLevelData.NothingBeyondGrid(), []);
    foreach ((SamusPoseId windupPose, SamusPoseId targetPose, ShinesparkPhase expectedPhase,
        bool expectsHorizontal, bool expectsVertical) in new[]
    {
        new VerifySamusStoredShineAndShinesparkCase(SamusPoseId.ShinesparkWindupRightPose, SamusPoseId.ShinesparkHorizontalRightPose,
            ShinesparkPhase.Horizontal, true, false),
        new VerifySamusStoredShineAndShinesparkCase(SamusPoseId.ShinesparkWindupLeftPose, SamusPoseId.ShinesparkHorizontalLeftPose,
            ShinesparkPhase.Horizontal, true, false),
        new VerifySamusStoredShineAndShinesparkCase(SamusPoseId.ShinesparkWindupRightPose, SamusPoseId.ShinesparkVerticalRightPose,
            ShinesparkPhase.Vertical, false, true),
        new VerifySamusStoredShineAndShinesparkCase(SamusPoseId.ShinesparkWindupLeftPose, SamusPoseId.ShinesparkVerticalLeftPose,
            ShinesparkPhase.Vertical, false, true),
        new VerifySamusStoredShineAndShinesparkCase(SamusPoseId.ShinesparkWindupRightPose, SamusPoseId.ShinesparkDiagonalRightPose,
            ShinesparkPhase.Diagonal, true, true),
        new VerifySamusStoredShineAndShinesparkCase(SamusPoseId.ShinesparkWindupLeftPose, SamusPoseId.ShinesparkDiagonalLeftPose,
            ShinesparkPhase.Diagonal, true, true),
    })
    {
        var directional = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
        {
            Position = FixedPosition<RoomPixels>.FromWhole(new(160, 160)),
            Health = 99,
        }.InPose(windupPose);
        SamusPoseMetadata.RefreshCollisionRadii(directional);
        SamusPresentation.InitializeAnimation(directional);
        directional.Shinespark.TryStoreFromSpeedBooster(new SamusSpeedBoostCounter(4, 0));
        directional.Shinespark.BeginWindup(directional);
        directional.Shinespark.BeginDirectionalLaunch(directional, targetPose);
        AssertEqual(1, directional.AudioRequests.CountOf(RoomSound.Shinespark),
            $"pose ${(int)SamusPoseNumbers.Number(targetPose):X2} queues one launch sound");
        directional.Kinematics.YAcceleration = new UnsignedSixteenSixteen(0, 0x2800);
        FixedCoordinate directionalXBefore = directional.Kinematics.Position.X;
        FixedCoordinate directionalYBefore = directional.Kinematics.Position.Y;
        directional.Shinespark.StepAndCommit(SamusTerrain.InRoom(directionLevel, SamusRoomObjectFixtures.Uninstalled), directional, frameCounter: 0, projectiles: Shots());
        AssertEqual(expectedPhase, directional.Shinespark.Phase,
            $"pose ${(int)SamusPoseNumbers.Number(targetPose):X2} installs expected shinespark handler");
        AssertEqual(expectsHorizontal, directional.Kinematics.Position.X != directionalXBefore,
            $"pose ${(int)SamusPoseNumbers.Number(targetPose):X2} horizontal-axis dispatch moves X");
        AssertEqual(expectsVertical, directional.Kinematics.Position.Y != directionalYBefore,
            $"pose ${(int)SamusPoseNumbers.Number(targetPose):X2} vertical-axis dispatch moves Y");
        if (expectsHorizontal)
        {
            AssertEqual(targetPose is SamusPoseId.ShinesparkHorizontalLeftPose or
                SamusPoseId.ShinesparkDiagonalLeftPose,
                directional.Position.X.Whole < 160,
                $"pose ${(int)SamusPoseNumbers.Number(targetPose):X2} follows ROM X-direction metadata");
        }
        if (expectsVertical)
        {
            AssertTrue(directional.Position.Y.Whole < 160,
                $"pose ${(int)SamusPoseNumbers.Number(targetPose):X2} moves upward through block collision");
        }
    }

    // `$90:D261-$D29F` adds the producer after negating the spark speed. A sufficiently
    // positive external Y value can therefore reverse an upward shinespark. Native then
    // skips its 15-pixel upward-only clamp because the temporary magnitude is negative.
    // Starting at 7.0000 with acceleration 0.2800 and adding +8.0000 yields +0.D800.
    var externallyReversedSpark = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
    {
        Position = FixedPosition<RoomPixels>.FromWhole(new(160, 160)),
        Health = 99,
    }.InPose(SamusPoseId.ShinesparkWindupRightPose);
    SamusPoseMetadata.RefreshCollisionRadii(externallyReversedSpark);
    SamusPresentation.InitializeAnimation(externallyReversedSpark);
    externallyReversedSpark.Shinespark.TryStoreFromSpeedBooster(new SamusSpeedBoostCounter(4, 0));
    externallyReversedSpark.Shinespark.BeginWindup(externallyReversedSpark);
    externallyReversedSpark.Shinespark.BeginDirectionalLaunch(
        externallyReversedSpark,
        SamusPoseId.ShinesparkVerticalRightPose);
    externallyReversedSpark.Kinematics.YAcceleration = new UnsignedSixteenSixteen(0, 0x2800);
    externallyReversedSpark.Kinematics.ExtraDisplacement = externallyReversedSpark.Kinematics.ExtraDisplacement.WithWholeY(8).WithFractionY(0);
    FixedCoordinate reversedSparkYBefore = externallyReversedSpark.Kinematics.Position.Y;
    externallyReversedSpark.Shinespark.StepAndCommit(SamusTerrain.InRoom(directionLevel, SamusRoomObjectFixtures.Uninstalled), externallyReversedSpark, frameCounter: 0, projectiles: Shots());
    AssertEqual(SixteenSixteenWords.ToDisplacement(0x0000d800), externallyReversedSpark.Kinematics.Position.Y - reversedSparkYBefore,
        "positive external Y reverses vertical shinespark after native speed negation");
    AssertEqual(160, externallyReversedSpark.Position.Y.Whole,
        "subpixel shinespark reversal retains the whole Y coordinate");
    AssertEqual(0xd800, externallyReversedSpark.Kinematics.Position.Y.Fraction,
        "subpixel shinespark reversal publishes exact D800 fraction");

    for (ushort frame = 0; frame < 29; frame++)
    {
        ShinesparkMovementResult waiting = shine.Shinespark.StepAndCommit(SamusTerrain.InRoom(empty, SamusRoomObjectFixtures.Uninstalled), shine, frameCounter: frame, projectiles: Shots());
        AssertTrue(!waiting.WindupTimedOut, $"windup frame {frame} remains stationary");
    }
    ShinesparkMovementResult timeout = shine.Shinespark.StepAndCommit(SamusTerrain.InRoom(empty, SamusRoomObjectFixtures.Uninstalled), shine, frameCounter: 29, projectiles: Shots());
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
        var invincible = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
        {
            Position = FixedPosition<RoomPixels>.FromWhole(new(160, 160)),
            Health = energy,
        }.InPose(SamusPoseId.ShinesparkWindupRightPose);
        SamusPoseMetadata.RefreshCollisionRadii(invincible);
        SamusPresentation.InitializeAnimation(invincible);
        invincible.Shinespark.TryStoreFromSpeedBooster(SamusSpeedBoostCounter.AtFullStage);
        invincible.Shinespark.BeginWindup(invincible);
        invincible.Shinespark.BeginDirectionalLaunch(invincible, targetPose);
        invincible.Shinespark.StepAndCommit(SamusTerrain.InRoom(directionLevel, SamusRoomObjectFixtures.Uninstalled), invincible, frameCounter: 0, projectiles: Shots(), samusCannotDie: true);
        AssertTrue(invincible.Shinespark.Phase is not ShinesparkPhase.Crash,
            $"invincible pose {(int)SamusPoseNumbers.Number(targetPose):X2} continues at {energy} energy");
        AssertTrue(invincible.Position.X.Whole != 160 || invincible.Position.Y.Whole != 160,
            "invincible spark actually advances");
        AssertEqual(Math.Max(1, energy - 1), invincible.Health,
            "invincible spark drains without underflow or health restoration");
        AssertEqual(energy > 1, invincible.Health < energy,
            "drain occurs only as a real energy decrement");
    }

    // Use a fresh state so horizontal arithmetic begins from exactly CFFA's writes.
    var horizontalCompatibility = SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus();
    var horizontal = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), horizontalCompatibility, new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
    {
        Position = FixedPosition<RoomPixels>.FromWhole(new(160, 160)),
        Health = 30,
    }.InPose(SamusPoseId.ShinesparkWindupRightPose);
    SamusPoseMetadata.RefreshCollisionRadii(horizontal);
    SamusPresentation.InitializeAnimation(horizontal);
    horizontal.Shinespark.TryStoreFromSpeedBooster(new SamusSpeedBoostCounter(4, 0));
    horizontal.Shinespark.BeginWindup(horizontal);
    horizontal.Shinespark.BeginDirectionalLaunch(
        horizontal, SamusPoseId.ShinesparkHorizontalRightPose);
    horizontal.Kinematics.YAcceleration = new UnsignedSixteenSixteen(0, 0x2800);
    var horizontalShots = Shots();
    var shineBombObjects = RoomObjectFixtures.Unbound();

    FixedCoordinate firstYBefore = horizontal.Kinematics.Position.Y;
    horizontal.Shinespark.StepAndCommit(SamusTerrain.InRoom(empty, shineBombObjects), horizontal, frameCounter: 4, projectiles: horizontalShots);
    AssertTrue(horizontal.Shinespark.Phase == ShinesparkPhase.Horizontal &&
        horizontal.Kinematics.Position.Y == firstYBefore,
        "horizontal spark uses only block X movement without colliding");
    AssertSequenceEqual(new[] { new Position16<RoomBlocks>(10, 9) }, shineBombObjects.ActiveObjectBlocks,
        "horizontal spark breaks the BTS-7 block");
    AssertEqual(RoomTerrainBlock.Of(0x58, BlockMirroring.None, RoomCollisionType.Air),
        empty.GetCollisionBlockByIndex(sparkBombBlockIndex).Terrain,
        "bomb-block setup replaces the block with the air bomb-block parent, like CE83");
    AssertEqual(1, shineBombObjects.Places.ActiveCount,
        "shinespark collision installs the bank-$84 BTS-7 PLM in the active room owner");
    AssertEqual(RoomTerrainBlock.Of(0x56, BlockMirroring.Horizontal, RoomCollisionType.VerticalExtension),
        empty.GetCollisionBlockByIndex(sparkExtensionBlockIndex).Terrain,
        "extension redispatch mutates its target rather than the extension block");
    AssertEqual(168, horizontal.Position.X.Whole, "first horizontal spark moves 8 whole pixels");
    AssertEqual(0x2800, horizontal.Kinematics.Position.X.Fraction,
        "first horizontal spark preserves 16.16 fraction");
    AssertEqual(29, horizontal.Health, "active spark drains one energy at threshold 30");
    AssertEqual(SamusBodyAttack.Shinespark, horizontal.HorizontalSpeed.BodyAttack,
        "active spark publishes contact damage two");

    FixedCoordinate lowEnergyXBefore = horizontal.Kinematics.Position.X;
    horizontal.Shinespark.StepAndCommit(SamusTerrain.InRoom(empty, SamusRoomObjectFixtures.Uninstalled), horizontal, frameCounter: 5, projectiles: horizontalShots);
    AssertTrue(horizontal.Kinematics.Position.X != lowEnergyXBefore && horizontal.Health == 29,
        "29-energy frame moves then enters crash without another drain");
    AssertEqual(ShinesparkPhase.Crash, horizontal.Shinespark.Phase,
        "low energy installs crash handler");
    AssertSequenceEqual(new[] { new SoundRequest(SamusSound.SamusDamaged, SoundRequestPriority.Normal), new SoundRequest(RoomSound.ShinesparkEnd, SoundRequestPriority.Normal) },
        horizontal.AudioRequests.SoundRequests.Where(request => request.SoundEffect == SamusSound.SamusDamaged || request.SoundEffect == RoomSound.ShinesparkEnd),
        "crash setup queues its paired library-one and library-three sounds once, in that order");
    AssertEqual(0, horizontal.HorizontalSpeed.ExtraRunSpeed.Whole,
        "crash clears horizontal spark velocity");

    ushort crashCenterX = horizontal.Position.X.Whole;
    ushort crashCenterY = horizontal.Position.Y.Whole;
    ShinesparkMovementResult firstCrash = horizontal.Shinespark.StepAndCommit(SamusTerrain.InRoom(empty, SamusRoomObjectFixtures.Uninstalled), horizontal, frameCounter: 6, projectiles: horizontalShots);
    AssertEqual(4, horizontal.Shinespark.CrashRadius,
        "crash orbit expands by four");
    AssertEqual(unchecked((ushort)(crashCenterX - 2)),
        horizontal.Shinespark.FirstOrbitingCopy.X,
        "right-facing first crash echo folds into negative X half");
    AssertEqual(unchecked((ushort)(crashCenterY - 2)),
        horizontal.Shinespark.FirstOrbitingCopy.Y,
        "right-facing first crash echo folds into negative Y half");
    AssertEqual(unchecked((ushort)(crashCenterX + 2)),
        horizontal.Shinespark.SecondOrbitingCopy.X,
        "opposite crash echo uses positive X half");
    AssertTrue(!firstCrash.CrashSequenceFinished,
        "first crash orbit frame cannot finish sequence");

    // Four expansion calls, 32 separation calls, and four contraction calls comprise the
    // exact `$90:D383-$D3CC` orbit. One was consumed above, so 39 remain.
    StepFrames(39, frame =>
        horizontal.Shinespark.StepAndCommit(SamusTerrain.InRoom(empty, SamusRoomObjectFixtures.Uninstalled), horizontal, frameCounter: unchecked((ushort)(7 + frame)), projectiles: horizontalShots));
    AssertEqual(ShinesparkPhase.CrashHold, horizontal.Shinespark.Phase,
        "crash orbit contracts into echo-circle hold");
    AssertEqual(30, horizontal.Shinespark.CrashHoldCountdown.UpdatesLeft,
        "echo circle begins at 30 frames");

    StepFrames(30, frame =>
        horizontal.Shinespark.StepAndCommit(SamusTerrain.InRoom(empty, SamusRoomObjectFixtures.Uninstalled), horizontal, frameCounter: unchecked((ushort)(46 + frame)), projectiles: horizontalShots));
    AssertEqual(ShinesparkPhase.CrashFinish, horizontal.Shinespark.Phase,
        "thirtieth echo-circle frame installs finish handler");
    ShinesparkMovementResult finished = horizontal.Shinespark.StepAndCommit(SamusTerrain.InRoom(empty, SamusRoomObjectFixtures.Uninstalled), horizontal, frameCounter: 76, projectiles: horizontalShots);
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
    // angle $00/$80. Both echoes' shots begin centered at radius zero and do not
    // execute `$90:D4D2` until the following alpha projectile pass.
    AssertEqual(2, horizontal.Shinespark.ReleasedCrashEchoCount,
        "empty projectile capacity admits both departing crash echoes");
    AssertTrue(ReferenceEquals(horizontalShots.Shots[3], horizontal.Shinespark.FirstReleasedEcho.Shot) &&
        ReferenceEquals(horizontalShots.Shots[4], horizontal.Shinespark.SecondReleasedEcho.Shot),
        "the echoes take the shot pool's last two places, the first echo the one before the last");
    AssertEqual(0x00, horizontal.Shinespark.FirstReleasedEcho.Shot!.KindStateAs<ShinesparkEchoShotState>().Angle.RawValue,
        "horizontal-right first departing echo uses ROM table angle zero");
    AssertEqual(0x80, horizontal.Shinespark.SecondReleasedEcho.Shot!.KindStateAs<ShinesparkEchoShotState>().Angle.RawValue,
        "horizontal-right second departing echo uses opposite ROM table angle $80");
    AssertEqual(0, horizontal.Shinespark.FirstReleasedEcho.Shot!.KindStateAs<ShinesparkEchoShotState>().Radius,
        "departing projectile radius starts at zero, separate from drawing speed");
    AssertEqual(crashCenterX, horizontal.Shinespark.FirstReleasedEcho.ShownPosition!.Value.X,
        "departing echo does not move in its spawn frame");
    AssertEqual(crashCenterY, horizontalCompatibility.CrashOrbitTravel.Travel,
        "a released first echo leaves Samus's height as the next orbit's travel");

    horizontalShots.StepReleasedCrashEchoProjectiles(
        horizontal,
        cameraPosition: new(unchecked((ushort)(crashCenterX - 128)), 0));
    AssertEqual(8, horizontal.Shinespark.FirstReleasedEcho.Shot!.KindStateAs<ShinesparkEchoShotState>().Radius,
        "speed-echo pre-instruction expands radius by eight");
    AssertEqual(crashCenterX,
        horizontal.Shinespark.FirstReleasedEcho.ShownPosition!.Value.X,
        "angle-zero departing echo has zero X component");
    AssertEqual(unchecked((ushort)(horizontal.Position.Y.Whole - 8)),
        horizontal.Shinespark.FirstReleasedEcho.ShownPosition!.Value.Y,
        "angle-zero departing echo uses negative cosine-table Y component");
    AssertEqual(horizontal.Shinespark.FirstReleasedEcho.ShownPosition!.Value.Y, horizontalCompatibility.CrashOrbitTravel.Travel,
        "the first echo leaves its height as the next orbit's travel");
    AssertEqual(crashCenterX,
        horizontal.Shinespark.SecondReleasedEcho.ShownPosition!.Value.X,
        "angle-$80 departing echo has zero X component");
    AssertEqual(unchecked((ushort)(horizontal.Position.Y.Whole + 8)),
        horizontal.Shinespark.SecondReleasedEcho.ShownPosition!.Value.Y,
        "angle-$80 departing echo uses positive cosine-table Y component");

    // These are vertical rays, recentered on the aligned standing body rather
    // than the old crash body. Apply viewport bounds to that current center.
    // Verify each exact admission boundary rather than only eventual disappearance.
    for (int radius = 16; radius <= 168; radius += 8)
    {
        horizontalShots.StepReleasedCrashEchoProjectiles(
            horizontal,
            cameraPosition: new(unchecked((ushort)(crashCenterX - 128)), 0));
        AssertEqual(horizontal.Position.Y.Whole - radius >= 0,
            horizontal.Shinespark.FirstReleasedEcho.ShownPosition is not null,
            "upward echo remains active through row zero");
        AssertEqual(horizontal.Position.Y.Whole + radius < 256,
            horizontal.Shinespark.SecondReleasedEcho.ShownPosition is not null,
            "downward echo clears on reaching row 256");
    }
    AssertEqual(0, horizontal.Shinespark.ReleasedCrashEchoCount,
        "departing crash echoes delete themselves outside the 256-pixel viewport");
    AssertEqual(0, horizontalCompatibility.CrashOrbitTravel.Travel, "a first echo that leaves the screen leaves no travel");
    AssertEqual(0, horizontalShots.Pool.ShotCount.Count, "each echo that leaves the screen clears its shot");

    // Repeat only the state-machine spine for native projectile-capacity edges. A count of
    // four releases only the second echo; five releases neither.
    static SamusState FinishAtProjectileCount(
        RoomLevelData fixtureLevel,
        int projectileCounter,
        SuperMetroid.Core.Samus.SamusCompatibility compatibility)
    {
        var fixtureShots = Shots();
        fixtureShots.Pool.ShotCount.Restore(projectileCounter);
        var fixture = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), compatibility, new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
        {
            Position = FixedPosition<RoomPixels>.FromWhole(new(160, 160)),
            Health = 29,
        }.InPose(SamusPoseId.ShinesparkWindupRightPose);
        SamusPoseMetadata.RefreshCollisionRadii(fixture);
        SamusPresentation.InitializeAnimation(fixture);
        fixture.Shinespark.TryStoreFromSpeedBooster(new SamusSpeedBoostCounter(4, 0));
        fixture.Shinespark.BeginWindup(fixture);
        fixture.Shinespark.BeginDirectionalLaunch(
            fixture, SamusPoseId.ShinesparkHorizontalRightPose);
        fixture.Kinematics.YAcceleration = default;
        fixture.Shinespark.StepAndCommit(SamusTerrain.InRoom(fixtureLevel, SamusRoomObjectFixtures.Uninstalled), fixture, frameCounter: 0, projectiles: fixtureShots);
        for (ushort frame = 1; frame <= 71; frame++)
        {
            fixture.Shinespark.StepAndCommit(SamusTerrain.InRoom(fixtureLevel, SamusRoomObjectFixtures.Uninstalled), fixture, frameCounter: frame, projectiles: fixtureShots);
        }
        return fixture;
    }

    var countFourCompatibility = SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus();
    SamusState countFour = FinishAtProjectileCount(directionLevel, 4, countFourCompatibility);
    AssertEqual(1, countFour.Shinespark.ReleasedCrashEchoCount,
        "projectile count four admits only the second crash echo");
    AssertEqual(128, countFourCompatibility.CrashOrbitTravel.Travel,
        "a finish that releases no first echo leaves the travel its orbit ended on");
    AssertTrue(countFour.Shinespark.FirstReleasedEcho.ShownPosition is null &&
        countFour.Shinespark.SecondReleasedEcho.ShownPosition is not null,
        "a count of four releases the second echo alone");
    var countFiveCompatibility = SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus();
    SamusState countFive = FinishAtProjectileCount(directionLevel, 5, countFiveCompatibility);
    AssertEqual(0, countFive.Shinespark.ReleasedCrashEchoCount,
        "projectile count five suppresses both departing crash echoes");
    AssertEqual(128, countFiveCompatibility.CrashOrbitTravel.Travel,
        "a finish that releases no echo does not clear the crash travel");

    Console.WriteLine("  Shinespark: storage, palette, launch, motion, bomb blocks, crash orbit/circle, departing echoes, and standing return agree.");
}

    /// <summary>
    /// Checks that the crash orbit's angular travel and a departing echo's radius wrap past 65535 to zero. The travel
    /// is seeded as a preceding first echo leaves its height, and both values then advance only through the production
    /// crash and echo handlers.
    /// </summary>
    internal static void VerifyCrashTravelAndEchoRadiusWrap()
    {
        const int width = 24;
        const int height = 24;
        var level = new RoomLevelData(new Extent16<RoomBlocks>(width, height),
            new RoomLevelPlanes(new RoomTerrainBlock[width * height], new RoomBlockBehavior[width * height], new RoomBlockAppearance[width * height]), RoomLevelData.NothingBeyondGrid(), []);
        var compatibility = SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus();
        var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), compatibility, new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
        {
            Position = FixedPosition<RoomPixels>.FromWhole(new(160, 160)),
            Health = 29,
        }.InPose(SamusPoseId.ShinesparkWindupRightPose);
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);
        var shots = Shots();
        samus.Shinespark.TryStoreFromSpeedBooster(SamusSpeedBoostCounter.AtFullStage);
        samus.Shinespark.BeginWindup(samus);
        samus.Shinespark.BeginDirectionalLaunch(samus, SamusPoseId.ShinesparkHorizontalRightPose);
        samus.Kinematics.YAcceleration = default;

        // Four short of the largest travel, so the first orbiting call wraps it to zero.
        int seededTravel = SuperMetroid.Core.Compatibility.CrashOrbitTravelCarryover.LargestTravel - 3;
        compatibility.CrashOrbitTravel.LeaveEchoHeight(seededTravel);
        int nmiFrame = 0;
        void StepSpark() => samus.Shinespark.StepAndCommit(SamusTerrain.InRoom(level, SamusRoomObjectFixtures.Uninstalled), samus, frameCounter: nmiFrame++, projectiles: shots);

        StepSpark();
        AssertEqual(ShinesparkPhase.Crash, samus.Shinespark.Phase, "below the sustaining energy the spark installs the crash handler");
        AssertEqual(seededTravel, compatibility.CrashOrbitTravel.Travel, "crash entry keeps the travel a preceding echo left");
        for (int call = 1; call <= 4; call++)
            StepSpark();
        AssertEqual(16, samus.Shinespark.CrashRadius, "four expanding calls reach radius sixteen");
        AssertEqual(ShinesparkCrashOrbitStage.Orbiting, samus.Shinespark.CrashOrbitStage, "radius sixteen starts the orbit");
        // The copies stand the whole crash radius from Samus along their angles (#1627).
        AssertEqual(samus.Position.Whole + SamusShinesparkState.OffsetAlong(samus.Shinespark.FirstCrashEchoAngle, 16),
            samus.Shinespark.FirstOrbitingCopy, "the first copy stands sixteen pixels out along its angle");
        AssertEqual(samus.Position.Whole + SamusShinesparkState.OffsetAlong(samus.Shinespark.SecondCrashEchoAngle, 16),
            samus.Shinespark.SecondOrbitingCopy, "the second copy stands sixteen pixels out along its angle");

        StepSpark();
        AssertEqual(0, compatibility.CrashOrbitTravel.Travel, "the first orbiting call wraps the travel past 65535 to zero");
        // From zero the travel climbs four per call; 124 still orbits and 128 starts contracting.
        for (int call = 2; call <= 32; call++)
        {
            StepSpark();
            AssertEqual(ShinesparkCrashOrbitStage.Orbiting, samus.Shinespark.CrashOrbitStage, $"orbiting call {call} keeps orbiting below 128");
        }
        StepSpark();
        AssertEqual(128, compatibility.CrashOrbitTravel.Travel, "the wrapped travel climbs back to 128");
        AssertEqual(ShinesparkCrashOrbitStage.Contracting, samus.Shinespark.CrashOrbitStage, "travel 128 starts contracting");

        // A departing echo's radius keeps growing past the lookup's 256 pixels; a radius that is a whole number of
        // lookups places the echo on Samus again.
        AssertTrue(shots.Shinespark.ReleaseCrashEchoes(samus.Shinespark.FirstReleasedEcho, samus.Shinespark.SecondReleasedEcho,
                new ShinesparkDepartureAngles(RotationAngle.Zero, RotationAngle.HalfTurn), SuperMetroid.Core.Progression.SamusBeamCombination.Power, samus.Position.Whole),
            "an empty shot pool releases both echoes");
        SuperMetroid.Core.Samus.Projectiles.SamusProjectile echoShot = samus.Shinespark.FirstReleasedEcho.Shot!;
        echoShot.KindStateAs<ShinesparkEchoShotState>().Radius = 65528;
        shots.Shinespark.StepShinesparkEcho(samus, echoShot, new Position16<RoomPixels>(0, 0));
        AssertTrue(samus.Shinespark.FirstReleasedEcho.ShownPosition is not null, "an echo a whole number of lookups out stays inside the viewport");
        AssertEqual(65536, echoShot.KindStateAs<ShinesparkEchoShotState>().Radius, "a departing echo's radius grows by eight past 65535");
        AssertEqual(samus.Position.Whole, samus.Shinespark.FirstReleasedEcho.ShownPosition!.Value, "a radius of whole lookups places the echo on Samus");
        Console.WriteLine("Shinespark crash: angular travel wraps past 65535 to zero; a departing echo at whole lookups is on Samus.");
    }

    /// <summary>
    /// Checks that the spark's step runs only while the spark moves Samus (#2525): a stored shine and no spark at all
    /// are not stepped, as the frame runs the step only for the shinespark movement owner, so stepping either is rejected.
    /// </summary>
    internal static void VerifyShinesparkStepRequiresMovingSpark()
    {
        var level = new RoomLevelData(new Extent16<RoomBlocks>(16, 16),
            new RoomLevelPlanes(new RoomTerrainBlock[16 * 16], new RoomBlockBehavior[16 * 16], new RoomBlockAppearance[16 * 16]), RoomLevelData.NothingBeyondGrid(), []);
        var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
        {
            Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)),
        }.InPose(SamusPoseId.FacingRightNormalPose);
        var terrain = SamusTerrain.InRoom(level, SamusRoomObjectFixtures.Uninstalled);
        var frame = new ShinesparkFrame
        {
            FrameCounter = 0,
            SamusCannotDie = false,
            Projectiles = Shots(),
            GameTimeFrames = 0,
            PreviousCheckpoint = samus.Position,
        };
        AssertEqual(ShinesparkPhase.Inactive, samus.Shinespark.Phase, "a new Samus has no spark");
        AssertThrows<InvalidOperationException>(() => samus.Shinespark.Step(terrain, samus, frame), "no spark is not stepped");
        samus.Shinespark.TryStoreFromSpeedBooster(SamusSpeedBoostCounter.AtFullStage);
        AssertEqual(ShinesparkPhase.Stored, samus.Shinespark.Phase, "a full boost stores the shine");
        AssertThrows<InvalidOperationException>(() => samus.Shinespark.Step(terrain, samus, frame), "a stored shine is not stepped");
        Console.WriteLine("Shinespark step: no spark and a stored shine are rejected.");
    }

    /// <summary>
    /// Checks the angles a Shinespark crash releases its echoes at for every pose number (#1500): the six crash poses take
    /// their authored pairs, the drained poses Mother Brain's drain installs mid-crash take the installed service's pairs,
    /// and both are the cartridge's bytes at the pose's distance from the first crash pose; every other pose is rejected.
    /// </summary>
    internal static void VerifyCrashEchoDepartures()
    {
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        DrainedCrashEchoDepartures<SamusPoseId, ShinesparkDepartureAngles> drained = InstalledFixtures.SamusOverreads.Projectiles.DrainedCrashDepartures;
        int checkedPoses = 0;
        for (int number = 0; number <= byte.MaxValue; number++)
        {
            if (!SamusPoseNumbering.NamesPose(number)) continue;
            SamusPoseId pose = SamusPoseNumbering.FromNumber(number);
            bool crashPose = number is >= 0xc9 and <= 0xce;
            bool drainedPose = pose is SamusPoseId.DrainedCrouchingRightPose or SamusPoseId.DrainedCrouchingLeftPose;
            if (!crashPose && !drainedPose)
            {
                AssertThrows<ArgumentOutOfRangeException>(() => ShinesparkEchoDepartures.DepartureAngles(pose, drained), $"pose {number} finishes no crash");
                continue;
            }
            ShinesparkDepartureAngles angles = ShinesparkEchoDepartures.DepartureAngles(pose, drained);
            int pair = 0x90d4c6 + (number - 0xc9) * 2;
            AssertEqual(rom.ReadCartridgeByte(pair), (int)angles.First.ToTrigonometryAngle().RawValue, $"pose {number} first echo angle");
            AssertEqual(rom.ReadCartridgeByte(pair + 1), (int)angles.Second.ToTrigonometryAngle().RawValue, $"pose {number} second echo angle");
            if (drainedPose)
                AssertEqual(angles, drained.Departures(pose), $"drained pose {number} reads its angles from the installed service");
            else
                AssertThrows<ArgumentOutOfRangeException>(() => drained.Departures(pose), $"crash pose {number} reads no angles past the table");
            checkedPoses++;
        }
        AssertEqual(8, checkedPoses, "six crash poses and two drained poses finish a crash");
        AssertThrows<InvalidOperationException>(() => ShinesparkEchoDepartures.DepartureAngles(SamusPoseId.DrainedCrouchingRightPose, null),
            "a drained crash without the installed departures is rejected");
        Console.WriteLine("Shinespark crash departures: six authored pairs, two drained pairs read past the table and every other pose's rejection pass.");
    }

    /// <summary>Runs the crash echo departure check against the cartridge and the installed drained-crash service.</summary>
    [VerificationCommand("--shinespark-crash-departures")]
    internal static int ShinesparkCrashDeparturesCommand()
    {
        Suite(nameof(VerifyCrashEchoDepartures), () => VerifyCrashEchoDepartures());
        return 0;
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
