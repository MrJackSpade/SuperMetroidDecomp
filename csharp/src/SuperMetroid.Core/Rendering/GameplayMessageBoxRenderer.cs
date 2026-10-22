using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Hud.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Rendering;

/// <summary>Composites bank-$85's message text, as its pre-coloured image, over a gameplay frame.</summary>
public static class GameplayMessageBoxRenderer
{
    /// <summary>Width of the gameplay raster receiving the message overlay.</summary>
    internal const int ScreenWidth = SnesGameplayFrameRenderer.Width;
    /// <summary>Height of the gameplay raster used for centered message clipping.</summary>
    internal const int ScreenHeight = SnesGameplayFrameRenderer.Height;

    /// <summary>Publishes the image of the currently exposed message (#1392); inactive or zero-radius messages emit none.</summary>
    /// <param name="messageBox">The message box state.</param>
    /// <param name="messages">The room's message box images; required while a message is exposed.</param>
    /// <exception cref="InvalidOperationException">A message is exposed with no room's message box images.</exception>
    /// <exception cref="InvalidDataException">The images picture no message of the words shown.</exception>
    public static MessageBoxRenderLayer? Capture(GameplayMessageBoxState messageBox, MessageBoxSheet? messages)
    {
        Ensure.NotNull(messageBox);
        if (!messageBox.IsActive || messageBox.RadiusPixels == 0)
            return null;
        return MessageBoxRenderLayer.FromTilemap(messageBox.Tilemap, messageBox.RadiusPixels, messages
            ?? throw new InvalidOperationException("An exposed message box requires the room's message box images."));
    }

    /// <summary>
    /// Paints the message's image over <paramref name="frame"/> on the reveal window's scanlines. Bank $85 overwrites only
    /// CGRAM $19/$1A while the box is open, so the text sees the gameplay palette with those two temporary colours: a row
    /// whose colours are then the image's standard shows the image's own colours, another the live ones.
    /// </summary>
    /// <param name="frame">The gameplay frame.</param>
    /// <param name="layer">The message layer.</param>
    /// <param name="cgram">The displayed palette.</param>
    [PpuLayoutBoundary("backend: software message-box palette from the displayed CGRAM")]
    public static void Composite(Span<Rgba32> frame, MessageBoxRenderLayer layer, IPaletteColors cgram)
    {
        Ensure.NotNull(layer);
        Ensure.NotNull(cgram);
        Ensure.LengthEqual<Rgba32>(frame, ScreenWidth * ScreenHeight);
        var shown = new SnesCgram();
        for (int index = 0; index < SnesCgram.ColorCount; index++)
            shown.SetColor(index, cgram.Colors[index]);
        shown.SetColor(GameplayMessageRomData.Palette.TemporaryLightIndex, GameplayMessageRomData.Palette.TemporaryLightColor);
        shown.SetColor(GameplayMessageRomData.Palette.TemporaryDarkIndex, GameplayMessageRomData.Palette.TemporaryDarkColor);
        var palette = new Rgba32[SnesCgram.ColorCount];
        for (int index = 0; index < palette.Length; index++)
            palette[index] = shown.GetRgba(index);
        var pictures = new BgPicturePixels(shown);
        int first = Math.Max(layer.FirstScanline, layer.Top), end = Math.Min(Math.Min(layer.EndScanline, ScreenHeight), layer.Top + layer.Picture.Size.Y);
        for (int y = first; y < end; y++)
        {
            for (int x = 0; x < ScreenWidth; x++)
            {
                BgSample sample = BgSample.OfPicture(layer.Picture, new Position16<ImagePixels>((ushort)x, (ushort)(y - layer.Top)));
                if (pictures.TryDecode(sample, palette, ColorZero.Transparent, out Rgba32 color, out _))
                    frame[y * ScreenWidth + x] = color;
            }
        }
    }
}
