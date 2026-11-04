using System.Text.Json;
using SuperMetroid.Core.Escape.Artwork;

namespace SuperMetroid.AssetExtraction.Areas.Escape;

/// <summary>Imports the timer's visual compositions without exposing its countdown mechanics.</summary>
/// <summary>Writes the escape timer's stock layout: its anchors and digit spacing. Its frames are the installed sheet.</summary>
public static class EscapeTimerPresentationExtractor
{
    /// <summary>Writes the stock timer layout as JSON: the cartridge renderer's anchor immediates and eight-pixel digit spacing.</summary>
    public static byte[] Extract()
    {
        using var output = new MemoryStream();
        Write(output, new()
        {
            Version = EscapeTimerLayoutDefinitions.Version,
            DigitSpacing = 8,
            Anchors = new(StringComparer.Ordinal)
            {
                [EscapeTimerLayoutNames.AnchorName(EscapeTimerAnchor.Label)] = new(0, 0),
                [EscapeTimerLayoutNames.AnchorName(EscapeTimerAnchor.Minutes)] = new(-28, 0),
                [EscapeTimerLayoutNames.AnchorName(EscapeTimerAnchor.Seconds)] = new(-4, 0),
                [EscapeTimerLayoutNames.AnchorName(EscapeTimerAnchor.Centiseconds)] = new(20, 0),
            },
        });
        return output.ToArray();
    }

    /// <summary>Serializes a timer document and writes its UTF-8 JSON bytes.</summary>
    /// <param name="output">Destination stream written at its current position and left open; existing trailing bytes are not truncated.</param>
    /// <param name="document">Authored layout whose collections are read for serialization, not retained by the writer.</param>
    public static void Write(Stream output, EscapeTimerPresentationDocument document)
    {
        output.Write(JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options));
    }
}
