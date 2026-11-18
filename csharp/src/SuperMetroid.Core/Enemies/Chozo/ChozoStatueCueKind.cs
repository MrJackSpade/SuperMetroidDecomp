namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>What a cue of the Chozo statue's animations does: start awaiting Samus at its hand, play its footstep or grab sound, release Samus, lower the acid, or give Samus her controls back.</summary>
internal enum ChozoStatueCueKind
{
    /// <summary>The statue starts awaiting Samus at its hand (<see cref="ChozoStatueState.AwaitHand"/>).</summary>
    AwaitHand,
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
