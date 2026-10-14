namespace SuperMetroid.Core.Assets;

/// <summary>Editable JSON schema shared by beam-charge and Grapple visual flare placements; the importing resource determines source kind, and the engine retains animation and physical-origin rules.</summary>
public sealed record ChargeFlarePlacementDocument
{
    /// <summary>Schema revision, which must equal <see cref="ChargeFlarePlacementDefinitions.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>All 32 non-null offsets keyed <c>standing-00</c> through <c>standing-15</c> and <c>running-00</c> through <c>running-15</c>, using two-digit decimal direction selectors and independently editable signed 16-bit coordinates.</summary>
    public required Dictionary<string, ChargeFlareOffset> Offsets { get; init; }
}
