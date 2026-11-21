namespace SuperMetroid.Core.Doors;

/// <summary>A door, locked until it is unlocked, which sets its lock back to the default.</summary>
public sealed class Door
{
    /// <summary>The lock, locked from the start.</summary>
    private DoorLock state = DoorLock.Locked;

    /// <summary>Whether the door is locked.</summary>
    public bool IsLocked => state is DoorLock.Locked;

    /// <summary>Whether a door moving at <paramref name="speed"/> moves; a call without one asks about the default.</summary>
    public static bool Moves(DoorSpeed speed = default) => speed is DoorSpeed.Fast;

    /// <summary>Unlocks the door: its lock goes back to the default, which no code names.</summary>
    public void Unlock() => state = default;
}
