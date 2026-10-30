using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Hud;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// Pre-assembles and pre-colours each tileset's gameplay message boxes (#1392, #1466): one image of each form the message
/// box shows (<see cref="GameplayMessageArtwork.Forms"/>), named by the form, painted from the installed messages' words
/// over the HUD's characters in the tileset's standard BG3 colours with the two colours a message box installs.
/// </summary>
internal static class MessageBoxSheetComposer
{
    /// <summary>Composes <paramref name="tileset"/>'s message box images.</summary>
    internal static ComposedSpriteSheet Compose(RoomBlockSheetInputs inputs, TilesetDefinition tileset)
    {
        Ensure.NotNull(inputs);
        GameplayMessageArtwork messages = GameplayMessageArtwork.Of(inputs.Maps);
        RgbColor[] colours = RoomStaticPaletteImage.Colours(inputs.Palettes.Get(tileset.Palette));
        int textSet = BackgroundColourSetImage.First(GameplayMessageBoxDefinitions.Palette.TextSet);
        colours[textSet + GameplayMessageBoxDefinitions.Palette.TemporaryLightColour] = GameplayMessageBoxDefinitions.Palette.TemporaryLightColor;
        colours[textSet + GameplayMessageBoxDefinitions.Palette.TemporaryDarkColour] = GameplayMessageBoxDefinitions.Palette.TemporaryDarkColor;
        int width = GameplayMessageBoxDefinitions.Layout.TilemapWidth;
        (ShownMessage Form, BackgroundTile[] Tiles)[] forms = [.. GameplayMessageArtwork.Forms().Select(form => (form, messages.Tiles(form)))];
        int rows = forms.Sum(form => form.Tiles.Length / width);
        var canvas = new RoomTileCanvas(new Extent16<ImagePixels>(checked((ushort)(width * RoomTileCanvas.CharacterPixels)), checked((ushort)(rows * RoomTileCanvas.CharacterPixels))));
        ReadOnlySpan<byte> characters = inputs.Hud.Bytes.Span;
        var frames = new Dictionary<string, SpriteSheetFrameDocument>(StringComparer.Ordinal);
        int top = 0;
        foreach ((ShownMessage form, BackgroundTile[] tiles) in forms)
        {
            for (int index = 0; index < tiles.Length; index++)
            {
                BackgroundTile tile = tiles[index];
                canvas.PaintTwoBppTile(index % width * RoomTileCanvas.CharacterPixels, (top + index / width) * RoomTileCanvas.CharacterPixels, tile,
                    characters.Slice(tile.Tile * RoomTileCanvas.TwoBppCharacterBytes, RoomTileCanvas.TwoBppCharacterBytes), colours);
            }
            int messageRows = tiles.Length / width;
            frames.Add(form.FrameName, WrittenPictureSheetComposer.Frame(canvas, top, messageRows, WrittenPictureLayer.Bg3));
            top += messageRows;
        }
        return canvas.ToRoomSheet(colours, frames, SpriteImageColours.TwoBppBackground);
    }
}
