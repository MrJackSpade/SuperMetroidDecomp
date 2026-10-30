using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>
/// The tile word the cartridge clears a room's BG3 effect area to before an effect's page is drawn (#1348, #1466), which
/// the BG3 picture sheet pictures as the plane's cleared picture.
/// </summary>
internal static class Layer3ClearedCellDefinitions
{
    /// <summary>The cleared cell: the blank character 78 in colour variant 6, without priority.</summary>
    internal static BackgroundTile ClearedCell { get; } = new(78, 6, highPriority: false);
}
