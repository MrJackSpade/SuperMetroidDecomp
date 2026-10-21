using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>The Bomb Torizo hand's typed program (#1296).</summary>
internal sealed class BombTorizoHandPlmAnimation()
    : PlmProgramAnimation<BombTorizoHandProgram, BombTorizoHandDraw, BombTorizoHandCue>(BombTorizoHandProgram.Sleeping)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<BombTorizoHandProgram, BombTorizoHandDraw, BombTorizoHandCue> Programs =>
        BombTorizoHandPlmProgramDefinitions.Programs;

    /// <inheritdoc/>
    private protected override PlmDraw DrawList(BombTorizoHandDraw frame) => PlmDraw.Of(frame);

    /// <inheritdoc/>
    private protected override BombTorizoHandProgram? ProgramAfterSleep(BombTorizoHandProgram program) =>
        BombTorizoHandPlmProgramDefinitions.ProgramAfterSleep(program);

    /// <inheritdoc/>
    private protected override BombTorizoHandProgram? Publish(BombTorizoHandCue cue, PlmSlot slot, PlmStepContext context)
    {
        BombTorizoHandPlms hand = context.Plms.Bosses().BombTorizoHand;
        switch (cue.Kind)
        {
            case BombTorizoHandCueKind.ArmBombWake:
                slot.PreInstruction = BombTorizoHandPreInstructions.WakeIfSamusHasBombs;
                break;
            case BombTorizoHandCueKind.CopyDebrisTiles:
                hand.CopyDebrisTiles();
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
