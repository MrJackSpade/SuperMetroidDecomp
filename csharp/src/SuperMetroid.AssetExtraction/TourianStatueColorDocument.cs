using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction;

/// <summary>
/// The import's Tourian statue colours (#1637), import data only the import pre-colours the statue's entrance and
/// unlock-effect shades from; boss selectors and animation timing stay in runtime code.
/// </summary>
public sealed record TourianStatueColorDocument
{
    /// <summary>Palette schema revision; loading currently requires version 1.</summary>
    public required int Version { get; init; }
    /// <summary>Sixteen ordered base-decoration inks corresponding to $AA:D785 and OBJ palette 7, including transparent slot zero.</summary>
    public required PaletteRgb5[] Base { get; init; }
    /// <summary>Sixteen ordered statue inks corresponding to $AA:D765 and OBJ palette 2, including transparent slot zero.</summary>
    public required PaletteRgb5[] Statue { get; init; }
    /// <summary>Four rows of four eye inks corresponding to $86:B91E, ordered Phantoon, Ridley, Draygon, Kraid and installed over base-decoration colors 9–12.</summary>
    public required PaletteRgb5[][] Eye { get; init; }
    /// <summary>Eight ordered grey-transition inks corresponding to $87:839C; the animated-tile instruction supplies the destination palette band.</summary>
    public required PaletteRgb5[] Grey { get; init; }
}
