using SuperMetroid.AssetExtraction.Frontend.Menus;
namespace SuperMetroid.Core.Frontend.Menus.Artwork;

/// <summary>Verification access to <see cref="MenuPanelTileArtwork"/> members production does not use.</summary>
internal static class MenuPanelTileArtworkAccess
{
    /// <summary>Inspects explicit pixel edits to menu panel tiles.</summary>
    extension(MenuPanelTileArtwork self)
    {
        /// <summary>Counts sparse menu-panel byte replacements.</summary>
        internal int StoredEditCount => self.edits?.Count ?? 0;
    }
}
