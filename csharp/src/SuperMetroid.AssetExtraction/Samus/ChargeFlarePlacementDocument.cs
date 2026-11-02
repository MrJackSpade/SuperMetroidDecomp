
namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Editable JSON schema shared by beam-charge and Grapple visual flare placements; the importing resource determines source kind, and the engine retains animation and physical-origin rules.</summary>
public sealed record ChargeFlarePlacementDocument
{
    /// <summary>Schema revision, which must equal <see cref="ChargeFlarePlacementFile.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Every non-null offset keyed <c>standing-00</c> through <c>standing-09</c> and <c>running-00</c> through <c>running-09</c>, plus the beam flare's turning placements <c>standing-10</c> through <c>standing-12</c>, using two-digit decimal direction selectors and independently editable signed 16-bit coordinates.</summary>
    public required Dictionary<string, ChargeFlareOffset> Offsets { get; init; }
}
