using SuperMetroid.Core.Frontend.Intro;
namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>Visual identity of the opening-cinematic text caret.</summary>
internal static class IntroCaretSpriteDefinitions
{
    /// <summary>The hardware OAM limit for an authored visual composition.</summary>
    internal const int MaximumParts = IntroCinematicSpriteCompiler.MaximumParts;

    /// <summary>The one-part visible caret the still pose shows; blink control selects visibility separately.</summary>
    internal static CinematicSpriteFrameDefinition Visible { get; } = new("caret-visible", CinematicSpritePose.CaretVisible, 1);

    /// <summary>Exposes only the visible caret; blink frames are a visibility decision, not separate art.</summary>
    internal static IReadOnlyList<CinematicSpriteFrameDefinition> Frames { get; } = [Visible];
}
