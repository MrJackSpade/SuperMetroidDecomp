using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>A Kraid room ceiling, platform or spike PLM's typed program.</summary>
internal sealed class KraidRoomObjectAnimation(KraidRoomProgram program)
    : RoomObjectProgramAnimation<KraidRoomProgram, KraidRoomDraw, KraidRoomCue>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<KraidRoomProgram, KraidRoomDraw, KraidRoomCue> Programs =>
        KraidRoomObjectProgramDefinitions.Programs;

    /// <inheritdoc/>
    private protected override RoomObjectDraw DrawList(KraidRoomDraw frame) => RoomObjectDraw.Of(frame);

    /// <inheritdoc/>
    private protected override KraidRoomProgram? Publish(KraidRoomCue cue, RoomObjectSlot slot, RoomObjectStepContext context)
    {
        switch (cue)
        {
            case KraidRoomCue.MoveRightOneBlock:
                context.MoveOrigin(slot, columns: 1, rows: 0);
                return null;
            default:
                throw new InvalidOperationException($"Undefined {nameof(KraidRoomCue)} {cue}.");
        }
    }
}
