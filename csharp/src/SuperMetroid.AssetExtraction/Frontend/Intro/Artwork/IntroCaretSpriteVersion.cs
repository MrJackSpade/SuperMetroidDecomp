using System.Collections.Immutable;

namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>A schema revision of the opening caret composition document.</summary>
/// <param name="Revision">The revision number a document stores.</param>
public sealed record IntroCaretSpriteVersion(int Revision)
{
    /// <summary>Supported legacy revision with four mistakenly separate frame names; loading uses <c>caret-still</c> and discards the three blink compositions.</summary>
    public static readonly IntroCaretSpriteVersion Previous = new(1);
    /// <summary>Current schema revision, containing only the visible <c>caret-visible</c> composition; the native script supplies the hidden blink phase.</summary>
    public static readonly IntroCaretSpriteVersion Current = new(2);

    /// <summary>Every accepted revision, oldest first.</summary>
    internal static readonly ImmutableArray<IntroCaretSpriteVersion> Supported = [Previous, Current];

    /// <summary>The accepted revision a document stores as <paramref name="revision"/>, or null when none is.</summary>
    internal static IntroCaretSpriteVersion? FromRevision(int revision) => Supported.FirstOrDefault(version => version.Revision == revision);
}
