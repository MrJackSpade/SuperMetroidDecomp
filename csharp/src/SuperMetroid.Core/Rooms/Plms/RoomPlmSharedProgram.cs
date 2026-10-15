namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The one-instruction bank-$84 lists several PLM families share, as typed programs (#1296).</summary>
internal enum RoomPlmSharedProgram
{
    /// <summary>Deletes the PLM on its first pass (<c>InstList_PLM_Delete</c>, $84:AAE3).</summary>
    Delete,
    /// <summary>Sleeps forever; the family's pre-instruction does the work (<c>InstList_PLM_SetMetroidsClearedStatesWhenRequired</c>, $84:DB42).</summary>
    Sleep,
}
