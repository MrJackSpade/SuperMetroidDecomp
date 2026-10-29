using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>
/// What the import reads boss body frames from (#1466): the tile Phantoon, Draygon, Crocomire and Mother Brain clear
/// their body layer with, and where Crocomire's death writes his melt images.
/// </summary>
internal static class BossBodySourceDefinitions
{
    /// <summary>The blank tile, 824 under colour variant zero, unmirrored, behind sprites, that clears the shared body layer.</summary>
    internal static BackgroundTile BlankTile { get; } = new(824, 0, highPriority: false);

    /// <summary>The cell Crocomire's death writes a melt image from: the start of the layer's second row.</summary>
    internal const int CrocomireMeltFirstCell = 32;
}
