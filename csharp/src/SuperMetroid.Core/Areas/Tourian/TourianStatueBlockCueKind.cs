namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>What one cue of a Tourian boss statue's picture animation does; the values it acts on come from the statue's definition.</summary>
internal enum TourianStatueBlockCueKind
{
    /// <summary>Sets the statue's own animating flag in the shared animation state.</summary>
    MarkAnimating,

    /// <summary>The statue may already be grey: when its grey event exists, its owner starts its already-grey finish (#2189).</summary>
    MayAlreadyBeGrey,

    /// <summary>The statue may be released: when its area boss is dead, its owner starts its release (#2189).</summary>
    MayBeReleased,

    /// <summary>Clears the statue's own animating flag.</summary>
    ClearAnimating,

    /// <summary>
    /// The statue may wait its turn: while another statue's release is in progress, its owner starts it oscillating again
    /// (#2189).
    /// </summary>
    MayWaitItsTurn,

    /// <summary>Claims the single release turn by setting the release-in-progress flag.</summary>
    ClaimRelease,

    /// <summary>Shows the statue's eyes blacked out, as its release starts.</summary>
    BlackOutEyes,

    /// <summary>Spawns the statue's eye glow.</summary>
    SpawnEyeGlow,

    /// <summary>Spawns the statue's rising soul.</summary>
    SpawnSoul,

    /// <summary>Spawns the colour effect that greys the statue out.</summary>
    SpawnGreyColourEffect,

    /// <summary>Sets the statue's grey event.</summary>
    SetGreyEvent,

    /// <summary>Clears the release-in-progress flag together with the statue's own animating flag.</summary>
    FinishRelease,

    /// <summary>Gives the statue's eight greying colours the common grey colours.</summary>
    ApplyGreyColors,
}
