using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Bosses.Draygon.Artwork;

/// <summary>Draygon's selected colours: the opening, the normal materials, the white-flash edits and the health bands.</summary>
/// <param name="Intro">Selected opening colors for Draygon's intro colour region.</param>
/// <param name="Background">Normal background material colors used outside white hurt flashes.</param>
/// <param name="Sprite">Normal sprite material colors used outside white hurt flashes.</param>
/// <param name="WhiteFlash">Only edited flash colors; stock entries are calculated on lookup.</param>
/// <param name="HealthBands">Four selected colors for each of Draygon's eight health bands.</param>
internal sealed record DraygonColorSelection(DraygonIntroPaint Intro, DraygonMaterialPaint Background, DraygonMaterialPaint Sprite,
    Dictionary<ColourCell, RgbColor> WhiteFlash, DraygonHealthPaint HealthBands);
