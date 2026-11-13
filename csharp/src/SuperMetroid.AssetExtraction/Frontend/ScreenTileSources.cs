using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Ending;

namespace SuperMetroid.AssetExtraction.Frontend;

/// <summary>The installed stock artwork the screen tile sheets compose from (#1392), each family read when first needed.</summary>
/// <param name="maps">Reads the menus' presentation.</param>
/// <param name="intro">Reads the opening cinematic's artwork.</param>
/// <param name="endingObjects">Reads the ending's object artwork.</param>
/// <param name="endingPalettes">Reads the ending's palettes.</param>
/// <param name="endingMode7">Reads the ending's Mode 7 artwork.</param>
internal sealed class ScreenTileSources(Func<ImportAreaMapPresentation> maps, Func<ImportIntroCinematicArtwork> intro,
    Func<ImportEndingObjectArtwork> endingObjects, Func<EndingPaletteSet> endingPalettes, Func<EndingFloorCharacters> endingMode7)
{
    /// <summary>The ending's Mode 7 artwork, once read.</summary>
    private readonly Lazy<EndingFloorCharacters> endingMode7 = new(endingMode7);

    /// <summary>The ending's Mode 7 artwork.</summary>
    internal EndingFloorCharacters EndingMode7 => endingMode7.Value;

    /// <summary>The ending's object artwork, once read.</summary>
    private readonly Lazy<ImportEndingObjectArtwork> endingObjects = new(endingObjects);

    /// <summary>The ending's palettes, once read.</summary>
    private readonly Lazy<EndingPaletteSet> endingPalettes = new(endingPalettes);

    /// <summary>The ending's object artwork as the import read it, with the character atlases its sheets are composed from (#1636).</summary>
    internal ImportEndingObjectArtwork EndingObjects => endingObjects.Value;

    /// <summary>The ending's palettes.</summary>
    internal EndingPaletteSet EndingPalettes => endingPalettes.Value;

    /// <summary>The menus' presentation, once read.</summary>
    private readonly Lazy<ImportAreaMapPresentation> maps = new(maps);

    /// <summary>The opening cinematic's artwork, once read.</summary>
    private readonly Lazy<ImportIntroCinematicArtwork> intro = new(intro);

    /// <summary>The menus' presentation.</summary>
    internal ImportAreaMapPresentation Maps => maps.Value;

    /// <summary>The opening cinematic's artwork.</summary>
    internal ImportIntroCinematicArtwork Intro => intro.Value;
}
