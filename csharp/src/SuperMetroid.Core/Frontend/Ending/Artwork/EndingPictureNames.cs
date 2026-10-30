namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>
/// The names of the ending's pre-assembled background pictures (#1467), which the import composes into the ending's
/// picture sheets and the scenes select by what they show.
/// </summary>
public static class EndingPictureNames
{
    /// <summary>The waiting scene's backdrop, a whole screen.</summary>
    public const string WaitingBackdrop = "WaitingBackdrop";

    /// <summary>The Super Metroid logo, a whole screen.</summary>
    public const string Logo = "Logo";

    /// <summary>The wavy Samus transformation, a whole screen of four-colour pictures.</summary>
    public const string WavySamus = "WavySamus";

    /// <summary>The result panel: the producer and development credits, nine rows.</summary>
    public const string ResultPanel = "ResultPanel";

    /// <summary>The copyright panel, two rows.</summary>
    public const string CopyrightPanel = "CopyrightPanel";

    /// <summary>The Japanese subtitle shown under the item percentage, two rows.</summary>
    public const string JapaneseSubtitle = "JapaneseSubtitle";

    /// <summary>The item percentage's percent sign, two rows.</summary>
    public const string PercentSign = "PercentSign";

    /// <summary>The explosion's map <paramref name="map"/>, a whole screen.</summary>
    public static string ExplosionMap(EndingObjectFragmentId map) => "ExplosionMap" + map;

    /// <summary>The credits roll's line <paramref name="line"/>, or the lower row of a two-row line.</summary>
    public static string CreditsLine(int line, bool bottom) =>
        "CreditsLine" + line.ToString("D2", System.Globalization.CultureInfo.InvariantCulture) + (bottom ? "Bottom" : "");

    /// <summary>The letter <paramref name="character"/> of the typed text style <paramref name="style"/>.</summary>
    public static string Glyph(EndingTextStyle style, char character) => style + "Glyph" + character;

    /// <summary>The item percentage's digit <paramref name="digit"/>, two rows.</summary>
    public static string PercentDigit(int digit) => "PercentDigit" + digit.ToString(System.Globalization.CultureInfo.InvariantCulture);
}
