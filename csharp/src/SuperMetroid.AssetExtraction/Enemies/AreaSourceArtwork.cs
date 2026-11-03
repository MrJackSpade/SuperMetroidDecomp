using SuperMetroid.AssetExtraction.Areas.Ceres;
using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Areas.Ceres.Artwork;
using SuperMetroid.AssetExtraction.Areas.Tourian.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The areas' installed artwork, which only the import pipeline holds (#1463, #1512): the Ceres door's and the Tourian
/// statues' colours and the Ceres escape's warning-text and door character pages, which the pre-coloured sheets are
/// composed from, and the identity of the whole selected area presentation. Gameplay reads none of it.
/// </summary>
public sealed record AreaSourceArtwork
{
    /// <summary>The Ceres door's characters and colours.</summary>
    public CeresDoorSourceArtwork? CeresDoor { get; init; }

    /// <summary>The Ceres door's colours, which the import pre-colours the doors' and the elevator room's shades from (#1512).</summary>
    public CeresDoorVisuals? CeresDoorVisual => CeresDoor?.Visuals;

    /// <summary>Editable Tourian statue entrance, eye and grey colours, which the import pre-colours the statues' shades from (#1512).</summary>
    public TourianStatuePalettes? TourianStatueColors { get; init; }

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
