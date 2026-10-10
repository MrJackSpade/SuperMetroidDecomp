using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend;

public sealed partial class SuperMetroidGame
{
    /// <summary>Installed ending and credits object artwork retained for binding to active or newly created ending state.</summary>
    [NonSerialized] private EndingObjectArtworkCatalog? endingObjectArt;

    /// <summary>Binds installed ending OBJ artwork after construction or restoration.</summary>
    public void BindEndingObjectArt(EndingObjectArtworkCatalog? artwork)
    {
        endingObjectArt = artwork;
        endingCredits?.BindObjectArtwork(artwork);
    }
}
