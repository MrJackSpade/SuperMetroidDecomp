using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Grapple;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Reads and writes the installed Grapple swing-frame document (#1502).</summary>
public static class GrappleSwingFrameFile
{
    /// <summary>Supported revision of the displayed-frame JSON schema: one frame for each swing direction.</summary>
    public const int Version = 2;

    /// <summary>Strict camel-case JSON settings for the editable swing-direction document.</summary>
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    /// <summary>Loads the supported JSON schema, rejecting duplicate or unknown top-level properties and requiring exactly one displayed frame index, in 0..31, for each of the 32 swing directions; copies the supplied indices without exposing mutable document storage.</summary>
    /// <param name="json">UTF-8 JSON stream containing the complete swing-direction-to-displayed-frame mapping.</param>
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
        if (document.Version != GrappleSwingFrameFile.Version || document.Frames is null || document.Frames.Length != GrappleSwingDirection.Count ||
            document.Frames.Any(frame => frame is < 0 or >= GrappleSwingFrameDefinitions.FrameCount))
            throw new InvalidDataException(
                $"Grapple swing frames require version {GrappleSwingFrameFile.Version} and {GrappleSwingDirection.Count} frame indices in 0..{GrappleSwingFrameDefinitions.FrameCount - 1}.");
        return new(document.Frames.ToArray());
    }

    /// <summary>Serializes a swing-frame document as indented camel-case UTF-8 JSON.</summary>
    /// <param name="document">Document containing the supported version and a valid orientation-frame selection for every swing direction.</param>
    /// <returns>JSON bytes suitable for the swing-frame asset file.</returns>
    public static byte[] Write(GrappleSwingFrameDocument document) =>
        JsonSerializer.SerializeToUtf8Bytes(document, Options);
}
