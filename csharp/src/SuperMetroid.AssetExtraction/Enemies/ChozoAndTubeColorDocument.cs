using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The import's tube-crack and Chozo-statue colours (#1637), import data only the import pre-colours their sheets from: each
/// image's two ordered sixteen-color OBJ palettes as the cartridge stores them, transparent-slot words included.
/// </summary>
public sealed partial record ChozoAndTubeColorDocument
{
    /// <summary>Color schema revision; loading currently requires version 1.</summary>
    public required int Version { get; init; }
    /// <summary>32 ordered tube-crack inks corresponding to $AA:E2DD–$E31C, filling OBJ palettes 1 and 2; stock halves repeat but edits need not.</summary>
    public required PaletteRgb5[] TubeCracks { get; init; }
    /// <summary>32 ordered Wrecked Ship Chozo inks corresponding to $AA:E31D–$E35C, filling OBJ palettes 1 and 2.</summary>
    public required PaletteRgb5[] WreckedShip { get; init; }
    /// <summary>32 ordered Lower Norfair Chozo inks corresponding to $AA:E35D–$E39C, filling OBJ palettes 1 and 2 independently of the Wrecked Ship image.</summary>
    public required PaletteRgb5[] LowerNorfair { get; init; }
}
