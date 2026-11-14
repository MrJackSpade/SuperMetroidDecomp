using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;

/// <summary>Editable JSON payload for persistent destruction-scene placements, separate from generated explosion bursts and the later Zebes reveal layout.</summary>
public sealed record CeresDestructionActorLayoutDocument
{
    /// <summary>Schema revision required to equal <see cref="CeresDestructionActorLayoutFormat.Version"/> during loading.</summary>
    public required int Version { get; init; }
    /// <summary>One placement for each destruction role (<see cref="Core.Frontend.CeresCinematics.CeresDestructionActorRole"/>), written in spawn order.</summary>
    public required CeresDestructionActorPlacement[] Actors { get; init; }
}
