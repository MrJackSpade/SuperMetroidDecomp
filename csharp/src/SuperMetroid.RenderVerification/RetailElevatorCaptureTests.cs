using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Rendering.Direct3D11;

/// <summary>New-game Ceres arrival through the real frontend: fade, moving pad, landing and release.</summary>
internal static class RetailElevatorCaptureTests
{
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        byte[] rom = File.ReadAllBytes("Super Metroid.smc");
        var options = new SuperMetroidGameOptions { SkipOpeningCinematic = true };
        var legacy = new SuperMetroidGame(new SuperMetroidAddressSpace(rom), options);
        var captured = new SuperMetroidGame(new SuperMetroidAddressSpace(rom), options);
        var padPositions = new HashSet<ushort>();
        bool fade = false, arriving = false, landed = false;
        int samples = 0, releasedFrames = 0;
        for (int tick = 0; tick < 2000; tick++)
        {
            // Navigate only startup menus. Once the room owns the frontend, no
            // controller actions participate in elevator motion or completion.
            ushort input = legacy.RuntimeForVerification is null && tick % 47 == 0
                ? (ushort)SnesButton.Start : (ushort)0;
            var expected = legacy.Step(input);
            var actual = captured.StepCaptured(input, tick + 1, 1);
            if (actual.UsedLegacyRaster || expected.GameState != actual.Frame.GameState ||
                expected.Phase != actual.Frame.Phase || expected.FrameNumber != actual.Frame.FrameNumber ||
                !expected.AudioCommands.SequenceEqual(actual.Frame.AudioCommands))
                throw new InvalidOperationException($"Elevator frontend capture diverged at {tick}.");
            var arrival = legacy.RuntimeForVerification?.CeresElevatorArrival;
            if (arrival is null) continue;
            var capturedArrival = captured.RuntimeForVerification?.CeresElevatorArrival;
            if (capturedArrival is null || arrival.PadYPosition != capturedArrival.PadYPosition ||
                arrival.PlatformYPosition != capturedArrival.PlatformYPosition || arrival.IsComplete != capturedArrival.IsComplete)
                throw new InvalidOperationException("Elevator publication changed arrival state.");
            fade |= expected.GameState == SuperMetroidGameState.MainGameplayFadeIn;
            arriving |= expected.GameState == SuperMetroidGameState.MadeItToCeresElevator;
            landed |= arrival.IsComplete;
            padPositions.Add(arrival.PadYPosition);
            var packet = RenderFrameSnapshotCodec.Deserialize(RenderFrameSnapshotCodec.Serialize(actual.Snapshot!));
            PixelComparison.Verify(packet, expected.Pixels, renderer.RenderForReadback(packet),
                $"{device.Kind}: Ceres elevator {tick}, pad Y={arrival.PadYPosition}, complete={arrival.IsComplete}");
            samples++;
            if (arrival.IsComplete && expected.GameState == SuperMetroidGameState.MainGameplay &&
                legacy.RuntimeForVerification!.Samus is { InputLocked: false })
            {
                if (++releasedFrames == 60) break;
            }
        }
        if (!fade || !arriving || !landed || padPositions.Count < 10 || releasedFrames != 60)
            throw new InvalidOperationException("Elevator fixture missed fade, travel, landing or sustained control release.");
        Console.WriteLine($"{device.Kind}: {samples} exact Ceres elevator frames, {padPositions.Count} pad positions, fade through landing and 60 released frames.");
    }
}
