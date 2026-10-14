namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>Verification access to <see cref="FileSelectMenuState"/> members production does not use.</summary>
internal static class FileSelectMenuStateAccess
{
    /// <summary>Reads file-select BG tiles and the selected helmet frame without mutation.</summary>
    extension(FileSelectMenuState self)
    {
        /// <summary>
        /// Read-only access for cartridge-layout verification. Gameplay code uploads this same
        /// buffer to BG1; exposing a span avoids adding a second, test-only tilemap builder.
        /// </summary>
        internal ReadOnlySpan<ushort> BackgroundTilemap => self.bg1Tilemap;

        /// <summary>Selected helmet frame retained through the fade into loading/options.</summary>
        internal int SelectedHelmetFrame => self.helmetAnimationFrame;
    }
}
