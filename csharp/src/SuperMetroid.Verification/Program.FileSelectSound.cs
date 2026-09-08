using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

internal static partial class Program
{
    private static void VerifyFileSelectSound()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var audio = new CartridgeAudioState();
        var menu = new FileSelectMenuState(bus, audio);
        var renderer = new CartridgeAudioRenderer(ExtractedAudioAssetCatalog.Load(Path.GetFullPath("standalone-assets/audio")));
        int writes = 0, nonzero = 0;
        for (int frame = 0; frame < 180; frame++)
        {
            menu.Step(frame == 30 ? (ushort)SnesButton.A : (ushort)0);
            var commands = audio.AdvanceFrame(bus, renderer.ReadAcknowledgements());
            writes += commands.Count(command => command.Kind == CartridgeAudioCommandKind.WritePort && command.Port == 1 && command.Value == SoundEffectLibrary1Sounds.FileSelectSwoosh.Value);
            var pcm = renderer.RenderFrame(commands);
            if (frame < 30) AssertTrue(pcm.All(value => value == 0), "isolated selection must be silent before accept");
            else nonzero += pcm.Count(value => value != 0);
        }
        AssertEqual(1, writes, "one file selection sends exactly one cartridge swoosh request");
        AssertTrue(nonzero > 0, "file selection generates audible PCM");
        Console.WriteLine($"File selection: {writes} sound writes, {nonzero} nonzero PCM samples.");
        VerifyFullFrontendFileSelectSound();
    }

    private static void VerifyFullFrontendFileSelectSound()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var game = new SuperMetroidGame(bus);
        var assets = ExtractedAudioAssetCatalog.Load(Path.GetFullPath("standalone-assets/audio"));
        var actual = new CartridgeAudioRenderer(assets);
        var withoutSwoosh = new CartridgeAudioRenderer(assets);
        int writes = 0, differentSamples = 0;
        for (int tick = 0; tick < 500; tick++)
        {
            game.SetAudioAcknowledgements(actual.ReadAcknowledgements());
            var frame = game.StepCaptured(tick % 47 == 0 ? (ushort)SnesButton.Start : (ushort)0, tick + 1, 1).Frame;
            bool IsSwoosh(CartridgeAudioCommand command) => command.Kind == CartridgeAudioCommandKind.WritePort &&
                command.Port == 1 && command.Value == SoundEffectLibrary1Sounds.FileSelectSwoosh.Value;
            writes += frame.AudioCommands.Count(IsSwoosh);
            short[] pcm = actual.RenderFrame(frame.AudioCommands);
            short[] control = withoutSwoosh.RenderFrame(frame.AudioCommands.Where(command => !IsSwoosh(command)).ToArray());
            for (int sample = 0; sample < pcm.Length; sample++)
                if (pcm[sample] != control[sample]) differentSamples++;
        }
        AssertEqual(1, writes, "full captured frontend emits one file acceptance sound");
        AssertTrue(differentSamples > 0, "selection changes PCM even with frontend music and bank uploads");
        Console.WriteLine($"Full frontend selection: {writes} requests; {differentSamples} PCM samples differ from suppressed-swoosh control.");
    }
}
