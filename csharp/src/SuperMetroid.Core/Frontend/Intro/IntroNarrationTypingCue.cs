namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>What a narration page's typing tells its narration to do as it plays (#2488).</summary>
internal enum IntroNarrationTypingCue
{
    /// <summary>The page's opening pause has passed: it shows its first subtitle.</summary>
    ShowSubtitle,
    /// <summary>The final page is typed: its caret starts blinking for the page's final hold.</summary>
    BlinkCaret,
    /// <summary>The page is typed, and on the final page held: the page starts its input wait or ends the narration.</summary>
    FinishPage,
}
