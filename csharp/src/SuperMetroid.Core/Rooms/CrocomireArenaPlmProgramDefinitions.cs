using SuperMetroid.Core.Game;
using Step = SuperMetroid.Core.Game.AnimationProgramStep<SuperMetroid.Core.Rooms.CrocomireArenaProgram, SuperMetroid.Core.Rooms.CrocomireArenaDraw, SuperMetroid.Core.Rooms.PlmNoCue>;

namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Crocomire's arena PLM programs (#1296), in place of their bank-$84 instruction lists at $84:AFCA..AFE7: each
/// draws its arena layout for one update, then deletes the PLM.
/// </summary>
internal static class CrocomireArenaPlmProgramDefinitions
{
    /// <summary>Every arena program.</summary>
    internal static AnimationProgramSet<CrocomireArenaProgram, CrocomireArenaDraw, PlmNoCue> Programs { get; } = new(
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
