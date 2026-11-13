using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;

/// <summary>Editable initial-position schema for the planet, four star sheets, and title shown after Ceres destruction.</summary>
public sealed record CeresRevealActorLayoutDocument
{
    /// <summary>Schema revision; loading requires version one from <see cref="CeresRevealActorLayoutFormat.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Exactly seven nonnull entries in order: planet, stars-upper-left, stars-upper-right, stars-lower-left, stars-lower-right, planet-zebes-title, planet-zebes-subtitle; IDs cannot be reassigned or reordered.</summary>
    public required CeresRevealActorPlacement[] Actors { get; init; }
}
