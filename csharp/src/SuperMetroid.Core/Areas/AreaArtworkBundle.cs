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
    /// <summary>The Ceres door's colours; its characters belong to the import pipeline's subclass.</summary>
    public CeresDoorVisuals? CeresDoorVisual { get; init; }

    /// <summary>Editable entrance, eye and grey colors; statue unlocking stays compiled.</summary>
    public TourianStatuePalettes? TourianStatueColors { get; init; }

    /// <summary>The selected area presentation, computed from the installed content rather than kept with the game state.</summary>
    public abstract string ContentIdentity { get; }
}
