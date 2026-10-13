using SuperMetroid.Core.Audio;
using System.ComponentModel;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Rendering.Direct3D11;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Desktop;
using System.Diagnostics;

/// <summary>Hidden-window checks for D3D11 swapchain presentation, worker recovery and bounded frame publication.</summary>
internal static partial class SwapchainTests
{
    /// <summary>Reads the worker mailbox's private publication counters under its own lock.</summary>
    private static RenderMailboxCounterView MailboxCounters(D3D11RenderWorker worker) =>
        worker.mailbox.VerificationCounters;

    /// <summary>Checks worker startup, latest-frame mailbox accounting, retained redraw, resize and both device-loss recoveries.</summary>
    internal static void RunWorker(D3D11RenderDevice selection)
    {
        nint window = CreateWindowExW(0, "STATIC", "Hidden GPU worker verification", 0, 0, 0, 640, 480, 0, 0, 0, 0);
        if (window == 0) throw new Win32Exception(Marshal.GetLastWin32Error());
        D3D11RenderWorker? worker = null;
        try
        {
            int injectedLoss = 0;
            worker = new(window, new Extent16<DisplayPixels>(640, 480), 1, selection.Kind, () =>
            {
                int failure = Interlocked.Exchange(ref injectedLoss, 0);
                if (failure != 0) new SharpGen.Runtime.Result(failure).CheckError();
            });
            PumpUntil(() => worker.Ready.IsCompleted);
            string adapter = worker.Ready.GetAwaiter().GetResult();
            if (!adapter.Contains($"backend={selection.Kind}") ||
                !adapter.Contains($"feature level={selection.ActualFeatureLevel}"))
                throw new InvalidOperationException("Worker startup omitted actual backend/feature-level diagnostics.");
            // Allow the worker to observe initial readiness while the mailbox is empty.
            var idle = System.Diagnostics.Stopwatch.StartNew();
            PumpUntil(() => idle.ElapsedMilliseconds >= 40);
            for (int sequence = 1; sequence <= 1000; sequence++)
                worker.Publish(new(new(sequence, 1, 0), new Rgba32(19,73,129)));
            PumpUntil(() => { worker.ThrowIfFaulted(); return worker.LastConsumedSequence == 1000; });
            long previousRedraws = worker.RetainedRedraws;
            worker.Resize(new Extent16<DisplayPixels>(800, 600));
            PumpUntil(() => { worker.ThrowIfFaulted(); return worker.RetainedRedraws > previousRedraws && worker.LastDrawnSize == new Extent16<DisplayPixels>(800, 600); });
            if (MailboxCounters(worker).Published != 1000 || worker.LastConsumedSequence != 1000)
                throw new InvalidOperationException("Retained redraw consumed or manufactured a simulation frame.");
            previousRedraws = worker.RetainedRedraws;
            Interlocked.Exchange(ref injectedLoss, (int)DxgiDeviceLoss.DeviceRemoved);
            worker.Resize(new Extent16<DisplayPixels>(801, 601));
            PumpUntil(() => { worker.ThrowIfFaulted(); return worker.DeviceRecoveries == 1 &&
                worker.RetainedRedraws > previousRedraws && worker.LastDrawnSize == new Extent16<DisplayPixels>(801, 601); });
            if (MailboxCounters(worker).Published != 1000 || worker.LastConsumedSequence != 1000)
                throw new InvalidOperationException("Device recovery manufactured a simulation frame.");
            var loss = worker.LastDeviceLoss;
            if (loss is null || loss.FailureHResult != (int)DxgiDeviceLoss.DeviceRemoved ||
                loss.RemovalReason != 0 || loss.Adapter != selection.AdapterDescription || loss.Backend != selection.Kind ||
                loss.Target.X != 801 || loss.Target.Y != 601 || loss.Frame?.Sequence != 1000)
                throw new InvalidOperationException("Device-loss diagnostic lost original failure/device/frame context.");
            worker.AdvanceGeneration(2);
            worker.Resize(new Extent16<DisplayPixels>(319, 601));
            Interlocked.Exchange(ref injectedLoss, (int)DxgiDeviceLoss.DeviceReset);
            worker.Publish(new(new(1001,2,0), new Rgba32(91,27,173)));
            PumpUntil(() => { worker.ThrowIfFaulted(); return worker.DeviceRecoveries == 2 && worker.LastConsumedSequence == 1001; });
            if (worker.LastDeviceLoss?.FailureHResult != (int)DxgiDeviceLoss.DeviceReset ||
                (worker.LastDeviceLoss ?? throw new InvalidOperationException("Verification requires worker.LastDeviceLoss.")).Frame?.Generation != 2)
                throw new InvalidOperationException("Reset diagnostic retained obsolete frame/failure context.");
            RenderMailboxCounterView metrics = MailboxCounters(worker);
            var timings = worker.CaptureTimings();
            if (timings.CpuComposition.Observed == 0 || timings.CpuDisplayAndPresent.Observed == 0)
                throw new InvalidOperationException("Worker did not record CPU submission/presentation timings.");
            if (metrics.HasPendingFrame || metrics.Published != metrics.Taken + metrics.Replaced + metrics.Invalidated)
                throw new InvalidOperationException("GPU worker lost mailbox accounting.");
            Console.WriteLine($"{selection.Kind}: worker {adapter}; idle-start, 1001 publications, retained redraw, two device-loss recoveries, generation/resize, bounded accounting passed.");
        }
        finally
        {
            if (worker is not null)
            {
                Task stop = worker.StopAsync(); PumpUntil(() => stop.IsCompleted); stop.GetAwaiter().GetResult();
            }
            if (!DestroyWindow(window)) throw new Win32Exception(Marshal.GetLastWin32Error());
        }
        VerifyRecoveryFailure(selection.Kind, repeatDeviceLoss: true);
        VerifyRecoveryFailure(selection.Kind, repeatDeviceLoss: false);
        VerifySurfaceSuspension(selection.Kind);
    }

    /// <summary>Pumps Win32 messages while waiting up to ten seconds for an asynchronous worker condition.</summary>
    private static void PumpUntil(Func<bool> finished)
    {
        var timeout = System.Diagnostics.Stopwatch.StartNew();
        while (!finished())
        {
            if (timeout.Elapsed > TimeSpan.FromSeconds(10)) throw new TimeoutException("GPU worker verification timed out.");
            while (PeekMessageW(out var message, 0, 0, 0, 1))
            { TranslateMessage(in message); DispatchMessageW(in message); }
            Thread.Sleep(1);
        }
    }

    /// <summary>Blittable Win32 MSG payload used to keep hidden verification windows responsive.</summary>
    [StructLayout(LayoutKind.Sequential)]
    [SpatialComponents("Win32 interop: the blittable MSG layout flattens its POINT cursor field into two ints.")]
    private struct WindowMessage
    {
        /// <summary>Target HWND supplied by the Win32 message queue.</summary>
        public nint Window;
        /// <summary>Win32 message identifier.</summary>
        public uint Id;
        /// <summary>Pointer-sized first message parameter.</summary>
        public nuint WParam;
        /// <summary>Pointer-sized second message parameter.</summary>
        public nint LParam;
        /// <summary>Queue timestamp in the native MSG layout.</summary>
        public uint Time;
        /// <summary>Cursor X coordinate from the flattened Win32 POINT.</summary>
        public int X;
        /// <summary>Cursor Y coordinate from the flattened Win32 POINT.</summary>
        public int Y;
        /// <summary>Private queue field retained for the full MSG layout.</summary>
        public uint Private;
    }
    /// <summary>Dequeues pending Win32 messages while hidden GPU windows are active.</summary>
    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool PeekMessageW(out WindowMessage message, nint window, uint min, uint max, uint flags);
    /// <summary>Produces character messages from a dequeued keyboard event.</summary>
    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool TranslateMessage(in WindowMessage message);
    /// <summary>Dispatches a dequeued message to its hidden window procedure.</summary>
    [LibraryImport("user32.dll")]
    private static partial nint DispatchMessageW(in WindowMessage message);

    /// <summary>Checks flip-swapchain readiness, generation rejection, frame identity and several hidden-window resizes.</summary>
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        // Predefined STATIC class, no WS_VISIBLE, no activation or message dialog.
        nint window = CreateWindowExW(0, "STATIC", "Hidden renderer verification", 0,
            0, 0, 640, 480, 0, 0, 0, 0);
        if (window == 0) throw new Win32Exception(Marshal.GetLastWin32Error());
        try
        {
            using var presenter = new D3D11SwapchainPresenter(device, window, new Extent16<DisplayPixels>(640, 480));
            var gate = new RenderPresentationGate(1);
            var packet = new RenderFrameSnapshot(new(1, 1, 0), new Rgba32(173,91,27));
            renderer.Render(packet);
            _ = presenter.TryAcquireFrameOpportunity();
            var outcome = presenter.Present(renderer, packet.Identity, gate, null);
            if (outcome == D3D11PresentationResult.StaleGeneration) throw new InvalidOperationException("Current frame was rejected.");
            gate.AdvanceGeneration(2);
            if (presenter.Present(renderer, packet.Identity, gate, null) != D3D11PresentationResult.StaleGeneration)
                throw new InvalidOperationException("Swapchain presented stale generation.");
            bool mismatch = false;
            try { presenter.Present(renderer, new(2,2,0), gate, null); }
            catch (InvalidOperationException) { mismatch = true; }
            if (!mismatch) throw new InvalidOperationException("Swapchain accepted mismatched GPU frame identity.");
            foreach (var size in new Extent16<DisplayPixels>[] { new(319, 601), new(1, 1), new(800, 600) })
            {
                presenter.Resize(size);
                packet = new(new(packet.Identity.Sequence + 1, 2, 0), new Rgba32(27,91,173));
                renderer.Render(packet);
                outcome = presenter.Present(renderer, packet.Identity, gate, null);
                if (outcome == D3D11PresentationResult.StaleGeneration) throw new InvalidOperationException("Resized frame was rejected.");
            }
            Console.WriteLine($"{device.Kind}: hidden flip swapchain creation, readiness, resize and generation checks passed; last status {outcome}.");
        }
        finally { if (!DestroyWindow(window)) throw new Win32Exception(Marshal.GetLastWin32Error()); }
    }

    /// <summary>Creates a nonvisible STATIC window for swapchain and worker checks.</summary>
    [LibraryImport("user32.dll", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
    private static partial nint CreateWindowExW(uint extendedStyle, string className, string windowName,
        uint style, int x, int y, int width, int height, nint parent, nint menu, nint instance, nint parameter);
    /// <summary>Releases the hidden window after worker or presenter shutdown.</summary>
    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool DestroyWindow(nint window);

    /// <summary>Checks that a blocked GPU consumer does not let a queued gameplay door transition corrupt frame identity or progress.</summary>
    internal static void VerifyBlockedDoor(D3D11RenderDevice device, D3D11FrameRenderer renderer,
        SuperMetroidRuntime runtime, ISnesAddressSpace bus, ushort door, ushort destination, string context)
    {
        nint window = CreateWindowExW(0, "STATIC", "Hidden door backlog verification", 0, 0, 0, 640, 480, 0, 0, 0, 0);
        if (window == 0) throw new Win32Exception(Marshal.GetLastWin32Error());
        using var entered = new ManualResetEventSlim();
        using var release = new ManualResetEventSlim();
        D3D11RenderWorker? worker = null;
        try
        {
            worker = new(window, new Extent16<DisplayPixels>(640, 480), 1, device.Kind, () =>
            {
                entered.Set();
                if (!release.Wait(TimeSpan.FromSeconds(60))) throw new TimeoutException("Door consumer release missing.");
            });
            PumpUntil(() => worker.Ready.IsCompleted);
            worker.Ready.GetAwaiter().GetResult();
            long sequence = 1;
            worker.Publish(new(new(sequence, 1, 0), GameplayDisplayCapture.CaptureFrame(runtime)));
            PumpUntil(() => entered.IsSet);
            var clock = Stopwatch.StartNew();
            RenderFrameSnapshot? latest = null;
            RetailDoorCaptureTests.VerifyTransition(device, renderer, runtime, bus, new RetailDoorCase { Door = door, Destination = destination }, context, packet =>
                {
                    latest = new(new(++sequence, 1, packet.Identity.SimulationFrame), (packet.Layers ?? throw new InvalidOperationException("Verification requires packet.Layers.")), packet.BrightnessPasses);
                    worker.Publish(latest);
                });
            PumpUntil(() => clock.ElapsedMilliseconds >= 250);
            LatestRenderFrameMailbox mailbox = worker.mailbox;
            if (latest is null || worker.LastConsumedSequence != 0 || !mailbox.VerificationCounters.HasPendingFrame ||
                worker.MailboxMetrics.Replaced != sequence - 2)
                throw new InvalidOperationException("Door transition failed to advance under the held consumer with a bounded mailbox.");
            release.Set();
            PumpUntil(() => { worker.ThrowIfFaulted(); return worker.LastConsumedSequence == sequence; });
            RenderMailboxCounterView resumed = mailbox.VerificationCounters;
            if (resumed.HasPendingFrame || resumed.Taken != 2)
                throw new InvalidOperationException("Resumed door consumer did not skip superseded transition packets.");
            PixelComparison.Verify(latest, SuperMetroidRuntimeFrameRenderer.Render(runtime),
                renderer.RenderForReadback(latest), $"{device.Kind}: retained destination after blocked {context} door");
            Console.WriteLine($"{device.Kind}: blocked {context} door completed; {sequence - 2} replaced frames, latest destination consumed.");
        }
        finally
        {
            release.Set();
            if (worker is not null)
            {
                Task stop = worker.StopAsync(); PumpUntil(() => stop.IsCompleted); stop.GetAwaiter().GetResult();
            }
            if (!DestroyWindow(window)) throw new Win32Exception(Marshal.GetLastWin32Error());
        }
    }

    /// <summary>Checks bounded recreation attempts and preservation of the original terminal worker fault.</summary>
    private static void VerifyRecoveryFailure(D3D11DeviceKind kind, bool repeatDeviceLoss)
    {
        nint window = CreateWindowExW(0, "STATIC", "Hidden recovery failure verification", 0, 0, 0, 640, 480, 0, 0, 0, 0);
        if (window == 0) throw new Win32Exception(Marshal.GetLastWin32Error());
        D3D11RenderWorker? worker = null;
        int attempts = 0;
        try
        {
            worker = new(window, new Extent16<DisplayPixels>(640, 480), 1, kind, () =>
            {
                Interlocked.Increment(ref attempts);
                if (repeatDeviceLoss) new SharpGen.Runtime.Result((int)DxgiDeviceLoss.DeviceRemoved).CheckError();
                throw new InvalidDataException("Injected coverage failure must not be retried.");
            });
            PumpUntil(() => worker.Ready.IsCompleted);
            worker.Ready.GetAwaiter().GetResult();
            worker.Publish(new(new(1, 1, 0), new Rgba32(1, 2, 3)));
            PumpUntil(() => worker.Completion.IsCompleted);
            if (!worker.Completion.IsFaulted) throw new InvalidOperationException("Renderer failure was suppressed.");
            int expected = repeatDeviceLoss ? D3D11RecoveryPolicy.MaximumConsecutiveRecreations + 1 : 1;
            if (attempts != expected || worker.DeviceRecoveries != expected - 1)
                throw new InvalidOperationException($"Recovery limit mismatch: {attempts} attempts, {worker.DeviceRecoveries} recreations.");
            // Observe the original fault through the same health API used by the desktop.
            Exception? observed = null;
            try { worker.ThrowIfFaulted(); } catch (Exception error) { observed = error; }
            if (repeatDeviceLoss ? observed?.HResult != (int)DxgiDeviceLoss.DeviceRemoved : observed is not InvalidDataException)
                throw new InvalidOperationException("Renderer fault lost its original type/HRESULT.");
            Console.WriteLine($"{kind}: repeated loss={repeatDeviceLoss}; {attempts} attempts then original failure surfaced.");
        }
        finally
        {
            if (worker is not null)
            {
                Task stop = worker.StopAsync(); PumpUntil(() => stop.IsCompleted);
                _ = stop.Exception; // Expected terminal fault has already been asserted above.
            }
            if (!DestroyWindow(window)) throw new Win32Exception(Marshal.GetLastWin32Error());
        }
    }

    /// <summary>Compares real desktop audio under normal and blocked GPU consumption in two recorded room slices.</summary>
    internal static void RunSlowConsumerAudio(D3D11RenderDevice selection)
    {
        foreach (ushort room in new[] { SlowConsumerFixture.AlphaPowerBombRoom, SlowConsumerFixture.MaridiaTubeRoom })
            foreach (bool blockConsumer in new[] { false, true })
                RunSlowConsumerAudioRoom(selection, room, blockConsumer);
    }

    /// <summary>Runs one room-local controller and pause slice through legacy, captured and headless audio paths.</summary>
    private static void RunSlowConsumerAudioRoom(D3D11RenderDevice selection, ushort room, bool blockConsumer)
    {
        byte[] rom = File.ReadAllBytes("Super Metroid.smc");
        var options = new SuperMetroidGameOptions { SkipOpeningCinematic = true };
        var legacy = RenderRetailFixture.CreateGame(new SuperMetroid.AssetExtraction.CartridgeImportAddressSpace(rom), options);
        var captured = RenderRetailFixture.CreateGame(new SuperMetroid.AssetExtraction.CartridgeImportAddressSpace(rom), options);
        var headless = RenderRetailFixture.CreateGame(new SuperMetroid.AssetExtraction.CartridgeImportAddressSpace(rom), options);
        // Each session synthesizes through its own Core audio session, as the desktop host does;
        // do not substitute a second hand-written queue-to-PCM implementation.
        var audioAssets = DesktopAccess.LoadAudioAssets();
        legacy.BindAudio(audioAssets);
        captured.BindAudio(audioAssets);
        headless.BindAudio(audioAssets);
        var left = new short[CartridgeAudioSession.PcmSamplesPerFrame];
        var right = new short[CartridgeAudioSession.PcmSamplesPerFrame];
        var silentDisplay = new short[CartridgeAudioSession.PcmSamplesPerFrame];
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
            legacy.CopyAudioFrame(left);
            captured.CopyAudioFrame(right);
            headless.CopyAudioFrame(silentDisplay);
            if (!right.SequenceEqual(silentDisplay)) throw new InvalidOperationException($"Headless PCM divergence at {sequence}.");
            if (!left.SequenceEqual(right)) throw new InvalidOperationException($"PCM divergence at {sequence}.");
            pcmSamples += right.Length;
            foreach (short sample in right) if (sample != 0) nonzero++;
            if (legacy.GameplaySamusX != captured.GameplaySamusX || legacy.GameplaySamusY != captured.GameplaySamusY ||
                legacy.GameplaySamusPose != captured.GameplaySamusPose ||
                headless.GameplaySamusX != captured.GameplaySamusX || headless.GameplaySamusY != captured.GameplaySamusY ||
                headless.GameplaySamusPose != captured.GameplaySamusPose)
                throw new InvalidOperationException($"Samus state divergence at {sequence}.");
            return actual;
        }
        for (int tick = 0; tick < 2000 && legacy.GameState != SuperMetroidGameState.MainGameplay; tick++)
            Step(tick % 47 == 0 ? (ushort)SnesButton.Start : (ushort)0);
        if (legacy.GameState != SuperMetroidGameState.MainGameplay) throw new InvalidOperationException("Audio fixture failed startup.");
        // Room-local pause slice, matching the portable capture fixture; no traversal.
        foreach (var game in new[] { legacy, captured, headless })
        {
            game.GameplayRuntimeForVerification.LoadCartridgeRoomForDebug(room, new Position16<RoomPixels>(0, 0));
            game.GameplayRuntimeForVerification.NmiHandler.RunNmi(0, true);
        }
        nint window = CreateWindowExW(0, "STATIC", "Hidden slow-consumer test", 0, 0, 0, 640, 480, 0, 0, 0, 0);
        if (window == 0) throw new Win32Exception(Marshal.GetLastWin32Error());
        using var entered = new ManualResetEventSlim(); using var release = new ManualResetEventSlim();
        if (!blockConsumer) release.Set();
        D3D11RenderWorker? worker = null;
        try
        {
            worker = new(window, new Extent16<DisplayPixels>(640, 480), 1, selection.Kind, () =>
            {
                entered.Set();
                if (!release.Wait(TimeSpan.FromSeconds(60))) throw new TimeoutException("Slow-consumer release missing.");
            });
            PumpUntil(() => worker.Ready.IsCompleted); worker.Ready.GetAwaiter().GetResult();
            worker.Publish((Step(0).Snapshot ?? throw new InvalidOperationException("Verification requires Step(0).Snapshot.")));
            PumpUntil(() => entered.IsSet);
            CompareRuntimeGraphs("before room slice");
            var blockedTime = System.Diagnostics.Stopwatch.StartNew();
            bool paused = false;
            for (int tick = 0; tick < 160; tick++)
            {
                var actual = Step(tick is 10 or >= 100 and <= 105 ? (ushort)SnesButton.Start : (ushort)0);
                worker.Publish(actual.Snapshot ?? throw new InvalidOperationException("Capture fell back to raster."));
                paused |= actual.Frame.GameState is SuperMetroidGameState.PausedA or SuperMetroidGameState.PausedB;
                if (tick % 40 == 39) CompareRuntimeGraphs($"room slice tick {tick}");
            }
            var positions = new HashSet<(ushort?, ushort?)>();
            var poses = new HashSet<SamusPoseId?>();
            var roomBeforeMovement = legacy.GameplayActiveRoomIdentity;
            // Bounded room-local inputs: held movement, single-tick direction taps,
            // jump/shoot edges and explicit releases. No room traversal is intended.
            for (int tick = 0; tick < 64; tick++)
            {
                ushort input = tick switch
                {
                    < 8 => (ushort)SnesButton.Right,
                    10 or 12 => (ushort)SnesButton.Left,
                    >= 16 and < 24 => (ushort)(SnesButton.Left | SnesButton.B),
                    28 or 30 => (ushort)SnesButton.X,
                    >= 40 and < 48 => (ushort)SnesButton.Right,
                    _ => 0,
                };
                var actual = Step(input);
                worker.Publish(actual.Snapshot ?? throw new InvalidOperationException("Movement capture fell back to raster."));
                positions.Add((legacy.GameplaySamusX, legacy.GameplaySamusY));
                poses.Add(legacy.GameplaySamusPose);
                if (legacy.GameplayActiveRoomIdentity != roomBeforeMovement)
                    throw new InvalidOperationException("Movement scheduling fixture crossed a room boundary.");
                if (tick % 16 == 15) CompareRuntimeGraphs($"movement input tick {tick}");
            }
            if (positions.Count < 2 || poses.Count < 2)
                throw new InvalidOperationException("Movement input fixture did not change position and pose.");
            if (blockConsumer) PumpUntil(() => blockedTime.ElapsedMilliseconds >= 250);
            if ((blockConsumer && release.IsSet) || !paused || nonzero == 0 || legacy.GameState != SuperMetroidGameState.MainGameplay)
                throw new InvalidOperationException("Slow-consumer coverage incomplete.");
            release.Set();
            PumpUntil(() => { worker.ThrowIfFaulted(); return worker.LastConsumedSequence == sequence; });
            if (blockConsumer && worker.MailboxMetrics.Replaced < 223) throw new InvalidOperationException("Blocked visuals were not superseded as expected.");
            CompareRuntimeGraphs("after GPU resumes");
            Console.WriteLine($"{selection.Kind}: room {room:X4}, software/GPU/headless agree; deliberate GPU block={blockConsumer}; 160 room/pause + 64 movement/input frames; {positions.Count} positions, {poses.Count} poses; {pcmSamples} exact PCM samples ({nonzero} nonzero); latest frame consumed.");
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
            DebuggerObjectGraphSerializer.Serialize(expected, legacy.GameplayRuntimeForVerification);
            DebuggerObjectGraphSerializer.Serialize(actual, captured.GameplayRuntimeForVerification);
            using var undrawn = new MemoryStream();
            DebuggerObjectGraphSerializer.Serialize(undrawn, headless.GameplayRuntimeForVerification);
            if (!expected.GetBuffer().AsSpan(0, checked((int)expected.Length))
                .SequenceEqual(undrawn.GetBuffer().AsSpan(0, checked((int)undrawn.Length))))
                throw new InvalidOperationException($"Headless runtime graph diverged: {context}.");
            if (!expected.GetBuffer().AsSpan(0, checked((int)expected.Length))
                .SequenceEqual(actual.GetBuffer().AsSpan(0, checked((int)actual.Length))))
                throw new InvalidOperationException($"Exact runtime graph diverged: {context} (lengths {expected.Length}/{actual.Length}).");
            Console.WriteLine($"  Exact runtime graph agrees: {context}, {expected.Length} bytes.");
        }
    }

    /// <summary>Checks zero-size suspension, latest-frame replacement, generation invalidation and restore without presenting while suspended.</summary>
    private static void VerifySurfaceSuspension(D3D11DeviceKind kind)
    {
        // Exercise the real worker and swapchain without showing or minimizing the
        // player's windows. Zero-width/height are the host's suspended-surface input.
        nint window = CreateWindowExW(0, "STATIC", "Hidden surface suspension verification",
            0, 0, 0, 640, 480, 0, 0, 0, 0);
        if (window == 0) throw new Win32Exception(Marshal.GetLastWin32Error());
        D3D11RenderWorker? worker = null;
        try
        {
            worker = new(window, new Extent16<DisplayPixels>(640, 480), 1, kind);
            PumpUntil(() => worker.Ready.IsCompleted);
            worker.Ready.GetAwaiter().GetResult();
            long sequence = 1, generation = 1;
            worker.Publish(new(new(sequence, generation, 0), new Rgba32(23, 71, 127)));
            PumpUntil(() => { worker.ThrowIfFaulted(); return worker.LastConsumedSequence == sequence; });

            foreach (var size in new Extent16<DisplayPixels>[] { new(0, 480), new(640, 0), new(0, 0) })
            {
                worker.Resize(size);
                PumpUntil(() => { worker.ThrowIfFaulted(); return worker.IsSurfaceSuspended; });
                long consumed = worker.LastConsumedSequence;
                long submissions = worker.SubmittedUploadCalls;
                long presentations = worker.PresentedFrames + worker.OccludedFrames;
                long replacements = worker.MailboxMetrics.Replaced;
                for (int i = 0; i < 32; i++)
                    worker.Publish(new(new(++sequence, generation, 0), new Rgba32(127, 71, 23)));

                // The acknowledgement prevents an arbitrary sleep from masking an
                // unprocessed resize. Wait across multiple actual worker wake cycles.
                var elapsed = Stopwatch.StartNew();
                PumpUntil(() => { worker.ThrowIfFaulted(); return elapsed.ElapsedMilliseconds >= 250; });
                if (worker.LastConsumedSequence != consumed || worker.SubmittedUploadCalls != submissions ||
                    worker.PresentedFrames + worker.OccludedFrames != presentations ||
                    !MailboxCounters(worker).HasPendingFrame || worker.MailboxMetrics.Replaced != replacements + 31)
                    throw new InvalidOperationException("Suspended surface consumed/rendered frames or lost bounded latest-frame accounting.");

                worker.AdvanceGeneration(++generation);
                if (MailboxCounters(worker).HasPendingFrame)
                    throw new InvalidOperationException("Generation change retained a queued pre-load frame while suspended.");
                worker.Publish(new(new(++sequence, generation, 0), new Rgba32(43, 91, 173)));
                worker.Resize(new Extent16<DisplayPixels>(641, 481));
                PumpUntil(() => { worker.ThrowIfFaulted(); return !worker.IsSurfaceSuspended &&
                    worker.LastConsumedSequence == sequence && worker.LastDrawnSize == new Extent16<DisplayPixels>(641, 481); });
                RenderMailboxCounterView metrics = MailboxCounters(worker);
                if (metrics.HasPendingFrame || metrics.Generation != generation ||
                    metrics.Published != metrics.Taken + metrics.Replaced + metrics.Invalidated)
                    throw new InvalidOperationException("Surface restore lost latest generation/mailbox accounting.");
            }

            worker.Resize(new Extent16<DisplayPixels>(0, 0));
            PumpUntil(() => { worker.ThrowIfFaulted(); return worker.IsSurfaceSuspended; });
            worker.Publish(new(new(++sequence, generation, 0), new Rgba32(1, 2, 3)));
            Task stop = worker.StopAsync();
            PumpUntil(() => stop.IsCompleted);
            stop.GetAwaiter().GetResult();
            Console.WriteLine($"{kind}: zero width/height suspension, bounded publication, generation invalidation, latest-frame restore and suspended shutdown passed.");
        }
        finally
        {
            if (worker is not null)
            {
                Task stop = worker.StopAsync();
                PumpUntil(() => stop.IsCompleted);
                stop.GetAwaiter().GetResult();
            }
            if (!DestroyWindow(window)) throw new Win32Exception(Marshal.GetLastWin32Error());
        }
    }
}
