using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;

/// <summary>Editable JSON payload for the five rear-view initial placements, excluding front stars, actor instruction programs, and movement mechanics.</summary>
public sealed record CeresFlightActorLayoutDocument
{
    /// <summary>Schema revision required to equal <see cref="CeresFlightActorLayoutFormat.Version"/> during loading.</summary>
    public required int Version { get; init; }
    /// <summary>One placement for each rear-view role (<see cref="Core.Frontend.CeresCinematics.CeresRearViewActorRole"/>), written in spawn order.</summary>
    public required CeresFlightActorPlacement[] Actors { get; init; }
}
