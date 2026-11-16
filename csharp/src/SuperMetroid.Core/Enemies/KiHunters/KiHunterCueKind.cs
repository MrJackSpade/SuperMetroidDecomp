namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>What a signalling cue of the Ki-Hunter's animations does: return to flight, or hand main AI to the wingless hop or decision.</summary>
internal enum KiHunterCueKind
{
    /// <summary>Returns to the flying animation for the current direction and restarts attached wings to match.</summary>
    ReturnToFlying,
    /// <summary>Hands main AI to the wingless hop.</summary>
    StartHop,
    /// <summary>Hands main AI back to the wingless decision action.</summary>
    StartWinglessThinking,
}
