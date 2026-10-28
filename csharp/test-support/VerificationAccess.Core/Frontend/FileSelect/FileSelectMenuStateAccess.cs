using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>Verification access to <see cref="FileSelectMenuState"/> members production does not use.</summary>
internal static class FileSelectMenuStateAccess
{
    /// <summary>Reads file-select BG tiles and the selected helmet frame without mutation.</summary>
    extension(FileSelectMenuState self)
    {
        /// <summary>The BG1 page the menu shows, as the cartridge's tilemap words, for cartridge-layout verification.</summary>
        internal ReadOnlySpan<ushort> BackgroundTilemap => BackgroundTileWordBytes.Words(self.screen.Tilemaps.Bg1);

        /// <summary>Selected helmet frame retained through the fade into loading/options.</summary>
        internal int SelectedHelmetFrame => self.HelmetAnimationFrame;
    }
}
