namespace SuperMetroid.Core.Enemies.Elevator;

/// <summary>
/// The elevator's two-frame animation loop as a typed program (#641), one per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="ElevatorAnimations"/>.
/// </summary>
internal enum ElevatorSegment
{
    /// <summary>The loop animation.</summary>
    Loop,
}
