using SuperMetroid.Core.Assets;
using System.ComponentModel;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Rendering.Direct3D11;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;
using System.Diagnostics;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Diagnostics.SaveStates;
using SuperMetroid.RenderVerification.Rooms;
using SuperMetroid.Rendering.Direct3D11.Device;
using SuperMetroid.Rendering.Direct3D11.Frames;
using SuperMetroid.Rendering.Direct3D11.Presentation;
namespace SuperMetroid.RenderVerification.Device;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.FrameCapture;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;
using SuperMetroid.TestSupport.FrameRendering;

/// <summary>Hidden-window checks for D3D11 swapchain presentation, worker recovery and bounded frame publication.</summary>
internal static partial class SwapchainTests
{
    /// <summary>Reads the worker mailbox's private publication counters under its own lock.</summary>
    private static RenderMailboxCounterView MailboxCounters(D3D11RenderWorker worker) =>
        worker.mailbox.VerificationCounters;

    /// <summary>
    /// Checks worker startup, latest-frame mailbox accounting, retained redraw, resize and a generation change, and the
    /// bounded device recreation the worker runs on a device loss.
    /// </summary>
    internal static void RunWorker(D3D11RenderDevice selection)
    {
        nint window = CreateWindowExW(0, "STATIC", "Hidden GPU worker verification", 0, 0, 0, 640, 480, 0, 0, 0, 0);
        if (window == 0) throw new Win32Exception(Marshal.GetLastWin32Error());
        D3D11RenderWorker? worker = null;
        try
        {
            worker = new D3D11RenderWorker(window, new Extent16<DisplayPixels>(640, 480), 1, selection.Kind);
            PumpUntil(() => worker.Ready.IsCompleted);
            string adapter = worker.Ready.GetAwaiter().GetResult();
            if (!adapter.Contains($"backend={selection.Kind}") ||
                !adapter.Contains($"feature level={selection.ActualFeatureLevel}"))
                throw new InvalidOperationException("Worker startup omitted actual backend/feature-level diagnostics.");
            // Allow the worker to observe initial readiness while the mailbox is empty.
            var idle = System.Diagnostics.Stopwatch.StartNew();
            PumpUntil(() => idle.ElapsedMilliseconds >= 40);
            for (int sequence = 1; sequence <= 1000; sequence++)
                worker.Publish(new(new(sequence, 1), new DisplayPacket(0, new RgbaPixel(19,73,129))));
            PumpUntil(() => { worker.ThrowIfFaulted(); return worker.LastConsumedSequence == 1000; });
            // A redraw or a recovery that consumed or published a frame would change the replacements or leave one pending.
            long publishedReplacements = worker.MailboxMetrics.Replaced;
            D3D11RenderWorker running = worker;
            Func<bool> mailboxUntouched = () => !MailboxCounters(running).HasPendingFrame && running.MailboxMetrics.Replaced == publishedReplacements;
            long previousRedraws = worker.RetainedRedraws;
            worker.Resize(new Extent16<DisplayPixels>(800, 600));
            PumpUntil(() => { worker.ThrowIfFaulted(); return worker.RetainedRedraws > previousRedraws && worker.LastDrawnSize == new Extent16<DisplayPixels>(800, 600); });
            if (!mailboxUntouched() || worker.LastConsumedSequence != 1000)
                throw new InvalidOperationException("Retained redraw consumed or manufactured a simulation frame.");
            worker.AdvanceGeneration(2);
            worker.Resize(new Extent16<DisplayPixels>(319, 601));
            worker.Publish(new(new(1001, 2), new DisplayPacket(0, new RgbaPixel(91,27,173))));
            PumpUntil(() => { worker.ThrowIfFaulted(); return worker.LastConsumedSequence == 1001 &&
                worker.LastDrawnSize == new Extent16<DisplayPixels>(319, 601); });
            RenderMailboxCounterView metrics = MailboxCounters(worker);
            var timings = worker.CaptureTimings();
            if (timings.CpuComposition.Observed == 0 || timings.CpuDisplayAndPresent.Observed == 0)
                throw new InvalidOperationException("Worker did not record CPU submission/presentation timings.");
            if (metrics.HasPendingFrame || metrics.Generation != 2 || worker.LastConsumedSequence != 1001)
                throw new InvalidOperationException("GPU worker lost mailbox accounting.");
            // A device that was never lost is never recreated.
            if (worker.DeviceRecoveries != 0)
                throw new InvalidOperationException("GPU worker reported a device recovery no device loss caused.");
            Console.WriteLine($"{selection.Kind}: worker {adapter}; idle-start, 1001 publications, retained redraw, generation/resize, bounded accounting passed.");
        }
        finally
        {
            if (worker is not null)
            {
                Task stop = worker.StopAsync(); PumpUntil(() => stop.IsCompleted); stop.GetAwaiter().GetResult();
            }
            if (!DestroyWindow(window)) throw new Win32Exception(Marshal.GetLastWin32Error());
        }
        VerifyDeviceRecovery(selection);
        VerifySurfaceSuspension(selection.Kind);
    }

    /// <summary>
    /// Checks the device-loss decisions the worker makes (<see cref="D3D11DeviceRecovery"/>), driven with the failures real
    /// DXGI calls raise: each device loss after startup recreates the device until the consecutive recreations reach their
    /// bound, a successful submission ends the run, each recreated generation counts one recovery, and the next loss is the
    /// worker's terminal fault, rethrown with its own result; a loss at startup or any other failure is never retried and
    /// faults with its own type; a loss is reported with the failed device's context.
    /// </summary>
    private static void VerifyDeviceRecovery(D3D11RenderDevice selection)
    {
        Exception removed = DxgiFailure((int)DxgiDeviceLoss.DeviceRemoved);
        Exception reset = DxgiFailure((int)DxgiDeviceLoss.DeviceReset);

        var startup = new D3D11DeviceRecovery();
        if (startup.Recreates(removed, started: false))
            throw new InvalidOperationException("A device lost before startup was recreated.");
        startup.Fault(removed);
        RequireFault(startup, failure => failure.HResult == (int)DxgiDeviceLoss.DeviceRemoved, "a device lost before startup");

        var recovery = new D3D11DeviceRecovery();
        recovery.ThrowIfFaulted();
        for (int run = 0; run < 2; run++)
        {
            for (int attempt = 1; attempt <= D3D11RecoveryPolicy.MaximumConsecutiveRecreations; attempt++)
            {
                Exception loss = attempt % 2 == 0 ? reset : removed;
                if (!recovery.Recreates(loss, started: true))
                    throw new InvalidOperationException($"Device loss {attempt} of run {run} ({loss.HResult:X8}) was not recreated.");
                recovery.Recovered();
            }
            // A frame submitted on the last recreated device ends the first run, so the second may recreate as often again.
            if (run == 0)
                recovery.Submitted();
        }
        if (recovery.Recoveries != 2 * D3D11RecoveryPolicy.MaximumConsecutiveRecreations)
            throw new InvalidOperationException($"{recovery.Recoveries} recoveries were counted for {2 * D3D11RecoveryPolicy.MaximumConsecutiveRecreations} recreated devices.");
        if (recovery.Recreates(removed, started: true))
            throw new InvalidOperationException("A device loss past the recreation bound was retried instead of surfacing.");
        recovery.Fault(removed);
        RequireFault(recovery, failure => failure.HResult == (int)DxgiDeviceLoss.DeviceRemoved, "a device loss past the bound");

        var other = new D3D11DeviceRecovery();
        var unrelated = new InvalidDataException("A failure that is no device loss.");
        if (other.Recreates(unrelated, started: true))
            throw new InvalidOperationException("A failure that is no device loss was retried.");
        other.Fault(unrelated);
        other.Fault(removed);
        RequireFault(other, failure => ReferenceEquals(failure, unrelated), "a failure that is no device loss, kept over a later one");

        var frame = new RenderFrameIdentity(1000, 2);
        D3D11DeviceLossDiagnostic context = D3D11DeviceRecovery.Loss(reset, 0, selection.AdapterDescription, selection.Kind,
            new Extent16<DisplayPixels>(801, 601), frame);
        if (context.FailureHResult != (int)DxgiDeviceLoss.DeviceReset || context.RemovalReason != 0 ||
            context.Adapter != selection.AdapterDescription || context.Backend != selection.Kind ||
            context.Target != new Extent16<DisplayPixels>(801, 601) || context.Frame != frame)
            throw new InvalidOperationException($"A device loss lost its failure, device or frame context: {context}.");
        bool refused = false;
        try
        {
            _ = D3D11DeviceRecovery.Loss(unrelated, 0, selection.AdapterDescription, selection.Kind, new Extent16<DisplayPixels>(801, 601), frame);
        }
        catch (ArgumentException)
        {
            refused = true;
        }
        if (!refused)
            throw new InvalidOperationException("A failure that is no device loss was reported as one.");
        Console.WriteLine($"Device recovery: {D3D11RecoveryPolicy.MaximumConsecutiveRecreations} consecutive device losses recreate the device and count, a submission ends the run, the next loss faults with its own result, other failures are never retried; a loss reports its device's context.");
    }

    /// <summary>Requires <paramref name="recovery"/>'s terminal fault to be rethrown as the failure <paramref name="original"/> accepts.</summary>
    private static void RequireFault(D3D11DeviceRecovery recovery, Func<Exception, bool> original, string label)
    {
        try
        {
            recovery.ThrowIfFaulted();
        }
        catch (Exception failure) when (original(failure))
        {
            return;
        }
        throw new InvalidOperationException($"The worker's fault after {label} was not its original failure.");
    }

    /// <summary>The exception a DXGI call failing with <paramref name="result"/> raises.</summary>
    private static SharpGen.Runtime.SharpGenException DxgiFailure(int result)
    {
        try
        {
            new SharpGen.Runtime.Result(result).CheckError();
        }
        catch (SharpGen.Runtime.SharpGenException failure)
        {
            return failure;
        }
        throw new InvalidOperationException($"DXGI result {result:X8} raised no failure.");
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
            var packet = new RenderFrame(new(1, 1), new DisplayPacket(0, new RgbaPixel(173,91,27)));
            renderer.Render(packet.Display);
            _ = presenter.TryAcquireFrameOpportunity();
            var outcome = presenter.Present(renderer, packet, gate, null, waitForVerticalBlank: true);
            if (outcome == D3D11PresentationResult.StaleGeneration) throw new InvalidOperationException("Current frame was rejected.");
            gate.AdvanceGeneration(2);
            if (presenter.Present(renderer, packet, gate, null, waitForVerticalBlank: true) != D3D11PresentationResult.StaleGeneration)
                throw new InvalidOperationException("Swapchain presented stale generation.");
            bool mismatch = false;
            try { presenter.Present(renderer, new RenderFrame(new(2, 2), new DisplayPacket(0, new RgbaPixel(173,91,27))), gate, null, waitForVerticalBlank: true); }
            catch (InvalidOperationException) { mismatch = true; }
            if (!mismatch) throw new InvalidOperationException("Swapchain accepted mismatched GPU frame identity.");
            foreach (var size in new Extent16<DisplayPixels>[] { new(319, 601), new(1, 1), new(800, 600) })
            {
                presenter.Resize(size);
                packet = new RenderFrame(new(packet.Identity.Sequence + 1, 2), new DisplayPacket(0, new RgbaPixel(27,91,173)));
                renderer.Render(packet.Display);
                outcome = presenter.Present(renderer, packet, gate, null, waitForVerticalBlank: true);
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

    /// <summary>
    /// Checks that a GPU consumer that stops taking frames does not let a queued gameplay door transition corrupt frame
    /// identity or progress: the host suspends the worker's surface, as a minimized window does, after its first frame.
    /// </summary>
    internal static void VerifyBlockedDoor(D3D11RenderDevice device, D3D11FrameRenderer renderer,
        SuperMetroidRuntime runtime, ushort door, RoomDefinition destination, string context)
    {
        nint window = CreateWindowExW(0, "STATIC", "Hidden door backlog verification", 0, 0, 0, 640, 480, 0, 0, 0, 0);
        if (window == 0) throw new Win32Exception(Marshal.GetLastWin32Error());
        D3D11RenderWorker? worker = null;
        try
        {
            worker = new D3D11RenderWorker(window, new Extent16<DisplayPixels>(640, 480), 1, device.Kind);
            PumpUntil(() => worker.Ready.IsCompleted);
            worker.Ready.GetAwaiter().GetResult();
            long sequence = 1;
            worker.Publish(new(new(sequence, 1), new DisplayPacket(0, GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame()))));
            PumpUntil(() => { worker.ThrowIfFaulted(); return worker.LastConsumedSequence == 1; });
            worker.Resize(new Extent16<DisplayPixels>(0, 0));
            PumpUntil(() => { worker.ThrowIfFaulted(); return worker.IsSurfaceSuspended; });
            var clock = Stopwatch.StartNew();
            RenderFrame? latest = null;
            RetailDoorCaptureTests.VerifyTransition(device, renderer, runtime, new RetailDoorCase { Door = door, Destination = destination }, context, packet =>
                {
                    latest = new(new(++sequence, 1), new DisplayPacket(packet.FrameNumber, packet.Scene));
                    worker.Publish(latest);
                });
            PumpUntil(() => clock.ElapsedMilliseconds >= 250);
            LatestRenderFrameMailbox mailbox = worker.mailbox;
            if (latest is null || worker.LastConsumedSequence != 1 || !mailbox.VerificationCounters.HasPendingFrame ||
                worker.MailboxMetrics.Replaced != sequence - 2)
                throw new InvalidOperationException("Door transition failed to advance under the held consumer with a bounded mailbox.");
            worker.Resize(new Extent16<DisplayPixels>(640, 480));
            PumpUntil(() => { worker.ThrowIfFaulted(); return worker.LastConsumedSequence == sequence; });
            RenderMailboxCounterView resumed = mailbox.VerificationCounters;
            // Every packet but the first, which the consumer took before it stopped, and the last, which it took on resuming, was replaced.
            if (resumed.HasPendingFrame || resumed.Replaced != sequence - 2)
                throw new InvalidOperationException("Resumed door consumer did not skip superseded transition packets.");
            PixelComparison.Verify(latest.Display, FrontendScenePixelsAccess.Pixels(runtime.PublishDisplayFrame()),
                renderer.RenderForReadback(latest.Display), $"{device.Kind}: retained destination after blocked {context} door");
            Console.WriteLine($"{device.Kind}: blocked {context} door completed; {sequence - 2} replaced frames, latest destination consumed.");
        }
        finally
        {
            if (worker is not null)
            {
                Task stop = worker.StopAsync(); PumpUntil(() => stop.IsCompleted); stop.GetAwaiter().GetResult();
            }
            if (!DestroyWindow(window)) throw new Win32Exception(Marshal.GetLastWin32Error());
        }
    }

    /// <summary>
    /// Compares real desktop audio under normal and blocked GPU consumption in two recorded room slices. Each room loads
    /// under a camera that shows a floor wide enough to walk on, and Samus stands on it: the startup leaves her at room
    /// position (128, 0), which in both rooms lies inside solid blocks, where the game rightly lets her move nowhere.
    /// Alpha Power Bomb's floor is block row 13 under its first screen; the Glass Tunnel's is row 26, between its doors,
    /// under its second screen.
    /// </summary>
    internal static void RunSlowConsumerAudio(D3D11RenderDevice selection)
    {
        foreach ((RoomDefinition room, Position16<RoomPixels> camera, int floorBlockRow) in new[]
                 {
                     (RoomDefinitions.AlphaPowerBomb, new Position16<RoomPixels>(0, 0), 13),
                     (RoomDefinitions.GlassTunnel, new Position16<RoomPixels>(0, 256), 26),
                 })
            foreach (bool blockConsumer in new[] { false, true })
                RunSlowConsumerAudioRoom(selection, room, camera, floorBlockRow, blockConsumer);
    }

    /// <summary>Runs one room-local controller and pause slice through legacy, captured and headless audio paths.</summary>
    private static void RunSlowConsumerAudioRoom(D3D11RenderDevice selection, RoomDefinition room, Position16<RoomPixels> camera,
        int floorBlockRow, bool blockConsumer)
    {
        byte[] rom = File.ReadAllBytes("Super Metroid.smc");
        var options = new SuperMetroid.Core.Gameplay.GameplayAssists { SkipOpeningCinematic = true };
        var audioAssets = DesktopAccess.LoadAudioClips();
        var legacy = SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.CreateGameWithAudio(new SuperMetroid.Core.Saves.SaveSlotStore(SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.Maps), audioAssets, options);
        var captured = SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.CreateGameWithAudio(new SuperMetroid.Core.Saves.SaveSlotStore(SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.Maps), audioAssets, options);
        var headless = SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.CreateGameWithAudio(new SuperMetroid.Core.Saves.SaveSlotStore(SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.Maps), audioAssets, options);
        // Each session synthesizes through its own Core audio session, as the desktop host does;
        // do not substitute a second hand-written queue-to-PCM implementation.
        var bound = new BoundObjectGraph([.. SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.BoundContent, audioAssets]);
        var left = new short[AudioSession.PcmSamplesPerFrame];
        var right = new short[AudioSession.PcmSamplesPerFrame];
        var silentDisplay = new short[AudioSession.PcmSamplesPerFrame];
        long sequence = 0, pcmSamples = 0, nonzero = 0;
        CapturedFrontendFrame Step(ControllerButton input)
        {
            var expected = legacy.Step(input);
            sequence++;
            var actual = captured.StepCaptured(input);
            // No raster request, GPU publication, or reference render for this owner.
            var undrawn = headless.StepCaptured(input);
            if (undrawn.Frame.GameState != actual.Frame.GameState ||
                undrawn.Frame.Phase != actual.Frame.Phase || undrawn.Frame.FrameNumber != actual.Frame.FrameNumber ||
                !undrawn.Frame.Audio.Events.SequenceEqual(actual.Frame.Audio.Events))
                throw new InvalidOperationException($"Headless frame/command divergence at {sequence}.");
            if (expected.GameState != actual.Frame.GameState || expected.Phase != actual.Frame.Phase ||
                expected.FrameNumber != actual.Frame.FrameNumber ||
                !expected.Audio.Events.SequenceEqual(actual.Frame.Audio.Events))
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
            Step(tick % 47 == 0 ? ControllerButton.Start : ControllerButton.None);
        if (legacy.GameState != SuperMetroidGameState.MainGameplay) throw new InvalidOperationException("Audio fixture failed startup.");
        // Room-local pause slice, matching the portable capture fixture; no traversal.
        foreach (var game in new[] { legacy, captured, headless })
        {
            SuperMetroidRuntime runtime = game.GameplayRuntimeForVerification;
            runtime.LoadCartridgeRoomForDebug(room, camera);
            // Samus stands on the floor at the middle of the screen: her bottom row is the one above the floor's top.
            SuperMetroid.Core.Samus.SamusState samus = runtime.Player.Samus;
            samus.Position = FixedPosition<RoomPixels>.FromWhole(new(camera.X + 128,
                floorBlockRow * 16 - samus.Kinematics.Radius.Y));
            runtime.FrameBoundary.BeginAcceptedFrame(0);
        }
        nint window = CreateWindowExW(0, "STATIC", "Hidden slow-consumer test", 0, 0, 0, 640, 480, 0, 0, 0, 0);
        if (window == 0) throw new Win32Exception(Marshal.GetLastWin32Error());
        D3D11RenderWorker? worker = null;
        try
        {
            worker = new D3D11RenderWorker(window, new Extent16<DisplayPixels>(640, 480), 1, selection.Kind);
            PumpUntil(() => worker.Ready.IsCompleted); worker.Ready.GetAwaiter().GetResult();
            var first = Step(0);
            // The startup's updates took sequence numbers too: the first frame the worker draws is this one.
            long firstSequence = sequence;
            worker.Publish(new RenderFrame(new(sequence, 1), first.Snapshot));
            PumpUntil(() => { worker.ThrowIfFaulted(); return worker.LastConsumedSequence == sequence; });
            // A blocked consumer is one whose surface the host suspended, as a minimized window does: it takes no frame
            // until the surface returns.
            if (blockConsumer)
            {
                worker.Resize(new Extent16<DisplayPixels>(0, 0));
                PumpUntil(() => { worker.ThrowIfFaulted(); return worker.IsSurfaceSuspended; });
            }
            CompareRuntimeGraphs("before room slice");
            var blockedTime = System.Diagnostics.Stopwatch.StartNew();
            bool paused = false;
            for (int tick = 0; tick < 160; tick++)
            {
                var actual = Step(tick is 10 or >= 100 and <= 105 ? ControllerButton.Start : ControllerButton.None);
                worker.Publish(new RenderFrame(new(sequence, 1), actual.Snapshot));
                paused |= legacy.IsAt(PauseStep.FadingIn) || legacy.IsAt(PauseStep.Menu);
                if (tick % 40 == 39) CompareRuntimeGraphs($"room slice tick {tick}");
            }
            var positions = new HashSet<OptionalScreenPosition>();
            var poses = new HashSet<SamusPoseId?>();
            var roomBeforeMovement = legacy.GameplayActiveRoom;
            // Bounded room-local inputs: held movement, single-tick direction taps,
            // jump/shoot edges and explicit releases. No room traversal is intended.
            for (int tick = 0; tick < 64; tick++)
            {
                ControllerButton input = tick switch
                {
                    < 8 => ControllerButton.Right,
                    10 or 12 => ControllerButton.Left,
                    >= 16 and < 24 => (ControllerButton.Left | ControllerButton.B),
                    28 or 30 => ControllerButton.X,
                    >= 40 and < 48 => ControllerButton.Right,
                    _ => 0,
                };
                var actual = Step(input);
                worker.Publish(new RenderFrame(new(sequence, 1), actual.Snapshot));
                positions.Add(new OptionalScreenPosition(legacy.GameplaySamusX, legacy.GameplaySamusY));
                poses.Add(legacy.GameplaySamusPose);
                if (legacy.GameplayActiveRoom != roomBeforeMovement)
                    throw new InvalidOperationException("Movement scheduling fixture crossed a room boundary.");
                if (tick % 16 == 15) CompareRuntimeGraphs($"movement input tick {tick}");
            }
            if (positions.Count < 2 || poses.Count < 2)
                throw new InvalidOperationException("Movement input fixture did not change position and pose.");
            if (blockConsumer) PumpUntil(() => blockedTime.ElapsedMilliseconds >= 250);
            if ((blockConsumer && (!worker.IsSurfaceSuspended || worker.LastConsumedSequence != firstSequence)) || !paused || nonzero == 0 ||
                legacy.GameState != SuperMetroidGameState.MainGameplay)
                throw new InvalidOperationException("Slow-consumer coverage incomplete.");
            if (blockConsumer) worker.Resize(new Extent16<DisplayPixels>(640, 480));
            PumpUntil(() => { worker.ThrowIfFaulted(); return worker.LastConsumedSequence == sequence; });
            if (blockConsumer && worker.MailboxMetrics.Replaced < 223) throw new InvalidOperationException("Blocked visuals were not superseded as expected.");
            CompareRuntimeGraphs("after GPU resumes");
            Console.WriteLine($"{selection.Kind}: room {room.Name}, WARP/GPU/headless agree; deliberate GPU block={blockConsumer}; 160 room/pause + 64 movement/input frames; {positions.Count} positions, {poses.Count} poses; {pcmSamples} exact PCM samples ({nonzero} nonzero); latest frame consumed.");
        }
        finally
        {
            if (worker is not null) { var stop = worker.StopAsync(); PumpUntil(() => stop.IsCompleted); stop.GetAwaiter().GetResult(); }
            if (!DestroyWindow(window)) throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        void CompareRuntimeGraphs(string context)
        {
            // Reuse the exact-build debugger graph writer rather than a hand-picked
            // list of gameplay properties. Private fields, arrays, aliases, cycles
            // and delegates are included. No state is deserialized or normalized. The sessions share the installed content
            // and audio clips, which each graph names by path, as a host's store names the content it binds. The two
            // sessions stepped as a host steps them, capturing each update's frame, drawn on the GPU and undrawn, hold the
            // same graph. The uncaptured session publishes no frame for a host, so its display holds the frame its scene
            // composed where a capturing session's holds the frame it published; its game state is compared update by
            // update (position, pose, phase, audio commands and PCM) instead.
            bound.Refresh();
            using var drawn = new MemoryStream();
            using var undrawn = new MemoryStream();
            DebuggerObjectGraphSerializer.Serialize(drawn, captured.GameplayRuntimeForVerification, bound);
            DebuggerObjectGraphSerializer.Serialize(undrawn, headless.GameplayRuntimeForVerification, bound);
            if (!drawn.GetBuffer().AsSpan(0, checked((int)drawn.Length))
                .SequenceEqual(undrawn.GetBuffer().AsSpan(0, checked((int)undrawn.Length))))
                throw new InvalidOperationException($"Headless runtime graph diverged: {context} (lengths {drawn.Length}/{undrawn.Length}).");
            Console.WriteLine($"  Exact runtime graph agrees: {context}, {drawn.Length} bytes.");
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
            worker.Publish(new(new(sequence, generation), new DisplayPacket(0, new RgbaPixel(23, 71, 127))));
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
                    worker.Publish(new(new(++sequence, generation), new DisplayPacket(0, new RgbaPixel(127, 71, 23))));

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
                worker.Publish(new(new(++sequence, generation), new DisplayPacket(0, new RgbaPixel(43, 91, 173))));
                worker.Resize(new Extent16<DisplayPixels>(641, 481));
                PumpUntil(() => { worker.ThrowIfFaulted(); return !worker.IsSurfaceSuspended &&
                    worker.LastConsumedSequence == sequence && worker.LastDrawnSize == new Extent16<DisplayPixels>(641, 481); });
                RenderMailboxCounterView metrics = MailboxCounters(worker);
                if (metrics.HasPendingFrame || metrics.Generation != generation)
                    throw new InvalidOperationException("Surface restore lost latest generation/mailbox accounting.");
            }

            worker.Resize(new Extent16<DisplayPixels>(0, 0));
            PumpUntil(() => { worker.ThrowIfFaulted(); return worker.IsSurfaceSuspended; });
            worker.Publish(new(new(++sequence, generation), new DisplayPacket(0, new RgbaPixel(1, 2, 3))));
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
