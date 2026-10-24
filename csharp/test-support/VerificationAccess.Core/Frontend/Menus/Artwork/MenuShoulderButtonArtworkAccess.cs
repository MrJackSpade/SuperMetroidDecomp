using SuperMetroid.AssetExtraction.Frontend.Menus;
namespace SuperMetroid.Core.Frontend.Menus.Artwork;

/// <summary>Verification access to <see cref="MenuShoulderButtonArtwork"/> members production does not use.</summary>
internal static class MenuShoulderButtonArtworkAccess
{
    /// <summary>Inspects explicit edits to shoulder-button artwork.</summary>
    extension(MenuShoulderButtonArtwork self)
    {
        /// <summary>Counts sparse shoulder-button byte replacements.</summary>
        internal int StoredEditCount => self.edits?.Count ?? 0;
    }
}
