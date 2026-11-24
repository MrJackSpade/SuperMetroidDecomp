using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>What a shade of a sheet does to the sheet's colours as a whole.</summary>
internal sealed partial class EngineSheets
{
    /// <summary>
    /// How much of the red, green and blue of a sheet's own colours one of its shades shows, taken over every colour
    /// the shade speaks of: the tint that, laid over the sheet's pictures, comes nearest the shade. A shade that only
    /// darkens or lightens is shown exactly; one that changes each colour its own way is shown in the round.
    /// </summary>
    /// <exception cref="InvalidDataException">The sheet has no such shade, or has none of one of the three colours to compare.</exception>
    internal (double Red, double Green, double Blue) ShadeTint(string family, string sheet, string shade)
    {
        InstalledSheetDocument document = _documents.TryGetValue((family, sheet), out InstalledSheetDocument? found)
            ? found
            : throw new InvalidDataException($"The installation has no sheet {sheet} in {family}.");
        long red = 0, green = 0, blue = 0, shownRed = 0, shownGreen = 0, shownBlue = 0;
        foreach (((int colour, int _), RgbaPixel shown) in ShadeOf(sheet, document, shade))
        {
            red += (colour >> 16) & 0xFF;
            green += (colour >> 8) & 0xFF;
            blue += colour & 0xFF;
            shownRed += shown.R;
            shownGreen += shown.G;
            shownBlue += shown.B;
        }

        if (red == 0 || green == 0 || blue == 0)
            throw new InvalidDataException($"The sheet {sheet} has no red, green or blue for its shade {shade} to be measured against.");
        return ((double)shownRed / red, (double)shownGreen / green, (double)shownBlue / blue);
    }
}
