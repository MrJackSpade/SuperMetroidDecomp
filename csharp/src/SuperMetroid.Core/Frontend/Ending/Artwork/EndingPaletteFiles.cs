namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>
/// Names and sizes each ending palette resource (<see cref="EndingPaletteDefinitions"/>) by its semantic palette role.
/// </summary>
public static class EndingPaletteFiles
{
    /// <summary>Published asset-file identities selected by semantic palette role.
    /// Explicit cases preserve independently editable stock and override filenames.</summary>
    public static string FileName(EndingPaletteId id) =>
        Ensure.Listed(EndingPaletteDefinitions.FileNames, id);

    /// <summary>Full editable resource sizes, not individual palette load lengths:
    /// four 256-color allocations, two 16-color palettes, and sixteen pairs
    /// of 16-color crossfade palettes. The credits' palette loads can select a subset.</summary>
    public static int ColorCount(EndingPaletteId id) =>
        Ensure.Listed(EndingPaletteDefinitions.ColorCounts, id);
}
