
namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Editable JSON schema shared by beam-charge and Grapple visual flare placements; the importing resource determines source kind, and the engine retains animation and physical-origin rules.</summary>
public sealed record ChargeFlarePlacementDocument
{
    /// <summary>Schema revision, which must equal <see cref="ChargeFlarePlacementFile.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Every non-null offset keyed by movement mode and direction, <c>standing-UpFacingRight</c> through <c>running-UpFacingLeft</c>, plus the beam flare's turning placements <c>standing-turning-Up</c>, <c>standing-turning-Neutral</c> and <c>standing-turning-Down</c>, with independently editable signed 16-bit coordinates.</summary>
    public required Dictionary<string, ChargeFlareOffset> Offsets { get; init; }
}
