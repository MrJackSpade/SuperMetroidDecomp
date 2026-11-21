using SuperMetroid.Core.Progression;
using System.Text.Json.Serialization;

namespace SuperMetroid.AssetExtraction.Frontend.Options.Artwork;

/// <summary>One primary-page language text region, recolored by replacing only its BG palette bits.</summary>
public sealed record GameOptionsLanguageRegionDocument
{
    /// <summary>Nonempty caller-owned array of row-major tile cell indexes 0-1023, unique across all four language regions.</summary>
    public required int[] Cells { get; init; }
    /// <summary>The language whose label the region holds, named in the document; it receives the selected palette while that language is chosen (#2560).</summary>
    [JsonConverter(typeof(TextLanguageJsonConverter))]
    public required TextLanguage Language { get; init; }
}
