using SuperMetroid.Desktop;

/// <summary>Developer-only smoke commands formerly hosted by the player executable.</summary>
internal static class DesktopSmokeAuditCommands
{
    public static bool TryRun(string[] args)
    {
        if (args is ["--production-assembly-audit", .. var assemblies] && assemblies.Length != 0)
        {
            ProductionAssemblyVerification.Run(assemblies);
            return true;
        }
        if (args.Length != 0 &&
            args[0].Equals("--unhandled-exception-console-audit", StringComparison.OrdinalIgnoreCase))
        {
            if (args.Length != 1)
            {
                throw new ArgumentException(
                    "--unhandled-exception-console-audit does not accept additional arguments.");
            }
            UnhandledExceptionConsoleSmokeTestResult result = UnhandledExceptionConsoleSmokeTest.Run();
            Console.WriteLine(
                $"Unhandled-exception console passed: complete {result.ReportLength}-character " +
                "diagnostic was flushed before the acknowledgment wait.");
            return true;
        }

        if (args.Length != 0 &&
            args[0].Equals("--keyboard-input-audit", StringComparison.OrdinalIgnoreCase))
        {
            if (args.Length != 1)
                throw new ArgumentException("--keyboard-input-audit does not accept additional arguments.");
            HostKeyboardInputSmokeTestResult result = HostKeyboardInputSmokeTest.Run();
            Console.WriteLine(
                $"Keyboard input passed: Enter=${result.EnterControllerWord:X4}, " +
                $"released=${result.ReleasedControllerWord:X4}; no Right bit was emitted.");
            return true;
        }

        if (args.Length != 0 &&
            args[0].Equals("--github-error-reporter-audit", StringComparison.OrdinalIgnoreCase))
        {
            if (args.Length != 1)
                throw new ArgumentException("--github-error-reporter-audit does not accept arguments.");
            GitHubErrorReporterSmokeTestResult result = GitHubErrorReporterSmokeTest.Run();
            Console.WriteLine(
                $"GitHub error reporter passed: {result.Fingerprint}, " +
                $"{result.RemoteLookups} distinct lookups, {result.IssuesCreated} issue created.");
            return true;
        }

        if (args.Length != 0 &&
            args[0].Equals("--viewport-layout-audit", StringComparison.OrdinalIgnoreCase))
        {
            if (args.Length != 1)
                throw new ArgumentException("--viewport-layout-audit does not accept additional arguments.");
            HostViewportLayoutSmokeTestResult result = HostViewportLayoutSmokeTest.Run();
            Console.WriteLine(
                $"Viewport layout passed: canvas remained {result.BeforeCanvasBounds}.");
            return true;
        }

        if (args.Length == 1 && args[0] == "--input-batch-audit")
        {
            PlaybackFrameBatchSmokeTest.Run();
            Console.WriteLine("Input batch passed: short press/release, exhausted replay, and empty batch.");
            return true;
        }

        if (args.Length != 0 &&
            args[0].Equals("--frame-timing-audit", StringComparison.OrdinalIgnoreCase))
        {
            if (args.Length != 1)
                throw new ArgumentException("--frame-timing-audit does not accept additional arguments.");
            FrameTimingCounterSmokeTestResult result = FrameTimingCounterSmokeTest.Run();
            RgbaBitmapSmokeTestResult bitmap = RgbaBitmapSmokeTest.Run();
            Console.WriteLine(
                $"Frame timing passed: emulation {result.EmulatedFramesPerSecond:F1} fps, " +
                $"paint {result.PaintedFramesPerSecond:F1} fps, " +
                $"step {result.AverageEmulationMilliseconds:F2}/{result.WorstEmulationMilliseconds:F2} ms, " +
                $"late {result.LateFrames:F1}; persistent {bitmap.Width}x{bitmap.Height} " +
                $"bitmap replacement preserved RGBA.");
            return true;
        }

        if (args.Length != 0 && args[0].Equals("--state-audit", StringComparison.OrdinalIgnoreCase))
        {
            string[] stateRomArguments = args.Length == 2 ? [args[1]] : [];
            if (args.Length > 2)
                throw new ArgumentException("--state-audit accepts one optional private ROM path.");
            string stateRomPath = PrivateRomPath.Resolve(stateRomArguments);
            DebuggerSaveStateSmokeTestResult result = DebuggerSaveStateSmokeTest.Run(stateRomPath);
            Console.WriteLine(
                $"Debugger state passed: frame {result.SavedFrame}, " +
                $"{result.ContinuationFrames} deterministic continuation frames, " +
                $"{result.StateFileBytes} bytes, wrong-ROM rejection={result.WrongRomRejected}, " +
                $"empty-slot report={result.EmptySlotReported}.");
            return true;
        }

        if (args.Length != 0 &&
            args[0].Equals("--audio-input-replay-audit", StringComparison.OrdinalIgnoreCase))
        {
            if (args.Length is < 3 or > 4)
            {
                throw new ArgumentException(
                    "--audio-input-replay-audit requires a .smrec path and private ROM path, " +
                    "then accepts one optional capture directory.");
            }
            AudioInputReplaySmokeTestResult result = AudioInputReplaySmokeTest.Run(
                args[1],
                args[2],
                args.Length == 4 ? args[3] : null);
            Console.WriteLine(
                $"Audio replay passed in {result.FramesExecuted} frames: projectile/SFX acknowledged; " +
                $"first door ${result.SourceRoom:X4} -> ${result.DestinationRoom:X4} completed.");
            return true;
        }

        if (args.Length != 0 &&
            args[0].Equals("--full-audio-input-replay-audit", StringComparison.OrdinalIgnoreCase))
        {
            if (args.Length != 3)
            {
                throw new ArgumentException(
                    "--full-audio-input-replay-audit requires a .smrec path and private ROM path.");
            }
            int frames = AudioInputReplaySmokeTest.RunCompleteManagedAudio(args[1], args[2]);
            Console.WriteLine($"Full managed-audio replay passed {frames} recorded calls.");
            return true;
        }

        if (args.Length != 0 &&
            args[0].Equals("--recorded-pause-audio-audit", StringComparison.OrdinalIgnoreCase))
        {
            if (args.Length is < 3 or > 4)
            {
                throw new ArgumentException(
                    "--recorded-pause-audio-audit requires a .smrec path and private ROM path, " +
                    "then accepts an optional maximum frame count.");
            }
            int maximumFrames = args.Length == 4
                ? int.Parse(args[3], System.Globalization.CultureInfo.InvariantCulture)
                : int.MaxValue;
            IReadOnlyList<RecordedPauseAudioResult> pauses =
                AudioInputReplaySmokeTest.AnalyzeRecordedPauses(args[1], args[2], maximumFrames);
            foreach (RecordedPauseAudioResult pause in pauses)
            {
                Console.WriteLine(
                    $"Recorded pause {pause.EntryFrame}-{pause.ResumeFrame} in " +
                    $"$8F:{pause.RoomPointer:X4}: {pause.PauseOwnedFrames} pause frames, " +
                    $"RMS {pause.MinimumPausedRms:F1}-{pause.MaximumPausedRms:F1} " +
                    $"(mean {pause.MeanPausedRms:F1}), {pause.AudioCommands} APU commands; " +
                    $"{pause.CommandTrace}.");
            }
            return true;
        }

        if (args.Length != 0 && args[0].Equals("--waveout-audit", StringComparison.OrdinalIgnoreCase))
        {
            if (args.Length != 1)
                throw new ArgumentException("--waveout-audit does not accept additional arguments.");
            WaveOutAudioSmokeTestResult result = WaveOutAudioSmokeTest.Run();
            Console.WriteLine(
                $"waveOut backpressure passed: {result.BuffersSubmitted} one-frame buffers " +
                $"submitted without loss in {result.SubmissionTime.TotalMilliseconds:F0} ms.");
            return true;
        }

        if (args.Length != 0 && args[0].Equals("--audio-audit", StringComparison.OrdinalIgnoreCase))
        {
            string[] audioRomArguments = args.Length == 2 ? [args[1]] : [];
            if (args.Length > 2)
                throw new ArgumentException("--audio-audit accepts one optional private ROM path.");
            string audioRomPath = PrivateRomPath.Resolve(audioRomArguments);
            CartridgeAudioSmokeTestResult result = CartridgeAudioSmokeTest.Run(audioRomPath);
            Console.WriteLine(
                $"Cartridge audio passed: {result.FramesGenerated} frames, " +
                $"{result.NonZeroSamples} music samples, peak {result.PeakAmplitude}; " +
                $"power-beam SFX produced {result.PowerBeamNonZeroSamples} samples " +
                "and completed its request/clear handshake.");
            return true;
        }

        if (args.Length != 0 && args[0].Equals("--managed-audio-audit", StringComparison.OrdinalIgnoreCase))
        {
            if (args.Length != 1)
                throw new ArgumentException("--managed-audio-audit does not accept a ROM path.");
            ManagedAudioRegressionSmokeTestResult result = ManagedAudioRegressionSmokeTest.Run();
            Console.WriteLine(
                $"Managed audio regression passed: {result.Scenarios} scenarios, {result.Frames} frames, " +
                $"{result.PcmSamples} samples, {result.CanonicalSamples} canonical WAVs/" +
                $"{result.SourceAliases} source aliases; PCM {result.PcmSha256}, " +
                $"acknowledgements {result.AcknowledgementSha256}.");
            return true;
        }

        if (args.Length != 0 && args[0].Equals("--pause-audio-audit", StringComparison.OrdinalIgnoreCase))
        {
            string[] pauseAudioRomArguments = args.Length == 2 ? [args[1]] : [];
            if (args.Length > 2)
                throw new ArgumentException("--pause-audio-audit accepts one optional private ROM path.");
            string pauseAudioRomPath = PrivateRomPath.Resolve(pauseAudioRomArguments);
            PauseAudioSmokeTestResult result = PauseAudioSmokeTest.Run(pauseAudioRomPath);
            Console.WriteLine(
                $"Pause audio passed: {result.FramesGenerated} PCM frames, " +
                $"{result.PauseFrames} pause-owned frames, {result.AudioCommandCount} APU commands; " +
                $"max adjacent delta {result.MaximumAdjacentSampleDelta}, " +
                $"max frame-boundary delta {result.MaximumFrameBoundaryDelta}.");
            return true;
        }

        return false;
    }
}
