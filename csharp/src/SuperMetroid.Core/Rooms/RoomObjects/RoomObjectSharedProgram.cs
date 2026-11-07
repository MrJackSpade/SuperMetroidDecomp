namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The one-instruction lists several PLM families share, as typed programs (#1296).</summary>
internal enum RoomObjectSharedProgram
{
    /// <summary>Deletes the PLM on its first pass.</summary>
    Delete,
    /// <summary>Sleeps forever; the family's pre-instruction does the work.</summary>
    Sleep,
}
