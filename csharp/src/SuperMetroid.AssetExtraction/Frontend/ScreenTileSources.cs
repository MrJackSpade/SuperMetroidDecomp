using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend;

/// <summary>The installed stock artwork the screen tile sheets compose from (#1392), each family read when first needed.</summary>
/// <param name="maps">Reads the menus' presentation.</param>
/// <param name="intro">Reads the opening cinematic's artwork.</param>
/// <param name="endingObjects">Reads the ending's object artwork.</param>
/// <param name="endingPalettes">Reads the ending's palettes.</param>
/// <param name="endingMode7">Reads the ending's Mode 7 artwork.</param>
internal sealed class ScreenTileSources(Func<AreaMapPresentation> maps, Func<IntroCinematicArtwork> intro,
    Func<EndingObjectArtwork> endingObjects, Func<EndingPaletteSet> endingPalettes, Func<EndingFloorArtwork> endingMode7)
{
    /// <summary>The ending's Mode 7 artwork, once read.</summary>
    private readonly Lazy<EndingFloorArtwork> endingMode7 = new(endingMode7);

    /// <summary>The ending's Mode 7 artwork.</summary>
    internal EndingFloorArtwork EndingMode7 => endingMode7.Value;

    /// <summary>The ending's object artwork, once read.</summary>
    private readonly Lazy<EndingObjectArtwork> endingObjects = new(endingObjects);

    /// <summary>The ending's palettes, once read.</summary>
    private readonly Lazy<EndingPaletteSet> endingPalettes = new(endingPalettes);

    /// <summary>The ending's object artwork, with the character atlases its sheets are composed from.</summary>
    internal EndingObjectSourceArtwork EndingObjects => EndingObjectSourceArtwork.Of(endingObjects.Value);

    /// <summary>The ending's palettes.</summary>
    internal EndingPaletteSet EndingPalettes => endingPalettes.Value;

    /// <summary>The menus' presentation, once read.</summary>
    private readonly Lazy<AreaMapPresentation> maps = new(maps);

    /// <summary>The opening cinematic's artwork, once read.</summary>
    private readonly Lazy<IntroCinematicArtwork> intro = new(intro);

    /// <summary>The menus' presentation.</summary>
    internal AreaMapPresentation Maps => maps.Value;

    /// <summary>The opening cinematic's artwork.</summary>
    internal IntroCinematicArtwork Intro => intro.Value;
}
