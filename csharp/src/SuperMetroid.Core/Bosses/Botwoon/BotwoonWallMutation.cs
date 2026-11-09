namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>What the room object Botwoon spawns at its wall does to the wall: the only spawns Botwoon's wall spawner performs.</summary>
public enum BotwoonWallMutation
{
    /// <summary>Clears the wall of a room whose Botwoon is already defeated.</summary>
    Clear,
    /// <summary>Crumbles the wall row by row once Botwoon's final body segment lands.</summary>
    Crumble,
}
