namespace SuperMetroid.Core.Enemies.EyeDoor;

/// <summary>A cue of the eye door's sweat drop's animations (#2554): what it does to the drop.</summary>
internal enum EyeDoorSweatCue
{
    /// <summary>Stops the drop moving as it splashes on the floor.</summary>
    StopMoving,

    /// <summary>Deletes the drop once its splash has played.</summary>
    Delete,
}
