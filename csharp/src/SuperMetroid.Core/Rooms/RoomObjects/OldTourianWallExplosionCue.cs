namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>A cue of the old Tourian escape-shaft fake-wall explosion's animation.</summary>
internal enum OldTourianWallExplosionCue
{
    /// <summary>Stops the explosion's movement.</summary>
    ClearFrameBehavior,

    /// <summary>Scatters the explosion about the wall.</summary>
    Scatter,

    /// <summary>Requests the small-explosion sound.</summary>
    ExplosionSound,

    /// <summary>Deletes the explosion.</summary>
    Delete,
}
