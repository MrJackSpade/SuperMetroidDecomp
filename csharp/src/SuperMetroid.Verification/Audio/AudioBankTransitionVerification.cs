using SuperMetroid.AssetExtraction.Audio;
using static SuperMetroid.Verification.Audio.ManagedAudioVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.AssetExtraction.Audio.Synthesis;
namespace SuperMetroid.Verification.Audio;
using SuperMetroid.Verification.Harness;

/// <summary>Checks DSP voice retirement and music playback across SPC sample-bank uploads.</summary>
internal static class AudioBankTransitionVerification
{
    /// <summary>Confirms a released voice remains silent after its sample disappears while BRR headers still set ENDX and a new key-on plays.</summary>
    internal static void VerifyReleasedVoiceBankSwitch()
    {
        var ram = new byte[65536];
        var dsp = new ManagedSnesDsp(ram);
        var positive = new ManagedPcmSample("positive", 32000, Enumerable.Repeat((short)8000, 16).ToArray(), 0);
        dsp.SetSampleBank(new ManagedPcmSampleBank("before-upload", 0,
            new Dictionary<int, ManagedPcmSample> { [0] = positive }));
        ConfigureAudibleVoiceZero(dsp);
        for (int cycle = 0; cycle < 32; cycle++) dsp.Cycle();
        dsp.WriteRegister((byte)DspGlobalRegister.KeyOff, 1);
        for (int cycle = 0; cycle < 300; cycle++) dsp.Cycle();
        AssertEqual((byte)0, dsp.ReadRegister(SnesDspRegisterLayout.Voice.EnvelopeOutput), "release reaches zero");
        // DIR=0, source 0 loop=$FFFF. BRR advances by nine bytes and wraps to $0008.
        ram[2] = ram[3] = byte.MaxValue;
        ram[ushort.MaxValue] = 0;
        ram[8] = 1;
        dsp.SetSampleBank(new ManagedPcmSampleBank("after-upload", 0,
            new Dictionary<int, ManagedPcmSample> { [1] = positive }));
        for (int cycle = 0; cycle < 32; cycle++) dsp.Cycle();
        dsp.WriteRegister((byte)DspGlobalRegister.EndFlags, 0);
        for (int cycle = 0; cycle < 32; cycle++) dsp.Cycle();
        AssertTrue((dsp.ReadRegister((byte)DspGlobalRegister.EndFlags) & 1) != 0, "retired raw headers continue setting ENDX");
        AssertEqual((byte)0, dsp.ReadRegister(SnesDspRegisterLayout.Voice.SampleOutput), "retired voice remains silent");
        dsp.WriteRegister((byte)DspVoiceRegister.SourceNumber, 1);
        dsp.WriteRegister((byte)DspGlobalRegister.KeyOn, 1);
        for (int cycle = 0; cycle < 32; cycle++) dsp.Cycle();
        AssertTrue(unchecked((sbyte)dsp.ReadRegister(SnesDspRegisterLayout.Voice.SampleOutput)) > 0, "key-on restores ordinary PCM playback");
        dsp.WriteRegister((byte)DspVoiceRegister.SourceNumber, 0);
        AssertThrows<InvalidDataException>(() =>
        {
            for (int cycle = 0; cycle < 32; cycle++) dsp.Cycle();
        }, "an audible unmapped source must still fail loudly");
    }

    /// <summary>Checks that each installed music bank can transition to the captivity track with audible output.</summary>
    internal static void VerifyAudioBankTransition(string directory)
    {
        Suite(nameof(VerifyReleasedVoiceBankSwitch), () => VerifyReleasedVoiceBankSwitch());
        var assets = ExtractedAudioAssetCatalog.Load(directory);
        foreach (var bank in AudioAssetCatalogDefinitions.Music)
        {
            var renderer = AudioSessionAccess.CreateSynthesizing(assets);
            renderer.RenderFrame([new AudioRequest.ChangeMusicDataSet(null)]);
            renderer.RenderFrame([new AudioRequest.ChangeMusicDataSet(bank.DataSet)]);
            renderer.RenderFrame([AudioRequest.ForTrack(new MusicTrack(5))]);
            for (int frame = 0; frame < 600; frame++) renderer.RenderFrame([]);
            renderer.RenderFrame([new AudioRequest.ChangeMusicDataSet(MusicDataSet.LastMetroidInCaptivity)]);
            renderer.RenderFrame([AudioRequest.ForTrack(new MusicTrack(5))]);
            bool audible = false;
            for (int frame = 0; frame < 600; frame++)
                audible |= renderer.RenderFrame([]).Any(sample => sample != 0);
            AssertTrue(audible, $"{bank.Name} -> captivity retains audible playback");
            Console.WriteLine($"{bank.Name} -> captivity passed.");
        }
    }

    /// <summary>Runs the DSP release and installed music-bank transition checks.</summary>
    [VerificationCommand("--audio-bank-transition")]
    internal static int AudioBankTransitionCommand()
    {
        string audioDirectory = RepositoryInstallation.Installation.AudioDirectory;
        Suite(nameof(VerifyAudioBankTransition), () => VerifyAudioBankTransition(audioDirectory));
        return 0;
    }
}
