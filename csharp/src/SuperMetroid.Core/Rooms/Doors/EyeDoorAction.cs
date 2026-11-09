namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>What an eye door's eye does as one of its images draws (#1554).</summary>
internal enum EyeDoorAction
{
    /// <summary>Nothing besides the image.</summary>
    None,
    /// <summary>The eye fires its shot.</summary>
    Shoot,
    /// <summary>Two randomly placed smoke puffs rise from the eye.</summary>
    SpawnTwoSmoke,
    /// <summary>The eye sweats.</summary>
    SpawnSweat,
}
