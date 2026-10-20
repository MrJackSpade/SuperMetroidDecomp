using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hud;

namespace SuperMetroid.Core.Rendering;

/// <summary>Composites bank-$85's message text layer over a gameplay frame.</summary>
public static class GameplayMessageBoxRenderer
{
    /// <summary>Width of the gameplay raster receiving the message overlay.</summary>
    internal const int ScreenWidth = SnesGameplayFrameRenderer.Width;
    /// <summary>Height of the gameplay raster used for centered message clipping.</summary>
    internal const int ScreenHeight = SnesGameplayFrameRenderer.Height;

    /// <summary>Publishes the currently exposed message over the HUD characters; inactive or zero-radius messages emit none.</summary>
    /// <param name="messageBox">The message box state.</param>
    /// <param name="hudCharacters">The installed BG3 characters; required while a message is exposed.</param>
    /// <exception cref="InvalidOperationException">A message is exposed but no HUD characters are bound.</exception>
    public static MessageBoxRenderLayer? Capture(GameplayMessageBoxState messageBox, BgArtwork? hudCharacters)
    {
        Ensure.NotNull(messageBox);
        if (!messageBox.IsActive || messageBox.RadiusPixels == 0)
            return null;
        return MessageBoxRenderLayer.FromTilemap(messageBox.Tilemap, messageBox.RadiusPixels, hudCharacters
            ?? throw new InvalidOperationException("An exposed message box requires the installed HUD characters."));
    }

    /// <summary>
    /// Paints the message text over <paramref name="frame"/>. Bank $85 overwrites only CGRAM $19/$1A while the box is
    /// open, so the text sees the gameplay palette with those two temporary colors.
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
        Span<Rgba32> palette = stackalloc Rgba32[SnesCgram.ColorCount];
        for (int index = 0; index < palette.Length; index++)
            palette[index] = cgram.GetRgba(index);
        palette[GameplayMessageRomData.Palette.TemporaryLightIndex] =
            GameplayMessageRomData.Palette.TemporaryLightColor.ToRgba32();
        palette[GameplayMessageRomData.Palette.TemporaryDarkIndex] =
            GameplayMessageRomData.Palette.TemporaryDarkColor.ToRgba32();
        BgLayerRenderer.Composite(frame, ScreenWidth, layer.Text, palette);
    }
}
