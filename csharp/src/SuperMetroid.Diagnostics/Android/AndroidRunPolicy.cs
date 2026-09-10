namespace SuperMetroid.Android;

/// <summary>All host gates must agree before either simulation or gameplay input runs.</summary>
internal static class AndroidRunPolicy
{
    public static bool CanRun(bool resumed, bool focused, bool menuOpen, bool destroyed,
        bool audioEnabled, bool audioFocusGranted) =>
        resumed && focused && !menuOpen && !destroyed && (!audioEnabled || audioFocusGranted);
}
