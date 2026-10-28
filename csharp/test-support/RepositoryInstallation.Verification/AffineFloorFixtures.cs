using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>
/// Turns a fixture's memory image into a Mode 7 floor (#1403): its 256 characters, the high byte of each word, become
/// pre-coloured tiles composed under the fixture's palette as extraction composes installed Mode 7 artwork, and its map,
/// the low byte of each word, becomes the floor's cells.
/// </summary>
internal static class AffineFloorFixtures
{
    /// <summary>The floor <paramref name="image"/>'s map and characters show, coloured from <paramref name="palette"/>.</summary>
    internal static AffineFloor Floor(ReadOnlySpan<byte> image, ReadOnlySpan<RgbColor> palette)
    {
        var pixels = new byte[FloorTilePictures.Tiles * FloorCharacterSheet.TileBytes];
        for (int pixel = 0; pixel < pixels.Length; pixel++)
            pixels[pixel] = image[pixel * sizeof(ushort) + 1];
        ComposedSpriteSheet composed = ScreenTileSheetComposer.ComposeFloorPixels(pixels, palette);
        SpriteSheet sheet = SpriteSheet.Load(new MemoryStream(SpriteSheet.Write(composed.Manifest), writable: false), "mode7-fixture",
            composed.Size, composed.Pixels, composed.PaletteIndices, composed.Priorities, composed.PartRows);
        FloorTilePictures tiles = new FloorCharacters()
            .Show(new FloorCharacterSheet(SpriteSheetId.TitleFloorTiles, sheet), 0, 0, FloorTilePictures.Tiles).Pictures();
        return AffineFloorConstants.Capture(image, tiles);
    }
}
