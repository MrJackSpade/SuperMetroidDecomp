using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// The pre-coloured sheets Samus's body draws from (#1392): each spritemap over the body characters the selected tile
/// definitions make visible, named by the spritemap and the definition records whose characters it reads. Every
/// spritemap reads either the top half's characters (0-7 and 16-23) or the bottom half's (8-15 and 24-31), so a frame
/// is named by the one record it shows; a spritemap reading neither shows no record.
/// </summary>
public static class SamusBodySheetDefinitions
{
    /// <summary>The body's characters; any beyond them come from the standard OBJ sheet.</summary>
    private const int BodyCharacterCount = 32;

    /// <summary>The character bit set in the bottom half's characters.</summary>
    private const int BottomHalfBit = 8;

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
                    if ((character & BottomHalfBit) == 0)
                        top = true;
                    else
                        bottom = true;
                }
            }
        }
        return new SamusBodyFrameKey(spritemap, top ? topRecord : null, bottom ? bottomRecord : null);
    }

    /// <summary>The sheet that holds a frame: the bottom sheet for frames reading only the bottom half, the top sheet otherwise.</summary>
    public static SpriteSheetId Sheet(SamusBodyFrameKey key) =>
        key.TopRecord is null && key.BottomRecord is not null ? SpriteSheetId.SamusBodyBottom : SpriteSheetId.SamusBodyTop;
}
