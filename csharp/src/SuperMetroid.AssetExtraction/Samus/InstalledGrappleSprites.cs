using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Spatial;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Immutable Grapple visual attributes; cadence, placement, geometry and physics are not editable here.</summary>
public sealed class InstalledGrappleSprites
{
    /// <summary>The installed appearance of each of the rope's four pictures (#1596, #2565), keyed by the picture.</summary>
    private readonly Dictionary<GrappleRopePicture, SpritePart> segments;
    /// <summary>Selected single-piece endpoint appearance used by both connected and unconnected drawing paths.</summary>
    public SpritePart Endpoint { get; }
    /// <summary>Keeps the installed endpoint and every rope frame's installed appearance.</summary>
    private InstalledGrappleSprites(SpritePart endpoint, Dictionary<GrappleRopePicture, SpritePart> segments)
    {
        Endpoint = endpoint;
        this.segments = segments;
    }
    /// <summary>Resolves one rope picture's selected appearance without advancing any segment's independent gameplay timer.</summary>
    /// <param name="picture">One of the rope's four pictures.</param>
    /// <returns>The character, palette, depth and authored mirroring; drawing additionally mirrors it by the rope's angle.</returns>
    /// <exception cref="InvalidDataException">The rope shows no such picture.</exception>
    public SpritePart Segment(GrappleRopePicture picture)
        => segments.TryGetValue(picture, out SpritePart appearance)
            ? appearance
            : throw new InvalidDataException($"Invalid Grapple rope picture {picture}.");

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
            document.Segments.Count != GrappleSheetDefinitions.RopePattern.Count)
            throw new InvalidDataException($"Grapple sprites require version {GrappleSpriteDefinitions.Version} and one appearance for each rope picture.");
        return new(Compile(document.Endpoint), GrappleSheetDefinitions.RopePattern.ToDictionary(picture => picture, picture =>
            Compile(document.Segments.TryGetValue(picture.ToString(), out GrappleSpriteStyle? style) ? style : null)));
    }

    /// <summary>Validates an authored tile, palette, depth, and flip style into a single-piece appearance.</summary>
    private static SpritePart Compile(GrappleSpriteStyle? style)
    {
        if (style is null || style.TileColumn is < 0 or >= ProjectileSpriteDefinitions.TileColumns || style.TileRow is < 0 or >= ProjectileSpriteDefinitions.TileRows ||
            style.Palette is < 0 or > 7)
            throw new InvalidDataException("Invalid Grapple tile or palette.");
        return SpritePart.Single(style.TileRow * ProjectileSpriteDefinitions.TileColumns + style.TileColumn, style.Palette, style.Depth,
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
