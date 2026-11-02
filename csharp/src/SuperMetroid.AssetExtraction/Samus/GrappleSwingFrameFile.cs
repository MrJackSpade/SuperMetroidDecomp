using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Reads and writes the installed Grapple swing-frame document (#1502).</summary>
public static class GrappleSwingFrameFile
{
    /// <summary>Supported revision of the 256-entry displayed-frame JSON schema.</summary>
    public const int Version = 1;

    /// <summary>One displayed animation frame for each high angle byte.</summary>
    public const int AngleCount = 256;

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
    public static InstalledGrappleSwingFrames Load(Stream json)
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
        if (document.Version != GrappleSwingFrameFile.Version || document.Frames is null || document.Frames.Length != GrappleSwingFrameFile.AngleCount ||
            document.Frames.Any(frame => frame is < 0 or >= GrappleSwingFrameDefinitions.FrameCount))
            throw new InvalidDataException("Grapple swing frames require version 1 and 256 frame indices in 0..31.");
        return new(document.Frames.ToArray());
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
