namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>Where the Shitroid's living victim, the sidehopper it drains, stands and hops in its room.</summary>
internal static class DeadSidehopperVictimDefinitions
{
    /// <summary>The height of the floor the victim stands on and lands its scripted hops on.</summary>
    internal const int FloorY = 184;

    /// <summary>The room point the victim stands at when its room loads.</summary>
    internal static readonly Position16<RoomPixels> StartPosition = new(488, FloorY);

    /// <summary>The room column the camera's left edge must pass to its left before the victim starts hopping.</summary>
    internal const int ActivationCameraX = 513;

    /// <summary>
    /// The room column from which the victim's hops collide with the room's terrain; left of it they follow the scripted
    /// floor at <see cref="FloorY"/>.
    /// </summary>
    internal const int TerrainFromX = 544;
}
