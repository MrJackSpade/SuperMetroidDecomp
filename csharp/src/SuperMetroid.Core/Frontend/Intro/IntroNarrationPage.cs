using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// The narration page being typed (#2553): its identity, its installed characters and its typing, held together so a page
/// never runs without its characters.
/// </summary>
internal sealed class IntroNarrationPage
{
    /// <summary>Builds <paramref name="id"/>'s characters from <paramref name="presentation"/> and starts its typing after the page's opening pause.</summary>
    internal IntroNarrationPage(IntroNarrationPageId id, IntroNarrationPresentation presentation)
    {
        Id = id;
        Characters = Ensure.NotNull(presentation).Compile(id);
        AnimationSegments<IntroNarrationPageId, IntroNarrationTyping, IntroNarrationTypingCue> segments =
            IntroNarrationTypingAnimations.Segments(id, Characters.Length);
        Typing = AnimationPlayer<IntroNarrationPageId, IntroNarrationTyping, IntroNarrationTypingCue>.StartingAfter(
            segments, IntroNarrationTypingAnimations.Typing(segments, id), IntroNarrationDefinitions.PageOpeningPauseFrames);
    }

    /// <summary>The page.</summary>
    internal IntroNarrationPageId Id { get; }

    /// <summary>The page's glyphs, built from the installed presentation.</summary>
    internal IntroNarrationCharacter[] Characters { get; private set; }

    /// <summary>The page's typing.</summary>
    internal AnimationPlayer<IntroNarrationPageId, IntroNarrationTyping, IntroNarrationTypingCue> Typing { get; }

    /// <summary>The characters the page has typed, which its typing shows; none before its first.</summary>
    private int TypedCharacters => Typing.Shown?.Letters ?? 0;

    /// <summary>Rebuilds the page's characters from the newly bound <paramref name="presentation"/>, keeping its typing's place.</summary>
    /// <exception cref="InvalidDataException">The page has typed more characters than the rebound page holds.</exception>
    internal void Rebind(IntroNarrationPresentation presentation)
    {
        Characters = Ensure.NotNull(presentation).Compile(Id);
        if (TypedCharacters > Characters.Length)
        {
            throw new InvalidDataException(
                $"Saved narration cursor {TypedCharacters} exceeds the rebound {Id} narration.");
        }
        AnimationSegments<IntroNarrationPageId, IntroNarrationTyping, IntroNarrationTypingCue> rebound =
            IntroNarrationTypingAnimations.Segments(Id, Characters.Length);
        Typing.Rebind(rebound, IntroNarrationTypingAnimations.Typing(rebound, Id));
    }
}
