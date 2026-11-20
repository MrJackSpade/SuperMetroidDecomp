namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Gameplay cues of Kraid's part animations.</summary>
internal enum KraidPartCue
{
    /// <summary>Raises Kraid's body one pixel as the foot lifts.</summary>
    RaiseBody,
    /// <summary>Lowers Kraid's body one pixel as the foot lands, shaking the screen.</summary>
    LowerBodyAndShake,
    /// <summary>Queues the footstep sound.</summary>
    StepSound,
    /// <summary>Moves Kraid's body one step (<see cref="KraidArenaDefinitions.StepPixels"/>) left.</summary>
    StepLeft,
    /// <summary>Moves Kraid's body one step (<see cref="KraidArenaDefinitions.StepPixels"/>) right.</summary>
    StepRight,
    /// <summary>Switches the arm to its slow swing once Kraid is below half health.</summary>
    SlowArmBelowHalfHealth,
    /// <summary>Marks the start of the arm's normal swing (#2301): the arm has yet to reach its pause.</summary>
    ArmSwingBegins,
    /// <summary>
    /// Marks the arm's normal swing reaching its last frame (#2301): from that frame on the arm counts as paused, through
    /// its pause and any later animation, until its normal swing begins again.
    /// </summary>
    ArmReachesPause,
    /// <summary>
    /// Marks the foot's walk or lunge reaching its last frame (#2301): a forward walk or lunge has finished and rests on it,
    /// and the backward walk has reached the frame from which it loops.
    /// </summary>
    WalkReachesEnd,
    /// <summary>Marks the start of each lap of the foot's backward walk (#2301): the foot no longer shows the walk's last frame.</summary>
    WalkPlaysOn,
}
