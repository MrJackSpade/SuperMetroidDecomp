namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// A respawning breakable Grapple block (#1554, <see cref="BlockReactionChangeDefinitions.RespawningBreakableGrappleBlock"/>):
/// just before it grows its saved block back it gives the block its respawning Grapple reaction again, so the restored
/// block breaks the next time it is grappled.
/// </summary>
internal sealed class RespawningGrappleBlockObject() : BlockReactionObject(BlockReactionChangeDefinitions.RespawningBreakableGrappleBlock)
{
    /// <inheritdoc/>
    private protected override void BeforeRestoringBlock(RoomObjectSlot place, RoomObjectStepContext context) =>
        context.Level.SetBehavior(place.BlockIndex, BlockReactionObjectDefinitions.RespawningGrappleBehavior);
}
