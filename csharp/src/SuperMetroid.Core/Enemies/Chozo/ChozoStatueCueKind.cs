namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>What a cue of the Chozo statue's programs does: one per kind of step its lists use.</summary>
internal enum ChozoStatueCueKind
{
    /// <summary>Clears the frame behaviour.</summary>
    ClearFrameBehavior,
    /// <summary>Plays the footstep sound.</summary>
    PlayFootstepSound,
    /// <summary>Plays the grab sound.</summary>
    PlayGrabSound,
    /// <summary>Releases Samus and blocks the slope.</summary>
    ReleaseSamus,
    /// <summary>Sets the acid's lowered height.</summary>
    SetLoweredAcidPosition,
    /// <summary>Starts lowering the acid.</summary>
    StartLoweringAcid,
    /// <summary>Gives Samus her controls back.</summary>
    UnlockSamus,
}
