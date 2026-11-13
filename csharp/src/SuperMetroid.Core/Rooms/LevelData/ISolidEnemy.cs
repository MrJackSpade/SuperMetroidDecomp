namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// An enemy Samus's solid-enemy collision can stop against (#1697): what her collision records of it and what her
/// movement asks of it.
/// </summary>
public interface ISolidEnemy
{
    /// <summary>
    /// The enemy's place in the room's enemy table, by which Samus's per-direction contact records name it: the enemy
    /// pass's contact tests compare and combine those records by place, and a record left when Samus leaves a room names
    /// that place in the next.
    /// </summary>
    int ContactRecordPlace { get; }

    /// <summary>Shakes the enemy as Samus's wall jump has launched off it: its next draws shake it one pixel sideways.</summary>
    void ShakeFromWallJumpLaunch();
}
