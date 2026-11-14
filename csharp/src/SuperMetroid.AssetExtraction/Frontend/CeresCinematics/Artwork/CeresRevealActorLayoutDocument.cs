using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;

/// <summary>Editable initial-position schema for the planet, four star sheets, and title shown after Ceres destruction.</summary>
public sealed record CeresRevealActorLayoutDocument
{
    /// <summary>Schema revision; loading requires <see cref="CeresRevealActorLayoutFormat.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>One placement for each reveal role (<see cref="Core.Frontend.CeresCinematics.ZebesRevealActorRole"/>), written in spawn order, then the Japanese subtitle's.</summary>
    public required CeresRevealActorPlacement[] Actors { get; init; }
}
