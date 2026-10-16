using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rendering;

/// <summary>Cartridge values and helpers of <see cref="GameplayMessageBoxRenderer"/> that only verification reads.</summary>
internal static class GameplayMessageBoxRendererConstants
{
    /// <summary>Mask keeping a VRAM word address inside the 32K-word VRAM.</summary>
    private const int VramWordMask = 32767;

    /// <summary>
    /// Draws the currently exposed scanline band. The original uses BG3 vertical-scroll
    /// HDMA to reveal a centered box; evaluating the resulting clipped raster directly is
    /// equivalent and avoids mutating the gameplay HUD tilemap in modeled VRAM.
    /// </summary>
    public static void Composite(
        Span<Rgba32> frame,
        GameplayMessageBoxState messageBox,
        SnesVram vram,
        SnesCgram cgram)
    {
        Ensure.NotNull(messageBox);
        Ensure.NotNull(vram);
        Ensure.NotNull(cgram);
        Ensure.LengthEqual(frame, GameplayMessageBoxRenderer.ScreenWidth * GameplayMessageBoxRenderer.ScreenHeight);
        if (!messageBox.IsActive || messageBox.RadiusPixels == 0)
            return;

        ReadOnlySpan<ushort> tilemap = messageBox.Tilemap;
        int rowCount = messageBox.TilemapRowCount;
        CompositeTilemap(frame, tilemap, rowCount, messageBox.RadiusPixels, vram, cgram);
    }

    /// <summary>The 1024 two-bpp characters a message tilemap can name, read from a test VRAM image at the BG3 base.</summary>
    /// <param name="vram">The VRAM image.</param>
    [PpuLayoutBoundary("exact hardware test: message characters at the BG3 VRAM base")]
    public static BgArtwork MessageCharacters(ReadOnlySpan<byte> vram) =>
        new(new BgArtworkId(BgArtworkKind.HudCharacters), 2, vram.Slice(GameplayVramLayout.HudCharacterWord * 2, 1024 * 16));

    /// <summary>The message layer the state exposes, reading characters from a test VRAM image.</summary>
    /// <param name="messageBox">The message state.</param>
    /// <param name="vram">The VRAM image.</param>
    [PpuLayoutBoundary("exact hardware test: message characters captured from a test VRAM image")]
    public static MessageBoxRenderLayer? Capture(GameplayMessageBoxState messageBox, SnesVram vram) =>
        GameplayMessageBoxRenderer.Capture(messageBox, MessageCharacters(vram.CaptureImage()));

    /// <summary>A message layer of the given words over a test VRAM image's characters.</summary>
    /// <param name="tilemap">The message words.</param>
    /// <param name="radiusPixels">The reveal radius.</param>
    /// <param name="vram">The VRAM image.</param>
    public static MessageBoxRenderLayer Layer(ReadOnlySpan<ushort> tilemap, int radiusPixels, ReadOnlySpan<byte> vram) =>
        MessageBoxRenderLayer.FromTilemap(tilemap, radiusPixels, MessageCharacters(vram));

    /// <summary>Rasterizes a message tilemap from BG3 characters in VRAM, as the renderer did before #1309.</summary>
    [PpuLayoutBoundary("exact hardware test: the pre-#1309 VRAM message rasterizer as an independent oracle")]
    internal static void CompositeTilemap(Span<Rgba32> frame, ReadOnlySpan<ushort> tilemap,
        int rowCount, int radiusPixels, SnesVram vram, SnesCgram cgram)
    {
        Ensure.NotNull(vram);
        Ensure.NotNull(cgram);
        Ensure.LengthEqual<Rgba32>(frame, GameplayMessageBoxRenderer.ScreenWidth * GameplayMessageBoxRenderer.ScreenHeight);
        // InitializePpuForMessageBoxes overwrites only CGRAM $19/$1A, then
        // RestorePpuForMessageBox restores the gameplay palette: the box sees CGRAM
        // with those two temporary colors applied.
        Span<Rgba32> palette = stackalloc Rgba32[SnesCgram.ColorCount];
        for (int index = 0; index < palette.Length; index++)
            palette[index] = cgram.GetRgba(index);
        palette[GameplayMessageRomData.Palette.TemporaryLightIndex] =
            GameplayMessageRomData.Palette.TemporaryLightColor.ToRgba32();
        palette[GameplayMessageRomData.Palette.TemporaryDarkIndex] =
            GameplayMessageRomData.Palette.TemporaryDarkColor.ToRgba32();
        // The ROM builder accepts a variable number of content rows. In particular,
        // message $14 uses three content rows plus two small-border rows (five total),
        // so validating only the common 3-row and 6-row shapes rejects retail data.
        // The fixed 24-pixel half-window can expose at most six 8-pixel rows.
        if (rowCount < GameplayMessageRomData.Layout.MinimumRows ||
            rowCount > GameplayMessageRomData.Layout.MaximumRows ||
            tilemap.Length != rowCount * GameplayMessageRomData.Layout.TilemapWidth)
        {
            throw new InvalidDataException(
                $"Active message box has invalid {rowCount}x" +
                $"{GameplayMessageRomData.Layout.TilemapWidth} tilemap.");
        }

        int artTop = GameplayMessageRomData.Layout.WindowCenterY -
            rowCount * GameplayMessageRomData.Layout.TilePixels / 2;
        int clipTop = GameplayMessageRomData.Layout.WindowCenterY - radiusPixels;
        int clipBottomExclusive =
            GameplayMessageRomData.Layout.WindowCenterY + radiusPixels;

        for (int tileY = 0; tileY < rowCount; tileY++)
        {
            for (int tileX = 0; tileX < GameplayMessageRomData.Layout.TilemapWidth; tileX++)
            {
                SnesBgTilemapWord entry = tilemap[
                    tileY * GameplayMessageRomData.Layout.TilemapWidth + tileX];
                int characterByteAddress =
                    ((GameplayVramLayout.HudCharacterWord +
                        entry.CharacterIndex * GameplayMessageRomData.Layout.TilePixels) &
                        VramWordMask) * 2;

                for (int outputY = 0;
                    outputY < GameplayMessageRomData.Layout.TilePixels;
                    outputY++)
                {
                    int screenY = artTop +
                        tileY * GameplayMessageRomData.Layout.TilePixels + outputY;
                    if (screenY < clipTop || screenY >= clipBottomExclusive ||
                        (uint)screenY >= GameplayMessageBoxRenderer.ScreenHeight)
                    {
                        continue;
                    }

                    int sourceY = entry.FlipVertically
                        ? GameplayMessageRomData.Layout.TilePixels - 1 - outputY
                        : outputY;
                    int planes = characterByteAddress + sourceY * 2;
                    byte plane0 = vram.ReadByte(planes);
                    byte plane1 = vram.ReadByte(planes + 1);
                    for (int outputX = 0;
                        outputX < GameplayMessageRomData.Layout.TilePixels;
                        outputX++)
                    {
                        int sourceX = entry.FlipHorizontally
                            ? GameplayMessageRomData.Layout.TilePixels - 1 - outputX
                            : outputX;
                        int mask = 1 << (
                            GameplayMessageRomData.Layout.TilePixels - 1 - sourceX);
                        int color = ((plane0 & mask) != 0 ? 1 : 0)
                                  | ((plane1 & mask) != 0 ? 2 : 0);
                        if (color == 0)
                            continue;

                        int paletteIndex = entry.PaletteIndex * 4 + color;
                        Rgba32 rgba = palette[paletteIndex];
                        int screenX =
                            tileX * GameplayMessageRomData.Layout.TilePixels + outputX;
                        frame[screenY * GameplayMessageBoxRenderer.ScreenWidth + screenX] = rgba;
                    }
                }
            }
        }
    }
}
