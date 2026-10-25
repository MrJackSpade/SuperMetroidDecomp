using System.Text.Json;
using System.Text.Json.Serialization;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Displayed swing-frame selection only; physical body placement remains compiled separately.</summary>
public sealed class GrappleSwingFrameCatalog
{
    /// <summary>Copied edited angle mapping; null retains the compiled nearest-eight-angle rule.</summary>
    internal readonly byte[]? frames;
    /// <summary>Stores a separate frame table only when an edited angle differs from the stock selector.</summary>
    private GrappleSwingFrameCatalog(byte[] frames)
    {
        for (int angle = 0; angle < frames.Length; angle++)
            if (frames[angle] != GrappleSwingFrameDefinitions.FrameForAngle(new ByteAngle((byte)angle)))
            { this.frames = frames; return; }
    }
    /// <summary>Selects the editable displayed orientation frame for one high angle byte, independently of the compiled body-placement frame and the runtime's separate straight-down animation handling.</summary>
    /// <param name="angle">High byte of the mirrored native swing angle, representing one of 256 positions around a full turn.</param>
    /// <returns>A displayed orientation frame index from 0 through 31, within the current facing's swing artwork.</returns>
    public byte Resolve(ByteAngle angle) => frames is null
        ? GrappleSwingFrameDefinitions.FrameForAngle(angle) : frames[angle.RawValue];
    /// <summary>Strict camel-case JSON settings for the editable 256-angle document.</summary>
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
            document.Frames.Any(frame => frame is < 0 or >= GrappleSwingFrameDefinitions.FrameCount))
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
