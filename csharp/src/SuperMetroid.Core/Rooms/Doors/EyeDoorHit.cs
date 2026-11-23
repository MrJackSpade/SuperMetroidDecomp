namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>What a weapon that hits an eye door's eye does to it.</summary>
internal enum EyeDoorHit
{
    /// <summary>The hit counts against the eye.</summary>
    Counts,

    /// <summary>The hit destroys the eye at once.</summary>
    DestroysAtOnce,
}
