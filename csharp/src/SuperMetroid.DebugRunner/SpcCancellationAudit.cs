using SuperMetroid.Core.Audio;

/// <summary>Managed counterpart of the console-only original-SPC cancellation probe.</summary>
internal static class SpcCancellationAudit
{
    public static int Run(string audioDirectory, int musicAddress)
    {
        var assets = ExtractedAudioAssetCatalog.Load(audioDirectory);
        var player = new ManagedSpcPlayer();
        player.Upload(assets.GetUpload(AudioUploadAddresses.SpcEngine).Span);
        player.Upload(assets.GetUpload(musicAddress).Span);
        player.SetSampleBank(assets.GetSampleBank(musicAddress));
        var samples = new short[1600];
        bool heard = false;
        for (int frame = 0; frame < 600; frame++)
        {
            if (frame == 60) player.WritePort(0, 5);
            if (frame == 240) player.WritePort(1, SoundEffectLibrary1Sounds.ChargeBeamStart.Value);
            if (frame == 300)
            {
                player.WritePort(1, SoundEffectLibrary1Sounds.CancelAll.Value);
                player.WritePort(2, SoundEffectLibrary2Sounds.CancelAll.Value);
                player.WritePort(3, SoundEffectLibrary3Sounds.CancelAll.Value);
            }
            player.GenerateFrame(samples);
            long energy = 0;
            int peak = 0;
            foreach (short sample in samples)
            {
                energy += (long)sample * sample;
                peak = Math.Max(peak, Math.Abs((int)sample));
            }
            if (!heard && peak != 0) { Console.WriteLine($"FIRST_PCM frame={frame}"); heard = true; }
            if (frame % 30 != 0 && (frame < 298 || frame > 315)) continue;
            // Host-rate PCM cannot be compared sample-for-sample to the CPU probe's
            // native-rate PCM. These metrics locate gross divergence; DSP registers
            // and acknowledgements expose command handling independently of resampling.
            Console.WriteLine($"MANAGED frame={frame} ports={player.ReadPort(0):X2},{player.ReadPort(1):X2},{player.ReadPort(2):X2},{player.ReadPort(3):X2} " +
                $"peak={peak} mean-square={energy / samples.Length} " +
                $"FLG={player.ReadDspRegisterForVerification(SnesDspRegisterMap.Global.Flags):X2} " +
                $"NON={player.ReadDspRegisterForVerification(SnesDspRegisterMap.Global.NoiseEnable):X2} " +
                $"EON={player.ReadDspRegisterForVerification(SnesDspRegisterMap.Global.EchoEnable):X2} " +
                $"SRC=[{string.Join(',', Enumerable.Range(0, 8).Select(voice => player.ReadDspRegisterForVerification((byte)(voice * 16 + 4)).ToString("X2")))}]");
        }
        return 0;
    }
}
