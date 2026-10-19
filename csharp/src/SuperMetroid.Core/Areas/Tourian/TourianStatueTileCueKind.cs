namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>What one cue of a Tourian boss statue's animated-tile program does; the operands come from the statue's definition.</summary>
internal enum TourianStatueTileCueKind
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

    /// <summary>Blacks out the three colours that start at the statue's cleared colour index.</summary>
    ClearThreeColors,

    /// <summary>Spawns the statue's eye glow.</summary>
    SpawnEyeGlow,

    /// <summary>Spawns the statue's rising soul.</summary>
    SpawnSoul,

    /// <summary>Spawns the palette effect that greys the statue out.</summary>
    SpawnGreyPaletteFx,

    /// <summary>Sets the statue's grey event.</summary>
    SetGreyEvent,

    /// <summary>Clears the release-in-progress flag together with the statue's own animating flag.</summary>
    FinishRelease,

    /// <summary>Installs the eight common grey colours at the statue's grey colour index.</summary>
    ApplyGreyColors,
}
