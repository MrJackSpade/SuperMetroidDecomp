namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>A movement of the downward gate's column, which one image of its course starts (<see cref="DownwardGateImage"/>).</summary>
internal enum DownwardGateMovement
{
    /// <summary>No movement starts: the image only shows.</summary>
    None,

    /// <summary>
    /// The column starts lowering: the moving gate projectile that carries its collision down spawns, and the movement
    /// sound plays.
    /// </summary>
    Lowering,

    /// <summary>The column starts rising: the gate projectile wakes to slide back up, and the movement sound plays.</summary>
    Rising,
}
