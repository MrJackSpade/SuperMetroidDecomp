namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>The poses of an eye door's body's or bottom's pulse (#2481). Its values are arbitrary.</summary>
internal enum EyeDoorPartPose
{
    /// <summary>The resting pose, the first of its pulse and the pose it stands still in.</summary>
    Resting,

    /// <summary>The swelling pose, the middle of its pulse.</summary>
    Swelling,

    /// <summary>The swollen pose, the height of its pulse.</summary>
    Swollen,
}
