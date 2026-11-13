namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Where the Ceres station's self-destruct warning has reached after Ridley's getaway: the steps the warning runs
/// through in order, one or more updates each, before the station's countdown starts.
/// </summary>
internal enum CeresSelfDestructPhase
{
    /// <summary>The getaway has just ended: the warning's colours are copied in, and the warning starts readying.</summary>
    Start,

    /// <summary>The escape timer and warning are brought up over their duration; the last update shows them.</summary>
    ReadyingWarning,

    /// <summary>The arena's backgrounds are readied over their duration; the last update draws EMERGENCY and starts the music.</summary>
    ReadyingBackgrounds,

    /// <summary>The EMERGENCY title holds, the alarm colours cycling, until the text delay runs out.</summary>
    HoldingTitle,

    /// <summary>Japanese text only: the typewriter runs while the subtitle's delay counts down, then the subtitle is drawn.</summary>
    JapaneseSubtitle,

    /// <summary>The typewriter types the warning, one step per update, until its message ends.</summary>
    TypingWarning,

    /// <summary>The warning is complete: Ridley stops, the station's countdown starts and the area boss counts as beaten.</summary>
    Finishing,
}
