using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Block behaviours the block-reaction PLMs write.</summary>
internal static class BlockReactionPlmDefinitions
{
    /// <summary>
    /// The behaviour a respawning breakable Grapple block is restored with: the respawning entry of the Grapple
    /// reaction table, so the restored block breaks again.
    /// </summary>
    internal static readonly RoomBlockBehavior RespawningGrappleBehavior = new(1);
}
