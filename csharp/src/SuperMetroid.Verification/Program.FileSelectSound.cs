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
    }
}
