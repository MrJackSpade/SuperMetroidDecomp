using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// Pre-colours each Ceres tileset's Mode 7 tiles (#1392): the 256 tiles whose pixels are the high bytes of the VRAM words
/// the room's character uploads fill, coloured with the tileset's standard palette and, where a pixel names a sprite
/// colour, the sprite half as a room load leaves it: the initial gameplay palette with the common sprite and
/// enemy-projectile rows restored.
/// </summary>
internal static class Mode7TileSheetComposer
{
    /// <summary>Composes <paramref name="tileset"/>'s tiles over <paramref name="content"/>, the room as it has loaded.</summary>
    /// <exception cref="InvalidDataException">The installed palettes are invalid.</exception>
    [PpuLayoutBoundary("codec: Mode 7 characters' pixels in the high bytes of the room tile characters' words")]
    internal static ComposedMode7Tiles Compose(RoomBlockSheetInputs inputs, TilesetDefinition tileset, BgArtworkBindings content)
    {
        Ensure.NotNull(inputs);
        Ensure.NotNull(content);
        // The room tile characters' bytes as their bindings leave them, a later binding over an earlier.
        var tiles = new byte[BgCharacterSet.Tiles.CharacterCount() * BgCharacterSet.Tiles.CharacterBytes()];
        foreach (BgArtworkBinding binding in content.Bound.Where(binding => binding.Set == BgCharacterSet.Tiles))
            binding.Bytes.Span.CopyTo(tiles.AsSpan(binding.FirstCharacter * BgCharacterSet.Tiles.CharacterBytes()));
        Bgr555[] background = RoomTileCanvas.StandardColours(inputs.Palettes.Get(tileset.Palette).PackedColours.Span);
        // The sprite half as a room load leaves it: gameplay starts from the initial palette, and each room load restores
        // the common sprite and enemy-projectile rows; an actor's own row shows its live colours over these.
        var loaded = new ScreenPalette();
        inputs.Objects.Palettes.LoadInitial(loaded);
        inputs.Objects.Palettes.LoadCommonSprites(loaded.SpriteRow(SpriteColourSetDefinitions.Common));
        inputs.Objects.Palettes.LoadEnemyProjectileSprites(loaded.SpriteRow(SpriteColourSetDefinitions.EnemyProjectiles));
        var colours = new Bgr555[SpriteImage.ColourCountOf(SpriteImageColours.FullPalette)];
        loaded.Colors.CopyTo(colours);
        background.CopyTo(colours, 0);
        SpritePaletteRows coloured = SpritePaletteRows.None;
        int side = Mode7TileSheet.TilesPerRow * Mode7TileSheet.TilePixels;
        var canvas = new RoomTileCanvas(new Extent16<ImagePixels>(checked((ushort)(side)), checked((ushort)(side))));
        var shown = new BgArtworkShown?[Mode7TileSheet.Tiles];
        for (int tile = 0; tile < Mode7TileSheet.Tiles; tile++)
        {
            // A tile whose characters nothing binds is not pictured; the map may not name it.
            if (!content.TryShowing(BgCharacterSet.Tiles, tile * Mode7TileSheet.TileCharacters, Mode7TileSheet.TileCharacters, out BgArtworkShown bound))
                continue;
            shown[tile] = bound;
            Position16<ImagePixels> corner = Mode7TileSheet.Corner(tile);
            for (int pixel = 0; pixel < Mode7TileSheet.TileWords; pixel++)
            {
                byte index = tiles[(tile * Mode7TileSheet.TileWords + pixel) * 2 + 1];
                if (index % 16 != 0 && index < SpriteImage.SpriteColourCount)
                    coloured |= SpritePaletteRowSet.Rows[index / 16];
                canvas.PaintIndex(new(checked((ushort)(corner.X + pixel % Mode7TileSheet.TilePixels)), checked((ushort)(corner.Y + pixel / Mode7TileSheet.TilePixels))), index, colours);
            }
        }
        // The frame's rows name the background rows it is coloured from; its sprite rows are the room load's own.
        var frames = new Dictionary<string, SpriteSheetFrameDocument>(StringComparer.Ordinal)
        {
            [Mode7TileSheet.FrameName] = new SpriteSheetFrameDocument
            {
                Corner = default,
                Size = new Extent16<ImagePixels>((ushort)side, (ushort)side),
                Origin = default,
                Rows = coloured,
                Parts = [],
            },
        };
        return new ComposedMode7Tiles(tileset, canvas.ToSheet(colours, frames, SpriteImageColours.FullPalette), shown);
    }
}
