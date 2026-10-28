using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Tooling;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Frontend.Map.MapSpriteNamesVerification;
using static SuperMetroid.Verification.Frontend.Map.MapSpritesVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.Menus.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.Core.Rendering;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.Verification.Frontend.Map;

/// <summary>Checks calculated map sprites and independent edits to every authored OAM part.</summary>
internal static class MapSpriteCasesVerification
{
    /// <summary>Compares stock and edited compositions while requiring every named map role.</summary>
    internal static void VerifyMapSpriteCompositionCases(ISnesAddressSpace rom)
    {
        var files = MapSpriteExtractor.Extract(rom);
        byte[] png = files[MapSpriteFormat.PngFile];
        var document = RequiredJson.Deserialize<MapSpriteDocument>(files[MapSpriteFormat.JsonFile], ArtworkJson.Options);
        var stock = MapSpriteArtwork.Read(new MemoryStream(files[MapSpriteFormat.JsonFile]), new MemoryStream(png));
        Suite(nameof(VerifyMapSpriteNativeCompositions), () => VerifyMapSpriteNativeCompositions(rom, stock));
        foreach (var role in MapSpriteRoleOracle())
        {
            AssertTrue(!stock.StoresComposition(MapSpriteOrdinals.Spritemaps.Decode(role.NativeId, "map spritemap ordinal")), "regular stock map composition has no stored parts");
            var original = document.Frames[role.Name];
            bool worldLabel = role.NativeId is >= 0x39 and <= 0x3e;
            if (worldLabel)
                AssertEqual(role.NativeId switch { 0x39 => 3, 0x3a => 4, 0x3b => 2, 0x3c => 3, 0x3d => 3, 0x3e => 5, _ => 0 },
                    stock.StoredLabelHorizontalCount(MapSpriteOrdinals.Spritemaps.Decode(role.NativeId, "map spritemap ordinal")), "only line origins and nonstandard advances remain stored");
            else AssertEqual(original.Length, MapMarkerGeometry.PartCount(MapSpriteOrdinals.Spritemaps.Decode(role.NativeId, "map spritemap ordinal")), "original marker part count");
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
                    AssertEqual(!(worldLabel && horizontalOnly), edited.StoresComposition(MapSpriteOrdinals.Spritemaps.Decode(role.NativeId, "map spritemap ordinal")),
                        "horizontal label edits remain positions; other authored edits retain their composition");
                    var expected = new OamBuffer(); var actual = ScreenSpritesFixtures.Blank(); expected.BeginFrame(); actual.BeginFrame();
                    MenuSpriteCompiler.Compile(parts, role.Name).DrawOnScreen(expected, new(100, 100), 0x600);
                    MapSpriteSheet.Draw(MenuSheetFixtures.MapSprites(edited), MapSpriteOrdinals.Spritemaps.Decode(role.NativeId, "map spritemap ordinal"), actual, new(100, 100), 3);
                    expected.FinalizeFrame(); actual.FinalizeFrame();
                    AssertTrue(expected.ImageEquals(actual),
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
                var expected = new OamBuffer(); var actual = ScreenSpritesFixtures.Blank(); expected.BeginFrame(); actual.BeginFrame();
                if (frame.NativeId != changed.NativeId)
                {
                    int pointer = 0x820000 | ReadVerificationWord(rom, 0x82c569 + frame.NativeId * 2);
                    DrawImportedSpritemap(rom, expected, pointer, new(100, 100), 0x600);
                }
                MapSpriteSheet.Draw(MenuSheetFixtures.MapSprites(edited), MapSpriteOrdinals.Spritemaps.Decode(frame.NativeId, "map spritemap ordinal"), actual, new(100, 100), 3);
                expected.FinalizeFrame(); actual.FinalizeFrame();
                AssertTrue(expected.ImageEquals(actual),
                    "independently edited map sprite role and all unchanged roles");
            }
            frames.Remove(changed.Name);
            AssertThrows<InvalidDataException>(() => Load(frames), "each map sprite role remains required");
        }
        foreach (ushort invalid in new ushort[] { 0, MapSpriteDefinitions.Count + 1, ushort.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => MapSpriteSheet.Draw(MenuSheetFixtures.MapSprites(stock), (MapSpriteId)invalid,
                ScreenSpritesFixtures.Blank(), new(0, 0), 0), "undefined map sprite identity fails");
        AssertThrows<ArgumentOutOfRangeException>(() => MapSpriteSheet.Draw(MenuSheetFixtures.MapSprites(stock), 0, ScreenSpritesFixtures.Blank(),
            new(0, 0), 0xffff), "undefined identity precedes invalid palette");
        MapSpriteCatalog Load(Dictionary<string, SpriteVisualPart[]> frames)
        {
            byte[] json = JsonSerializer.SerializeToUtf8Bytes(document with { Frames = frames }, ArtworkJson.Options);
            return MapSpriteArtwork.Read(new MemoryStream(json), new MemoryStream(png));
        }
    }

    /// <summary>Compares native and installed OAM across anchors, palettes, and nearly full sprite buffers.</summary>
    internal static void VerifyMapSpriteNativeCompositions(ISnesAddressSpace bus, MapSpriteCatalog catalog)
    {
        OamBuffer native; var installed = ScreenSpritesFixtures.Blank();
        foreach (var frame in MapSpriteRoleOracle())
        {
            int pointer = MapSourceDefinitions.Menu.ObjectBank | RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus),
                MapSourceDefinitions.Menu.SpritemapPointers + frame.NativeId * 2);
            foreach (ushort x in new ushort[] { 0, 1, 127, 255, 256, 511, 65535 })
            foreach (ushort y in new ushort[] { 0, 1, 127, 128, 223, 224, 255, 65535 })
            for (int palette = 0; palette < 8; palette++)
            foreach (int occupied in new[] { 0, 127, 128 })
            {
                native = new OamBuffer(); native.BeginFrame(); installed.BeginFrame();
                for (int index = 0; index < occupied; index++)
                {
                    native.AddOccupyingPart(new Position16<SpritePixels>(12, 34), 56);
                    installed.AddOccupyingPart(new Position16<SpritePixels>(12, 34), 56);
                }
                DrawImportedSpritemap(bus, native, pointer, new(x, y), (ushort)(palette << 9));
                MapSpriteSheet.Draw(MenuSheetFixtures.MapSprites(catalog), MapSpriteOrdinals.Spritemaps.Decode(frame.NativeId, "map spritemap ordinal"), installed, new(x, y), palette);
                native.FinalizeFrame(); installed.FinalizeFrame();
                AssertEqual(native.LastFinalizedSpriteCount, installed.LastFinalizedSpriteCount, "map sprite count/capacity matches native");
                AssertTrue(native.ImageEquals(installed),
                    $"{frame.Name} preserves native order, coordinates, attributes and clipping " +
                    $"(at {x},{y} palette {palette} with {occupied} objects placed; native {native.FirstDifference(ScreenSpritesFixtures.ToOam(installed))})");
            }
        }
    }
}
