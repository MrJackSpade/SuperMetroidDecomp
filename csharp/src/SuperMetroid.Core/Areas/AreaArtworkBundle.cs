using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Areas.Tourian.Artwork;

namespace SuperMetroid.Core.Areas;

/// <summary>
/// The areas' installed artwork beside the shared enemy sheets (#1395) that gameplay reads: the Ceres door and the
/// Tourian statues' colors. The Ceres escape's character pages and warning pages, which only the pre-coloured sheets
/// are composed from, belong to the import pipeline's subclass (#1463, #1466).
/// </summary>
public abstract record AreaArtworkBundle
{
    /// <summary>The Ceres door actor's special tile transfer and RGB5 rows.</summary>
    public CeresDoorVisuals? CeresDoorVisual { get; init; }

    /// <summary>Editable entrance, eye and grey colors; statue unlocking stays compiled.</summary>
    public TourianStatuePalettes? TourianStatueColors { get; init; }

    /// <summary>The selected area presentation, computed rather than persisted in an exact debugger snapshot.</summary>
    public abstract string ContentIdentity { get; }
}
