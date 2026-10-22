using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
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
        EscapeTimerPresentation.Write(output, new()
        {
            Version = EscapeTimerPresentationDefinitions.Version,
            DigitSpacing = 8,
            Anchors = new(StringComparer.Ordinal)
            {
                [EscapeTimerPresentationDefinitions.AnchorName(EscapeTimerAnchor.Label)] = new(0, 0),
                [EscapeTimerPresentationDefinitions.AnchorName(EscapeTimerAnchor.Minutes)] = new(-28, 0),
                [EscapeTimerPresentationDefinitions.AnchorName(EscapeTimerAnchor.Seconds)] = new(-4, 0),
                [EscapeTimerPresentationDefinitions.AnchorName(EscapeTimerAnchor.Centiseconds)] = new(20, 0),
            },
        });
        return output.ToArray();
    }
}
