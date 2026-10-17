namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>The portrait-eye animations of the narration pages, as <see cref="IntroEyeAnimations"/> defines them.</summary>
internal enum IntroEyeProgram
{
    /// <summary>The repeating pair of blinks shown while pages one to five are written.</summary>
    Blinking,
    /// <summary>The final page's opening: the eye shuts, then half opens.</summary>
    FinalPageClosing,
    /// <summary>The final page's level stare, repeated for the rest of the narration.</summary>
    FinalPageDeadpan,
}
