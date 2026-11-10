using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Reads and writes the installed beam-charge and Grapple flare placement documents (#1502).</summary>
public static class ChargeFlarePlacementFile
{
    /// <summary>
    /// Required revision of the beam and Grapple placement schemas. Revision three keys each placement by the name of
    /// its direction or turn (#1595); revision two keyed the ten directions of each movement mode and the beam flare's
    /// three turning placements by the cartridge's row numbers; revision one also held the selections past them, which
    /// no pose produces (#1500).
    /// </summary>
    public const int Version = 3;

    /// <summary>Formats the exact JSON identity for one placement; which placements a flare holds belongs to catalog loading, not this formatter.</summary>
    /// <param name="placement">The movement mode, and the direction whose muzzle or the turn whose placement the flare takes.</param>
    /// <returns>A key such as <c>standing-UpFacingRight</c>, <c>running-UpFacingLeft</c> or <c>standing-turning-Down</c>.</returns>
    /// <exception cref="ArgumentException">The placement names neither a muzzle nor a turn.</exception>
    internal static string Key(ChargeFlarePlacement placement)
    {
        string mode = placement.Running ? "running" : "standing";
        return placement.Aim.Muzzle is { } direction ? $"{mode}-{direction}"
            : placement.Aim.Turning is { } turning ? $"{mode}-turning-{turning}"
            : throw new ArgumentException("A flare is placed at a muzzle or while turning.", nameof(placement));
    }

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
        ChargeFlarePlacement[] placements = [.. ChargeFlarePlacements.Placements(source)];
        if (document.Version != ChargeFlarePlacementFile.Version || document.Offsets is null || document.Offsets.Count != placements.Length)
            throw new InvalidDataException($"Charge-flare placement requires version {ChargeFlarePlacementFile.Version} and all standing/running directions.");
        var offsets = new Dictionary<ChargeFlarePlacement, Offset16<RoomPixels>>();
        foreach (ChargeFlarePlacement placement in placements)
        {
            string key = ChargeFlarePlacementFile.Key(placement);
            if (!document.Offsets.TryGetValue(key, out var value) || value is null)
                throw new InvalidDataException($"Missing charge-flare placement {key}.");
            offsets.Add(placement, value.Offset);
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
