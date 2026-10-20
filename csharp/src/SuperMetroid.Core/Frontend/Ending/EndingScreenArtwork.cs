using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>The installed artwork the ending's transfers read, bound by the host.</summary>
/// <param name="Mode7">The ending's Mode 7 scenes and reward icon.</param>
/// <param name="Flight">The Ceres flight artwork the first flyaway reuses.</param>
/// <param name="Objects">The ending's OBJ sheets, fragments and post-credits art.</param>
/// <param name="Font">The ending font.</param>
/// <param name="Reward">The post-credits reward, which selects the reward characters.</param>
internal sealed record EndingScreenArtwork(EndingMode7ArtworkCatalog? Mode7, CeresFlightArtworkCatalog? Flight,
    EndingObjectArtworkCatalog? Objects, EndingFontAtlas? Font, EndingReward Reward);
