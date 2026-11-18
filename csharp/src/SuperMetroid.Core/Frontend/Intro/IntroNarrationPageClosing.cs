namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>What a narration page does once its text is typed (#2489).</summary>
internal enum IntroNarrationPageClosing
{
    /// <summary>The page waits for the press that turns it as soon as its text ends.</summary>
    AwaitsInput,

    /// <summary>
    /// The page waits for the press that turns it; with Japanese text it first waits for the press that shows its second
    /// subtitle, which starts that wait.
    /// </summary>
    AwaitsSecondSubtitle,

    /// <summary>The page holds its blinking caret, then ends the intro.</summary>
    EndsIntro,
}
