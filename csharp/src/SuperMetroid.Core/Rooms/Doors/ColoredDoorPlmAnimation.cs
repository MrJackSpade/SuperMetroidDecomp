using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>A coloured door cap's typed program (#1296).</summary>
internal sealed class ColoredDoorPlmAnimation(ColoredDoorColor color, ColoredDoorOrientation orientation, ColoredDoorProgram program)
    : PlmProgramAnimation<ColoredDoorProgram, DoorCapFrame, ColoredDoorCue>(program), IResidentDoorAnimation
{
    /// <summary>The cap's colour.</summary>
    internal ColoredDoorColor Color { get; } = color;

    /// <summary>The cap's facing.</summary>
    internal ColoredDoorOrientation Orientation { get; } = orientation;

    /// <inheritdoc/>
    private protected override AnimationProgramSet<ColoredDoorProgram, DoorCapFrame, ColoredDoorCue> Programs =>
        ColoredDoorPlmProgramDefinitions.Sets[new(Color, Orientation)];

    /// <inheritdoc/>
    public void StartClosing() => Restart(ColoredDoorProgram.Closing);

    /// <inheritdoc/>
    private protected override PlmDraw DrawList(DoorCapFrame frame) => frame switch
    {
        DoorCapFrame.Clear => DoorCaps.Clear(Orientation),
        DoorCapFrame.Closed0 => DoorCaps.Colored(Color, Orientation, 0),
        DoorCapFrame.Closed1 => DoorCaps.Colored(Color, Orientation, 1),
        DoorCapFrame.Closed2 => DoorCaps.Colored(Color, Orientation, 2),
        DoorCapFrame.Closed3 => DoorCaps.Colored(Color, Orientation, 3),
        DoorCapFrame.BlueFlash => DoorCaps.Blue(Orientation, 0),
        _ => throw new InvalidOperationException($"Undefined {nameof(DoorCapFrame)} {frame}."),
    };

    /// <inheritdoc/>
    private protected override ColoredDoorProgram? Publish(ColoredDoorCue cue, PlmSlot slot, PlmStepContext context)
    {
        DoorPlms doors = context.Plms.DoorPlms;
        switch (cue)
        {
            case ColoredDoorCue.BranchIfOpened:
                if (doors.IsDoorOpened(slot))
                {
                    slot.Family = null;
                    slot.StartAnimation(new BlueDoorPlmAnimation(BlueDoorPlmProgramDefinitions.ClosedPrograms[Orientation]));
                }
                return null;
            case ColoredDoorCue.LinkHit:
                SetLink(ColoredDoorProgram.Hit);
                return null;
            case ColoredDoorCue.ArmWake:
                slot.PreInstruction = ColoredDoorPreInstructions.Of(ColoredDoorPlmProgramDefinitions.Wakes[Color]);
                return null;
            case ColoredDoorCue.CountHit:
                return doors.CountColoredDoorHit(slot) ? ColoredDoorProgram.Open : null;
            case ColoredDoorCue.QueueHitSound:
                QueueDoorSound(context, ColoredDoorPlmProgramDefinitions.RedHitSound);
                return null;
            case ColoredDoorCue.QueueOpenSound:
                QueueDoorSound(context, BlueDoorPlmProgramDefinitions.OpenSound);
                return null;
            case ColoredDoorCue.QueueCloseSound:
                QueueDoorSound(context, BlueDoorPlmProgramDefinitions.CloseSound);
                return null;
            default:
                throw new InvalidOperationException($"Undefined {nameof(ColoredDoorCue)} {cue}.");
        }
    }

    /// <summary>Queues a library-3 door sound under the door sounds' cap.</summary>
    private static void QueueDoorSound(PlmStepContext context, SoundEffectId sound) =>
        context.QueueSound(sound, BlueDoorPlmProgramDefinitions.SoundMaximumQueued);
}
