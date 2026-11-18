using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// A narration page's typing (#2488), played by the shared animation player as the title cards and the ending's texts
/// type: after the page's opening pause its subtitle, the pause again, each installed character typed for
/// <see cref="IntroNarrationDefinitions.CharacterDelayFrames"/> updates, on a page that ends the intro its caret's
/// blinking hold, then the page's finish. The page's characters are installed, so its typing is built for the page it types.
/// </summary>
internal static class IntroNarrationTypingAnimations
{
    /// <summary>The typing of <paramref name="page"/>, which has <paramref name="characters"/> installed characters.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="characters"/> is negative.</exception>
    internal static AnimationSegments<IntroNarrationPageId, IntroNarrationTyping, IntroNarrationTypingCue> Segments(IntroNarrationPageId page, int characters) =>
        new(new Dictionary<IntroNarrationPageId, AnimationBeat<IntroNarrationTyping, IntroNarrationTypingCue>[]>
        {
            [page] = Typed(page, Ensure.AtLeastZero(characters)),
        }, [page]);

    /// <summary>The animation that types <paramref name="page"/> once, after which it is over.</summary>
    internal static Animation<IntroNarrationPageId> Typing(
        AnimationSegments<IntroNarrationPageId, IntroNarrationTyping, IntroNarrationTypingCue> segments, IntroNarrationPageId page) =>
        new([Ensure.NotNull(segments).Whole(page)], AnimationEnd.Stops);

    /// <summary>
    /// The beats of <paramref name="page"/>'s typing, played from the end of its opening pause: its subtitle, the pause again,
    /// each character, on a page that ends the intro its caret's blinking hold, then its finish.
    /// </summary>
    private static AnimationBeat<IntroNarrationTyping, IntroNarrationTypingCue>[] Typed(IntroNarrationPageId page, int characters)
    {
        var beats = new List<AnimationBeat<IntroNarrationTyping, IntroNarrationTypingCue>>
        {
            new AnimationBeat<IntroNarrationTyping, IntroNarrationTypingCue>.Event(IntroNarrationTypingCue.ShowSubtitle),
            new AnimationBeat<IntroNarrationTyping, IntroNarrationTypingCue>.Pause(IntroNarrationDefinitions.PageOpeningPauseFrames),
        };
        beats.AddRange(Enumerable.Range(1, characters).Select(letters =>
            new AnimationBeat<IntroNarrationTyping, IntroNarrationTypingCue>.Frame(new(letters), IntroNarrationDefinitions.CharacterDelayFrames)));
        // A page that ends the intro holds its blinking caret before it finishes (#2489).
        if (IntroNarrationPageDefinitions.Pages[page].Closing == IntroNarrationPageClosing.EndsIntro)
        {
            beats.Add(new AnimationBeat<IntroNarrationTyping, IntroNarrationTypingCue>.Event(IntroNarrationTypingCue.BlinkCaret));
            beats.Add(new AnimationBeat<IntroNarrationTyping, IntroNarrationTypingCue>.Pause(IntroNarrationDefinitions.FinalPageHoldFrames));
        }
        beats.Add(new AnimationBeat<IntroNarrationTyping, IntroNarrationTypingCue>.Event(IntroNarrationTypingCue.FinishPage));
        return [.. beats];
    }
}
