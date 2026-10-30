using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// The message box's text as the overlay shows it (#1392): the pre-coloured image of the message it shows, centered on the
/// window, shown only on the reveal window's screen lines.
/// </summary>
public sealed record MessageBoxLayer : DisplayLayer
{
    /// <summary>Checks the image's shape and the reveal radius.</summary>
    /// <param name="picture">The image of the message's text rows.</param>
    /// <param name="radiusPixels">The centered reveal half-height in pixels, from zero through 24.</param>
    /// <exception cref="ArgumentException">The image is not three through six full-width rows.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The radius is out of range.</exception>
    public MessageBoxLayer(SpriteSheetFrame picture, int radiusPixels)
    {
        Ensure.NotNull(picture);
        int rows = picture.Size.Y / GameplayMessageBoxDefinitions.Layout.TilePixels;
        if (picture.Size.X != GameplayMessageBoxDefinitions.Layout.TilemapWidth * GameplayMessageBoxDefinitions.Layout.TilePixels
            || picture.Size.Y % GameplayMessageBoxDefinitions.Layout.TilePixels != 0
            || rows < GameplayMessageBoxDefinitions.Layout.MinimumRows || rows > GameplayMessageBoxDefinitions.Layout.MaximumRows)
            throw new ArgumentException("A message's image must be three through six complete rows.", nameof(picture));
        if (radiusPixels is < 0 or > GameplayMessageBoxDefinitions.Timing.MaximumRadiusPixels)
            throw new ArgumentOutOfRangeException(nameof(radiusPixels));
        Picture = picture;
        RadiusPixels = radiusPixels;
    }

    /// <summary>The image of the message's text rows.</summary>
    public SpriteSheetFrame Picture { get; }

    /// <summary>The current centered reveal half-height in pixels, from zero through 24.</summary>
    public int RadiusPixels { get; }

    /// <summary>The screen line the image's top row is drawn on: the rows centered on the window.</summary>
    public int Top => GameplayMessageBoxDefinitions.Layout.WindowCenterY - Picture.Size.Y / 2;

    /// <summary>The first screen line the reveal window shows.</summary>
    public int FirstLine => GameplayMessageBoxDefinitions.Layout.WindowCenterY - RadiusPixels;

    /// <summary>The screen line after the last one the reveal window shows.</summary>
    public int EndLine => GameplayMessageBoxDefinitions.Layout.WindowCenterY + RadiusPixels;

    /// <summary>
    /// The colours the message's text shows in over <paramref name="room"/>, the frame's: the room's, with the box's light
    /// and dark text colours in place of the two inks they replace while the box is open.
    /// </summary>
    public static BackgroundColours ShownIn(BackgroundColours room) => Ensure.NotNull(room)
        .WithBackgroundColour(GameplayMessageBoxDefinitions.Palette.TextSet, GameplayMessageBoxDefinitions.Palette.TemporaryLightColour,
            GameplayMessageBoxDefinitions.Palette.TemporaryLightColor)
        .WithBackgroundColour(GameplayMessageBoxDefinitions.Palette.TextSet, GameplayMessageBoxDefinitions.Palette.TemporaryDarkColour,
            GameplayMessageBoxDefinitions.Palette.TemporaryDarkColor);

    /// <summary>Publishes the image of the currently exposed message (#1392, #1466); inactive or zero-radius messages emit none.</summary>
    /// <param name="messageBox">The message box state.</param>
    /// <param name="messages">The room's message box images; required while a message is exposed.</param>
    /// <exception cref="InvalidOperationException">A message is exposed with no room's message box images.</exception>
    /// <exception cref="InvalidDataException">The images picture no such form of the message shown.</exception>
    public static MessageBoxLayer? Capture(GameplayMessageBoxState messageBox, MessageBoxSheet? messages)
    {
        Ensure.NotNull(messageBox);
        if (!messageBox.IsActive || messageBox.RadiusPixels == 0)
            return null;
        ShownMessage shown = messageBox.Shown ?? throw new InvalidOperationException("An exposed message box shows a message.");
        return Of(shown, messageBox.RadiusPixels, messages
            ?? throw new InvalidOperationException("An exposed message box requires the room's message box images."));
    }

    /// <summary>The layer showing <paramref name="sheet"/>'s image of <paramref name="shown"/>.</summary>
    /// <param name="shown">The message form shown.</param>
    /// <param name="radiusPixels">The reveal half-height in pixels.</param>
    /// <param name="sheet">The room's message box images.</param>
    /// <exception cref="InvalidDataException">The sheet pictures no such form.</exception>
    public static MessageBoxLayer Of(ShownMessage shown, int radiusPixels, MessageBoxSheet sheet) =>
        new(Ensure.NotNull(sheet).Picture(shown.FrameName), radiusPixels);
}
