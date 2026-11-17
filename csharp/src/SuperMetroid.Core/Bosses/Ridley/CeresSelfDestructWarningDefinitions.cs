namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>How the self-destruct warning after Ceres Ridley's getaway holds its title and waits for its subtitle (#2232, #2368).</summary>
internal static class CeresSelfDestructWarningDefinitions
{
    /// <summary>The updates the warning's EMERGENCY title holds, its alarm cycling, before the warning text types.</summary>
    internal const int TitleHoldUpdates = 128;

    /// <summary>
    /// Japanese text only: the updates from the title's hold ending to the subtitle showing, on the last of them, while the
    /// typewriter types.
    /// </summary>
    internal const int JapaneseSubtitleWaitUpdates = 32;
}
