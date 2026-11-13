namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>An animation of the old Tourian escape-shaft fake-wall explosion.</summary>
internal enum OldTourianWallExplosionSegment
{
    /// <summary>Stops the explosion's movement and loads the burst count.</summary>
    Start,

    /// <summary>Scatters, sounds and plays the six-pose burst, repeating while the count lasts.</summary>
    Burst,
}
