namespace SuperMetroid.Core.Frontend.Title;

/// <summary>
/// How far the skip to the title screen has run (#1719). The skip runs alongside the title's running step rather than in
/// its place: its fade-out darkens the screen while the opening keeps running, and the update after it reaches black
/// rebuilds the title objects and hands the title to the fade-in step.
/// </summary>
internal enum TitleSkipStage
{
    /// <summary>No skip is running; a new B, Start or A press read after an opening step starts one.</summary>
    NotSkipping,
    /// <summary>The skip's fast fade-out lowers the brightness by two levels per update, after the running step has run.</summary>
    FadingOut,
    /// <summary>
    /// The fade-out has reached black and stopped the running step; the next update rebuilds the title objects in its
    /// place and starts the fade-in.
    /// </summary>
    Rebuilding,
}
