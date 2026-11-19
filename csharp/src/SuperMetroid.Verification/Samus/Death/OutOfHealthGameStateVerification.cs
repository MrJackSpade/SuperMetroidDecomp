using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Gunship;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;

namespace SuperMetroid.Verification.Samus.Death;

/// <summary>
/// Checks which game state Samus running out of health leaves an update in (#2517). The cartridge's out-of-health check
/// ($82:DB69), after room main, enters the AUTO reserves' refill whatever the game state, but begins Samus's death only
/// while the game state still reads main gameplay (8), and otherwise returns before counting the play time. A door hit
/// (9, $94:93B8), a pause (C, $90:EA7A, or $85:8107 when the map station's message closes), the Ceres elevator (20h,
/// $89:AD17) and the gunship's takeoff (26h, $A2:AD23) written earlier in the update therefore take it from the death,
/// and every game state that runs main gameplay's update behind it (the pause's darkening and brightening, the Ceres
/// departure, leaving Zebes) withholds the death and, while Samus is out of health, the play time.
/// </summary>
internal static class OutOfHealthGameStateVerification
{
    /// <summary>A door block showing block zero: the column Samus sparks into.</summary>
    private static readonly RoomTerrainBlock Door = RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.DoorBlock);

    /// <summary>A plain solid block: the wall behind the door column.</summary>
    private static readonly RoomTerrainBlock Solid = RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.SolidBlock);

    /// <summary>Where the door column leads: the Tourian elevator room's door into its save room.</summary>
    private static readonly DoorBlockExit TourianSaveDoor = DoorDefinitions.Resolve(0xdad5, 3);

    /// <summary>
    /// Start pressed on the update Samus runs out of health without reserves: the pause takes the update, and neither its
    /// darkening, its screens nor its brightening kill her or count the play time; the first main-gameplay update after
    /// the brightening begins her death and counts the play time.
    /// </summary>
    internal static void VerifyPauseWinsOverDeathWithoutReserves()
    {
        (SuperMetroidGame game, SuperMetroidRuntime runtime, SamusState samus) = MainGameplayInTheScopeRoom();
        RunsOutOfHealthWithoutReserves(samus);
        GameClockReading beforePause = runtime.GameTime.Reading;

        game.Step(ControllerButton.Start);
        AssertTrue(game.IsAt(PauseStep.DarkeningGameplay), $"the pause takes the update Samus runs out of health on, not her death; ended in {game.GameState}");
        AssertTrue(!runtime.Freeze.GameplayTimeFrozen, "the withheld death does not freeze gameplay");
        AssertTrue(!samus.Control.InputLocked, "the withheld death does not lock Samus out");
        AssertEqual(beforePause, runtime.GameTime.Reading, "the out-of-health check returns before counting the play time");

        int updates = 0;
        while (!game.IsAt(PauseStep.Menu))
        {
            game.Step(0);
            AssertEqual(SuperMetroidGameState.Pause, game.GameState, "the pause's darkening and screens do not kill Samus out of health");
            AssertEqual(beforePause, runtime.GameTime.Reading, "the darkening's gameplay with Samus out of health counts no play time");
            AssertTrue(++updates < 200, $"the pause reaches its menu; reached {game.Session.PauseStep}");
        }
        // Start, held through the pause screen's repeat delay, unpauses.
        while (game.IsAt(PauseStep.Menu))
        {
            game.Step(ControllerButton.Start);
            AssertTrue(++updates < 200, "held Start unpauses");
        }
        while (game.GameState == SuperMetroidGameState.Pause)
        {
            game.Step(0);
            AssertTrue(game.GameState is SuperMetroidGameState.Pause or SuperMetroidGameState.MainGameplay,
                $"the unpause and its brightening do not kill Samus out of health; ended in {game.GameState}");
            AssertEqual(0, samus.Health, "Samus stays out of health through the pause");
            AssertEqual(beforePause, runtime.GameTime.Reading, "the brightening's gameplay with Samus out of health counts no play time");
            AssertTrue(++updates < 400, $"the unpause returns to main gameplay; reached {game.Session.PauseStep}");
        }

        game.Step(0);
        AssertEqual(SuperMetroidGameState.DeathSequence, game.GameState, "the first main-gameplay update after the pause begins the death");
        AssertTrue(!runtime.GameTime.Reading.Equals(beforePause), "the death's update counts the play time");

        Console.WriteLine("Out of health on a pause: the pause wins, its darkening and brightening neither kill Samus nor count the play time, and main gameplay then kills her.");
    }

    /// <summary>
    /// Samus running out of health with AUTO reserves during the pause's darkening: the refill begins over the pause,
    /// counting the play time; the darkening's fade still takes that update's step, and the refill and the main gameplay
    /// it returns to keep the screen at that level.
    /// </summary>
    internal static void VerifyDarkeningEntersReserveRefill()
    {
        (SuperMetroidGame game, SuperMetroidRuntime runtime, SamusState samus) = MainGameplayInTheScopeRoom();
        game.Step(ControllerButton.Start);
        AssertTrue(game.IsAt(PauseStep.DarkeningGameplay), "Start begins the pause's darkening");
        game.Step(0);
        AssertTrue(game.IsAt(PauseStep.DarkeningGameplay), "the first darkening update leaves the screen lit");
        samus.Health = 0;
        samus.MaxHealth = 99;
        samus.ReserveEnergy = 50;
        samus.MaxReserveEnergy = 100;
        samus.ReserveTankMode = ReserveTankMode.Auto;
        GameClockReading beforeRefill = runtime.GameTime.Reading;

        game.Step(0);
        AssertEqual(SuperMetroidGameState.ReserveTankRefill, game.GameState, "the darkening's gameplay enters the AUTO reserves' refill");
        AssertTrue(runtime.Freeze.GameplayTimeFrozen, "the refill freezes gameplay");
        AssertTrue(!runtime.GameTime.Reading.Equals(beforeRefill), "the refill's update counts the play time");
        int dimmed = game.Session.PauseBrightness;
        AssertTrue(dimmed is > 0 and < DisplayBrightness.Full, $"the darkening's fade stopped part-way at {dimmed}");

        int updates = 0;
        while (game.GameState == SuperMetroidGameState.ReserveTankRefill)
        {
            game.Step(0);
            AssertEqual(dimmed, game.Session.PauseBrightness, "the refill keeps the screen at the darkening's level");
            AssertTrue(++updates < 200, "the refill completes");
        }
        AssertEqual(SuperMetroidGameState.MainGameplay, game.GameState, "the completed refill returns to main gameplay");
        game.Step(0);
        AssertEqual(dimmed, game.Session.PauseBrightness, "main gameplay after the refill keeps the darkened screen");

        Console.WriteLine("Out of health with AUTO reserves while the pause darkens: the refill begins, and the screen keeps the darkening's level.");
    }

    /// <summary>
    /// Periodic damage emptying Samus on the update the AUTO reserves' refill completes on: that update returns to main
    /// gameplay before its gameplay runs unfrozen, so with the reserves spent her death begins on it.
    /// </summary>
    internal static void VerifyRefillCompletionUpdateBeginsDeath()
    {
        (SuperMetroidGame game, SuperMetroidRuntime runtime, SamusState samus) = MainGameplayInTheScopeRoom();
        samus.Health = 0;
        samus.MaxHealth = 99;
        samus.ReserveEnergy = 1;
        samus.MaxReserveEnergy = 100;
        samus.ReserveTankMode = ReserveTankMode.Auto;
        game.Step(0);
        AssertEqual(SuperMetroidGameState.ReserveTankRefill, game.GameState, "zero health with AUTO reserves enters the refill");

        // The refill's next update moves the last reserve unit and completes; its unfrozen gameplay applies the damage.
        samus.PeriodicDamage.Accumulate(new UnsignedSixteenSixteen(999, 0));
        game.Step(0);
        AssertEqual(0, samus.ReserveEnergy, "the refill's update moves the last reserve unit");
        AssertEqual(0, samus.Health, "the completion update's gameplay applies the periodic damage");
        AssertEqual(SuperMetroidGameState.DeathSequence, game.GameState, "the refill's completion update, back in main gameplay, begins the death");
        AssertTrue(runtime.Freeze.GameplayTimeFrozen, "the death freezes gameplay");

        Console.WriteLine("Out of health on the refill's completion update: back in main gameplay, the death begins on that update.");
    }

    /// <summary>
    /// Samus touching a door on the update periodic damage takes her last energy without reserves: the door takes the
    /// update and the transition runs without counting the play time; the first main-gameplay update in the room beyond
    /// begins her death.
    /// </summary>
    internal static void VerifyDoorWinsOverDeathWithoutReserves()
    {
        (SuperMetroidGame game, SuperMetroidRuntime runtime, SamusState samus) = MainGameplaySparkingTowardADoor();

        // The next update's spark reaches the door, and the periodic damage Samus's movement applies after it empties her.
        PlaceJustShortOfTheDoor(samus);
        samus.PeriodicDamage.Accumulate(new UnsignedSixteenSixteen(999, 0));
        GameClockReading beforeDoor = runtime.GameTime.Reading;
        game.Step(0);
        AssertEqual(0, samus.Health, "the periodic damage takes Samus's last energy on the update she touches the door");
        AssertTrue(game.IsAt(DoorTransitionStep.EnteringDoor), $"the door Samus touched takes the update, not her death; ended in {game.GameState}");
        AssertEqual(beforeDoor, runtime.GameTime.Reading, "the out-of-health check returns before counting the play time");

        int updates = 0;
        while (game.GameState == SuperMetroidGameState.DoorTransition)
        {
            game.Step(0);
            AssertTrue(game.GameState is SuperMetroidGameState.DoorTransition or SuperMetroidGameState.MainGameplay,
                $"the door transition does not kill Samus out of health; ended in {game.GameState}");
            AssertEqual(beforeDoor, runtime.GameTime.Reading, "the door transition counts no play time");
            AssertTrue(++updates < 600, "the door transition returns to main gameplay");
        }
        AssertEqual(TourianSaveDoor.Door?.DestinationRoom, runtime.Room.LoadedRoom.Definition, "the transition reaches the room beyond the door");

        game.Step(0);
        AssertEqual(SuperMetroidGameState.DeathSequence, game.GameState, "the first main-gameplay update beyond the door begins the death");
        AssertTrue(!runtime.GameTime.Reading.Equals(beforeDoor), "the death's update counts the play time");

        Console.WriteLine("Out of health on a door hit: the door wins, its transition counts no play time, and main gameplay beyond it kills Samus.");
    }

    /// <summary>
    /// Samus touching a door on the update periodic damage empties her into the AUTO reserves' refill (#2534): the refill's
    /// game state (1Bh, $82:DB89) replaces the door's (9, $94:93B8) written earlier in that update, so the door hit is
    /// forgotten, and the update the refill completes on, whose Samus does not touch the door again, stays in main gameplay.
    /// </summary>
    internal static void VerifyReserveRefillReplacesDoorHit()
    {
        (SuperMetroidGame game, SuperMetroidRuntime runtime, SamusState samus) = MainGameplaySparkingTowardADoor();
        samus.MaxHealth = 999;
        samus.ReserveEnergy = 1;
        samus.MaxReserveEnergy = 100;
        samus.ReserveTankMode = ReserveTankMode.Auto;

        // The next update's spark reaches the door, and the periodic damage Samus's movement applies after it empties her.
        PlaceJustShortOfTheDoor(samus);
        samus.PeriodicDamage.Accumulate(new UnsignedSixteenSixteen(999, 0));
        game.Step(0);
        AssertEqual(0, samus.Health, "the periodic damage takes Samus's last energy on the update she touches the door");
        AssertEqual(SuperMetroidGameState.ReserveTankRefill, game.GameState, "the AUTO reserves' refill takes the update from the door");
        AssertTrue(!runtime.RoomLoader.HasPendingDoorTransition, "the refill's game state replaces the door's: the door hit is forgotten");

        // The refill holds Samus; placed back where the completion update's one spark step stays short of the door, she does
        // not touch it again.
        samus.Position = samus.Position.WithWholeX(64);
        game.Step(0);
        AssertEqual(0, samus.ReserveEnergy, "the refill's update moves the last reserve unit");
        AssertTrue(!runtime.RoomLoader.HasPendingDoorTransition, "the completion update's spark stays short of the door");
        AssertEqual(SuperMetroidGameState.MainGameplay, game.GameState, "the refill's completion update, with no door touched, stays in main gameplay");

        Console.WriteLine("A door touched on the update the refill begins: the refill replaces the door, which is entered only if touched again.");
    }

    /// <summary>
    /// Samus reaching the Ceres elevator on the update she runs out of health without reserves: the elevator's departure
    /// takes the update, and neither it nor its hold kills her or counts the play time.
    /// </summary>
    internal static void VerifyCeresElevatorWinsOverDeathWithoutReserves()
    {
        (SuperMetroidGame game, SuperMetroidRuntime runtime, SamusState samus) = MainGameplayOnTheDormantCeresElevator();

        runtime.EnemySession.CeresStatus = CeresEscapeStatus.ElevatorDoorDestroyed;
        RunsOutOfHealthWithoutReserves(samus);
        GameClockReading beforeElevator = runtime.GameTime.Reading;
        game.Step(0);
        AssertEqual(SuperMetroidGameState.CeresDeparture, game.GameState, "the Ceres elevator takes the update Samus runs out of health on, not her death");
        AssertEqual(beforeElevator, runtime.GameTime.Reading, "the out-of-health check returns before counting the play time");

        int holdUpdates = game.Session.ceresDeparture.HoldFramesRemaining;
        for (int update = 0; update < holdUpdates + 2; update++)
        {
            game.Step(0);
            AssertEqual(SuperMetroidGameState.CeresDeparture, game.GameState, "the Ceres departure does not kill Samus out of health");
            AssertEqual(beforeElevator, runtime.GameTime.Reading, "the Ceres departure's gameplay with Samus out of health counts no play time");
        }

        Console.WriteLine("Out of health on the Ceres elevator: the departure wins, and its hold and blackout neither kill Samus nor count the play time.");
    }

    /// <summary>
    /// The escaping gunship's top hull rising past the takeoff height on the update Samus runs out of health without
    /// reserves: leaving Zebes takes the update, and its updates neither kill her nor count the play time.
    /// </summary>
    internal static void VerifyEscapeTakeoffWinsOverDeathWithoutReserves()
    {
        // The camera shows the landed ship, whose pieces the enemies' update then runs.
        (SuperMetroidGame game, SuperMetroidRuntime runtime, SamusState samus) = MainGameplayAtTheLandingSite(new Position16<RoomPixels>(0x0400, 0x0400));
        RoomEnemyPlace top = runtime.Enemies.Places.InUpdateOrder.Single(place => place.SpeciesState<GunshipState>() is not null);
        GunshipState.Of(top).Phase = GunshipPhase.AcceleratingLiftoff;
        // The ship carries Samus, the top hull above her: past the takeoff height, the update's ascent completes the takeoff.
        samus.Position = samus.Position.WithWholeY(GunshipTakeoffDefinitions.TakeoffCompleteY + GunshipMotionDefinitions.TopHullAboveSamus - 8);
        RunsOutOfHealthWithoutReserves(samus);
        GameClockReading beforeTakeoff = runtime.GameTime.Reading;

        game.Step(0);
        AssertEqual(GunshipFrameEvent.EscapeTakeoffCompleted, runtime.EnemyFrameResults.LastEnemyFrame.Gunship,
            $"the ship's ascent completes the takeoff; the top hull is at {top.Position.Y.Whole} in {GunshipState.Of(top).Phase}");
        AssertTrue(game.IsAt(EndingStep.LeavingZebes), $"the gunship's takeoff takes the update Samus runs out of health on, not her death; ended in {game.GameState}");
        AssertEqual(beforeTakeoff, runtime.GameTime.Reading, "the out-of-health check returns before counting the play time");

        for (int update = 0; update < 4; update++)
        {
            game.Step(0);
            AssertTrue(game.IsAt(EndingStep.LeavingZebes), "leaving Zebes does not kill Samus out of health");
            AssertEqual(beforeTakeoff, runtime.GameTime.Reading, "leaving Zebes with Samus out of health counts no play time");
        }

        Console.WriteLine("Out of health on the gunship's takeoff: leaving Zebes wins, and its updates neither kill Samus nor count the play time.");
    }

    /// <summary>Leaving Zebes runs main gameplay's whole update behind its fade, so each of its updates counts the play time.</summary>
    internal static void VerifyLeavingZebesCountsPlayTime()
    {
        (SuperMetroidGame game, SuperMetroidRuntime runtime, _) = MainGameplayAtTheLandingSite(default);
        game.Session.BeginZebesEscapeFade();
        int before = TotalFrames(runtime.GameTime.Reading);

        int updates = 0;
        while (game.IsAt(EndingStep.LeavingZebes))
        {
            game.Step(0);
            updates++;
            AssertTrue(updates <= 40, "the escape fade completes");
        }
        AssertEqual(updates, TotalFrames(runtime.GameTime.Reading) - before, "every update leaving Zebes counts one frame of play time");

        Console.WriteLine("Leaving Zebes: each of the fade's gameplay updates counts the play time.");
    }

    /// <summary>
    /// The map station's completion message, open on an update Samus is out of health without reserves: the box's own
    /// updates run no out-of-health check, and the update the box closes on, which the map station's pause takes, does
    /// not kill her or count the play time.
    /// </summary>
    internal static void VerifyMapStationPauseWinsOverDeathWithoutReserves()
    {
        (SuperMetroidGame game, SuperMetroidRuntime runtime, SamusState samus) = MainGameplayInTheScopeRoom();
        runtime.MessageBox.Begin(GameplayMessageId.MapDataAccessCompleted, 0, runtime.Options.ControllerBindings);
        RunsOutOfHealthWithoutReserves(samus);
        GameClockReading beforeMessage = runtime.GameTime.Reading;

        int updates = 0;
        while (runtime.MessageBox.IsActive)
        {
            game.Step(updates % 2 == 0 ? 0 : ControllerButton.A);
            AssertTrue(game.GameState != SuperMetroidGameState.DeathSequence, "the message box's updates do not kill Samus out of health");
            AssertEqual(beforeMessage, runtime.GameTime.Reading, "the message box's updates count no play time");
            AssertTrue(++updates < 400, "the message box closes");
        }
        AssertTrue(game.IsAt(PauseStep.DarkeningGameplay), $"the map station's pause takes the update the box closes on, not Samus's death; ended in {game.GameState}");

        Console.WriteLine("Out of health on the map station's message: its updates leave Samus alone, and its pause wins the update it closes on.");
    }

    /// <summary>Empties Samus's energy and her reserves, as damage earlier in the next update would.</summary>
    private static void RunsOutOfHealthWithoutReserves(SamusState samus)
    {
        samus.Health = 0;
        samus.ReserveEnergy = 0;
        samus.ReserveTankMode = ReserveTankMode.Auto;
    }

    /// <summary>The play time a clock reading shows, in frames.</summary>
    private static int TotalFrames(GameClockReading reading) =>
        (((reading.Hours * 60) + reading.Minutes) * 60 + reading.Seconds) * 60 + reading.Frames;

    /// <summary>
    /// A game in main gameplay in a room whose block column 10 is a door into the Tourian save room, with a wall two columns
    /// beyond it and no enemies, its Samus in full health and without reserves sparking rightwards from x 128 under her
    /// control; one update of the spark has run, short of the door.
    /// </summary>
    internal static (SuperMetroidGame Game, SuperMetroidRuntime Runtime, SamusState Samus) MainGameplaySparkingTowardADoor()
    {
        var game = new SuperMetroidGame(new SuperMetroid.Core.Saves.SaveSlotStore(InstalledFixtures.Maps));
        InstalledFixtures.BindGame(game);
        game.Session.CreateGameplayRuntime(GameplayRuntimePurpose.SelectedGame);
        SuperMetroidRuntime runtime = game.Session.GameplayRuntime;
        runtime.HudSetup.InitializeHud(HudSnapshot.NewGame);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.System.SetEvent(StoryEvent.ZebesAwake);
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.Parlor);
        foreach (RoomEnemyPlace enemy in runtime.Enemies.Places.InUpdateOrder)
            enemy.Clear();
        var words = new RoomTerrainBlock[32 * 16];
        var bts = new RoomBlockBehavior[words.Length];
        var doorBlocks = new List<RoomDoorBlock>();
        for (int row = 0; row < 16; row++)
        {
            words[row * 32 + 10] = Door;
            doorBlocks.Add(new RoomDoorBlock(new(10, row), TourianSaveDoor));
            words[row * 32 + 12] = Solid;
        }
        RoomLevelData level = Harness.VerificationHelpersVerification.CreateRoom(new Extent16<RoomBlocks>(32, 16), words, bts, doorBlocks: doorBlocks);
        runtime.Room.Adopt((runtime.Room.Scope ?? throw new InvalidOperationException("Verification requires a loaded room.")) with { LevelData = level });
        SamusState samus = runtime.Player.IntroducedSamus;
        samus.Control.Unlock();
        samus.Health = 999;
        samus.ReserveEnergy = 0;
        samus.Pose = SamusPoseId.FacingRightNormalPose;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);
        samus.Shinespark.TryStoreFromSpeedBooster(SamusSpeedBoostCounter.AtFullStage);
        samus.Shinespark.BeginWindup(samus);
        samus.Shinespark.BeginDirectionalLaunch(samus, SamusPoseId.ShinesparkHorizontalRightPose);
        samus.CommitPoseHistory();
        samus.Position = samus.Position.WithWhole(new(128, 128));
        game.Session.Enter(SuperMetroidGameState.MainGameplay);
        game.Step(0);
        AssertTrue(!runtime.RoomLoader.HasPendingDoorTransition, "the spark's first update is short of the door");
        AssertEqual(SuperMetroidGameState.MainGameplay, game.GameState, "the spark's first update stays in main gameplay");
        return (game, runtime, samus);
    }

    /// <summary>
    /// Places the sparking Samus of <see cref="MainGameplaySparkingTowardADoor"/> one pixel short of the door column, which
    /// her spark's next update reaches.
    /// </summary>
    internal static void PlaceJustShortOfTheDoor(SamusState samus) =>
        samus.Position = samus.Position.WithWholeX((ushort)(160 - samus.Kinematics.Radius.X - 1));

    /// <summary>
    /// A game in main gameplay in the Ceres elevator shaft after Ceres Ridley's defeat, its Samus landed on the elevator
    /// inside its trigger under her control while the shaft stays dormant: the elevator room's door is not destroyed yet.
    /// </summary>
    internal static (SuperMetroidGame Game, SuperMetroidRuntime Runtime, SamusState Samus) MainGameplayOnTheDormantCeresElevator()
    {
        var random = new SharedRandom();
        SuperMetroidGame game = CreateRetailGameFixture(new SuperMetroid.Core.Saves.SaveSlotStore(InstalledFixtures.Maps), renderGameplayFrames: false, random: random);
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture(random: random, audio: game.AudioForVerification);
        runtime.HudSetup.InitializeHud(HudSnapshot.NewGame);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.System.RecordBossesDefeated(AreaId.Ceres, DefeatedBosses.AreaBoss);
        DoorDefinition door = CeresDoorDefinitions.FallingTileToCeresElevator;
        runtime.LoadCartridgeRoomThroughDoorForVerification(door, door.DestinationScreen.ToRoomPixels());
        AssertTrue(runtime.Room.LoadedScope.CeresElevatorShaft is not null, "the elevator door's event starts the shaft's turn");
        // The shaft stays dormant until the elevator room's door is destroyed, while Samus lands on the elevator.
        runtime.EnemySession.CeresStatus = CeresEscapeStatus.SelfDestructStarted;
        SamusState samus = runtime.Player.IntroducedSamus;
        samus.Control.Unlock();
        samus.Pose = SamusPoseId.FacingRightNormalPose;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);
        samus.Position = samus.Position.WithWhole(new(128, 96));
        game.AdoptRuntime(runtime);
        game.Session.LastAudioRoomState = runtime.Room.LoadedRoom.State;
        game.Session.Enter(SuperMetroidGameState.MainGameplay);
        int landing = 0;
        do
        {
            game.Step(0);
            AssertTrue(++landing < 120, "Samus lands on the elevator");
        }
        while (!samus.Kinematics.YSpeed.IsZero || landing < 4);
        AssertEqual(SuperMetroidGameState.MainGameplay, game.GameState, "the dormant shaft keeps Samus in main gameplay");
        AssertTrue(samus.Position.Y.Whole is >= 75 and < 128 && samus.Position.X.Whole is > 112 and <= 144,
            $"Samus stands inside the elevator's trigger at {samus.Position.Whole}");
        return (game, runtime, samus);
    }

    /// <summary>A game in main gameplay in the X-ray Scope room, its Samus standing on the open floor under her control.</summary>
    internal static (SuperMetroidGame Game, SuperMetroidRuntime Runtime, SamusState Samus) MainGameplayInTheScopeRoom()
    {
        var random = new SharedRandom();
        SuperMetroidGame game = CreateRetailGameFixture(new SuperMetroid.Core.Saves.SaveSlotStore(InstalledFixtures.Maps), renderGameplayFrames: false, random: random);
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture(random: random, audio: game.AudioForVerification);
        runtime.HudSetup.InitializeHud(HudSnapshot.NewGame);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.XrayScope);
        SamusState samus = runtime.Player.IntroducedSamus;
        samus.Control.Unlock();
        samus.Pose = SamusPoseId.FacingRightNormalPose;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);
        samus.Position = samus.Position.WithWhole(new(128, 139));
        game.AdoptRuntime(runtime);
        game.Session.LastAudioRoomState = runtime.Room.LoadedRoom.State;
        game.Session.Enter(SuperMetroidGameState.MainGameplay);
        game.Step(0);
        AssertEqual(SuperMetroidGameState.MainGameplay, game.GameState, "the fixture's update stays in main gameplay");
        return (game, runtime, samus);
    }

    /// <summary>A game in main gameplay at the Landing Site under <paramref name="camera"/>, its Samus under her control.</summary>
    internal static (SuperMetroidGame Game, SuperMetroidRuntime Runtime, SamusState Samus) MainGameplayAtTheLandingSite(Position16<RoomPixels> camera)
    {
        var random = new SharedRandom();
        SuperMetroidGame game = CreateRetailGameFixture(new SuperMetroid.Core.Saves.SaveSlotStore(InstalledFixtures.Maps), renderGameplayFrames: false, random: random);
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture(random: random, audio: game.AudioForVerification);
        runtime.HudSetup.InitializeHud(HudSnapshot.NewGame);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.LandingSite, camera);
        SamusState samus = runtime.Player.IntroducedSamus;
        samus.Control.Unlock();
        game.AdoptRuntime(runtime);
        game.Session.LastAudioRoomState = runtime.Room.LoadedRoom.State;
        game.Session.Enter(SuperMetroidGameState.MainGameplay);
        return (game, runtime, samus);
    }
}
