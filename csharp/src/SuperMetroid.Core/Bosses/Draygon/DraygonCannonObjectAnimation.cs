using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>A Draygon cannon PLM's typed program (#1296).</summary>
internal sealed class DraygonCannonObjectAnimation(DraygonCannonProgram program)
    : RoomObjectProgramAnimation<DraygonCannonProgram, DraygonCannonDraw, DraygonCannonCue>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<DraygonCannonProgram, DraygonCannonDraw, DraygonCannonCue> Programs =>
        DraygonCannonObjectProgramDefinitions.Programs;

    /// <inheritdoc/>
    private protected override RoomObjectDraw DrawList(DraygonCannonDraw frame) => RoomObjectDraw.Of(frame);

    /// <inheritdoc/>
    private protected override DraygonCannonProgram? Publish(DraygonCannonCue cue, RoomObjectSlot slot, RoomObjectStepContext context)
    {
        bool right = DraygonCannonPrograms.FacesRight(Cursor.Program);
        switch (cue)
        {
            case DraygonCannonCue.LinkHit:
                SetLink(right ? DraygonCannonProgram.RightHit : DraygonCannonProgram.LeftHit);
                return null;
            case DraygonCannonCue.ArmMissileWake:
                slot.FrameBehavior = DraygonCannonFrameBehaviors.MissileHit;
                return null;
            case DraygonCannonCue.CountHit:
                return DraygonCannonObjects.CountHit(slot)
                    ? right ? DraygonCannonProgram.RightDestroy : DraygonCannonProgram.LeftDestroy
                    : null;
            case DraygonCannonCue.Damage:
                context.RoomObjects.Bosses().DraygonCannons.DamageDraygonCannon(context.Level, slot);
                return null;
            default:
                throw new InvalidOperationException($"Undefined {nameof(DraygonCannonCue)} {cue}.");
        }
    }
}
