using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>The Ceres cinematics' turning floors (#1467): the installed picture of each <see cref="CeresFloorShot"/>.</summary>
public static class CeresFloors
{
    /// <summary>The name of <paramref name="shot"/>'s picture in the installed Ceres floor sheet.</summary>
    public static string FrameName(CeresFloorShot shot) => shot.ToString();

    /// <summary>The floor <paramref name="shot"/> shows.</summary>
    /// <exception cref="InvalidDataException">The installed Ceres floor sheet has no picture of the shot.</exception>
    internal static AffineFloor Of(InstalledSpriteSheets sheets, CeresFloorShot shot) =>
        FloorShots.Of(sheets, SpriteSheetId.CeresFloor, FrameName(shot));
}
