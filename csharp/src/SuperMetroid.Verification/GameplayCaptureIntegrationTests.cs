using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyGameplayCaptureIntegration()
    {
        byte[] rom = File.ReadAllBytes(Path.GetFullPath("Super Metroid.smc"));
        var bus = new SuperMetroidAddressSpace(rom);
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug); runtime.RunNmi(0, true);
        runtime.InitializeStartingCeresRoom(); runtime.InitializeCeresStartSamus();
        AssertTrue(GameplayDisplayCapture.TryCaptureFrame(runtime) is null, "Mode-7 capture remains explicitly unavailable");
        runtime.LoadCartridgeRoomForDebug(PowerBombRuntimeVerificationDefinitions.AlphaPowerBombRoomHeader, 0, 0);
        runtime.RunNmi(0, true);
        runtime.BombProjectiles.PowerBombExplosion.Arm();
        runtime.BombProjectiles.PowerBombExplosion.Spawn(128, 120);
        for (int i = 0; i < 50; i++) runtime.BombProjectiles.PowerBombExplosion.StepFrame(bus);
        runtime.MessageBox.Begin(bus, GameplayMessageIds.MapDataAccessCompleted);
        for (int i = 0; i < 13; i++) runtime.MessageBox.Step(0);
        runtime.SuitPickup.Begin(bus, runtime.Samus!, 0, 0, SamusSuitPickupKind.Varia);
        runtime.SuitPickup.Step(bus, runtime.Samus!, runtime.Cgram);
        Rgba32[] expected = SuperMetroidRuntimeFrameRenderer.Render(runtime);
        LayeredRenderSnapshot captured = GameplayDisplayCapture.TryCaptureFrame(runtime)!;
        AssertTrue(captured.Layers.Length == 4 && captured.Layers[0] is OrdinaryGameplayRenderLayer
            && captured.Layers[1] is ScanlineColorAddRenderLayer && captured.Layers[2] is MessageBoxRenderLayer
            && captured.Layers[3] is ScanlineColorAddRenderLayer, "base/Power Bomb/message/suit order");
        var retained = RoundTripRenderPacket(new(new(1, 1, 0), captured));
        AssertTrue(expected.AsSpan().SequenceEqual(SoftwareFrameSnapshotRenderer.Render(retained)), "composed gameplay overlay parity");
        runtime.MessageBox.Step((ushort)SnesButton.A);
        runtime.SuitPickup.Step(bus, runtime.Samus!, runtime.Cgram);
        runtime.BombProjectiles.PowerBombExplosion.StepFrame(bus);
        AssertTrue(expected.AsSpan().SequenceEqual(SoftwareFrameSnapshotRenderer.Render(retained)), "composed gameplay packet survives all effect updates");

        var options = new SuperMetroidGameOptions { SkipOpeningCinematic = true };
        var legacy = new SuperMetroidGame(new SuperMetroidAddressSpace(rom), options);
        var packets = new SuperMetroidGame(new SuperMetroidAddressSpace(rom), options);
        long sequence = 0;
        bool sawMode7Fallback = false;
        for (int tick = 0; tick < 2000 && legacy.GameState != SuperMetroidGameState.MainGameplay; tick++)
        {
            ushort input = tick % 47 == 0 ? (ushort)SnesButton.Start : (ushort)0;
            FrontendFrame reference = legacy.Step(input);
            CapturedFrontendFrame actual = packets.StepCaptured(input, ++sequence, 1);
            Compare(reference, actual);
            sawMode7Fallback |= actual.UsedLegacyRaster && packets.RuntimeForVerification?.ActiveDoor?.UsesCeresElevatorMode7 == true;
        }
        AssertTrue(legacy.GameState == SuperMetroidGameState.MainGameplay && sawMode7Fallback, "startup reaches playable Ceres with explicit fallback");
        // Direct fixture room loading avoids a cross-room controller marathon. The
        // following slice only exercises one room and its pause/fade dispatch states.
        foreach (SuperMetroidGame game in new[] { legacy, packets })
        {
            game.RuntimeForVerification!.LoadCartridgeRoomForDebug(PowerBombRuntimeVerificationDefinitions.AlphaPowerBombRoomHeader, 0, 0);
            game.RuntimeForVerification.RunNmi(0, true);
        }
        bool sawPause = false;
        for (int tick = 0; tick < 160; tick++)
        {
            ushort input = tick == 10 || tick is >= 100 and <= 105 ? (ushort)SnesButton.Start : (ushort)0;
            FrontendFrame reference = legacy.Step(input);
            CapturedFrontendFrame actual = packets.StepCaptured(input, ++sequence, 1);
            Compare(reference, actual);
            AssertTrue(!actual.UsedLegacyRaster, "ordinary gameplay/pause uses packets");
            sawPause |= legacy.GameState is SuperMetroidGameState.PausedA or SuperMetroidGameState.PausedB;
            AssertEqual(legacy.GameplaySamusX, packets.GameplaySamusX, "capture preserves Samus X");
            AssertEqual(legacy.GameplaySamusY, packets.GameplaySamusY, "capture preserves Samus Y");
            AssertEqual(legacy.GameplaySamusPose, packets.GameplaySamusPose, "capture preserves pose");
        }
        AssertTrue(sawPause && legacy.GameState == SuperMetroidGameState.MainGameplay,
            $"ordinary capture slice must complete unpause; ended in {legacy.GameState}");
        Console.WriteLine("  Gameplay capture integration: ordered overlays and 160-frame ordinary room/pause slice preserve pixels, state and audio commands.");

        static void Compare(FrontendFrame expected, CapturedFrontendFrame actual)
        {
            AssertEqual(expected.GameState, actual.Frame.GameState, "gameplay captured state cadence");
            AssertEqual(expected.Phase, actual.Frame.Phase, "gameplay captured phase");
            AssertSequenceEqual(expected.AudioCommands, actual.Frame.AudioCommands, "gameplay capture audio command order");
            Rgba32[] pixels = actual.Snapshot is { } packet ? SoftwareFrameSnapshotRenderer.Render(packet) : actual.Frame.Pixels;
            AssertTrue(expected.Pixels.AsSpan().SequenceEqual(pixels), $"gameplay capture pixels in {expected.Phase}");
        }
    }
}
