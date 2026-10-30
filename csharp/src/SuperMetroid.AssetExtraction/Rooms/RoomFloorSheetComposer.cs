using SuperMetroid.AssetExtraction.Areas.Ceres;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// Pictures each Ceres tileset's turning floor (#1392, #1467): the whole floor a room of the tileset starts from and every
/// floor animation frame drawn over it. Its tiles' pixels are the high bytes of the VRAM words the room's character uploads
/// fill, and its 128 by 128 cells the low bytes of the first 16384 of those words (#1422); they are coloured with the
/// tileset's standard palette and, where a pixel names a sprite colour, the sprite half as a room load leaves it: the
/// initial gameplay palette with the common sprite and enemy-projectile rows restored.
/// </summary>
internal static class RoomFloorSheetComposer
{
    /// <summary>Composes <paramref name="tileset"/>'s floor sheet over the room as it has loaded.</summary>
    /// <exception cref="InvalidDataException">The installed palettes are invalid, or the floor's pictures overlap.</exception>
    internal static ComposedSpriteSheet Compose(RoomBlockSheetInputs inputs, TilesetDefinition tileset)
    {
        Ensure.NotNull(inputs);
        // The room tile characters' bytes as the room's load leaves them, a later upload over an earlier.
        var words = new byte[BgCharacterSet.Tiles.CharacterCount() * BgCharacterSet.Tiles.CharacterBytes()];
        foreach (BgCharacterUpload upload in RoomBlockUploads.LoadUploads(inputs, tileset).Where(upload => upload.Set == BgCharacterSet.Tiles))
            upload.Bytes.Span.CopyTo(words.AsSpan(upload.FirstCharacter * BgCharacterSet.Tiles.CharacterBytes()));
        RgbColor[] background = RoomStaticPaletteImage.Colours(inputs.Palettes.Get(tileset.Palette));
        // The sprite half as a room load leaves it: gameplay starts from the initial palette, and each room load restores
        // the common sprite and enemy-projectile rows; an actor's own row shows its live colours over these.
        var loaded = new ScreenPalette();
        BackgroundColourSetImage.LoadStarting(inputs.Objects.Palettes, loaded);
        inputs.Objects.Palettes.LoadCommonSprites(SpriteColourSetDefinitions.Common.In(loaded.SpriteSets));
        inputs.Objects.Palettes.LoadEnemyProjectileSprites(SpriteColourSetDefinitions.EnemyProjectiles.In(loaded.SpriteSets));
        var colours = new RgbColor[SpriteImage.ColourCountOf(SpriteImageColours.FullPalette)];
        ScreenColourImage.Encode(loaded).CopyTo(colours, 0);
        background.CopyTo(colours, 0);
        // Each VRAM word holds a floor cell in its low byte and a tile pixel in its high byte.
        var map = new byte[FloorSheetComposer.CellCount];
        var pixels = new byte[FloorSheetComposer.TileSetBytes];
        for (int word = 0; word < map.Length; word++)
        {
            map[word] = words[word * sizeof(ushort)];
            pixels[word] = words[word * sizeof(ushort) + 1];
        }
        return FloorSheetComposer.Compose(
            [FloorSheetComposer.Whole(RoomFloorSheet.FloorFrameName, map, pixels), .. CeresFloorDrawings.Pictures(map, pixels)],
            colours, roomColours: true);
    }
}
