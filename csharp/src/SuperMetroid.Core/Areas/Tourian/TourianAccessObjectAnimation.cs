using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>A Tourian elevator access PLM's typed program (#1296).</summary>
internal sealed class TourianAccessObjectAnimation(TourianAccessProgram program)
    : RoomObjectProgramAnimation<TourianAccessProgram, TourianAccessDraw, TourianAccessCue>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<TourianAccessProgram, TourianAccessDraw, TourianAccessCue> Programs =>
        TourianAccessObjectProgramDefinitions.Programs;

    /// <inheritdoc/>
    private protected override RoomObjectDraw DrawList(TourianAccessDraw frame) => RoomObjectDraw.Of(frame);

    /// <inheritdoc/>
    private protected override TourianAccessProgram? Publish(TourianAccessCue cue, RoomObjectSlot slot, RoomObjectStepContext context)
    {
        if (cue != TourianAccessCue.MoveDown)
            throw new InvalidOperationException($"Undefined {nameof(TourianAccessCue)} {cue}.");
        context.MoveOrigin(slot, columns: 0, rows: 1);
        return null;
    }
}
