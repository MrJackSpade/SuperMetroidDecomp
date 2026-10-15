using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>Botwoon's wall PLM's typed program.</summary>
internal sealed class BotwoonWallPlmAnimation(BotwoonWallProgram program)
    : PlmProgramAnimation<BotwoonWallProgram, BotwoonWallDraw, BotwoonWallCue>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<BotwoonWallProgram, BotwoonWallDraw, BotwoonWallCue> Programs =>
        BotwoonWallPlmProgramDefinitions.Programs;

    /// <inheritdoc/>
    private protected override ushort DrawList(BotwoonWallDraw frame) => (ushort)frame;

    /// <inheritdoc/>
    private protected override BotwoonWallProgram? Publish(BotwoonWallCue cue, PlmSlot slot, PlmAnimationContext context)
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
