namespace SuperMetroid.Core.Enemies.Elevator;

/// <summary>A step of the elevator's ride that the runtime carries out after the enemy pass (#2562).</summary>
/// <param name="Kind">The step: the departure, the arrival's return or its completion.</param>
public sealed record ElevatorEvent(ElevatorFrameEvent Kind) : EnemyPassEvent;
