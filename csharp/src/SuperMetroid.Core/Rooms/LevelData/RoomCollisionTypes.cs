namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>Reads of the collision kinds' definitions (<see cref="RoomCollisionTypeDefinitions"/>).</summary>
internal static class RoomCollisionTypes
{
    /// <summary>The definition of <paramref name="kind"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="kind"/> is undefined.</exception>
    internal static RoomCollisionTypeDefinition Of(RoomCollisionType kind) => Ensure.Listed(RoomCollisionTypeDefinitions.Types, kind);

    /// <summary>Whether <paramref name="kind"/>'s blocks react to shots: the kinds that name a shootable reaction.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="kind"/> is undefined.</exception>
    internal static bool ReactsToShots(RoomCollisionType kind) => Of(kind).Reactions == BlockReactionFamily.Shootable;
}
