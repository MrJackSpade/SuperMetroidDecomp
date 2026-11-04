using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Reads and writes the installed beam-charge and Grapple flare placement documents (#1502).</summary>
public static class ChargeFlarePlacementFile
{
    /// <summary>
    /// Required revision of the beam and Grapple placement schemas. Revision two holds the ten named directions of each
    /// movement mode and the beam flare's three turning placements; revision one also held the selections past them,
    /// which no pose produces (#1500).
    /// </summary>
    public const int Version = 2;

    /// <summary>Formats the exact JSON identity for one movement-mode/direction placement; direction range validation belongs to catalog loading or lookup, not this formatter.</summary>
    /// <param name="running">True for the running visual-origin table; false for standing.</param>
    /// <param name="direction">A named aiming direction or, standing, a turning placement; formatted as two decimal digits rather than hexadecimal.</param>
    /// <returns>A key such as <c>standing-00</c>, <c>standing-12</c> or <c>running-09</c>.</returns>
    public static string Key(bool running, int direction) => $"{(running ? "running" : "standing")}-{direction:D2}";

    /// <summary>Rejects unknown JSON properties while writing a stable camel-case authoring document.</summary>
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    /// <summary>Loads beam-charge flare placements, requiring the supported version and every standing, turning and running key while rejecting duplicate or unknown JSON properties.</summary>
    /// <param name="json">UTF-8 JSON stream containing signed 16-bit X/Y offsets for every direction in both modes.</param>
    /// <returns>The immutable beam-flare placement catalog; use <see cref="LoadGrapple"/> for Grapple's source contract.</returns>
    public static ChargeFlarePlacements Load(Stream json) => Load(json, ChargeFlareSource.Beam);

    /// <summary>Imports the Grapple placement resource: the ten named directions of each movement mode.</summary>
    /// <param name="json">UTF-8 JSON stream containing the twenty-key placement schema for Grapple flare origins.</param>
    /// <returns>The immutable placement catalog interpreted with Grapple's origin definitions.</returns>
    public static ChargeFlarePlacements LoadGrapple(Stream json) => Load(json, ChargeFlareSource.Grapple);

    /// <summary>Validates every standing and running direction key of the source and binds their offsets to the import-selected source kind.</summary>
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
        // Deserialization runs each offset's signed-word range check in its setter, so a document
        // holding an offset outside that range surfaces here as the setter's rejection.
        catch (ArgumentOutOfRangeException error)
        {
            throw new InvalidDataException("A charge-flare placement offset is outside the signed 16-bit range.", error);
        }
        int keys = ChargeFlarePlacements.DirectionsFor(source, running: false) + ChargeFlarePlacements.DirectionsFor(source, running: true);
        if (document.Version != ChargeFlarePlacementFile.Version || document.Offsets is null || document.Offsets.Count != keys)
            throw new InvalidDataException($"Charge-flare placement requires version {ChargeFlarePlacementFile.Version} and all standing/running directions.");
        var offsets = new Dictionary<int, Offset16<RoomPixels>>();
        foreach (bool running in (bool[])[false, true])
        for (int direction = 0; direction < ChargeFlarePlacements.DirectionsFor(source, running); direction++)
        {
            string key = ChargeFlarePlacementFile.Key(running, direction);
            if (!document.Offsets.TryGetValue(key, out var value) || value is null)
                throw new InvalidDataException($"Missing charge-flare placement {key}.");
            offsets.Add(ChargeFlarePlacements.Slot(running, direction), value.Offset);
        }
        return new(offsets, source);
    }

    /// <summary>Serializes a beam placement document as indented camel-case UTF-8 JSON.</summary>
    /// <param name="document">Document containing every standing, turning and running direction placement.</param>
    /// <returns>Placement JSON bytes.</returns>
    public static byte[] Write(ChargeFlarePlacementDocument document) => JsonSerializer.SerializeToUtf8Bytes(document, Options);

    /// <summary>Serializes a Grapple placement document as indented camel-case UTF-8 JSON.</summary>
    /// <param name="document">Document containing every standing and running direction placement.</param>
    /// <returns>Placement JSON bytes.</returns>
    public static byte[] WriteGrapple(ChargeFlarePlacementDocument document) => JsonSerializer.SerializeToUtf8Bytes(document, Options);

    /// <summary>Rejects duplicate top-level or nested object keys before deserialization can overwrite them.</summary>
    private static void ValidateUnique(JsonElement element) =>
        JsonAssetDocument.RejectDuplicateProperties(element, StringComparer.Ordinal,
            name => new InvalidDataException("Duplicate charge-flare placement property."), DuplicatePropertyScope.OutsideArrays);
}
