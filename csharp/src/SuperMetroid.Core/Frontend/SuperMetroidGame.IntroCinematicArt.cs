using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend;

public sealed partial class SuperMetroidGame
{
    [NonSerialized] private IntroCinematicArtworkCatalog? introCinematicArt;

    /// <summary>
    /// Binds installed opening artwork independently of emulated game state. A loaded
    /// debugger state keeps its timeline but displays the currently selected PNG.
    /// </summary>
    public void BindIntroCinematicArt(IntroCinematicArtworkCatalog? artwork)
    {
        introCinematicArt = artwork;
        intro?.BindCharacterArtwork(artwork);
        ceresDestruction?.BindArtwork(artwork);
        endingCredits?.BindFlightArtwork(artwork?.CeresFlight);
    }
}
