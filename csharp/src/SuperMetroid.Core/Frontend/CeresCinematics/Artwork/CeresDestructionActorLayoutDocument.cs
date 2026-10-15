namespace SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

/// <summary>Editable JSON payload for persistent destruction-scene placements, separate from generated explosion bursts and the later Zebes reveal layout.</summary>
public sealed record CeresDestructionActorLayoutDocument
{
    /// <summary>Schema revision required to equal <see cref="CeresDestructionActorLayoutFormat.Version"/> during loading.</summary>
    public required int Version { get; init; }
    /// <summary>Exactly three nonnull placements in native spawn order: <c>large-asteroid</c>, <c>small-asteroid</c>, and <c>vortex</c>.</summary>
    public required CeresDestructionActorPlacement[] Actors { get; init; }
}
