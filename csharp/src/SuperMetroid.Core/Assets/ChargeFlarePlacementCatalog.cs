using System.Text.Json;
using System.Text.Json.Serialization;

namespace SuperMetroid.Core.Assets;

/// <summary>Immutable visual muzzle offsets; never used for projectile physics or Grapple connection.</summary>
public sealed class ChargeFlarePlacementCatalog
{
    // Source kind comes from the importing file contract, never from supplied pixel/offset values.
    private readonly bool grapple;
    private readonly Dictionary<int, ChargeFlareOffset> offsets = new();
    private ChargeFlarePlacementCatalog(ChargeFlareOffset[] supplied, bool grapple)
    {
        this.grapple = grapple;
        for (int index = 0; index < supplied.Length; index++)
            if (supplied[index] != CalculatedOffset(index >= ChargeFlarePlacementDefinitions.DirectionCount,
                index % ChargeFlarePlacementDefinitions.DirectionCount)) offsets.Add(index, supplied[index]);
    }
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };
    /// <summary>Resolves an independently editable visual muzzle offset for the requested movement mode and full native low-nibble direction domain, using the beam or Grapple source contract selected at import.</summary>
    /// <param name="running">True for running placement; false for the standing/default row.</param>
    /// <param name="direction">Direction selector 0..15, including adjacent-row selections beyond the named aiming directions.</param>
    /// <returns>The signed whole-pixel placement offset, without camera or pose-graphics Y adjustments.</returns>
    public ChargeFlareOffset Resolve(bool running, int direction)
    {
        if ((uint)direction >= ChargeFlarePlacementDefinitions.DirectionCount) throw new ArgumentOutOfRangeException(nameof(direction));
        return offsets.TryGetValue((running ? ChargeFlarePlacementDefinitions.DirectionCount : 0) + direction, out var supplied)
            ? supplied : CalculatedOffset(running, direction);
    }
    private ChargeFlareOffset CalculatedOffset(bool running, int direction) => grapple
        ? ChargeFlarePlacementDefinitions.GrappleOffset(running, direction)
        : ChargeFlarePlacementDefinitions.BeamOffset(running, direction);

    /// <summary>Loads beam-charge flare placements corresponding to native <c>$90:C1A8-C203</c>, requiring the supported version and all 32 standing/running keys while rejecting duplicate or unknown JSON properties.</summary>
    /// <param name="json">UTF-8 JSON stream containing signed 16-bit X/Y offsets for every direction in both modes.</param>
    /// <returns>The immutable beam-flare placement catalog; use <see cref="LoadGrapple"/> for Grapple's distinct adjacent-row source contract.</returns>
    public static ChargeFlarePlacementCatalog Load(Stream json) => Load(json, false);
    /// <summary>Imports the Grapple placement resource with its distinct adjacent-row owners.</summary>
    /// <param name="json">UTF-8 JSON stream containing the same 32-key placement schema for Grapple flare origins.</param>
    /// <returns>The immutable placement catalog interpreted with Grapple's native origin and adjacent-row definitions.</returns>
    public static ChargeFlarePlacementCatalog LoadGrapple(Stream json) => Load(json, true);
    private static ChargeFlarePlacementCatalog Load(Stream json, bool grapple)
    {
        ChargeFlarePlacementDocument document;
        try
        {
            using var parsed = JsonDocument.Parse(json);
            ValidateUnique(parsed.RootElement);
            document = parsed.RootElement.Deserialize<ChargeFlarePlacementDocument>(Options)
                ?? throw new InvalidDataException("Missing charge-flare placement document.");
        }
        catch (JsonException error) { throw new InvalidDataException("Invalid charge-flare placement JSON.", error); }
        int count = ChargeFlarePlacementDefinitions.DirectionCount;
        if (document.Version != ChargeFlarePlacementDefinitions.Version || document.Offsets is null || document.Offsets.Count != count * 2)
            throw new InvalidDataException("Charge-flare placement requires version 1 and all standing/running directions.");
        var offsets = new ChargeFlareOffset[count * 2];
        for (int mode = 0; mode < 2; mode++)
        for (int direction = 0; direction < count; direction++)
        {
            string key = ChargeFlarePlacementDefinitions.Key(mode != 0, direction);
            if (!document.Offsets.TryGetValue(key, out var value) || value is null)
                throw new InvalidDataException($"Missing charge-flare placement {key}.");
            offsets[mode * count + direction] = value;
        }
        return new(offsets, grapple);
    }
    /// <summary>Serializes the shared placement document as indented camel-case UTF-8 JSON and validates its version, complete key set, and signed offsets through the beam loader before returning it; the document does not encode its beam/Grapple import kind.</summary>
    /// <param name="document">Document containing every standing and running direction placement.</param>
    /// <returns>Validated placement JSON bytes.</returns>
    public static byte[] Write(ChargeFlarePlacementDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, Options);
        _ = Load(new MemoryStream(bytes));
        return bytes;
    }
    private static void ValidateUnique(JsonElement element) =>
        JsonAssetDocument.RejectDuplicateProperties(element, StringComparer.Ordinal,
            name => new InvalidDataException("Duplicate charge-flare placement property."), descendArrays: false);
}

/// <summary>Signed whole-pixel visual muzzle displacement from Samus's render center, applied before camera subtraction and the renderer's separate pose-graphics Y adjustment.</summary>
public sealed record ChargeFlareOffset
{
    /// <summary>Horizontal displacement in signed pixels; positive values move the visual origin right.</summary>
    public required short X { get; init; }
    /// <summary>Vertical displacement in signed pixels; positive values move the visual origin down, before the separate pose-graphics Y offset is subtracted.</summary>
    public required short Y { get; init; }
}
/// <summary>Editable JSON schema shared by beam-charge and Grapple visual flare placements; the importing resource determines source kind, and the engine retains animation and physical-origin rules.</summary>
public sealed record ChargeFlarePlacementDocument
{
    /// <summary>Schema revision, which must equal <see cref="ChargeFlarePlacementDefinitions.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>All 32 non-null offsets keyed <c>standing-00</c> through <c>standing-15</c> and <c>running-00</c> through <c>running-15</c>, using two-digit decimal direction selectors and independently editable signed 16-bit coordinates.</summary>
    public required Dictionary<string, ChargeFlareOffset> Offsets { get; init; }
}
