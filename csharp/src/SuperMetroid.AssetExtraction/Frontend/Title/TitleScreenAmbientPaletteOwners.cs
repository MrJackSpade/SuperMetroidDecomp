using SuperMetroid.AssetExtraction.Frontend.Title.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Title;

/// <summary>
/// The title screen's two ambient palette owners in the order the title palette file records them (#1502). Only the
/// import walks them; the game runs each owner's program directly.
/// </summary>
public static class TitleScreenAmbientPaletteOwners
{
    /// <summary>The two semantic palette owners in definition order.</summary>
    public static IReadOnlyList<TitleScreenAmbientPaletteFxColorDefinition> All { get; } =
        [TitleScreenAmbientPaletteFxColorDefinitions.TubeLight, TitleScreenAmbientPaletteFxColorDefinitions.Displays];
}
