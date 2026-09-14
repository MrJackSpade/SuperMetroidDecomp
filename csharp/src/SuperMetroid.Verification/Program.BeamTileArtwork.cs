using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyBeamTileArtwork(ISnesAddressSpace bus)
    {
        var files = BeamTileExtractor.Extract(bus);
        AssertEqual(12, files.Count, "Every legal beam combination has editable artwork");
        for (ushort selection = 0; selection < 12; selection++)
        {
            byte[] png = files[BeamTileAtlasDefinitions.FileName(selection)];
            var atlas = BeamTileAtlas.Load(new MemoryStream(png));
            var native = new SnesVram(); var extracted = new SnesVram();
            SamusProjectileSystem.LoadBeamTilesAndPalette(bus, native, new SnesCgram(), selection);
            atlas.LoadTo(extracted);
            AssertTrue(native.Bytes.SequenceEqual(extracted.Bytes), "PNG matches production beam upload across complete VRAM");
            var image = IndexedPng.Read(new MemoryStream(png), 64, 8);
            image.Pixels[0] ^= 1;
            using var editedPng = new MemoryStream();
            IndexedPng.Write(editedPng, image.Width, image.Height, image.Pixels, image.Palette);
            editedPng.Position = 0;
            var edited = BeamTileAtlas.Load(editedPng);
            edited.LoadTo(extracted);
            int firstByte = BeamTileAtlasDefinitions.DestinationWord * 2;
            AssertEqual((byte)(native.Bytes[firstByte] ^ 0x80), extracted.Bytes[firstByte], "Edited pixel changes the correct tile plane bit");
            for (int index = 0; index < native.Bytes.Length; index++)
                if (index != firstByte) AssertEqual(native.Bytes[index], extracted.Bytes[index], "Beam edit cannot change neighboring VRAM or other pixels");
            AssertEqual(BeamTileAtlasDefinitions.ByteCount, edited.Transfer.Length, "Edited upload retains native DMA size");
        }
        using var wrongSize = new MemoryStream();
        IndexedPng.Write(wrongSize, 8, 8, new byte[64], new[] { new Rgba32(0, 0, 0, 255) });
        wrongSize.Position = 0;
        AssertThrows<InvalidDataException>(() => BeamTileAtlas.Load(wrongSize), "Wrong beam atlas dimensions rejected");
        using var invalidIndex = new MemoryStream();
        var colors = Enumerable.Range(0, 17).Select(i => new Rgba32((byte)i, 0, 0, 255)).ToArray();
        var pixels = new byte[512]; pixels[0] = 16;
        IndexedPng.Write(invalidIndex, 64, 8, pixels, colors);
        invalidIndex.Position = 0;
        AssertThrows<InvalidDataException>(() => BeamTileAtlas.Load(invalidIndex), "Beam index exceeds four bit hardware palette");
        AssertThrows<InvalidDataException>(() => BeamTileAtlas.Load(new MemoryStream(new byte[8])), "Malformed beam PNG rejected");
        Console.WriteLine("Beam PNG artwork: twelve production VRAM uploads, exact edited-pixel isolation and malformed resource rejection pass.");
    }
}
