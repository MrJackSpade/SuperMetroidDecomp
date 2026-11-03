namespace SuperMetroid.Core.Areas;

/// <summary>
/// The areas' installed artwork as the session binds it (#1395). Gameplay reads none of it: the Ceres door's and the
/// Tourian statues' colours, the Ceres escape's character pages and warning pages, which only the pre-coloured sheets
/// are composed from, all belong to the import pipeline's subclass (#1463, #1466, #1512).
/// </summary>
public abstract record AreaArtworkBundle;
