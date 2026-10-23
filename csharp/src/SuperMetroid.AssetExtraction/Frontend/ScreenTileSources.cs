using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Frontend.Map.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend;

/// <summary>The installed stock artwork the screen tile sheets compose from (#1392), each family read when first needed.</summary>
/// <param name="maps">Reads the menus' presentation.</param>
/// <param name="intro">Reads the opening cinematic's artwork.</param>
/// <param name="endingObjects">Reads the ending's object artwork.</param>
/// <param name="endingPalettes">Reads the ending's palettes.</param>
/// <param name="endingMode7">Reads the ending's Mode 7 artwork.</param>
internal sealed class ScreenTileSources(Func<AreaMapPresentationCatalog> maps, Func<IntroCinematicArtworkCatalog> intro,
    Func<EndingObjectArtworkCatalog> endingObjects, Func<EndingPaletteCatalog> endingPalettes, Func<EndingFloorArtworkCatalog> endingMode7)
{
    /// <summary>The ending's Mode 7 artwork, once read.</summary>
    private readonly Lazy<EndingFloorArtworkCatalog> endingMode7 = new(endingMode7);

    /// <summary>The ending's Mode 7 artwork.</summary>
    internal EndingFloorArtworkCatalog EndingMode7 => endingMode7.Value;

    /// <summary>The ending's object artwork, once read.</summary>
    private readonly Lazy<EndingObjectArtworkCatalog> endingObjects = new(endingObjects);

    /// <summary>The ending's palettes, once read.</summary>
    private readonly Lazy<EndingPaletteCatalog> endingPalettes = new(endingPalettes);

    /// <summary>The ending's object artwork.</summary>
    internal EndingObjectArtworkCatalog EndingObjects => endingObjects.Value;

    /// <summary>The ending's palettes.</summary>
    internal EndingPaletteCatalog EndingPalettes => endingPalettes.Value;

    /// <summary>The menus' presentation, once read.</summary>
    private readonly Lazy<AreaMapPresentationCatalog> maps = new(maps);

    /// <summary>The opening cinematic's artwork, once read.</summary>
    private readonly Lazy<IntroCinematicArtworkCatalog> intro = new(intro);

    /// <summary>The menus' presentation.</summary>
    internal AreaMapPresentationCatalog Maps => maps.Value;

    /// <summary>The opening cinematic's artwork.</summary>
    internal IntroCinematicArtworkCatalog Intro => intro.Value;
}
