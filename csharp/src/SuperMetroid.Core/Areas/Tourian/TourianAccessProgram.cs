namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>The Tourian elevator access PLM programs (#1296), in place of their bank-$84 lists.</summary>
internal enum TourianAccessProgram
{
    /// <summary>Seeds the six-row crumble (<c>InstList_PLM_CrumbleAccessToTourianElevator_0</c>).</summary>
    Crumble,
    /// <summary>Crumbles one row and moves down, repeated per row (<c>InstList_PLM_CrumbleAccessToTourianElevator_1</c>).</summary>
    CrumbleRow,
    /// <summary>Clears the whole access in one draw (<c>InstList_PLM_ClearAccessToTourianElevator</c>).</summary>
    Clear,
}
