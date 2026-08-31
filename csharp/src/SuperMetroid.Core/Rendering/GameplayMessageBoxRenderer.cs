using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rendering;

/// <summary>Composites bank-$85's temporary BG3 message tilemap over a gameplay frame.</summary>
public static class GameplayMessageBoxRenderer
{
    private const int ScreenWidth = SnesGameplayFrameRenderer.Width;
    private const int ScreenHeight = SnesGameplayFrameRenderer.Height;
    private const int TilemapWidth = 32;
    private const int CharacterBaseWord = 0x4000;
    private const int WindowCenterY = 124;

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
        if (rowCount is not (3 or 6) || tilemap.Length != rowCount * TilemapWidth)
        {
            throw new InvalidDataException(
                $"Active message box has invalid {rowCount}x{TilemapWidth} tilemap.");
        }

        int artTop = WindowCenterY - rowCount * 8 / 2;
        int clipTop = WindowCenterY - messageBox.RadiusPixels;
        int clipBottomExclusive = WindowCenterY + messageBox.RadiusPixels;

        for (int tileY = 0; tileY < rowCount; tileY++)
        {
            for (int tileX = 0; tileX < TilemapWidth; tileX++)
            {
                SnesBgTilemapWord entry = tilemap[tileY * TilemapWidth + tileX];
                int characterByteAddress =
                    ((CharacterBaseWord + entry.CharacterIndex * 8) & 0x7fff) * 2;

                for (int outputY = 0; outputY < 8; outputY++)
                {
                    int screenY = artTop + tileY * 8 + outputY;
                    if (screenY < clipTop || screenY >= clipBottomExclusive ||
                        (uint)screenY >= ScreenHeight)
                    {
                        continue;
                    }

                    int sourceY = entry.FlipVertically ? 7 - outputY : outputY;
                    int planes = characterByteAddress + sourceY * 2;
                    byte plane0 = vram.ReadByte(planes);
                    byte plane1 = vram.ReadByte(planes + 1);
                    for (int outputX = 0; outputX < 8; outputX++)
                    {
                        int sourceX = entry.FlipHorizontally ? 7 - outputX : outputX;
                        int mask = 1 << (7 - sourceX);
                        int color = ((plane0 & mask) != 0 ? 1 : 0)
                                  | ((plane1 & mask) != 0 ? 2 : 0);
                        if (color == 0)
                            continue;

                        int paletteIndex = entry.PaletteIndex * 4 + color;
                        Rgba32 rgba = paletteIndex switch
                        {
                            // InitializePpuForMessageBoxes overwrites only CGRAM $19/$1A,
                            // then RestorePpuForMessageBox restores the gameplay palette.
                            25 => SnesGraphics.DecodeBgr555Color(0x0bb1),
                            26 => SnesGraphics.DecodeBgr555Color(0x001f),
                            _ => cgram.GetRgba(paletteIndex),
                        };
                        int screenX = tileX * 8 + outputX;
                        frame[screenY * ScreenWidth + screenX] = rgba;
                    }
                }
            }
        }
    }
}
