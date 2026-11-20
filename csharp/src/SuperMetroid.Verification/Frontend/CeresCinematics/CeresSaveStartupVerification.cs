using SuperMetroid.Diagnostics.Saves;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend;
using static SuperMetroid.Verification.Harness.InstalledArtworkBindingVerification;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using SuperMetroid.Core.Enemies.Gunship;
using SuperMetroid.Core.Frontend.GameOver;
using SuperMetroid.Core.Frontend.Options;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Saves;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Runtime;
namespace SuperMetroid.Verification.Frontend.CeresCinematics;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Ceres departure saves, menu reload routing, and the transition to Zebes landing.</summary>
internal static class CeresSaveStartupVerification
{
    /// <summary>
    /// #1153: confirm the native Ceres checkpoint mode, not a playthrough. Enter
    /// the actual options dispatcher at its completed fade with checksummed SRAM.
    /// The cinematic checkpoint must never construct an escaping Ceres room.
    /// </summary>
    internal static void VerifyCeresSaveStartup()
    {
        // The shared installation is only read: its game artwork and room-object visuals load once for every game here.
        GameInstallation installation = RepositoryInstallation.Installation;
        var blueDoorVisuals = installation.LoadRoomObjectBlueDoorVisuals();
        var elevatorPlatformVisuals = installation.LoadRoomObjectElevatorPlatformVisuals();
        // The repository's installation binds the process's shared catalogs; a fresh load would be the check's own cost.
        var maps = InstalledFixtures.Maps;
        var departureProgress = new PersistentProgress();
        departureProgress.SetDefeatedBosses(AreaId.Ceres, DefeatedBosses.AreaBoss);
        var departure = SaveSnapshotFixtures.NewGame with
        {
            Checkpoint = SuperMetroid.Core.Progression.LoadStationDefinitions.CeresElevator,
            StartupRoute = SaveStartupRoute.CeresDestruction,
            Progress = departureProgress,
            Health = 77,
            GameTime = new SuperMetroid.Core.Progression.GameClockReading(frames: 44, seconds: 11, minutes: 2, hours: 0),
        };
        var saves = new SaveSlotStore(RetailPresentationFixture());
        saves.SaveSlot(SaveSlot.A, departure);
        var game = new SuperMetroidGame(saves);
        Action<SuperMetroidGame, bool> bind = BindInstalledArtwork(InstalledFixtures.GameArtwork, enemyArtwork: null);
        bind(game, true);
        game.BindArtwork(game.Artwork with
        {
            RoomObjectBlueDoorVisuals = blueDoorVisuals,
            RoomObjectElevatorPlatformVisuals = elevatorPlatformVisuals,
        });
        var options = new GameOptionsMenuState(AudioQueues.Standalone(), screenContent: maps, spriteSheets: MenuSheetFixtures.For(maps), controllerBindings: SuperMetroid.Core.Input.ControllerBindings.Default, iconCancelEnabled: false, moonwalkEnabled: false, japaneseText: false);
        options.SkipToPhase(GameOptionsPhase.StartGame);
        game.Session.Show(options);
        game.Session.ContinueSavedGame();
        game.Session.Enter(FileSelectStep.Options);
        byte[] savedBefore = SaveRamCodec.Encode(saves, RetailPresentationFixture());
        game.Step(0);
        AssertTrue(game.IsAt(CeresDestructionOutcome.SamusEscaped),
            "#1153 mode $22 dispatches the destruction cinematic, not the initial shaft/map");
        var runtime = game.GameplayRuntimeForVerification;
        AssertTrue(runtime.Room.ActiveRoom is null, "#1153 destruction load creates no Ceres room");
        AssertTrue(runtime.System.HasDefeatedAnyOf(AreaId.Ceres, DefeatedBosses.AreaBoss),
            "#1153 destruction load retains the defeated-Ridley flag");
        AssertEqual(77, runtime.Player.Samus.Health, "#1153 destruction load retains energy");
        AssertEqual(2, runtime.GameTime.Minutes, "#1153 destruction load retains minutes");
        AssertEqual(11, runtime.GameTime.Seconds, "#1153 destruction load retains seconds");
        AssertEqual(44, runtime.GameTime.Frames, "#1153 destruction load retains frames");
        AssertTrue(savedBefore.SequenceEqual(SaveRamCodec.Encode(saves, RetailPresentationFixture())),
            "#1153 normal load does not rewrite the checkpoint");

        // A resumed cinematic constructs a fresh gameplay owner, unlike an ongoing
        // escape. Its first Landing Site draw must have the same initialized HUD.
        var resumedScene = game.Session.ScreenAs<CeresDestructionCinematicState>();
        (resumedScene ?? throw new InvalidOperationException("Verification requires resumedScene.")).SkipToPhase(CeresDestructionPhase.Finished);
        game.Step(0);
        game.Step(0);
        // Loading spans its native NMI waits; the property here is reaching the fade.
        for (int wait = 0; wait < 256 && game.GameState != SuperMetroidGameState.GameplayFadeIn; wait++)
            game.Step(0);
        AssertEqual(SuperMetroidGameState.GameplayFadeIn, game.GameState,
            "#1153 normal mode-$22 load reaches the first Landing Site fade");
        game.Step(0);
        AssertTrue(runtime.Hud.IsInitialized,
            "#1153 normal mode-$22 load initializes the HUD before gameplay resumes");

        // A new game must write $1F before capturing the initial checkpoint. Reuse
        // immutable installed catalogs, but enter only the one reported room.
        var fresh = new SuperMetroidGame(new SaveSlotStore(RetailPresentationFixture()));
        bind(fresh, true);
        fresh.BindArtwork(fresh.Artwork with
        {
            RoomObjectBlueDoorVisuals = blueDoorVisuals,
            RoomObjectElevatorPlatformVisuals = elevatorPlatformVisuals,
        });
        fresh.Session.Enter(GameDataLoadRoute.SelectedGame);
        fresh.Step(0);
        var arrivalSlot = fresh.Saves.ReadSlot(SaveSlot.A);
        AssertEqual(SaveStartupRoute.CeresElevatorArrival, (arrivalSlot ?? throw new InvalidOperationException("Verification requires arrivalSlot.")).StartupRoute,
            "#1153 initial automatic save records native mode $1F");
        AssertEqual(DefeatedBosses.None, arrivalSlot.Progression.BossesDefeatedIn(AreaId.Ceres),
            "#1153 initial checkpoint does not activate the escape");

        // Reload the captured initial checkpoint through options, not Load State.
        var reloadSaves = SaveRamCodec.Decode(SaveRamCodec.Encode(fresh.Saves, RetailPresentationFixture()), RetailPresentationFixture());
        var reload = new SuperMetroidGame(reloadSaves);
        bind(reload, true);
        reload.BindArtwork(reload.Artwork with
        {
            RoomObjectBlueDoorVisuals = blueDoorVisuals,
            RoomObjectElevatorPlatformVisuals = elevatorPlatformVisuals,
        });
        var reloadOptions = new GameOptionsMenuState(AudioQueues.Standalone(), screenContent: maps, spriteSheets: MenuSheetFixtures.For(maps), controllerBindings: SuperMetroid.Core.Input.ControllerBindings.Default, iconCancelEnabled: false, moonwalkEnabled: false, japaneseText: false);
        reloadOptions.SkipToPhase(GameOptionsPhase.StartGame);
        reload.Session.Show(reloadOptions);
        reload.Session.ContinueSavedGame();
        reload.Session.Enter(FileSelectStep.Options);
        reload.Step(0);
        AssertTrue(reload.IsAt(GameDataLoadRoute.SelectedGame),
            "#1153 mode $1F dispatches Ceres initialization directly");
        reload.Step(0);
        var loaded = reload.GameplayRuntimeForVerification;
        AssertEqual(fresh.GameplayRuntimeForVerification.Room.LoadedRoom.State.Pointer,
            loaded.Room.LoadedRoom.State.Pointer, "#1153 initial room state is unchanged by normal reload");
        AssertTrue(!loaded.System.HasDefeatedAnyOf(AreaId.Ceres, DefeatedBosses.AreaBoss),
            "#1153 initial reload does not set the defeated-Ridley flag");
        AssertEqual(SuperMetroid.Core.Enemies.CeresEscapeStatus.RidleyNotEscaped, loaded.Enemies.RidleyEncounter.CeresStatus,
            "#1153 initial reload leaves station activation inactive");
        AssertTrue(loaded.Room.CeresElevatorArrival is not null,
            "#1153 initial reload uses the arrival, not ordinary save appearance");

        // #1275: the game-over menu runs no gameplay owner, so its updates keep counting
        // accepted NMIs in the frontend, and Yes on a $1F save reloads Ceres directly
        // ($81:9171) instead of fading into the area map.
        var gameOverProbe = new SuperMetroidGame(reloadSaves);
        bind(gameOverProbe, true);
        gameOverProbe.BindArtwork(gameOverProbe.Artwork with
        {
            RoomObjectBlueDoorVisuals = blueDoorVisuals,
            RoomObjectElevatorPlatformVisuals = elevatorPlatformVisuals,
        });
        var probeOptions = new GameOptionsMenuState(AudioQueues.Standalone(), screenContent: maps, spriteSheets: MenuSheetFixtures.For(maps), controllerBindings: SuperMetroid.Core.Input.ControllerBindings.Default, iconCancelEnabled: false, moonwalkEnabled: false, japaneseText: false);
        probeOptions.SkipToPhase(GameOptionsPhase.StartGame);
        gameOverProbe.Session.Show(probeOptions);
        gameOverProbe.Session.ContinueSavedGame();
        gameOverProbe.Session.Enter(FileSelectStep.Options);
        gameOverProbe.Step(0);
        for (int wait = 0; wait < 256 && gameOverProbe.IsAt(GameDataLoadRoute.SelectedGame); wait++)
            gameOverProbe.Step(0);
        int nmiBeforeDeath = gameOverProbe.GameplayRuntimeForVerification.Clock.Counter;
        // Layer-one fractions $090F/$0913 are written only by camera movement, so they
        // survive game over into the reloaded room.
        var deathCamera = gameOverProbe.GameplayRuntimeForVerification.Room.LoadedCamera;
        deathCamera.Position = deathCamera.Position.WithFractionX(0xbfff).WithFractionY(0x9c00);
        gameOverProbe.Session.DeathFade.Restart(1, 0);
        gameOverProbe.Session.Enter(DeathSequenceStep.FinalBlackOut);
        gameOverProbe.Step(0);
        AssertEqual(SuperMetroidGameState.GameOverMenu, gameOverProbe.GameState, "#1275 state $19 reaches the game-over menu");
        AssertTrue(gameOverProbe.RuntimeForVerification is null, "#1275 game over releases the gameplay owners");
        for (int update = 0; update < 3; update++)
            gameOverProbe.Step(0);
        AssertEqual(unchecked((ushort)(nmiBeforeDeath + 4)),
            gameOverProbe.Session.frameClock.Counter,
            "#1275 accepted NMIs keep counting through the game-over menu");
        var probeMenu = gameOverProbe.Session.ScreenAs<GameOverMenuState>();
        (probeMenu ?? throw new InvalidOperationException("Verification requires probeMenu.")).SkipToPhase(GameOverMenuPhase.Main);
        gameOverProbe.Step(0);
        gameOverProbe.Step(ControllerButton.A);
        AssertTrue(gameOverProbe.IsAt(GameDataLoadRoute.SelectedGame),
            "#1275 Yes on a $1F save dispatches the Ceres loader directly");
        gameOverProbe.Step(0);
        var reloadedCamera = gameOverProbe.GameplayRuntimeForVerification.Room.LoadedCamera;
        AssertEqual((ushort)0xbfff, reloadedCamera.Position.X.Fraction, "#1275 layer-one X fraction survives the game-over reload");
        AssertEqual((ushort)0x9c00, reloadedCamera.Position.Y.Fraction, "#1275 layer-one Y fraction survives the game-over reload");

        // Exercise the same atomic saver used at the escape blackout. A boss flag
        // must be captured alongside $22, not erased to manufacture a fresh station.
        loaded.System.RecordBossesDefeated(AreaId.Ceres, DefeatedBosses.AreaBoss);
        AutomaticCheckpointSaver.SaveCeresDeparture(reloadSaves, loaded, SaveSlot.A);
        var escapeSlot = reloadSaves.ReadSlot(SaveSlot.A);
        AssertEqual(SaveStartupRoute.CeresDestruction, (escapeSlot ?? throw new InvalidOperationException("Verification requires escapeSlot.")).StartupRoute,
            "#1153 escape-blackout saver records native mode $22");
        AssertEqual(DefeatedBosses.AreaBoss, escapeSlot.Progression.BossesDefeatedIn(AreaId.Ceres),
            "#1153 escape checkpoint retains defeated-Ridley progression");

        // Stub only the already-reported landing event, not an entire flight.
        // The production landing saver must reset $22 to $05 for later saves.
        loaded.Enemies.PassEvents.Publish(new GunshipEvent(GunshipFrameEvent.LandingCompleted));
        AssertTrue(AutomaticCheckpointSaver.TrySaveGunshipLanding(reloadSaves, loaded, SaveSlot.A),
            "#1153 landing checkpoint uses the production event saver");
        var landingSlot = reloadSaves.ReadSlot(SaveSlot.A);
        AssertEqual(SaveStartupRoute.MainGame, (landingSlot ?? throw new InvalidOperationException("Verification requires landingSlot.")).StartupRoute,
            "#1153 Zebes landing saver records native mode $05");
        AssertTrue(ReferenceEquals(SuperMetroid.Core.Progression.LoadStationDefinitions.Gunship, landingSlot.Checkpoint),
            "#1153 Zebes checkpoint is the gunship, not the Ceres start");

        // Do not infer a cinematic mode from area/boss flags in malformed old saves.
        // Preserve the persisted word and follow its dispatcher branch literally.
        saves.SaveSlot(SaveSlot.A, departure with
        {
            StartupRoute = SaveStartupRoute.MainGame,
        });
        var unmodified = new SuperMetroidGame(saves);
        unmodified.BindArtwork(InstalledFixtures.GameArtwork with
        {
            ScreenContent = maps,
        });
        var oldOptions = new GameOptionsMenuState(AudioQueues.Standalone(), screenContent: maps, spriteSheets: MenuSheetFixtures.For(maps), controllerBindings: SuperMetroid.Core.Input.ControllerBindings.Default, iconCancelEnabled: false, moonwalkEnabled: false, japaneseText: false);
        oldOptions.SkipToPhase(GameOptionsPhase.StartGame);
        unmodified.Session.Show(oldOptions);
        unmodified.Session.ContinueSavedGame();
        unmodified.Session.Enter(FileSelectStep.Options);
        unmodified.Step(0);
        AssertTrue(unmodified.IsAt(FileSelectStep.AreaMap),
            "#1153 mode $05 is not reinterpreted using an inferred compatibility rule");
        AssertEqual(SaveStartupRoute.MainGame,
            (saves.ReadSlot(SaveSlot.A) ?? throw new InvalidOperationException("Verification requires saves.ReadSlot(SaveSlot.A).")).StartupRoute,
            "#1153 malformed legacy slot is not silently rewritten");
        Console.WriteLine("#1153: native $1F/$22/$05 checkpoint writes; normal $1F/$22 reload routing, first Landing Site fade with initialized HUD, inactive initial station and preserved progress/SRAM; no inferred legacy recovery pass.");
    }

    /// <summary>Runs the Ceres checkpoint and reload scenarios against an installed game.</summary>
    [VerificationCommand("--ceres-save-startup")]
    internal static int CeresSaveStartupCommand()
    {
        Suite(nameof(VerifyCeresSaveStartup), () => VerifyCeresSaveStartup());
        return 0;
    }
}
