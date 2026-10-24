using SuperMetroid.Diagnostics.Saves;
using System.Security.Cryptography;
using System.Text;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Diagnostics.Recording;
namespace SuperMetroid.DebugRunner.Tools;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation;

/// <summary>
/// Local-only diagnostic for a reset-origin journal and Android output WAV. Compares
/// the same frontend route with and without its file-acceptance sound. Normalized
/// correlation tolerates device volume, but not missing/replaced sound content.
/// This is evidence reporting, not an automatic issue-closure decision.
/// </summary>
internal static class FileSelectCaptureComparison
{
    /// <summary>Compares a reset-origin file-selection replay's synthesized swoosh with captured Android PCM.</summary>
    public static int Run(string journalPath, string wavePath)
    {
        VerifyMatcher();
        using var input = File.OpenRead(journalPath);
        var recording = ControllerInputRecording.Read(input);
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        if (!SHA256.HashData(bus.Rom).SequenceEqual(recording.RomSha256))
            throw new InvalidDataException("Journal and local ROM differ.");
        // Caller must select a reset-origin journal, not a debugger-state continuation.
        var game = new SuperMetroidGame(
            SaveRamCodec.Decode(recording.InitialSaveRam, RepositoryInstallation.Installation.LoadMaps()), recording.GameOptions);
        ClipAudioCatalog clips = RepositoryInstallation.Installation.LoadAudioClips();
        game.BindAudio(clips);
        var pcm = new short[CartridgeAudioSession.PcmSamplesPerFrame];
        var muted = new ClipAudioBackend(clips);
        var suppressed = new short[CartridgeAudioSession.PcmSamplesPerFrame];
        var expected = new List<double>();
        var control = new List<double>();
        int requests = 0, firstDifference = -1;
        for (int tick = 0; tick < recording.ControllerInputs.Length; tick++)
        {
            var frame = game.StepCaptured(recording.InputAt(tick), tick + 1, 1).Frame;
            static bool IsSwoosh(AudioEvent audioEvent) => audioEvent is AudioEvent.StartSoundEffect started &&
                started.SoundEffect == Library1Sound.FileSelected;
            if (game.Session.LastAudioEvents.Any(IsSwoosh))
            {
                requests++;
                Console.WriteLine($"Swoosh start: tick={tick}, phase={frame.Phase}");
            }
            game.CopyAudioFrame(pcm);
            muted.Render(new AudioFrameEvents([.. game.Session.LastAudioEvents.Where(audioEvent => !IsSwoosh(audioEvent))]), suppressed);
            for (int i = 0; i < pcm.Length; i += 2)
            {
                double sample = (pcm[i] + pcm[i + 1]) * 0.5;
                double other = (suppressed[i] + suppressed[i + 1]) * 0.5;
                if (sample != other && firstDifference < 0) firstDifference = expected.Count;
                expected.Add(sample);
                control.Add(other);
            }
        }
        if (requests != 1 || firstDifference < 0)
            throw new InvalidDataException("Expected one audible file selection in this reset-origin journal.");
        // A quarter-second window beginning at actual PCM onset isolates the swoosh
        // from later option-screen actions. Do not align using silence before onset.
        int length = Math.Min(12000, expected.Count - firstDifference);
        var target = expected.GetRange(firstDifference, length).ToArray();
        var absent = control.GetRange(firstDifference, length).ToArray();
        double[] captured = ReadWave(wavePath);
        var best = Find(captured, target);
        double absentScore = Correlation(captured, absent, best.Offset, 1);
        Console.WriteLine($"PCM onset={firstDifference / 48000.0:F6}s; capture offset={best.Offset / 48000.0:F6}s; window={length / 48000.0:F3}s");
        Console.WriteLine($"Correlation: with swoosh={best.Score:F6}; suppressed at same offset={absentScore:F6}");
        return 0;
    }

    /// <summary>Checks the correlation search against a known shifted and gain-scaled signal.</summary>
    private static void VerifyMatcher()
    {
        // A known shifted, volume-scaled signal checks the measurement itself before
        // it is used as evidence. The offset intentionally falls between coarse steps.
        var target = Enumerable.Range(0, 1200).Select(i =>
            Math.Sin(i * 0.037) + 0.6 * Math.Sin(i * 0.083) + 0.2 * Math.Cos(i * 0.113)).ToArray();
        var capture = new double[5000];
        const int insertedOffset = 1539;
        for (int i = 0; i < target.Length; i++) capture[insertedOffset + i] = target[i] * 0.37;
        var found = Find(capture, target);
        if (found.Offset != insertedOffset || found.Score < 0.999999)
            throw new InvalidDataException("Waveform matcher failed known offset/gain fixture.");
        if (Correlation(new double[5000], target, insertedOffset, 1) != 0)
            throw new InvalidDataException("Silent capture incorrectly matches a nonzero signal.");
    }

    /// <summary>Finds the captured window with strongest normalized correlation using coarse then exact offsets.</summary>
    private static CaptureAlignment Find(double[] capture, double[] target)
    {
        if (capture.Length < target.Length) throw new InvalidDataException("Capture is shorter than comparison window.");
        int best = 0;
        double score = double.NegativeInfinity;
        // Coarse search is decimated for speed; final comparison uses every sample.
        for (int offset = 0; offset <= capture.Length - target.Length; offset += 8)
        {
            double candidate = Correlation(capture, target, offset, 16);
            if (candidate > score) {best = offset;
score = candidate;
}        }
        int center = best;
        score = double.NegativeInfinity;
        for (int offset = Math.Max(0, center - 8); offset <= Math.Min(capture.Length - target.Length, center + 8); offset++)
        {
            double candidate = Correlation(capture, target, offset, 1);
            if (candidate > score) {best = offset;
score = candidate;
}        }
        return new CaptureAlignment(best, score);
    }

    /// <summary>Measures gain-independent similarity of one captured window and a target waveform.</summary>
    private static double Correlation(double[] capture, double[] target, int offset, int stride)
    {
        double dot = 0, a = 0, b = 0;
        for (int i = 0; i < target.Length; i += stride)
        {
            double x = capture[offset + i], y = target[i];
            dot += x * y;
            a += x * x;
            b += y * y;
        }
        return a == 0 || b == 0 ? 0 : dot / Math.Sqrt(a * b);
    }

    /// <summary>Reads a 48 kHz stereo PCM16 WAV and averages each pair into a mono comparison sample.</summary>
    private static double[] ReadWave(string path)
    {
        using var reader = new BinaryReader(File.OpenRead(path));
        string Tag() => Encoding.ASCII.GetString(reader.ReadBytes(4));
        if (Tag() != "RIFF") throw new InvalidDataException("Expected RIFF WAV.");
        reader.ReadUInt32();
        if (Tag() != "WAVE") throw new InvalidDataException("Expected WAVE form.");
        bool supported = false;
        while (reader.BaseStream.Position + 8 <= reader.BaseStream.Length)
        {
            string tag = Tag();
            uint size = reader.ReadUInt32();
            long end = reader.BaseStream.Position + size;
            if (end > reader.BaseStream.Length) throw new InvalidDataException("Truncated WAV chunk.");
            if (tag == "fmt " && size >= 16)
            {
                supported = reader.ReadUInt16() == 1 && reader.ReadUInt16() == 2 && reader.ReadUInt32() == 48000;
                reader.BaseStream.Position += 6;
                supported &= reader.ReadUInt16() == 16;
            }
            else if (tag == "data")
            {
                if (!supported || size % 4 != 0) throw new InvalidDataException("Expected 48 kHz stereo PCM16 WAV.");
                var samples = new double[size / 4];
                for (int i = 0; i < samples.Length; i++) samples[i] = (reader.ReadInt16() + reader.ReadInt16()) * 0.5;
                return samples;
            }
            reader.BaseStream.Position = end + (size & 1);
        }
        throw new InvalidDataException("WAV omitted audio data.");
    }
}
