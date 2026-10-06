namespace SuperMetroid.Core.Game;

public sealed partial class RoomEnemySystem
{
    /// <summary>
    /// WRAM $0797, door_transition_flag_enemies. Ridley's shared A35B wait
    /// remains active while destination enemy visuals run during the room fade.
    /// This is distinct from the elevator/Zebetite gate at $0795.
    /// </summary>
    public bool EnemyDoorTransitionActive { get; set; }
}
