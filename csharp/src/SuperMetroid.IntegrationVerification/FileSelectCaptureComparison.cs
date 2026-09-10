using System.Security.Cryptography;
using System.Text;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

/// <summary>
/// Local-only diagnostic for a reset-origin journal and Android output WAV. Compares
/// the same frontend route with and without its file-acceptance sound. Normalized
/// correlation tolerates device volume, but not missing/replaced sound content.
/// This is evidence reporting, not an automatic issue-closure decision.
/// </summary>
internal static class FileSelectCaptureComparison
{
    public static int Run(string journalPath, string wavePath)
    {
        VerifyMatcher();
        using var input = File.OpenRead(journalPath);
        var recording = ControllerInputRecording.Read(input);
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        if (!SHA256.HashData(bus.Rom).AsSpan().SequenceEqual(recording.RomSha256))
            throw new InvalidDataException("Journal and local ROM differ.");
        // Caller must select a reset-origin journal, not a debugger-state continuation.
        recording.InitialSaveRam.CopyTo(bus.SaveRam);
        var game = new SuperMetroidGame(bus, recording.GameOptions);
        var assets = ExtractedAudioAssetCatalog.Load(Path.GetFullPath("standalone-assets/audio"));
        var actual = new CartridgeAudioRenderer(assets);
        var muted = new CartridgeAudioRenderer(assets);
        var expected = new List<double>();
        var control = new List<double>();
        int requests = 0, firstDifference = -1;
        for (int tick = 0; tick < recording.ControllerInputs.Length; tick++)
        {
            game.SetAudioAcknowledgements(actual.ReadAcknowledgements());
            var frame = game.StepCaptured(recording.ControllerInputs[tick], tick + 1, 1).Frame;
            bool IsSwoosh(CartridgeAudioCommand command) =>
                command.Kind == CartridgeAudioCommandKind.WritePort && command.Port == 1 &&
                command.Value == SoundEffectLibrary1Sounds.FileSelectSwoosh.Value;
            if (frame.AudioCommands.Any(IsSwoosh))
            {
                requests++;
                Console.WriteLine($"Swoosh command: tick={tick}, phase={frame.Phase}");
            }
            short[] pcm = actual.RenderFrame(frame.AudioCommands);
            short[] suppressed = muted.RenderFrame(frame.AudioCommands.Where(c => !IsSwoosh(c)).ToArray());
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

    private static (int Offset, double Score) Find(double[] capture, double[] target)
    {
        if (capture.Length < target.Length) throw new InvalidDataException("Capture is shorter than comparison window.");
        int best = 0;
        double score = double.NegativeInfinity;
        // Coarse search is decimated for speed; final comparison uses every sample.
        for (int offset = 0; offset <= capture.Length - target.Length; offset += 8)
        {
            double candidate = Correlation(capture, target, offset, 16);
            if (candidate > score) (best, score) = (offset, candidate);
        }
        int center = best;
        score = double.NegativeInfinity;
        for (int offset = Math.Max(0, center - 8); offset <= Math.Min(capture.Length - target.Length, center + 8); offset++)
        {
            double candidate = Correlation(capture, target, offset, 1);
            if (candidate > score) (best, score) = (offset, candidate);
        }
        return (best, score);
    }

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
