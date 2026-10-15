using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>A Draygon cannon PLM's typed program (#1296).</summary>
internal sealed class DraygonCannonPlmAnimation(DraygonCannonProgram program)
    : PlmProgramAnimation<DraygonCannonProgram, DraygonCannonDraw, DraygonCannonCue>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<DraygonCannonProgram, DraygonCannonDraw, DraygonCannonCue> Programs =>
        DraygonCannonPlmProgramDefinitions.Programs;

    /// <inheritdoc/>
    private protected override ushort DrawList(DraygonCannonDraw frame) => (ushort)frame;

    /// <inheritdoc/>
    private protected override DraygonCannonProgram? Publish(DraygonCannonCue cue, PlmSlot slot, PlmAnimationContext context)
    {
        bool right = DraygonCannonPlmProgramDefinitions.FacesRight(Cursor.Program);
        switch (cue)
        {
            case DraygonCannonCue.LinkHit:
                SetLink(right ? DraygonCannonProgram.RightHit : DraygonCannonProgram.LeftHit);
                return null;
            case DraygonCannonCue.ArmMissileWake:
                slot.PreInstruction = DraygonCannonRomData.MissileHitPreInstruction;
                return null;
            case DraygonCannonCue.CountHit:
                return DraygonCannonPlms.CountHit(slot)
                    ? right ? DraygonCannonProgram.RightDestroy : DraygonCannonProgram.LeftDestroy
                    : null;
            case DraygonCannonCue.Damage:
                context.Plms.DraygonCannons.DamageDraygonCannon(context.Level, slot);
                return null;
            default:
                throw new InvalidOperationException($"Undefined {nameof(DraygonCannonCue)} {cue}.");
        }
    }
}
