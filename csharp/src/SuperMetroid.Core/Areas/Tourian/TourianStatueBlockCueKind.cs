namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>What one cue of a Tourian boss statue's picture animation does; the values it acts on come from the statue's definition.</summary>
internal enum TourianStatueBlockCueKind
{
    /// <summary>Sets the statue's own animating flag in the shared animation state.</summary>
    MarkAnimating,

    /// <summary>Continues at the cue's target when the statue's grey event already exists.</summary>
    GoToIfGrey,

    /// <summary>Continues at the cue's target when the statue's area boss is dead.</summary>
    GoToIfBossDefeated,

    /// <summary>Clears the statue's own animating flag.</summary>
    ClearAnimating,

    /// <summary>Continues at the cue's target while another statue's release is in progress.</summary>
    GoToIfReleaseInProgress,

    /// <summary>Claims the single release turn by setting the release-in-progress flag.</summary>
    ClaimRelease,

    /// <summary>Blacks out the statue's three cleared colours.</summary>
    ClearThreeColors,

    /// <summary>Spawns the statue's eye glow.</summary>
    SpawnEyeGlow,

    /// <summary>Spawns the statue's rising soul.</summary>
    SpawnSoul,

    /// <summary>Spawns the palette effect that greys the statue out.</summary>
    SpawnGreyColourEffect,

    /// <summary>Sets the statue's grey event.</summary>
    SetGreyEvent,

    /// <summary>Clears the release-in-progress flag together with the statue's own animating flag.</summary>
    FinishRelease,

    /// <summary>Gives the statue's eight greying colours the common grey colours.</summary>
    ApplyGreyColors,
}
