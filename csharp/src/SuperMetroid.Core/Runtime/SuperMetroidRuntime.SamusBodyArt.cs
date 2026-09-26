using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Runtime;

public sealed partial class SuperMetroidRuntime
{
    [NonSerialized] private SamusBodyArtworkCatalog? samusBodyArt;

    /// <summary>Host-owned Samus visual tiles; restored and future Samus states use the same edit.</summary>
    public SamusBodyArtworkCatalog? SamusBodyArt
    {
        get => samusBodyArt;
        set
        {
            samusBodyArt = value;
            Samus?.TileTransfers.BindArtwork(value);
            if (Samus is not null) Samus.ArmCannon.Artwork = value?.ArmCannon;
        }
    }
}
