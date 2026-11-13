using SuperMetroid.Core.Spatial;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Immutable Grapple visual attributes; cadence, placement, geometry and physics are not editable here.</summary>
public sealed class InstalledGrappleSprites
{
    /// <summary>The installed appearance of each of the rope's four timed frames (#1596).</summary>
    private readonly SpritePart[] segments;
    /// <summary>Selected single-piece endpoint appearance used by both connected and unconnected drawing paths.</summary>
    public SpritePart Endpoint { get; }
    /// <summary>Keeps the installed endpoint and every rope frame's installed appearance.</summary>
    private InstalledGrappleSprites(SpritePart endpoint, SpritePart[] segments)
    {
        Endpoint = endpoint;
        this.segments = segments;
    }
    /// <summary>Resolves one rope animation frame's selected appearance without advancing any segment's independent gameplay timer.</summary>
    /// <param name="frame">Zero-based frame 0..3 in native $94:B18B..B19A record order.</param>
    /// <returns>The character, palette, priority and authored mirroring; drawing additionally mirrors it by the rope's angle.</returns>
    /// <exception cref="InvalidDataException"><paramref name="frame"/> is outside the four segment appearances.</exception>
    public SpritePart Segment(int frame)
        => (uint)frame < GrappleSpriteDefinitions.SegmentFrameCount
            ? segments[frame]
            : throw new InvalidDataException($"Invalid Grapple visual frame {frame}.");

    /// <summary>Strict camel-case JSON settings shared by grapple sprite import and export.</summary>
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    /// <summary>Validates the endpoint and four rope appearances and keeps each as installed.</summary>
    /// <param name="json">Case-sensitive UTF-8 JSON read from its current position; the caller retains ownership of the stream.</param>
    /// <returns>An immutable appearance catalog that does not retain mutable document arrays or control rope geometry, cadence or physics.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="InvalidDataException">JSON is malformed or contains duplicate/unknown properties, the version or segment count is wrong, or a style is null or has invalid tile, palette or priority values.</exception>
    public static InstalledGrappleSprites Load(Stream json)
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
            document.Segments.Length != GrappleSpriteDefinitions.SegmentFrameCount)
            throw new InvalidDataException("Grapple sprites require version 1 and exactly four segment appearances.");
        return new(Compile(document.Endpoint), document.Segments.Select(Compile).ToArray());
    }

    /// <summary>Validates an authored tile, palette, priority, and flip style into a single-piece appearance.</summary>
    private static SpritePart Compile(GrappleSpriteStyle? style)
    {
        if (style is null || style.TileColumn is < 0 or >= ProjectileSpriteDefinitions.TileColumns || style.TileRow is < 0 or >= ProjectileSpriteDefinitions.TileRows ||
            style.Palette is < 0 or > 7 || style.Priority is < 0 or > 3)
            throw new InvalidDataException("Invalid Grapple tile, palette or priority.");
        return SpritePart.Single(style.TileRow * ProjectileSpriteDefinitions.TileColumns + style.TileColumn, style.Palette, style.Priority,
            (style.FlipX ? PictureFlip.Horizontal : 0) | (style.FlipY ? PictureFlip.Vertical : 0));
    }

    /// <summary>Serializes all five appearances in the schema <see cref="Load"/> reads.</summary>
    /// <param name="document">Authored endpoint and segment styles read for serialization, not retained by the writer.</param>
    /// <returns>A new caller-owned UTF-8 JSON byte array with camel-case properties and indented formatting.</returns>
    public static byte[] Write(GrappleSpriteDocument document) =>
        JsonSerializer.SerializeToUtf8Bytes(document, Options);

    /// <summary>Rejects duplicate style keys before JSON deserialization can overwrite them.</summary>
    private static void RejectDuplicates(JsonElement value) =>
        JsonAssetDocument.RejectDuplicateProperties(value, StringComparer.Ordinal,
            name => new InvalidDataException("Duplicate Grapple sprite property."));
}
