namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>The Tourian elevator access PLM programs (#1296).</summary>
internal enum TourianAccessProgram
{
    /// <summary>Seeds the six-row crumble of the access to the Tourian elevator.</summary>
    Crumble,
    /// <summary>Crumbles one row and moves down, repeated per row.</summary>
    CrumbleRow,
    /// <summary>Clears the whole access to the Tourian elevator in one draw.</summary>
    Clear,
}
