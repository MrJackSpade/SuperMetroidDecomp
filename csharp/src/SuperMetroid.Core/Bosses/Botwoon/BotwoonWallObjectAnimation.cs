using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>Botwoon's wall PLM's typed program.</summary>
internal sealed class BotwoonWallObjectAnimation(BotwoonWallProgram program)
    : RoomObjectProgramAnimation<BotwoonWallProgram, BotwoonWallDraw, BotwoonWallCue>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<BotwoonWallProgram, BotwoonWallDraw, BotwoonWallCue> Programs =>
        BotwoonWallObjectProgramDefinitions.Programs;

    /// <inheritdoc/>
    private protected override RoomObjectDraw DrawList(BotwoonWallDraw frame) => frame switch
    {
        BotwoonWallDraw.CrumbleFirst => ShotBlockObjectDraws.Breakup(RoomObjectShotBlockDrawDefinitions.Shape.Single, 0),
        BotwoonWallDraw.CrumbleSecond => ShotBlockObjectDraws.Breakup(RoomObjectShotBlockDrawDefinitions.Shape.Single, 1),
        BotwoonWallDraw.CrumbleThird => ShotBlockObjectDraws.Breakup(RoomObjectShotBlockDrawDefinitions.Shape.Single, 2),
        BotwoonWallDraw.CrumbleFourth => ShotBlockObjectDraws.Breakup(RoomObjectShotBlockDrawDefinitions.Shape.Single, 3),
        BotwoonWallDraw.Clear => BotwoonWallObjectDrawDefinitions.Clear,
        _ => throw new InvalidOperationException($"Undefined {nameof(BotwoonWallDraw)} {frame}."),
    };

    /// <inheritdoc/>
    private protected override BotwoonWallProgram? Publish(BotwoonWallCue cue, RoomObjectSlot slot, RoomObjectStepContext context)
    {
        switch (cue)
        {
            case BotwoonWallCue.SetScrollsBlue:
                RoomScrollGrid scrolls = context.Frame.Scrolls ?? throw new InvalidOperationException(
                    "Botwoon's scroll PLM requires the active room scroll grid.");
                // The wall's opening step performs one 16-bit store to the first two cells of the scroll grid.
                scrolls.SetLogicalState(new(0, 0), RoomScrollState.Blue);
                scrolls.SetLogicalState(new(1, 0), RoomScrollState.Blue);
                return null;
            case BotwoonWallCue.QueueCrumbleSound:
                context.QueueSound(BotwoonWallObjectProgramDefinitions.CrumbleSound, BotwoonWallObjectProgramDefinitions.CrumbleSoundPriority);
                return null;
            case BotwoonWallCue.MoveDownOneBlock:
                context.MoveOrigin(slot, columns: 0, rows: 1);
                return null;
            default:
                throw new InvalidOperationException($"Undefined {nameof(BotwoonWallCue)} {cue}.");
        }
    }
}
