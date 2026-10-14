namespace SuperMetroid.Diagnostics.Android;

/// <summary>All host gates must agree before either simulation or gameplay input runs.</summary>
internal static class AndroidRunPolicy
{
    /// <summary>Allows updates only for a live foreground activity outside its menu, with audio focus when sound is enabled.</summary>
    public static bool CanRun(bool resumed, bool focused, bool menuOpen, bool destroyed,
        bool audioEnabled, bool audioFocusGranted) =>
        resumed && focused && !menuOpen && !destroyed && (!audioEnabled || audioFocusGranted);
}
