using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending;

/// <summary>
/// The installed escape, planet-explosion and reward-icon artwork as the import pipeline reads it (#1409), which names its
/// content identity from each scene's map and characters and the reward icon's transfer.
/// </summary>
public sealed class EndingFloorCharacters : EndingFloorArtwork
{
    /// <summary>Installs the three decoded backdrop pairs and the complete post-credits beam/icon transfer.</summary>
    /// <param name="escapeA">Zebes zoom-out map and character lanes for the first escape scene.</param>
    /// <param name="escapeB">Grey-cloud map and character lanes for the second escape scene.</param>
    /// <param name="planetExplosion">Large-Zebes map and character lanes for the explosion scene.</param>
    /// <param name="rewardIcon">Interleaved map and characters progressively uploaded during the post-credits reward jump.</param>
    /// <exception cref="ArgumentNullException">Any artwork argument is null.</exception>
    public EndingFloorCharacters(EndingFloorSceneArtwork escapeA, EndingFloorSceneArtwork escapeB,
        EndingFloorSceneArtwork planetExplosion, EndingRewardIconArtwork rewardIcon)
        : base(escapeA, escapeB, planetExplosion, rewardIcon)
    {
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidDataException">A scene or the reward icon was not read from installed artwork.</exception>
    public override string ContentIdentity => SelectedPresentationHash.Create("EndingFloorArtworkCatalog", content =>
    {
        content.Append("scene-count", 3);
        AppendScene(this[EndingFloorSceneId.EscapeA]);
        AppendScene(this[EndingFloorSceneId.EscapeB]);
        AppendScene(this[EndingFloorSceneId.PlanetExplosion]);
        content.Append("reward-icon", EndingRewardIconCharacters.Of(RewardIcon).Bytes.Span);

        void AppendScene(EndingFloorSceneArtwork scene)
        {
            content.Append("map", scene.Map.Span);
            content.Append("characters", EndingFloorSceneCharacters.Of(scene).Characters.Span);
        }
    });
}
