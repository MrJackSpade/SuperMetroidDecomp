using SuperMetroid.Core.Map;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>
/// The cartridge's exploration rules, read from the installed map's cells (#1466, #1513): which cells can be explored and
/// which reveal the cell above. The import decides them once from the cells, so the game never asks what a cell shows.
/// </summary>
internal static class AreaMapExplorationRules
{
    /// <summary>Determines whether a map cell participates in exploration.</summary>
    /// <param name="stockCell">The installed cell.</param>
    /// <returns>True for every cell that maps a place; false for the empty cell.</returns>
    internal static bool IsDiscoverable(MapCell stockCell) => stockCell.Kind != MapCellKind.Empty;

    /// <summary>
    /// Whether exploring the cell also explores the cell above it: the lower floor piece of a sloped hallway, whose hallway
    /// runs up into the cell above, as <c>$90:AAFD</c> recognises that piece's character in either mirroring.
    /// </summary>
    internal static bool RevealsCellAbove(MapCell stockCell) =>
        stockCell.Slope is MapCellSlope.RightwardFloorLow or MapCellSlope.LeftwardFloorLow;
}
