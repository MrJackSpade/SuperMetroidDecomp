using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>A Kraid room ceiling, platform or spike PLM's typed program.</summary>
internal sealed class KraidRoomPlmAnimation(KraidRoomProgram program)
    : PlmProgramAnimation<KraidRoomProgram, KraidRoomDraw, KraidRoomCue>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<KraidRoomProgram, KraidRoomDraw, KraidRoomCue> Programs =>
        KraidRoomPlmProgramDefinitions.Programs;

    /// <inheritdoc/>
    private protected override ushort DrawList(KraidRoomDraw frame) => (ushort)frame;

    /// <inheritdoc/>
    private protected override KraidRoomProgram? Publish(KraidRoomCue cue, PlmSlot slot, PlmStepContext context)
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
