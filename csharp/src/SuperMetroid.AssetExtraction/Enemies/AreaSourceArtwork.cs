using SuperMetroid.AssetExtraction.Areas.Ceres;
using SuperMetroid.Core;
using SuperMetroid.Core.Areas;
using SuperMetroid.AssetExtraction.Areas.Ceres.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The areas' installed artwork as the import pipeline holds it (#1463): beside what gameplay reads, the Ceres escape's
/// warning-text and door character pages the pre-coloured sheets are composed from, and the identity of the whole
/// selected area presentation.
/// </summary>
public sealed record AreaSourceArtwork : AreaArtworkBundle
{
    /// <summary>The Ceres door's characters and colours; setting it also sets the colours gameplay reads.</summary>
    public CeresDoorSourceArtwork? CeresDoor
    {
        get;
        init
        {
            field = value;
            CeresDoorVisual = value?.Visuals;
        }
    }

    /// <summary>Editable warning-text and door character pages for the Ceres escape.</summary>
    public CeresEscapeTileArtwork? CeresEscapeTiles { get; init; }

    /// <summary>
    /// Editable English and Japanese tile words of the Ceres warning pages, which only the written picture sheets are composed
    /// from (#1466): gameplay writes each page's picture by name.
    /// </summary>
    public CeresEscapeOverlays? CeresEscapeOverlayTilemaps { get; init; }

    /// <summary>The selected area presentation, computed from the installed content rather than kept with the game state.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("area-bundle-v1", content =>
    {
        content.AppendIdentity("CeresDoorVisual", CeresDoor?.ContentIdentity);
        content.AppendIdentity("CeresEscapeTiles", CeresEscapeTiles?.ContentIdentity);
        content.AppendIdentity("CeresEscapeOverlayTilemaps", CeresEscapeOverlayTilemaps?.ContentIdentity);
        content.AppendIdentity("TourianStatueColors", TourianStatueColors?.ContentIdentity);
    });

    /// <summary>Returns installed area artwork, which must carry both Ceres transfer providers.</summary>
    /// <exception cref="InvalidDataException">Either Ceres transfer provider is absent.</exception>
    public static AreaSourceArtwork FromInstalledArtwork(AreaSourceArtwork artwork)
    {
        Ensure.NotNull(artwork);
        if (artwork.CeresEscapeTiles is null || artwork.CeresEscapeOverlayTilemaps is null)
            throw new InvalidDataException("Installed area artwork requires both Ceres transfer providers.");
        return artwork;
    }
}
