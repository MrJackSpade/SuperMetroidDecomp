using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

namespace SuperMetroid.AssetExtraction.Areas.Ceres;

/// <summary>
/// How the cartridge lays out the Ceres cinematics' floor maps (#1467), which the import composes each shot's floor from:
/// the installed views, one cell per byte, are the flight's front and rear gunship views, the station scene's two views,
/// then the blank view that clears the scene; each is copied over the start of a floor filled with the blank tile.
/// </summary>
internal static class CeresFloorShotDefinitions
{
    /// <summary>The floor tile every cell outside a loaded view shows.</summary>
    internal const byte BlankTile = 140;

    /// <summary>The cells of one view: 32 by 24.</summary>
    internal const int ViewCells = CeresDestructionArtworkFormat.CellsPerView;

    /// <summary>Where the gunship's front view starts.</summary>
    internal const int FrontViewStart = 0;

    /// <summary>Where the gunship's rear view starts.</summary>
    internal const int RearViewStart = ViewCells;

    /// <summary>Where the station scene's two views start.</summary>
    internal const int StationStart = 2 * ViewCells;

    /// <summary>The cells of the station scene's two views.</summary>
    internal const int StationCells = 2 * ViewCells;

    /// <summary>Where the blank view starts.</summary>
    internal const int BlankViewStart = 4 * ViewCells;

    /// <summary>The cells of every installed view together.</summary>
    internal const int MapCells = 5 * ViewCells;

    /// <summary>The floor cell the lower view starts at.</summary>
    internal const int LowerViewFirstCell = ViewCells;
}
