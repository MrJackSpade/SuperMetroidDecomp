using SuperMetroid.AssetExtraction.Frontend.FileSelect.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Map.Artwork;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>Verification access to <see cref="FileSelectMenuState"/> members production does not use.</summary>
internal static class FileSelectMenuStateAccess
{
    /// <summary>Reads file-select BG tiles and the selected helmet frame without mutation.</summary>
    extension(FileSelectMenuState self)
    {
        /// <summary>
        /// The page the menu shows, laid out as the cartridge's tilemap words from <paramref name="maps"/>' sources of the
        /// pictures it places (#1467), for cartridge-layout verification; <paramref name="maps"/> is the presentation the menu
        /// was built with.
        /// </summary>
        internal ushort[] BackgroundTilemap(AreaMapPresentation maps) =>
            BackgroundTileWordBytes.Words(FileSelectSourcePresentation.Of(Ensure.NotNull(maps).FileSelect).LayOut(self.ShownPage));

        /// <summary>Selected helmet frame retained through the fade into loading/options.</summary>
        internal int SelectedHelmetFrame => self.HelmetAnimationFrame;
    }
}
