namespace SuperMetroid.AssetExtraction.Frontend.Map.Artwork;

/// <summary>
/// The shape of one frame of the cartridge's map highlight colours, which only the import reads (#1637): it installs each
/// frame's highlight colour by purpose.
/// </summary>
public static class MapPaletteCycleFormat
{
    /// <summary>Number of colors the cartridge writes per frame, equal to one sixteen-color colour set.</summary>
    public const int ColorCount = 16;

    /// <summary>
    /// The colours of each cartridge frame that the highlighted map sprites' highlight shows, in the highlighted colouring's
    /// set; every frame keeps them equal, and no other colour of the highlighted sprites changes over the cycle.
    /// </summary>
    internal static readonly int[] HighlightInks = [6, 11];
}
