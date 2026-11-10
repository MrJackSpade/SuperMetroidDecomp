using SuperMetroid.AssetExtraction.Frontend.Map.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Tooling;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Frontend.Map.MapSpriteNamesVerification;
using static SuperMetroid.Verification.Frontend.Map.MapSpritesVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.Core.Rendering;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction;

namespace SuperMetroid.Verification.Frontend.Map;

/// <summary>Checks calculated stock map sprites and required map roles.</summary>
internal static class MapSpriteCasesVerification
{
    /// <summary>Compares stock compositions while requiring every named map role.</summary>
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
        }
        foreach (var changed in MapSpriteRoleOracle())
        {
            var frames = new Dictionary<string, SpriteVisualPart[]>(document.Frames);
            frames.Remove(changed.Name);
            AssertThrows<InvalidDataException>(() => Load(frames), "each map sprite role remains required");
        }
        foreach (ushort invalid in new ushort[] { 0, MapSpriteFormat.FrameCount + 1, ushort.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => MapSpriteSheet.Draw(MenuSheetFixtures.MapSprites(stock), (MapSpriteId)invalid,
                ScreenSpritesFixtures.Blank(), new(0, 0), MapSpriteScreen.FileSelect, MapSpriteColouring.Labels), "undefined map sprite identity fails");
        AssertThrows<ArgumentOutOfRangeException>(() => MapSpriteSheet.Draw(MenuSheetFixtures.MapSprites(stock), 0, ScreenSpritesFixtures.Blank(),
            new(0, 0), MapSpriteScreen.FileSelect, (MapSpriteColouring)ushort.MaxValue), "undefined identity precedes an undefined colouring");
        MapSpriteCompositions Load(Dictionary<string, SpriteVisualPart[]> frames)
        {
            byte[] json = JsonSerializer.SerializeToUtf8Bytes(document with { Frames = frames }, ArtworkJson.Options);
            return MapSpriteArtwork.Read(new MemoryStream(json), new MemoryStream(png));
        }
    }

    /// <summary>
    /// Compares native and installed OAM across anchors and every colouring the map draws in. The object limit that cuts a
    /// frame short in a nearly full sprite buffer is not replicated (#1513), so frames are compared in an empty one.
    /// </summary>
    internal static void VerifyMapSpriteNativeCompositions(ISnesAddressSpace bus, MapSpriteCompositions catalog)
    {
        OamBuffer native; var installed = ScreenSpritesFixtures.Blank();
        foreach (var frame in MapSpriteRoleOracle())
        {
            int pointer = MapSourceDefinitions.Menu.ObjectBank | RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus),
                MapSourceDefinitions.Menu.SpritemapPointers + frame.NativeId * 2);
            // Origins are signed screen pixels (#1587): an X of 511 is no longer the column left of the screen, and a Y of
            // 65535 is the line above it, whose parts the display clips rather than the sprite table culling them whole.
            foreach (ushort x in new ushort[] { 0, 1, 127, 255, 256, 65535 })
            foreach (ushort y in new ushort[] { 0, 1, 127, 128, 223, 224, 255 })
            foreach (PictureShading colouring in MapSpriteShadings)
            {
                int palette = MapSpriteSets[colouring];
                native = new OamBuffer(); native.BeginFrame(); installed.BeginFrame();
                DrawImportedSpritemap(bus, native, pointer, new(x, y), (ushort)(palette << 9));
                MapSpriteSheet.Draw(MenuSheetFixtures.MapSprites(catalog), MapSpriteOrdinals.Spritemaps.Decode(frame.NativeId, "map spritemap ordinal"), installed, new(x, y), colouring);
                native.FinalizeFrame(); installed.FinalizeFrame();
                AssertEqual(native.LastFinalizedSpriteCount, installed.LastFinalizedSpriteCount, "map sprite count/capacity matches native");
                AssertTrue(native.ImageEquals(installed),
                    $"{frame.Name} preserves native order, coordinates, attributes and clipping " +
                    $"(at {x},{y} palette {palette}; native {native.FirstDifference(ScreenSpritesFixtures.ToOam(installed))})");
            }
        }
    }
}
