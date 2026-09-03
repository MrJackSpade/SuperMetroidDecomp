using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rendering;

/// <summary>Composites bank-$85's temporary BG3 message tilemap over a gameplay frame.</summary>
public static class GameplayMessageBoxRenderer
{
    private const int ScreenWidth = SnesGameplayFrameRenderer.Width;
    private const int ScreenHeight = SnesGameplayFrameRenderer.Height;

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
        ArgumentNullException.ThrowIfNull(messageBox);
        ArgumentNullException.ThrowIfNull(vram);
        ArgumentNullException.ThrowIfNull(cgram);
        if (frame.Length != ScreenWidth * ScreenHeight)
            throw new ArgumentException("A gameplay frame must contain exactly 256x224 pixels.", nameof(frame));
        if (!messageBox.IsActive || messageBox.RadiusPixels == 0)
            return;

        ReadOnlySpan<ushort> tilemap = messageBox.Tilemap;
        int rowCount = messageBox.TilemapRowCount;
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
        int clipTop = GameplayMessageRomData.Layout.WindowCenterY - messageBox.RadiusPixels;
        int clipBottomExclusive =
            GameplayMessageRomData.Layout.WindowCenterY + messageBox.RadiusPixels;

        for (int tileY = 0; tileY < rowCount; tileY++)
        {
            for (int tileX = 0; tileX < GameplayMessageRomData.Layout.TilemapWidth; tileX++)
            {
                SnesBgTilemapWord entry = tilemap[
                    tileY * GameplayMessageRomData.Layout.TilemapWidth + tileX];
                int characterByteAddress =
                    ((GameplayMessageRomData.Layout.CharacterBaseWord +
                        entry.CharacterIndex * GameplayMessageRomData.Layout.TilePixels) &
                        GameplayMessageRomData.Layout.VramWordMask) * 2;

                for (int outputY = 0;
                    outputY < GameplayMessageRomData.Layout.TilePixels;
                    outputY++)
                {
                    int screenY = artTop +
                        tileY * GameplayMessageRomData.Layout.TilePixels + outputY;
                    if (screenY < clipTop || screenY >= clipBottomExclusive ||
                        (uint)screenY >= ScreenHeight)
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
                        Rgba32 rgba = paletteIndex switch
                        {
                            // InitializePpuForMessageBoxes overwrites only CGRAM $19/$1A,
                            // then RestorePpuForMessageBox restores the gameplay palette.
                            GameplayMessageRomData.Palette.TemporaryLightIndex =>
                                SnesGraphics.DecodeBgr555Color(
                                    GameplayMessageRomData.Palette.TemporaryLightColor),
                            GameplayMessageRomData.Palette.TemporaryDarkIndex =>
                                SnesGraphics.DecodeBgr555Color(
                                    GameplayMessageRomData.Palette.TemporaryDarkColor),
                            _ => cgram.GetRgba(paletteIndex),
                        };
                        int screenX =
                            tileX * GameplayMessageRomData.Layout.TilePixels + outputX;
                        frame[screenY * ScreenWidth + screenX] = rgba;
                    }
                }
            }
        }
    }
}
