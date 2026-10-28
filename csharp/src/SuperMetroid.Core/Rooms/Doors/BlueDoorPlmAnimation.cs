using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>A blue door cap's typed program (#1296).</summary>
internal sealed class BlueDoorPlmAnimation(BlueDoorProgram program)
    : PlmProgramAnimation<BlueDoorProgram, BlueDoorFrame, BlueDoorCue>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<BlueDoorProgram, BlueDoorFrame, BlueDoorCue> Programs =>
        BlueDoorPlmProgramDefinitions.Programs;

    /// <summary>The facing this cap draws for.</summary>
    private ColoredDoorOrientation Orientation => BlueDoorPlmProgramDefinitions.Orientation(Cursor.Program);

    /// <inheritdoc/>
    private protected override PlmDraw DrawList(BlueDoorFrame frame) => frame switch
    {
        BlueDoorFrame.Clear => DoorCaps.Clear(Orientation),
        BlueDoorFrame.Blue0 => DoorCaps.Blue(Orientation, 0),
        BlueDoorFrame.Blue1 => DoorCaps.Blue(Orientation, 1),
        BlueDoorFrame.Blue2 => DoorCaps.Blue(Orientation, 2),
        BlueDoorFrame.Blue3 => DoorCaps.Blue(Orientation, 3),
        _ => throw new InvalidOperationException($"Undefined {nameof(BlueDoorFrame)} {frame}."),
    };

    /// <inheritdoc/>
    private protected override BlueDoorProgram? Publish(BlueDoorCue cue, PlmSlot slot, PlmStepContext context)
    {
        switch (cue)
        {
            case BlueDoorCue.QueueOpenSound:
                context.QueueSound(BlueDoorPlmProgramDefinitions.OpenSound, BlueDoorPlmProgramDefinitions.SoundMaximumQueued);
                break;
            case BlueDoorCue.QueueCloseSound:
                context.QueueSound(BlueDoorPlmProgramDefinitions.CloseSound, BlueDoorPlmProgramDefinitions.SoundMaximumQueued);
                break;
            case BlueDoorCue.RestoreBlueBts:
                context.Level.SetBehavior(slot.BlockIndex, RoomBlockBehaviorCodec.BlueDoor(Orientation).ToBtsByte());
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(BlueDoorCue)} {cue}.");
        }
        return null;
    }
}
