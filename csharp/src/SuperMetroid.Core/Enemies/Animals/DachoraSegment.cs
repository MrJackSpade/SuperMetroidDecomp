namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// The Dachora's run, idle, blink, shinespark charge, echo and fall animations as typed programs (#641), one per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="DachoraAnimations"/>.
/// </summary>
internal enum DachoraSegment
{
    /// <summary>The running left animation.</summary>
    RunningLeft,
    /// <summary>The running left fast animation.</summary>
    RunningLeftFast,
    /// <summary>The running left very fast animation.</summary>
    RunningLeftVeryFast,
    /// <summary>The idle left animation.</summary>
    IdleLeft,
    /// <summary>The blink left animation.</summary>
    BlinkLeft,
    /// <summary>The echo left animation.</summary>
    EchoLeft,
    /// <summary>The falling left animation.</summary>
    FallingLeft,
    /// <summary>The running right animation.</summary>
    RunningRight,
    /// <summary>The running right fast animation.</summary>
    RunningRightFast,
    /// <summary>The running right very fast animation.</summary>
    RunningRightVeryFast,
    /// <summary>The idle right animation.</summary>
    IdleRight,
    /// <summary>The blink right animation.</summary>
    BlinkRight,
    /// <summary>The charge right animation.</summary>
    ChargeRight,
    /// <summary>The echo right animation.</summary>
    EchoRight,
    /// <summary>The falling right animation.</summary>
    FallingRight,
}
