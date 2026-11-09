using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>A blue door cap's typed program (#1296).</summary>
internal sealed class BlueDoorObjectAnimation(BlueDoorProgram program)
    : RoomObjectProgramAnimation<BlueDoorProgram, BlueDoorFrame, BlueDoorCue>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<BlueDoorProgram, BlueDoorFrame, BlueDoorCue> Programs =>
        BlueDoorObjectProgramDefinitions.Programs;

    /// <summary>The facing this cap draws for.</summary>
    private ColoredDoorOrientation Orientation => BlueDoorObjectProgramDefinitions.Orientations[Cursor.Program];

    /// <inheritdoc/>
    private protected override RoomObjectDraw DrawList(BlueDoorFrame frame) => frame switch
    {
        BlueDoorFrame.Clear => DoorCaps.Clear(Orientation),
        BlueDoorFrame.Blue0 => DoorCaps.Blue(Orientation, 0),
        BlueDoorFrame.Blue1 => DoorCaps.Blue(Orientation, 1),
        BlueDoorFrame.Blue2 => DoorCaps.Blue(Orientation, 2),
        BlueDoorFrame.Blue3 => DoorCaps.Blue(Orientation, 3),
        _ => throw new InvalidOperationException($"Undefined {nameof(BlueDoorFrame)} {frame}."),
    };

    /// <inheritdoc/>
    private protected override BlueDoorProgram? Publish(BlueDoorCue cue, RoomObjectSlot slot, RoomObjectStepContext context)
    {
        switch (cue)
        {
            case BlueDoorCue.QueueOpenSound:
                context.QueueSound(BlueDoorObjectProgramDefinitions.OpenSound, BlueDoorObjectProgramDefinitions.SoundPriority);
                break;
            case BlueDoorCue.QueueCloseSound:
                context.QueueSound(BlueDoorObjectProgramDefinitions.CloseSound, BlueDoorObjectProgramDefinitions.SoundPriority);
                break;
            case BlueDoorCue.RestoreBlueBehavior:
                context.Level.SetBehavior(slot.BlockIndex, RoomBlockBehavior.Reacting(BlockReactionDefinitions.BlueDoorCaps.ReactionFor(Orientation)));
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(BlueDoorCue)} {cue}.");
        }
        return null;
    }
}
