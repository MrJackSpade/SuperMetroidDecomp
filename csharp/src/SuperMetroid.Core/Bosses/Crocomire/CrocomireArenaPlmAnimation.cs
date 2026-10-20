using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>A Crocomire arena PLM's typed program.</summary>
internal sealed class CrocomireArenaPlmAnimation(CrocomireArenaProgram program)
    : PlmProgramAnimation<CrocomireArenaProgram, CrocomireArenaDraw, PlmNoCue>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<CrocomireArenaProgram, CrocomireArenaDraw, PlmNoCue> Programs =>
        CrocomireArenaPlmProgramDefinitions.Programs;

    /// <inheritdoc/>
    private protected override ushort DrawList(CrocomireArenaDraw frame) => (ushort)frame;

    /// <inheritdoc/>
    private protected override CrocomireArenaProgram? Publish(PlmNoCue cue, PlmSlot slot, PlmStepContext context) =>
        throw new InvalidOperationException($"Crocomire's arena programs publish no cues; got {cue}.");
}
