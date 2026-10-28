using SuperMetroid.Core.Assets;
using static SuperMetroid.Core.Samus.Artwork.SamusBodySheetDefinitions;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Which installed sheet each body half and set draws from.</summary>
public static class SamusBodySheets
{
    /// <summary>
    /// The frame a body draw shows: the spritemap, with the selected records of the halves whose characters it reads.
    /// </summary>
    /// <param name="spritemap">The spritemap's composition.</param>
    /// <param name="frame">The composition as a whole frame, whose parts' characters are OBJ tile numbers.</param>
    /// <param name="topRecord">The selected top definition record, or null when none is selected.</param>
    /// <param name="bottomRecord">The selected bottom definition record, or null when none is selected.</param>
    public static SamusBodyFrameKey Key(SamusSpritemapId spritemap, SpriteFrame frame, int? topRecord, int? bottomRecord)
    {
        Ensure.NotNull(frame);
        bool top = false, bottom = false;
        foreach (SpriteFramePart part in frame.Parts)
        {
            int columns = part.Size.X / SpriteArtwork.CharacterPixels, rows = part.Size.Y / SpriteArtwork.CharacterPixels;
            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    int character = part.Character + row * SpriteArtwork.CharactersPerRow + column;
                    if (character >= BodyCharacterCount)
                        continue;
                    if (Column(character) < TopHalfColumns)
                        top = true;
                    else
                        bottom = true;
                }
            }
        }
        return new SamusBodyFrameKey(spritemap, top ? topRecord : null, bottom ? bottomRecord : null);
    }

    /// <summary>The column of <paramref name="character"/> within its sixteen-wide character row, 0 through 15.</summary>
    private static int Column(int character)
    {
        int remainder = character % SpriteArtwork.CharactersPerRow;
        return remainder < 0 ? remainder + SpriteArtwork.CharactersPerRow : remainder;
    }

    /// <summary>The sheet that holds a frame: the bottom sheet for frames reading only the bottom half, the top sheet otherwise.</summary>
    public static SpriteSheetId Sheet(SamusBodyFrameKey key) =>
        key.TopRecord is null && key.BottomRecord is not null ? SpriteSheetId.SamusBodyBottom : SpriteSheetId.SamusBodyTop;
}
