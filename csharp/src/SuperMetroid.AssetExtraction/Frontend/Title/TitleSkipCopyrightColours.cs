using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Frontend.Title;

/// <summary>
/// The copyright glyph colours the title's skip writes as it rebuilds the title objects (#1512): the cartridge's code
/// writes them as immediates, so the extracted title palette document records these as its skip colours.
/// </summary>
internal static class TitleSkipCopyrightColours
{
    /// <summary>The copyright's white on the skipped-to title screen.</summary>
    internal static RgbColor White => RgbColor.White;

    /// <summary>The skip-only copyright colour, kept under its historical Red name.</summary>
    internal static RgbColor Red => new(0, 99, 255);
}
