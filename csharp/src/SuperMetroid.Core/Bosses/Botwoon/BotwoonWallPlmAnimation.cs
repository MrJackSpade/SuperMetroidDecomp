using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>Botwoon's wall PLM's typed program.</summary>
internal sealed class BotwoonWallPlmAnimation(BotwoonWallProgram program)
    : PlmProgramAnimation<BotwoonWallProgram, BotwoonWallDraw, BotwoonWallCue>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<BotwoonWallProgram, BotwoonWallDraw, BotwoonWallCue> Programs =>
        BotwoonWallPlmProgramDefinitions.Programs;

    /// <inheritdoc/>
    private protected override PlmDraw DrawList(BotwoonWallDraw frame) => frame switch
    {
        BotwoonWallDraw.CrumbleFirst => RoomPlmShotBlockDrawDefinitions.Breakup(RoomPlmShotBlockDrawDefinitions.Shape.Single, 0),
        BotwoonWallDraw.CrumbleSecond => RoomPlmShotBlockDrawDefinitions.Breakup(RoomPlmShotBlockDrawDefinitions.Shape.Single, 1),
        BotwoonWallDraw.CrumbleThird => RoomPlmShotBlockDrawDefinitions.Breakup(RoomPlmShotBlockDrawDefinitions.Shape.Single, 2),
        BotwoonWallDraw.CrumbleFourth => RoomPlmShotBlockDrawDefinitions.Breakup(RoomPlmShotBlockDrawDefinitions.Shape.Single, 3),
        BotwoonWallDraw.Clear => BotwoonWallPlmDrawDefinitions.Clear,
        _ => throw new InvalidOperationException($"Undefined {nameof(BotwoonWallDraw)} {frame}."),
    };

    /// <inheritdoc/>
    private protected override BotwoonWallProgram? Publish(BotwoonWallCue cue, PlmSlot slot, PlmStepContext context)
    {
        switch (cue)
        {
            case BotwoonWallCue.SetScrollsBlue:
                RoomScrollGrid scrolls = context.Frame.Scrolls ?? throw new InvalidOperationException(
                    "Botwoon's scroll PLM requires the active room scroll grid.");
                // `$84:AB51` performs one 16-bit store to the first two cells of the scroll grid.
                scrolls.SetLogicalState(new(0, 0), RoomScrollState.Blue);
                scrolls.SetLogicalState(new(1, 0), RoomScrollState.Blue);
                return null;
            case BotwoonWallCue.QueueCrumbleSound:
                context.QueueSound(BotwoonWallPlmProgramDefinitions.CrumbleSound, BotwoonWallPlmProgramDefinitions.CrumbleSoundMaximumQueued);
                return null;
            case BotwoonWallCue.MoveDownOneBlock:
                context.MoveOrigin(slot, columns: 0, rows: 1);
                return null;
            default:
                throw new InvalidOperationException($"Undefined {nameof(BotwoonWallCue)} {cue}.");
        }
    }
}
