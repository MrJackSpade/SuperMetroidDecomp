using System.Text.Json;
using SuperMetroid.Core.Escape.Artwork;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Areas.Escape;

/// <summary>Reads the escape timer's installed layout document into the layout the game draws with (#1502).</summary>
public static class EscapeTimerPresentationFile
{
    /// <summary>Validates an authored timer layout.</summary>
    /// <param name="json">UTF-8 JSON read from the current position to the end; the caller retains ownership of the stream.</param>
    /// <returns>A presentation that retains edited anchors, using calculated stock anchors for entries that match them.</returns>
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

        if (document.Version != EscapeTimerPresentationDefinitions.Version || document.Anchors is null ||
            document.DigitSpacing is < -256 or > 255)
            throw new InvalidDataException($"Escape timer presentation requires version {EscapeTimerPresentationDefinitions.Version}, valid anchors and spacing.");
        if (document.Anchors.Count != EscapeTimerPresentationDefinitions.Anchors.Count ||
            EscapeTimerPresentationDefinitions.Anchors.Any(anchor => !document.Anchors.ContainsKey(EscapeTimerComposition.AnchorName(anchor))))
            throw new InvalidDataException("Escape timer presentation requires Label, Minutes, Seconds and Centiseconds anchors exactly once.");

        var anchors = new Dictionary<EscapeTimerAnchor, Offset16<SpritePixels>>();
        foreach (EscapeTimerAnchor anchor in EscapeTimerPresentationDefinitions.Anchors)
        {
            Offset16<SpritePixels> point = document.Anchors[EscapeTimerComposition.AnchorName(anchor)];
            if (point.X is < -256 or > 255 || point.Y is < -224 or > 223)
                throw new InvalidDataException($"Escape timer anchor {anchor} is outside the supported screen-relative range.");
            if (point != EscapeTimerComposition.DefaultAnchor(anchor)) anchors.Add(anchor, point);
        }
        return new(anchors, document.DigitSpacing);
    }
}
