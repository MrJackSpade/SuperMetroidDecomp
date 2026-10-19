namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The door-transition gates a door load carries from the room it replaces. Door-transition code owns both words for
/// the whole transition; the destination's enemy population initializes with them clear, and the load re-asserts them
/// once the enemies are loaded (#1329).
/// </summary>
/// <param name="EnemyDoorTransitionActive">WRAM $0797, door_transition_flag_enemies.</param>
/// <param name="ElevatorDoorTransitionActive">WRAM $0795, the gate that suppresses elevator and Zebetite movement.</param>
public readonly record struct EnemyDoorTransitionGates(bool EnemyDoorTransitionActive, bool ElevatorDoorTransitionActive);
