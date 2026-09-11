using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Runtime;

public sealed partial class SuperMetroidRuntime
{
    /// <summary>External map content supplied by the host; never serialized into a debugger snapshot.</summary>
    [NonSerialized] private AreaMapPresentationCatalog? mapPresentation;
    [NonSerialized] private bool hudArtworkRefreshPending;
    public AreaMapPresentationCatalog? MapPresentation
    {
        get => mapPresentation;
        set
        {
            mapPresentation = value;
            hudArtworkRefreshPending = value is not null && Hud.IsInitialized;
            if (value is not null)
                VramWrites.RebindBusSource(HudTileAtlasFormat.SourceAddress, HudTileAtlasFormat.TransferByteCount,
                    SuperMetroid.Core.Hardware.VramAssetId.StandardHudTiles);
        }
    }

    private void PublishReboundHudArtwork()
    {
        if (!hudArtworkRefreshPending) return;
        // Binding a restored session changes content ownership, not the already
        // published frame. Refresh at accepted NMI, before native queued writes.
        // The original upload's padding may now contain live BG2 tilemaps.
        mapPresentation!.HudTiles.LoadCharactersTo(Vram, HudTileAtlasFormat.DestinationWord * 2);
        hudArtworkRefreshPending = false;
    }
}
