using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;

internal static partial class Program
{
    private static void VerifyPpuMemorySnapshotOwnership()
    {
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        var oam = new OamBuffer();
        byte[] characters = new byte[SnesVram.ByteCount];
        new Random(321).NextBytes(characters);
        vram.LoadBytes(0, characters);
        for (int index = 0; index < SnesCgram.ColorCount; index++)
            cgram.SetColor(index, (ushort)(index * 97));
        oam.BeginFrame();
        oam.FinalizeFrame();
        byte[] expectedOam = oam.CreateUploadPayload();
        ushort[] expectedPalette = cgram.Colors.ToArray();

        PpuMemorySnapshot first = PpuMemorySnapshot.Capture(vram, cgram, oam);
        PpuMemorySnapshot repeated = PpuMemorySnapshot.Capture(vram, cgram, oam);
        AssertTrue(first.Vram.SequenceEqual(characters), "VRAM capture changed byte order");
        AssertTrue(first.Cgram.SequenceEqual(expectedPalette), "CGRAM capture changed native words");
        AssertTrue(first.Oam.SequenceEqual(expectedOam), "OAM capture changed DMA layout");
        AssertTrue(repeated.Vram.SequenceEqual(first.Vram) && repeated.Cgram.SequenceEqual(first.Cgram)
            && repeated.Oam.SequenceEqual(first.Oam), "repeat capture changed the image");
        AssertTrue(vram.Bytes.SequenceEqual(characters) && cgram.Colors.SequenceEqual(expectedPalette)
            && oam.CreateUploadPayload().AsSpan().SequenceEqual(expectedOam), "capture mutated its source");

        vram.LoadBytes(0, new byte[SnesVram.ByteCount]);
        cgram.SetColor(1, 0);
        oam.BeginFrame();
        AssertTrue(first.Vram.SequenceEqual(characters) && first.Cgram.SequenceEqual(expectedPalette)
            && first.Oam.SequenceEqual(expectedOam), "later simulation writes changed a published image");

        var fromFixture = new PpuMemorySnapshot(characters, expectedPalette, expectedOam);
        characters[0] ^= byte.MaxValue;
        expectedPalette[0] ^= ushort.MaxValue;
        expectedOam[0] ^= byte.MaxValue;
        AssertTrue(fromFixture.Vram.SequenceEqual(first.Vram) && fromFixture.Cgram.SequenceEqual(first.Cgram)
            && fromFixture.Oam.SequenceEqual(first.Oam), "fixture arrays escaped into snapshot ownership");

        AssertThrows<ArgumentException>(() => new PpuMemorySnapshot(new byte[1], expectedPalette, expectedOam), "short VRAM");
        AssertThrows<ArgumentException>(() => new PpuMemorySnapshot(characters, new ushort[1], expectedOam), "short CGRAM");
        AssertThrows<ArgumentException>(() => new PpuMemorySnapshot(characters, expectedPalette, new byte[1]), "short OAM");
        Console.WriteLine("PPU memory snapshot: byte parity, ownership, observation and size checks passed.");

    }
}
