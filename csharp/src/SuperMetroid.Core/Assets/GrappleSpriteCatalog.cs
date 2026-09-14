using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Immutable Grapple visual attributes; cadence, placement, geometry and physics are not editable here.</summary>
public sealed class GrappleSpriteCatalog
{
    private readonly ushort[] segments;
    public ushort Endpoint { get; }
    private GrappleSpriteCatalog(ushort endpoint, ushort[] segments) { Endpoint = endpoint; this.segments = segments; }
    public ushort Segment(int frame)
        => (uint)frame < segments.Length ? segments[frame] : throw new InvalidDataException($"Invalid Grapple visual frame {frame}.");

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

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

    private static ushort Compile(GrappleSpriteStyle? style)
    {
        if (style is null || style.TileColumn is < 0 or >= ProjectileSpriteDefinitions.TileColumns || style.TileRow is < 0 or >= ProjectileSpriteDefinitions.TileRows ||
            style.Palette is < 0 or > 7 || style.Priority is < 0 or > 3)
            throw new InvalidDataException("Invalid Grapple tile, palette or priority.");
        return SnesObjAttributeWord.Create(style.TileRow * ProjectileSpriteDefinitions.TileColumns + style.TileColumn, style.Palette, style.Priority,
            (style.FlipX ? SnesTileFlipFlags.Horizontal : 0) | (style.FlipY ? SnesTileFlipFlags.Vertical : 0)).Raw;
    }

    public static byte[] Write(GrappleSpriteDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, Options);
        _ = Load(new MemoryStream(bytes));
        return bytes;
    }

    private static void RejectDuplicates(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.Object)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (var property in value.EnumerateObject())
            {
                if (!names.Add(property.Name)) throw new InvalidDataException("Duplicate Grapple sprite property.");
                RejectDuplicates(property.Value);
            }
        }
        else if (value.ValueKind == JsonValueKind.Array)
            foreach (var child in value.EnumerateArray()) RejectDuplicates(child);
    }
}

public sealed record GrappleSpriteDocument
{
    public required int Version { get; init; }
    public required GrappleSpriteStyle Endpoint { get; init; }
    public required GrappleSpriteStyle[] Segments { get; init; }
}

/// <summary>Only the appearance of a native eight-pixel OBJ; no simulation fields.</summary>
public sealed record GrappleSpriteStyle
{
    public required int TileColumn { get; init; }
    public required int TileRow { get; init; }
    public required int Palette { get; init; }
    public required int Priority { get; init; }
    public required bool FlipX { get; init; }
    public required bool FlipY { get; init; }
}
