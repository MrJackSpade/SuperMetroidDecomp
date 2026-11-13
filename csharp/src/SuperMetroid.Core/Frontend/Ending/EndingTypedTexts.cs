using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The ending's typed texts (#1513): THE OPERATION WAS, COMPLETED SUCCESSFULLY and CLEAR TIME, each typed one letter per
/// step of its actor's animation. The explosion sheet pictures their glyphs once each and the result text draws the glyphs
/// typed so far.
/// </summary>
internal static class EndingTypedTexts
{
    /// <summary>The family whose typed texts these are: the result text's.</summary>
    internal const CinematicSpriteFamily Family = CinematicSpriteFamily.EndingCompletionText;

    /// <summary>The ending's typed texts, by the animations that type them.</summary>
    internal static CinematicTypedTexts Texts { get; } = new(
    [
        EndingActorDefinitions.OperationWasText.Animation, EndingActorDefinitions.CompletedSuccessfullyText.Animation,
        EndingActorDefinitions.ClearTimeText.Animation,
    ]);
}
