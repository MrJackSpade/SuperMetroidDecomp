using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Versioned installed <c>samus-arm-cannon.json</c>: what the game binds of the arm cannon (#1638).</summary>
public sealed record SamusArmCannonLayoutDocument
{
    /// <summary>Layout schema revision; loading requires <see cref="SamusArmCannonArtworkFormat.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>
    /// Where each pose with a record's descriptor begins within <see cref="DrawingData"/>, keyed by the pose's name, for
    /// exactly those poses; loading requires room for a descriptor.
    /// </summary>
    public required Dictionary<SamusPoseId, int> PoseDescriptors { get; init; }
    /// <summary>Exactly 608 byte values: direction and drawing-mode bytes, optional alternate direction/mode, then signed X/Y pixel-offset pairs indexed by body animation frame.</summary>
    public required int[] DrawingData { get; init; }
}
