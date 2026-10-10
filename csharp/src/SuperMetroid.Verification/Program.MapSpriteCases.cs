using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyMapSpriteCompositionCases(ISnesAddressSpace rom)
    {
        var files = MapSpriteExtractor.Extract(rom);
        byte[] png = files[MapSpriteFormat.PngFile];
        var document = JsonSerializer.Deserialize<MapSpriteDocument>(files[MapSpriteFormat.JsonFile], MapPresentationFormat.JsonOptions)!;
        var stock = MapSpriteCatalog.Load(new MemoryStream(files[MapSpriteFormat.JsonFile]), new MemoryStream(png));
        Suite(nameof(VerifyMapSpriteNativeCompositions), () => VerifyMapSpriteNativeCompositions(rom, stock));
        foreach (var role in MapSpriteRoleOracle())
        {
            AssertTrue(!stock.StoresComposition((MapSpriteId)role.NativeId), "regular stock map composition has no stored parts");
            var original = document.Frames[role.Name];
            bool worldLabel = role.NativeId is >= 0x39 and <= 0x3e;
            if (worldLabel)
                AssertEqual(role.NativeId switch { 0x39 => 3, 0x3a => 4, 0x3b => 2, 0x3c => 3, 0x3d => 3, 0x3e => 5, _ => 0 },
                    stock.StoredLabelHorizontalCount((MapSpriteId)role.NativeId), "only line origins and nonstandard advances remain stored");
            else AssertEqual(original.Length, MapMarkerGeometry.PartCount((MapSpriteId)role.NativeId), "original marker part count");
            for (int index = 0; index < original.Length; index++)
            {
                var part = original[index];
                foreach (var change in new[] { part with { OffsetX = 17 }, part with { OffsetX = -256 }, part with { OffsetX = 255 }, part with { OffsetY = -23 },
                    part with { TileColumn = 0 }, part with { TileRow = 0 }, part with { Size = 16, TileColumn = Math.Min(part.TileColumn, 14) },
                    part with { Priority = 0 }, part with { Palette = 3 },
                    part with { FlipX = !part.FlipX }, part with { FlipY = !part.FlipY } }.Where(change => change != part))
                {
                    var parts = (SpriteVisualPart[])original.Clone(); parts[index] = change;
                    var frames = new Dictionary<string, SpriteVisualPart[]>(document.Frames); frames[role.Name] = parts;
                    var edited = Load(frames);
                    bool horizontalOnly = (change with { OffsetX = part.OffsetX }) == part;
                    AssertEqual(!(worldLabel && horizontalOnly), edited.StoresComposition((MapSpriteId)role.NativeId),
                        "horizontal label edits remain positions; other authored edits retain their composition");
                    var expected = new OamBuffer(); var actual = new OamBuffer(); expected.BeginFrame(); actual.BeginFrame();
                    MenuSpriteCompiler.Compile(parts, role.Name).DrawOnScreen(expected, 100, 100, 0x600);
                    edited.Draw((MapSpriteId)role.NativeId, actual, 100, 100, 0x600);
                    expected.FinalizeFrame(); actual.FinalizeFrame();
                    AssertTrue(expected.LowTable.SequenceEqual(actual.LowTable) && expected.HighTable.SequenceEqual(actual.HighTable),
                        "every marker part field preserves independently authored edits");
                }
            }
        }
        foreach (var changed in MapSpriteRoleOracle())
        {
            var frames = new Dictionary<string, SpriteVisualPart[]>(document.Frames); frames[changed.Name] = [];
            var edited = Load(frames);
            foreach (var frame in MapSpriteRoleOracle())
            {
                var expected = new OamBuffer(); var actual = new OamBuffer(); expected.BeginFrame(); actual.BeginFrame();
                if (frame.NativeId != changed.NativeId)
                {
                    int pointer = 0x820000 | ReadVerificationWord(rom, 0x82c569 + frame.NativeId * 2);
                    DrawImportedSpritemap(rom, expected, pointer, 100, 100, 0x600);
                }
                edited.Draw((MapSpriteId)frame.NativeId, actual, 100, 100, 0x600);
                expected.FinalizeFrame(); actual.FinalizeFrame();
                AssertTrue(expected.LowTable.SequenceEqual(actual.LowTable) && expected.HighTable.SequenceEqual(actual.HighTable),
                    "independently edited map sprite role and all unchanged roles");
            }
            frames.Remove(changed.Name);
            AssertThrows<InvalidDataException>(() => Load(frames), "each map sprite role remains required");
        }
        foreach (ushort invalid in new ushort[] { 0, 3, 8, 12, 0x37, 0x3f, 0x58, 0x5e, 0x64, ushort.MaxValue })
            AssertThrows<InvalidOperationException>(() => stock.Draw((MapSpriteId)invalid, new OamBuffer(), 0, 0, 0), "undefined map sprite identity fails");
        AssertThrows<InvalidOperationException>(() => stock.Draw((MapSpriteId)0, new OamBuffer(), 0, 0, 0xffff), "undefined identity precedes invalid palette");
        MapSpriteCatalog Load(Dictionary<string, SpriteVisualPart[]> frames)
        {
            byte[] json = JsonSerializer.SerializeToUtf8Bytes(document with { Frames = frames }, MapPresentationFormat.JsonOptions);
            return MapSpriteCatalog.Load(new MemoryStream(json), new MemoryStream(png));
        }
    }

    private static void VerifyMapSpriteNativeCompositions(ISnesAddressSpace bus, MapSpriteCatalog catalog)
    {
        var native = new OamBuffer(); var installed = new OamBuffer();
        foreach (var frame in MapSpriteRoleOracle())
        {
            int pointer = FileSelectMapRomData.MenuObjectBank | RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus),
                MenuPpuState.SpritemapPointerTableAddress + frame.NativeId * 2);
            foreach (ushort x in new ushort[] { 0, 1, 127, 255, 256, 511, 65535 })
            foreach (ushort y in new ushort[] { 0, 1, 127, 128, 223, 224, 255, 65535 })
            for (int palette = 0; palette < 8; palette++)
            foreach (int occupied in new[] { 0, 127, 128 })
            {
                native.BeginFrame(); installed.BeginFrame();
                for (int index = 0; index < occupied; index++)
                {
                    native.AddRawSmallSprite(12, 34, 56);
                    installed.AddRawSmallSprite(12, 34, 56);
                }
                DrawImportedSpritemap(bus, native, pointer, x, y, (ushort)(palette << 9));
                catalog.Draw((MapSpriteId)frame.NativeId, installed, x, y, (ushort)(palette << 9));
                AssertEqual(native.NextByteOffset, installed.NextByteOffset, "map sprite count/capacity matches native");
                native.FinalizeFrame(); installed.FinalizeFrame();
                AssertTrue(native.LowTable.SequenceEqual(installed.LowTable) && native.HighTable.SequenceEqual(installed.HighTable),
                    $"{frame.Name} preserves native order, coordinates, attributes and clipping");
            }
        }
    }
}
