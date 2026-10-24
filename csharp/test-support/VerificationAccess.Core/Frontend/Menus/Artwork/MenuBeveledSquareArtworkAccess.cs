using SuperMetroid.AssetExtraction.Frontend.Menus;
namespace SuperMetroid.Core.Frontend.Menus.Artwork;

/// <summary>Verification access to <see cref="MenuBeveledSquareArtwork"/> members production does not use.</summary>
internal static class MenuBeveledSquareArtworkAccess
{
    /// <summary>Inspects sparse bevel-tile edits in the menu artwork.</summary>
    extension(MenuBeveledSquareArtwork self)
    {
        /// <summary>Counts explicit bevel pixel replacements only.</summary>
        internal int StoredEditCount => self.edits?.Count ?? 0;
    }
}
