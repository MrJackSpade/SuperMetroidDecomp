using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Runtime;

public sealed partial class SuperMetroidRuntime
{
    /// <summary>External map content supplied by the host; never serialized into a debugger snapshot.</summary>
    [NonSerialized] private AreaMapPresentationCatalog? mapPresentation;
    public AreaMapPresentationCatalog? MapPresentation
    {
        get => mapPresentation;
        set
        {
            mapPresentation = value;
            if (value is not null)
                VramWrites.RebindBusSource(HudTileAtlasFormat.SourceAddress, HudTileAtlasFormat.TransferByteCount,
                    SuperMetroid.Core.Hardware.VramAssetId.StandardHudTiles);
        }
    }
}
