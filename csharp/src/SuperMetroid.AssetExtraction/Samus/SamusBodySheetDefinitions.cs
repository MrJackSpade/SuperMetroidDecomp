namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// How the body sheet names Samus's pictures (#1392, #1513): each full body over the body characters the selected tile
/// definitions make visible, named by its spritemaps and the definition records whose characters they read. A
/// spritemap reads the top half's characters (0-7 and 16-23), the bottom half's (8-15 and 24-31), or neither.
/// </summary>
internal static class SamusBodySheetDefinitions
{
    /// <summary>The body's characters; any beyond them come from the standard OBJ sheet.</summary>
    internal const int BodyCharacterCount = 32;

    /// <summary>The columns of each character row the top half holds; the bottom half holds the rest of the row.</summary>
    internal const int TopHalfColumns = 8;

    /// <summary>The animation frames a pose's body pictures are covered for: the animation frame is a byte.</summary>
    internal const int AnimationFrames = 256;
}
