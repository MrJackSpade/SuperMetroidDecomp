using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend;

public sealed partial class SuperMetroidGame
{
    [NonSerialized] private SamusBodyArtworkCatalog? samusBodyArt;

    /// <summary>Supplies the installed body PNGs to gameplay, demos and intro scenes.</summary>
    public void BindSamusBodyArt(SamusBodyArtworkCatalog? value)
    {
        samusBodyArt = value;
        if (runtime is not null) runtime.SamusBodyArt = value;
        intro?.BindSamusBodyArtwork(value);
    }
}
