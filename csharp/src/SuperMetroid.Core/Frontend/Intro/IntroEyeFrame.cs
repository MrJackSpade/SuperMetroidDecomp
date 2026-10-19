namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>The four portrait-eye rectangles the narration pages show over Samus's portrait.</summary>
internal enum IntroEyeFrame
{
    /// <summary>The eye fully open, held between blinks.</summary>
    Open,
    /// <summary>The eye half shut, shown on the way into and out of a blink.</summary>
    HalfOpen,
    /// <summary>The eye fully shut at the middle of a blink, and at the start of the final page.</summary>
    Closed,
    /// <summary>The level stare the final page settles on and keeps.</summary>
    Deadpan,
}
