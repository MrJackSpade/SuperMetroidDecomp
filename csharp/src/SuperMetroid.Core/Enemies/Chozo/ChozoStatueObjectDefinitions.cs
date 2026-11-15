using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>The authored draws and timing of the terrain changes the two Chozo statues' room objects make.</summary>
internal static class ChozoStatueObjectDefinitions
{
    /// <summary>What the Lower Norfair hand draws once the acid is lowered.</summary>
    internal static RoomObjectDraw LoweredHand => ChozoStatueDraws.Family.Of(ChozoStatueDraw.LowerNorfairClearedHand);

    /// <summary>The Wrecked Ship hand: its whole effect is its spawn's block, and it leaves the room on its first update.</summary>
    internal static TerrainChange WreckedShipHand => TerrainChange.MadeAtSpawn;

    /// <summary>The Wrecked Ship slope access opening, drawn once before the spikes become slopes.</summary>
    internal static TerrainChange ClearSlopeAccess { get; } =
        TerrainChange.DrawnOnce(ChozoStatueDraws.Family.Of(ChozoStatueDraw.ClearSlopeAccess));

    /// <summary>The Wrecked Ship slope access closing, drawn once before the slopes become spikes again.</summary>
    internal static TerrainChange BlockSlopeAccess { get; } =
        TerrainChange.DrawnOnce(ChozoStatueDraws.Family.Of(ChozoStatueDraw.BlockSlopeAccess));

    /// <summary>The Lower Norfair plug crumbling: a one-block breakup whose last image, the cleared block, shows for one update.</summary>
    internal static TerrainChange CrumblePlug { get; } = new(1,
    [
        new(ShotBlockObjectDraws.Breakup(RoomObjectShotBlockDrawDefinitions.Shape.Single, BlockBreakupStage.Rock), ScriptedTerrainChangeDefinitions.BreakupFrameDuration),
        new(ShotBlockObjectDraws.Breakup(RoomObjectShotBlockDrawDefinitions.Shape.Single, BlockBreakupStage.Pieces), ScriptedTerrainChangeDefinitions.BreakupFrameDuration),
        new(ShotBlockObjectDraws.Breakup(RoomObjectShotBlockDrawDefinitions.Shape.Single, BlockBreakupStage.Dust), ScriptedTerrainChangeDefinitions.BreakupFrameDuration),
        new(ShotBlockObjectDraws.Breakup(RoomObjectShotBlockDrawDefinitions.Shape.Single, BlockBreakupStage.Gone), 1),
    ]);
}
