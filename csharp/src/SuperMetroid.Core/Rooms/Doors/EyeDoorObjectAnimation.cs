using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>An eye door component's typed program (#1296).</summary>
internal sealed class EyeDoorObjectAnimation(EyeDoorOrientation orientation, EyeDoorComponent component, EyeDoorObjectProgram program)
    : RoomObjectProgramAnimation<EyeDoorObjectProgram, EyeDoorObjectFrame, EyeDoorObjectCue>(program)
{
    /// <summary>The door's facing.</summary>
    internal EyeDoorOrientation Orientation { get; } = orientation;

    /// <summary>Which of the door's three PLMs this is.</summary>
    internal EyeDoorComponent Component { get; } = component;

    /// <inheritdoc/>
    private protected override AnimationProgramSet<EyeDoorObjectProgram, EyeDoorObjectFrame, EyeDoorObjectCue> Programs =>
        EyeDoorObjectProgramDefinitions.Sets[Orientation];

    /// <inheritdoc/>
    private protected override RoomObjectDraw DrawList(EyeDoorObjectFrame frame) =>
        EyeDoorDraws.DrawList(Orientation, Component, frame);

    /// <inheritdoc/>
    private protected override EyeDoorObjectProgram? Publish(EyeDoorObjectCue cue, RoomObjectSlot slot, RoomObjectStepContext context)
    {
        EyeDoorObjects eyes = context.RoomObjects.EyeDoors;
        DoorObjects doors = context.RoomObjects.DoorObjects;
        switch (cue)
        {
            case EyeDoorObjectCue.ConvertIfOpened:
                return doors.IsDoorOpened(slot) ? EyeDoorObjectProgram.EyeOpened : null;
            case EyeDoorObjectCue.DeleteIfOpened:
                return doors.IsDoorOpened(slot) ? EyeDoorObjectProgram.ComponentDelete : null;
            case EyeDoorObjectCue.ActivateIfSamusNear:
            case EyeDoorObjectCue.CloseIfSamusInFront:
            case EyeDoorObjectCue.WatchIfSamusNear:
            case EyeDoorObjectCue.PulseIfSamusNear:
            case EyeDoorObjectCue.KeepPulsingIfSamusNear:
                EyeDoorProximity proximity = EyeDoorObjectProgramDefinitions.Proximities[cue];
                return eyes.IsSamusNear(context.Level, slot, proximity.Reach) ? proximity.Target : null;
            case EyeDoorObjectCue.LinkHit:
                SetLink(EyeDoorObjectProgram.EyeHit);
                return null;
            case EyeDoorObjectCue.LinkDelete:
                SetLink(EyeDoorObjectProgram.ComponentDelete);
                return null;
            case EyeDoorObjectCue.ArmMissileWake:
                slot.FrameBehavior = EyeDoorFrameBehaviors.MissileHit;
                return null;
            case EyeDoorObjectCue.ArmDoorBitWake:
                slot.FrameBehavior = EyeDoorFrameBehaviors.WakeWhenDoorBitSet;
                return null;
            case EyeDoorObjectCue.Shoot:
                eyes.Shoot(slot, Orientation);
                return null;
            case EyeDoorObjectCue.QueueHitSound:
                context.QueueSound(EyeDoorObjectProgramDefinitions.HitSound,
                    EyeDoorObjectProgramDefinitions.HitSoundPriority);
                return null;
            case EyeDoorObjectCue.SpawnTwoSmoke:
                eyes.SpawnTwoSmoke(slot, Orientation);
                return null;
            case EyeDoorObjectCue.CountHit:
                return EyeDoorObjects.CountEyeHit(slot, doors) ? EyeDoorObjectProgram.EyeDestroyed : null;
            case EyeDoorObjectCue.SpawnSweat:
                eyes.SpawnSweat(slot, Orientation);
                return null;
            case EyeDoorObjectCue.ClearFrameBehavior:
                slot.FrameBehavior = RoomObjectFrameBehaviors.Inert;
                return null;
            case EyeDoorObjectCue.SpawnSmoke:
                eyes.SpawnSmoke(slot, Orientation);
                return null;
            case EyeDoorObjectCue.MakeBlue:
                EyeDoorObjects.ConvertEyeToBlueDoor(context.Level, slot, Orientation);
                return null;
            case EyeDoorObjectCue.BecomeBlueDoor:
                slot.Family = null;
                slot.StartAnimation(new BlueDoorObjectAnimation(BlueDoorObjectProgramDefinitions.ClosedPrograms[Orientation switch
                {
                    EyeDoorOrientation.Left => ColoredDoorOrientation.Left,
                    EyeDoorOrientation.Right => ColoredDoorOrientation.Right,
                    _ => throw new InvalidOperationException($"Undefined {nameof(EyeDoorOrientation)} {Orientation}."),
                }]));
                return null;
            default:
                throw new InvalidOperationException($"Undefined {nameof(EyeDoorObjectCue)} {cue}.");
        }
    }
}
