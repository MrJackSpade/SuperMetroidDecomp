namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Where the Ceres station's self-destruct warning has reached after Ridley's getaway: the steps the warning runs
/// through in order, one or more updates each, before the station's countdown starts.
/// </summary>
internal enum CeresSelfDestructPhase
{
    /// <summary>The getaway has just ended: the warning's colours are copied in, and the warning starts showing its pages.</summary>
    Start,

    /// <summary>The warning's sprite character pages are shown, one per update.</summary>
    ShowingObjectPages,

    /// <summary>The warning's background pages are shown, one per update; the last draws EMERGENCY and starts the music.</summary>
    ShowingBackgroundPages,

    /// <summary>The EMERGENCY title holds, the alarm colours cycling, until the text delay runs out.</summary>
    HoldingTitle,

    /// <summary>Japanese text only: the typewriter runs while the subtitle's delay counts down, then the subtitle is drawn.</summary>
    JapaneseSubtitle,

    /// <summary>The typewriter types the warning, one step per update, until its program ends.</summary>
    TypingWarning,

    /// <summary>The warning is complete: Ridley stops, the station's countdown starts and the area boss counts as beaten.</summary>
    Finishing,
}
