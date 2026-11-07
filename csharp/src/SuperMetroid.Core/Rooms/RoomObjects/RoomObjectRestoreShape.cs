namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The block shape a linked bomb-block or contact-crumble restoration list rebuilds.</summary>
internal enum RoomObjectRestoreShape
{
    /// <summary>Two blocks side by side.</summary>
    Horizontal,

    /// <summary>Two blocks stacked.</summary>
    Vertical,

    /// <summary>A two-by-two square.</summary>
    Square,
}
