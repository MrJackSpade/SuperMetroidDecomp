using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Immutable visual muzzle offsets; never used for projectile physics or Grapple connection.</summary>
public sealed class ChargeFlarePlacements
{
    /// <summary>Import-selected source kind; placement values themselves cannot distinguish Grapple from beam art.</summary>
    private readonly bool grapple;
    /// <summary>Only authored offsets differing from the selected native source definitions.</summary>
    internal readonly Dictionary<int, Offset16<RoomPixels>> offsets = new();
    /// <summary>Copies changed visual offsets while retaining the beam or Grapple fallback contract.</summary>
    private ChargeFlarePlacements(Offset16<RoomPixels>[] supplied, bool grapple)
    {
        this.grapple = grapple;
        for (int index = 0; index < supplied.Length; index++)
            if (supplied[index] != CalculatedOffset(index >= ChargeFlarePlacementDefinitions.DirectionCount,
                index % ChargeFlarePlacementDefinitions.DirectionCount)) offsets.Add(index, supplied[index]);
    }
    /// <summary>Rejects unknown JSON properties while writing a stable camel-case authoring document.</summary>
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
    public Offset16<RoomPixels> Resolve(bool running, int direction)
    {
        if ((uint)direction >= ChargeFlarePlacementDefinitions.DirectionCount) throw new ArgumentOutOfRangeException(nameof(direction));
        return offsets.TryGetValue((running ? ChargeFlarePlacementDefinitions.DirectionCount : 0) + direction, out var supplied)
            ? supplied : CalculatedOffset(running, direction);
    }
    /// <summary>Gets the native fallback from the beam or Grapple definitions chosen at import.</summary>
    private Offset16<RoomPixels> CalculatedOffset(bool running, int direction) => grapple
        ? ChargeFlarePlacementOffsets.GrappleOffset(running, direction)
        : ChargeFlarePlacementOffsets.BeamOffset(running, direction);

    /// <summary>Loads beam-charge flare placements corresponding to native <c>$90:C1A8-C203</c>, requiring the supported version and all 32 standing/running keys while rejecting duplicate or unknown JSON properties.</summary>
    /// <param name="json">UTF-8 JSON stream containing signed 16-bit X/Y offsets for every direction in both modes.</param>
    /// <returns>The immutable beam-flare placement catalog; use <see cref="LoadGrapple"/> for Grapple's distinct adjacent-row source contract.</returns>
    public static ChargeFlarePlacements Load(Stream json) => Load(json, ChargeFlareSource.Beam);
    /// <summary>Imports the Grapple placement resource with its distinct adjacent-row owners.</summary>
    /// <param name="json">UTF-8 JSON stream containing the same 32-key placement schema for Grapple flare origins.</param>
    /// <returns>The immutable placement catalog interpreted with Grapple's native origin and adjacent-row definitions.</returns>
    public static ChargeFlarePlacements LoadGrapple(Stream json) => Load(json, ChargeFlareSource.Grapple);
    /// <summary>Validates all standing/running direction keys and binds their offsets to the import-selected source kind.</summary>
    private static ChargeFlarePlacements Load(Stream json, ChargeFlareSource source)
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
        var offsets = new Offset16<RoomPixels>[count * 2];
        for (int mode = 0; mode < 2; mode++)
        for (int direction = 0; direction < count; direction++)
        {
            string key = ChargeFlarePlacementOffsets.Key(mode != 0, direction);
            if (!document.Offsets.TryGetValue(key, out var value) || value is null)
                throw new InvalidDataException($"Missing charge-flare placement {key}.");
            offsets[mode * count + direction] = value.Offset;
        }
        return new(offsets, source == ChargeFlareSource.Grapple);
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
    /// <summary>Rejects duplicate top-level or nested object keys before deserialization can overwrite them.</summary>
    private static void ValidateUnique(JsonElement element) =>
        JsonAssetDocument.RejectDuplicateProperties(element, StringComparer.Ordinal,
            name => new InvalidDataException("Duplicate charge-flare placement property."), DuplicatePropertyScope.OutsideArrays);
}
