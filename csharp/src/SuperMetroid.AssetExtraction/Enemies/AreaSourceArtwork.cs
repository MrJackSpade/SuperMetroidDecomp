using SuperMetroid.Core;
using SuperMetroid.Core.Areas;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The areas' installed artwork as the import pipeline holds it (#1463): beside what gameplay reads, the Ceres escape's
/// warning-text and door character pages the pre-coloured sheets are composed from, and the identity of the whole
/// selected area presentation.
/// </summary>
public sealed record AreaSourceArtwork : AreaArtworkBundle
{
    /// <summary>Editable warning-text and door character pages for the Ceres escape.</summary>
    public CeresEscapeTileArtwork? CeresEscapeTiles { get; init; }

    /// <inheritdoc/>
    public override string ContentIdentity => SelectedPresentationHash.Create("area-bundle-v1", content =>
    {
        content.AppendIdentity("CeresDoorVisual", CeresDoorVisual?.ContentIdentity);
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
