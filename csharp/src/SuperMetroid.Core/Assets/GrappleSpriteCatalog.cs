using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Immutable Grapple visual attributes; cadence, placement, geometry and physics are not editable here.</summary>
public sealed class GrappleSpriteCatalog
{
    /// <summary>Compiled segment words, retained only when at least one authored appearance differs from stock.</summary>
    private readonly ushort[]? segments;
    /// <summary>Selected small-OBJ endpoint attribute word used by both connected and unconnected drawing paths; native $94:B13D/$B17D supplies tile $20, palette 5 and priority 3.</summary>
    public ushort Endpoint { get; }
    /// <summary>Creates the runtime catalog and omits the segment array when every compiled word matches stock.</summary>
    /// <param name="endpoint">Compiled OBJ attribute word shared by connected and unconnected endpoint drawing.</param>
    /// <param name="segments">Compiled OBJ attribute words in native timed-record order for the four rope frames.</param>
    private GrappleSpriteCatalog(ushort endpoint, ushort[] segments)
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

    /// <summary>Strict camel-case serializer settings shared by grapple appearance loading and writing.</summary>
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

    /// <summary>Validates one editable OBJ appearance and packs its tile, palette, priority, and flip flags.</summary>
    /// <param name="style">Appearance fields to encode, or null when the document contains a missing style.</param>
    /// <returns>The native OBJ attribute word used by the grapple drawing paths.</returns>
    /// <exception cref="InvalidDataException">The style is null or its tile, palette, or priority values fall outside the hardware-supported range.</exception>
    private static ushort Compile(GrappleSpriteStyle? style)
    {
        if (style is null || style.TileColumn is < 0 or >= ProjectileSpriteDefinitions.TileColumns || style.TileRow is < 0 or >= ProjectileSpriteDefinitions.TileRows ||
            style.Palette is < 0 or > 7 || style.Priority is < 0 or > 3)
            throw new InvalidDataException("Invalid Grapple tile, palette or priority.");
        return SnesObjAttributeWord.Create(style.TileRow * ProjectileSpriteDefinitions.TileColumns + style.TileColumn, style.Palette, style.Priority,
            (style.FlipX ? SnesTileFlipFlags.Horizontal : 0) | (style.FlipY ? SnesTileFlipFlags.Vertical : 0)).Raw;
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

    /// <summary>Rejects repeated JSON property names throughout a parsed grapple appearance document.</summary>
    /// <param name="value">Parsed JSON value whose objects are checked using ordinal, case-sensitive name comparison.</param>
    /// <exception cref="InvalidDataException">An object in the document contains a duplicate property name.</exception>
    private static void RejectDuplicates(JsonElement value) =>
        JsonAssetDocument.RejectDuplicateProperties(value, StringComparer.Ordinal,
            name => new InvalidDataException("Duplicate Grapple sprite property."));
}

/// <summary>Editable appearance-only schema for the single grapple endpoint and four timed rope styles; the segment array remains caller-mutable until compilation.</summary>
public sealed record GrappleSpriteDocument
{
    /// <summary>Schema revision; loading requires <see cref="GrappleSpriteDefinitions.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Nonnull endpoint appearance shared by connected and unconnected drawing; its position remains owned by the live grapple state.</summary>
    public required GrappleSpriteStyle Endpoint { get; init; }
    /// <summary>Exactly four nonnull appearances in native $94:B18D/$B191/$B195/$B199 attribute-operand order; no duration or instruction fields are editable here.</summary>
    public required GrappleSpriteStyle[] Segments { get; init; }
}

/// <summary>Only the appearance of a native eight-pixel OBJ; no simulation fields.</summary>
public sealed record GrappleSpriteStyle
{
    /// <summary>Zero-based column 0..15 in the gameplay OBJ tile layout, not a position in the grapple atlas or on screen.</summary>
    public required int TileColumn { get; init; }
    /// <summary>Zero-based row 0..31 in the gameplay OBJ tile layout; row * 16 + column selects the complete nine-bit character number.</summary>
    public required int TileRow { get; init; }
    /// <summary>OBJ palette selector 0..7 packed into bits 9..11; stock endpoint and rope styles use 5.</summary>
    public required int Palette { get; init; }
    /// <summary>OBJ priority tier 0..3 packed into bits 12..13; stock endpoint and rope styles use 3.</summary>
    public required int Priority { get; init; }
    /// <summary>Authored horizontal reflection in OBJ bit 14; rope drawing can additionally set this bit from its angle.</summary>
    public required bool FlipX { get; init; }
    /// <summary>Authored vertical reflection in OBJ bit 15; rope drawing can additionally set this bit from its angle.</summary>
    public required bool FlipY { get; init; }
}
