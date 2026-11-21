namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>What one narration page does beyond typing its text (#2489).</summary>
/// <param name="Opening">How the page starts.</param>
/// <param name="Closing">What the page does once its text is typed.</param>
/// <param name="TurnsTo">The illustration the press that turns the page starts, or null when the next page follows at once.</param>
internal readonly partial record struct IntroNarrationPageDefinition(
    IntroNarrationPageOpening Opening,
    IntroNarrationPageClosing Closing,
    IntroIllustration? TurnsTo);
