using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>A Crocomire arena PLM's typed program.</summary>
internal sealed class CrocomireArenaPlmAnimation(CrocomireArenaProgram program)
    : PlmCuelessProgramAnimation<CrocomireArenaProgram, CrocomireArenaDraw>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<CrocomireArenaProgram, CrocomireArenaDraw, PlmNoCue> Programs =>
        CrocomireArenaPlmProgramDefinitions.Programs;

    /// <inheritdoc/>
    private protected override ushort DrawList(CrocomireArenaDraw frame) => (ushort)frame;
}
