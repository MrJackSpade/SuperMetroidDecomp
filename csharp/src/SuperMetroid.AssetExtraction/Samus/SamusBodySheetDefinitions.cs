namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// How the body sheets name Samus's pictures (#1392): each spritemap over the body characters the selected tile
/// definitions make visible, named by the spritemap and the definition records whose characters it reads. Every
/// spritemap reads either the top half's characters (0-7 and 16-23) or the bottom half's (8-15 and 24-31), so a frame
/// is named by the one record it shows; a spritemap reading neither shows no record.
/// </summary>
internal static class SamusBodySheetDefinitions
{
    /// <summary>The body's characters; any beyond them come from the standard OBJ sheet.</summary>
    internal const int BodyCharacterCount = 32;

    /// <summary>The columns of each character row the top half holds; the bottom half holds the rest of the row.</summary>
    internal const int TopHalfColumns = 8;
}
