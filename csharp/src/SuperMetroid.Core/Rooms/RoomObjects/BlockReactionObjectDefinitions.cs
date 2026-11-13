using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Block behaviours the block-reaction room objects write.</summary>
internal static class BlockReactionObjectDefinitions
{
    /// <summary>
    /// The behaviour a respawning breakable Grapple block is restored with: the respawning entry of the Grapple
    /// reaction table, so the restored block breaks again.
    /// </summary>
    internal static readonly RoomBlockBehavior RespawningGrappleBehavior = RoomBlockBehavior.Reacting(GrappleReaction.RespawningBreakableAnchor);
}
