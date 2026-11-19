using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// The message box's text as the overlay shows it (#1392, #1555): the pre-coloured picture of the message it shows, placed
/// in screen pixels centred across the screen and on the window, shown only on the reveal window's screen lines.
/// </summary>
public sealed record MessageBoxLayer : DisplayLayer
{
    /// <summary>Checks the picture fits the screen and the fully open window, and the reveal radius.</summary>
    /// <param name="picture">The picture of the message.</param>
    /// <param name="radiusPixels">The centered reveal half-height in pixels, from zero through 24.</param>
    /// <exception cref="ArgumentException">The picture is empty, wider than the screen or taller than the open window.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The radius is out of range.</exception>
    public MessageBoxLayer(SpriteSheetFrame picture, int radiusPixels)
    {
        Ensure.NotNull(picture);
        if (picture.Size.X == 0 || picture.Size.Y == 0 || picture.Size.X > ScreenGeometry.ScreenWidthPixels
            || picture.Size.Y > 2 * GameplayMessageBoxDefinitions.Timing.MaximumRadiusPixels)
            throw new ArgumentException("A message's picture must fit across the screen and within the open window.", nameof(picture));
        if (radiusPixels is < 0 or > GameplayMessageBoxDefinitions.Timing.MaximumRadiusPixels)
            throw new ArgumentOutOfRangeException(nameof(radiusPixels));
        Picture = picture;
        RadiusPixels = radiusPixels;
    }

    /// <summary>The picture of the message.</summary>
    public SpriteSheetFrame Picture { get; }

    /// <summary>The current centered reveal half-height in pixels, from zero through 24.</summary>
    public int RadiusPixels { get; }

    /// <summary>The screen pixel the picture's top-left corner lies on: the picture centred across the screen and on the window.</summary>
    public Position16<ScreenPixels> Corner => new((ScreenGeometry.ScreenWidthPixels - Picture.Size.X) / 2,
        GameplayMessageBoxDefinitions.Layout.WindowCenterY - Picture.Size.Y / 2);

    /// <summary>The first screen line the reveal window shows.</summary>
    public int FirstLine => GameplayMessageBoxDefinitions.Layout.WindowCenterY - RadiusPixels;

    /// <summary>The screen line after the last one the reveal window shows.</summary>
    public int EndLine => GameplayMessageBoxDefinitions.Layout.WindowCenterY + RadiusPixels;

    /// <summary>
    /// Publishes the image of the message the box's window exposes (#1392, #1466, #2505), or none while its window shows
    /// nothing.
    /// </summary>
    /// <param name="messageBox">The message box state.</param>
    /// <param name="messages">The room's message box images.</param>
    /// <exception cref="InvalidDataException">The images picture no such form of the message shown.</exception>
    public static MessageBoxLayer? Capture(GameplayMessageBoxState messageBox, MessageBoxSheet messages)
    {
        Ensure.NotNull(messages);
        return Ensure.NotNull(messageBox).Exposed is { } exposed ? Of(exposed.Message, exposed.RadiusPixels, messages) : null;
    }

    /// <summary>The layer showing <paramref name="sheet"/>'s image of <paramref name="shown"/>.</summary>
    /// <param name="shown">The message form shown.</param>
    /// <param name="radiusPixels">The reveal half-height in pixels.</param>
    /// <param name="sheet">The room's message box images.</param>
    /// <exception cref="InvalidDataException">The sheet pictures no such form.</exception>
    private static MessageBoxLayer Of(ShownMessage shown, int radiusPixels, MessageBoxSheet sheet) =>
        new(Ensure.NotNull(sheet).Picture(shown), radiusPixels);
}
