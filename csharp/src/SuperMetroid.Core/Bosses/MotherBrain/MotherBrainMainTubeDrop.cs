using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The main tube, which falls last as her main falling tube piece; no tube follows it, so the collapse waits for none.</summary>
/// <param name="Clears">The room mutation clearing the tube's place, and the block it is spawned at.</param>
internal sealed record MotherBrainMainTubeDrop(RoomObjectSpawnRequest<MotherBrainRoomMutation> Clears) : MotherBrainTubeDrop(Clears)
{
    /// <inheritdoc/>
    internal override void React<TContext>(MotherBrainTubeDropReactions<TContext> reactions, TContext context) =>
        Ensure.NotNull(reactions).Main(this, context);
}
