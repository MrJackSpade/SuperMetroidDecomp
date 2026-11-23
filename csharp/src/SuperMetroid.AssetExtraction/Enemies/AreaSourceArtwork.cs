using SuperMetroid.AssetExtraction.Areas.Ceres;
using SuperMetroid.AssetExtraction.Areas.Ceres.Artwork;
using SuperMetroid.AssetExtraction.Areas.Tourian.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The areas' installed artwork, which only the import pipeline holds (#1463, #1512): the Ceres door's and the Tourian
/// statues' colours and the Ceres escape's warning-text and door character pages, which the pre-coloured sheets are
/// composed from, and the identity of the whole selected area presentation. Gameplay reads none of it.
/// </summary>
public sealed partial record AreaSourceArtwork
{
    /// <summary>The Ceres door's characters and colours.</summary>
    public required CeresDoorSourceArtwork CeresDoor { get; init; }

    /// <summary>The Ceres door's colours, which the import pre-colours the doors' and the elevator room's shades from (#1512).</summary>
    public CeresDoorVisuals CeresDoorVisual => CeresDoor.Visuals;

    /// <summary>Editable Tourian statue entrance, eye and grey colours, which the import pre-colours the statues' shades from (#1512).</summary>
    public required TourianStatuePalettes TourianStatueColors { get; init; }

    /// <summary>Editable warning-text and door character pages for the Ceres escape.</summary>
    public required CeresEscapeTileArtwork CeresEscapeTiles { get; init; }

    /// <summary>
    /// Editable English and Japanese tile words of the Ceres warning pages, which only the written picture sheets are composed
    /// from (#1466): gameplay writes each page's picture by name.
    /// </summary>
    public required CeresEscapeOverlays CeresEscapeOverlayTilemaps { get; init; }
}
