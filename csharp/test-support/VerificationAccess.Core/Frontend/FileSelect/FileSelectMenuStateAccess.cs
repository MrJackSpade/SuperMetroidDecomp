using SuperMetroid.AssetExtraction.Frontend.FileSelect.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>Verification access to <see cref="FileSelectMenuState"/> members production does not use.</summary>
internal static class FileSelectMenuStateAccess
{
    /// <summary>Reads file-select BG tiles and the selected helmet frame without mutation.</summary>
    extension(FileSelectMenuState self)
    {
        /// <summary>
        /// The page the menu shows, laid out as the cartridge's tilemap words from the import's sources of the pictures it
        /// places (#1467), for cartridge-layout verification.
        /// </summary>
        internal ReadOnlySpan<ushort> BackgroundTilemap => BackgroundTileWordBytes.Words(FileSelectSourcePresentation.Of(
            (self.mapPresentation ?? throw new InvalidOperationException("File select's presentation is not bound.")).FileSelect).LayOut(self.ShownPage));

        /// <summary>Selected helmet frame retained through the fade into loading/options.</summary>
        internal int SelectedHelmetFrame => self.HelmetAnimationFrame;
    }
}
