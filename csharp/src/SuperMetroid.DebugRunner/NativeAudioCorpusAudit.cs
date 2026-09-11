using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Input;

/// <summary>
/// Rechecks the desktop's shared-cue corpus against native BRR playback before accepting
/// a changed PCM golden hash. The native DLL is diagnostic-only and supplied explicitly.
/// </summary>
internal static class NativeAudioCorpusAudit
{
    public static int Run(string audioDirectory, string dllPath, string? romPath = null, string? recordingPath = null, bool survey = false,
        string? captureDirectory = null, bool fileSelectOnly = false, string? ridleyTracePath = null, bool missileImpactOnly = false,
        bool healthWarningOnly = false, SoundEffectId? soundOnly = null)
    {
        var assets = ExtractedAudioAssetCatalog.Load(audioDirectory);
        nint library = NativeLibrary.Load(Path.GetFullPath(dllPath));
        T Export<T>(string name) where T : Delegate => Marshal.GetDelegateForFunctionPointer<T>(NativeLibrary.GetExport(library, name));
        var create = Export<Create>("sm_audio_create");
        var destroy = Export<Destroy>("sm_audio_destroy");
        var upload = Export<Upload>("sm_audio_upload");
        var write = Export<Write>("sm_audio_write_port");
        var read = Export<Read>("sm_audio_read_port");
        var readDsp = Export<Read>("sm_audio_read_dsp_register");
        var beginCapture = Export<Count>("sm_audio_begin_dsp_write_capture");
        var writeCount = Export<Count>("sm_audio_dsp_write_count");
        var writeAddress = Export<Read>("sm_audio_dsp_write_address");
        var writeValue = Export<Read>("sm_audio_dsp_write_value");
        var generate = Export<Generate>("sm_audio_generate_frame");
        using var pcmHash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        using var portHash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        int totalFrames = 0;
        try
        {
            if (soundOnly is { } sound)
            {
                byte port = checked((byte)(SoundEffectLibraries.ToQueueIndex(sound.Library) + 1));
                int audiblePeak = 0;
                Scenario($"standalone-{sound}", 180, frame => frame switch
                {
                    0 => [U(AudioUploadAddresses.SpcEngine), W(port, sound.Value)],
                    1 => [W(port, 0)],
                    _ => [],
                }, observePcm: (frame, actual, expected) =>
                {
                    // Supplying an observer replaces Scenario's default PCM assertion.
                    // Keep exact equality here as well as the non-silence coverage check.
                    if (!actual.AsSpan().SequenceEqual(expected))
                        throw new InvalidDataException($"{sound} PCM differs from native translation at frame {frame}.");
                    audiblePeak = Math.Max(audiblePeak, actual.Max(sample => Math.Abs((int)sample)));
                });
                if (audiblePeak == 0)
                    throw new InvalidDataException($"Standalone {sound} produced no audible PCM.");
                Console.WriteLine($"{sound}: 180 PCM/acknowledgement frames match native translation; peak={audiblePeak}. This does not verify endpoint playback or gameplay triggering.");
                return 0;
            }
            if (healthWarningOnly)
            {
                int activePeak = 0, stoppedPeak = 0;
                Scenario("critical-health-start-stop", 300, frame => frame switch
                {
                    0 => [U(AudioUploadAddresses.SpcEngine), W(3, SamusHealthWarningRomData.Start.Value)],
                    1 => [W(3, 0)],
                    180 => [W(3, SamusHealthWarningRomData.Stop.Value)],
                    181 => [W(3, 0)],
                    _ => [],
                }, observePcm: (frame, actual, expected) =>
                {
                    if (!actual.AsSpan().SequenceEqual(expected))
                        throw new InvalidDataException($"Low-health warning PCM differs from native translation at frame {frame}.");
                    int peak = actual.Max(sample => Math.Abs((int)sample));
                    if (frame is >= 60 and < 180) activePeak = Math.Max(activePeak, peak);
                    if (frame >= 240) stoppedPeak = Math.Max(stoppedPeak, peak);
                });
                if (activePeak == 0 || stoppedPeak != 0)
                    throw new InvalidDataException($"Warning PCM did not sustain then stop: active peak={activePeak}, stopped peak={stoppedPeak}.");
                Console.WriteLine($"Warning PCM: 300 frames match native translation; active peak={activePeak}, post-stop peak={stoppedPeak}. This does not verify Windows/RDP endpoint playback.");
                return 0;
            }
            if (missileImpactOnly)
            {
                MissileImpactAudioSequence.VerifyEnemyImpactAndCinematicSuppression(romPath ?? throw new ArgumentNullException(nameof(romPath)));
                foreach (ushort selection in new ushort[] { 1, 2 })
                {
                    var sequence = new MissileImpactAudioSequence(romPath ?? throw new ArgumentNullException(nameof(romPath)), selection);
                    Scenario($"missile-impact-{selection}", 240, sequence.Step, sequence.Acknowledge);
                    sequence.Verify();
                }
                Console.WriteLine($"Missile impact audio: {totalFrames} complete PCM and acknowledgement frames matched native playback.");
                return 0;
            }
            if (ridleyTracePath != null)
            {
                foreach (int warmup in new[] { 120, 600, 1800 })
                {
                    var sequence = new RidleyDeathAudioSequence(romPath ?? throw new ArgumentNullException(nameof(romPath)), ridleyTracePath, warmup);
                    Scenario($"ridley-death-warmup={warmup}", sequence.FrameCount + warmup + 240, sequence.Step, sequence.Acknowledge);
                }
                Console.WriteLine($"Ridley mixed audio: {totalFrames} complete PCM/acknowledgement frames matched.");
                return 0;
            }
            if (fileSelectOnly)
            {
                foreach (bool saved in new[] { false, true })
                foreach (SnesButton accept in new[] { SnesButton.A, SnesButton.Start })
                {
                    var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath ?? throw new ArgumentNullException(nameof(romPath)));
                    if (saved) new SuperMetroidSaveRam(bus).SaveSlot(0, new SuperMetroidSaveSnapshot());
                    var game = new SuperMetroidGame(bus);
                    int swooshes = 0;
                    Scenario($"file-select-saved={saved}-accept={accept}", 500, tick =>
                    {
                        SnesButton key = game.GameState == SuperMetroidGameState.FileSelectMenus ? accept : SnesButton.Start;
                        var frame = game.StepCaptured(tick % 47 == 0 ? (ushort)key : (ushort)0, tick + 1, 1).Frame;
                        swooshes += frame.AudioCommands.Count(command => command.Kind == CartridgeAudioCommandKind.WritePort &&
                            command.Port == 1 && command.Value == SoundEffectLibrary1Sounds.FileSelectSwoosh.Value);
                        return frame.AudioCommands.ToArray();
                    }, ports => game.SetAudioAcknowledgements(new(ports[0], ports[1], ports[2], ports[3])));
                    if (swooshes != 1) throw new InvalidDataException($"File-select audit expected one swoosh, received {swooshes}.");
                }
                Console.WriteLine($"File-select native comparison: {totalFrames} complete PCM/acknowledgement frames matched across four routes.");
                return 0;
            }
            if (recordingPath is not null)
            {
                var recording = ControllerInputRecording.Read(recordingPath);
                string rom = romPath ?? throw new ArgumentNullException(nameof(romPath));
                if (!SHA256.HashData(File.ReadAllBytes(rom)).AsSpan().SequenceEqual(recording.RomSha256))
                    throw new InvalidDataException("Recorded audio probe ROM digest mismatch.");
                var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
                recording.InitialSaveRam.CopyTo(bus.SaveRam);
                var game = new SuperMetroidGame(bus, recording.GameOptions, renderGameplayFrames: false);
                int pausedFrames = 0;
                bool paused = false;
                int mismatchedFrames = 0, mismatchedPauseFrames = 0;
                int mismatchStart = -1;
                using var capture = captureDirectory is null ? null : new PausePcmCapture(captureDirectory);
                Scenario("recorded-player-audio", recording.ControllerInputs.Length, frame =>
                {
                    FrontendFrame result;
                    try { result = game.Step(recording.ControllerInputs[frame]); }
                    catch (Exception error)
                    {
                        Console.WriteLine($"Replay failed at frame {frame}; compared {pausedFrames} stable pause frames, {mismatchedPauseFrames} differed.");
                        throw new InvalidOperationException($"Recorded audio replay failed at frame {frame}.", error);
                    }
                    bool nextPaused = result.GameState == SuperMetroidGameState.PausedB;
                    if (survey && paused && !nextPaused)
                        Console.WriteLine($"Pause exited at frame {frame}: cumulative {mismatchedPauseFrames}/{pausedFrames} mismatched stable pause frames.");
                    paused = nextPaused;
                    if (paused) pausedFrames++;
                    return result.AudioCommands.ToArray();
                }, ports => game.SetAudioAcknowledgements(new(ports[0], ports[1], ports[2], ports[3])),
                survey ? (frame, actual, expected) =>
                {
                    capture?.Observe(frame, paused, actual);
                    bool differs = !actual.AsSpan().SequenceEqual(expected);
                    if (differs)
                    {
                        mismatchedFrames++;
                        if (paused) mismatchedPauseFrames++;
                        if (mismatchStart < 0) mismatchStart = frame;
                    }
                    else if (mismatchStart >= 0)
                    {
                        Console.WriteLine($"PCM mismatch interval [{mismatchStart},{frame}); now paused={paused}.");
                        mismatchStart = -1;
                    }
                } : null);
                if (pausedFrames == 0) throw new InvalidDataException("Recording did not reach an active pause menu.");
                if (survey)
                {
                    if (mismatchStart >= 0) Console.WriteLine($"PCM mismatch interval [{mismatchStart},{totalFrames}).");
                    Console.WriteLine($"Survey: {mismatchedFrames}/{totalFrames} mismatched frames; {mismatchedPauseFrames}/{pausedFrames} mismatched stable pause frames. All port acknowledgements matched.");
                    return mismatchedFrames == 0 ? 0 : 1;
                }
                Console.WriteLine($"Recorded audio matched {totalFrames} complete PCM/acknowledgement frames, including {pausedFrames} stable pause frames; PCM={Convert.ToHexString(pcmHash.GetHashAndReset())}.");
                Console.WriteLine("This compares generated PCM, not Windows/RDP endpoint playback; it does not establish that the reported audible defect is fixed.");
                return 0;
            }
            Music("title-long", AudioAssetCatalogData.Music[0].SnesAddress, 600);
            foreach (var bank in AudioAssetCatalogData.Music) Music(bank.Name, bank.SnesAddress, 120);
            Sound("library-1-power-beam", [(1, SoundEffectLibrary1Sounds.PowerBeam.Value)]);
            Sound("library-2-door", [(2, SoundEffectLibrary2Sounds.DoorOpening.Value)]);
            Sound("library-3-cinematic", [(3, 0x23)]);
            Scenario("map-scroll-confirm-overlap", 120, frame => frame switch
            {
                0 => [U(AudioUploadAddresses.SpcEngine), W(1, SoundEffectLibrary1Sounds.MapScroll.Value), W(3, SoundEffectLibrary3Sounds.MapPaletteLoop.Value)],
                3 => [W(1, 0), W(3, 0)],
                4 => [W(1, SoundEffectLibrary1Sounds.MenuConfirm.Value)],
                _ => [],
            });
            Sound("three-library-overlap-and-cancel", [(1, SoundEffectLibrary1Sounds.PowerBeam.Value),
                (2, SoundEffectLibrary2Sounds.DoorOpening.Value), (3, 0x23)], true);
            Scenario("music-lifecycle", 600, frame => frame switch
            {
                0 => [U(AudioUploadAddresses.SpcEngine), U(AudioAssetCatalogData.Music[3].SnesAddress), W(0, 1)],
                120 => [W(0, 0)],
                150 => [U(AudioAssetCatalogData.Music[4].SnesAddress), W(0, 1)],
                270 => [W(0, AudioRomData.Apu.PauseMusic)],
                330 => [W(0, AudioRomData.Apu.ResumeMusic)],
                390 => [W(0, 2)],
                510 => [W(0, 1)],
                _ => [],
            });
            Console.WriteLine($"Native corpus matched {totalFrames} complete frames; PCM={Convert.ToHexString(pcmHash.GetHashAndReset())}; ports={Convert.ToHexString(portHash.GetHashAndReset())}");
            int sharedCueFrames = totalFrames;
            // Unlike the historical shared-cue hash, exercise each bank's actual
            // track-five sequence, with a sustained SFX and pause cancellation.
            foreach (var bank in AudioAssetCatalogData.Music)
                Scenario("bank-music-" + bank.Name, 600, frame => frame switch
                {
                    0 => [U(AudioUploadAddresses.SpcEngine), U(bank.SnesAddress), W(0, 5)],
                    240 => [W(1, SoundEffectLibrary1Sounds.ChargeBeamStart.Value)],
                    300 => [W(1, SoundEffectLibrary1Sounds.CancelAll.Value), W(2, SoundEffectLibrary2Sounds.CancelAll.Value), W(3, SoundEffectLibrary3Sounds.CancelAll.Value)],
                    _ => [],
                });
            Console.WriteLine($"Native bank-music comparison matched {totalFrames - sharedCueFrames} additional complete PCM/acknowledgement frames.");
            return 0;
        }
        finally { NativeLibrary.Free(library); }

        static CartridgeAudioCommand U(int address) => CartridgeAudioCommand.Upload(address);
        static CartridgeAudioCommand W(byte port, byte command) => CartridgeAudioCommand.WritePort(port, command);
        void Music(string name, int address, int frames) => Scenario(name, frames, frame => frame == 0
            ? [U(AudioUploadAddresses.SpcEngine), U(address), W(0, 1)] : []);
        void Sound(string name, (byte Port, byte Command)[] start, bool cancel = false) => Scenario(name, 120,
            frame => frame == 0 ? new[] { U(AudioUploadAddresses.SpcEngine) }.Concat(start.Select(c => W(c.Port, c.Command))).ToArray()
            : cancel && frame == 60 ? [W(1, SoundEffectLibrary1Sounds.CancelAll.Value), W(2, SoundEffectLibrary2Sounds.CancelAll.Value), W(3, SoundEffectLibrary3Sounds.CancelAll.Value)] : []);

        void Scenario(string name, int frames, Func<int, CartridgeAudioCommand[]> commands, Action<byte[]>? acknowledge = null,
            Action<int, short[], short[]>? observePcm = null)
        {
            nint native = create();
            if (native == 0) throw new InvalidOperationException("Native audio allocation failed.");
            try
            {
                var managed = new ManagedSpcPlayer();
                var mutedImpactControl = missileImpactOnly ? new ManagedSpcPlayer() : null;
                var mutedImpactPcm = missileImpactOnly ? new short[1600] : null;
                bool impactChangedPcm = false;
                var nativeRaw = new short[1068];
                var nativeHost = new short[1600];
                var actual = new short[1600];
                byte[] nameBytes = Encoding.UTF8.GetBytes(name);
                var ports = new byte[4];
                var recentCommands = new Queue<string>();
                for (int frame = 0; frame < frames; frame++)
                {
                    if (beginCapture(native) != 1) throw new InvalidOperationException("Native DSP capture allocation failed.");
                    foreach (var command in commands(frame))
                    {
                        recentCommands.Enqueue($"frame={frame} {command}");
                        if (recentCommands.Count > 12) recentCommands.Dequeue();
                        if (command.Kind == CartridgeAudioCommandKind.Upload)
                        {
                            byte[] stream = assets.GetUpload(command.UploadAddress).ToArray();
                            if (upload(native, stream, stream.Length) != 1) throw new InvalidDataException("Native upload rejected.");
                            managed.Upload(stream);
                            managed.SetSampleBank(assets.GetSampleBank(command.UploadAddress));
                            mutedImpactControl?.Upload(stream);
                            mutedImpactControl?.SetSampleBank(assets.GetSampleBank(command.UploadAddress));
                        }
                        else
                        {
                            if (write(native, command.Port, command.Value) != 1) throw new InvalidDataException("Native port write rejected.");
                            managed.WritePort(command.Port, command.Value);
                            if (command.Port != 2 || command.Value != SoundEffectLibrary2Sounds.MissileImpact.Value)
                                mutedImpactControl?.WritePort(command.Port, command.Value);
                        }
                    }
                    // Read native samples at native rate, then use the independently tested
                    // host conversion. The DLL's old nearest-neighbor resampler is bypassed.
                    if (generate(native, nativeRaw, 534) != 534) throw new InvalidDataException("Native PCM frame incomplete.");
                    PcmFrameResampler.ResampleStereoLinear(nativeRaw, nativeHost);
                    managed.GenerateFrame(actual);
                    if (mutedImpactControl is not null)
                    {
                        mutedImpactControl.GenerateFrame(mutedImpactPcm!);
                        impactChangedPcm |= !actual.AsSpan().SequenceEqual(mutedImpactPcm);
                    }
                    if ((observePcm is null || soundOnly is not null) && !actual.AsSpan().SequenceEqual(nativeHost))
                    {
                        foreach (string recent in recentCommands) Console.WriteLine(recent);
                        for (int index = 0; index < writeCount(native); index++)
                            Console.WriteLine($"DSP write {index}: ${writeAddress(native, index):X2}=${writeValue(native, index):X2}");
                        for (int voice = 0; voice < 8; voice++)
                            Console.WriteLine($"Voice {voice}: source=${readDsp(native, voice * 16 + 4):X2} envelope=${readDsp(native, voice * 16 + 8):X2}");
                        for (byte register = 0; register < 128; register++)
                        {
                            int expected = readDsp(native, register);
                            int value = managed.ReadDspRegisterForVerification(register);
                            if (value != expected) Console.WriteLine($"DSP ${register:X2}: managed=${value:X2}, native=${expected:X2}");
                        }
                        int first = Enumerable.Range(0, actual.Length).First(i => actual[i] != nativeHost[i]);
                        throw new InvalidDataException($"Native corpus {name} frame={frame} sample={first}: managed={actual[first]}, native={nativeHost[first]}.");
                    }
                    observePcm?.Invoke(frame, actual, nativeHost);
                    for (int port = 0; port < 4; port++)
                    {
                        int expected = read(native, port);
                        if (expected != managed.ReadPort(port)) throw new InvalidDataException($"Native port mismatch in {name}/{frame}/{port}.");
                        ports[port] = checked((byte)expected);
                    }
                    pcmHash.AppendData(nameBytes);
                    pcmHash.AppendData(MemoryMarshal.AsBytes(nativeHost.AsSpan()));
                    portHash.AppendData(nameBytes);
                    portHash.AppendData(ports);
                    acknowledge?.Invoke(ports);
                    totalFrames++;
                }
                if (missileImpactOnly && !impactChangedPcm)
                    throw new InvalidDataException("Impact request did not change audible PCM against the impact-muted control.");
            }
            finally { destroy(native); }
        }
    }
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate nint Create();
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate int Count(nint player);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate void Destroy(nint player);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate int Upload(nint player, byte[] data, int length);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate int Write(nint player, int port, byte value);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate int Read(nint player, int port);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate int Generate(nint player, [Out] short[] data, int frames);
}
