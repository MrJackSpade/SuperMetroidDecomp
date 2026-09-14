using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyProjectileTrailAtlas(ISnesAddressSpace bus)
    {
        byte[] png = ProjectileTrailAtlasExtractor.Extract(bus);
        var atlas = ProjectileTrailAtlas.Load(new MemoryStream(png));
        var runtime = new SuperMetroid.Core.Runtime.SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom(); runtime.InitializeCeresStartSamus();
        runtime.RunNmi(0, true);
        byte[] native = runtime.Vram.Bytes.ToArray();
        atlas.LoadTo(runtime.Vram);
        AssertTrue(native.AsSpan().SequenceEqual(runtime.Vram.Bytes), "Stock trail PNG preserves complete VRAM after production room/NMI setup");
        for (int tile = 0; tile < 12; tile++)
        {
            var image = IndexedPng.Read(new MemoryStream(png), ProjectileTrailAtlasDefinitions.Width, ProjectileTrailAtlasDefinitions.Height);
            image.Pixels[tile * 8] ^= 1;
            using var changed = new MemoryStream();
            IndexedPng.Write(changed, image.Width, image.Height, image.Pixels, image.Palette);
            changed.Position = 0;
            var edited = ProjectileTrailAtlas.Load(changed);
            var actual = new SnesVram(); actual.LoadBytes(0, native);
            edited.LoadTo(actual);
            int changedByte = tile < 8 ? ProjectileTrailAtlasDefinitions.IceWaveDestinationWord * 2 + tile * 32
                : ProjectileTrailAtlasDefinitions.MissileDestinationWord * 2 + (tile - 8) * 32;
            for (int i = 0; i < native.Length; i++)
                AssertEqual((byte)(native[i] ^ (i == changedByte ? 0x80 : 0)), actual.Bytes[i], "Trail pixel edit changes exactly its mapped bit, preserving gap and neighboring characters");
        }
        using var wrong = new MemoryStream();
        IndexedPng.Write(wrong, 8, 8, new byte[64], SnesGraphics.DiagnosticPalette(16));
        wrong.Position = 0;
        AssertThrows<InvalidDataException>(() => ProjectileTrailAtlas.Load(wrong), "Wrong trail PNG dimensions rejected");
        using var invalidIndex = new MemoryStream();
        var invalidPixels = new byte[ProjectileTrailAtlasDefinitions.Width * ProjectileTrailAtlasDefinitions.Height];
        invalidPixels[0] = 16;
        IndexedPng.Write(invalidIndex, ProjectileTrailAtlasDefinitions.Width, ProjectileTrailAtlasDefinitions.Height,
            invalidPixels, SnesGraphics.DiagnosticPalette(32));
        invalidIndex.Position = 0;
        AssertThrows<InvalidDataException>(() => ProjectileTrailAtlas.Load(invalidIndex), "Trail PNG indices exceeding four bits rejected");
        AssertThrows<InvalidDataException>(() => ProjectileTrailAtlas.Load(new MemoryStream(new byte[12])), "Malformed trail PNG rejected");
        Console.WriteLine("Trail PNG: native room/NMI parity and twelve edited pixels preserve every other VRAM byte, including the interleaved graphics gap.");
    }
}
