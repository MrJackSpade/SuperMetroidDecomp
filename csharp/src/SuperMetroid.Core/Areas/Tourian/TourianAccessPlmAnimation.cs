using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>A Tourian elevator access PLM's typed program (#1296).</summary>
internal sealed class TourianAccessPlmAnimation(TourianAccessProgram program)
    : PlmProgramAnimation<TourianAccessProgram, TourianAccessDraw, TourianAccessCue>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<TourianAccessProgram, TourianAccessDraw, TourianAccessCue> Programs =>
        TourianAccessPlmProgramDefinitions.Programs;

    /// <inheritdoc/>
    private protected override ushort DrawList(TourianAccessDraw frame) => (ushort)frame;

    /// <inheritdoc/>
    private protected override TourianAccessProgram? Publish(TourianAccessCue cue, PlmSlot slot, PlmStepContext context)
    {
        if (cue != TourianAccessCue.MoveDown)
            throw new InvalidOperationException($"Undefined {nameof(TourianAccessCue)} {cue}.");
        context.MoveOrigin(slot, columns: 0, rows: 1);
        return null;
    }
}
