using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend;

public sealed partial class SuperMetroidGame
{
    /// <summary>Installed ending Mode 7 catalog retained for binding when the ending-credits state is created.</summary>
    [NonSerialized] private EndingMode7ArtworkCatalog? endingMode7Art;

    /// <summary>Attaches the installed ending backdrops without changing scene mechanics.</summary>
    public void BindEndingMode7Art(EndingMode7ArtworkCatalog? artwork)
    {
        endingMode7Art = artwork;
        endingCredits?.BindMode7Artwork(artwork);
    }
}
