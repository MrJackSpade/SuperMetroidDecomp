using System.Text.Json;
using System.Text.Json.Serialization;

namespace SuperMetroid.Core.Assets;

/// <summary>Displayed swing-frame selection only; physical body placement remains compiled separately.</summary>
public sealed class GrappleSwingFrameCatalog
{
    private readonly byte[]? frames;
    private GrappleSwingFrameCatalog(byte[] frames)
    {
        for (int angle = 0; angle < frames.Length; angle++)
            if (frames[angle] != GrappleSwingFrameDefinitions.FrameForAngle((byte)angle))
            { this.frames = frames; return; }
    }
    /// <summary>Selects the editable displayed orientation frame for one high angle byte, independently of the compiled body-placement frame and the runtime's separate straight-down animation handling.</summary>
    /// <param name="angle">High byte of the mirrored native swing angle, representing one of 256 positions around a full turn.</param>
    /// <returns>A displayed orientation frame index from 0 through 31, within the current facing's swing artwork.</returns>
    public byte Resolve(byte angle) => frames is null
        ? GrappleSwingFrameDefinitions.FrameForAngle(angle) : frames[angle];
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };
    /// <summary>Loads the supported JSON schema, rejecting duplicate or unknown top-level properties and requiring exactly 256 displayed frame indices, each in 0..31; copies the supplied indices without exposing mutable document storage.</summary>
    /// <param name="json">UTF-8 JSON stream containing the complete angle-to-displayed-frame mapping.</param>
    /// <returns>The immutable visual swing-frame catalog.</returns>
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
    /// <summary>Serializes a swing-frame document as indented camel-case UTF-8 JSON and validates it through <see cref="Load"/> before returning the bytes.</summary>
    /// <param name="document">Document containing the supported version and all 256 valid orientation-frame selections.</param>
    /// <returns>Validated JSON bytes suitable for the swing-frame asset file.</returns>
    public static byte[] Write(GrappleSwingFrameDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, Options);
        _ = Load(new MemoryStream(bytes));
        return bytes;
    }
}

/// <summary>Editable JSON schema for displayed Grapple swing orientations; physical body offsets, facing selection, and animation timing remain engine-owned.</summary>
public sealed record GrappleSwingFrameDocument
{
    /// <summary>Schema revision, which must equal <see cref="GrappleSwingFrameDefinitions.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Exactly 256 orientation-frame indices in 0..31, indexed by the high byte of the mirrored native swing angle, corresponding to visual selections at <c>$9B:C1C2-C2C1</c>.</summary>
    public required int[] Frames { get; init; }
}

/// <summary>Native displayed-frame identities, independent of the physical offset table.</summary>
public static class GrappleSwingFrameDefinitions
{
    /// <summary>Asset filename for the editable angle-to-displayed-orientation mapping.</summary>
    public const string FileName = "grapple-swing-frames.json";
    /// <summary>Supported revision of the 256-entry displayed-frame JSON schema.</summary>
    public const int Version = 1;
    /// <summary>$9B:C1C2..C2C1: one displayed animation frame for each high angle byte.</summary>
    public const int AngleCount = 256;
    /// <summary>$9B:BD95: Grapple swing art has 32 orientation frames per facing.</summary>
    public const int FrameCount = 32;
    /// <summary>$9B:C1C2 angle selector rounds to the nearest eight-angle frame, wrapping at 32.</summary>
    public static byte FrameForAngle(byte angle) => (byte)(((angle + 4) >> 3) & (FrameCount - 1));
}
