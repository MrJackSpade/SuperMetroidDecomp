using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

/// <summary>
/// Rechecks the desktop's shared-cue corpus against native BRR playback before accepting
/// a changed PCM golden hash. The native DLL is diagnostic-only and supplied explicitly.
/// </summary>
internal static class NativeAudioCorpusAudit
{
    public static int Run(string audioDirectory, string dllPath, string? romPath = null, string? recordingPath = null)
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
                Scenario("recorded-player-audio", recording.ControllerInputs.Length, frame =>
                {
                    var result = game.Step(recording.ControllerInputs[frame]);
                    if (result.GameState == SuperMetroidGameState.PausedB) pausedFrames++;
                    return result.AudioCommands.ToArray();
                }, ports => game.SetAudioAcknowledgements(new(ports[0], ports[1], ports[2], ports[3])));
                if (pausedFrames == 0) throw new InvalidDataException("Recording did not reach an active pause menu.");
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

        void Scenario(string name, int frames, Func<int, CartridgeAudioCommand[]> commands, Action<byte[]>? acknowledge = null)
        {
            nint native = create();
            if (native == 0) throw new InvalidOperationException("Native audio allocation failed.");
            try
            {
                var managed = new ManagedSpcPlayer();
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
                        }
                        else
                        {
                            if (write(native, command.Port, command.Value) != 1) throw new InvalidDataException("Native port write rejected.");
                            managed.WritePort(command.Port, command.Value);
                        }
                    }
                    // Read native samples at native rate, then use the independently tested
                    // host conversion. The DLL's old nearest-neighbor resampler is bypassed.
                    if (generate(native, nativeRaw, 534) != 534) throw new InvalidDataException("Native PCM frame incomplete.");
                    PcmFrameResampler.ResampleStereoLinear(nativeRaw, nativeHost);
                    managed.GenerateFrame(actual);
                    if (!actual.AsSpan().SequenceEqual(nativeHost))
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
