using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>The room objects the two Chozo statues spawn, and the authored timing of their terrain changes.</summary>
internal static class ChozoStatueObjectDefinitions
{
    /// <summary>The five room objects a Chozo statue spawns, in their authored order; no other object is a statue's.</summary>
    internal static IReadOnlyList<RoomObjectHeaderId> Headers { get; } =
    [
        RoomObjectHeaderId.LowerNorfairChozoHand,
        RoomObjectHeaderId.WreckedShipChozoHand,
        RoomObjectHeaderId.ClearSlopeAccessForWreckedShipChozo,
        RoomObjectHeaderId.BlockSlopeAccessForWreckedShipChozo,
        RoomObjectHeaderId.CrumbleLowerNorfairChozoRoomPlug,
    ];

    /// <summary>What the Lower Norfair hand draws once the acid is lowered.</summary>
    internal static RoomObjectDraw LoweredHand => ScriptedRoomObjectDrawDefinitions.DrawLists[ScriptedRoomObjectFrame.LowerNorfairChozoHand];

    /// <summary>The Wrecked Ship hand: its whole effect is its spawn's block, and it leaves the room on its first update.</summary>
    internal static TerrainChange WreckedShipHand => TerrainChange.MadeAtSpawn;

    /// <summary>The Wrecked Ship slope access opening, drawn once before the spikes become slopes.</summary>
    internal static TerrainChange ClearSlopeAccess { get; } =
        TerrainChange.DrawnOnce(ScriptedRoomObjectDrawDefinitions.DrawLists[ScriptedRoomObjectFrame.ChozoSlopeCleared]);

    /// <summary>The Wrecked Ship slope access closing, drawn once before the slopes become spikes again.</summary>
    internal static TerrainChange BlockSlopeAccess { get; } =
        TerrainChange.DrawnOnce(ScriptedRoomObjectDrawDefinitions.DrawLists[ScriptedRoomObjectFrame.ChozoSlopeBlocked]);

    /// <summary>The Lower Norfair plug crumbling: a one-block breakup whose last image, the cleared block, shows for one update.</summary>
    internal static TerrainChange CrumblePlug { get; } = new(1,
    [
        new(ScriptedRoomObjectDrawDefinitions.DrawLists[ScriptedRoomObjectFrame.SingleBreakup0], ScriptedTerrainChangeDefinitions.BreakupFrameDuration),
        new(ScriptedRoomObjectDrawDefinitions.DrawLists[ScriptedRoomObjectFrame.SingleBreakup1], ScriptedTerrainChangeDefinitions.BreakupFrameDuration),
        new(ScriptedRoomObjectDrawDefinitions.DrawLists[ScriptedRoomObjectFrame.SingleBreakup2], ScriptedTerrainChangeDefinitions.BreakupFrameDuration),
        new(ScriptedRoomObjectDrawDefinitions.DrawLists[ScriptedRoomObjectFrame.SingleBreakup3], 1),
    ]);
}
