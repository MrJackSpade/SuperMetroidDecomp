using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Frontend.Map.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend;

/// <summary>The installed stock artwork the screen tile sheets compose from (#1392), each family read when first needed.</summary>
/// <param name="maps">Reads the menus' presentation.</param>
/// <param name="intro">Reads the opening cinematic's artwork.</param>
internal sealed class ScreenTileSources(Func<AreaMapPresentationCatalog> maps, Func<IntroCinematicArtworkCatalog> intro)
{
    /// <summary>The menus' presentation, once read.</summary>
    private readonly Lazy<AreaMapPresentationCatalog> maps = new(maps);

    /// <summary>The opening cinematic's artwork, once read.</summary>
    private readonly Lazy<IntroCinematicArtworkCatalog> intro = new(intro);

    /// <summary>The menus' presentation.</summary>
    internal AreaMapPresentationCatalog Maps => maps.Value;

    /// <summary>The opening cinematic's artwork.</summary>
    internal IntroCinematicArtworkCatalog Intro => intro.Value;
}
