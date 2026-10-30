using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Frontend;

/// <summary>Helpers of <see cref="ScreenPictureSheetComposer"/> that only verification reads.</summary>
internal static class ScreenPictureSheetComposerConstants
{
    /// <summary>
    /// The sheet picturing <paramref name="subjects"/> over <paramref name="characters"/> under <paramref name="palette"/>,
    /// painted as <see cref="ScreenPictureSheetComposer.Compose"/> paints a screen's pictures, but naming for each colour
    /// group the gameplay background set of its order, as the import names installed room sheets' sets (#1464, #1493): a
    /// gameplay frame colours it from its background sets, and a frontend screen's frame from the screen's sets.
    /// </summary>
    /// <exception cref="InvalidDataException">Two subjects share a name, or the pictures need more than one sheet.</exception>
    internal static ComposedSpriteSheet ComposeRoom(IReadOnlyList<ScreenPictureSubject> subjects, ScreenCharacters characters, ReadOnlySpan<RgbColor> palette)
    {
        ScreenPictureSheetComposer.PaintedScreen painted = ScreenPictureSheetComposer.Paint(subjects, characters, palette);
        return painted.Canvas.ToRoomSheet(painted.Colours, painted.Frames, characters.Colours);
    }
}
