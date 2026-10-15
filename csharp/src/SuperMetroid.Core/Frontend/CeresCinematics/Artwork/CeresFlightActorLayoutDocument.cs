namespace SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

/// <summary>Editable JSON payload for the five rear-view initial placements, excluding front stars, actor instruction programs, and movement mechanics.</summary>
public sealed record CeresFlightActorLayoutDocument
{
    /// <summary>Schema revision required to equal <see cref="CeresFlightActorLayoutFormat.Version"/> during loading.</summary>
    public required int Version { get; init; }
    /// <summary>Exactly five nonnull placements in order: <c>large-asteroid</c>, <c>station-under-attack</c>, <c>small-asteroid</c>, <c>vortex</c>, and <c>rear-stars</c>.</summary>
    public required CeresFlightActorPlacement[] Actors { get; init; }
}
