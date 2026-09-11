using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend;

internal sealed partial class PauseMenuState
{
    internal void BindMapPresentation(AreaMapPresentationCatalog? catalog)
    {
        mapPresentation = catalog;
        // Preserve the serialized scroll position and transition timing. Only refresh
        // BG1 when it currently contains the map; the equipment page shares that VRAM.
        if (ScreenMode == 0) LoadPauseMapTilemap();
    }
}
