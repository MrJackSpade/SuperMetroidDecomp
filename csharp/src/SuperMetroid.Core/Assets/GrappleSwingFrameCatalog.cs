using System.Text.Json;
using System.Text.Json.Serialization;

namespace SuperMetroid.Core.Assets;

/// <summary>Displayed swing-frame selection only; physical body placement remains compiled separately.</summary>
public sealed class GrappleSwingFrameCatalog
{
    private readonly byte[] frames;
    private GrappleSwingFrameCatalog(byte[] frames) => this.frames = frames;
    public byte Resolve(byte angle) => frames[angle];
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };
    public static GrappleSwingFrameCatalog Load(Stream json)
    {
        GrappleSwingFrameDocument document;
        try
        {
            using var parsed = JsonDocument.Parse(json);
            if (parsed.RootElement.ValueKind != JsonValueKind.Object) throw new InvalidDataException("Grapple swing frames require an object.");
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (var property in parsed.RootElement.EnumerateObject())
                if (!names.Add(property.Name)) throw new InvalidDataException("Duplicate Grapple swing-frame property.");
            document = parsed.RootElement.Deserialize<GrappleSwingFrameDocument>(Options)
                ?? throw new InvalidDataException("Missing Grapple swing frames.");
        }
        catch (JsonException error) { throw new InvalidDataException("Invalid Grapple swing-frame JSON.", error); }
        if (document.Version != GrappleSwingFrameDefinitions.Version || document.Frames is null || document.Frames.Length != GrappleSwingFrameDefinitions.AngleCount ||
            document.Frames.Any(frame => frame < 0 || frame >= GrappleSwingFrameDefinitions.FrameCount))
            throw new InvalidDataException("Grapple swing frames require version 1 and 256 frame indices in 0..31.");
        return new(document.Frames.Select(frame => (byte)frame).ToArray());
    }
    public static byte[] Write(GrappleSwingFrameDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, Options);
        _ = Load(new MemoryStream(bytes));
        return bytes;
    }
}

public sealed record GrappleSwingFrameDocument
{
    public required int Version { get; init; }
    public required int[] Frames { get; init; }
}

/// <summary>Native displayed-frame identities, independent of the physical offset table.</summary>
public static class GrappleSwingFrameDefinitions
{
    public const string FileName = "grapple-swing-frames.json";
    public const int Version = 1;
    /// <summary>$9B:C1C2..C2C1: one displayed animation frame for each high angle byte.</summary>
    public const int AngleCount = 256;
    /// <summary>$9B:BD95: Grapple swing art has 32 orientation frames per facing.</summary>
    public const int FrameCount = 32;
}
