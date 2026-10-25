using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Immutable Grapple visual attributes; cadence, placement, geometry and physics are not editable here.</summary>
public sealed class GrappleSpriteCatalog
{
    /// <summary>Independent rope-style words, absent when all four frames match calculated stock.</summary>
    internal readonly ushort[]? segments;
    /// <summary>Selected small-OBJ endpoint attribute word used by both connected and unconnected drawing paths; native $94:B13D/$B17D supplies tile $20, palette 5 and priority 3.</summary>
    public int Endpoint { get; }
    /// <summary>Retains the selected endpoint and only the rope frame styles that depart from stock.</summary>
    private GrappleSpriteCatalog(int endpoint, ushort[] segments)
    {
        Endpoint = endpoint;
        for (int frame = 0; frame < segments.Length; frame++)
            if (segments[frame] != GrappleSpriteDefinitions.StockSegment(frame))
            { this.segments = segments; return; }
    }
    /// <summary>Resolves one rope animation frame's selected OBJ attributes without advancing any segment's independent gameplay timer.</summary>
    /// <param name="frame">Zero-based frame 0..3 in native $94:B18B..B19A record order.</param>
    /// <returns>The complete tile number, palette, priority and authored flip flags; drawing additionally ORs angle-derived flips into this word.</returns>
    /// <exception cref="InvalidDataException"><paramref name="frame"/> is outside the four segment appearances.</exception>
    public ushort Segment(int frame)
        => (uint)frame < GrappleSpriteDefinitions.SegmentFrameCount
            ? segments is null ? GrappleSpriteDefinitions.StockSegment(frame) : segments[frame]
            : throw new InvalidDataException($"Invalid Grapple visual frame {frame}.");

    /// <summary>Strict camel-case JSON settings shared by grapple sprite import and export.</summary>
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    /// <summary>Validates endpoint and four rope appearances and compiles independent OBJ attribute words, preserving authored differences from stock.</summary>
    /// <param name="json">Case-sensitive UTF-8 JSON read from its current position; the caller retains ownership of the stream.</param>
    /// <returns>An immutable appearance catalog that does not retain mutable document arrays or control rope geometry, cadence or physics.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="InvalidDataException">JSON is malformed or contains duplicate/unknown properties, the version or segment count is wrong, or a style is null or has invalid tile, palette or priority values.</exception>
    public static GrappleSpriteCatalog Load(Stream json)
    {
        GrappleSpriteDocument document;
        try
        {
            using var parsed = JsonDocument.Parse(json);
            RejectDuplicates(parsed.RootElement);
            document = parsed.RootElement.Deserialize<GrappleSpriteDocument>(Options)
                ?? throw new InvalidDataException("Grapple sprite document is null.");
        }
        catch (JsonException error) { throw new InvalidDataException("Invalid Grapple sprite JSON.", error); }
        if (document.Version != GrappleSpriteDefinitions.Version || document.Segments is null ||
            document.Segments.Length != GrappleSpriteDefinitions.SegmentAttributeAddresses.Length)
            throw new InvalidDataException("Grapple sprites require version 1 and exactly four segment appearances.");
        return new(Compile(document.Endpoint), document.Segments.Select(Compile).ToArray());
    }

    /// <summary>Validates an authored tile, palette, priority, and flip style into an OBJ attribute word.</summary>
    private static ushort Compile(GrappleSpriteStyle? style)
    {
        if (style is null || style.TileColumn is < 0 or >= ProjectileSpriteDefinitions.TileColumns || style.TileRow is < 0 or >= ProjectileSpriteDefinitions.TileRows ||
            style.Palette is < 0 or > 7 || style.Priority is < 0 or > 3)
            throw new InvalidDataException("Invalid Grapple tile, palette or priority.");
        return SnesObjAttributeWord.Create(style.TileRow * ProjectileSpriteDefinitions.TileColumns + style.TileColumn, style.Palette, style.Priority,
            (style.FlipX ? TileFlip.Horizontal : 0) | (style.FlipY ? TileFlip.Vertical : 0)).Raw;
    }

    /// <summary>Serializes and validates all five appearances using the same strict schema as <see cref="Load"/>.</summary>
    /// <param name="document">Authored endpoint and segment styles read for serialization, not retained by the writer.</param>
    /// <returns>A new caller-owned UTF-8 JSON byte array with camel-case properties and indented formatting.</returns>
    /// <exception cref="InvalidDataException">The serialized document fails version, segment-count or appearance validation.</exception>
    public static byte[] Write(GrappleSpriteDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, Options);
        _ = Load(new MemoryStream(bytes));
        return bytes;
    }

    /// <summary>Rejects duplicate style keys before JSON deserialization can overwrite them.</summary>
    private static void RejectDuplicates(JsonElement value) =>
        JsonAssetDocument.RejectDuplicateProperties(value, StringComparer.Ordinal,
            name => new InvalidDataException("Duplicate Grapple sprite property."));
}
