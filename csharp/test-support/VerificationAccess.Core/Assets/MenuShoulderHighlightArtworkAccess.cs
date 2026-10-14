using SuperMetroid.Core.Assets;

/// <summary>Verification access to <see cref="MenuShoulderHighlightArtwork"/> members production does not use.</summary>
internal static class MenuShoulderHighlightArtworkAccess
{
    /// <summary>Inspects explicit edits to shoulder-button highlight artwork.</summary>
    extension(MenuShoulderHighlightArtwork self)
    {
        /// <summary>Counts sparse highlight byte replacements.</summary>
        internal int StoredEditCount => self.edits?.Count ?? 0;
    }
}
