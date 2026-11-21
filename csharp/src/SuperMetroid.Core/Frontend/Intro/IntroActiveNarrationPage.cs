using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// The narration page being typed (#2488): which page it is, its glyphs as the installed presentation builds them, and its
/// typing, which shows its subtitle, its characters, the final page's caret hold and its finish.
/// </summary>
internal sealed class IntroActiveNarrationPage
{
    /// <summary>Starts typing <paramref name="page"/> with <paramref name="characters"/> and <paramref name="typing"/>.</summary>
    /// <param name="page">The page typed.</param>
    /// <param name="characters">The page's glyphs, built from the installed presentation.</param>
    /// <param name="typing">The page's typing, played by the shared player.</param>
    internal IntroActiveNarrationPage(IntroNarrationPageId page, IntroNarrationCharacter[] characters,
        AnimationPlayer<IntroNarrationPageId, IntroNarrationTyping, IntroNarrationTypingCue> typing)
    {
        Page = page;
        Characters = characters;
        Typing = typing;
    }

    /// <summary>The page typed.</summary>
    internal IntroNarrationPageId Page { get; }

    /// <summary>The page's glyphs, rebuilt whenever the installed presentation is bound again.</summary>
    internal IntroNarrationCharacter[] Characters { get; set; }

    /// <summary>The page's typing.</summary>
    internal AnimationPlayer<IntroNarrationPageId, IntroNarrationTyping, IntroNarrationTypingCue> Typing { get; }
}
