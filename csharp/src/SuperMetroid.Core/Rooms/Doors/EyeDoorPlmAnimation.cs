using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>An eye door component's typed program (#1296).</summary>
internal sealed class EyeDoorPlmAnimation(EyeDoorOrientation orientation, EyeDoorComponent component, EyeDoorPlmProgram program)
    : PlmProgramAnimation<EyeDoorPlmProgram, EyeDoorPlmFrame, EyeDoorPlmCue>(program)
{
    /// <summary>The door's facing.</summary>
    internal EyeDoorOrientation Orientation { get; } = orientation;

    /// <summary>Which of the door's three PLMs this is.</summary>
    internal EyeDoorComponent Component { get; } = component;

    /// <inheritdoc/>
    private protected override AnimationProgramSet<EyeDoorPlmProgram, EyeDoorPlmFrame, EyeDoorPlmCue> Programs =>
        EyeDoorPlmProgramDefinitions.ProgramsFor(Orientation);

    /// <inheritdoc/>
    private protected override PlmDraw DrawList(EyeDoorPlmFrame frame) =>
        EyeDoorDraws.DrawList(Orientation, Component, frame);

    /// <inheritdoc/>
    private protected override EyeDoorPlmProgram? Publish(EyeDoorPlmCue cue, PlmSlot slot, PlmStepContext context)
    {
        EyeDoorPlms eyes = context.Plms.EyeDoors;
        DoorPlms doors = context.Plms.DoorPlms;
        switch (cue)
        {
            case EyeDoorPlmCue.ConvertIfOpened:
                return doors.IsDoorOpened(slot) ? EyeDoorPlmProgram.EyeOpened : null;
            case EyeDoorPlmCue.DeleteIfOpened:
                return doors.IsDoorOpened(slot) ? EyeDoorPlmProgram.ComponentDelete : null;
            case EyeDoorPlmCue.ActivateIfSamusNear:
            case EyeDoorPlmCue.CloseIfSamusInFront:
            case EyeDoorPlmCue.WatchIfSamusNear:
            case EyeDoorPlmCue.PulseIfSamusNear:
            case EyeDoorPlmCue.KeepPulsingIfSamusNear:
                EyeDoorProximity proximity = EyeDoorPlmProgramDefinitions.ProximityOf(cue);
                return eyes.IsSamusNear(context.Level, slot, proximity.Reach) ? proximity.Target : null;
            case EyeDoorPlmCue.LinkHit:
                SetLink(EyeDoorPlmProgram.EyeHit);
                return null;
            case EyeDoorPlmCue.LinkDelete:
                SetLink(EyeDoorPlmProgram.ComponentDelete);
                return null;
            case EyeDoorPlmCue.ArmMissileWake:
                slot.PreInstruction = EyeDoorPreInstructions.MissileHit;
                return null;
            case EyeDoorPlmCue.ArmDoorBitWake:
                slot.PreInstruction = EyeDoorPreInstructions.WakeWhenDoorBitSet;
                return null;
            case EyeDoorPlmCue.Shoot:
                eyes.Shoot(slot, Orientation);
                return null;
            case EyeDoorPlmCue.QueueHitSound:
                context.QueueSound(EyeDoorPlmProgramDefinitions.HitSound,
                    EyeDoorPlmProgramDefinitions.HitSoundMaximumQueued);
                return null;
            case EyeDoorPlmCue.SpawnTwoSmoke:
                eyes.SpawnTwoSmoke(slot, Orientation);
                return null;
            case EyeDoorPlmCue.CountHit:
                return EyeDoorPlms.CountEyeHit(slot, doors) ? EyeDoorPlmProgram.EyeDestroyed : null;
            case EyeDoorPlmCue.SpawnSweat:
                eyes.SpawnSweat(slot, Orientation);
                return null;
            case EyeDoorPlmCue.ClearPreInstruction:
                slot.PreInstruction = PlmPreInstructions.Inert;
                return null;
            case EyeDoorPlmCue.SpawnSmoke:
                eyes.SpawnSmoke(slot, Orientation);
                return null;
            case EyeDoorPlmCue.MakeBlue:
                EyeDoorPlms.ConvertEyeToBlueDoor(context.Level, slot, Orientation);
                return null;
            case EyeDoorPlmCue.BecomeBlueDoor:
                slot.Family = null;
                slot.StartAnimation(new BlueDoorPlmAnimation(BlueDoorPlmProgramDefinitions.Closed(Orientation switch
                {
                    EyeDoorOrientation.Left => ColoredDoorOrientation.Left,
                    EyeDoorOrientation.Right => ColoredDoorOrientation.Right,
                    _ => throw new InvalidOperationException($"Undefined {nameof(EyeDoorOrientation)} {Orientation}."),
                })));
                return null;
            default:
                throw new InvalidOperationException($"Undefined {nameof(EyeDoorPlmCue)} {cue}.");
        }
    }
}
