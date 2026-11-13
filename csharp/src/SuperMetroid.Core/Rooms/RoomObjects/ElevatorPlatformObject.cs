namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// An elevator platform (#1554): from its first update it plays its loop
/// (<see cref="ElevatorPlatformObjectDefinitions.Cycle"/>) for as long as the room holds it.
/// </summary>
internal sealed class ElevatorPlatformObject : SequencedRoomObject<TerrainChangeFrame>
{
    /// <inheritdoc/>
    private protected override int Show(TerrainChangeFrame step, RoomObjectSlot place, RoomObjectStepContext context)
    {
        if (step.MoveBefore != Offset16<RoomBlocks>.Zero || step.SoundBefore is not null)
            throw new InvalidOperationException("An elevator platform's loop only draws its layouts.");
        context.Draw(place, step.Draw);
        return step.Updates;
    }

    /// <inheritdoc/>
    private protected override void SequenceEnded(RoomObjectSlot place, RoomObjectStepContext context) =>
        Play(ElevatorPlatformObjectDefinitions.Cycle, place, context);
}
