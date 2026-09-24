using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend;

public sealed partial class SuperMetroidGame
{
    [NonSerialized] private EndingObjectArtworkCatalog? endingObjectArt;

    /// <summary>Binds installed ending OBJ artwork after construction or restoration.</summary>
    public void BindEndingObjectArt(EndingObjectArtworkCatalog? artwork)
    {
        endingObjectArt = artwork;
        endingCredits?.BindObjectArtwork(artwork);
    }
}
