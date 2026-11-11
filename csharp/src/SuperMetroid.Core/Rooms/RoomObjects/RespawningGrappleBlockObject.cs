namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// A respawning breakable Grapple block (#1554, <see cref="BlockReactionChangeDefinitions.RespawningBreakableGrappleBlock"/>):
/// just before it grows its saved block back it gives the block its respawning Grapple reaction again, so the restored
/// block breaks the next time it is grappled.
/// </summary>
internal sealed class RespawningGrappleBlockObject() : BlockReactionObject(BlockReactionChangeDefinitions.RespawningBreakableGrappleBlock)
{
    /// <summary>Gives the block its respawning Grapple reaction again, then restores its saved terrain.</summary>
    private protected override void RestoreBlock(RoomObjectSlot place, RoomObjectStepContext context)
    {
        Ensure.NotNull(place);
        context.Level.SetBehavior(place.Block, BlockReactionObjectDefinitions.RespawningGrappleBehavior);
        base.RestoreBlock(place, context);
    }
}
