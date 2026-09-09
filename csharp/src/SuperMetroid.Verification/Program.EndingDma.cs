using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyEndingDma()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom("Super Metroid.smc");
        var audio = new CartridgeAudioState();
        var ending = new EndingCreditsState(bus, audio, 0, 0);
        ending.Step();
        var projection = ending.CaptureRenderSnapshot().Layers.ToArray().OfType<SuperMetroid.Core.Rendering.Mode7RenderLayer>().Single().Registers;
        AssertEqual((short)0, projection.HorizontalOffset, "native atmospheric scroll X");
        AssertEqual((short)0, projection.VerticalOffset, "native atmospheric scroll Y");
        AssertEqual((short)45, projection.MatrixA, "native initial angle 32, scale 64 cosine");
        AssertEqual((short)45, projection.MatrixB, "native initial angle 32, scale 64 sine");
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
        for (int frame = 0; frame < 20000 && ending.Phase != EndingCreditsPhase.Credits; frame++)
        {
            ending.Step();
            audio.AdvanceFrame(bus, default);
        }
        AssertEqual(EndingCreditsPhase.Credits, ending.Phase, "ending reaches credits setup");
        byte[] reward = RomDataReader.Decompress(bus, 0x97b957, 0x8000);
        AssertTrue(reward.AsSpan(0, 0x4000).SequenceEqual(ending.CaptureRenderSnapshot().Memory.Vram[..0x4000]),
            "native post-credits reward DMA writes contiguous bytes, not the Mode-7 high lane");
    }
}
