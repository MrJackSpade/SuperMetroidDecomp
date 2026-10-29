using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// Pre-colours each Ceres tileset's Mode 7 tiles (#1392): the 256 tiles whose pixels are the high bytes of the VRAM words
/// the room's character uploads fill, coloured with the tileset's standard palette and, where a pixel names a sprite
/// colour, the sprite half as a room load leaves it: the initial gameplay palette with the common sprite and
/// enemy-projectile rows restored. It also imports the floor map a room of the tileset starts from (#1422): the 128 by
/// 128 tile numbers are the low bytes of the first 16384 of those words.
/// </summary>
internal static class Mode7TileSheetComposer
{
    /// <summary>The room tile characters whose bytes one floor tile's pixels share.</summary>
    private const int TileCharacters = 4;

    /// <summary>Composes <paramref name="tileset"/>'s tiles over <paramref name="content"/>, the room as it has loaded.</summary>
    /// <exception cref="InvalidDataException">The installed palettes are invalid.</exception>
    internal static ComposedMode7Tiles Compose(RoomBlockSheetInputs inputs, TilesetDefinition tileset, BgArtworkBindings content)
    {
        Ensure.NotNull(inputs);
        Ensure.NotNull(content);
        // The room tile characters' bytes as the room's load leaves them, a later upload over an earlier.
        var tiles = new byte[BgCharacterSet.Tiles.CharacterCount() * BgCharacterSet.Tiles.CharacterBytes()];
        foreach (BgCharacterUpload upload in RoomBlockUploads.LoadUploads(inputs, tileset).Where(upload => upload.Set == BgCharacterSet.Tiles))
            upload.Bytes.Span.CopyTo(tiles.AsSpan(upload.FirstCharacter * BgCharacterSet.Tiles.CharacterBytes()));
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
        ImageColourGroups coloured = ImageColourGroups.None;
        int side = FloorTileSheet.TilesPerRow * FloorTileSheet.TilePixels;
        var canvas = new RoomTileCanvas(new Extent16<ImagePixels>(checked((ushort)(side)), checked((ushort)(side))));
        var shown = new BgArtworkShown?[FloorTileSheet.Tiles];
        for (int tile = 0; tile < FloorTileSheet.Tiles; tile++)
        {
            // A tile whose characters nothing binds is not pictured; the map may not name it.
            if (!content.TryShowing(BgCharacterSet.Tiles, tile * TileCharacters, TileCharacters, out BgArtworkShown bound))
                continue;
            shown[tile] = bound;
            Position16<ImagePixels> corner = FloorTileSheet.Corner(tile);
            for (int pixel = 0; pixel < FloorTileSheet.TileWords; pixel++)
            {
                int index = tiles[(tile * FloorTileSheet.TileWords + pixel) * 2 + 1];
                if (index % 16 != 0 && index < SpriteImage.SpriteColourCount)
                    coloured = coloured.With(ImageColourGroup.OfInk(index));
                canvas.PaintIndex(new(checked((ushort)(corner.X + pixel % FloorTileSheet.TilePixels)), checked((ushort)(corner.Y + pixel / FloorTileSheet.TilePixels))), index, colours);
            }
        }
        // The frame's colour groups name the background groups it is coloured from; its sprite groups are the room load's own.
        var frames = new Dictionary<string, SpriteSheetFrameDocument>(StringComparer.Ordinal)
        {
            [FloorTileSheet.FrameName] = new SpriteSheetFrameDocument
            {
                Corner = default,
                Size = new Extent16<ImagePixels>((ushort)side, (ushort)side),
                Origin = default,
                ColourGroups = coloured.ManifestMembers,
                Parts = [],
            },
        };
        var map = new byte[FloorMap.Cells];
        for (int cell = 0; cell < map.Length; cell++)
            map[cell] = tiles[cell * 2];
        return new ComposedMode7Tiles(tileset, canvas.ToRoomSheet(colours, frames, SpriteImageColours.FullPalette), shown, map);
    }
}
