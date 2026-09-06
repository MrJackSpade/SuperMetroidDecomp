using SuperMetroid.Core.Audio;

/// <summary>Compares native-rate PCM of one extracted source with native BRR DSP playback.</summary>
internal static class DspSampleComparisonAudit
{
    public static int Run(string directory, int musicAddress, byte source, string trace)
    {
        var assets = ExtractedAudioAssetCatalog.Load(directory);
        var bank = assets.GetSampleBank(musicAddress);
        var sample = bank.Resolve(source);
        var dsp = new ManagedSnesDsp(new byte[65536]);
        dsp.SetSampleBank(bank);
        dsp.WriteRegister(0x6c, 0x20);
        dsp.WriteRegister(0x5d, 0x6d);
        dsp.WriteRegister(0x0c, 0x7f); dsp.WriteRegister(0x1c, 0x7f);
        dsp.WriteRegister(0x00, 0x7f); dsp.WriteRegister(0x01, 0x7f);
        dsp.WriteRegister(0x02, 0); dsp.WriteRegister(0x03, 0x10);
        dsp.WriteRegister(0x04, source);
        dsp.WriteRegister(0x05, 0); dsp.WriteRegister(0x07, 0x7f);
        dsp.WriteRegister(0x5c, 0); dsp.WriteRegister(0x4c, 1);
        var pcm = new short[1068];
        int frames = 0, mismatches = 0;
        foreach (string line in File.ReadLines(trace))
        {
            if (!line.StartsWith("S ", StringComparison.Ordinal)) continue;
            string[] parts = line.Split(' ');
            if (int.Parse(parts[1]) != frames) throw new InvalidDataException("DSP trace is not sequential.");
            for (int tick = 0; tick < 534; tick++) dsp.Cycle();
            dsp.CopyResampledSamples(pcm, 534); // Same rate: interpolation is the identity.
            uint hash = 2166136261;
            foreach (short value in pcm) hash = unchecked((hash ^ (ushort)value) * 16777619);
            uint expected = Convert.ToUInt32(parts[2], 16);
            if (hash != expected)
            {
                if (mismatches < 4) Console.WriteLine($"DIFF sample={source:X2} frame={frames} actual={hash:X8} expected={expected:X8}");
                mismatches++;
            }
            frames++;
        }
        Console.WriteLine($"DSP sample={source:X2} id={sample.Id} length={sample.Samples.Length} loop={sample.LoopSampleIndex} frames={frames} mismatched={mismatches}.");
        return frames == 128 && mismatches == 0 ? 0 : 1;
    }
}
