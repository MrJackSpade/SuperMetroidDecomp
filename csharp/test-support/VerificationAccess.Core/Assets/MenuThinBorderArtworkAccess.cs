using SuperMetroid.Core.Assets;

/// <summary>Verification access to <see cref="MenuThinBorderArtwork"/> members production does not use.</summary>
internal static class MenuThinBorderArtworkAccess
{
    /// <summary>Inspects sparse edits to thin menu border tiles.</summary>
    extension(MenuThinBorderArtwork self)
    {
        /// <summary>Counts explicit border byte replacements.</summary>
        internal int StoredEditCount => self.edits?.Count ?? 0;
    }
}
