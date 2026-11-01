using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Immutable visual muzzle offsets; never used for projectile physics or Grapple connection.</summary>
public sealed class ChargeFlarePlacements
{
    /// <summary>Import-selected source kind; placement values themselves cannot distinguish Grapple from beam art.</summary>
    private readonly ChargeFlareSource source;
    /// <summary>Only authored offsets differing from the selected stock source definitions, by <see cref="Slot"/>.</summary>
    internal readonly Dictionary<int, Offset16<RoomPixels>> offsets = new();
    /// <summary>Copies changed visual offsets while retaining the beam or Grapple fallback contract.</summary>
    private ChargeFlarePlacements(IReadOnlyDictionary<int, Offset16<RoomPixels>> supplied, ChargeFlareSource source)
    {
        this.source = source;
        foreach ((int slot, Offset16<RoomPixels> offset) in supplied)
            if (offset != CalculatedOffset(slot >= ChargeFlarePlacementDefinitions.StandingDirectionCount,
                slot % ChargeFlarePlacementDefinitions.StandingDirectionCount)) offsets.Add(slot, offset);
    }
    /// <summary>Rejects unknown JSON properties while writing a stable camel-case authoring document.</summary>
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };
    /// <summary>Resolves an independently editable visual muzzle offset for the requested movement mode and direction, using the beam or Grapple source contract selected at import.</summary>
    /// <param name="running">True for running placement; false for the standing/default row.</param>
    /// <param name="direction">
    /// A named aiming direction, 0..9, or for the standing beam flare a turning placement, 10..12. No pose selects a row
    /// past them, so other selections are rejected (#1500).
    /// </param>
    /// <returns>The signed whole-pixel placement offset, without camera or pose-graphics Y adjustments.</returns>
    public Offset16<RoomPixels> Resolve(bool running, int direction)
    {
        if ((uint)direction >= DirectionsFor(source, running)) throw new ArgumentOutOfRangeException(nameof(direction));
        return offsets.TryGetValue(Slot(running, direction), out var supplied)
            ? supplied : CalculatedOffset(running, direction);
    }
    /// <summary>Gets the fallback from the beam or Grapple definitions chosen at import.</summary>
    private Offset16<RoomPixels> CalculatedOffset(bool running, int direction) => source == ChargeFlareSource.Grapple
        ? ChargeFlarePlacementOffsets.GrappleOffset(running, direction)
        : ChargeFlarePlacementOffsets.BeamOffset(running, direction);

    /// <summary>
    /// How many directions a movement mode places the flare for: the ten named aiming directions, and for the standing
    /// beam flare its turning placements too; Grapple never fires while turning.
    /// </summary>
    private static int DirectionsFor(ChargeFlareSource source, bool running) => source == ChargeFlareSource.Grapple || running
        ? ChargeFlarePlacementDefinitions.DirectionCount
        : ChargeFlarePlacementDefinitions.StandingDirectionCount;

    /// <summary>Where a movement mode's direction is kept: the standing directions first, then the running ones.</summary>
    private static int Slot(bool running, int direction) =>
        (running ? ChargeFlarePlacementDefinitions.StandingDirectionCount : 0) + direction;

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
        int keys = DirectionsFor(source, running: false) + DirectionsFor(source, running: true);
        if (document.Version != ChargeFlarePlacementDefinitions.Version || document.Offsets is null || document.Offsets.Count != keys)
            throw new InvalidDataException($"Charge-flare placement requires version {ChargeFlarePlacementDefinitions.Version} and all standing/running directions.");
        var offsets = new Dictionary<int, Offset16<RoomPixels>>();
        foreach (bool running in (bool[])[false, true])
        for (int direction = 0; direction < DirectionsFor(source, running); direction++)
        {
            string key = ChargeFlarePlacementOffsets.Key(running, direction);
            if (!document.Offsets.TryGetValue(key, out var value) || value is null)
                throw new InvalidDataException($"Missing charge-flare placement {key}.");
            offsets.Add(Slot(running, direction), value.Offset);
        }
        return new(offsets, source);
    }
    /// <summary>Serializes a beam placement document as indented camel-case UTF-8 JSON and validates its version, complete key set, and signed offsets through the beam loader before returning it.</summary>
    /// <param name="document">Document containing every standing, turning and running direction placement.</param>
    /// <returns>Validated placement JSON bytes.</returns>
    public static byte[] Write(ChargeFlarePlacementDocument document) => Write(document, ChargeFlareSource.Beam);
    /// <summary>Serializes a Grapple placement document and validates it through the Grapple loader before returning it.</summary>
    /// <param name="document">Document containing every standing and running direction placement.</param>
    /// <returns>Validated placement JSON bytes.</returns>
    public static byte[] WriteGrapple(ChargeFlarePlacementDocument document) => Write(document, ChargeFlareSource.Grapple);
    /// <summary>Serializes a placement document and validates it through the loader of <paramref name="source"/>.</summary>
    private static byte[] Write(ChargeFlarePlacementDocument document, ChargeFlareSource source)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, Options);
        _ = Load(new MemoryStream(bytes), source);
        return bytes;
    }    /// <summary>Rejects duplicate top-level or nested object keys before deserialization can overwrite them.</summary>
    private static void ValidateUnique(JsonElement element) =>
        JsonAssetDocument.RejectDuplicateProperties(element, StringComparer.Ordinal,
            name => new InvalidDataException("Duplicate charge-flare placement property."), DuplicatePropertyScope.OutsideArrays);
}
