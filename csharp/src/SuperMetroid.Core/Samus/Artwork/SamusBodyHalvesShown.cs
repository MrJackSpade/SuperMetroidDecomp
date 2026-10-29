namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// The body halves a Samus composition's picture shows (#1463): a picture showing a half is drawn with the definition
/// record selected for that half, and a picture showing neither shows no record.
/// </summary>
public enum SamusBodyHalvesShown
{
    /// <summary>The picture shows no body record.</summary>
    Neither,

    /// <summary>The picture shows only the top half's record.</summary>
    Top,

    /// <summary>The picture shows only the bottom half's record.</summary>
    Bottom,

    /// <summary>The picture shows the records of both halves.</summary>
    Both,
}
