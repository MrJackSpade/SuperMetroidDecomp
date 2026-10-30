using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Artwork;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The ending's turning floor (#1404, #1467): the shot whose floor its scenes have loaded, and that shot's installed
/// picture. A floor shows once its load completes; while the flyaway's or the reward icon's floor is still loading, the
/// floor keeps showing the shot loaded before it, and the credits' character art leaves no floor.
/// </summary>
internal static class EndingScreenFloor
{
    /// <summary>The shot whose floor <paramref name="uploads"/> leave loaded, in order.</summary>
    internal static EndingFloorShot Shot(IReadOnlyList<EndingUpload> uploads)
    {
        EndingFloorShot shot = EndingFloorShot.None;
        foreach (EndingUpload upload in Ensure.NotNull(uploads))
        {
            if (upload.Kind == EndingUploadKind.FloorSceneTiles)
                shot = SceneShot(upload.As<EndingFloorSceneUpload>().Scene);
            else if (upload.Kind == EndingUploadKind.FlyawayChunk && upload.As<EndingChunkUpload>().Chunk == EndingFloorDefinitions.FlyawayChunkCount - 1)
                shot = EndingFloorShot.Flyaway;
            else if (upload.Kind == EndingUploadKind.RewardChunk && upload.As<EndingChunkUpload>().Chunk == EndingRewardJumpDefinitions.IconChunkCount - 1)
                shot = EndingFloorShot.RewardIcon;
            else if (upload.Kind == EndingUploadKind.CreditsCharacterArt)
                shot = EndingFloorShot.None;
        }
        return shot;
    }

    /// <summary>The floor <paramref name="shot"/> shows.</summary>
    /// <exception cref="InvalidOperationException">No floor is loaded.</exception>
    /// <exception cref="InvalidDataException">The installed floor sheet has no picture of the shot.</exception>
    internal static AffineFloor Of(InstalledSpriteSheets sheets, EndingFloorShot shot) => shot == EndingFloorShot.None
        ? throw new InvalidOperationException("The ending shows its floor before a floor has loaded.")
        : FloorShots.Of(sheets, EndingFloorDefinitions.Sheets[shot], EndingFloorDefinitions.FrameName);

    /// <summary>The shot a floor scene's load shows.</summary>
    private static EndingFloorShot SceneShot(EndingFloorSceneId scene) => scene switch
    {
        EndingFloorSceneId.EscapeA => EndingFloorShot.EscapeA,
        EndingFloorSceneId.EscapeB => EndingFloorShot.EscapeB,
        EndingFloorSceneId.PlanetExplosion => EndingFloorShot.PlanetExplosion,
        _ => throw new ArgumentOutOfRangeException(nameof(scene), scene, "Undefined ending floor scene."),
    };
}
