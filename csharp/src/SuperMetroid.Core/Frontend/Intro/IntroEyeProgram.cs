namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>The portrait-eye animations of the narration pages, as <see cref="IntroEyeAnimations"/> defines them.</summary>
internal enum IntroEyeProgram
{
    /// <summary>The repeating pair of blinks shown while pages one to five are written: a long-open blink followed by a shorter, quicker one.</summary>
    Blinking,
    /// <summary>The final page's opening: the eye shuts, then half opens, and runs on into the stare.</summary>
    FinalPageClosing,
    /// <summary>The final page's level stare, repeated for the rest of the narration.</summary>
    FinalPageDeadpan,
}
