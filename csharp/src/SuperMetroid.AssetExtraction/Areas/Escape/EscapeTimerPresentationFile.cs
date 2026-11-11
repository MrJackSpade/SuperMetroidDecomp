using System.Text.Json;
using SuperMetroid.Core.Escape.Artwork;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Areas.Escape;

/// <summary>Reads the escape timer's installed layout document into the layout the game draws with (#1502).</summary>
public static class EscapeTimerPresentationFile
{
    /// <summary>Validates an authored timer layout.</summary>
    /// <param name="json">UTF-8 JSON read from the current position to the end; the caller retains ownership of the stream.</param>
    /// <returns>A presentation holding every installed anchor and the installed digit spacing.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="InvalidDataException">JSON is malformed, of another revision, or has missing or out-of-range anchors or spacing.</exception>
    public static EscapeTimerPresentation Load(Stream json)
    {
        EscapeTimerPresentationDocument document;
        try
        {
            document = JsonAssetDocument.Read<EscapeTimerPresentationDocument>(json,
                ArtworkJson.Options) ??
                throw new InvalidDataException("Escape timer presentation document is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid escape timer presentation JSON.", error);
        }

        if (document.Version != EscapeTimerLayoutDefinitions.Version || document.Anchors is null ||
            document.DigitSpacing is < -256 or > 255)
            throw new InvalidDataException($"Escape timer presentation requires version {EscapeTimerLayoutDefinitions.Version}, valid anchors and spacing.");
        if (document.Anchors.Count != EscapeTimerLayoutDefinitions.Anchors.Count ||
            EscapeTimerLayoutDefinitions.Anchors.Any(anchor => !document.Anchors.ContainsKey(EscapeTimerLayoutNames.AnchorName(anchor))))
            throw new InvalidDataException("Escape timer presentation requires Label, Minutes, Seconds and Centiseconds anchors exactly once.");

        var anchors = new Dictionary<EscapeTimerAnchor, Offset16<SpritePixels>>();
        foreach (EscapeTimerAnchor anchor in EscapeTimerLayoutDefinitions.Anchors)
        {
            Offset16<SpritePixels> point = document.Anchors[EscapeTimerLayoutNames.AnchorName(anchor)];
            if (point.X is < -256 or > 255 || point.Y is < -224 or > 223)
                throw new InvalidDataException($"Escape timer anchor {anchor} is outside the supported screen-relative range.");
            anchors.Add(anchor, point);
        }
        return new(anchors, document.DigitSpacing);
    }
}
