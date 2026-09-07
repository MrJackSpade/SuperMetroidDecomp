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
        foreach (ushort room in new[] { SlowConsumerFixture.AlphaPowerBombRoom, SlowConsumerFixture.MaridiaTubeRoom })
            foreach (bool blockConsumer in new[] { false, true })
                RunSlowConsumerAudioRoom(selection, room, blockConsumer);
    }

    private static void RunSlowConsumerAudioRoom(D3D11RenderDevice selection, ushort room, bool blockConsumer)
    {
        byte[] rom = File.ReadAllBytes("Super Metroid.smc");
        var options = new SuperMetroidGameOptions { SkipOpeningCinematic = true };
        var legacy = new SuperMetroidGame(new SuperMetroidAddressSpace(rom), options);
        var captured = new SuperMetroidGame(new SuperMetroidAddressSpace(rom), options);
        var headless = new SuperMetroidGame(new SuperMetroidAddressSpace(rom), options);
        // Compile the real desktop audio adapter into this diagnostic executable;
        // do not substitute a second hand-written queue-to-PCM implementation.
        using var legacyAudio = new SpcAudioEngine();
        using var capturedAudio = new SpcAudioEngine();
        using var headlessAudio = new SpcAudioEngine();
        long sequence = 0, pcmSamples = 0, nonzero = 0;
        CapturedFrontendFrame Step(ushort input)
        {
            var expected = legacy.Step(input);
            var actual = captured.StepCaptured(input, ++sequence, 1);
            // No raster request, GPU publication, or reference render for this owner.
            var undrawn = headless.StepCaptured(input, sequence, 1);
            if (undrawn.UsedLegacyRaster || undrawn.Frame.GameState != actual.Frame.GameState ||
                undrawn.Frame.Phase != actual.Frame.Phase || undrawn.Frame.FrameNumber != actual.Frame.FrameNumber ||
                !undrawn.Frame.AudioCommands.SequenceEqual(actual.Frame.AudioCommands))
                throw new InvalidOperationException($"Headless frame/command divergence at {sequence}.");
            if (expected.GameState != actual.Frame.GameState || expected.Phase != actual.Frame.Phase ||
                expected.FrameNumber != actual.Frame.FrameNumber ||
                !expected.AudioCommands.SequenceEqual(actual.Frame.AudioCommands))
                throw new InvalidOperationException($"Slow-consumer game/audio command divergence at {sequence}.");
            var left = legacyAudio.RenderFrame(expected.AudioCommands);
            var right = capturedAudio.RenderFrame(actual.Frame.AudioCommands);
            var silentDisplay = headlessAudio.RenderFrame(undrawn.Frame.AudioCommands);
            if (!right.SequenceEqual(silentDisplay)) throw new InvalidOperationException($"Headless PCM divergence at {sequence}.");
            if (!left.SequenceEqual(right)) throw new InvalidOperationException($"PCM divergence at {sequence}.");
            pcmSamples += right.Length;
            foreach (short sample in right) if (sample != 0) nonzero++;
            legacy.SetAudioAcknowledgements(legacyAudio.ReadAcknowledgements());
            captured.SetAudioAcknowledgements(capturedAudio.ReadAcknowledgements());
            headless.SetAudioAcknowledgements(headlessAudio.ReadAcknowledgements());
            if (legacy.GameplaySamusX != captured.GameplaySamusX || legacy.GameplaySamusY != captured.GameplaySamusY ||
                legacy.GameplaySamusPose != captured.GameplaySamusPose)
                throw new InvalidOperationException($"Samus state divergence at {sequence}.");
            return actual;
        }
        for (int tick = 0; tick < 2000 && legacy.GameState != SuperMetroidGameState.MainGameplay; tick++)
            Step(tick % 47 == 0 ? (ushort)SnesButton.Start : (ushort)0);
        if (legacy.GameState != SuperMetroidGameState.MainGameplay) throw new InvalidOperationException("Audio fixture failed startup.");
        // Room-local pause slice, matching the portable capture fixture; no traversal.
        foreach (var game in new[] { legacy, captured, headless })
        {
            game.RuntimeForVerification!.LoadCartridgeRoomForDebug(room, 0, 0);
            game.RuntimeForVerification.RunNmi(0, true);
        }
        nint window = CreateWindowExW(0, "STATIC", "Hidden slow-consumer test", 0, 0, 0, 640, 480, 0, 0, 0, 0);
        if (window == 0) throw new Win32Exception(Marshal.GetLastWin32Error());
        using var entered = new ManualResetEventSlim(); using var release = new ManualResetEventSlim();
        if (!blockConsumer) release.Set();
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
            CompareRuntimeGraphs("before room slice");
            var blockedTime = System.Diagnostics.Stopwatch.StartNew();
            bool paused = false;
            for (int tick = 0; tick < 160; tick++)
            {
                var actual = Step(tick == 10 || tick is >= 100 and <= 105 ? (ushort)SnesButton.Start : (ushort)0);
                worker.Publish(actual.Snapshot ?? throw new InvalidOperationException("Capture fell back to raster."));
                paused |= actual.Frame.GameState is SuperMetroidGameState.PausedA or SuperMetroidGameState.PausedB;
                if (tick % 40 == 39) CompareRuntimeGraphs($"room slice tick {tick}");
            }
            if (blockConsumer) PumpUntil(() => blockedTime.ElapsedMilliseconds >= 250);
            if ((blockConsumer && release.IsSet) || !paused || nonzero == 0 || legacy.GameState != SuperMetroidGameState.MainGameplay)
                throw new InvalidOperationException("Slow-consumer coverage incomplete.");
            release.Set();
            PumpUntil(() => { worker.ThrowIfFaulted(); return worker.LastConsumedSequence == sequence; });
            if (blockConsumer && worker.MailboxMetrics.Replaced < 159) throw new InvalidOperationException("Blocked visuals were not superseded as expected.");
            CompareRuntimeGraphs("after GPU resumes");
            Console.WriteLine($"{selection.Kind}: room {room:X4}, software/GPU/headless agree; deliberate GPU block={blockConsumer}; 160 room/pause frames advanced; {pcmSamples} exact PCM samples ({nonzero} nonzero); latest frame consumed.");
        }
        finally
        {
            release.Set();
            if (worker is not null) { var stop = worker.StopAsync(); PumpUntil(() => stop.IsCompleted); stop.GetAwaiter().GetResult(); }
            if (!DestroyWindow(window)) throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        void CompareRuntimeGraphs(string context)
        {
            // Reuse the exact-build debugger graph writer rather than a hand-picked
            // list of gameplay properties. Private fields, arrays, aliases, cycles
            // and delegates are included. No state is deserialized or normalized.
            using var expected = new MemoryStream();
            using var actual = new MemoryStream();
            DebuggerObjectGraphSerializer.Serialize(expected, legacy.RuntimeForVerification!);
            DebuggerObjectGraphSerializer.Serialize(actual, captured.RuntimeForVerification!);
            using var undrawn = new MemoryStream();
            DebuggerObjectGraphSerializer.Serialize(undrawn, headless.RuntimeForVerification!);
            if (!expected.GetBuffer().AsSpan(0, checked((int)expected.Length))
                .SequenceEqual(undrawn.GetBuffer().AsSpan(0, checked((int)undrawn.Length))))
                throw new InvalidOperationException($"Headless runtime graph diverged: {context}.");
            if (!expected.GetBuffer().AsSpan(0, checked((int)expected.Length))
                .SequenceEqual(actual.GetBuffer().AsSpan(0, checked((int)actual.Length))))
                throw new InvalidOperationException($"Exact runtime graph diverged: {context} (lengths {expected.Length}/{actual.Length}).");
            Console.WriteLine($"  Exact runtime graph agrees: {context}, {expected.Length} bytes.");
        }
    }
}

internal static class SlowConsumerFixture
{
    /// <summary>Retail $8F:A3AE room header: room-local gameplay/pause fixture, matching portable capture tests.</summary>
    internal const ushort AlphaPowerBombRoom = 0xa3ae;
    /// <summary>Retail $8F:CEFB tube room: bounded Maridia liquid/pause coverage.</summary>
    internal const ushort MaridiaTubeRoom = 0xcefb;
}
