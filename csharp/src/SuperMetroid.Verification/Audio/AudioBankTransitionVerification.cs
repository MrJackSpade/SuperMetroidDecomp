using SuperMetroid.AssetExtraction.Audio;
using static SuperMetroid.Verification.Audio.ManagedAudioVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.Audio.Synthesis;
namespace SuperMetroid.Verification.Audio;
using SuperMetroid.Verification.Harness;

/// <summary>Checks DSP voice retirement and music playback across SPC sample-bank uploads.</summary>
internal static class AudioBankTransitionVerification
{
    /// <summary>Confirms a released voice remains silent after its sample disappears while BRR headers still set ENDX and a new key-on plays.</summary>
    internal static void VerifyReleasedVoiceBankSwitch()
    {
        var ram = new SpcRam();
        var dsp = new ManagedSnesDsp(ram);
        var positive = new ManagedPcmSample("positive", 32000, Enumerable.Repeat((short)8000, 16).ToArray(), 0);
        dsp.SetSampleBank(new ManagedPcmSampleBank("before-upload", 0,
            new Dictionary<int, ManagedPcmSample> { [0] = positive }));
        ConfigureAudibleVoiceZero(dsp);
        for (int cycle = 0; cycle < 32; cycle++) dsp.Cycle();
        dsp.WriteRegister(checked((byte)SnesDspRegisterLayout.AddressOf(DspGlobalRegister.KeyOff)), 1);
        for (int cycle = 0; cycle < 300; cycle++) dsp.Cycle();
        AssertEqual((byte)0, dsp.ReadRegister(SnesDspRegisterLayout.Voice.EnvelopeOutput), "release reaches zero");
        // DIR=0, source 0 loop=$FFFF. BRR advances by nine bytes and wraps to $0008.
        ram[2] = ram[3] = byte.MaxValue;
        ram[ushort.MaxValue] = 0;
        ram[8] = 1;
        dsp.SetSampleBank(new ManagedPcmSampleBank("after-upload", 0,
            new Dictionary<int, ManagedPcmSample> { [1] = positive }));
        for (int cycle = 0; cycle < 32; cycle++) dsp.Cycle();
        dsp.WriteRegister(checked((byte)SnesDspRegisterLayout.AddressOf(DspGlobalRegister.EndFlags)), 0);
        for (int cycle = 0; cycle < 32; cycle++) dsp.Cycle();
        AssertTrue((dsp.ReadRegister(checked((byte)SnesDspRegisterLayout.AddressOf(DspGlobalRegister.EndFlags))) & 1) != 0, "retired raw headers continue setting ENDX");
        AssertEqual((byte)0, dsp.ReadRegister(SnesDspRegisterLayout.Voice.SampleOutput), "retired voice remains silent");
        dsp.WriteRegister((byte)DspVoiceRegister.SourceNumber, 1);
        dsp.WriteRegister(checked((byte)SnesDspRegisterLayout.AddressOf(DspGlobalRegister.KeyOn)), 1);
        for (int cycle = 0; cycle < 32; cycle++) dsp.Cycle();
        AssertTrue(unchecked((sbyte)dsp.ReadRegister(SnesDspRegisterLayout.Voice.SampleOutput)) > 0, "key-on restores ordinary PCM playback");
        dsp.WriteRegister((byte)DspVoiceRegister.SourceNumber, 0);
        AssertThrows<InvalidDataException>(() =>
        {
            for (int cycle = 0; cycle < 32; cycle++) dsp.Cycle();
        }, "an audible unmapped source must still fail loudly");
    }

    /// <summary>Each installed music bank's transition to the captivity track, a suite of its own.</summary>
    internal static FullRunSuitesVerification.RegisteredSuite[] AudioBankTransitionSuites() =>
    [
        .. AudioAssetCatalogDefinitions.Music.Select(bank =>
            new FullRunSuitesVerification.RegisteredSuite($"{nameof(VerifyAudioBankTransition)}{bank.Name}", () => VerifyAudioBankTransition(bank))),
    ];

    /// <summary>Checks that <paramref name="bank"/>'s area theme can hand over to the captivity track with audible output.</summary>
    private static void VerifyAudioBankTransition(AudioUploadAssetDefinition bank)
    {
        var renderer = AudioSessionAccess.CreateSynthesizing(InstalledFixtures.Audio);
        renderer.RenderFrame([new AudioRequest.ChangeMusicDataSet(null)]);
        MusicDataSet dataSet = bank.DataSet ?? throw new InvalidOperationException($"{bank.Name} holds no music data set.");
        renderer.RenderFrame([new AudioRequest.ChangeMusicDataSet(dataSet)]);
        renderer.RenderFrame([AudioRequest.ForTrack(AreaThemeDefinitions.Themes[dataSet])]);
        for (int frame = 0; frame < 600; frame++) renderer.RenderFrame([]);
        renderer.RenderFrame([new AudioRequest.ChangeMusicDataSet(MusicDataSet.LastMetroidInCaptivity)]);
        renderer.RenderFrame([AudioRequest.ForTrack(MusicTrack.LastMetroidInCaptivity)]);
        bool audible = false;
        for (int frame = 0; frame < 600; frame++)
            audible |= renderer.RenderFrame([]).Any(sample => sample != 0);
        AssertTrue(audible, $"{bank.Name} -> captivity retains audible playback");
        Console.WriteLine($"{bank.Name} -> captivity passed.");
    }

    /// <summary>Runs every installed music bank's transition check, one suite per bank.</summary>
    [VerificationCommand("--audio-bank-transition")]
    internal static int AudioBankTransitionCommand()
    {
        foreach (FullRunSuitesVerification.RegisteredSuite suite in AudioBankTransitionSuites())
            Suite(suite.Name, suite.Body);
        return 0;
    }
}
