namespace SuperMetroid.Core.Enemies.Elevator;

/// <summary>
/// The elevator's two-frame animation loop as a typed animation (#641).
/// Their steps are <see cref="ElevatorAnimations"/>.
/// </summary>
internal enum ElevatorSegment
{
    /// <summary>The elevator's idle flash (#2477): its pad unlit then lit, repeating.</summary>
    Idle,
}
