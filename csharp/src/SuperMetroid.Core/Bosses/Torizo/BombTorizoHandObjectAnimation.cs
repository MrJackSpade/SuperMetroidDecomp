using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>The Bomb Torizo hand's typed program (#1296).</summary>
internal sealed class BombTorizoHandObjectAnimation()
    : RoomObjectProgramAnimation<BombTorizoHandProgram, BombTorizoHandDraw, BombTorizoHandCue>(BombTorizoHandProgram.Sleeping)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<BombTorizoHandProgram, BombTorizoHandDraw, BombTorizoHandCue> Programs =>
        BombTorizoHandObjectProgramDefinitions.Programs;

    /// <inheritdoc/>
    private protected override RoomObjectDraw DrawList(BombTorizoHandDraw frame) => RoomObjectDraw.Of(frame);

    /// <inheritdoc/>
    private protected override BombTorizoHandProgram? ProgramAfterSleep(BombTorizoHandProgram program) =>
        BombTorizoHandObjectProgramDefinitions.ProgramsAfterSleep.TryGetValue(program, out BombTorizoHandProgram? next)
            ? next
            : throw new InvalidOperationException($"Undefined {nameof(BombTorizoHandProgram)} {program}.");

    /// <inheritdoc/>
    private protected override BombTorizoHandProgram? Publish(BombTorizoHandCue cue, RoomObjectSlot slot, RoomObjectStepContext context)
    {
        BombTorizoHandObjects hand = context.RoomObjects.Bosses().BombTorizoHand;
        switch (cue.Kind)
        {
            case BombTorizoHandCueKind.ArmBombWake:
                slot.FrameBehavior = BombTorizoHandFrameBehaviors.WakeIfSamusHasBombs;
                break;
            case BombTorizoHandCueKind.SpawnFragment:
                hand.SpawnFragment(slot, cue.Fragment);
                break;
            case BombTorizoHandCueKind.QueueMusic:
                hand.QueueMusic();
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(BombTorizoHandCueKind)} {cue.Kind}.");
        }
        return null;
    }
}
