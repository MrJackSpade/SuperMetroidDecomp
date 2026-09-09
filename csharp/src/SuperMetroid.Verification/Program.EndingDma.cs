using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyEndingDma()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom("Super Metroid.smc");
        var ending = new EndingCreditsState(bus, new CartridgeAudioState(), 0, 0);
        ending.Step();
        var memory = ending.CaptureRenderSnapshot().Memory;
        byte[] interleaved = RomDataReader.Decompress(bus, 0x99d17e, 0x8000);
        byte[] characters = RomDataReader.Decompress(bus, 0x98bcd6, 0x8000);
        // $8B:D4FC/D51C use mode-1 DMA to both ports; D55C then overwrites
        // only the high lane. These checks are independent of either C# renderer.
        for (int word = 0; word < 0x4000; word++)
        {
            AssertEqual(interleaved[(word * 2) % 0x4000], memory.Vram[word * 2], "ending native low-lane map DMA");
            AssertEqual(characters[word], memory.Vram[word * 2 + 1], "ending native high-lane character DMA");
        }
        Console.WriteLine("Ending escape DMA matches native low/high VRAM lanes.");
    }
}
