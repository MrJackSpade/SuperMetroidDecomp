using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Runtime;

public sealed partial class SuperMetroidRuntime
{
    /// <summary>Host-owned Samus visual tiles; restored and future Samus states use the same edit.</summary>
    [field: NonSerialized]     public SamusBodyArtworkCatalog? SamusBodyArt
    {
        get;
        set
        {
            field = value;
            Samus?.TileTransfers.BindArtwork(value);
            if (Samus is not null) Samus.ArmCannon.Artwork = value?.ArmCannon;
        }
    }
}
