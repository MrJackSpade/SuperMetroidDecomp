namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// The body halves a Samus composition reads characters of (#1463): a composition reading a half is composed with the
/// half picture its frame selects for that half, and a composition reading neither shows no half picture.
/// </summary>
public enum SamusBodyHalvesShown
{
    /// <summary>The composition reads no half picture.</summary>
    Neither,

    /// <summary>The composition reads only the top half's picture.</summary>
    Top,

    /// <summary>The composition reads only the bottom half's picture.</summary>
    Bottom,

    /// <summary>The composition reads the pictures of both halves.</summary>
    Both,
}
