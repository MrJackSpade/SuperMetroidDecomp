using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>
/// Spore Spawn's ceiling block draws (#2034): the block shape each draws, from
/// <see cref="SporeSpawnCeilingObjectDrawDefinitions"/>, and the installed picture (#1557) each shows.
/// </summary>
internal static class SporeSpawnCeilingDraws
{
    /// <summary>The ceiling's four frames, pictured by the installed ceiling frames.</summary>
    internal static RoomObjectDrawFamily<SporeSpawnCeilingDraw> Family { get; } =
        new RoomObjectDrawFamily<SporeSpawnCeilingDraw>("Spore Spawn ceiling",
            frame => RoomObjectDrawFamily<SporeSpawnCeilingDraw>.Defined(SporeSpawnCeilingObjectDrawDefinitions.Shapes, frame));
}
