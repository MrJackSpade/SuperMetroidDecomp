using System.Globalization;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// How an installed document writes a colour (#1633, #2564): <c>#rrggbb</c>, each channel two lowercase hexadecimal digits
/// of its eight-bit value, as an editor shows it.
/// </summary>
public static class InstalledColourText
{
    /// <summary>The characters of one written colour: the mark and two digits per channel.</summary>
    private const int Length = 7;

    /// <summary><paramref name="colour"/> as the manifest writes it.</summary>
    public static string Format(RgbColor colour) =>
        string.Create(CultureInfo.InvariantCulture, $"#{colour.Red:x2}{colour.Green:x2}{colour.Blue:x2}");

    /// <summary>The colour <paramref name="text"/> writes.</summary>
    /// <param name="text">The written colour.</param>
    /// <param name="context">Names what the colour belongs to in a failure.</param>
    /// <exception cref="InvalidDataException">The text is not a colour written as <c>#rrggbb</c>.</exception>
    public static RgbColor Parse(string? text, string context)
    {
        if (text is not { Length: Length } || text[0] != '#' ||
            !int.TryParse(text.AsSpan(1, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out int red) ||
            !int.TryParse(text.AsSpan(3, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out int green) ||
            !int.TryParse(text.AsSpan(5, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out int blue))
        {
            throw new InvalidDataException($"{Ensure.NotNull(context)} names colour \"{text}\", not one written as #rrggbb.");
        }
        return new RgbColor(red, green, blue);
    }

    /// <summary>
    /// The colour <paramref name="text"/> writes in an installed colour document (#2564), which lies on the game's fade scale
    /// (<see cref="FadeLevelColour"/>), as every backdrop, tint, glow and fade colour the game steps from does.
    /// </summary>
    /// <param name="text">The written colour.</param>
    /// <param name="context">Names what the colour belongs to in a failure.</param>
    /// <exception cref="InvalidDataException">The text is not a colour written as <c>#rrggbb</c>, or a channel is not one of the scale's levels.</exception>
    public static RgbColor ParseOnFadeScale(string? text, string context)
    {
        RgbColor colour = Parse(text, context);
        try
        {
            _ = FadeLevelColour.Of(colour);
        }
        catch (ArgumentException error)
        {
            throw new InvalidDataException($"{context} names colour \"{text}\", whose channels are not all levels of the game's fade scale.", error);
        }
        return colour;
    }
}
