namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Where the Ceres station's self-destruct warning has reached after Ridley's getaway: the steps the warning runs
/// through in order, one or more updates each, before the station's countdown starts.
/// </summary>
internal enum CeresSelfDestructPhase
{
    /// <summary>The getaway has just ended: the wait for the escape timer and warning starts, its first update this one.</summary>
    Start,

    /// <summary>The escape timer and warning wait to show; the wait's last update shows them and starts the wait for the title.</summary>
    WaitingForWarning,

    /// <summary>The EMERGENCY title waits to show, its wait's first update the warning's showing; the last draws it and starts the music.</summary>
    WaitingForTitle,

    /// <summary>The EMERGENCY title holds, the alarm colours cycling, until its hold runs out.</summary>
    HoldingTitle,

    /// <summary>Japanese text only: the typewriter runs while the subtitle's wait counts down, then the subtitle is drawn.</summary>
    JapaneseSubtitle,

    /// <summary>The typewriter types the warning, one step per update, until its message ends.</summary>
    TypingWarning,

    /// <summary>The warning is complete: Ridley stops, the station's countdown starts and the area boss counts as beaten.</summary>
    Finishing,
}
