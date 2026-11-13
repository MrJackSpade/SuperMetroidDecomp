namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The door-transition gates a door load carries from the room it replaces. Door-transition code owns both gates for
/// the whole transition; the destination's enemy population initializes with them clear, and the load re-asserts them
/// once the enemies are loaded (#1329).
/// </summary>
/// <param name="EnemyDoorTransitionActive">Whether enemies see a door transition in progress.</param>
/// <param name="ElevatorDoorTransitionActive">The gate that suppresses elevator and Zebetite movement.</param>
[IndependentFlag("independent gates: an enemy door transition and an elevator door transition")]
public readonly record struct EnemyDoorTransitionGates(bool EnemyDoorTransitionActive, bool ElevatorDoorTransitionActive);
