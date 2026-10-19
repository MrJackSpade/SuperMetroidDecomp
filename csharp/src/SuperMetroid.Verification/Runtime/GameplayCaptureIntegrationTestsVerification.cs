using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.GraphDigestVerification;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Gameplay;
namespace SuperMetroid.Verification.Runtime;
using SuperMetroid.Verification.Samus.Projectiles;
using SuperMetroid.Core.Rooms;

/// <summary>Compares captured render packets with legacy frames during Ceres startup and a room pause cycle.</summary>
internal static class GameplayCaptureIntegrationTestsVerification
{
    /// <summary>Checks NMI window publication, overlay ordering, packet persistence, and pixel and audio parity through fades.</summary>
    [PpuLayoutBoundary("exact hardware test: checks PPU register numbers")]
    internal static void VerifyGameplayCaptureIntegration()
    {
        byte[] rom = File.ReadAllBytes(Path.GetFullPath("Super Metroid.smc"));
        var bus = new SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace(rom);
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug); runtime.FrameBoundary.BeginAcceptedFrame(0);
        runtime.RoomLoader.InitializeStartingCeresRoom(); runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.AlphaPowerBomb, new Position16<RoomPixels>(0, 0));
        runtime.FrameBoundary.BeginAcceptedFrame(0);

        // Chainsaw slot four stores a word at $68: WOBJLOG receives the low byte and TM
        // receives the high byte. Prove this is the runtime's shared cache rather than a
        // projectile-private diagnostic copy, and that only an accepted NMI publishes it.
        runtime.Projectiles.Chainsaw.ChainsawWindowRegisters.WriteWord(
            GameplayWindowRegisterAddresses.ObjectColorLogic,
            0x0412);
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        OrdinaryGameplayRegisters acceptedRegisters =
            ((OrdinaryGameplayRenderLayer)GameplayDisplayCapture.CaptureOrdinaryBase(runtime.PublishDisplayFrame())
                .Layers[0]).Registers;
        AssertEqual((byte)0x12, acceptedRegisters.Windows.ObjectColorLogic,
            "accepted NMI publishes Chainsaw WOBJLOG store");
        AssertEqual(SnesMainScreenLayers.Bg3, acceptedRegisters.MainScreenLayers,
            "accepted NMI publishes slot-four Chainsaw TM overwrite");
        runtime.Projectiles.Chainsaw.ChainsawWindowRegisters.InitializeWindowAndScreenSelection();
        runtime.FrameBoundary.BeginAcceptedFrame(0);

        runtime.BombProjectiles.PowerBombExplosion.Arm();
        runtime.BombProjectiles.PowerBombExplosion.Spawn(new(128, 120));
        for (int i = 0; i < 50; i++) runtime.BombProjectiles.PowerBombExplosion.StepFrame();
        runtime.MessageBox.Begin(GameplayMessageIds.MapDataAccessCompleted, 0);
        for (int i = 0; i < 13; i++) runtime.MessageBox.Step(0);
        runtime.SuitPickup.Begin(runtime.Player.IntroducedSamus, new(0, 0), SamusSuitPickupKind.Varia);
        runtime.SuitPickup.Step(runtime.Player.IntroducedSamus, runtime.Cgram);
        Rgba32[] expected = FrontendScenePixelsAccess.Pixels(runtime.PublishDisplayFrame());
        LayeredRenderSnapshot captured = GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame());
        AssertTrue(captured.Layers.Length == 4 && captured.Layers[0] is OrdinaryGameplayRenderLayer
            && captured.Layers[1] is ScanlineColorAddRenderLayer && captured.Layers[2] is MessageBoxRenderLayer
            && captured.Layers[3] is ScanlineColorAddRenderLayer, "base/Power Bomb/message/suit order");
        var retained = new RenderFrameSnapshot(new(1, 1, 0), captured);
        AssertTrue(expected.SequenceEqual(RenderForComparison(retained)), "composed gameplay overlay parity");
        runtime.MessageBox.Step(SnesButton.A);
        runtime.SuitPickup.Step(runtime.Player.IntroducedSamus, runtime.Cgram);
        runtime.BombProjectiles.PowerBombExplosion.StepFrame();
        AssertTrue(expected.SequenceEqual(RenderForComparison(retained)), "composed gameplay packet survives all effect updates");

        var options = new SuperMetroidGameOptions { SkipOpeningCinematic = true };
        AreaMapPresentationCatalog presentation = RetailPresentationFixture();
        var legacy = CreateRetailGameFixture(new SuperMetroid.Core.Saves.SaveSlotStore(), options);
        var packets = CreateRetailGameFixture(new SuperMetroid.Core.Saves.SaveSlotStore(), options);
        legacy.BindArtwork(legacy.Artwork with
        {
            MapPresentation = presentation,
        });
        packets.BindArtwork(packets.Artwork with
        {
            MapPresentation = presentation,
        });
        long sequence = 0;
        bool sawMode7Packet = false;
        for (int tick = 0; tick < 2000 && legacy.GameState != SuperMetroidGameState.MainGameplay; tick++)
        {
            SnesButton input = tick % 47 == 0 ? SnesButton.Start : (ushort)0;
            FrontendFrame reference = legacy.Step(input);
            CapturedFrontendFrame actual = packets.StepCaptured(input, ++sequence, 1);
            Compare(reference, actual);
            if (packets.RuntimeForVerification?.Room.ActiveDoor?.UsesCeresElevatorMode7 == true)
            {
                sawMode7Packet = true;
            }
        }
        AssertTrue(legacy.GameState == SuperMetroidGameState.MainGameplay && sawMode7Packet, "startup reaches playable Ceres through packets");
        // Direct fixture room loading avoids a cross-room controller marathon. The
        // following slice only exercises one room and its pause/fade dispatch states.
        foreach (SuperMetroidGame game in new[] { legacy, packets })
        {
            game.GameplayRuntimeForVerification.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.AlphaPowerBomb, new Position16<RoomPixels>(0, 0));
            game.GameplayRuntimeForVerification.FrameBoundary.BeginAcceptedFrame(0);
        }
        bool sawPause = false;
        var fadeFrames = new Dictionary<SuperMetroidGameState, int>
        {
            [SuperMetroidGameState.PausingDarkening] = 0,
            [SuperMetroidGameState.PausedA] = 0,
            [SuperMetroidGameState.UnpausingA] = 0,
            [SuperMetroidGameState.Unpausing] = 0,
        };
        // The cartridge fade counter adds a hold frame for each brightness step.
        // Include the complete 30-frame gameplay restoration, not the former
        // truncated 15-frame fade that happened to finish inside 160 frames.
        for (int tick = 0; tick < 180; tick++)
        {
            if (fadeFrames.ContainsKey(legacy.GameState)) fadeFrames[legacy.GameState]++;
            SnesButton input = tick is 10 or >= 100 and <= 105 ? SnesButton.Start : (ushort)0;
            FrontendFrame reference = legacy.Step(input);
            CapturedFrontendFrame actual = packets.StepCaptured(input, ++sequence, 1);
            Compare(reference, actual);
            sawPause |= legacy.GameState is SuperMetroidGameState.PausedA or SuperMetroidGameState.PausedB;
            AssertEqual(legacy.GameplaySamusX, packets.GameplaySamusX, "capture preserves Samus X");
            AssertEqual(legacy.GameplaySamusY, packets.GameplaySamusY, "capture preserves Samus Y");
            AssertEqual(legacy.GameplaySamusPose, packets.GameplaySamusPose, "capture preserves pose");
        }
        AssertTrue(sawPause && legacy.GameState == SuperMetroidGameState.MainGameplay,
            $"ordinary capture slice must complete unpause; ended in {legacy.GameState}");
        foreach (var phase in fadeFrames)
            AssertEqual(30, phase.Value, $"cartridge pause fade duration for {phase.Key}");
        Console.WriteLine("  Gameplay capture integration: ordered overlays and 180-frame room/pause slice preserve pixels, state and audio commands; all four fades take 30 frames.");

        void Compare(FrontendFrame expected, CapturedFrontendFrame actual)
        {
            AssertEqual(expected.GameState, actual.Frame.GameState, "gameplay captured state cadence");
            AssertEqual(expected.Phase, actual.Frame.Phase, "gameplay captured phase");
            AssertSequenceEqual(legacy.AudioEventsForVerification, packets.AudioEventsForVerification, "gameplay capture audio event order");
            AssertTrue(legacy.PublishedPixels().SequenceEqual(SoftwareFrameSnapshotRenderer.Render(actual.Snapshot)),
                $"gameplay capture pixels in {expected.Phase}");
        }
    }
}
