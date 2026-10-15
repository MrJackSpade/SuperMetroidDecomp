namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>What a Mother Brain body cue does.</summary>
internal enum MotherBrainBodyCueKind
{
    /// <summary>Publishes a body pose to the AI.</summary>
    SetPose,
    /// <summary>Moves the body, keeping BG2 aligned with it.</summary>
    Move,
    /// <summary>Plants a foot (shaking the room), then moves the body.</summary>
    StepAndMove,
    /// <summary>Moves the body vertically, counter-scrolling BG2 with a horizontal bias.</summary>
    MoveWithScrollBias,
    /// <summary>Spawns a charge particle at the hand.</summary>
    SpawnHandDust,
    /// <summary>Fires the red hand beam at Samus.</summary>
    FireHandBeam,
    /// <summary>Advances the hand-beam attack phase.</summary>
    AdvanceHandBeamPhase,
}
