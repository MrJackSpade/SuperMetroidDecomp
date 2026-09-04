using SuperMetroid.Core.Audio;
using System.Runtime.InteropServices;
using System.Text.Json;

internal static partial class Program
{
    private static void VerifyManagedSnesDsp()
    {
        VerifyManagedDspResetProducesSilence();
        VerifyManagedDspPlaysConstructedPcmSample();
        VerifyManagedDspPlaysAndCancelsHighDefinitionReplacement();
        VerifyPcmReplacementPreservesStableIdentity();
        VerifyManagedDspRejectsInvalidBoundaries();
        VerifyManagedSpcUsesAddressedFirCoefficients();
    }

    /// <summary>
    /// Reproduces Kraid's post-defeat `$F7 02 0A 0A` command with a minimal APU image.
    /// Preset `$0A` deliberately lands beyond the four conventional filters and must read
    /// the resident bytes at `$1E82`, exactly as the SPC's indexed load does.
    /// </summary>
    private static void VerifyManagedSpcUsesAddressedFirCoefficients()
    {
        const ushort topLevel = 0x6000;
        const ushort patternTable = 0x6100;
        const ushort channelPattern = 0x6200;
        byte[] expectedCoefficients = [0x56, 0x65, 0x72, 0x20, 0x53, 0x31, 0x2e, 0x32];
        var upload = new List<byte>();

        AddSpcUploadRecord(upload, SpcDriverData.Ram.DefaultMusicPointer,
            [unchecked((byte)topLevel), (byte)(topLevel >> 8)]);
        AddSpcUploadRecord(upload, SpcDriverData.Ram.MusicTrackPointerTable,
            [unchecked((byte)topLevel), (byte)(topLevel >> 8)]);
        AddSpcUploadRecord(upload, topLevel,
            [unchecked((byte)patternTable), (byte)(patternTable >> 8)]);
        AddSpcUploadRecord(upload, patternTable,
            [
                unchecked((byte)channelPattern), (byte)(channelPattern >> 8),
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            ]);
        AddSpcUploadRecord(upload, channelPattern,
            [(byte)SpcMusicEffect.ConfigureEcho, 0x02, 0x0a, 0x0a, 1, SpcDriverData.Music.RestNote]);
        AddSpcUploadRecord(
            upload,
            SpcDriverData.Echo.FirCoefficientTableAddress + 10 * SpcDriverData.Echo.FirTapCount,
            expectedCoefficients);
        upload.Add(0);
        upload.Add(0);

        var player = new ManagedSpcPlayer();
        player.Upload(CollectionsMarshal.AsSpan(upload));
        player.WritePort(AudioRomData.Apu.MusicPort, 1);
        short[] pcm = new short[SpcDriverData.HostStereoFramesPerVideoFrame * 2];
        // The SPC starts at tempo $10 and requires multiple 64-cycle timer carries before
        // the two-tick track startup handshake reaches the first pattern command.
        for (int frame = 0; frame < 60; frame++)
            player.GenerateFrame(pcm);

        for (int tap = 0; tap < expectedCoefficients.Length; tap++)
        {
            byte register = unchecked((byte)(SnesDspRegisterMap.Global.FirstFirCoefficient +
                tap * SnesDspRegisterMap.VoiceStride));
            AssertEqual(
                expectedCoefficients[tap],
                player.ReadDspRegisterForVerification(register),
                $"addressed FIR coefficient {tap}");
        }
    }

    private static void AddSpcUploadRecord(List<byte> stream, ushort target, byte[] payload)
    {
        stream.Add(unchecked((byte)payload.Length));
        stream.Add(unchecked((byte)(payload.Length >> 8)));
        stream.Add(unchecked((byte)target));
        stream.Add(unchecked((byte)(target >> 8)));
        stream.AddRange(payload);
    }

    /// <summary>
    /// A reset S-DSP is muted. This catches accidental output from uninitialised voices,
    /// interpolation history, echo RAM, or the noise generator.
    /// </summary>
    private static void VerifyManagedDspResetProducesSilence()
    {
        byte[] apuRam = new byte[0x10000];
        ManagedSnesDsp dsp = new(apuRam);
        for (int index = 0; index < ManagedSnesDsp.NativeStereoFramesPerVideoFrame; index++)
            dsp.Cycle();

        short[] output = new short[1_600];
        dsp.CopyResampledSamples(output, 800);
        for (int index = 0; index < output.Length; index++)
            AssertEqual(0, output[index], $"reset managed S-DSP output sample {index}");
    }

    /// <summary>
    /// Exercises PCM source lookup, Gaussian interpolation, direct-gain envelope, pitch
    /// stepping, stereo volumes, and the end/loop path without a cartridge room.
    /// </summary>
    private static void VerifyManagedDspPlaysConstructedPcmSample()
    {
        byte[] apuRam = new byte[0x10000];
        ManagedSnesDsp dsp = new(apuRam);
        short[] waveform = Enumerable.Range(0, 16)
            .Select(index => unchecked((short)((index & 1) == 0 ? 14_336 : -16_384)))
            .ToArray();
        dsp.SetSampleBank(new ManagedPcmSampleBank(
            "constructed",
            0xC00000,
            new Dictionary<byte, ManagedPcmSample>
            {
                [0] = new ManagedPcmSample("constructed-loop", 32_000, waveform, 0),
            }));
        ConfigureAudibleVoiceZero(dsp);

        for (int index = 0; index < ManagedSnesDsp.NativeStereoFramesPerVideoFrame; index++)
            dsp.Cycle();

        short[] output = new short[1_600];
        dsp.CopyResampledSamples(output, 800);
        AssertTrue(output.Any(sample => sample != 0), "constructed PCM waveform is audible");
        AssertEqual(output[0], output[1], "constructed PCM waveform uses equal stereo volumes");
        AssertTrue((dsp.ReadRegister(0x7c) & 1) != 0, "constructed PCM loop raises ENDX");
    }

    /// <summary>
    /// A replacement WAV may carry more source frames than the stock 32-kHz decode. The DSP
    /// normalizes phase advance, applies the same live envelope/pan path, and still obeys key-off.
    /// </summary>
    private static void VerifyManagedDspPlaysAndCancelsHighDefinitionReplacement()
    {
        ManagedSnesDsp dsp = new(new byte[0x10000]);
        short[] waveform = Enumerable.Range(0, 96)
            .Select(index => unchecked((short)(Math.Sin(index * Math.PI / 12) * 20_000)))
            .ToArray();
        dsp.SetSampleBank(new ManagedPcmSampleBank(
            "hd-replacement",
            0xC00003,
            new Dictionary<byte, ManagedPcmSample>
            {
                [0] = new ManagedPcmSample("sample-00-00", 48_000, waveform, 24),
            }));
        ConfigureAudibleVoiceZero(dsp);

        for (int index = 0; index < ManagedSnesDsp.NativeStereoFramesPerVideoFrame; index++)
            dsp.Cycle();
        short[] output = new short[1_600];
        dsp.CopyResampledSamples(output, 800);
        AssertTrue(output.Any(sample => sample != 0), "48-kHz replacement waveform is audible");

        dsp.WriteRegister(0x5c, 1);
        for (int index = 0; index < 300; index++)
            dsp.Cycle();
        AssertEqual(0, dsp.ReadRegister(0x08), "replacement voice reaches silence after key-off");
    }

    private static void ConfigureAudibleVoiceZero(ManagedSnesDsp dsp)
    {
        dsp.WriteRegister(0x00, 0x7f);
        dsp.WriteRegister(0x01, 0x7f);
        dsp.WriteRegister(0x02, 0xff);
        dsp.WriteRegister(0x03, 0x3f);
        dsp.WriteRegister(0x04, 0);
        dsp.WriteRegister(0x05, 0);
        dsp.WriteRegister(0x07, 0x7f);
        dsp.WriteRegister(0x0c, 0x7f);
        dsp.WriteRegister(0x1c, 0x7f);
        dsp.WriteRegister(0x6c, 0x20);
        dsp.WriteRegister(0x4c, 1);
    }

    /// <summary>
    /// Exercises the build-stage replacement seam: the ID and bank aliases remain stable while
    /// a higher-rate WAV, rescaled loop position, sample count, and integrity hash replace data.
    /// </summary>
    private static void VerifyPcmReplacementPreservesStableIdentity()
    {
        string directory = Path.Combine(Path.GetTempPath(), $"sm-pcm-replacement-{Guid.NewGuid():N}");
        string samplesDirectory = Path.Combine(directory, "samples");
        Directory.CreateDirectory(samplesDirectory);
        try
        {
            string target = Path.Combine(samplesDirectory, "sample-00-00.wav");
            short[] stock = Enumerable.Range(0, 16).Select(index => unchecked((short)index)).ToArray();
            PcmWaveFile.WriteMonoPcm16(target, 32_000, stock);
            var original = new AudioCanonicalSampleMetadata(
                "sample-00-00",
                "samples/sample-00-00.wav",
                32_000,
                stock.Length,
                8,
                Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(target))));
            var manifest = new AudioAssetManifest(
                AudioAssetManifest.CurrentFormatVersion,
                [],
                [original],
                [],
                []);
            File.WriteAllText(
                Path.Combine(directory, ExtractedAudioAssetCatalog.ManifestFileName),
                JsonSerializer.Serialize(manifest, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true,
                }));

            string replacement = Path.Combine(directory, "replacement.wav");
            short[] hd = Enumerable.Range(0, 32)
                .Select(index => unchecked((short)(index * 257)))
                .ToArray();
            PcmWaveFile.WriteMonoPcm16(replacement, 48_000, hd);
            AudioCanonicalSampleMetadata installed = PcmSampleReplacementInstaller.Install(
                directory,
                original.Id,
                replacement,
                PcmSampleLoopReplacement.PreserveTime);

            AssertEqual(original.Id, installed.Id, "PCM replacement stable sample ID");
            AssertEqual(48_000, installed.SampleRate, "PCM replacement sample rate");
            AssertEqual(32, installed.SampleCount, "PCM replacement sample count");
            AssertEqual(12, installed.LoopSampleIndex, "PCM replacement time-scaled loop");
            (int rate, short[] samples) = PcmWaveFile.ReadMonoPcm16(File.ReadAllBytes(target), target);
            AssertEqual(48_000, rate, "installed PCM WAV rate");
            AssertTrue(samples.SequenceEqual(hd), "installed PCM WAV content");

            AssertThrows<InvalidDataException>(
                () => PcmSampleReplacementInstaller.Install(
                    directory,
                    original.Id,
                    replacement,
                    PcmSampleLoopReplacement.At(hd.Length)),
                "PCM replacement rejects loop at end of sample");
        }
        finally
        {
            if (Directory.Exists(directory))
                Directory.Delete(directory, recursive: true);
        }
    }

    private static void VerifyManagedDspRejectsInvalidBoundaries()
    {
        AssertThrows<ArgumentException>(
            () => _ = new ManagedSnesDsp(new byte[0xffff]),
            "managed DSP rejects non-64-KiB APU RAM");

        ManagedSnesDsp dsp = new(new byte[0x10000]);
        AssertThrows<ArgumentOutOfRangeException>(
            () => dsp.WriteRegister(0x80, 0),
            "managed DSP rejects registers beyond $7F");
        AssertThrows<ArgumentOutOfRangeException>(
            () => dsp.CopyResampledSamples(new short[2], 0),
            "managed DSP rejects an empty host frame");
        dsp.SetSampleBank(new ManagedPcmSampleBank(
            "missing-source",
            0xC00000,
            new Dictionary<byte, ManagedPcmSample>()));
        AssertThrows<InvalidDataException>(
            () => dsp.WriteRegister(0x4c, 1),
            "managed DSP rejects an unmapped source at key-on");
        AssertThrows<InvalidDataException>(
            () => _ = new ManagedPcmSample(
                "bad-rate",
                PcmSampleFormat.MinimumReplacementSampleRate - 1,
                [0],
                null),
            "managed PCM rejects unsupported sample rate");
        AssertThrows<InvalidDataException>(
            () => _ = new ManagedPcmSample("bad-loop", 32_000, [0], 1),
            "managed PCM rejects loop outside waveform");
        AssertThrows<InvalidDataException>(
            () => PcmWaveFile.ReadMonoPcm16("not-wave"u8, "constructed invalid WAV"),
            "managed PCM rejects malformed WAV container");
    }
}
