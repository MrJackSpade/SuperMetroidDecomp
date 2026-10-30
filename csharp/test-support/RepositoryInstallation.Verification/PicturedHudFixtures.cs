using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Hud.Artwork;

namespace SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>
/// Turns a fixture's memory image into a HUD the HUD layer draws (#1466): the four rows the gameplay BG3 tilemap holds at its
/// top, painted from their two-bit characters as extraction paints the HUD's images, as one picture placed over the whole
/// HUD.
/// </summary>
internal static class PicturedHudFixtures
{
    /// <summary>
    /// The HUD the gameplay BG3 tilemap's top rows in <paramref name="image"/> show, their words' characters read from
    /// <paramref name="characterWord"/> and coloured from <paramref name="palette"/>.
    /// </summary>
    internal static PicturedHud FromVram(ReadOnlySpan<byte> image, ReadOnlySpan<RgbColor> palette, int characterWord = GameplayVramLayout.HudCharacterWord)
    {
        const int tile = GameplayHudDefinitions.CellPixels, characterWords = 8;
        const int columns = GameplayHudDefinitions.Width, rows = GameplayHudDefinitions.Rows;
        var canvas = new RoomTileCanvas(new Extent16<ImagePixels>(columns * tile, rows * tile));
        RgbColor[] colours = palette[..SpriteImage.SpriteColourCount].ToArray();
        var character = new byte[characterWords * sizeof(ushort)];
        for (int cell = 0; cell < columns * rows; cell++)
        {
            int at = SnesVramAddressingConstants.ByteOffsetOfWord(GameplayVramLayoutConstants.HudTilemapWord + cell);
            var word = new SnesBgTilemapWord(LittleEndianBytes.Word(image[at], image[at + 1]));
            for (int part = 0; part < characterWords; part++)
            {
                int source = SnesVramAddressingConstants.ByteOffsetOfWord(characterWord + word.CharacterIndex * characterWords + part);
                character[part * 2] = image[source];
                character[part * 2 + 1] = image[source + 1];
            }
            canvas.PaintTwoBppTile(cell % columns * tile, cell / columns * tile, word, character, colours);
        }
        var frames = new Dictionary<string, SpriteSheetFrameDocument>(StringComparer.Ordinal)
        {
            ["hud"] = WrittenPictureSheetComposer.Frame(canvas, 0, rows, WrittenPictureLayer.Bg3),
        };
        ComposedSpriteSheet composed = canvas.ToRoomSheet(colours, frames, SpriteImageColours.TwoBppBackground);
        SpriteSheet sheet = SpriteSheet.Load(new MemoryStream(SpriteSheet.Write(composed.Manifest), writable: false), "hud-fixture",
            composed.Size, composed.Pixels, composed.PaletteIndices, composed.Priorities, composed.PartGroups);
        return new PicturedHud([new PlacedHudPicture(sheet.Frame("hud"), new Position16<TilemapCells>(0, 0))]);
    }
}
