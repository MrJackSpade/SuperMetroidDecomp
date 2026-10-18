using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Gameplay;
namespace SuperMetroid.Verification.Samus.Abilities;
using SuperMetroid.Verification.Harness;

/// <summary>Confirms X-ray admission reads the live game state (#1335).</summary>
internal static class XrayGameStateVerification
{
    /// <summary>
    /// <c>XraySetup</c> (<c>$91:E16D</c>) rejects activation unless game state <c>$0998</c>
    /// is eight. States <c>$0C</c> (pause darkening) and <c>$12</c> (unpausing) run the whole
    /// state-eight routine behind the INIDISP fade, so Samus's alpha pass reaches the
    /// selected-X-ray HUD path with held Run, but setup must still refuse (#1335).
    /// </summary>
    internal static void VerifyXrayGameState()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        (SuperMetroidGame Game, SuperMetroidRuntime Runtime, SamusState Samus) CreateWithXraySelected()
        {
            SuperMetroidRuntime runtime = CreateRetailRuntimeFixture();
            runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
            runtime.RoomLoader.InitializeStartingCeresRoom();
            runtime.RoomLoader.InitializeCeresStartSamus();
            runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.XrayScope);
            SamusState samus = runtime.Player.IntroducedSamus;
            samus.InputLocked = false;
            samus.Pose = SamusPoseId.FacingRightNormalPose;
            samus.PoseMetadata.RefreshCollisionRadii();
            samus.Presentation.InitializeAnimation();
            // Open floor in the Scope chamber, as used by the X-ray input fixture.
            samus.Position = samus.Position.WithWhole(new(128, 139));
            samus.EquippedItems = SamusEquipmentFlags.XrayScope;
            samus.Missiles = samus.SuperMissiles = samus.PowerBombs = 0;
            var game = CreateRetailGameFixture(new SuperMetroid.Core.Saves.SaveSlotStore(), renderGameplayFrames: false);
            game.Session.runtime = runtime;
            game.Session.lastAudioRoomStatePointer = runtime.Room.LoadedRoom.State.Pointer;
            game.GameState = SuperMetroidGameState.MainGameplay;
            game.Step(0);
            game.Step(SnesButton.Select);
            AssertEqual(SuperMetroidGameState.MainGameplay, game.GameState, "selecting X-ray stays in state eight");
            AssertEqual(HudItem.XRay, samus.SelectedHudItem, "Select reaches the equipped X-ray");
            AssertTrue(!samus.Xray.IsActive, "selection alone does not activate X-ray");
            return (game, runtime, samus);
        }

        var (game, runtime, samus) = CreateWithXraySelected();
        // State $0C: Start publishes the darkening state; the next dispatch runs gameplay.
        {
            game.Step(SnesButton.Start);
            AssertEqual(SuperMetroidGameState.PausingDarkening, game.GameState, "Start begins pause darkening");
            game.Step(runtime.Options.ControllerBindings.Dash);
            AssertEqual(SuperMetroidGameState.PausingDarkening, game.GameState, "the held-Run frame is a darkening gameplay frame");
            AssertEqual(SuperMetroidGameState.PausingDarkening, runtime.DispatchGameState, "the runtime sees the live darkening state");
            AssertTrue(!samus.Xray.IsActive, "state $0C rejects X-ray setup");
            AssertTrue(!runtime.Freeze.TimeIsFrozen, "a rejected setup does not freeze gameplay");
        }

        // State $12: the unpause fade also runs the state-eight routine.
        {
            game.Session.pauseBrightness = 0;
            game.GameState = SuperMetroidGameState.Unpausing;
            game.Step(runtime.Options.ControllerBindings.Dash);
            AssertEqual(SuperMetroidGameState.Unpausing, game.GameState, "the held-Run frame is an unpausing gameplay frame");
            AssertEqual(SuperMetroidGameState.Unpausing, runtime.DispatchGameState, "the runtime sees the live unpausing state");
            AssertTrue(!samus.Xray.IsActive, "state $12 rejects X-ray setup");
            AssertTrue(!runtime.Freeze.TimeIsFrozen, "a rejected setup does not freeze gameplay");
        }

        // State eight still admits the same held-Run input.
        {
            game.GameState = SuperMetroidGameState.MainGameplay;
            game.Step(runtime.Options.ControllerBindings.Dash);
            AssertEqual(SuperMetroidGameState.MainGameplay, game.GameState, "the held-Run frame is a state-eight frame");
            AssertTrue(samus.Xray.IsActive, "state eight admits X-ray setup");
            AssertTrue(runtime.Freeze.TimeIsFrozen, "admitted X-ray freezes gameplay");
        }
        Console.WriteLine("  X-ray game state: setup rejects states $0C and $12 and admits state eight.");
    }

    /// <summary>Runs the X-ray game state suite.</summary>
    [VerificationCommand("--xray-game-state")]
    internal static int XrayGameStateCommand()
    {
        Suite(nameof(VerifyXrayGameState), () => VerifyXrayGameState());
        return 0;
    }
}
