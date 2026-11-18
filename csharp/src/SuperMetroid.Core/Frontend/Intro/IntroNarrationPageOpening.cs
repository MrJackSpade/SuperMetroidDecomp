namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>How a narration page starts beyond typing its text (#2489).</summary>
internal enum IntroNarrationPageOpening
{
    /// <summary>The first page keeps the caret its setup started, still running its first animation.</summary>
    KeepsSetupCaret,

    /// <summary>A page after an illustration shows the caret again at the first line, restarting its animation.</summary>
    RestoresCaret,

    /// <summary>
    /// The last page clears the earlier pages' text, shows the caret again at the first line and closes the portrait's eye
    /// for good.
    /// </summary>
    ReplacesEarlierPages,
}
