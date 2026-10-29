using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Video;

/// <summary>
/// Where each of gameplay's background sets lies among the cartridge's background colours (#1464): the import
/// pipeline's private knowledge, with which it decodes installed tileset palettes and the starting image into sets, and
/// names the set each colour group of an installed gameplay background image draws in. Core never addresses a set by
/// its position.
/// </summary>
internal static class BackgroundColourSetPositionDefinitions
{
    /// <summary>The sets in the order the cartridge's background colours hold them, sixteen colours each.</summary>
    internal static readonly BackgroundColourSet[] InCartridgeOrder =
    [
        BackgroundColourSet.Hud,
        BackgroundColourSet.HudItemsAndEffects,
        BackgroundColourSet.TilesFirst,
        BackgroundColourSet.TilesSecond,
        BackgroundColourSet.TilesThird,
        BackgroundColourSet.TilesFourth,
        BackgroundColourSet.TilesFifth,
        BackgroundColourSet.TilesSixth,
    ];
}
