using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// Decodes the pixels of pre-coloured block pictures one frame draws (#1392): a pixel whose palette row still holds its
/// sheet's standard colours shows the picture's own colour, and one whose row a palette effect has changed shows that
/// row's live colour, as the characters did.
/// </summary>
/// <param name="cgram">The frame's CGRAM.</param>
internal sealed class BgPicturePixels(IPaletteColors cgram)
{
    /// <summary>Colours in one palette row.</summary>
    private const int RowColours = 16;

    /// <summary>Colours in one two-bit palette row.</summary>
    private const int TwoBppRowColours = 4;

    /// <summary>The sheet whose standard rows were last checked.</summary>
    private CheckedSheet last = new(null, 0);

    /// <summary>The sheet checked before it, as a door's source room and the destination alternate across one frame.</summary>
    private CheckedSheet previous = new(null, 0);

    /// <summary>Decodes a picture sample's colour and tile priority; false where the pixel is transparent.</summary>
    /// <param name="sample">A sample of a block picture.</param>
    /// <param name="palette">The frame's 256 CGRAM colours as RGBA.</param>
    /// <param name="colorZero">
    /// Whether the layer shows colour zero, as the HUD does: a two-bit picture's colour-zero pixel then shows its row's live
    /// colour zero, as the characters did.
    /// </param>
    /// <param name="color">The pixel's colour.</param>
    /// <param name="highPriority">Whether the pixel's tile had the priority bit.</param>
    /// <exception cref="InvalidOperationException">An opaque colour zero is asked of a picture that keeps no row for it.</exception>
    internal bool TryDecode(in BgSample sample, ReadOnlySpan<Rgba32> palette, ColorZero colorZero, out Rgba32 color, out bool highPriority)
    {
        SpriteSheetFrame picture = sample.Picture ?? throw new ArgumentException("The sample shows a tile, not a block picture.", nameof(sample));
        SpriteImage sheet = picture.Sheet;
        int pixel = (picture.Corner.Y + sample.PicturePixel.Y) * sheet.Size.X + picture.Corner.X + sample.PicturePixel.X;
        byte index = sheet.PaletteIndices[pixel];
        bool twoBpp = sheet.Colours == SpriteImageColours.TwoBppBackground;
        if (twoBpp ? index % TwoBppRowColours == 0 : index == 0)
        {
            if (colorZero == ColorZero.Opaque)
            {
                if (!twoBpp)
                    throw new InvalidOperationException($"{sheet.Name} keeps no row for its transparent pixels, which an opaque colour zero needs.");
                color = palette[sheet.FirstColour + index];
                highPriority = sheet.Priorities[pixel] != 0;
                return true;
            }
            color = default;
            highPriority = false;
            return false;
        }
        highPriority = sheet.Priorities[pixel] != 0;
        color = (StandardRows(sheet) >> (index / RowColours) & 1) != 0 ? sheet.Pixels[pixel] : palette[sheet.FirstColour + index];
        return true;
    }

    /// <summary>The rows of <paramref name="sheet"/> whose live colours are its standard ones, one bit each.</summary>
    private int StandardRows(SpriteImage sheet)
    {
        if (ReferenceEquals(last.Sheet, sheet))
            return last.Rows;
        CheckedSheet found = ReferenceEquals(previous.Sheet, sheet) ? previous : new CheckedSheet(sheet, Check(sheet));
        previous = last;
        last = found;
        return found.Rows;
    }

    /// <summary>A sheet whose rows were checked against the frame's CGRAM.</summary>
    /// <param name="Sheet">The sheet, or null before any was checked.</param>
    /// <param name="Rows">Its rows whose live colours are its standard ones, one bit each.</param>
    private sealed record CheckedSheet(SpriteImage? Sheet, int Rows);

    /// <summary>Compares each row of <paramref name="sheet"/>'s standard colours with the live ones; colour zero never shows.</summary>
    private int Check(SpriteImage sheet)
    {
        ReadOnlySpan<Bgr555> live = cgram.Colors;
        ReadOnlySpan<Bgr555> standard = sheet.StandardColours;
        int rows = 0;
        for (int row = 0; row < standard.Length / RowColours; row++)
        {
            bool same = true;
            for (int colour = 1; colour < RowColours && same; colour++)
                same = live[sheet.FirstColour + row * RowColours + colour] == standard[row * RowColours + colour];
            if (same)
                rows |= 1 << row;
        }
        return rows;
    }
}
