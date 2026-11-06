using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>
/// The tile word the cartridge clears a room's BG3 effect area to before an effect's page is drawn (#1348, #1466), which
/// verification lays out to compare with native memory; the game shows nothing there and installs no picture of it (#1514).
/// </summary>
internal static class Layer3ClearedCellDefinitions
{
    /// <summary>The cleared cell: the blank character 78 in colour variant 6, without priority.</summary>
    internal static BackgroundTile ClearedCell { get; } = new(78, 6, highPriority: false);
}
