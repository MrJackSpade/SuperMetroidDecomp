namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>A schema revision of the opening caret composition document.</summary>
/// <param name="Revision">The revision number a document stores.</param>
public sealed record IntroCaretSpriteVersion(int Revision)
{
    /// <summary>Current schema revision, containing only the visible <c>caret-visible</c> composition; the native script supplies the hidden blink phase.</summary>
    public static readonly IntroCaretSpriteVersion Current = new(2);
}
