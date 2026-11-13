namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>What a cue of the Ki-Hunter's programs does: one per kind of step its lists use.</summary>
internal enum KiHunterCueKind
{
    /// <summary>Returns to the flying animation for the current direction and restarts attached wings to match.</summary>
    ReturnToFlying,
    /// <summary>Spits acid to the left and starts the spit wait.</summary>
    SpitLeft,
    /// <summary>Spits acid to the right and starts the spit wait.</summary>
    SpitRight,
    /// <summary>Hands main AI to the wingless hop.</summary>
    StartHop,
    /// <summary>Hands main AI back to the wingless decision function.</summary>
    StartWinglessThinking,
}
