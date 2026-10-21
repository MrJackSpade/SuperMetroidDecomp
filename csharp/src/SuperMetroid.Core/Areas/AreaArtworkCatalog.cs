using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Areas.Tourian.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Areas;

/// <summary>
/// The areas' installed artwork beside the shared enemy sheets (#1395): the Ceres door, the Ceres escape's tiles and
/// overlay, and the Tourian statues' colors.
/// </summary>
public sealed record AreaArtworkCatalog
{
    /// <summary>The Ceres door actor's special tile transfer and RGB5 rows.</summary>
    public CeresDoorVisualCatalog? CeresDoorVisual { get; init; }

    /// <summary>Editable warning-text and door character pages for the Ceres escape.</summary>
    public CeresEscapeTileArtwork? CeresEscapeTiles { get; init; }

    /// <summary>Editable English and Japanese visual tile words for the Ceres warning overlay.</summary>
    public CeresEscapeOverlayTilemapCatalog? CeresEscapeOverlayTilemaps { get; init; }

    /// <summary>Editable entrance, eye and grey colors; statue unlocking stays compiled.</summary>
    public TourianStatueColorCatalog? TourianStatueColors { get; init; }

    /// <summary>The selected area presentation, computed rather than persisted in an exact debugger snapshot.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("area-bundle-v1", content =>
    {
        content.AppendIdentity("CeresDoorVisual", CeresDoorVisual?.ContentIdentity);
        content.AppendIdentity("CeresEscapeTiles", CeresEscapeTiles?.ContentIdentity);
        content.AppendIdentity("CeresEscapeOverlayTilemaps", CeresEscapeOverlayTilemaps?.ContentIdentity);
        content.AppendIdentity("TourianStatueColors", TourianStatueColors?.ContentIdentity);
    });

    /// <summary>Returns installed area artwork, which must carry both Ceres transfer providers the room-entry queue resolves.</summary>
    public static AreaArtworkCatalog FromInstalledArtwork(AreaArtworkCatalog artwork)
    {
        Ensure.NotNull(artwork);
        if (artwork.CeresEscapeTiles is null || artwork.CeresEscapeOverlayTilemaps is null)
            throw new InvalidDataException("Installed area artwork requires both Ceres transfer providers.");
        return artwork;
    }
}
