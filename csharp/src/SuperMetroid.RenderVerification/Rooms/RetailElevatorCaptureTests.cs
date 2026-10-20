using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Rendering.Direct3D11.Device;
using SuperMetroid.Rendering.Direct3D11.Frames;
namespace SuperMetroid.RenderVerification.Rooms;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.FrameCapture;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>New-game Ceres arrival through the real frontend: fade, moving pad, landing and release.</summary>
internal static class RetailElevatorCaptureTests
{
    /// <summary>Compares captured and direct frames from Ceres fade through landing and 60 released-control frames.</summary>
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        byte[] rom = File.ReadAllBytes("Super Metroid.smc");
        var options = new SuperMetroidGameOptions { SkipOpeningCinematic = true };
        var legacy = InstalledFixtures.CreateGame(new SuperMetroid.Core.Saves.SaveSlotStore(), options);
        var captured = InstalledFixtures.CreateGame(new SuperMetroid.Core.Saves.SaveSlotStore(), options);
        var padPositions = new HashSet<ushort>();
        bool fade = false, arriving = false, landed = false;
        int samples = 0, releasedFrames = 0;
        for (int tick = 0; tick < 2000; tick++)
        {
            // Navigate only startup menus. Once the room owns the frontend, no
            // controller actions participate in elevator motion or completion.
            SnesButton input = legacy.RuntimeForVerification is null && tick % 47 == 0
                ? SnesButton.Start : (ushort)0;
            var expected = legacy.Step(input);
            var actual = captured.StepCaptured(input, tick + 1, 1);
            if (expected.GameState != actual.Frame.GameState ||
                expected.Phase != actual.Frame.Phase || expected.FrameNumber != actual.Frame.FrameNumber ||
                !legacy.AudioEventsForVerification.SequenceEqual(captured.AudioEventsForVerification))
                throw new InvalidOperationException($"Elevator frontend capture diverged at {tick}.");
            var arrival = legacy.RuntimeForVerification?.Room.CeresElevatorArrival;
            if (arrival is null) continue;
            var capturedArrival = captured.RuntimeForVerification?.Room.CeresElevatorArrival;
            if (capturedArrival is null || arrival.PadPosition.Y != capturedArrival.PadPosition.Y ||
                arrival.PlatformPosition.Y != capturedArrival.PlatformPosition.Y || arrival.IsComplete != capturedArrival.IsComplete)
                throw new InvalidOperationException("Elevator publication changed arrival state.");
            fade |= expected.GameState == SuperMetroidGameState.MainGameplayFadeIn;
            arriving |= expected.GameState == SuperMetroidGameState.MainGameplay && !arrival.IsComplete;
            landed |= arrival.IsComplete;
            padPositions.Add(arrival.PadPosition.Y);
            var packet = actual.Snapshot;
            PixelComparison.Verify(packet, legacy.PublishedPixels(), renderer.RenderForReadback(packet),
                $"{device.Kind}: Ceres elevator {tick}, pad Y={arrival.PadPosition.Y}, complete={arrival.IsComplete}");
            samples++;
            if (arrival.IsComplete && expected.GameState == SuperMetroidGameState.MainGameplay &&
                legacy.GameplayRuntimeForVerification.Player.Samus is { Control.AlphaLocked: false })
            {
                if (++releasedFrames == 60) break;
            }
        }
        if (!fade || !arriving || !landed || padPositions.Count < 10 || releasedFrames != 60)
            throw new InvalidOperationException("Elevator fixture missed fade, travel, landing or sustained control release.");
        Console.WriteLine($"{device.Kind}: {samples} exact Ceres elevator frames, {padPositions.Count} pad positions, fade through landing and 60 released frames.");
    }
}
