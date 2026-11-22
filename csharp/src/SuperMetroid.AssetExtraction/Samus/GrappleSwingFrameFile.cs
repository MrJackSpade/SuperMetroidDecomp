using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Grapple;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Reads and writes the installed Grapple swing-frame document (#1502).</summary>
public static partial class GrappleSwingFrameFile
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

    /// <summary>Loads the supported JSON schema, rejecting duplicate or unknown top-level properties and requiring exactly one displayed picture, the number 0..31 of the swing direction it was drawn for, for each of the 32 swing directions in direction order.</summary>
    /// <param name="json">UTF-8 JSON stream containing the complete swing-direction-to-displayed-frame mapping.</param>
    /// <returns>The immutable visual swing-frame catalog.</returns>
    public static InstalledGrappleSwingFrames Load(Stream json)
    {
        GrappleSwingFrameDocument document;
        try
        {
            using var parsed = JsonDocument.Parse(json);
            if (parsed.RootElement.ValueKind != JsonValueKind.Object) throw new InvalidDataException("Grapple swing frames require an object.");
            JsonAssetDocument.RejectDuplicateProperties(parsed.RootElement, StringComparer.Ordinal,
                _ => new InvalidDataException("Duplicate Grapple swing-frame property."), DuplicatePropertyScope.OutsideArrays);
            document = parsed.RootElement.Deserialize<GrappleSwingFrameDocument>(global::SuperMetroid.AssetExtraction.AssetExtractionJson.Info<global::SuperMetroid.AssetExtraction.Samus.GrappleSwingFrameDocument>(Options))
                ?? throw new InvalidDataException("Missing Grapple swing frames.");
        }
        catch (JsonException error) { throw new InvalidDataException("Invalid Grapple swing-frame JSON.", error); }
        if (document.Version != GrappleSwingFrameFile.Version || document.Frames is null || document.Frames.Length != GrappleSwingDirection.Count ||
            document.Frames.Any(frame => frame is < 0 or >= GrappleSwingDirection.Count))
            throw new InvalidDataException(
                $"Grapple swing frames require version {GrappleSwingFrameFile.Version} and {GrappleSwingDirection.Count} frame indices in 0..{GrappleSwingDirection.Count - 1}.");
        // The document lists each direction's picture in direction order, naming the picture by the direction it was drawn for.
        return new(GrappleSwingDirection.All.ToDictionary(direction => direction,
            direction => new GrappleSwingPicture(GrappleSwingDirection.FromNumber(document.Frames[direction.Number]))));
    }

    /// <summary>Serializes a swing-frame document as indented camel-case UTF-8 JSON.</summary>
    /// <param name="document">Document containing the supported version and a valid orientation-frame selection for every swing direction.</param>
    /// <returns>JSON bytes suitable for the swing-frame asset file.</returns>
    public static byte[] Write(GrappleSwingFrameDocument document) =>
        JsonSerializer.SerializeToUtf8Bytes(document, global::SuperMetroid.AssetExtraction.AssetExtractionJson.Info<global::SuperMetroid.AssetExtraction.Samus.GrappleSwingFrameDocument>(Options));
}
