using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using System.Collections.Immutable;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// Verification oracle (#1392): rasterizes frames composed from characters, as the cartridge's OBJ layer shows them, into
/// the same targets <see cref="SnesObjRenderer"/> resolves pre-coloured images into, so a check compares a picture with
/// the composition it stands for pixel by pixel.
/// </summary>
internal static class CharacterSpriteRaster
{
    /// <summary>
    /// Resolves <paramref name="draws"/> into the targets as <see cref="SnesObjRenderer.RenderResolved"/> resolves image
    /// draws: back to front, so the earliest-emitted draw owns each pixel, with its colour, priority and palette row.
    /// </summary>
    internal static void RenderResolved(ImmutableArray<CharacterSpriteDraw> draws, IPaletteColors cgram, ObjRasterTargets targets,
        Extent16<ScreenPixels>? size = null)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        Extent16<ScreenPixels> frame = size ?? SnesPpuLayout.ScreenSize;
        int pixelCount = frame.X * frame.Y;
        if (targets.Pixels.Length != pixelCount || targets.Priorities.Length != pixelCount ||
            (!targets.Palettes.IsEmpty && targets.Palettes.Length != pixelCount))
            throw new ArgumentException("OBJ raster targets have the wrong size.", nameof(targets));
        targets.Pixels.Clear();
        targets.Priorities.Fill(SnesObjRenderer.TransparentPriority);
        targets.Palettes.Fill(SnesObjRenderer.TransparentPriority);
        for (int draw = draws.Length - 1; draw >= 0; draw--)
            DrawCharacters(frame, draws[draw], cgram, targets);
    }

    /// <summary>Rasterizes one character draw from its bound artwork, last part first so the first part ends in front.</summary>
    private static void DrawCharacters(Extent16<ScreenPixels> frame, CharacterSpriteDraw draw, IPaletteColors cgram,
        ObjRasterTargets targets)
    {
        for (int index = draw.Frame.Parts.Length - 1; index >= 0; index--)
        {
            SpriteScreenPart part = draw.Place(index);
            for (ushort row = 0; row < part.Size.Y; row++)
            {
                int screenY = part.Position.Y + row;
                if ((uint)screenY >= frame.Y)
                    continue;
                for (ushort column = 0; column < part.Size.X; column++)
                {
                    int screenX = part.Position.X + column;
                    if ((uint)screenX >= frame.X)
                        continue;
                    int character = part.CharacterAt(new(column, row), out Position16<CharacterPixels> pixel);
                    byte colorIndex = draw.Artwork.ColorIndex(character, pixel);
                    // OBJ palettes live in the upper half of CGRAM. Each OBJ palette number
                    // selects one 16-color row: 128 + palette*16 + the 4-bpp pixel value.
                    if (colorIndex != 0)
                    {
                        int destination = screenY * frame.X + screenX;
                        targets.Pixels[destination] = cgram.GetRgba(128 + part.Palette * 16 + colorIndex);
                        targets.Priorities[destination] = (byte)part.Priority;
                        if (!targets.Palettes.IsEmpty)
                            targets.Palettes[destination] = (byte)part.Palette;
                    }
                }
            }
        }
    }
}
