using System.Reflection;
using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// #1153: confirm the native Ceres checkpoint mode, not a playthrough. Enter
    /// the actual options dispatcher at its completed fade with checksummed SRAM.
    /// The cinematic checkpoint must never construct an escaping Ceres room.
    /// </summary>
    private static void VerifyCeresSaveStartup(string installationRoot, EnemyTileArtworkCatalog? enemyArtwork = null)
    {
        var installation = new GameInstallation(Path.GetFullPath(installationRoot));
        var maps = installation.LoadMaps();
        var bossBytes = new byte[Bank80SystemState.AreaCount];
        bossBytes[(int)AreaId.Ceres] = (byte)BossBits.AreaBoss;
        var departure = new SuperMetroidSaveSnapshot
        {
            Area = (ushort)AreaId.Ceres,
            SaveStation = 0,
            LoadingGameState = SaveLoadingGameStates.CeresDestruction,
            BossBytes = bossBytes,
            Health = 77,
            GameTimeMinutes = 2,
            GameTimeSeconds = 11,
            GameTimeFrames = 44,
        };
        var bus = SuperMetroidAddressSpace.CreateWithoutCartridge();
        new SuperMetroidSaveRam(bus, RetailPresentationFixture()).SaveSlot(0, departure);
        var game = new SuperMetroidGame(bus);
        Action<SuperMetroidGame, bool> bind = PrepareRomFreeBindings(installation, enemyArtwork);
        bind(game, true);
        game.BindRoomPlmBlueDoorVisuals(installation.LoadRoomPlmBlueDoorVisuals());
        game.BindRoomPlmElevatorPlatformVisuals(installation.LoadRoomPlmElevatorPlatformVisuals());
        var options = new GameOptionsMenuState(bus, mapPresentation: maps);
        typeof(GameOptionsMenuState).GetProperty(nameof(options.Phase))!
            .SetValue(options, GameOptionsPhase.FadeOutToIntro);
        SetField(game, "options", options);
        SetField(game, "loadingExistingSave", true);
        SetState(game, SuperMetroidGameState.GameOptionsMenu);
        byte[] savedBefore = bus.SaveRam.ToArray();
        game.Step(0);
        AssertEqual(SuperMetroidGameState.CeresGoesBoom, game.GameState,
            "#1153 mode $22 dispatches the destruction cinematic, not the initial shaft/map");
        var runtime = game.RuntimeForVerification!;
        AssertTrue(runtime.ActiveRoom is null, "#1153 destruction load creates no Ceres room");
        AssertTrue(runtime.System.HasAnyBossBits(AreaId.Ceres, BossBits.AreaBoss),
            "#1153 destruction load retains the defeated-Ridley flag");
        AssertEqual((ushort)77, runtime.Samus!.Health, "#1153 destruction load retains energy");
        AssertEqual((ushort)2, runtime.GameTime.Minutes, "#1153 destruction load retains minutes");
        AssertEqual((ushort)11, runtime.GameTime.Seconds, "#1153 destruction load retains seconds");
        AssertEqual((ushort)44, runtime.GameTime.Frames, "#1153 destruction load retains frames");
        AssertTrue(savedBefore.AsSpan().SequenceEqual(bus.SaveRam),
            "#1153 normal load does not rewrite the checkpoint");

        // A resumed cinematic constructs a fresh gameplay owner, unlike an ongoing
        // escape. Its first Landing Site draw must have the same initialized HUD.
        var resumedScene = (CeresDestructionCinematicState)typeof(SuperMetroidGame)
            .GetField("ceresDestruction", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(game)!;
        typeof(CeresDestructionCinematicState).GetProperty(nameof(resumedScene.Phase))!
            .SetValue(resumedScene, CeresDestructionPhase.Finished);
        game.Step(0);
        game.Step(0);
        for (int wait = 0; wait < 15; wait++) game.Step(0);
        AssertEqual(SuperMetroidGameState.MainGameplayFadeIn, game.GameState,
            "#1153 normal mode-$22 load reaches the first Landing Site fade");
        game.Step(0);
        AssertTrue(runtime.Hud.IsInitialized,
            "#1153 normal mode-$22 load initializes the HUD before gameplay resumes");

        // A new game must write $1F before capturing the initial checkpoint. Reuse
        // immutable installed catalogs, but enter only the one reported room.
        var freshBus = SuperMetroidAddressSpace.CreateWithoutCartridge();
        var fresh = new SuperMetroidGame(freshBus);
        bind(fresh, true);
        fresh.BindRoomPlmBlueDoorVisuals(installation.LoadRoomPlmBlueDoorVisuals());
        fresh.BindRoomPlmElevatorPlatformVisuals(installation.LoadRoomPlmElevatorPlatformVisuals());
        SetState(fresh, SuperMetroidGameState.SetUpNewGame);
        fresh.Step(0);
        var arrivalSlot = new SuperMetroidSaveRam(freshBus, RetailPresentationFixture()).ReadSlot(0)!;
        AssertEqual(SaveLoadingGameStates.CeresElevatorArrival, arrivalSlot.LoadingGameState,
            "#1153 initial automatic save records native mode $1F");
        AssertEqual((byte)0, arrivalSlot.BossBytes[(int)AreaId.Ceres],
            "#1153 initial checkpoint does not activate the escape");

        // Reload the captured initial checkpoint through options, not Load State.
        var reloadBus = SuperMetroidAddressSpace.CreateWithoutCartridge();
        freshBus.SaveRam.CopyTo(reloadBus.SaveRam);
        var reload = new SuperMetroidGame(reloadBus);
        bind(reload, true);
        reload.BindRoomPlmBlueDoorVisuals(installation.LoadRoomPlmBlueDoorVisuals());
        reload.BindRoomPlmElevatorPlatformVisuals(installation.LoadRoomPlmElevatorPlatformVisuals());
        var reloadOptions = new GameOptionsMenuState(reloadBus, mapPresentation: maps);
        typeof(GameOptionsMenuState).GetProperty(nameof(reloadOptions.Phase))!
            .SetValue(reloadOptions, GameOptionsPhase.FadeOutToIntro);
        SetField(reload, "options", reloadOptions);
        SetField(reload, "loadingExistingSave", true);
        SetState(reload, SuperMetroidGameState.GameOptionsMenu);
        reload.Step(0);
        AssertEqual(SuperMetroidGameState.SetUpNewGame, reload.GameState,
            "#1153 mode $1F dispatches Ceres initialization directly");
        reload.Step(0);
        var loaded = reload.RuntimeForVerification!;
        AssertEqual(fresh.RuntimeForVerification!.ActiveRoom!.State.Pointer,
            loaded.ActiveRoom!.State.Pointer, "#1153 initial room state is unchanged by normal reload");
        AssertTrue(!loaded.System.HasAnyBossBits(AreaId.Ceres, BossBits.AreaBoss),
            "#1153 initial reload does not set the defeated-Ridley flag");
        AssertEqual((ushort)0, loaded.Enemies.CeresStatus,
            "#1153 initial reload leaves station activation inactive");
        AssertTrue(loaded.CeresElevatorArrival is not null,
            "#1153 initial reload uses the arrival, not ordinary save appearance");

        // Exercise the same atomic saver used at the escape blackout. A boss flag
        // must be captured alongside $22, not erased to manufacture a fresh station.
        loaded.System.SetBossBits(AreaId.Ceres, BossBits.AreaBoss);
        AutomaticCheckpointSaver.SaveCeresDeparture(reloadBus, loaded, 0);
        var escapeSlot = new SuperMetroidSaveRam(reloadBus, RetailPresentationFixture()).ReadSlot(0)!;
        AssertEqual(SaveLoadingGameStates.CeresDestruction, escapeSlot.LoadingGameState,
            "#1153 escape-blackout saver records native mode $22");
        AssertEqual((byte)BossBits.AreaBoss, escapeSlot.BossBytes[(int)AreaId.Ceres],
            "#1153 escape checkpoint retains defeated-Ridley progression");

        // Stub only the already-reported landing event, not an entire flight.
        // The production landing saver must reset $22 to $05 for later saves.
        typeof(RoomEnemySystem).GetProperty(nameof(loaded.Enemies.LastGunshipEvent))!
            .SetValue(loaded.Enemies, GunshipFrameEvent.LandingCompleted);
        AssertTrue(AutomaticCheckpointSaver.TrySaveGunshipLanding(reloadBus, loaded, 0),
            "#1153 landing checkpoint uses the production event saver");
        var landingSlot = new SuperMetroidSaveRam(reloadBus, RetailPresentationFixture()).ReadSlot(0)!;
        AssertEqual(SaveLoadingGameStates.MainGame, landingSlot.LoadingGameState,
            "#1153 Zebes landing saver records native mode $05");
        AssertEqual((ushort)AreaId.Crateria, landingSlot.Area,
            "#1153 Zebes checkpoint does not retain Ceres as its area");

        // Do not infer a cinematic mode from area/boss flags in malformed old saves.
        // Preserve the persisted word and follow its dispatcher branch literally.
        new SuperMetroidSaveRam(bus, RetailPresentationFixture()).SaveSlot(0, departure with
        {
            LoadingGameState = SaveLoadingGameStates.MainGame,
        });
        var unmodified = new SuperMetroidGame(bus);
        unmodified.BindMapPresentation(maps);
        var oldOptions = new GameOptionsMenuState(bus, mapPresentation: maps);
        typeof(GameOptionsMenuState).GetProperty(nameof(oldOptions.Phase))!
            .SetValue(oldOptions, GameOptionsPhase.FadeOutToIntro);
        SetField(unmodified, "options", oldOptions);
        SetField(unmodified, "loadingExistingSave", true);
        SetState(unmodified, SuperMetroidGameState.GameOptionsMenu);
        unmodified.Step(0);
        AssertEqual(SuperMetroidGameState.FileSelectMap, unmodified.GameState,
            "#1153 mode $05 is not reinterpreted using an inferred compatibility rule");
        AssertEqual(SaveLoadingGameStates.MainGame,
            new SuperMetroidSaveRam(bus, RetailPresentationFixture()).ReadSlot(0)!.LoadingGameState,
            "#1153 malformed legacy slot is not silently rewritten");
        Console.WriteLine("#1153: native $1F/$22/$05 checkpoint writes; normal $1F/$22 reload routing, first Landing Site fade with initialized HUD, inactive initial station and preserved progress/SRAM; no inferred legacy recovery pass.");
    }

    private static void SetState(SuperMetroidGame owner, SuperMetroidGameState state) =>
        typeof(SuperMetroidGame).GetProperty(nameof(owner.GameState))!.SetValue(owner, state);

    private static void SetField(SuperMetroidGame owner, string name, object value) =>
        typeof(SuperMetroidGame).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(owner, value);
}
