using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.RoomObjects;
using Step = SuperMetroid.Core.Programs.AnimationProgramStep<SuperMetroid.Core.Bosses.Crocomire.CrocomireArenaProgram, SuperMetroid.Core.Bosses.Crocomire.CrocomireArenaDraw, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectNoCue>;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// Crocomire's arena PLM programs (#1296), in place of their instruction lists: each
/// draws its arena layout for one update, then deletes the PLM.
/// </summary>
internal static class CrocomireArenaObjectProgramDefinitions
{
    /// <summary>Every arena program.</summary>
    internal static AnimationProgramSet<CrocomireArenaProgram, CrocomireArenaDraw, RoomObjectNoCue> Programs { get; } = new(
        new Dictionary<CrocomireArenaProgram, Step[]>
        {
            [CrocomireArenaProgram.ClearBridge] = DrawOnce(CrocomireArenaDraw.ClearBridge),
            [CrocomireArenaProgram.CrumbleBridgeBlock] = DrawOnce(CrocomireArenaDraw.CrumbleBridgeBlock),
            [CrocomireArenaProgram.ClearBridgeBlock] = DrawOnce(CrocomireArenaDraw.ClearBridgeBlock),
            [CrocomireArenaProgram.ClearInvisibleWall] = DrawOnce(CrocomireArenaDraw.ClearInvisibleWall),
            [CrocomireArenaProgram.CreateInvisibleWall] = DrawOnce(CrocomireArenaDraw.CreateInvisibleWall),
        });

    /// <summary>Draws <paramref name="layout"/> for one update, then deletes the PLM.</summary>
    private static Step[] DrawOnce(CrocomireArenaDraw layout) => [Step.Show(layout, 1), Step.Stop];
}
