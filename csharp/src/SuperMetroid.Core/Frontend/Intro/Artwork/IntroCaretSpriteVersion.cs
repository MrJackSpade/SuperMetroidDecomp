namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>Schema revisions of the opening caret composition document.</summary>
public enum IntroCaretSpriteVersion
{
    /// <summary>Supported legacy revision with four mistakenly separate frame names; loading uses <c>caret-still</c> and discards the three blink compositions.</summary>
    Previous = 1,
    /// <summary>Current schema revision, containing only the visible <c>caret-visible</c> composition; the native script supplies the hidden blink phase.</summary>
    Current = 2,
}
