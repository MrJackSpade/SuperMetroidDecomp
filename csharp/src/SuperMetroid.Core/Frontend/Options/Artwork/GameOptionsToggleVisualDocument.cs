namespace SuperMetroid.Core.Frontend.Options.Artwork;

/// <summary>Enabled and disabled label regions for one special setting, recolored independently of the setting's mechanics.</summary>
public sealed record GameOptionsToggleVisualDocument
{
    /// <summary>Nonempty caller-owned array of row-major tile indexes 0-1023 for the enabled label; selected when the setting is enabled.</summary>
    public required int[] EnabledCells { get; init; }
    /// <summary>Nonempty caller-owned array of row-major tile indexes 0-1023 for the disabled label; selected when the setting is disabled and disjoint from enabled cells.</summary>
    public required int[] DisabledCells { get; init; }
}
