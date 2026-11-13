using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Versioned installed <c>samus-arm-cannon.json</c>: what the game binds of the arm cannon, each pose's placement (#1638).</summary>
public sealed record SamusArmCannonLayoutDocument
{
    /// <summary>Layout schema revision; loading requires <see cref="SamusArmCannonArtworkFormat.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Each pose with a record's cover placement, keyed by the pose's name, for exactly those poses.</summary>
    public required Dictionary<SamusPoseId, SamusArmCannonPoseDocument> Poses { get; init; }
}
