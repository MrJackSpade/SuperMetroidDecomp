using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One of Mother Brain's fixed room mutations, spawned at a block.</summary>
/// <param name="Block">The room block the mutation is spawned at.</param>
/// <param name="Mutation">The mutation, which makes its own change wherever it is spawned.</param>
public sealed record MotherBrainMutationRequest(Position16<RoomBlocks> Block, MotherBrainRoomMutation Mutation)
    : MotherBrainRoomChangeRequest(Block)
{
    /// <inheritdoc/>
    internal override TerrainChange Change => Ensure.Listed(MotherBrainRoomMutationDefinitions.Changes, Mutation);
}
