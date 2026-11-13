namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// An enemy Samus's solid-enemy collision can stop against (#1697): what her collision records of it and what her
/// movement asks of it. Her per-direction contact records hold the enemy itself (#1702), and the enemy pass's contact
/// tests compare them with the enemy reading them.
/// </summary>
public interface ISolidEnemy
{
    /// <summary>Shakes the enemy as Samus's wall jump has launched off it: its next draws shake it one pixel sideways.</summary>
    void ShakeFromWallJumpLaunch();
}
