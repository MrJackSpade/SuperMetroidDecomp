using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>A Crocomire arena PLM's typed program.</summary>
internal sealed class CrocomireArenaObjectAnimation(CrocomireArenaProgram program)
    : RoomObjectCuelessProgramAnimation<CrocomireArenaProgram, CrocomireArenaDraw>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<CrocomireArenaProgram, CrocomireArenaDraw, RoomObjectNoCue> Programs =>
        CrocomireArenaObjectProgramDefinitions.Programs;

    /// <inheritdoc/>
    private protected override RoomObjectDraw DrawList(CrocomireArenaDraw frame) => RoomObjectDraw.Of(frame);
}
