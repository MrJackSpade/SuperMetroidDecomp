namespace SuperMetroid.Core.Enemies;

/// <summary>
/// One thing a room's enemies did, in an enemy pass or as their room loaded, that the room, Samus's update or the game
/// carries out after them (#2562). The enemies publish each as it happens, and the pass returns them, in that order, as
/// its result (<see cref="EnemyPassEvents"/>).
/// </summary>
public abstract partial record EnemyPassEvent;
