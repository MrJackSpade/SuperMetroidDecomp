using System.ComponentModel;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Desktop;
using SuperMetroid.Rendering.Direct3D11;

internal static partial class SwapchainTests
{
    internal static void RunSlowConsumerAudio(D3D11RenderDevice selection)
    {
        byte[] rom = File.ReadAllBytes("Super Metroid.smc");
        var options = new SuperMetroidGameOptions { SkipOpeningCinematic = true };
        var legacy = new SuperMetroidGame(new SuperMetroidAddressSpace(rom), options);
        var captured = new SuperMetroidGame(new SuperMetroidAddressSpace(rom), options);
        // Compile the real desktop audio adapter into this diagnostic executable;
        // do not substitute a second hand-written queue-to-PCM implementation.
        using var legacyAudio = new SpcAudioEngine();
        using var capturedAudio = new SpcAudioEngine();
        long sequence = 0, pcmSamples = 0, nonzero = 0;
        CapturedFrontendFrame Step(ushort input)
        {
            var expected = legacy.Step(input);
            var actual = captured.StepCaptured(input, ++sequence, 1);
            if (expected.GameState != actual.Frame.GameState || expected.Phase != actual.Frame.Phase ||
                expected.FrameNumber != actual.Frame.FrameNumber ||
                !expected.AudioCommands.SequenceEqual(actual.Frame.AudioCommands))
                throw new InvalidOperationException($"Slow-consumer game/audio command divergence at {sequence}.");
            var left = legacyAudio.RenderFrame(expected.AudioCommands);
            var right = capturedAudio.RenderFrame(actual.Frame.AudioCommands);
            if (!left.SequenceEqual(right)) throw new InvalidOperationException($"PCM divergence at {sequence}.");
            pcmSamples += right.Length;
            foreach (short sample in right) if (sample != 0) nonzero++;
            legacy.SetAudioAcknowledgements(legacyAudio.ReadAcknowledgements());
            captured.SetAudioAcknowledgements(capturedAudio.ReadAcknowledgements());
            if (legacy.GameplaySamusX != captured.GameplaySamusX || legacy.GameplaySamusY != captured.GameplaySamusY ||
                legacy.GameplaySamusPose != captured.GameplaySamusPose)
                throw new InvalidOperationException($"Samus state divergence at {sequence}.");
            return actual;
        }
        for (int tick = 0; tick < 2000 && legacy.GameState != SuperMetroidGameState.MainGameplay; tick++)
            Step(tick % 47 == 0 ? (ushort)SnesButton.Start : (ushort)0);
        if (legacy.GameState != SuperMetroidGameState.MainGameplay) throw new InvalidOperationException("Audio fixture failed startup.");
        // Room-local pause slice, matching the portable capture fixture; no traversal.
        foreach (var game in new[] { legacy, captured })
        {
            game.RuntimeForVerification!.LoadCartridgeRoomForDebug(SlowConsumerFixture.AlphaPowerBombRoom, 0, 0);
            game.RuntimeForVerification.RunNmi(0, true);
        }
        nint window = CreateWindowExW(0, "STATIC", "Hidden slow-consumer test", 0, 0, 0, 640, 480, 0, 0, 0, 0);
        if (window == 0) throw new Win32Exception(Marshal.GetLastWin32Error());
        using var entered = new ManualResetEventSlim(); using var release = new ManualResetEventSlim();
        D3D11RenderWorker? worker = null;
        try
        {
            worker = new(window, 640, 480, 1, selection.Kind, () =>
            {
                entered.Set();
                if (!release.Wait(TimeSpan.FromSeconds(60))) throw new TimeoutException("Slow-consumer release missing.");
            });
            PumpUntil(() => worker.Ready.IsCompleted); worker.Ready.GetAwaiter().GetResult();
            worker.Publish(Step(0).Snapshot!);
            PumpUntil(() => entered.IsSet);
            var blockedTime = System.Diagnostics.Stopwatch.StartNew();
            bool paused = false;
            for (int tick = 0; tick < 160; tick++)
            {
                var actual = Step(tick == 10 || tick is >= 100 and <= 105 ? (ushort)SnesButton.Start : (ushort)0);
                worker.Publish(actual.Snapshot ?? throw new InvalidOperationException("Capture fell back to raster."));
                paused |= actual.Frame.GameState is SuperMetroidGameState.PausedA or SuperMetroidGameState.PausedB;
            }
            PumpUntil(() => blockedTime.ElapsedMilliseconds >= 250);
            if (release.IsSet || !paused || nonzero == 0 || legacy.GameState != SuperMetroidGameState.MainGameplay)
                throw new InvalidOperationException("Slow-consumer coverage incomplete.");
            release.Set();
            PumpUntil(() => { worker.ThrowIfFaulted(); return worker.LastConsumedSequence == sequence; });
            if (worker.MailboxMetrics.Replaced < 159) throw new InvalidOperationException("Blocked visuals were not superseded as expected.");
            Console.WriteLine($"{selection.Kind}: blocked GPU owner >250ms; 160 room/pause frames advanced; {pcmSamples} exact PCM samples ({nonzero} nonzero); latest frame consumed.");
        }
        finally
        {
            release.Set();
            if (worker is not null) { var stop = worker.StopAsync(); PumpUntil(() => stop.IsCompleted); stop.GetAwaiter().GetResult(); }
            if (!DestroyWindow(window)) throw new Win32Exception(Marshal.GetLastWin32Error());
        }
    }
}

internal static class SlowConsumerFixture
{
    /// <summary>Retail $8F:A3AE room header: room-local gameplay/pause fixture, matching portable capture tests.</summary>
    internal const ushort AlphaPowerBombRoom = 0xa3ae;
}
