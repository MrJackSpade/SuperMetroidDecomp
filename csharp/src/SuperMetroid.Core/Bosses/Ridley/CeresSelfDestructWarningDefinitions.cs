namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// How the self-destruct warning after Ceres Ridley's getaway waits to show, holds its title and waits for its subtitle
/// (#2232, #2368, #2469).
/// </summary>
internal static partial class CeresSelfDestructWarningDefinitions
{
    /// <summary>
    /// The updates from the getaway's end to the escape timer and warning showing, the first being the update the getaway
    /// ends and the last the one they show on.
    /// </summary>
    internal const int BeforeWarningUpdates = 7;

    /// <summary>
    /// The updates from the escape timer and warning showing to the EMERGENCY title showing, the first being the update
    /// they show and the last the one the title shows on.
    /// </summary>
    internal const int BeforeTitleUpdates = 8;

    /// <summary>The updates the warning's EMERGENCY title holds, its alarm cycling, before the warning text types.</summary>
    internal const int TitleHoldUpdates = 128;

    /// <summary>
    /// Japanese text only: the updates from the title's hold ending to the subtitle showing, on the last of them, while the
    /// typewriter types.
    /// </summary>
    internal const int JapaneseSubtitleWaitUpdates = 32;
}
