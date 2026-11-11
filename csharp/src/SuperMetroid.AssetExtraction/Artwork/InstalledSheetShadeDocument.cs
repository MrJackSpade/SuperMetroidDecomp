using System.Text.Json.Serialization;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// One shade in an installed sheet manifest (#1633): the colours it shows in place of the image's own, each named by the
/// colour key of the pixels it recolours, and whether a draw in it blends.
/// </summary>
public sealed record InstalledSheetShadeDocument
{
    /// <summary>The colours the shade shows, one for each colour key it recolours.</summary>
    public required InstalledColourSubstitutionDocument[] Substitutions { get; init; }

    /// <summary>Whether a picture drawn in the shade blends with what lies behind it; absent when it does not.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool Blends { get; init; }
}
