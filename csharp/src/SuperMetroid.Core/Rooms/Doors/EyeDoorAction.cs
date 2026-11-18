namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>What an eye door's eye does just before one of its images draws (#1554), raised by its animation (#2480).</summary>
internal enum EyeDoorAction
{
    /// <summary>The eye fires its shot.</summary>
    Shoot,
    /// <summary>Two randomly placed smoke puffs rise from the eye.</summary>
    SpawnTwoSmoke,
    /// <summary>The eye sweats.</summary>
    SpawnSweat,
}
