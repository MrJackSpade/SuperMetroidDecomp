namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// How fast Mother Brain's body walks a step, forwards or backwards (#2258): each speed is a walking animation of its own,
/// whose poses hold for their own updates (<see cref="MotherBrainAnimations"/>).
/// </summary>
internal enum MotherBrainWalkSpeed
{
    /// <summary>Her fastest step.</summary>
    ReallyFast,
    /// <summary>A fast step.</summary>
    Fast,
    /// <summary>A step at middling speed.</summary>
    Medium,
    /// <summary>A slow step.</summary>
    Slow,
    /// <summary>Her slowest step.</summary>
    ReallySlow,
}
