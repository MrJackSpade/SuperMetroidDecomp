using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>One frame a sprite sheet composes: its installed name, its parts and the characters they index.</summary>
/// <param name="Name">The frame's name on the sheet.</param>
/// <param name="Frame">The parts.</param>
/// <param name="Characters">The characters the parts index.</param>
/// <param name="Palette">The palette row of parts that take their draw's, as the frame's draw selects it; null for the sheet's.</param>
/// <param name="DrawRow">
/// The palette bits the frame's draw ORs into every part's own (#2658), as an encounter drawing its frames in its own row
/// does: each pixel is inked in its part's own row with these bits ORed in, and a draw selecting another row shows it in
/// its part's own row combined with that one instead, as Ridley's tail and wings show his hurt flash by drawing with no
/// palette bits. Zero for a frame whose draw ORs in none beyond the sheet's.
/// </param>
internal sealed partial record SheetFrameSource(string Name, SpriteFrame Frame, SpriteArtwork Characters, int? Palette = null, int DrawRow = 0)
{
    /// <summary>
    /// Whether any part of the frame inks a pixel other than colour zero over its characters (#1513): a frame that inks
    /// none shows nothing, whatever colours it would be painted in.
    /// </summary>
    internal bool ShowsAnything()
    {
        foreach (SpriteFramePart part in Frame.Parts)
        {
            for (int row = 0; row < part.Size.Y; row++)
            {
                for (int column = 0; column < part.Size.X; column++)
                {
                    int character = part.CharacterAt(Position16<ScreenPixels>.Of(column, row), out Position16<CharacterPixels> pixel);
                    if (Characters.ColorIndex(character, pixel) != 0)
                        return true;
                }
            }
        }
        return false;
    }
}
