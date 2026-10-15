namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Gameplay cues of Kraid's part programs.</summary>
internal enum KraidPartCue
{
    /// <summary>Raises Kraid's body one pixel as the foot lifts.</summary>
    RaiseBody,
    /// <summary>Lowers Kraid's body one pixel as the foot lands, shaking the screen.</summary>
    LowerBodyAndShake,
    /// <summary>Queues the footstep sound.</summary>
    StepSound,
    /// <summary>Moves Kraid's body three pixels left.</summary>
    StepLeft,
    /// <summary>Moves Kraid's body three pixels right.</summary>
    StepRight,
    /// <summary>Switches the arm to its slow swing once Kraid is below half health.</summary>
    SlowArmBelowHalfHealth,
}
