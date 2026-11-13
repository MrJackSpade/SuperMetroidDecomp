namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// Definitions for the old Tourian escape shaft fake wall room object that the room's
/// setup spawns in the Climb during the escape.
/// </summary>
internal static class OldTourianEscapeShaftWallObjectDefinitions
{
    /// <summary>The column of the block the room object is spawned at.</summary>
    internal const int BlockX = 16;
    /// <summary>The row of the block the room object is spawned at.</summary>
    internal const int BlockY = 135;

    /// <summary>The wall's one draw.</summary>
    internal enum Draw
    {
        /// <summary>Opens the shaft wall's two columns.</summary>
        OpenWall,
    }

    /// <summary>The draw that opens the shaft wall's two columns.</summary>
    internal static RoomObjectFrameDraw<Draw> OpenWallDraw { get; } = RoomObjectDraw.Of(Draw.OpenWall);

    /// <summary>Samus must be strictly right of X 240 and below Y 2080.</summary>
    internal const int WakeTargetX = 240;
    /// <summary>Samus must be below Y 2080 as well as right of the X threshold.</summary>
    internal const int WakeTargetY = 2080;
}
