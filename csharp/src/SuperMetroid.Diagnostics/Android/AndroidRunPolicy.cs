namespace SuperMetroid.Android;

/// <summary>All host gates must agree before either simulation or gameplay input runs.</summary>
internal static class AndroidRunPolicy
{
    /// <summary>Determines whether lifecycle, focus, menu, and audio gates permit a game update.</summary>
    /// <param name="resumed">Whether the activity is resumed.</param>
    /// <param name="focused">Whether the activity currently has input focus.</param>
    /// <param name="menuOpen">Whether a host menu is open.</param>
    /// <param name="destroyed">Whether the host has been destroyed.</param>
    /// <param name="audioEnabled">Whether audio output is enabled.</param>
    /// <param name="audioFocusGranted">Whether the host currently owns audio focus.</param>
    /// <returns><see langword="true"/> only when every applicable gate permits simulation.</returns>
    public static bool CanRun(bool resumed, bool focused, bool menuOpen, bool destroyed,
        bool audioEnabled, bool audioFocusGranted) =>
        resumed && focused && !menuOpen && !destroyed && (!audioEnabled || audioFocusGranted);
}
