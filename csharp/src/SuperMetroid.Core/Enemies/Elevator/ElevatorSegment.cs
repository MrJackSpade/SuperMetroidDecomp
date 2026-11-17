namespace SuperMetroid.Core.Enemies.Elevator;

/// <summary>
/// The elevator's two-frame animation loop as a typed animation (#641): one segment per animation a spawn, branch or wake enters, in authored order.
/// Their steps are <see cref="ElevatorAnimations"/>.
/// </summary>
internal enum ElevatorSegment
{
    /// <summary>The elevator's pad flashing, unlit then lit, repeating.</summary>
    Loop,
}
