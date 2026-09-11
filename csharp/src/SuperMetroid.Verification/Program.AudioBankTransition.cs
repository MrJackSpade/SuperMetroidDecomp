using SuperMetroid.Core.Audio;

internal static partial class Program
{
    private static void VerifyReleasedVoiceBankSwitch()
    {
        var ram = new byte[65536];
        var dsp = new ManagedSnesDsp(ram);
        var positive = new ManagedPcmSample("positive", 32000, Enumerable.Repeat((short)8000, 16).ToArray(), 0);
        dsp.SetSampleBank(new ManagedPcmSampleBank("before-upload", 0,
            new Dictionary<byte, ManagedPcmSample> { [0] = positive }));
        ConfigureAudibleVoiceZero(dsp);
        for (int cycle = 0; cycle < 32; cycle++) dsp.Cycle();
        dsp.WriteRegister(SnesDspRegisterMap.Global.KeyOff, 1);
        for (int cycle = 0; cycle < 300; cycle++) dsp.Cycle();
        AssertEqual((byte)0, dsp.ReadRegister(SnesDspRegisterMap.Voice.EnvelopeOutput), "release reaches zero");
        // DIR=0, source 0 loop=$FFFF. BRR advances by nine bytes and wraps to $0008.
        ram[2] = ram[3] = byte.MaxValue;
        ram[ushort.MaxValue] = 0;
        ram[8] = 1;
        dsp.SetSampleBank(new ManagedPcmSampleBank("after-upload", 0,
            new Dictionary<byte, ManagedPcmSample> { [1] = positive }));
        for (int cycle = 0; cycle < 32; cycle++) dsp.Cycle();
        dsp.WriteRegister(SnesDspRegisterMap.Global.EndFlags, 0);
        for (int cycle = 0; cycle < 32; cycle++) dsp.Cycle();
        AssertTrue((dsp.ReadRegister(SnesDspRegisterMap.Global.EndFlags) & 1) != 0, "retired raw headers continue setting ENDX");
        AssertEqual((byte)0, dsp.ReadRegister(SnesDspRegisterMap.Voice.SampleOutput), "retired voice remains silent");
        dsp.WriteRegister(SnesDspRegisterMap.Voice.SourceNumber, 1);
        dsp.WriteRegister(SnesDspRegisterMap.Global.KeyOn, 1);
        for (int cycle = 0; cycle < 32; cycle++) dsp.Cycle();
        AssertTrue(unchecked((sbyte)dsp.ReadRegister(SnesDspRegisterMap.Voice.SampleOutput)) > 0, "key-on restores ordinary PCM playback");
        dsp.WriteRegister(SnesDspRegisterMap.Voice.SourceNumber, 0);
        AssertThrows<InvalidDataException>(() =>
        {
            for (int cycle = 0; cycle < 32; cycle++) dsp.Cycle();
        }, "an audible unmapped source must still fail loudly");
    }

    private static void VerifyAudioBankTransition(string directory)
    {
        VerifyReleasedVoiceBankSwitch();
        var assets = ExtractedAudioAssetCatalog.Load(directory);
        foreach (var bank in AudioAssetCatalogData.Music)
        {
            var renderer = new CartridgeAudioRenderer(assets);
            renderer.RenderFrame([CartridgeAudioCommand.Upload(AudioUploadAddresses.SpcEngine)]);
            renderer.RenderFrame([CartridgeAudioCommand.Upload(bank.SnesAddress)]);
            renderer.RenderFrame([CartridgeAudioCommand.WritePort(0, 5)]);
            for (int frame = 0; frame < 600; frame++) renderer.RenderFrame([]);
            renderer.RenderFrame([CartridgeAudioCommand.Upload(AudioUploadAddresses.LastMetroidInCaptivity)]);
            renderer.RenderFrame([CartridgeAudioCommand.WritePort(0, 5)]);
            bool audible = false;
            for (int frame = 0; frame < 600; frame++)
                audible |= renderer.RenderFrame([]).Any(sample => sample != 0);
            AssertTrue(audible, $"{bank.Name} -> captivity retains audible playback");
            Console.WriteLine($"{bank.Name} -> captivity passed.");
        }
    }
}
