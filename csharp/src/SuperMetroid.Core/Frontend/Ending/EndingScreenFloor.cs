using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Artwork;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The ending's turning floor (#1404, #1467): the installed picture of the shot whose floor its scenes have loaded.
/// </summary>
internal static class EndingScreenFloor
{
    /// <summary>The floor <paramref name="shot"/> shows.</summary>
    /// <exception cref="InvalidOperationException">No floor is loaded.</exception>
    /// <exception cref="InvalidDataException">The installed floor sheet has no picture of the shot.</exception>
    internal static AffineFloor Of(InstalledSpriteSheets sheets, EndingFloorShot shot) => shot == EndingFloorShot.None
        ? throw new InvalidOperationException("The ending shows its floor before a floor has loaded.")
        : FloorShots.Of(sheets, EndingFloorDefinitions.Sheets[shot], EndingFloorDefinitions.FrameName);

    /// <summary>The shot a floor scene's load shows.</summary>
    internal static EndingFloorShot SceneShot(EndingFloorSceneId scene) => scene switch
    {
        EndingFloorSceneId.EscapeA => EndingFloorShot.EscapeA,
        EndingFloorSceneId.EscapeB => EndingFloorShot.EscapeB,
        EndingFloorSceneId.PlanetExplosion => EndingFloorShot.PlanetExplosion,
        _ => throw new ArgumentOutOfRangeException(nameof(scene), scene, "Undefined ending floor scene."),
    };
}